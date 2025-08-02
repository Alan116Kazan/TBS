using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

/// <summary>
/// Отвечает за логику юнита: передвижение, атаку, выбор и взаимодействие с системой ходов.
/// Требует наличие компонентов движения, атаки и визуального отображения.
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
            Debug.LogError($"[UnitController] UnitStats не назначен для {gameObject.name}");

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
    /// Устанавливает статус выбора юнита.
    /// </summary>
    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        _selectionVisuals.ShowSelection(selected, HasAttacked);
    }

    /// <summary>
    /// Подсвечивает юнита как цель атаки.
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
    /// Сброс параметров юнита в начале нового хода.
    /// </summary>
    public void ResetTurn()
    {
        _attack?.ResetAttack();
        _movement?.ResetMovement();

        SetSelected(false);
        SetAttackTargetSelected(false);
    }

    /// <summary>
    /// Проверка, находится ли цель в радиусе атаки.
    /// </summary>
    public bool IsTargetInRange(Vector3 targetPosition) =>
        _attack?.IsTargetInRange(targetPosition) ?? false;

    /// <summary>
    /// Устанавливает режим бесконечного радиуса передвижения (для финальной стадии игры).
    /// </summary>
    public void SetInfiniteMovementRadius(bool enabled)
    {
        if (_movement is UnitMovementController mov)
            mov.SetInfiniteMovementRadius(enabled);
    }

    /// <summary>
    /// Вызывается на клиентах для отключения объекта.
    /// </summary>
    [ClientRpc]
    private void DisableOnClientRpc()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Обрабатывает смерть юнита на сервере и синхронизирует отключение на клиентах.
    /// </summary>
    public void Die()
    {
        if (!IsServer) return;

        Debug.Log($"[UnitController] Юнит {name} умирает и отключается на сервере.");

        DisableOnClientRpc();
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Запрос на смерть юнита от клиента (без необходимости владения).
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void RequestDieServerRpc()
    {
        Die();
    }
}
