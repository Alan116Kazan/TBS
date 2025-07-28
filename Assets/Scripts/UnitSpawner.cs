using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class UnitSpawner : NetworkBehaviour
{
    [Header("Unit Prefabs")]
    [SerializeField] private GameObject shortMoveLongRangePrefab;  // Медленный, дальнобойный
    [SerializeField] private GameObject longMoveShortRangePrefab;  // Быстрый, ближний

    [Header("Spawn Zones")]
    [SerializeField] private SpawnZone player1Zone;
    [SerializeField] private SpawnZone player2Zone;

    [Header("Hierarchy")]
    [SerializeField] private Transform unitsContainer;

    private readonly HashSet<ulong> spawnedClients = new();

    public override void OnNetworkSpawn()
    {
        if (!IsServer || NetworkManager.Singleton == null) return;

        NetworkManager.OnClientConnectedCallback += OnClientConnected;

        // Если хост — запускаем спавн после задержки
        StartCoroutine(SpawnAfterDelay());
    }

    private IEnumerator SpawnAfterDelay()
    {
        yield return new WaitForSeconds(1f);

        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            TrySpawnUnit(clientId);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;

        // Если клиент подключился после старта — спавним ему юнита
        TrySpawnUnit(clientId);
    }

    private void TrySpawnUnit(ulong clientId)
    {
        if (spawnedClients.Contains(clientId)) return;

        var selection = UnitSelectionManager.Instance.GetSelectionForClient(clientId);
        if (selection == null)
        {
            Debug.LogWarning($"[Server] Нет данных для clientId={clientId}");
            return;
        }

        SpawnZone zone = clientId == 0 ? player1Zone : player2Zone;

        for (int i = 0; i < selection.shortMoveLongRangeCount; i++)
        {
            SpawnUnit(shortMoveLongRangePrefab, clientId, zone);
        }

        for (int i = 0; i < selection.longMoveShortRangeCount; i++)
        {
            SpawnUnit(longMoveShortRangePrefab, clientId, zone);
        }

        spawnedClients.Add(clientId);
    }

    private void SpawnUnit(GameObject prefab, ulong clientId, SpawnZone zone)
    {
        Vector3 spawnPos = zone.GetRandomSpawnPosition();
        GameObject unit = Instantiate(prefab, spawnPos, Quaternion.identity, unitsContainer);

        if (unit.TryGetComponent(out NetworkObject netObj))
            netObj.SpawnWithOwnership(clientId);

        Debug.Log($"[Server] Спавн юнита {prefab.name} для clientId={clientId}");
    }


    public override void OnDestroy()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnected;
        }

        base.OnDestroy();
    }
}
