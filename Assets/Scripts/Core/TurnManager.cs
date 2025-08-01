using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TurnManager : NetworkBehaviour
{
    public static TurnManager Instance { get; private set; }

    [Header("Ќастройки хода")]
    [SerializeField] private float turnDuration = 60f;

    private readonly List<UnitController> registeredUnits = new();

    private NetworkVariable<ulong> currentClientId = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> timeLeft = new(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> currentRound = new(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private float turnTimer;

    public ulong CurrentPlayerId => currentClientId.Value;
    public int CurrentRound => currentRound.Value;

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
        if (IsClient)
        {
            timeLeft.OnValueChanged += (_, newValue) =>
            {
                GameEvents.TriggerTurnTimerUpdated(newValue);
            };
        }

        if (IsServer)
        {
            if (NetworkManager.Singleton.ConnectedClientsList.Count > 0)
            {
                currentClientId.Value = NetworkManager.Singleton.ConnectedClientsList[0].ClientId;
                turnTimer = turnDuration;
                timeLeft.Value = turnDuration;
                currentRound.Value = 1;
            }
        }
    }

    private void Update()
    {
        if (!IsServer) return;

        if (turnTimer > 0f)
        {
            turnTimer -= Time.deltaTime;
            timeLeft.Value = Mathf.Max(0f, turnTimer);

            if (turnTimer <= 0f)
            {
                Debug.Log($"TurnManager: таймер истЄк у игрока {currentClientId.Value}, завершение хода автоматически.");
                EndTurnServerRpc();
            }
        }
    }

    private void OnTurnChanged(ulong oldValue, ulong newValue)
    {
        Debug.Log($"TurnManager: ход сменилс€ на игрока {newValue}");

        if (IsServer)
        {
            turnTimer = turnDuration;
            timeLeft.Value = turnTimer;

            foreach (var unit in registeredUnits)
            {
                if (unit.OwnerId == newValue)
                    unit.ResetTurn();
            }
        }

        TriggerTurnStartedClientRpc(newValue, currentRound.Value);
    }

    [ClientRpc]
    private void TriggerTurnStartedClientRpc(ulong newPlayerId, int round)
    {
        GameEvents.TriggerTurnStarted(newPlayerId);
        GameEvents.TriggerRoundChanged(round);
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

        // ≈сли следующий игрок Ч первый игрок, увеличиваем раунд
        if (nextClientId == GetFirstClientId())
        {
            currentRound.Value += 1;
            Debug.Log($"TurnManager: началс€ новый раунд {currentRound.Value}");
            GameEvents.TriggerRoundChanged(currentRound.Value);
        }

        currentClientId.Value = nextClientId;
    }

    private ulong GetNextClientId()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.ClientId != currentClientId.Value)
                return client.ClientId;
        }

        return currentClientId.Value; // если один игрок
    }

    private ulong GetFirstClientId()
    {
        if (NetworkManager.Singleton.ConnectedClientsList.Count > 0)
            return NetworkManager.Singleton.ConnectedClientsList[0].ClientId;

        return 0;
    }
}
