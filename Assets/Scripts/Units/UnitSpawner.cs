using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

/// <summary>
/// �������� �� ����� ������ ��� ������� ������������� ������ �� �������.
/// ������� �� ������ ���������� ������������� � ������ �������� �������� ����� ��� ������� �������.
/// ���� ������ �������� � ����������.
/// </summary>
public class UnitSpawner : NetworkBehaviour
{
    [Header("Unit Prefabs")]
    [SerializeField] private GameObject shortMoveLongRangePrefab;  // ������ ���������� ������������� �����
    [SerializeField] private GameObject longMoveShortRangePrefab;  // ������ �������� �������� �����

    [Header("Spawn Zones")]
    [SerializeField] private SpawnZone player1Zone; // ���� ������ ��� ������ 1
    [SerializeField] private SpawnZone player2Zone; // ���� ������ ��� ������ 2

    [Header("Hierarchy")]
    [SerializeField] private Transform unitsContainer; // ������������ ������ ��� ���� ������ (��� �������� � ��������)

    // ��������� clientId, ��� ������� ����� ��� ���������� (����� �� ��������� ��������)
    private readonly HashSet<ulong> _spawnedClients = new();

    /// <summary>
    /// ���������� ��� ������ ������� ������.
    /// ���� ������ � ������, ������������� �� ������� ����������� �������� � ��������� �������� ������ ��� ��� ������������.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        if (!IsServer || NetworkManager.Singleton == null) return;

        // ������������� �� ������� ����������� ����� ��������
        NetworkManager.OnClientConnectedCallback += OnClientConnected;

        // ��������� ��������, ������� ������ ������� ��������� ������ ��� ���� ��� ������������ ��������
        StartCoroutine(SpawnAfterDelay());
    }

    /// <summary>
    /// ������� � ��������� ��� ������ ������ �������, ������� ��� ������������ � ������� �� ������.
    /// </summary>
    private IEnumerator SpawnAfterDelay()
    {
        yield return new WaitForSeconds(1f); // ���� �������, ����� ��� ������ ������ ������������

        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            TrySpawnUnit(clientId);
        }
    }

    /// <summary>
    /// ���������� ������� ����������� ������ ������� � �������.
    /// ��������� ����� ������ ��� ����� �������.
    /// </summary>
    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;

        TrySpawnUnit(clientId);
    }

    /// <summary>
    /// ������� ������ ��� �������, ���� ��� �� ����������.
    /// ������� �� ������ ���������� ������������� � ������ �������� �������� �����.
    /// </summary>
    /// <param name="clientId">ID �������</param>
    private void TrySpawnUnit(ulong clientId)
    {
        if (_spawnedClients.Contains(clientId)) return; // ���� ��� ���������� � ����������

        // ���������� ���� ������ ��� ������� (��� ���������� ������� � player1Zone, ��� ��������� � player2Zone)
        SpawnZone zone = clientId == NetworkManager.Singleton.LocalClientId ? player1Zone : player2Zone;

        // ������� �����
        SpawnUnit(shortMoveLongRangePrefab, clientId, zone);
        SpawnUnit(longMoveShortRangePrefab, clientId, zone);

        _spawnedClients.Add(clientId);
    }

    /// <summary>
    /// ������� ���� ���� �� ������� � ���� ������ � ��������� ��������� � ����.
    /// </summary>
    /// <param name="prefab">������ �����</param>
    /// <param name="clientId">ID ��������� (������)</param>
    /// <param name="zone">���� ������</param>
    private void SpawnUnit(GameObject prefab, ulong clientId, SpawnZone zone)
    {
        Vector3 spawnPos = zone.GetRandomSpawnPosition(); // �������� ��������� ������� ������ ����

        GameObject unit = Instantiate(prefab, spawnPos, Quaternion.identity, unitsContainer); // ������� ������ � ��������

        // ���� � ������� ���� NetworkObject, ������� � ��������� �������
        if (unit.TryGetComponent(out NetworkObject netObj))
            netObj.SpawnWithOwnership(clientId);

        Debug.Log($"[Server] ����� ����� {prefab.name} ��� clientId={clientId}");
    }

    /// <summary>
    /// ��� ����������� ������� � ������� �������� � ������� ����������� ��������.
    /// </summary>
    public override void OnDestroy()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnected;
        }

        base.OnDestroy();
    }
}
