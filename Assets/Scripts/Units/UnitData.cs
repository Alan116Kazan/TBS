using UnityEngine;

/// <summary>
/// Хранит данные юнита, ссылаясь на ScriptableObject с его характеристиками.
/// Обеспечивает удобный доступ к параметрам юнита.
/// </summary>
public class UnitData : MonoBehaviour
{
    // Ссылка на ScriptableObject с основными характеристиками юнита
    [SerializeField]
    private UnitStats stats;

    /// <summary>
    /// Публичное свойство для доступа к ScriptableObject со статистикой юнита.
    /// </summary>
    public UnitStats Stats => stats;

    /// <summary>
    /// Максимальная дистанция передвижения юнита (берётся из stats).
    /// </summary>
    public float MaxMoveDistance => stats.maxMoveDistance;

    /// <summary>
    /// Радиус атаки юнита (берётся из stats).
    /// </summary>
    public float AttackRange => stats.attackRange;
}
