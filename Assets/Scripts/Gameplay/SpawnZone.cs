using UnityEngine;

/// <summary>
/// ���������� ������������� ���� ��� ������ ��������.
/// ��������� �������� ��������� ������� ������ ����.
/// </summary>
public class SpawnZone : MonoBehaviour
{
    [Tooltip("������ ���� ������ �� ���� X (������) � Z (�������)")]
    [SerializeField] private Vector2 size = new(3f, 3f);

    /// <summary>
    /// ���������� ��������� ������� ������ ������������� ���� ������,
    /// ������������ ������� �������.
    /// Y ����������� ������ 0 � �������������� ������� �����������.
    /// </summary>
    public Vector3 GetRandomSpawnPosition()
    {
        float halfWidth = size.x * 0.5f;
        float halfDepth = size.y * 0.5f;

        // ���������� ��������� ���������� ������ �������������� [-halfWidth, halfWidth] � [-halfDepth, halfDepth]
        float x = Random.Range(-halfWidth, halfWidth);
        float z = Random.Range(-halfDepth, halfDepth);

        // ���������� ������� � ������� �����������, �������� �������� � ������� �������
        return transform.position + new Vector3(x, 0f, z);
    }

    /// <summary>
    /// ������������ ���� ������ � ���������, ����� ������ �������.
    /// ������ ����������� ������������� ��� �������� ��������������.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        // ������ ����������� ��� � ��������� size �� X � Z, � ����� ��������� ������� �� Y (0.01)
        Gizmos.DrawWireCube(transform.position, new Vector3(size.x, 0.01f, size.y));
    }

    /// <summary>
    /// ��������� �������� ��� ������� � ������� ���� ������.
    /// </summary>
    public Vector2 Size => size;
}
