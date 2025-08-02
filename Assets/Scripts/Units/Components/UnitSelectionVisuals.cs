using UnityEngine;

/// <summary>
/// Отвечает за визуальное отображение состояния юнита:
/// - выделен ли юнит (обводка)
/// - выбран ли как цель атаки (подсветка)
/// - визуализация радиуса атаки через AttackRangeVisualizer
/// </summary>
public class UnitSelectionVisuals : MonoBehaviour
{
    [Header("Визуальные элементы")]

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
    /// Инициализирует визуализатор радиусом атаки.
    /// </summary>
    /// <param name="attackRange">Радиус атаки юнита</param>
    public void Initialize(float attackRange)
    {
        _attackRange = attackRange;
    }

    /// <summary>
    /// Показывает/скрывает визуальные индикаторы выделения и радиус атаки.
    /// Радиус отображается только если юнит не атаковал.
    /// </summary>
    /// <param name="selected">Является ли юнит выбранным</param>
    /// <param name="hasAttacked">Уже ли атаковал юнит</param>
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
    /// Подсвечивает юнита как цель атаки.
    /// </summary>
    /// <param name="selected">Включить/отключить подсветку</param>
    public void ShowAttackTargetHighlight(bool selected)
    {
        attackTargetHighlight?.SetActive(selected);
    }
}
