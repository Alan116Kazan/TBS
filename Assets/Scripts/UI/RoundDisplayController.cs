using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// �������� �� ����������� �������� ������ ������ �� UI.
/// ������������� �� ������� ����� ������ � ��������� �����.
/// </summary>
public class RoundDisplayController : MonoBehaviour
{
    [SerializeField] private Text roundText;

    private void Start()
    {
        UpdateRoundUI();
    }

    private void OnEnable()
    {
        GameEvents.OnRoundChanged += HandleRoundChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnRoundChanged -= HandleRoundChanged;
    }

    /// <summary>
    /// ��������� ����������� ������ ������ ��� ��� ���������.
    /// </summary>
    /// <param name="round">����� �������� ������.</param>
    private void HandleRoundChanged(int round)
    {
        roundText.text = $"�����: {round}";
    }

    /// <summary>
    /// �������������� ����� ��� ������ �� ������ ������ TurnManager.
    /// </summary>
    private void UpdateRoundUI()
    {
        if (TurnManager.Instance != null)
            roundText.text = $"�����: {TurnManager.Instance.CurrentRound}";
    }
}
