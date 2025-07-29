using UnityEngine;

public class UnitData : MonoBehaviour
{
    [SerializeField] private UnitStats stats;

    public UnitStats Stats => stats;

    public float MaxMoveDistance => stats.maxMoveDistance;
    public float AttackRange => stats.attackRange;
}
