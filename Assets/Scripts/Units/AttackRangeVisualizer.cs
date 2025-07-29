using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class AttackRangeVisualizer : MonoBehaviour
{
    [SerializeField] private int segments = 40;
    [SerializeField] private float range = 5f;

    private LineRenderer _lineRenderer;

    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.useWorldSpace = false;
    }

    public void SetRange(float newRange)
    {
        range = newRange;
    }

    public void Draw()
    {
        if (_lineRenderer == null) return;

        _lineRenderer.positionCount = segments + 1;
        float angleStep = 360f / segments;

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            float x = Mathf.Sin(angle) * range;
            float z = Mathf.Cos(angle) * range;
            _lineRenderer.SetPosition(i, new Vector3(x, 0.01f, z));
        }
    }

    public void Show(bool show)
    {
        gameObject.SetActive(show);
    }
}
