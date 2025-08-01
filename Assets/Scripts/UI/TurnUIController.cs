using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System;

public class TurnUIController : MonoBehaviour
{
    [SerializeField] private Button endTurnButton;
    [SerializeField] private Text turnText;
    [SerializeField] private Text timerText;
    [SerializeField] private Text roundText; // ����� ���� � ������� � ����������

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
    }

    private void OnEnable()
    {
        GameEvents.OnTurnStarted += HandleTurnStarted;
        GameEvents.OnTurnTimerUpdated += HandleTimerUpdated;
        GameEvents.OnRoundChanged += HandleRoundChanged; // �������� �� ���������� ������
    }

    private void OnDisable()
    {
        GameEvents.OnTurnStarted -= HandleTurnStarted;
        GameEvents.OnTurnTimerUpdated -= HandleTimerUpdated;
        GameEvents.OnRoundChanged -= HandleRoundChanged;
    }

    private void HandleTurnStarted(ulong activePlayerId)
    {
        bool isMyTurn = activePlayerId == _myId;
        turnText.text = isMyTurn ? "��� ���" : "�������� ���� ���������...";
        endTurnButton.interactable = isMyTurn;

        timerText.text = "";
        UpdateRoundUI();
    }

    private void HandleTimerUpdated(float timeLeft)
    {
        TimeSpan time = TimeSpan.FromSeconds(Mathf.Ceil(timeLeft));
        timerText.text = $"��������: {time.Minutes:00}:{time.Seconds:00}";
    }

    private void HandleRoundChanged(int round)
    {
        roundText.text = $"�����: {round}";
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
