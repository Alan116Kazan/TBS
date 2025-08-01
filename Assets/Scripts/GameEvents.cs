using System;

public static class GameEvents
{
    public static event Action<float> OnTurnTimerUpdated;

    public static event Action<ulong> OnTurnStarted;
    public static event Action<ulong> OnTurnEnded;

    public static event Action<int> OnRoundChanged;

    // Новое событие для окончания игры
    public static event Action<ulong> OnGameEnded;

    public static void TriggerTurnStarted(ulong playerId)
    {
        OnTurnStarted?.Invoke(playerId);
    }

    public static void TriggerTurnEnded(ulong playerId)
    {
        OnTurnEnded?.Invoke(playerId);
    }

    public static void TriggerTurnTimerUpdated(float timeLeft)
    {
        OnTurnTimerUpdated?.Invoke(timeLeft);
    }

    public static void TriggerRoundChanged(int round)
    {
        OnRoundChanged?.Invoke(round);
    }

    // Новый метод-триггер для окончания игры
    public static void TriggerGameEnded(ulong winnerPlayerId)
    {
        OnGameEnded?.Invoke(winnerPlayerId);
    }
}
