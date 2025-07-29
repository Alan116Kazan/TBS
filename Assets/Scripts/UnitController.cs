using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(UnitMovementController))]
[RequireComponent(typeof(UnitAttackController))]
[RequireComponent(typeof(UnitSelectionVisuals))]
public class UnitController : NetworkBehaviour
{
    [Header("Stats")]
    [SerializeField] private UnitStats statsSO;

    private UnitMovementController _movement;
    private UnitAttackController _attack;
    private UnitSelectionVisuals _selectionVisuals;

    public bool IsSelected { get; private set; }
    public bool IsAttackTargetSelected { get; private set; }

    public ulong OwnerId => OwnerClientId;
    public float MaxMoveDistance => statsSO.maxMoveDistance;
    public float AttackRange => statsSO.attackRange;
    public float RemainingMoveDistance => _movement?.RemainingMoveDistance ?? 0f;
    public bool HasAttacked => _attack?.HasAttacked ?? false;

    private void Awake()
    {
        _movement = GetComponent<UnitMovementController>();
        _attack = GetComponent<UnitAttackController>();
        _selectionVisuals = GetComponent<UnitSelectionVisuals>();

        if (statsSO == null)
            Debug.LogError($"[UnitController] UnitStats не назначен у {gameObject.name}");

        _selectionVisuals.Initialize(AttackRange);

        SetSelected(false);
        SetAttackTargetSelected(false);
    }

    public override void OnNetworkSpawn()
    {
        _movement.Initialize(this);
        _attack.Initialize(this);

        if (IsServer)
        {
            TurnManager.Instance.RegisterUnit(this);
        }
    }

    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        _selectionVisuals.ShowSelection(selected, HasAttacked);
    }

    public void SetAttackTargetSelected(bool selected)
    {
        IsAttackTargetSelected = selected;
        _selectionVisuals.ShowAttackTargetHighlight(selected);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TryMoveServerRpc(Vector3 targetPosition, ServerRpcParams rpcParams = default)
    {
        _movement.TryMoveServerRpc(targetPosition, rpcParams);
    }

    [ServerRpc(RequireOwnership = false)]
    public void TryAttackServerRpc(Vector3 targetPosition, ServerRpcParams rpcParams = default)
    {
        _attack.TryAttackServerRpc(targetPosition, rpcParams);
    }

    [ServerRpc(RequireOwnership = false)]
    public void ResetTurnServerRpc()
    {
        _attack.ResetAttackServerRpc();
        _movement.ResetMovementServerRpc();
        SetSelected(false);
        SetAttackTargetSelected(false);
    }

    public NavMeshAgent NavAgent => _movement.Agent;

    public bool IsTargetInRange(Vector3 targetPosition)
    {
        return _attack.IsTargetInRange(targetPosition);
    }
}
