using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class TurnManager : NetworkBehaviour
{
    public static TurnManager Instance { get; private set; }

    [Header("Настройки хода")]
    [SerializeField] private float turnDuration = 60f;

    [Header("Настройки игры")]
    [SerializeField] private int infiniteMovementStartRound = 10; // Раунд, с которого начинается бесконечное движение

    private readonly List<UnitController> registeredUnits = new();

    private NetworkVariable<ulong> currentClientId = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> timeLeft = new(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> currentRound = new(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private float turnTimer;

    public ulong CurrentPlayerId => currentClientId.Value;
    public int CurrentRound => currentRound.Value;

    private bool gameEnded = false;

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
                gameEnded = false;
            }
        }
    }

    private void Update()
    {
        if (!IsServer) return;

        if (gameEnded)
            return;

        if (CheckForVictory(out ulong winnerClientId))
        {
            gameEnded = true;
            turnTimer = 0f;
            timeLeft.Value = 0f;
            TriggerGameEndedClientRpc(winnerClientId);
            return;
        }

        if (turnTimer > 0f)
        {
            turnTimer -= Time.deltaTime;
            timeLeft.Value = Mathf.Max(0f, turnTimer);

            if (turnTimer <= 0f)
            {
                EndTurnServerRpc();
            }
        }
    }

    private bool CheckForVictory(out ulong winnerClientId)
    {
        Dictionary<ulong, int> activeUnitsCount = new Dictionary<ulong, int>();

        foreach (var unit in registeredUnits)
        {
            if (unit.gameObject.activeSelf)
            {
                if (!activeUnitsCount.ContainsKey(unit.OwnerId))
                    activeUnitsCount[unit.OwnerId] = 0;
                activeUnitsCount[unit.OwnerId]++;
            }
        }

        var players = NetworkManager.Singleton.ConnectedClientsList;
        if (players.Count < 2)
        {
            winnerClientId = 0;
            return false;
        }

        int playersWithUnits = 0;
        ulong lastPlayerWithUnits = 0;

        foreach (var player in players)
        {
            ulong clientId = player.ClientId;
            int count = activeUnitsCount.ContainsKey(clientId) ? activeUnitsCount[clientId] : 0;
            if (count > 0)
            {
                playersWithUnits++;
                lastPlayerWithUnits = clientId;
            }
        }

        if (playersWithUnits == 1)
        {
            winnerClientId = lastPlayerWithUnits;
            return true;
        }

        winnerClientId = 0;
        return false;
    }

    private void OnTurnChanged(ulong oldValue, ulong newValue)
    {
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

    [ClientRpc]
    private void TriggerGameEndedClientRpc(ulong winnerClientId)
    {
        GameEvents.TriggerGameEnded(winnerClientId);
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
        GameEvents.TriggerTurnEnded(currentClientId.Value);

        ulong nextClientId = GetNextClientId();

        if (nextClientId == GetFirstClientId())
        {
            currentRound.Value++;
            GameEvents.TriggerRoundChanged(currentRound.Value);

            // Проверка победы по числу юнитов
            if (currentRound.Value == infiniteMovementStartRound)
            {
                if (TryDetermineWinnerByUnitCount(out ulong winnerClientId))
                {
                    gameEnded = true;
                    turnTimer = 0f;
                    timeLeft.Value = 0f;
                    TriggerGameEndedClientRpc(winnerClientId);
                    return;
                }
            }

            // Включаем бесконечное движение
            if (currentRound.Value >= infiniteMovementStartRound)
            {
                SetUnitsInfiniteMovement(true);
            }
        }

        currentClientId.Value = nextClientId;
    }


    private void SetUnitsInfiniteMovement(bool enabled)
    {
        foreach (var unit in registeredUnits)
        {
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
        if (NetworkManager.Singleton.ConnectedClientsList.Count > 0)
            return NetworkManager.Singleton.ConnectedClientsList[0].ClientId;

        return 0;
    }

    private bool TryDetermineWinnerByUnitCount(out ulong winnerClientId)
    {
        Dictionary<ulong, int> activeUnitsCount = new();

        foreach (var unit in registeredUnits)
        {
            if (unit.gameObject.activeSelf)
            {
                if (!activeUnitsCount.ContainsKey(unit.OwnerId))
                    activeUnitsCount[unit.OwnerId] = 0;
                activeUnitsCount[unit.OwnerId]++;
            }
        }

        var players = NetworkManager.Singleton.ConnectedClientsList;
        if (players.Count < 2)
        {
            winnerClientId = 0;
            return false;
        }

        if (activeUnitsCount.Count == 2)
        {
            var enumerator = activeUnitsCount.GetEnumerator();
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

}
