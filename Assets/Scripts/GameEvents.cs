using System;

/// <summary>
/// Статический класс для централизованного управления игровыми событиями,
/// связанными с ходами игроков в пошаговой игре.
/// Позволяет разным компонентам подписываться на события начала и конца хода.
/// </summary>
public static class GameEvents
{
    /// <summary>
    /// Событие, вызываемое при начале хода игрока.
    /// Параметр — clientId игрока, чей ход начался.
    /// </summary>
    public static event Action<ulong> OnTurnStarted;

    /// <summary>
    /// Событие, вызываемое при окончании хода игрока.
    /// Параметр — clientId игрока, чей ход закончился.
    /// </summary>
    public static event Action<ulong> OnTurnEnded;

    /// <summary>
    /// Метод для вызова события начала хода.
    /// </summary>
    /// <param name="playerId">Id игрока, чей ход начался.</param>
    public static void TriggerTurnStarted(ulong playerId)
    {
        OnTurnStarted?.Invoke(playerId);
    }

    /// <summary>
    /// Метод для вызова события окончания хода.
    /// </summary>
    /// <param name="playerId">Id игрока, чей ход закончился.</param>
    public static void TriggerTurnEnded(ulong playerId)
    {
        OnTurnEnded?.Invoke(playerId);
    }
}
