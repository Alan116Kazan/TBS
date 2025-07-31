// IMovable.cs
using UnityEngine;

/// <summary>
/// Интерфейс, описывающий поведение передвижения объекта.
/// Позволяет пытаться передвигаться к цели и сбрасывать оставшееся расстояние движения.
/// </summary>
public interface IMovable
{
    /// <summary>
    /// Оставшееся расстояние, которое объект может пройти за текущий ход.
    /// </summary>
    float RemainingMoveDistance { get; }

    /// <summary>
    /// Пытается переместить объект к указанной позиции.
    /// </summary>
    /// <param name="targetPosition">Целевая позиция для перемещения.</param>
    void TryMove(Vector3 targetPosition);

    /// <summary>
    /// Сбрасывает параметры движения (например, в начале нового хода).
    /// </summary>
    void ResetMovement();
}
