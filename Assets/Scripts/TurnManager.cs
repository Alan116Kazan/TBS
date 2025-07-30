using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Менеджер ходов в пошаговой сетевой игре.
/// Отвечает за хранение текущего игрока, переключение ходов и сброс состояния юнитов.
/// </summary>
public class TurnManager : NetworkBehaviour
{
    /// <summary>
    /// Singleton для удобного глобального доступа.
    /// </summary>
    public static TurnManager Instance { get; private set; }

    /// <summary>
    /// Синхронизируемый сетевой идентификатор игрока, который ходит в текущий момент.
    /// </summary>
    private readonly NetworkVariable<ulong> currentPlayerId = new(
        0, // по умолчанию 0 — означает, что ходящего игрока нет
        NetworkVariableReadPermission.Everyone, // читают все клиенты
        NetworkVariableWritePermission.Server  // писать может только сервер
    );

    /// <summary>
    /// Словарь, где ключ — clientId игрока, а значение — список юнитов, принадлежащих этому игроку.
    /// </summary>
    private readonly Dictionary<ulong, List<UnitController>> playerUnits = new();

    private void Awake()
    {
        Instance = this;
    }

    private new void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// При появлении объекта в сети (OnNetworkSpawn) на сервере
    /// устанавливаем первого подключённого клиента как первого ходящего.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        var firstClient = NetworkManager.Singleton.ConnectedClientsList.FirstOrDefault();
        if (firstClient != null)
        {
            currentPlayerId.Value = firstClient.ClientId;
        }
    }

    /// <summary>
    /// Регистрирует юнита за конкретного игрока.
    /// </summary>
    /// <param name="unit">Юнит, принадлежащий игроку</param>
    public void RegisterUnit(UnitController unit)
    {
        if (!playerUnits.TryGetValue(unit.OwnerClientId, out var units))
        {
            units = new List<UnitController>();
            playerUnits[unit.OwnerClientId] = units;
        }

        units.Add(unit);
    }

    /// <summary>
    /// Проверяет, принадлежит ли сейчас ход игроку с clientId.
    /// </summary>
    public bool IsPlayerTurn(ulong clientId) => currentPlayerId.Value == clientId;

    /// <summary>
    /// RPC для завершения хода текущим игроком.
    /// Вызывается клиентом, обрабатывается сервером.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void EndTurnServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong nextPlayerId = GetNextPlayerId();
        currentPlayerId.Value = nextPlayerId;

        ResetUnitsForPlayer(nextPlayerId);
    }

    /// <summary>
    /// Определяет следующего игрока по очереди.
    /// Если есть другой игрок — возвращает его clientId,
    /// иначе возвращает текущего.
    /// </summary>
    private ulong GetNextPlayerId()
    {
        // Перебираем всех игроков кроме текущего, и берём первого попавшегося
        foreach (var playerId in playerUnits.Keys.ToList())
        {
            if (playerId != currentPlayerId.Value)
                return playerId;
        }

        // Если других игроков нет, возвращаем текущего (одиночный режим)
        return currentPlayerId.Value;
    }

    /// <summary>
    /// Сбрасывает ход всех юнитов для указанного игрока.
    /// Вызывает RPC на каждом юните, чтобы сбросить атаки, движения и выделения.
    /// </summary>
    private void ResetUnitsForPlayer(ulong playerId)
    {
        if (!playerUnits.TryGetValue(playerId, out var units)) return;

        foreach (var unit in units)
        {
            unit.ResetTurnServerRpc();
        }
    }
}
