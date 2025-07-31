using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

/// <summary>
/// Отвечает за спавн юнитов для каждого подключенного игрока на сервере.
/// Спавнит по одному медленному дальнобойному и одному быстрому ближнему юниту для каждого клиента.
/// Зоны спавна задаются в инспекторе.
/// </summary>
public class UnitSpawner : NetworkBehaviour
{
    [Header("Unit Prefabs")]
    [SerializeField] private GameObject shortMoveLongRangePrefab;  // Префаб медленного дальнобойного юнита
    [SerializeField] private GameObject longMoveShortRangePrefab;  // Префаб быстрого ближнего юнита

    [Header("Spawn Zones")]
    [SerializeField] private SpawnZone player1Zone; // Зона спавна для игрока 1
    [SerializeField] private SpawnZone player2Zone; // Зона спавна для игрока 2

    [Header("Hierarchy")]
    [SerializeField] private Transform unitsContainer; // Родительский объект для всех юнитов (для удобства в иерархии)

    // Множество clientId, для которых юниты уже заспавнены (чтобы не создавать повторно)
    private readonly HashSet<ulong> _spawnedClients = new();

    /// <summary>
    /// Вызывается при старте сетевой сессии.
    /// Если объект — сервер, подписывается на событие подключения клиентов и запускает корутину спавна для уже подключенных.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        if (!IsServer || NetworkManager.Singleton == null) return;

        // Подписываемся на событие подключения новых клиентов
        NetworkManager.OnClientConnectedCallback += OnClientConnected;

        // Запускаем корутину, которая спустя секунду заспавнит юнитов для всех уже подключенных клиентов
        StartCoroutine(SpawnAfterDelay());
    }

    /// <summary>
    /// Корутин с задержкой для спавна юнитов игроков, которые уже подключились к серверу до старта.
    /// </summary>
    private IEnumerator SpawnAfterDelay()
    {
        yield return new WaitForSeconds(1f); // Ждем секунду, чтобы все игроки успели подключиться

        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            TrySpawnUnit(clientId);
        }
    }

    /// <summary>
    /// Обработчик события подключения нового клиента к серверу.
    /// Запускает спавн юнитов для этого клиента.
    /// </summary>
    private void OnClientConnected(ulong clientId)
    {
        if (!IsServer) return;

        TrySpawnUnit(clientId);
    }

    /// <summary>
    /// Спавнит юнитов для клиента, если ещё не заспавнены.
    /// Спавнит по одному медленному дальнобойному и одному быстрому ближнему юниту.
    /// </summary>
    /// <param name="clientId">ID клиента</param>
    private void TrySpawnUnit(ulong clientId)
    {
        if (_spawnedClients.Contains(clientId)) return; // Если уже заспавнено — пропускаем

        // Определяем зону спавна для клиента (для локального клиента — player1Zone, для остальных — player2Zone)
        SpawnZone zone = clientId == NetworkManager.Singleton.LocalClientId ? player1Zone : player2Zone;

        // Создаем юниты
        SpawnUnit(shortMoveLongRangePrefab, clientId, zone);
        SpawnUnit(longMoveShortRangePrefab, clientId, zone);

        _spawnedClients.Add(clientId);
    }

    /// <summary>
    /// Создает один юнит по префабу в зоне спавна и назначает владельца в сети.
    /// </summary>
    /// <param name="prefab">Префаб юнита</param>
    /// <param name="clientId">ID владельца (игрока)</param>
    /// <param name="zone">Зона спавна</param>
    private void SpawnUnit(GameObject prefab, ulong clientId, SpawnZone zone)
    {
        Vector3 spawnPos = zone.GetRandomSpawnPosition(); // Получаем случайную позицию внутри зоны

        GameObject unit = Instantiate(prefab, spawnPos, Quaternion.identity, unitsContainer); // Создаем объект в иерархии

        // Если у объекта есть NetworkObject, спавним с владением клиента
        if (unit.TryGetComponent(out NetworkObject netObj))
            netObj.SpawnWithOwnership(clientId);

        Debug.Log($"[Server] Спавн юнита {prefab.name} для clientId={clientId}");
    }

    /// <summary>
    /// При уничтожении объекта — снимаем подписку с события подключения клиентов.
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
