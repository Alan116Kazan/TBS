using UnityEngine;

/// <summary>
/// Простой логгер, который подписывается на события начала и конца хода
/// из класса GameEvents и выводит сообщения в консоль Unity.
/// Полезен для отладки и проверки корректности работы системы ходов.
/// </summary>
public class TurnEventsLogger : MonoBehaviour
{
    /// <summary>
    /// При включении компонента подписываемся на события начала и конца хода.
    /// </summary>
    private void OnEnable()
    {
        GameEvents.OnTurnStarted += OnTurnStarted;
        GameEvents.OnTurnEnded += OnTurnEnded;
    }

    /// <summary>
    /// При отключении компонента отписываемся от событий,
    /// чтобы избежать утечек памяти и двойных вызовов.
    /// </summary>
    private void OnDisable()
    {
        GameEvents.OnTurnStarted -= OnTurnStarted;
        GameEvents.OnTurnEnded -= OnTurnEnded;
    }

    /// <summary>
    /// Обработчик события начала хода — выводит в консоль информацию о том,
    /// какой игрок начал свой ход.
    /// </summary>
    private void OnTurnStarted(ulong playerId)
    {
        Debug.Log($"Turn started for player {playerId}");
    }

    /// <summary>
    /// Обработчик события окончания хода — выводит в консоль информацию о том,
    /// какой игрок завершил свой ход.
    /// </summary>
    private void OnTurnEnded(ulong playerId)
    {
        Debug.Log($"Turn ended for player {playerId}");
    }
}
