using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class UnitSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject unitPrefab;
    [SerializeField] private SpawnZone player1Zone;
    [SerializeField] private SpawnZone player2Zone;
    [SerializeField] private Transform unitsContainer;

    private readonly HashSet<ulong> spawnedClients = new();

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // Запускаем задержанный спавн через корутину
            StartCoroutine(DelayedSpawnRoutine());

            // Подписываемся на подключения новых клиентов
            NetworkManager.OnClientConnectedCallback += HandleClientConnected;
        }
    }

    private IEnumerator DelayedSpawnRoutine()
    {
        yield return new WaitForSeconds(1f); // ждем 1 секунды

        SpawnUnitsForAllConnectedClients();
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (!IsServer) return;

        if (spawnedClients.Contains(clientId)) return;

        // Спавним юнит клиента, но без задержки (если нужно, можно тоже отложить)
        SpawnUnitForClient(clientId);
    }

    private void SpawnUnitsForAllConnectedClients()
    {
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!spawnedClients.Contains(clientId))
            {
                SpawnUnitForClient(clientId);
            }
        }
    }

    private void SpawnUnitForClient(ulong clientId)
    {
        Vector3 spawnPos = clientId == 0
            ? player1Zone.GetRandomSpawnPosition()
            : player2Zone.GetRandomSpawnPosition();

        GameObject unit = Instantiate(unitPrefab, spawnPos, Quaternion.identity, unitsContainer);
        NetworkObject netObj = unit.GetComponent<NetworkObject>();
        netObj.SpawnWithOwnership(clientId);

        spawnedClients.Add(clientId);

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
