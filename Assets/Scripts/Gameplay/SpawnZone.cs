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

        // Генерируем случайные координаты внутри прямоугольника [-halfWidth, halfWidth] и [-halfDepth, halfDepth]
        float x = Random.Range(-halfWidth, halfWidth);
        float z = Random.Range(-halfDepth, halfDepth);

        // Возвращаем позицию в мировых координатах, добавляя смещение к позиции объекта
        return transform.position + new Vector3(x, 0f, z);
    }

    /// <summary>
    /// Визуализация зоны спавна в редакторе, когда объект выделен.
    /// Рисует проволочный прямоугольник для удобства редактирования.
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        // Рисуем проволочный куб с размерами size по X и Z, с очень маленькой высотой по Y (0.01)
        Gizmos.DrawWireCube(transform.position, new Vector3(size.x, 0.01f, size.y));
    }

    /// <summary>
    /// Публичное свойство для доступа к размеру зоны спавна.
    /// </summary>
    public Vector2 Size => size;
}
