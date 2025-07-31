using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Отвечает за генерацию карты с препятствиями.
/// Генерация выполняется **только на сервере** при спавне объекта.
/// </summary>
public class MapGenerator : NetworkBehaviour
{
    [Header("Размер поля")]
    [SerializeField] private int width = 10;   // Количество клеток по ширине карты
    [SerializeField] private int height = 10;  // Количество клеток по высоте карты

    [Header("Препятствия")]
    [SerializeField] private GameObject obstaclePrefab; // Префаб объекта-препятствия (обязательно с NetworkObject)
    [SerializeField] private int minObstacles = 5;      // Минимальное количество препятствий
    [SerializeField] private int maxObstacles = 15;     // Максимальное количество препятствий

    // Хранит координаты занятых клеток (чтобы не создавать препятствия в одной и той же точке)
    private readonly HashSet<Vector2Int> _occupiedCells = new();

    /// <summary>
    /// Метод вызывается автоматически при появлении объекта на сцене в сетевой сессии.
    /// Генерация карты выполняется только на сервере.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return; // Генерация должна происходить только на сервере, иначе клиенты будут дублировать объекты

        GenerateMap(); // Запускаем генерацию карты
    }

    /// <summary>
    /// Основной метод генерации карты с препятствиями.
    /// Создаёт случайное количество препятствий в случайных незанятых клетках.
    /// </summary>
    private void GenerateMap()
    {
        // Случайное количество препятствий в диапазоне от minObstacles до maxObstacles включительно
        int obstacleCount = Random.Range(minObstacles, maxObstacles + 1);

        for (int i = 0; i < obstacleCount; i++)
        {
            Vector2Int cell;

            // Ищем свободную клетку, в которую ещё не ставили препятствие
            do
            {
                cell = new Vector2Int(Random.Range(0, width), Random.Range(0, height));
                // Выбираем случайную позицию на сетке
            } while (!_occupiedCells.Add(cell));
            // HashSet.Add вернёт false, если такая клетка уже занята,
            // что заставит цикл искать новую позицию

            // Центрируем объект в клетке (по X и Z смещение 0.5, чтобы объект оказался в центре клетки)
            // Y = 0.5, предполагается, что препятствия — кубы размером 1 по высоте
            Vector3 position = new Vector3(cell.x + 0.5f, 0.5f, cell.y + 0.5f);

            // Создаём объект препятствия в сцене
            GameObject obstacle = Instantiate(obstaclePrefab, position, Quaternion.identity);

            // Проверяем, что префаб содержит компонент NetworkObject — обязательный для работы Netcode
            if (obstacle.TryGetComponent(out NetworkObject netObj))
            {
                netObj.Spawn(); // Спавним объект в сетевой сессии (видим для всех клиентов)
            }
            else
            {
                Debug.LogWarning("Obstacle prefab missing NetworkObject component!");
            }
        }

        Debug.Log($"[Server] Map generated with {obstacleCount} obstacles.");
    }

#if UNITY_EDITOR
    /// <summary>
    /// Визуализация сетки карты в редакторе Unity.
    /// Отрисовывает каркас клеток на плоскости для удобства редактирования и отладки.
    /// Выполняется только в редакторе (не в билде).
    /// </summary>
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        Vector3 size = new(1f, 0.01f, 1f); // Плоские клетки (почти без высоты)

        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                Gizmos.DrawWireCube(new Vector3(x + 0.5f, 0f, y + 0.5f), size);
    }
#endif
}
