// IAttackable.cs
using UnityEngine;

public interface IAttackable
{
    bool HasAttacked { get; }
    bool IsTargetInRange(Vector3 targetPosition);
    void TryAttack(Vector3 targetPosition);
    void ResetAttack();
}
