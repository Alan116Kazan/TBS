using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class UnitMovementController : NetworkBehaviour
{
    private NavMeshAgent _agent;
    private UnitController _unitController;

    private readonly NetworkVariable<float> remainingMoveDistance = new(
        writePerm: NetworkVariableWritePermission.Server,
        readPerm: NetworkVariableReadPermission.Everyone);

    public float RemainingMoveDistance => remainingMoveDistance.Value;

    public void Initialize(UnitController controller)
    {
        _unitController = controller;
        _agent = GetComponent<NavMeshAgent>();

        _agent.autoBraking = true;
        _agent.updatePosition = true;
        _agent.updateRotation = true;

        if (IsServer)
            remainingMoveDistance.Value = _unitController.MaxMoveDistance;
    }

    [ServerRpc(RequireOwnership = false)]
    public void TryMoveServerRpc(Vector3 targetPosition, ServerRpcParams rpcParams = default)
    {
        if (!TurnManager.Instance.IsPlayerTurn(_unitController.OwnerId) || RemainingMoveDistance <= 0f) return;

        NavMeshPath path = new NavMeshPath();
        if (!_agent.CalculatePath(targetPosition, path) || path.corners.Length < 2) return;

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
    }

    [ServerRpc(RequireOwnership = false)]
    public void ResetMovementServerRpc()
    {
        remainingMoveDistance.Value = _unitController.MaxMoveDistance;
    }

    public bool TryGetPathTo(Vector3 target, out NavMeshPath path)
    {
        path = new NavMeshPath();
        return _agent.CalculatePath(target, path);
    }

    public NavMeshAgent Agent => _agent;
}
