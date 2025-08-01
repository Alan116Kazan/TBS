using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System;

public class TurnUIController : MonoBehaviour
{
    [SerializeField] private Button endTurnButton;
    [SerializeField] private Text turnText;
    [SerializeField] private Text timerText;
    [SerializeField] private Text roundText;

    [Header("Game Over UI")]
    [SerializeField] private GameObject gameOverPanel;       // Панель конца игры
    [SerializeField] private Text gameOverMessageText;       // Текст с сообщением о победе

    private ulong _myId;

    private void Start()
    {
        _myId = NetworkManager.Singleton.LocalClientId;
        endTurnButton.onClick.AddListener(OnEndTurnClicked);

        if (TurnManager.Instance != null)
        {
            HandleTurnStarted(TurnManager.Instance.CurrentPlayerId);
            UpdateRoundUI();
        }

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false); // Скрываем панель конца игры при старте
    }

    private void OnEnable()
    {
        GameEvents.OnTurnStarted += HandleTurnStarted;
        GameEvents.OnTurnTimerUpdated += HandleTimerUpdated;
        GameEvents.OnRoundChanged += HandleRoundChanged;

        GameEvents.OnGameEnded += HandleGameEnded; // Подписка на окончание игры
    }

    private void OnDisable()
    {
        GameEvents.OnTurnStarted -= HandleTurnStarted;
        GameEvents.OnTurnTimerUpdated -= HandleTimerUpdated;
        GameEvents.OnRoundChanged -= HandleRoundChanged;

        GameEvents.OnGameEnded -= HandleGameEnded;
    }

    private void HandleTurnStarted(ulong activePlayerId)
    {
        if (gameOverPanel != null && gameOverPanel.activeSelf)
            return; // Если игра закончилась — не обновляем ход

        bool isMyTurn = activePlayerId == _myId;
        turnText.text = isMyTurn ? "Ваш ход" : "Ожидайте хода соперника...";
        endTurnButton.interactable = isMyTurn;

        timerText.text = "";
        UpdateRoundUI();
    }

    private void HandleTimerUpdated(float timeLeft)
    {
        if (gameOverPanel != null && gameOverPanel.activeSelf)
            return; // Если игра закончилась — не обновляем таймер

        TimeSpan time = TimeSpan.FromSeconds(Mathf.Ceil(timeLeft));
        timerText.text = $"Осталось: {time.Minutes:00}:{time.Seconds:00}";
    }

    private void HandleRoundChanged(int round)
    {
        if (gameOverPanel != null && gameOverPanel.activeSelf)
            return; // Если игра закончилась — не обновляем раунд

        roundText.text = $"Раунд: {round}";
    }

    private void HandleGameEnded(ulong winnerClientId)
    {
        if (gameOverPanel == null || gameOverMessageText == null)
        {
            Debug.LogWarning("GameOver UI elements are not assigned.");
            return;
        }

        // Показываем панель конца игры
        gameOverPanel.SetActive(true);

        // Пишем сообщение о победителе
        // Если нужно, можно заменить clientId на имя игрока (если у тебя есть такая логика)
        gameOverMessageText.text = $"Игра окончена!\nПобедил игрок {winnerClientId}";

        // Отключаем кнопку хода, чтобы нельзя было больше ходить
        endTurnButton.interactable = false;
    }

    private void UpdateRoundUI()
    {
        if (TurnManager.Instance != null)
            roundText.text = $"Раунд: {TurnManager.Instance.CurrentRound}";
    }

    private void OnEndTurnClicked()
    {
        TurnManager.Instance?.EndTurnServerRpc();
    }
}
