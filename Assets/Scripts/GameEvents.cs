using System;

public static class GameEvents
{
    public static event Action<float> OnTurnTimerUpdated;
    public static event Action<ulong> OnTurnStarted;
    public static event Action<ulong> OnTurnEnded;
    public static event Action<int> OnRoundChanged;
    public static event Action<ulong> OnGameEnded;

    // Новые события для UI
    public static event Action<UnitController> OnUnitMoved;
    public static event Action<UnitController> OnUnitAttacked;

    public static void TriggerTurnStarted(ulong playerId) => OnTurnStarted?.Invoke(playerId);
    public static void TriggerTurnEnded(ulong playerId) => OnTurnEnded?.Invoke(playerId);
    public static void TriggerTurnTimerUpdated(float timeLeft) => OnTurnTimerUpdated?.Invoke(timeLeft);
    public static void TriggerRoundChanged(int round) => OnRoundChanged?.Invoke(round);
    public static void TriggerGameEnded(ulong winnerPlayerId) => OnGameEnded?.Invoke(winnerPlayerId);

    // Триггеры для обновления UI
    public static void TriggerUnitMoved(UnitController unit) => OnUnitMoved?.Invoke(unit);
    public static void TriggerUnitAttacked(UnitController unit) => OnUnitAttacked?.Invoke(unit);
}
