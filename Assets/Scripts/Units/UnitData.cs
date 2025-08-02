using UnityEngine;

/// <summary>
/// Компонент, предоставляющий доступ к характеристикам юнита,
/// хранящимся в ScriptableObject UnitStats.
/// </summary>
public class UnitData : MonoBehaviour
{
    [Tooltip("Основные характеристики юнита, заданные через ScriptableObject.")]
    [SerializeField]
    private UnitStats stats;

    public UnitStats Stats => stats;

    /// <summary>
    /// Максимальная дистанция передвижения юнита.
    /// Получается из объекта UnitStats.
    /// </summary>
    public float MaxMoveDistance => stats != null ? stats.maxMoveDistance : 0f;

    /// <summary>
    /// Радиус атаки юнита.
    /// Получается из объекта UnitStats.
    /// </summary>
    public float AttackRange => stats != null ? stats.attackRange : 0f;

    private void Awake()
    {
        if (stats == null)
        {
            Debug.LogWarning($"[UnitData] На объекте {gameObject.name} не назначен UnitStats!");
        }
    }
}
