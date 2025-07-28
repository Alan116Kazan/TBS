using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MapGenerator : NetworkBehaviour
{
    [Header("Размер поля")]
    [SerializeField] private int width = 10;
    [SerializeField] private int height = 10;

    [Header("Препятствия")]
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private int minObstacles = 5;
    [SerializeField] private int maxObstacles = 15;

    private readonly HashSet<Vector2Int> occupiedCells = new();

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        GenerateMap();
    }

    private void GenerateMap()
    {
        int obstacleCount = Random.Range(minObstacles, maxObstacles + 1);

        for (int i = 0; i < obstacleCount; i++)
        {
            Vector2Int cell;
            do
            {
                cell = new Vector2Int(Random.Range(0, width), Random.Range(0, height));
            } while (!occupiedCells.Add(cell)); // Add returns false if already present

            Vector3 position = new Vector3(cell.x + 0.5f, 0.5f, cell.y + 0.5f);
            GameObject obstacle = Instantiate(obstaclePrefab, position, Quaternion.identity);

            if (obstacle.TryGetComponent(out NetworkObject netObj))
            {
                netObj.Spawn();
            }
            else
            {
                Debug.LogWarning("Obstacle prefab missing NetworkObject component!");
            }
        }

        Debug.Log($"[Server] Map generated with {obstacleCount} obstacles.");
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.gray;
        Vector3 size = new(1f, 0.01f, 1f);
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                Gizmos.DrawWireCube(new Vector3(x + 0.5f, 0f, y + 0.5f), size);
    }
#endif
}
