using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class UnitController : NetworkBehaviour
{
    [SerializeField] private GameObject selectionCircle;
    [SerializeField] private float maxMoveDistance = 5f;

    private NavMeshAgent agent;

    private bool hasAttacked = false;

    // Синхронизируемое значение оставшегося движения
    private NetworkVariable<float> networkRemainingMoveDistance = new NetworkVariable<float>(
        writePerm: NetworkVariableWritePermission.Server,
        readPerm: NetworkVariableReadPermission.Everyone);

    public bool IsSelected { get; private set; } = false;

    public ulong OwnerId => OwnerClientId;

    public float MaxMoveDistance => maxMoveDistance;

    // Всегда читаем синхронизированное значение
    public float RemainingMoveDistance => networkRemainingMoveDistance.Value;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.autoBraking = true;
        agent.updatePosition = true;
        agent.updateRotation = true;

        SetSelected(false);
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            networkRemainingMoveDistance.Value = maxMoveDistance;
            TurnManager.Instance.RegisterUnit(this);
        }
    }

    public void SetSelected(bool isSelected)
    {
        IsSelected = isSelected;
        if (selectionCircle != null)
            selectionCircle.SetActive(isSelected);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TryMoveServerRpc(Vector3 targetPosition, ServerRpcParams rpcParams = default)
    {
        if (!TurnManager.Instance.IsPlayerTurn(OwnerClientId)) return;
        if (RemainingMoveDistance <= 0f) return;

        NavMeshPath path = new NavMeshPath();
        if (!agent.CalculatePath(targetPosition, path) || path.corners.Length < 2) return;

        float pathDistance = 0f;
        Vector3 finalPosition = path.corners[0];

        for (int i = 1; i < path.corners.Length; i++)
        {
            float segment = Vector3.Distance(path.corners[i - 1], path.corners[i]);
            if (pathDistance + segment >= RemainingMoveDistance)
            {
                float remainingInSegment = RemainingMoveDistance - pathDistance;
                Vector3 direction = (path.corners[i] - path.corners[i - 1]).normalized;
                finalPosition = path.corners[i - 1] + direction * remainingInSegment;
                pathDistance = RemainingMoveDistance;
                break;
            }

            pathDistance += segment;
            finalPosition = path.corners[i];
        }

        agent.SetDestination(finalPosition);

        float newRemaining = RemainingMoveDistance - pathDistance;
        if (newRemaining < 0f) newRemaining = 0f;
        networkRemainingMoveDistance.Value = newRemaining;

        CheckEndOfActions();
    }

    [ServerRpc(RequireOwnership = false)]
    public void TryAttackServerRpc(ServerRpcParams rpcParams = default)
    {
        if (!TurnManager.Instance.IsPlayerTurn(OwnerClientId)) return;
        if (hasAttacked) return;

        hasAttacked = true;

        CheckEndOfActions();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ResetTurnServerRpc()
    {
        hasAttacked = false;
        networkRemainingMoveDistance.Value = maxMoveDistance;
        SetSelected(false);
    }

    private void CheckEndOfActions()
    {
        if (RemainingMoveDistance == 0f && hasAttacked)
        {
            SetSelected(false);
            // Можно вызвать смену хода:
            // TurnManager.Instance.EndUnitTurn(this);
        }
    }
}
