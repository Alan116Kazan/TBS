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
            // ��������� ����������� ����� ����� ��������
            StartCoroutine(DelayedSpawnRoutine());

            // ������������� �� ����������� ����� ��������
            NetworkManager.OnClientConnectedCallback += HandleClientConnected;
        }
    }

    private IEnumerator DelayedSpawnRoutine()
    {
        yield return new WaitForSeconds(1f); // ���� 1 �������

        SpawnUnitsForAllConnectedClients();
    }

    private void HandleClientConnected(ulong clientId)
    {
        if (!IsServer) return;

        if (spawnedClients.Contains(clientId)) return;

        // ������� ���� �������, �� ��� �������� (���� �����, ����� ���� ��������)
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
