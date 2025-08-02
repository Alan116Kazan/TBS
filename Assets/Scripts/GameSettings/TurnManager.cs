using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Управляет ходами игроков в сетевой пошаговой игре.
/// Отвечает за таймер хода, смену игрока, раунды и определение победителя.
/// </summary>
public class TurnManager : NetworkBehaviour
{
    public static TurnManager Instance { get; private set; }

    [Header("Настройки хода")]
    [SerializeField] private float turnDuration = 60f;

    [Header("Настройки игры")]
    [SerializeField] private int infiniteMovementStartRound = 10;

    private readonly List<UnitController> _registeredUnits = new();

    private NetworkVariable<ulong> _currentClientId = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<float> _timeLeft = new(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<int> _currentRound = new(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    private float _turnTimer;
    private bool _gameEnded = false;
    private bool _isTurnStarted = false;

    public ulong CurrentPlayerId => _currentClientId.Value;
    public int CurrentRound => _currentRound.Value;
    public bool IsTurnStarted => _isTurnStarted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _currentClientId.OnValueChanged += OnTurnChanged;
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            _timeLeft.OnValueChanged += (_, newValue) => GameEvents.TriggerTurnTimerUpdated(newValue);
        }

        if (IsServer && NetworkManager.Singleton.ConnectedClientsList.Count > 0)
        {
            StartFirstTurn();
        }
    }

    private void StartFirstTurn()
    {
        _currentClientId.Value = GetFirstClientId();
        _turnTimer = turnDuration;
        _timeLeft.Value = turnDuration;
        _currentRound.Value = 1;
        _gameEnded = false;
        _isTurnStarted = true;

        TriggerTurnStartedClientRpc(_currentClientId.Value, _currentRound.Value);
    }

    private void Update()
    {
        if (!IsServer || _gameEnded || !_isTurnStarted) return;

        if (CheckForVictory(out ulong winnerId))
        {
            EndGame(winnerId);
            return;
        }

        if (_turnTimer > 0f)
        {
            _turnTimer -= Time.deltaTime;
            _timeLeft.Value = Mathf.Max(0f, _turnTimer);

            if (_turnTimer <= 0f)
                EndTurnServerRpc();
        }
    }

    private void EndGame(ulong winnerId)
    {
        _gameEnded = true;
        _isTurnStarted = false;
        _turnTimer = 0f;
        _timeLeft.Value = 0f;
        TriggerGameEndedClientRpc(winnerId);
    }

    private bool CheckForVictory(out ulong winnerClientId)
    {
        CleanNullUnits();

        var unitCounts = new Dictionary<ulong, int>();
        foreach (var unit in _registeredUnits)
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
            _turnTimer = turnDuration;
            _timeLeft.Value = _turnTimer;

            foreach (var unit in _registeredUnits)
            {
                if (unit != null && unit.OwnerId == newValue)
                    unit.ResetTurn();
            }
        }

        TriggerTurnStartedClientRpc(newValue, _currentRound.Value);
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

    public bool IsPlayerTurn(ulong clientId) => _currentClientId.Value == clientId;

    public void RegisterUnit(UnitController unit)
    {
        if (unit != null && !_registeredUnits.Contains(unit))
            _registeredUnits.Add(unit);
    }

    [ServerRpc(RequireOwnership = false)]
    public void EndTurnServerRpc()
    {
        GameEvents.TriggerTurnEnded(_currentClientId.Value);

        ulong nextClientId = GetNextClientId();

        if (nextClientId == GetFirstClientId())
        {
            _currentRound.Value++;
            GameEvents.TriggerRoundChanged(_currentRound.Value);

            if (_currentRound.Value == infiniteMovementStartRound &&
                TryDetermineWinnerByUnitCount(out ulong winnerId))
            {
                EndGame(winnerId);
                return;
            }

            if (_currentRound.Value >= infiniteMovementStartRound)
                SetUnitsInfiniteMovement(true);
        }

        _currentClientId.Value = nextClientId;
    }

    private void SetUnitsInfiniteMovement(bool enabled)
    {
        foreach (var unit in _registeredUnits)
        {
            unit?.SetInfiniteMovementRadius(enabled);
        }
    }

    private ulong GetNextClientId()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.ClientId != _currentClientId.Value)
                return client.ClientId;
        }

        return _currentClientId.Value;
    }

    private ulong GetFirstClientId()
    {
        var clients = NetworkManager.Singleton.ConnectedClientsList;
        return clients.Count > 0 ? clients[0].ClientId : 0;
    }

    private bool TryDetermineWinnerByUnitCount(out ulong winnerClientId)
    {
        CleanNullUnits();

        var unitCounts = new Dictionary<ulong, int>();

        foreach (var unit in _registeredUnits)
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
        _registeredUnits.RemoveAll(unit => unit == null || unit.gameObject == null);
    }
}
