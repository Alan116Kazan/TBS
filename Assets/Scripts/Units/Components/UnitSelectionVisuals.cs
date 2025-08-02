using UnityEngine;

/// <summary>
/// �������� �� ���������� ����������� ��������� �����:
/// - ������� �� ���� (�������)
/// - ������ �� ��� ���� ����� (���������)
/// - ������������ ������� ����� ����� AttackRangeVisualizer
/// </summary>
public class UnitSelectionVisuals : MonoBehaviour
{
    [Header("���������� ��������")]

    [SerializeField]
    private GameObject selectionCircle;

    [SerializeField]
    private GameObject attackTargetHighlight;

    [SerializeField]
    private AttackRangeVisualizer rangeVisualizer;

    private float _attackRange;

    
    private void Reset()
    {
        if (rangeVisualizer == null)
        {
            rangeVisualizer = GetComponent<AttackRangeVisualizer>()
                           ?? GetComponentInChildren<AttackRangeVisualizer>();
        }
    }

    /// <summary>
    /// �������������� ������������ �������� �����.
    /// </summary>
    /// <param name="attackRange">������ ����� �����</param>
    public void Initialize(float attackRange)
    {
        _attackRange = attackRange;
    }

    /// <summary>
    /// ����������/�������� ���������� ���������� ��������� � ������ �����.
    /// ������ ������������ ������ ���� ���� �� ��������.
    /// </summary>
    /// <param name="selected">�������� �� ���� ���������</param>
    /// <param name="hasAttacked">��� �� �������� ����</param>
    public void ShowSelection(bool selected, bool hasAttacked)
    {
        selectionCircle?.SetActive(selected);

        bool showRange = selected && !hasAttacked;

        if (rangeVisualizer != null)
        {
            rangeVisualizer.Show(showRange);

            if (showRange)
            {
                rangeVisualizer.SetRange(_attackRange);
                rangeVisualizer.Draw();
            }
        }
    }

    /// <summary>
    /// ������������ ����� ��� ���� �����.
    /// </summary>
    /// <param name="selected">��������/��������� ���������</param>
    public void ShowAttackTargetHighlight(bool selected)
    {
        attackTargetHighlight?.SetActive(selected);
    }
}
