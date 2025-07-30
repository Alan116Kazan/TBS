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
        if (!IsServer) return; // Только сервер отвечает за генерацию карты

        GenerateMap();
    }

    /// <summary>
    /// Основной метод генерации карты с препятствиями.
    /// </summary>
    private void GenerateMap()
    {
        // Случайное количество препятствий
        int obstacleCount = Random.Range(minObstacles, maxObstacles + 1);

        for (int i = 0; i < obstacleCount; i++)
        {
            Vector2Int cell;

            // Ищем свободную клетку, в которую ещё не ставили препятствие
            do
            {
                cell = new Vector2Int(Random.Range(0, width), Random.Range(0, height));
            } while (!_occupiedCells.Add(cell));
            // HashSet.Add вернёт false, если такая клетка уже занята

            // Центрируем объект в клетке (0.5 по X и Z), высота Y — 0.5 (подходит для куба размером 1)
            Vector3 position = new Vector3(cell.x + 0.5f, 0.5f, cell.y + 0.5f);

            // Создаём объект препятствия
            GameObject obstacle = Instantiate(obstaclePrefab, position, Quaternion.identity);

            // Убедимся, что объект сетевой (обязательно для Netcode)
            if (obstacle.TryGetComponent(out NetworkObject netObj))
            {
                netObj.Spawn(); // Спавн в сетевом пространстве
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
    /// Удобно для отладки при разработке.
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
