using UnityEngine;

/// <summary>
/// Определяет прямоугольную зону для спавна объектов.
/// Позволяет получить случайную позицию внутри зоны.
/// </summary>
public class SpawnZone : MonoBehaviour
{
    [Tooltip("Размер зоны спавна по осям X (ширина) и Z (глубина)")]
    [SerializeField] private Vector2 size = new(3f, 3f);

    /// <summary>
    /// Возвращает случайную позицию внутри прямоугольной зоны спавна,
    /// относительно позиции объекта.
    /// Y фиксируется равным 0 — предполагается плоская поверхность.
    /// </summary>
    public Vector3 GetRandomSpawnPosition()
    {
        float halfWidth = size.x * 0.5f;
        float halfDepth = size.y * 0.5f;

        // Генерируем координаты внутри прямоугольника [-halfWidth, halfWidth], [-halfDepth, halfDepth]
        float x = Random.Range(-halfWidth, halfWidth);
        float z = Random.Range(-halfDepth, halfDepth);

        // Складываем с позицией объекта, возвращая мировую позицию
        return transform.position + new Vector3(x, 0f, z);
    }

    /// <summary>
    /// Визуализация зоны спавна в редакторе,
    /// когда объект выделен.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        // Рисуем проволочный прямоугольник, толщиной 0.01 по Y
        Gizmos.DrawWireCube(transform.position, new Vector3(size.x, 0.01f, size.y));
    }

    /// <summary>
    /// Публичное свойство для доступа к размеру зоны.
    /// </summary>
    public Vector2 Size => size;
}
