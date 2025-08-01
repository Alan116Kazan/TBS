using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI для отображения состояния выбранного юнита:
/// сколько осталось движения и доступна ли атака.
/// </summary>
public class UnitStatusUI : MonoBehaviour
{
    [SerializeField] private Text moveDistanceText;
    [SerializeField] private Text canAttackText;

    private UnitController trackedUnit;

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

    public void SetUnit(UnitController unit)
    {
        trackedUnit = unit;
        UpdateUI();
    }

    public void Clear()
    {
        trackedUnit = null;
        moveDistanceText.text = "";
        canAttackText.text = "";
    }

    private void OnUnitChanged(UnitController unit)
    {
        if (unit == trackedUnit)
            UpdateUI();
    }

    private void UpdateUI()
    {
        if (trackedUnit == null) return;

        moveDistanceText.text = $"Осталось хода: {trackedUnit.RemainingMoveDistance:0.0} м";
        canAttackText.text = trackedUnit.HasAttacked ? "Атака: Использована" : "Атака: Доступна";
    }
}
