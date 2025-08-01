using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TurnManager : NetworkBehaviour
{
    public static TurnManager Instance { get; private set; }

    [Header("Настройки хода")]
    [SerializeField] private float turnDuration = 60f;

    [Header("Настройки игры")]
    [SerializeField] private int infiniteMovementStartRound = 10;

    private readonly List<UnitController> registeredUnits = new();

    private NetworkVariable<ulong> currentClientId = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> timeLeft = new(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> currentRound = new(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private float turnTimer;
    private bool gameEnded = false;

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
            timeLeft.OnValueChanged += (_, newValue) => GameEvents.TriggerTurnTimerUpdated(newValue);
        }

        if (IsServer && NetworkManager.Singleton.ConnectedClientsList.Count > 0)
        {
            var firstClient = NetworkManager.Singleton.ConnectedClientsList[0].ClientId;
            currentClientId.Value = firstClient;
            turnTimer = turnDuration;
            timeLeft.Value = turnDuration;
            currentRound.Value = 1;
            gameEnded = false;
        }
    }

    private void Update()
    {
        if (!IsServer || gameEnded) return;

        if (CheckForVictory(out ulong winnerId))
        {
            EndGame(winnerId);
            return;
        }

        if (turnTimer > 0f)
        {
            turnTimer -= Time.deltaTime;
            timeLeft.Value = Mathf.Max(0f, turnTimer);

            if (turnTimer <= 0f)
                EndTurnServerRpc();
        }
    }

    private void EndGame(ulong winnerId)
    {
        gameEnded = true;
        turnTimer = 0f;
        timeLeft.Value = 0f;
        TriggerGameEndedClientRpc(winnerId);
    }

    private bool CheckForVictory(out ulong winnerClientId)
    {
        CleanNullUnits();

        Dictionary<ulong, int> unitCounts = new();

        foreach (var unit in registeredUnits)
        {
            if (unit != null && unit.gameObject != null && unit.gameObject.activeSelf)
            {
                if (!unitCounts.ContainsKey(unit.OwnerId))
                    unitCounts[unit.OwnerId] = 0;
                unitCounts[unit.OwnerId]++;
            }
        }

        int playersWithUnits = 0;
        ulong lastPlayerId = 0;

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (unitCounts.TryGetValue(client.ClientId, out int count) && count > 0)
            {
                playersWithUnits++;
                lastPlayerId = client.ClientId;
            }
        }

        winnerClientId = playersWithUnits == 1 ? lastPlayerId : 0;
        return playersWithUnits == 1;
    }

    private void OnTurnChanged(ulong oldValue, ulong newValue)
    {
        if (IsServer)
        {
            turnTimer = turnDuration;
            timeLeft.Value = turnTimer;

            foreach (var unit in registeredUnits)
            {
                if (unit != null && unit.OwnerId == newValue)
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

    [ClientRpc]
    private void TriggerGameEndedClientRpc(ulong winnerClientId)
    {
        GameEvents.TriggerGameEnded(winnerClientId);
    }

    public bool IsPlayerTurn(ulong clientId) => currentClientId.Value == clientId;

    public void RegisterUnit(UnitController unit)
    {
        if (unit != null && !registeredUnits.Contains(unit))
            registeredUnits.Add(unit);
    }

    [ServerRpc(RequireOwnership = false)]
    public void EndTurnServerRpc()
    {
        GameEvents.TriggerTurnEnded(currentClientId.Value);

        ulong nextClientId = GetNextClientId();

        if (nextClientId == GetFirstClientId())
        {
            currentRound.Value++;
            GameEvents.TriggerRoundChanged(currentRound.Value);

            if (currentRound.Value == infiniteMovementStartRound &&
                TryDetermineWinnerByUnitCount(out ulong winnerId))
            {
                EndGame(winnerId);
                return;
            }

            if (currentRound.Value >= infiniteMovementStartRound)
                SetUnitsInfiniteMovement(true);
        }

        currentClientId.Value = nextClientId;
    }

    private void SetUnitsInfiniteMovement(bool enabled)
    {
        foreach (var unit in registeredUnits)
        {
            if (unit != null)
                unit.SetInfiniteMovementRadius(enabled);
        }
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

    private ulong GetFirstClientId()
    {
        return NetworkManager.Singleton.ConnectedClientsList.Count > 0
            ? NetworkManager.Singleton.ConnectedClientsList[0].ClientId
            : 0;
    }

    private bool TryDetermineWinnerByUnitCount(out ulong winnerClientId)
    {
        CleanNullUnits();

        Dictionary<ulong, int> unitCounts = new();

        foreach (var unit in registeredUnits)
        {
            if (unit != null && unit.gameObject != null && unit.gameObject.activeSelf)
            {
                if (!unitCounts.ContainsKey(unit.OwnerId))
                    unitCounts[unit.OwnerId] = 0;
                unitCounts[unit.OwnerId]++;
            }
        }

        if (unitCounts.Count == 2)
        {
            using var enumerator = unitCounts.GetEnumerator();
            enumerator.MoveNext();
            var first = enumerator.Current;
            enumerator.MoveNext();
            var second = enumerator.Current;

            if (first.Value > second.Value)
            {
                winnerClientId = first.Key;
                return true;
            }
            else if (second.Value > first.Value)
            {
                winnerClientId = second.Key;
                return true;
            }
        }

        winnerClientId = 0;
        return false;
    }

    private void CleanNullUnits()
    {
        registeredUnits.RemoveAll(unit => unit == null || unit.gameObject == null);
    }
}
