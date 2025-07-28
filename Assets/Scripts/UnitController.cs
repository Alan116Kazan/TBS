using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public enum UnitType
{
    ShortMove_LongRangeAttack,
    LongMove_ShortRangeAttack
}

[System.Serializable]
public class UnitStats
{
    public float maxMoveDistance = 5f;
    public float attackRange = 2f;
}

public class UnitController : NetworkBehaviour
{
    [SerializeField] private LineRenderer attackRangeRenderer;
    [SerializeField] private int segments = 40;

    [SerializeField] private GameObject selectionCircle;
    [SerializeField] private GameObject attackTargetHighlight;

    [SerializeField] private UnitType unitType;

    [SerializeField] private UnitStats shortMoveLongRangeStats = new() { maxMoveDistance = 3f, attackRange = 7f };
    [SerializeField] private UnitStats longMoveShortRangeStats = new() { maxMoveDistance = 7f, attackRange = 2f };

    private UnitStats currentStats;

    private NavMeshAgent agent;

    private readonly NetworkVariable<float> networkRemainingMoveDistance = new(
        writePerm: NetworkVariableWritePermission.Server,
        readPerm: NetworkVariableReadPermission.Everyone);

    private readonly NetworkVariable<bool> hasAttacked = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public bool IsSelected { get; private set; }
    private bool isAttackTargetSelected = false;

    public ulong OwnerId => OwnerClientId;
    public float MaxMoveDistance => currentStats.maxMoveDistance;
    public float AttackRange => currentStats != null ? currentStats.attackRange : 0f;
    public float RemainingMoveDistance => networkRemainingMoveDistance.Value;
    public bool HasAttacked => hasAttacked.Value;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.autoBraking = true;
        agent.updatePosition = true;
        agent.updateRotation = true;

        SetSelected(false);
        SetAttackTargetSelected(false);
    }

    public override void OnNetworkSpawn()
    {
        currentStats = unitType == UnitType.ShortMove_LongRangeAttack
            ? shortMoveLongRangeStats
            : longMoveShortRangeStats;

        if (IsServer)
        {
            networkRemainingMoveDistance.Value = currentStats.maxMoveDistance;
            hasAttacked.Value = false;

            TurnManager.Instance.RegisterUnit(this);
        }
    }

    public void SetSelected(bool isSelected)
    {
        IsSelected = isSelected;

        if (selectionCircle != null)
            selectionCircle.SetActive(isSelected);

        if (attackRangeRenderer != null)
            attackRangeRenderer.gameObject.SetActive(isSelected && !HasAttacked);

        if (isSelected && !HasAttacked)
            DrawAttackRange();
    }

    public void SetAttackTargetSelected(bool selected)
    {
        isAttackTargetSelected = selected;
        if (attackTargetHighlight != null)
            attackTargetHighlight.SetActive(selected);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TryMoveServerRpc(Vector3 targetPosition, ServerRpcParams rpcParams = default)
    {
        if (!TurnManager.Instance.IsPlayerTurn(OwnerClientId) || RemainingMoveDistance <= 0f) return;

        NavMeshPath path = new NavMeshPath();
        if (!agent.CalculatePath(targetPosition, path) || path.corners.Length < 2) return;

        float totalDistance = 0f;
        Vector3 finalPosition = path.corners[0];

        for (int i = 1; i < path.corners.Length; i++)
        {
            float segmentLength = Vector3.Distance(path.corners[i - 1], path.corners[i]);
            if (totalDistance + segmentLength >= RemainingMoveDistance)
            {
                float remaining = RemainingMoveDistance - totalDistance;
                Vector3 dir = (path.corners[i] - path.corners[i - 1]).normalized;
                finalPosition = path.corners[i - 1] + dir * remaining;
                totalDistance = RemainingMoveDistance;
                break;
            }

            totalDistance += segmentLength;
            finalPosition = path.corners[i];
        }

        agent.SetDestination(finalPosition);
        networkRemainingMoveDistance.Value = Mathf.Max(0f, RemainingMoveDistance - totalDistance);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TryAttackServerRpc(Vector3 targetPosition, ServerRpcParams rpcParams = default)
    {
        if (!TurnManager.Instance.IsPlayerTurn(OwnerClientId) || HasAttacked) return;
        if (!IsTargetInRange(targetPosition)) return;

        Debug.Log($"Юнит {name} атакует цель на позиции {targetPosition}");

        hasAttacked.Value = true;
    }

    public bool IsTargetInRange(Vector3 targetPosition)
    {
        return Vector3.Distance(transform.position, targetPosition) <= AttackRange;
    }

    [ServerRpc(RequireOwnership = false)]
    public void ResetTurnServerRpc()
    {
        hasAttacked.Value = false;
        networkRemainingMoveDistance.Value = currentStats.maxMoveDistance;
        SetSelected(false);
        SetAttackTargetSelected(false);
    }

    private void DrawAttackRange()
    {
        if (attackRangeRenderer == null) return;

        attackRangeRenderer.positionCount = segments + 1;
        float angle = 0f;

        for (int i = 0; i <= segments; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * AttackRange;
            float z = Mathf.Cos(Mathf.Deg2Rad * angle) * AttackRange;
            attackRangeRenderer.SetPosition(i, new Vector3(x, 0.01f, z));
            angle += 360f / segments;
        }
    }
}
