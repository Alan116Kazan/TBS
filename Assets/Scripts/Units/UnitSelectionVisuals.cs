using UnityEngine;

/// <summary>
/// Отвечает за визуальное отображение состояния юнита:
/// - выбран/не выбран (обводка вокруг юнита)
/// - подсветка, если юнит является целью атаки
/// - визуализация радиуса атаки с помощью компонента AttackRangeVisualizer
/// </summary>
public class UnitSelectionVisuals : MonoBehaviour
{
    [Header("Ссылки на визуальные элементы")]

    [SerializeField]
    private GameObject selectionCircle;           // Обводка, показывающая, что юнит выбран

    [SerializeField]
    private GameObject attackTargetHighlight;     // Подсветка юнита как цели для атаки

    [SerializeField]
    private AttackRangeVisualizer rangeVisualizer; // Компонент для отрисовки радиуса атаки

    private float _attackRange;                    // Текущий радиус атаки, который будет визуализироваться

    /// <summary>
    /// Метод Reset вызывается в редакторе Unity при нажатии кнопки Reset компонента.
    /// Автоматически пытается найти компонент AttackRangeVisualizer, если он не назначен вручную.
    /// </summary>
    private void Reset()
    {
        if (rangeVisualizer == null)
            rangeVisualizer = GetComponent<AttackRangeVisualizer>() ?? GetComponentInChildren<AttackRangeVisualizer>();
    }

    /// <summary>
    /// Инициализация визуализатора: передача радиуса атаки,
    /// который затем используется для отрисовки круга.
    /// </summary>
    /// <param name="attackRange">Радиус атаки юнита</param>
    public void Initialize(float attackRange)
    {
        _attackRange = attackRange;
    }

    /// <summary>
    /// Управляет отображением обводки выбора и визуализацией радиуса атаки.
    /// Радиус отображается только если юнит выбран и ещё не атаковал.
    /// </summary>
    /// <param name="selected">Является ли юнит выбранным</param>
    /// <param name="hasAttacked">Совершил ли юнит атаку (если да, радиус не показываем)</param>
    public void ShowSelection(bool selected, bool hasAttacked)
    {
        // Показываем или скрываем обводку выбора
        selectionCircle?.SetActive(selected);

        // Показываем радиус атаки только если выбран и ещё может атаковать
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
    /// Управляет подсветкой юнита как цели атаки.
    /// Включается, когда юнит выбран как цель или наведён курсор.
    /// </summary>
    /// <param name="selected">Включить ли подсветку</param>
    public void ShowAttackTargetHighlight(bool selected)
    {
        attackTargetHighlight?.SetActive(selected);
    }
}
