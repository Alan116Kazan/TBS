using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TurnManager : NetworkBehaviour
{
    public static TurnManager Instance { get; private set; }

    private readonly List<UnitController> registeredUnits = new();

    private NetworkVariable<ulong> currentClientId = new(0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public ulong CurrentPlayerId => currentClientId.Value;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        currentClientId.OnValueChanged += OnTurnChanged;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            if (NetworkManager.Singleton.ConnectedClientsList.Count > 0)
            {
                currentClientId.Value = NetworkManager.Singleton.ConnectedClientsList[0].ClientId;
                // OnValueChanged вызоветс€ и оповестит всех
            }
        }
    }

    private void OnTurnChanged(ulong oldValue, ulong newValue)
    {
        Debug.Log($"TurnManager: ход сменилс€ на игрока {newValue}");

        // —брасываем состо€ние всех юнитов у нового активного игрока
        foreach (var unit in registeredUnits)
        {
            if (unit.OwnerId == newValue)
            {
                unit.ResetTurn();
            }
        }

        GameEvents.TriggerTurnStarted(newValue);
    }

    public bool IsPlayerTurn(ulong clientId) => currentClientId.Value == clientId;

    public void RegisterUnit(UnitController unit)
    {
        if (!registeredUnits.Contains(unit))
            registeredUnits.Add(unit);
    }

    [ServerRpc(RequireOwnership = false)]
    public void EndTurnServerRpc()
    {
        Debug.Log($"TurnManager: игрок {currentClientId.Value} завершил ход");

        GameEvents.TriggerTurnEnded(currentClientId.Value);

        ulong nextClientId = GetNextClientId();
        currentClientId.Value = nextClientId;
    }

    private ulong GetNextClientId()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.ClientId != currentClientId.Value)
                return client.ClientId;
        }

        return currentClientId.Value;
    }
}
