// IMovable.cs
using UnityEngine;

/// <summary>
/// ���������, ����������� ��������� ������������ �������.
/// ��������� �������� ������������� � ���� � ���������� ���������� ���������� ��������.
/// </summary>
public interface IMovable
{
    /// <summary>
    /// ���������� ����������, ������� ������ ����� ������ �� ������� ���.
    /// </summary>
    float RemainingMoveDistance { get; }

    /// <summary>
    /// �������� ����������� ������ � ��������� �������.
    /// </summary>
    /// <param name="targetPosition">������� ������� ��� �����������.</param>
    void TryMove(Vector3 targetPosition);

    /// <summary>
    /// ���������� ��������� �������� (��������, � ������ ������ ����).
    /// </summary>
    void ResetMovement();
}
