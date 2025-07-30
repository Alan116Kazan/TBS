// IMovable.cs
using UnityEngine;

public interface IMovable
{
    float RemainingMoveDistance { get; }
    void TryMove(Vector3 targetPosition);
    void ResetMovement();
}
