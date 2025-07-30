using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class UnitSpawner : NetworkBehaviour
{
    [Header("Unit Prefabs")]
    [SerializeField] private GameObject shortMoveLongRangePrefab;  // Медленный, дальнобойный юнит
    [SerializeField] private GameObject longMoveShortRangePrefab;  // Быстрый, ближний юнит

    [Header("Spawn Zones")]
    [SerializeField] private SpawnZone player1Zone; // Зона спавна для игрока 1
    [SerializeField] private SpawnZone player2Zone; // Зона спавна для игрока 2

    [Header("Hierarchy")]
    [SerializeField] private Transform unitsContainer; // Родительский объект для созданных юнитов в иерархии

    // Список клиентов, для которых уже спавнили юнитов — чтобы не спавнить повторно
    private readonly HashSet<ulong> _spawnedClients = new();

    // При старте сетевой сессии
    public override void OnNetworkSpawn()
    {
        if (!IsServer || NetworkManager.Singleton == null) return;

        // Подписываемся на событие подключения клиентов, чтобы спавнить их юнитов
        NetworkManager.OnClientConnectedCallback += OnClientConnected;

        // Запускаем корутину, чтобы подождать и заспавнить юнитов для уже подключенных игроков
        StartCoroutine(SpawnAfterDelay());
    }

    // Корутин для задержки и спавна юнитов для уже подключенных игроков
    private IEnumerator SpawnAfterDelay()
    {
        yield return new WaitForSeconds(1f); // Ждём секунду, чтобы успели все подключиться

        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            TrySpawnUnit(clientId);
        }
    }

    // Обработчик события подключения нового клиента во время игры
    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;

        TrySpawnUnit(clientId);
    }

    // Проверяет, спавнили ли уже юнитов для клиента, и если нет — спавнит по 2 юнита (медленного и быстрого)
    private void TrySpawnUnit(ulong clientId)
    {
        if (_spawnedClients.Contains(clientId)) return;

        // Определяем зону спавна для клиента
        SpawnZone zone = clientId == NetworkManager.Singleton.LocalClientId ? player1Zone : player2Zone;

        // Спавним жёстко по одному юниту каждого типа
        SpawnUnit(shortMoveLongRangePrefab, clientId, zone);
        SpawnUnit(longMoveShortRangePrefab, clientId, zone);

        _spawnedClients.Add(clientId);
    }

    // Создаёт конкретный юнит на позиции из зоны спавна и устанавливает владельца
    private void SpawnUnit(GameObject prefab, ulong clientId, SpawnZone zone)
    {
        Vector3 spawnPos = zone.GetRandomSpawnPosition(); // Получаем случайную позицию в зоне

        GameObject unit = Instantiate(prefab, spawnPos, Quaternion.identity, unitsContainer); // Создаём в иерархии

        // Если у префаба есть NetworkObject — спавним сетевой объект с владельцем clientId
        if (unit.TryGetComponent(out NetworkObject netObj))
            netObj.SpawnWithOwnership(clientId);

        Debug.Log($"[Server] Спавн юнита {prefab.name} для clientId={clientId}");
    }

    // Очистка подписок при уничтожении объекта
    public override void OnDestroy()
    {
        if (IsServer && NetworkManager.Singleton != null)
        {
            NetworkManager.OnClientConnectedCallback -= OnClientConnected;
        }

        base.OnDestroy();
    }
}
