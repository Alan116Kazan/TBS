using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// ќтвечает за отрисовку визуального предсказани€ пути движени€ юнита.
/// «елена€ лини€ Ч доступный дл€ движени€ путь (с учЄтом лимита),
/// красна€ лини€ Ч недоступный участок пути.
/// </summary>
public class MovePredictionDrawer
{
    private readonly LineRenderer _greenLine;
    private readonly LineRenderer _redLine;

    public MovePredictionDrawer(LineRenderer green, LineRenderer red)
    {
        _greenLine = green;
        _redLine = red;
    }

    public void Clear()
    {
        _greenLine.positionCount = 0;
        _redLine.positionCount = 0;
    }

    /// <summary>
    /// –исует путь юнита к целевой позиции с разделением на доступный и недоступный по лимиту участок.
    /// </summary>
    /// <param name="unit">ёнит, дл€ которого строитс€ предсказание.</param>
    /// <param name="target">÷елева€ позици€ движени€.</param>
    public void Draw(UnitController unit, Vector3 target)
    {
        var agent = unit.NavAgent;
        if (agent == null) return;

        NavMeshPath path = new();
        if (!agent.CalculatePath(target, path)) return;

        Vector3[] corners = path.corners;
        float moveLimit = unit.RemainingMoveDistance;

        float travelled = 0f;
        int splitIndex = corners.Length;
        float overshootDistance = 0f;

        for (int i = 1; i < corners.Length; i++)
        {
            float segmentLength = Vector3.Distance(corners[i - 1], corners[i]);
            if (travelled + segmentLength >= moveLimit)
            {
                splitIndex = i;
                overshootDistance = moveLimit - travelled;
                break;
            }
            travelled += segmentLength;
        }

        // ‘ормируем точки зеленой линии (доступна€ часть пути)
        List<Vector3> greenPoints = new();
        for (int i = 0; i < splitIndex; i++)
            greenPoints.Add(corners[i]);

        if (splitIndex < corners.Length)
        {
            Vector3 direction = (corners[splitIndex] - corners[splitIndex - 1]).normalized;
            greenPoints.Add(corners[splitIndex - 1] + direction * overshootDistance);
        }

        _greenLine.positionCount = greenPoints.Count;
        _greenLine.SetPositions(greenPoints.ToArray());

        // ‘ормируем точки красной линии (недоступна€ часть пути)
        if (splitIndex < corners.Length)
        {
            List<Vector3> redPoints = new() { greenPoints[^1] };
            for (int i = splitIndex; i < corners.Length; i++)
                redPoints.Add(corners[i]);

            _redLine.positionCount = redPoints.Count;
            _redLine.SetPositions(redPoints.ToArray());
        }
        else
        {
            _redLine.positionCount = 0;
        }
    }
}
