using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Контроллер передвижения юнита с использованием NavMeshAgent.
/// Реализует интерфейс IMovable и сетевую синхронизацию через NetworkVariable.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class UnitMovementController : NetworkBehaviour, IMovable
{
    private NavMeshAgent _agent;               // Компонент NavMeshAgent для навигации
    private UnitController _unitController;    // Ссылка на основной контроллер юнита

    // Сетевая переменная, хранит оставшееся расстояние для передвижения в текущем ходу
    // Запись — только на сервере, чтение — для всех клиентов
    private readonly NetworkVariable<float> remainingMoveDistance = new(
        writePerm: NetworkVariableWritePermission.Server,
        readPerm: NetworkVariableReadPermission.Everyone);

    /// <summary>
    /// Оставшееся расстояние для передвижения в текущем ходу.
    /// </summary>
    public float RemainingMoveDistance => remainingMoveDistance.Value;

    /// <summary>
    /// Публичный доступ к NavMeshAgent (например, для других компонентов).
    /// </summary>
    public NavMeshAgent Agent => _agent;

    /// <summary>
    /// Инициализация компонента с привязкой к UnitController.
    /// Настраивает NavMeshAgent и сбрасывает оставшееся расстояние на сервере.
    /// </summary>
    public void Initialize(UnitController controller)
    {
        _unitController = controller;
        _agent = GetComponent<NavMeshAgent>();

        _agent.autoBraking = true;       // Автоматическое торможение агента при достижении цели
        _agent.updatePosition = true;    // Обновлять позицию объекта
        _agent.updateRotation = true;    // Обновлять поворот объекта

        if (IsServer)
            remainingMoveDistance.Value = _unitController.MaxMoveDistance; // Устанавливаем максимум движения на сервере
    }

    /// <summary>
    /// Пытается переместить юнита к указанной позиции.
    /// Вызывает серверный RPC для выполнения логики.
    /// </summary>
    public void TryMove(Vector3 targetPosition)
    {
        TryMoveServerRpc(targetPosition);
    }

    /// <summary>
    /// Серверный RPC для перемещения юнита.
    /// Проверяет, что ход принадлежит игроку и осталось движение.
    /// Вычисляет путь с ограничением по оставшемуся расстоянию.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void TryMoveServerRpc(Vector3 targetPosition, ServerRpcParams rpcParams = default)
    {
        // Проверяем, чей сейчас ход и есть ли оставшееся движение
        if (!TurnManager.Instance.IsPlayerTurn(_unitController.OwnerId) || RemainingMoveDistance <= 0f)
            return;

        NavMeshPath path = new NavMeshPath();
        // Проверяем, возможно ли построить путь к цели, и есть ли он минимум из двух точек
        if (!_agent.CalculatePath(targetPosition, path) || path.corners.Length < 2)
            return;

        float totalDistance = 0f;
        Vector3 finalPosition = path.corners[0]; // Начинаем с текущей позиции

        // Проходим по сегментам пути, суммируя расстояния
        for (int i = 1; i < path.corners.Length; i++)
        {
            float segment = Vector3.Distance(path.corners[i - 1], path.corners[i]);

            // Если следующий сегмент превышает оставшееся расстояние, ограничиваем движение
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

        // Устанавливаем конечную точку движения NavMeshAgent
        _agent.SetDestination(finalPosition);

        // Обновляем оставшееся расстояние движения
        remainingMoveDistance.Value = Mathf.Max(0f, RemainingMoveDistance - totalDistance);
    }

    /// <summary>
    /// Сбрасывает параметры движения (например, в начале нового хода).
    /// Вызывает серверный RPC.
    /// </summary>
    public void ResetMovement()
    {
        ResetMovementServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResetMovementServerRpc()
    {
        // Восстанавливаем максимальное расстояние движения
        remainingMoveDistance.Value = _unitController.MaxMoveDistance;
    }
}
