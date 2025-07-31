using UnityEngine;

/// <summary>
/// ScriptableObject, ������� ������ �������� �������������� �����,
/// ����� ��� ������������ ��������� ������������ � ������ �����.
/// ��������� ����� ��������� � ����������� ������ ���� ������ � ��������� Unity.
/// </summary>
[CreateAssetMenu(menuName = "Units/Stats")]
public class UnitStats : ScriptableObject
{
    /// <summary>
    /// ������������ ��������� ������������ ����� �� ���, � ������.
    /// </summary>
    public float maxMoveDistance = 5f;

    /// <summary>
    /// ������ ����� �����, � ������.
    /// ���������� ����������� ���������� ���������� �� ���� ��� �����.
    /// </summary>
    public float attackRange = 2f;
}
