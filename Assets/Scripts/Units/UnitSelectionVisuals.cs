using UnityEngine;

/// <summary>
/// Отвечает за визуальное отображение состояния юнита:
/// - выбран/не выбран
/// - цель для атаки
/// - радиус атаки
/// </summary>
public class UnitSelectionVisuals : MonoBehaviour
{
    [Header("Ссылки на визуальные элементы")]
    [SerializeField] private GameObject selectionCircle;         // Обводка вокруг выбранного юнита
    [SerializeField] private GameObject attackTargetHighlight;   // Подсветка при наведении как на цель
    [SerializeField] private AttackRangeVisualizer rangeVisualizer; // Компонент отрисовки радиуса атаки

    private float _attackRange;

    /// <summary>
    /// Метод Reset вызывается в редакторе, если нажать "Reset" на компоненте.
    /// Здесь происходит автоматическое присвоение компонента rangeVisualizer.
    /// </summary>
    private void Reset()
    {
        // Если не задан вручную — найти среди компонентов объекта
        if (rangeVisualizer == null)
            rangeVisualizer = GetComponent<AttackRangeVisualizer>() ?? GetComponentInChildren<AttackRangeVisualizer>();
    }

    /// <summary>
    /// Инициализация визуализатора — передаём радиус атаки, который потом будет использоваться для отображения круга.
    /// </summary>
    public void Initialize(float attackRange)
    {
        this._attackRange = attackRange;
    }

    /// <summary>
    /// Отображает/скрывает круг выбора и радиус атаки в зависимости от состояния.
    /// </summary>
    /// <param name="selected">Выбран ли юнит</param>
    /// <param name="hasAttacked">Совершил ли он атаку (если да — радиус не показываем)</param>
    public void ShowSelection(bool selected, bool hasAttacked)
    {
        // Включаем обводку, если юнит выбран
        selectionCircle?.SetActive(selected);

        // Показываем радиус атаки, только если юнит выбран и ещё может атаковать
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
    /// Подсветка, когда на юнита наводятся как на цель атаки.
    /// </summary>
    /// <param name="selected">Наведён ли курсор/выделен ли как цель</param>
    public void ShowAttackTargetHighlight(bool selected)
    {
        attackTargetHighlight?.SetActive(selected);
    }
}
