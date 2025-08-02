using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Генерирует карту с препятствиями на плоскости (Plane).
/// Использует позицию и масштаб Plane для расчёта сетки.
/// Генерация выполняется только на сервере.
/// </summary>
public class MapGenerator : NetworkBehaviour
{
    [Header("Размер сетки")]
    [SerializeField] private int width = 10;
    [SerializeField] private int height = 10;

    [Header("Препятствия")]
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private int minObstacles = 5;
    [SerializeField] private int maxObstacles = 15;

    [Header("Ссылка на Plane")]
    [SerializeField] private Transform planeTransform;

    private readonly HashSet<Vector2Int> _occupiedCells = new();

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;
        GenerateMap();
    }

    private void GenerateMap()
    {
        if (planeTransform == null)
        {
            Debug.LogError("Plane Transform не установлен!");
            return;
        }

        int obstacleCount = Random.Range(minObstacles, maxObstacles + 1);

        Vector3 planeOrigin = planeTransform.position;
        Vector3 planeSize = planeTransform.localScale * 10f;

        float cellWidth = planeSize.x / width;
        float cellHeight = planeSize.z / height;

        for (int i = 0; i < obstacleCount; i++)
        {
            Vector2Int cell;
            do
            {
                cell = new Vector2Int(Random.Range(0, width), Random.Range(0, height));
            } while (!_occupiedCells.Add(cell));

            float posX = planeOrigin.x - planeSize.x / 2 + cell.x * cellWidth + cellWidth / 2;
            float posZ = planeOrigin.z - planeSize.z / 2 + cell.y * cellHeight + cellHeight / 2;
            Vector3 rayOrigin = new Vector3(posX, planeOrigin.y + 10f, posZ);

            if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, 20f))
            {
                Vector3 spawnPoint = hit.point;

                
                GameObject obstacle = Instantiate(obstaclePrefab, Vector3.zero, Quaternion.identity);

                if (obstacle.TryGetComponent(out Renderer renderer))
                {
                    float height = renderer.bounds.size.y;
                    spawnPoint.y += height / 2f;
                }

                obstacle.transform.position = spawnPoint;

                if (obstacle.TryGetComponent(out NetworkObject netObj))
                    netObj.Spawn();
                else
                    Debug.LogWarning("Obstacle prefab missing NetworkObject component!");
            }
            else
            {
                Debug.LogWarning($"Raycast не попал в Plane на позиции {rayOrigin}");
            }
        }

        Debug.Log($"[Server] Map generated with {obstacleCount} obstacles.");
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (planeTransform == null) return;

        Gizmos.color = Color.gray;
        Vector3 planeOrigin = planeTransform.position;
        Vector3 planeSize = planeTransform.localScale * 10f;

        float cellWidth = planeSize.x / width;
        float cellHeight = planeSize.z / height;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float posX = planeOrigin.x - planeSize.x / 2 + x * cellWidth + cellWidth / 2;
                float posZ = planeOrigin.z - planeSize.z / 2 + y * cellHeight + cellHeight / 2;
                Vector3 center = new Vector3(posX, planeOrigin.y + 0.01f, posZ);
                Gizmos.DrawWireCube(center, new Vector3(cellWidth, 0.01f, cellHeight));
            }
        }
    }
#endif
}
