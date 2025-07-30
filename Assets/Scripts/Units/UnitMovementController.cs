using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Компонент управления передвижением юнита через NavMeshAgent.
/// Отвечает за расчёт пути, перемещение и сетевую синхронизацию оставшейся дистанции.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class UnitMovementController : NetworkBehaviour
{
    private NavMeshAgent _agent;
    private UnitController _unitController;

    /// <summary>
    /// Оставшееся расстояние, которое юнит может пройти за текущий ход.
    /// Значение синхронизируется по сети: читается всеми, изменяется только сервером.
    /// </summary>
    private readonly NetworkVariable<float> remainingMoveDistance = new(
        writePerm: NetworkVariableWritePermission.Server,
        readPerm: NetworkVariableReadPermission.Everyone);

    /// <summary>
    /// Публичный доступ к оставшемуся расстоянию на этот ход.
    /// </summary>
    public float RemainingMoveDistance => remainingMoveDistance.Value;

    /// <summary>
    /// Инициализация компонента.
    /// Устанавливает NavMeshAgent и сбрасывает дистанцию передвижения на максимум (только на сервере).
    /// </summary>
    public void Initialize(UnitController controller)
    {
        _unitController = controller;
        _agent = GetComponent<NavMeshAgent>();

        _agent.autoBraking = true;       // Автоматическая остановка в конце пути
        _agent.updatePosition = true;    // Автоматическое обновление позиции
        _agent.updateRotation = true;    // Автоматическое обновление поворота

        if (IsServer)
            remainingMoveDistance.Value = _unitController.MaxMoveDistance;
    }

    /// <summary>
    /// Попытка передвинуть юнита на целевую позицию (вызов с клиента, выполняется на сервере).
    /// Если расстояние превышает оставшееся — перемещение обрезается.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void TryMoveServerRpc(Vector3 targetPosition, ServerRpcParams rpcParams = default)
    {
        // Проверка, что сейчас ход владельца юнита и у него есть оставшееся движение
        if (!TurnManager.Instance.IsPlayerTurn(_unitController.OwnerId) || RemainingMoveDistance <= 0f)
            return;

        NavMeshPath path = new NavMeshPath();

        // Проверка, что путь возможен и имеет хотя бы 2 точки (старт и конечная)
        if (!_agent.CalculatePath(targetPosition, path) || path.corners.Length < 2)
            return;

        float totalDistance = 0f;
        Vector3 finalPosition = path.corners[0]; // Начальная позиция

        // Проход по всем сегментам пути и расчёт расстояния
        for (int i = 1; i < path.corners.Length; i++)
        {
            float segment = Vector3.Distance(path.corners[i - 1], path.corners[i]);

            if (totalDistance + segment > RemainingMoveDistance)
            {
                // Если текущий сегмент превышает оставшееся расстояние — обрезаем путь
                float remaining = RemainingMoveDistance - totalDistance;
                Vector3 direction = (path.corners[i] - path.corners[i - 1]).normalized;
                finalPosition = path.corners[i - 1] + direction * remaining;
                totalDistance = RemainingMoveDistance;
                break;
            }

            totalDistance += segment;
            finalPosition = path.corners[i];
        }

        // Запускаем передвижение NavMeshAgent'а и обновляем оставшуюся дистанцию
        _agent.SetDestination(finalPosition);
        remainingMoveDistance.Value = Mathf.Max(0f, RemainingMoveDistance - totalDistance);
    }

    /// <summary>
    /// Сброс передвижения в начале нового хода (вызывается сервером).
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void ResetMovementServerRpc()
    {
        remainingMoveDistance.Value = _unitController.MaxMoveDistance;
    }

    /// <summary>
    /// Проверяет возможность построить путь до цели.
    /// </summary>
    public bool TryGetPathTo(Vector3 target, out NavMeshPath path)
    {
        path = new NavMeshPath();
        return _agent.CalculatePath(target, path);
    }

    /// <summary>
    /// Публичный доступ к NavMeshAgent'у.
    /// Может быть использован для настройки снаружи.
    /// </summary>
    public NavMeshAgent Agent => _agent;
}
