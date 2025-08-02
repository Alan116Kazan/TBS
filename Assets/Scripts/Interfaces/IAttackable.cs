using UnityEngine;

/// <summary>
/// ���������, ����������� ��������� �������� �����:
/// �������� ����������� �����, ���������� ����� � � �����.
/// </summary>
public interface IAttackable
{
    /// <summary>
    /// ��� �� ���� ��������� � ������� ����.
    /// </summary>
    bool HasAttacked { get; }

    /// <summary>
    /// ���������, ��������� �� ���� � �������� ������� �����.
    /// </summary>
    /// <param name="targetPosition">������� ����.</param>
    /// <returns>True � ���� � �������; ����� � false.</returns>
    bool IsTargetInRange(Vector3 targetPosition);

    /// <summary>
    /// ��������� ������� ����� �� ����.
    /// </summary>
    /// <param name="targetPosition">������� ����.</param>
    void TryAttack(Vector3 targetPosition);

    /// <summary>
    /// ����� ����� �����, ���������� ��� ���������� ����.
    /// </summary>
    void ResetAttack();
}
