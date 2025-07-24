using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MapGenerator : NetworkBehaviour
{
    [Header("Размер поля")]
    public int width = 10;
    public int height = 10;

    [Header("Препятствия")]
    public GameObject obstaclePrefab;
    public int minObstacles = 5;
    public int maxObstacles = 15;

    // Для отслеживания занятых ячеек
    private readonly HashSet<Vector2Int> occupiedCells = new();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            GenerateMap();
        }
    }

    private void GenerateMap()
    {
        int count = Random.Range(minObstacles, maxObstacles + 1);

        for (int i = 0; i < count; i++)
        {
            Vector2Int cell;
            do
            {
                cell = new Vector2Int(Random.Range(0, width), Random.Range(0, height));
            }
            while (occupiedCells.Contains(cell));

            occupiedCells.Add(cell);

            Vector3 worldPos = new Vector3(cell.x + 0.5f, 0.5f, cell.y + 0.5f);
            GameObject obstacle = Instantiate(obstaclePrefab, worldPos, Quaternion.identity);

            var netObj = obstacle.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Spawn();
            }
            else
            {
                Debug.LogWarning("Obstacle prefab missing NetworkObject component!");
            }
        }

        Debug.Log($"[Server] Map generated with {count} obstacles.");
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 center = new Vector3(x + 0.5f, 0f, y + 0.5f);
                Gizmos.DrawWireCube(center, new Vector3(1f, 0.01f, 1f));
            }
        }
    }
#endif
}
