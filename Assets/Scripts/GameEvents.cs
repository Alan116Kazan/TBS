using System;

/// <summary>
/// ����������� ����� ��� ����������������� ���������� �������� ���������,
/// ���������� � ������ ������� � ��������� ����.
/// ��������� ������ ����������� ������������� �� ������� ������ � ����� ����.
/// </summary>
public static class GameEvents
{
    /// <summary>
    /// �������, ���������� ��� ������ ���� ������.
    /// �������� � clientId ������, ��� ��� �������.
    /// </summary>
    public static event Action<ulong> OnTurnStarted;

    /// <summary>
    /// �������, ���������� ��� ��������� ���� ������.
    /// �������� � clientId ������, ��� ��� ����������.
    /// </summary>
    public static event Action<ulong> OnTurnEnded;

    /// <summary>
    /// ����� ��� ������ ������� ������ ����.
    /// </summary>
    /// <param name="playerId">Id ������, ��� ��� �������.</param>
    public static void TriggerTurnStarted(ulong playerId)
    {
        OnTurnStarted?.Invoke(playerId);
    }

    /// <summary>
    /// ����� ��� ������ ������� ��������� ����.
    /// </summary>
    /// <param name="playerId">Id ������, ��� ��� ����������.</param>
    public static void TriggerTurnEnded(ulong playerId)
    {
        OnTurnEnded?.Invoke(playerId);
    }
}
