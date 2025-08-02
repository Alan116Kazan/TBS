using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �������� �� ����������� ������ ����� ����.
/// ������������� �� ������� ���������� ���� � ���������� ��������� � ����������.
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
    /// ���������� ������ ���������� ���� � ���������� � ����������.
    /// </summary>
    /// <param name="winnerClientId">ID ����������� �������.</param>
    private void HandleGameEnded(ulong winnerClientId)
    {
        if (gameOverPanel == null || gameOverMessageText == null)
            return;

        gameOverPanel.SetActive(true);
        gameOverMessageText.text = $"���� ��������!\n������� ����� {winnerClientId}";
    }
}
