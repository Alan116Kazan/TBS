using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

/// <summary>
/// �������� �� ������ �����: ������������, �����, ����� � �������������� � �������� �����.
/// ������� ������� ����������� ��������, ����� � ����������� �����������.
/// </summary>
[RequireComponent(typeof(UnitMovementController))]
[RequireComponent(typeof(UnitAttackController))]
[RequireComponent(typeof(UnitSelectionVisuals))]
public class UnitController : NetworkBehaviour
{
    [SerializeField] private UnitStats statsSO;

    private IMovable _movement;
    private IAttackable _attack;
    private UnitSelectionVisuals _selectionVisuals;

    public bool IsSelected { get; private set; }
    public bool IsAttackTargetSelected { get; private set; }

    public ulong OwnerId => OwnerClientId;

    public float MaxMoveDistance => statsSO.maxMoveDistance;
    public float AttackRange => statsSO.attackRange;
    public float RemainingMoveDistance => _movement?.RemainingMoveDistance ?? 0f;
    public bool HasAttacked => _attack?.HasAttacked ?? false;

    public NavMeshAgent NavAgent => (_movement as UnitMovementController)?.Agent;

    private void Awake()
    {
        _movement = GetComponent<IMovable>();
        _attack = GetComponent<IAttackable>();
        _selectionVisuals = GetComponent<UnitSelectionVisuals>();

        if (statsSO == null)
            Debug.LogError($"[UnitController] UnitStats �� �������� ��� {gameObject.name}");

        _selectionVisuals.Initialize(AttackRange);

        SetSelected(false);
        SetAttackTargetSelected(false);
    }

    public override void OnNetworkSpawn()
    {
        if (_movement is UnitMovementController mov)
            mov.Initialize(this);

        if (_attack is UnitAttackController atk)
            atk.Initialize(this);

        if (IsServer)
            TurnManager.Instance.RegisterUnit(this);
    }

    /// <summary>
    /// ������������� ������ ������ �����.
    /// </summary>
    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        _selectionVisuals.ShowSelection(selected, HasAttacked);
    }

    /// <summary>
    /// ������������ ����� ��� ���� �����.
    /// </summary>
    public void SetAttackTargetSelected(bool selected)
    {
        IsAttackTargetSelected = selected;
        _selectionVisuals.ShowAttackTargetHighlight(selected);
    }

    public void TryMove(Vector3 targetPosition) =>
        _movement?.TryMove(targetPosition);

    public void TryAttack(Vector3 targetPosition) =>
        _attack?.TryAttack(targetPosition);

    /// <summary>
    /// ����� ���������� ����� � ������ ������ ����.
    /// </summary>
    public void ResetTurn()
    {
        _attack?.ResetAttack();
        _movement?.ResetMovement();

        SetSelected(false);
        SetAttackTargetSelected(false);
    }

    /// <summary>
    /// ��������, ��������� �� ���� � ������� �����.
    /// </summary>
    public bool IsTargetInRange(Vector3 targetPosition) =>
        _attack?.IsTargetInRange(targetPosition) ?? false;

    /// <summary>
    /// ������������� ����� ������������ ������� ������������ (��� ��������� ������ ����).
    /// </summary>
    public void SetInfiniteMovementRadius(bool enabled)
    {
        if (_movement is UnitMovementController mov)
            mov.SetInfiniteMovementRadius(enabled);
    }

    /// <summary>
    /// ���������� �� �������� ��� ���������� �������.
    /// </summary>
    [ClientRpc]
    private void DisableOnClientRpc()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// ������������ ������ ����� �� ������� � �������������� ���������� �� ��������.
    /// </summary>
    public void Die()
    {
        if (!IsServer) return;

        Debug.Log($"[UnitController] ���� {name} ������� � ����������� �� �������.");

        DisableOnClientRpc();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// ������ �� ������ ����� �� ������� (��� ������������� ��������).
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void RequestDieServerRpc()
    {
        Die();
    }
}
