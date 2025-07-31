using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Основной контроллер юнита, объединяющий логику передвижения, атаки и визуализации выбора.
/// Наследуется от NetworkBehaviour для сетевой синхронизации.
/// </summary>
[RequireComponent(typeof(UnitMovementController))]
[RequireComponent(typeof(UnitAttackController))]
[RequireComponent(typeof(UnitSelectionVisuals))]
public class UnitController : NetworkBehaviour
{
    [SerializeField] private UnitStats statsSO; // Скрипт-билд объект с параметрами юнита (движение, атака)

    private IMovable _movement;                  // Интерфейс для движения
    private IAttackable _attack;                 // Интерфейс для атаки
    private UnitSelectionVisuals _selectionVisuals; // Визуализация выделения юнита

    public bool IsSelected { get; private set; }             // Выделен ли юнит игроком
    public bool IsAttackTargetSelected { get; private set; } // Выделена ли цель для атаки

    public ulong OwnerId => OwnerClientId; // ID клиента-владельца юнита (из NetworkBehaviour)

    // Публичные свойства для параметров из statsSO и состояний
    public float MaxMoveDistance => statsSO.maxMoveDistance;
    public float AttackRange => statsSO.attackRange;
    public float RemainingMoveDistance => _movement?.RemainingMoveDistance ?? 0f;
    public bool HasAttacked => _attack?.HasAttacked ?? false;

    // Доступ к NavMeshAgent для навигации, если реализовано в UnitMovementController
    public NavMeshAgent NavAgent => (_movement as UnitMovementController)?.Agent;

    private void Awake()
    {
        // Получаем компоненты через интерфейсы
        _movement = GetComponent<IMovable>();
        _attack = GetComponent<IAttackable>();
        _selectionVisuals = GetComponent<UnitSelectionVisuals>();

        if (statsSO == null)
            Debug.LogError($"[UnitController] UnitStats not assigned for {gameObject.name}");

        // Инициализируем визуализацию с радиусом атаки
        _selectionVisuals.Initialize(AttackRange);

        // Сбрасываем выделение и выбор цели при старте
        SetSelected(false);
        SetAttackTargetSelected(false);
    }

    public override void OnNetworkSpawn()
    {
        // Инициализируем компоненты при спавне сетевого объекта
        if (_movement is UnitMovementController mov) mov.Initialize(this);
        if (_attack is UnitAttackController atk) atk.Initialize(this);

        // На сервере регистрируем юнит в TurnManager для управления ходами
        if (IsServer)
            TurnManager.Instance.RegisterUnit(this);
    }

    /// <summary>
    /// Устанавливает выделение юнита.
    /// Визуализирует состояние выделения и учитывает, совершал ли юнит атаку.
    /// </summary>
    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        _selectionVisuals.ShowSelection(selected, HasAttacked);
    }

    /// <summary>
    /// Устанавливает выделение цели для атаки.
    /// </summary>
    public void SetAttackTargetSelected(bool selected)
    {
        IsAttackTargetSelected = selected;
        _selectionVisuals.ShowAttackTargetHighlight(selected);
    }

    /// <summary>
    /// Пытается передвинуть юнита к указанной позиции через интерфейс движения.
    /// </summary>
    public void TryMove(Vector3 targetPosition) => _movement?.TryMove(targetPosition);

    /// <summary>
    /// Пытается атаковать цель по позиции через интерфейс атаки.
    /// </summary>
    public void TryAttack(Vector3 targetPosition) => _attack?.TryAttack(targetPosition);

    /// <summary>
    /// Сбрасывает состояния юнита в начале нового хода.
    /// Снимает выделение, сбрасывает атаки и движение.
    /// </summary>
    public void ResetTurn()
    {
        _attack?.ResetAttack();
        _movement?.ResetMovement();
        SetSelected(false);
        SetAttackTargetSelected(false);
    }

    /// <summary>
    /// Серверный RPC для сброса состояния хода юнита.
    /// Может вызываться клиентом без владения объектом.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void ResetTurnServerRpc()
    {
        ResetTurn();
    }

    /// <summary>
    /// Проверяет, находится ли цель в радиусе атаки.
    /// Вызывает соответствующий метод в интерфейсе атаки.
    /// </summary>
    public bool IsTargetInRange(Vector3 targetPosition)
    {
        return _attack?.IsTargetInRange(targetPosition) ?? false;
    }
}
