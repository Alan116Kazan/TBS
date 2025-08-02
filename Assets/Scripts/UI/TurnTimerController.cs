using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System;

/// <summary>
/// Контроллер UI для отображения таймера хода.
/// </summary>
public class TurnTimerController : MonoBehaviour
{
    [SerializeField] private Text timerText;

    private void OnEnable()
    {
        GameEvents.OnTurnTimerUpdated += HandleTimerUpdated;
        GameEvents.OnGameEnded += HandleGameEnded;
    }

    private void OnDisable()
    {
        GameEvents.OnTurnTimerUpdated -= HandleTimerUpdated;
        GameEvents.OnGameEnded -= HandleGameEnded;
    }

    /// <summary>
    /// Обновляет отображение оставшегося времени хода.
    /// </summary>
    /// <param name="timeLeft">Секунд до конца хода.</param>
    private void HandleTimerUpdated(float timeLeft)
    {
        TimeSpan time = TimeSpan.FromSeconds(Mathf.Ceil(timeLeft));
        timerText.text = $"Осталось: {time.Minutes:00}:{time.Seconds:00}";
    }

    /// <summary>
    /// Скрывает таймер при завершении игры.
    /// </summary>
    /// <param name="winnerClientId">ID победившего игрока.</param>
    private void HandleGameEnded(ulong winnerClientId)
    {
        timerText.text = "";
    }
}
