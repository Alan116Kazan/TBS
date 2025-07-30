using UnityEngine;

/// <summary>
/// Компонент, визуализирующий радиус атаки юнита в виде круга с помощью LineRenderer.
/// Требует наличие компонента LineRenderer на том же объекте.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class AttackRangeVisualizer : MonoBehaviour
{
    [Header("Настройки круга")]
    [SerializeField] private int segments = 40; // Количество сегментов круга (чем больше, тем круглее)
    [SerializeField] private float range = 5f;  // Радиус круга (дальность атаки)

    private LineRenderer _lineRenderer;

    /// <summary>
    /// В методе Awake кэшируем компонент LineRenderer и настраиваем его.
    /// Используем локальные координаты (useWorldSpace = false), чтобы круг масштабировался с объектом.
    /// </summary>
    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.useWorldSpace = false;
    }

    /// <summary>
    /// Устанавливает новый радиус круга (например, если дальность атаки меняется).
    /// </summary>
    public void SetRange(float newRange)
    {
        range = newRange;
    }

    /// <summary>
    /// Строит круг на основе заданного радиуса и количества сегментов.
    /// Задаёт точки LineRenderer'а по окружности.
    /// </summary>
    public void Draw()
    {
        if (_lineRenderer == null) return;

        _lineRenderer.positionCount = segments + 1; // +1, чтобы замкнуть круг
        float angleStep = 360f / segments;

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad; // перевод в радианы
            float x = Mathf.Sin(angle) * range;
            float z = Mathf.Cos(angle) * range;
            _lineRenderer.SetPosition(i, new Vector3(x, 0.01f, z)); // немного приподнят, чтобы не перекрывало пол
        }
    }

    /// <summary>
    /// Включает или выключает отображение круга.
    /// </summary>
    public void Show(bool show)
    {
        gameObject.SetActive(show);
    }
}
