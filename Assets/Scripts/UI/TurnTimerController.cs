using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using System;

/// <summary>
/// ���������� UI ��� ����������� ������� ����.
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
    /// ��������� ����������� ����������� ������� ����.
    /// </summary>
    /// <param name="timeLeft">������ �� ����� ����.</param>
    private void HandleTimerUpdated(float timeLeft)
    {
        TimeSpan time = TimeSpan.FromSeconds(Mathf.Ceil(timeLeft));
        timerText.text = $"��������: {time.Minutes:00}:{time.Seconds:00}";
    }

    /// <summary>
    /// �������� ������ ��� ���������� ����.
    /// </summary>
    /// <param name="winnerClientId">ID ����������� ������.</param>
    private void HandleGameEnded(ulong winnerClientId)
    {
        timerText.text = "";
    }
}
