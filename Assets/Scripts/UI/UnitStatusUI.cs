using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI-��������� ��� ����������� ��������� ���������� �����:
/// ���������� ���������� �������� � ������ �����.
/// </summary>
public class UnitStatusUI : MonoBehaviour
{
    [SerializeField] private Text moveDistanceText;
    [SerializeField] private Text canAttackText;

    private UnitController _trackedUnit;

    private void OnEnable()
    {
        GameEvents.OnUnitMoved += OnUnitChanged;
        GameEvents.OnUnitAttacked += OnUnitChanged;
    }

    private void OnDisable()
    {
        GameEvents.OnUnitMoved -= OnUnitChanged;
        GameEvents.OnUnitAttacked -= OnUnitChanged;
    }

    /// <summary>
    /// ������������� �����, �� ������� ����� ������� UI.
    /// </summary>
    /// <param name="unit">���� ��� ������������.</param>
    public void SetUnit(UnitController unit)
    {
        _trackedUnit = unit;
        UpdateUI();
    }

    /// <summary>
    /// ����� UI ��� ���������� ���������� �����.
    /// </summary>
    public void Clear()
    {
        _trackedUnit = null;
        moveDistanceText.text = "";
        canAttackText.text = "";
    }

    /// <summary>
    /// ��������� UI, ���� ������� �������� �������������� �����.
    /// </summary>
    private void OnUnitChanged(UnitController unit)
    {
        if (unit == _trackedUnit)
            UpdateUI();
    }

    /// <summary>
    /// ��������� ����������� ����������� ���� � ������� �����.
    /// </summary>
    private void UpdateUI()
    {
        if (_trackedUnit == null) return;

        moveDistanceText.text = $"�������� ����: {_trackedUnit.RemainingMoveDistance:0.0} �";
        canAttackText.text = _trackedUnit.HasAttacked ? "�����: ������������" : "�����: ��������";
    }
}
