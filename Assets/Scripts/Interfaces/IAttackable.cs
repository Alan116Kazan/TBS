using UnityEngine;

/// <summary>
/// Интерфейс, описывающий атакующие действия юнита:
/// проверку возможности атаки, выполнение атаки и её сброс.
/// </summary>
public interface IAttackable
{
    /// <summary>
    /// Был ли юнит атакующим в текущем ходе.
    /// </summary>
    bool HasAttacked { get; }

    /// <summary>
    /// Проверяет, находится ли цель в пределах радиуса атаки.
    /// </summary>
    /// <param name="targetPosition">Позиция цели.</param>
    /// <returns>True — цель в радиусе; иначе — false.</returns>
    bool IsTargetInRange(Vector3 targetPosition);

    /// <summary>
    /// Выполняет попытку атаки по цели.
    /// </summary>
    /// <param name="targetPosition">Позиция цели.</param>
    void TryAttack(Vector3 targetPosition);

    /// <summary>
    /// Сброс флага атаки, вызывается при завершении хода.
    /// </summary>
    void ResetAttack();
}
