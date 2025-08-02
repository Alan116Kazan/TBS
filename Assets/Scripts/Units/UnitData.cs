using UnityEngine;

/// <summary>
/// ���������, ��������������� ������ � ��������������� �����,
/// ���������� � ScriptableObject UnitStats.
/// </summary>
public class UnitData : MonoBehaviour
{
    [Tooltip("�������� �������������� �����, �������� ����� ScriptableObject.")]
    [SerializeField]
    private UnitStats stats;

    public UnitStats Stats => stats;

    /// <summary>
    /// ������������ ��������� ������������ �����.
    /// ���������� �� ������� UnitStats.
    /// </summary>
    public float MaxMoveDistance => stats != null ? stats.maxMoveDistance : 0f;

    /// <summary>
    /// ������ ����� �����.
    /// ���������� �� ������� UnitStats.
    /// </summary>
    public float AttackRange => stats != null ? stats.attackRange : 0f;

    private void Awake()
    {
        if (stats == null)
        {
            Debug.LogWarning($"[UnitData] �� ������� {gameObject.name} �� �������� UnitStats!");
        }
    }
}
