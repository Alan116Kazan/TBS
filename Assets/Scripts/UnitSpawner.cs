using UnityEngine;
using Unity.Netcode;

public class UnitSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private SpawnZone player1Zone;
    [SerializeField] private SpawnZone player2Zone;
    [SerializeField] private Transform unitsContainer;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // 1. Спавним юнит для самого хоста вручную (clientId == 0)
            SpawnUnitForClient(NetworkManager.LocalClientId);

            // 2. Обрабатываем подключения клиентов
            NetworkManager.OnClientConnectedCallback += HandleClientConnected;
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (clientId == NetworkManager.LocalClientId)
            return; // Хост уже получил своего юнита вручную

        SpawnUnitForClient(clientId);
    }

    private void SpawnUnitForClient(ulong clientId)
    {
        Vector3 spawnPos = clientId == 0
            ? player1Zone.GetRandomSpawnPosition()
            : player2Zone.GetRandomSpawnPosition();

        GameObject unit = Instantiate(unitPrefab, spawnPos, Quaternion.identity, unitsContainer);
        NetworkObject netObj = unit.GetComponent<NetworkObject>();
        netObj.SpawnWithOwnership(clientId);

        Debug.Log($"[Server] Spawned unit for client {clientId} at {spawnPos}");
    }

    public override void OnDestroy()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.OnClientConnectedCallback -= HandleClientConnected;
        }

        base.OnDestroy();
    }
}
