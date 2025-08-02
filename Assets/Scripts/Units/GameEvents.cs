using System;

/// <summary>
/// Статический класс для централизованного управления игровыми событиями.
/// Позволяет различным системам подписываться и реагировать на события игры.
/// </summary>
public static class GameEvents
{
    // События, связанные с ходами и раундами
    public static event Action<float> OnTurnTimerUpdated;
    public static event Action<ulong> OnTurnStarted;
    public static event Action<ulong> OnTurnEnded;
    public static event Action<int> OnRoundChanged;
    public static event Action<ulong> OnGameEnded;

    // События для UI, связанные с действиями юнитов
    public static event Action<UnitController> OnUnitMoved;
    public static event Action<UnitController> OnUnitAttacked;

    // Методы вызова событий (триггеры)

    public static void TriggerTurnStarted(ulong playerId) => OnTurnStarted?.Invoke(playerId);

    public static void TriggerTurnEnded(ulong playerId) => OnTurnEnded?.Invoke(playerId);

    public static void TriggerTurnTimerUpdated(float timeLeft) => OnTurnTimerUpdated?.Invoke(timeLeft);

    public static void TriggerRoundChanged(int round) => OnRoundChanged?.Invoke(round);

    public static void TriggerGameEnded(ulong winnerPlayerId) => OnGameEnded?.Invoke(winnerPlayerId);

    public static void TriggerUnitMoved(UnitController unit) => OnUnitMoved?.Invoke(unit);

    public static void TriggerUnitAttacked(UnitController unit) => OnUnitAttacked?.Invoke(unit);
}
