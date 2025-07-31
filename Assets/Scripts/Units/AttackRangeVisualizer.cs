using UnityEngine;

/// <summary>
/// ���������, ��������������� ������ ����� ����� � ���� ����� � ������� LineRenderer.
/// ������� ������� ���������� LineRenderer �� ��� �� �������.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class AttackRangeVisualizer : MonoBehaviour
{
    [Header("��������� �����")]
    [SerializeField] private int segments = 40; // ���������� ��������� ����� (��� ������ � ��� �������)
    [SerializeField] private float range = 5f;  // ������ ����� (��������� �����)

    private LineRenderer _lineRenderer;

    /// <summary>
    /// � ������ Awake �������� ��������� LineRenderer � ����������� ���.
    /// ���������� ��������� ���������� (useWorldSpace = false), ����� ���� ��������������� ������ � ��������.
    /// </summary>
    private void Awake()
    {
        _lineRenderer = GetComponent<LineRenderer>();
        _lineRenderer.useWorldSpace = false;
    }

    /// <summary>
    /// ������������� ����� ������ ����� (��������, ���� ��������� ����� ��������).
    /// </summary>
    public void SetRange(float newRange)
    {
        range = newRange;
    }

    /// <summary>
    /// ������ ���� �� ������ ��������� ������� � ���������� ���������.
    /// ����� ����� LineRenderer'� �� ����������.
    /// </summary>
    public void Draw()
    {
        if (_lineRenderer == null) return;

        _lineRenderer.positionCount = segments + 1; // +1 ��� ��������� �����
        float angleStep = 360f / segments;

        for (int i = 0; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad; // ����������� �������� � �������
            float x = Mathf.Sin(angle) * range;
            float z = Mathf.Cos(angle) * range;

            // ������������� ������� ����� ����� (Y = 0.01 ��� ���������� �������� ��� ������������)
            _lineRenderer.SetPosition(i, new Vector3(x, 0.01f, z));
        }
    }

    /// <summary>
    /// �������� ��� ��������� ����������� ����� ������� �����.
    /// </summary>
    public void Show(bool show)
    {
        gameObject.SetActive(show);
    }
}
