using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class UnitSpawner : NetworkBehaviour
{
    [Header("Unit Prefabs")]
    [SerializeField] private GameObject shortMoveLongRangePrefab;  // ���������, ������������ ����
    [SerializeField] private GameObject longMoveShortRangePrefab;  // �������, ������� ����

    [Header("Spawn Zones")]
    [SerializeField] private SpawnZone player1Zone; // ���� ������ ��� ������ 1
    [SerializeField] private SpawnZone player2Zone; // ���� ������ ��� ������ 2

    [Header("Hierarchy")]
    [SerializeField] private Transform unitsContainer; // ������������ ������ ��� ��������� ������ � ��������

    // ������ ��������, ��� ������� ��� �������� ������ � ����� �� �������� ��������
    private readonly HashSet<ulong> _spawnedClients = new();

    // ��� ������ ������� ������
    public override void OnNetworkSpawn()
    {
        if (!IsServer || NetworkManager.Singleton == null) return;

        // ������������� �� ������� ����������� ��������, ����� �������� �� ������
        NetworkManager.OnClientConnectedCallback += OnClientConnected;

        // ��������� ��������, ����� ��������� � ���������� ������ ��� ��� ������������ �������
        StartCoroutine(SpawnAfterDelay());
    }

    // ������� ��� �������� � ������ ������ ��� ��� ������������ �������
    private IEnumerator SpawnAfterDelay()
    {
        yield return new WaitForSeconds(1f); // ��� �������, ����� ������ ��� ������������

        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            TrySpawnUnit(clientId);
        }
    }

    // ���������� ������� ����������� ������ ������� �� ����� ����
    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;

        TrySpawnUnit(clientId);
    }

    // ���������, �������� �� ��� ������ ��� �������, � ���� ��� � ������� �� 2 ����� (���������� � ��������)
    private void TrySpawnUnit(ulong clientId)
    {
        if (_spawnedClients.Contains(clientId)) return;

        // ���������� ���� ������ ��� �������
        SpawnZone zone = clientId == NetworkManager.Singleton.LocalClientId ? player1Zone : player2Zone;

        // ������� ����� �� ������ ����� ������� ����
        SpawnUnit(shortMoveLongRangePrefab, clientId, zone);
        SpawnUnit(longMoveShortRangePrefab, clientId, zone);

        _spawnedClients.Add(clientId);
    }

    // ������ ���������� ���� �� ������� �� ���� ������ � ������������� ���������
    private void SpawnUnit(GameObject prefab, ulong clientId, SpawnZone zone)
    {
        Vector3 spawnPos = zone.GetRandomSpawnPosition(); // �������� ��������� ������� � ����

        GameObject unit = Instantiate(prefab, spawnPos, Quaternion.identity, unitsContainer); // ������ � ��������

        // ���� � ������� ���� NetworkObject � ������� ������� ������ � ���������� clientId
        if (unit.TryGetComponent(out NetworkObject netObj))
            netObj.SpawnWithOwnership(clientId);

        Debug.Log($"[Server] ����� ����� {prefab.name} ��� clientId={clientId}");
    }

    // ������� �������� ��� ����������� �������
    public override void OnDestroy()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnected;
        }

        base.OnDestroy();
    }
}
