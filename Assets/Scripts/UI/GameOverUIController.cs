using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Отвечает за отображение экрана конца игры.
/// Подписывается на событие завершения игры и показывает сообщение о победителе.
/// </summary>
public class GameOverUIController : MonoBehaviour
{
    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text gameOverMessageText;

    private void Start()
    {
        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);
    }

    private void OnEnable()
    {
        GameEvents.OnGameEnded += HandleGameEnded;
    }

    private void OnDisable()
    {
        GameEvents.OnGameEnded -= HandleGameEnded;
    }

    /// <summary>
    /// Отображает панель завершения игры с сообщением о победителе.
    /// </summary>
    /// <param name="winnerClientId">ID победившего клиента.</param>
    private void HandleGameEnded(ulong winnerClientId)
    {
        if (gameOverPanel == null || gameOverMessageText == null)
            return;

        gameOverPanel.SetActive(true);
        gameOverMessageText.text = $"Игра окончена!\nПобедил игрок {winnerClientId}";
    }
}
