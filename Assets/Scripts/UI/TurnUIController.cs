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
    [SerializeField] private GameObject gameOverPanel;       // ������ ����� ����
    [SerializeField] private Text gameOverMessageText;       // ����� � ���������� � ������

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
            gameOverPanel.SetActive(false); // �������� ������ ����� ���� ��� ������
    }

    private void OnEnable()
    {
        GameEvents.OnTurnStarted += HandleTurnStarted;
        GameEvents.OnTurnTimerUpdated += HandleTimerUpdated;
        GameEvents.OnRoundChanged += HandleRoundChanged;

        GameEvents.OnGameEnded += HandleGameEnded; // �������� �� ��������� ����
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
            return; // ���� ���� ����������� � �� ��������� ���

        bool isMyTurn = activePlayerId == _myId;
        turnText.text = isMyTurn ? "��� ���" : "�������� ���� ���������...";
        endTurnButton.interactable = isMyTurn;

        timerText.text = "";
        UpdateRoundUI();
    }

    private void HandleTimerUpdated(float timeLeft)
    {
        if (gameOverPanel != null && gameOverPanel.activeSelf)
            return; // ���� ���� ����������� � �� ��������� ������

        TimeSpan time = TimeSpan.FromSeconds(Mathf.Ceil(timeLeft));
        timerText.text = $"��������: {time.Minutes:00}:{time.Seconds:00}";
    }

    private void HandleRoundChanged(int round)
    {
        if (gameOverPanel != null && gameOverPanel.activeSelf)
            return; // ���� ���� ����������� � �� ��������� �����

        roundText.text = $"�����: {round}";
    }

    private void HandleGameEnded(ulong winnerClientId)
    {
        if (gameOverPanel == null || gameOverMessageText == null)
        {
            Debug.LogWarning("GameOver UI elements are not assigned.");
            return;
        }

        // ���������� ������ ����� ����
        gameOverPanel.SetActive(true);

        // ����� ��������� � ����������
        // ���� �����, ����� �������� clientId �� ��� ������ (���� � ���� ���� ����� ������)
        gameOverMessageText.text = $"���� ��������!\n������� ����� {winnerClientId}";

        // ��������� ������ ����, ����� ������ ���� ������ ������
        endTurnButton.interactable = false;
    }

    private void UpdateRoundUI()
    {
        if (TurnManager.Instance != null)
            roundText.text = $"�����: {TurnManager.Instance.CurrentRound}";
    }

    private void OnEndTurnClicked()
    {
        TurnManager.Instance?.EndTurnServerRpc();
    }
}
