using UnityEngine;

/// <summary>
/// ScriptableObject для хранения основных характеристик юнита.
/// Позволяет создавать разные типы юнитов с уникальными параметрами в редакторе Unity.
/// </summary>
[CreateAssetMenu(menuName = "Units/Stats")]
public class UnitStats : ScriptableObject
{
    /// <summary>
    /// Максимальная дистанция перемещения юнита за один ход (в метрах).
    /// </summary>
    public float maxMoveDistance = 5f;

    /// <summary>
    /// Радиус атаки юнита (в метрах).
    /// Цель должна находиться в этом радиусе для успешной атаки.
    /// </summary>
    public float attackRange = 2f;
}
