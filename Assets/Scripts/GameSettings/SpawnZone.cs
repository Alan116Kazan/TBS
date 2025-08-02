using UnityEngine;

/// <summary>
/// Определяет зону спавна юнитов
/// </summary>
public class SpawnZone : MonoBehaviour
{
    [Tooltip("Размер зоны спавна по осям X (ширина) и Z (глубина)")]
    [SerializeField] private Vector2 size = new(3f, 3f);

    /// <summary>
    /// Возвращает случайную позицию внутри прямоугольной зоны спавна,
    /// относительно позиции объекта.
    /// </summary>
    public Vector3 GetRandomSpawnPosition()
    {
        float halfWidth = size.x * 0.5f;
        float halfDepth = size.y * 0.5f;

        float x = Random.Range(-halfWidth, halfWidth);
        float z = Random.Range(-halfDepth, halfDepth);

        return transform.position + new Vector3(x, 0f, z);
    }

    #region Editor Gizmos

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(size.x, 0.01f, size.y));
    }
#endif

    #endregion
    public Vector2 Size => size;
}
