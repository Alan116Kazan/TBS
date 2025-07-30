using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Главный контроллер юнита — отвечает за управление движением, атакой и визуализацией.
/// Обеспечивает взаимодействие с компонентами UnitMovementController, UnitAttackController и UnitSelectionVisuals.
/// </summary>
[RequireComponent(typeof(UnitMovementController))]
[RequireComponent(typeof(UnitAttackController))]
[RequireComponent(typeof(UnitSelectionVisuals))]
public class UnitController : NetworkBehaviour
{
    [Header("Stats")]
    [SerializeField]
    private UnitStats statsSO; // Скриптабельный объект, хранящий параметры юнита (макс. движение, радиус атаки и т.п.)

    // Внутренние ссылки на компоненты управления движением, атакой и визуальными эффектами выбора
    private UnitMovementController _movement;
    private UnitAttackController _attack;
    private UnitSelectionVisuals _selectionVisuals;

    // Состояния выделения юнита и выделения цели атаки
    public bool IsSelected { get; private set; }
    public bool IsAttackTargetSelected { get; private set; }

    // ClientId владельца юнита (игрока)
    public ulong OwnerId => OwnerClientId;

    // Максимальная дистанция движения и радиус атаки (берутся из ScriptableObject)
    public float MaxMoveDistance => statsSO.maxMoveDistance;
    public float AttackRange => statsSO.attackRange;

    // Оставшееся расстояние для движения (получается из UnitMovementController)
    public float RemainingMoveDistance => _movement?.RemainingMoveDistance ?? 0f;

    // Был ли уже выполнен ход с атакой (из UnitAttackController)
    public bool HasAttacked => _attack?.HasAttacked ?? false;

    private void Awake()
    {
        // Получаем ссылки на необходимые компоненты
        _movement = GetComponent<UnitMovementController>();
        _attack = GetComponent<UnitAttackController>();
        _selectionVisuals = GetComponent<UnitSelectionVisuals>();

        // Проверка наличия статистики юнита
        if (statsSO == null)
            Debug.LogError($"[UnitController] UnitStats не назначен у {gameObject.name}");

        // Инициализируем визуализацию радиуса атаки
        _selectionVisuals.Initialize(AttackRange);

        // Сбрасываем состояния выделения и выделения цели
        SetSelected(false);
        SetAttackTargetSelected(false);
    }

    public override void OnNetworkSpawn()
    {
        // Инициализируем контроллеры движения и атаки, передавая ссылку на этот UnitController
        _movement.Initialize(this);
        _attack.Initialize(this);

        // Если это сервер, регистрируем юнит в TurnManager для управления ходами
        if (IsServer)
        {
            TurnManager.Instance.RegisterUnit(this);
        }
    }

    /// <summary>
    /// Устанавливает состояние выделения юнита.
    /// Обновляет визуализацию (например, подсветку).
    /// </summary>
    public void SetSelected(bool selected)
    {
        IsSelected = selected;
        _selectionVisuals.ShowSelection(selected, HasAttacked);
    }

    /// <summary>
    /// Устанавливает состояние выделения юнита как цели атаки.
    /// Обновляет визуальные эффекты выделения цели.
    /// </summary>
    public void SetAttackTargetSelected(bool selected)
    {
        IsAttackTargetSelected = selected;
        _selectionVisuals.ShowAttackTargetHighlight(selected);
    }

    /// <summary>
    /// RPC запрос на сервер о попытке перемещения юнита.
    /// Клиент вызывает, сервер обрабатывает.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void TryMoveServerRpc(Vector3 targetPosition, ServerRpcParams rpcParams = default)
    {
        _movement.TryMoveServerRpc(targetPosition, rpcParams);
    }

    /// <summary>
    /// RPC запрос на сервер о попытке атаки по цели.
    /// Клиент вызывает, сервер обрабатывает.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void TryAttackServerRpc(Vector3 targetPosition, ServerRpcParams rpcParams = default)
    {
        _attack.TryAttackServerRpc(targetPosition, rpcParams);
    }

    /// <summary>
    /// RPC сброса состояния юнита в начале нового хода.
    /// Сбрасывает движение, атаки, визуальное выделение.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void ResetTurnServerRpc()
    {
        _attack.ResetAttackServerRpc();
        _movement.ResetMovementServerRpc();
        SetSelected(false);
        SetAttackTargetSelected(false);
    }

    /// <summary>
    /// Публичный доступ к NavMeshAgent для навигации.
    /// </summary>
    public NavMeshAgent NavAgent => _movement.Agent;

    /// <summary>
    /// Проверка, находится ли цель в радиусе атаки.
    /// Делегирует проверку в UnitAttackController.
    /// </summary>
    public bool IsTargetInRange(Vector3 targetPosition)
    {
        return _attack.IsTargetInRange(targetPosition);
    }
}
