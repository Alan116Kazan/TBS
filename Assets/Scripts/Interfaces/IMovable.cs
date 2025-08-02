using UnityEngine;

/// <summary>
/// Интерфейс, описывающий логику передвижения юнита:
/// перемещение, проверку оставшейся дистанции и сброс.
/// </summary>
public interface IMovable
{
    /// <summary>
    /// Оставшееся расстояние, доступное для движения в этом ходе.
    /// </summary>
    float RemainingMoveDistance { get; }

    /// <summary>
    /// Пытается переместить юнита к целевой позиции.
    /// </summary>
    /// <param name="targetPosition">Целевая позиция.</param>
    void TryMove(Vector3 targetPosition);

    /// <summary>
    /// Сброс параметров движения, вызывается в начале хода.
    /// </summary>
    void ResetMovement();
}
