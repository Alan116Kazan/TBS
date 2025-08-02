using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI-компонент для отображения состояния выбранного юнита:
/// оставшееся расстояние движения и статус атаки.
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
    /// Устанавливает юнита, за которым будет следить UI.
    /// </summary>
    /// <param name="unit">Юнит для отслеживания.</param>
    public void SetUnit(UnitController unit)
    {
        _trackedUnit = unit;
        UpdateUI();
    }

    /// <summary>
    /// Сброс UI при отсутствии выбранного юнита.
    /// </summary>
    public void Clear()
    {
        _trackedUnit = null;
        moveDistanceText.text = "";
        canAttackText.text = "";
    }

    /// <summary>
    /// Обновляет UI, если событие касается отслеживаемого юнита.
    /// </summary>
    private void OnUnitChanged(UnitController unit)
    {
        if (unit == _trackedUnit)
            UpdateUI();
    }

    /// <summary>
    /// Обновляет отображение оставшегося хода и статуса атаки.
    /// </summary>
    private void UpdateUI()
    {
        if (_trackedUnit == null) return;

        moveDistanceText.text = $"Осталось хода: {_trackedUnit.RemainingMoveDistance:0.0} м";
        canAttackText.text = _trackedUnit.HasAttacked ? "Атака: Использована" : "Атака: Доступна";
    }
}
