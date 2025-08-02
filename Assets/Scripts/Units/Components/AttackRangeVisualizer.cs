using UnityEngine;

/// <summary>
/// Компонент, визуализирующий радиус атаки юнита в виде круга с помощью LineRenderer.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class AttackRangeVisualizer : MonoBehaviour
{
    [Header("Настройки круга")]
    [SerializeField] private int segments = 40; // Количество сегментов круга
    [SerializeField] private float range = 5f;  // Радиус круга

    private LineRenderer _lineRenderer;

    /// <summary>
    /// В методе Awake кэшируем компонент LineRenderer и настраиваем его.
    /// </summary>
    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.useWorldSpace = false;
    }

    /// <summary>
    /// Устанавливает новый радиус круга
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

        _lineRenderer.positionCount = segments + 1; //
        float angleStep = 360f / segments;

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            float x = Mathf.Sin(angle) * range;
            float z = Mathf.Cos(angle) * range;

            _lineRenderer.SetPosition(i, new Vector3(x, 0.01f, z));
        }
    }

    /// <summary>
    /// Включает или выключает отображение круга радиуса атаки.
    /// </summary>
    public void Show(bool show)
    {
        gameObject.SetActive(show);
    }
}
