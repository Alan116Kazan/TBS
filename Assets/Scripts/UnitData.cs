using UnityEngine;

/// <summary>
/// ������ ������ �����, �������� �� ScriptableObject � ��� ����������������.
/// ������������ ������� ������ � ���������� �����.
/// </summary>
public class UnitData : MonoBehaviour
{
    // ������ �� ScriptableObject, ��� �������� �������� �������������� �����
    [SerializeField]
    private UnitStats stats;

    /// <summary>
    /// ��������� �������� ��� ������� � ScriptableObject �� ����������� �����.
    /// </summary>
    public UnitStats Stats => stats;

    /// <summary>
    /// ������������ ��������� ������������ ����� (������ �� stats).
    /// </summary>
    public float MaxMoveDistance => stats.maxMoveDistance;

    /// <summary>
    /// ������ ����� ����� (������ �� stats).
    /// </summary>
    public float AttackRange => stats.attackRange;
}
