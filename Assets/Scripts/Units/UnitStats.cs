using UnityEngine;

/// <summary>
/// ScriptableObject, который хранит основные характеристики юнита,
/// такие как максимальная дистанция передвижения и радиус атаки.
/// Позволяет легко создавать и настраивать разные типы юнитов в редакторе Unity.
/// </summary>
[CreateAssetMenu(menuName = "Units/Stats")]
public class UnitStats : ScriptableObject
{
    /// <summary>
    /// Максимальная дистанция передвижения юнита за ход, в метрах.
    /// </summary>
    public float maxMoveDistance = 5f;

    /// <summary>
    /// Радиус атаки юнита, в метрах.
    /// Определяет максимально допустимое расстояние до цели для атаки.
    /// </summary>
    public float attackRange = 2f;
}
