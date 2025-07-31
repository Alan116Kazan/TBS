// IAttackable.cs
using UnityEngine;

/// <summary>
/// Интерфейс, описывающий поведение атакующего объекта.
/// Позволяет проверить возможность атаки, выполнить её и сбросить состояние атаки.
/// </summary>
public interface IAttackable
{
    /// <summary>
    /// Свойство, указывающее, совершал ли объект атаку в текущем ходе.
    /// </summary>
    bool HasAttacked { get; }

    /// <summary>
    /// Проверяет, находится ли цель в пределах досягаемости атаки.
    /// </summary>
    /// <param name="targetPosition">Позиция потенциальной цели для атаки.</param>
    /// <returns>True, если цель в радиусе атаки; иначе false.</returns>
    bool IsTargetInRange(Vector3 targetPosition);

    /// <summary>
    /// Пытается выполнить атаку по цели с указанной позицией.
    /// </summary>
    /// <param name="targetPosition">Позиция цели для атаки.</param>
    void TryAttack(Vector3 targetPosition);

    /// <summary>
    /// Сбрасывает состояние атаки (например, после завершения хода).
    /// </summary>
    void ResetAttack();
}
