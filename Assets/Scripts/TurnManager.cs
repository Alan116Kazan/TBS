using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TurnManager : NetworkBehaviour
{
    public static TurnManager Instance;

    private NetworkVariable<ulong> currentPlayerId = new NetworkVariable<ulong>(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private Dictionary<ulong, List<UnitController>> playerUnits = new();

    private void Awake()
    {
        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        // Начинаем с первого подключенного игрока
        if (NetworkManager.Singleton.ConnectedClientsList.Count > 0)
        {
            ulong firstPlayerId = NetworkManager.Singleton.ConnectedClientsList[0].ClientId;
            currentPlayerId.Value = firstPlayerId;
        }
    }

    public void RegisterUnit(UnitController unit)
    {
        ulong ownerId = unit.OwnerClientId;

        if (!playerUnits.ContainsKey(ownerId))
            playerUnits[ownerId] = new List<UnitController>();

        playerUnits[ownerId].Add(unit);
    }

    public bool IsPlayerTurn(ulong clientId)
    {
        return currentPlayerId.Value == clientId;
    }

    [ServerRpc(RequireOwnership = false)]
    public void EndTurnServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong nextPlayerId = GetNextPlayerId();
        currentPlayerId.Value = nextPlayerId;

        ResetUnitsForPlayer(nextPlayerId);
    }

    private ulong GetNextPlayerId()
    {
        foreach (var key in playerUnits.Keys)
        {
            if (key != currentPlayerId.Value)
                return key;
        }
        return currentPlayerId.Value;
    }

    private void ResetUnitsForPlayer(ulong playerId)
    {
        foreach (var unit in playerUnits[playerId])
        {
            unit.ResetTurnServerRpc(); // будет в следующем скрипте
        }
    }
}
