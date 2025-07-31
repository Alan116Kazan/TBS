using UnityEngine;

/// <summary>
/// ������� ������, ������� ������������� �� ������� ������ � ����� ����
/// �� ������ GameEvents � ������� ��������� � ������� Unity.
/// ������� ��� ������� � �������� ������������ ������ ������� �����.
/// </summary>
public class TurnEventsLogger : MonoBehaviour
{
    /// <summary>
    /// ��� ��������� ���������� ������������� �� ������� ������ � ����� ����.
    /// </summary>
    private void OnEnable()
    {
        GameEvents.OnTurnStarted += OnTurnStarted;
        GameEvents.OnTurnEnded += OnTurnEnded;
    }

    /// <summary>
    /// ��� ���������� ���������� ������������ �� �������,
    /// ����� �������� ������ ������ � ������� �������.
    /// </summary>
    private void OnDisable()
    {
        GameEvents.OnTurnStarted -= OnTurnStarted;
        GameEvents.OnTurnEnded -= OnTurnEnded;
    }

    /// <summary>
    /// ���������� ������� ������ ���� � ������� � ������� ���������� � ���,
    /// ����� ����� ����� ���� ���.
    /// </summary>
    private void OnTurnStarted(ulong playerId)
    {
        Debug.Log($"Turn started for player {playerId}");
    }

    /// <summary>
    /// ���������� ������� ��������� ���� � ������� � ������� ���������� � ���,
    /// ����� ����� �������� ���� ���.
    /// </summary>
    private void OnTurnEnded(ulong playerId)
    {
        Debug.Log($"Turn ended for player {playerId}");
    }
}
