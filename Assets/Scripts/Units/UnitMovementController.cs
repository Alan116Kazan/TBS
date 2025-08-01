using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class UnitMovementController : NetworkBehaviour, IMovable
{
    private NavMeshAgent _agent;
    private UnitController _unitController;

    private readonly NetworkVariable<float> remainingMoveDistance = new(
        writePerm: NetworkVariableWritePermission.Server,
        readPerm: NetworkVariableReadPermission.Everyone);

    private float defaultMaxMoveDistance;
    private bool infiniteMovementEnabled = false;

    public float RemainingMoveDistance => remainingMoveDistance.Value;

    public NavMeshAgent Agent => _agent;

    public void Initialize(UnitController controller)
    {
        _unitController = controller;
        _agent = GetComponent<NavMeshAgent>();

        _agent.autoBraking = true;
        _agent.updatePosition = true;
        _agent.updateRotation = true;

        defaultMaxMoveDistance = _unitController.MaxMoveDistance;

        if (IsServer)
            remainingMoveDistance.Value = defaultMaxMoveDistance;

        // �������� �� ���������
        remainingMoveDistance.OnValueChanged += (prev, next) =>
        {
            if (IsClient)
                GameEvents.TriggerUnitMoved(_unitController);
        };
    }


    public void SetInfiniteMovementRadius(bool enabled)
    {
        if (!IsServer) return;

        infiniteMovementEnabled = enabled;

        if (infiniteMovementEnabled)
            remainingMoveDistance.Value = float.MaxValue;
        else
            remainingMoveDistance.Value = defaultMaxMoveDistance;

        GameEvents.TriggerUnitMoved(_unitController);
    }

    public void TryMove(Vector3 targetPosition)
    {
        TryMoveServerRpc(targetPosition);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TryMoveServerRpc(Vector3 targetPosition, ServerRpcParams rpcParams = default)
    {
        if (!TurnManager.Instance.IsPlayerTurn(_unitController.OwnerId) || RemainingMoveDistance <= 0f)
            return;

        NavMeshPath path = new NavMeshPath();

        if (!_agent.CalculatePath(targetPosition, path) || path.corners.Length < 2)
            return;

        float totalDistance = 0f;
        Vector3 finalPosition = path.corners[0];

        for (int i = 1; i < path.corners.Length; i++)
        {
            float segment = Vector3.Distance(path.corners[i - 1], path.corners[i]);

            if (totalDistance + segment > RemainingMoveDistance)
            {
                float remaining = RemainingMoveDistance - totalDistance;
                Vector3 direction = (path.corners[i] - path.corners[i - 1]).normalized;
                finalPosition = path.corners[i - 1] + direction * remaining;
                totalDistance = RemainingMoveDistance;
                break;
            }

            totalDistance += segment;
            finalPosition = path.corners[i];
        }

        _agent.SetDestination(finalPosition);
        remainingMoveDistance.Value = Mathf.Max(0f, RemainingMoveDistance - totalDistance);
        GameEvents.TriggerUnitMoved(_unitController);
    }

    public void ResetMovement()
    {
        ResetMovementServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResetMovementServerRpc()
    {
        if (infiniteMovementEnabled)
            remainingMoveDistance.Value = float.MaxValue;
        else
            remainingMoveDistance.Value = defaultMaxMoveDistance;

        GameEvents.TriggerUnitMoved(_unitController);
    }
}
