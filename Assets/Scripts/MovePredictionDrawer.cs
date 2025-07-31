using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MovePredictionDrawer
{
    private readonly LineRenderer _green, _red;

    public MovePredictionDrawer(LineRenderer green, LineRenderer red)
    {
        _green = green;
        _red = red;
    }

    public void Clear()
    {
        _green.positionCount = 0;
        _red.positionCount = 0;
    }

    public void Draw(UnitController unit, Vector3 target)
    {
        var agent = unit.NavAgent;
        if (!agent) return;

        NavMeshPath path = new();
        if (!agent.CalculatePath(target, path)) return;

        Vector3[] corners = path.corners;
        float limit = unit.RemainingMoveDistance;

        float total = 0f;
        int split = corners.Length;
        float overshoot = 0f;

        for (int i = 1; i < corners.Length; i++)
        {
            float segment = Vector3.Distance(corners[i - 1], corners[i]);
            if (total + segment >= limit)
            {
                split = i;
                overshoot = limit - total;
                break;
            }
            total += segment;
        }

        List<Vector3> greenPoints = new();
        for (int i = 0; i < split; i++) greenPoints.Add(corners[i]);

        if (split < corners.Length)
        {
            Vector3 dir = (corners[split] - corners[split - 1]).normalized;
            greenPoints.Add(corners[split - 1] + dir * overshoot);
        }

        _green.positionCount = greenPoints.Count;
        _green.SetPositions(greenPoints.ToArray());

        if (split < corners.Length)
        {
            List<Vector3> redPoints = new() { greenPoints[^1] };
            for (int i = split; i < corners.Length; i++) redPoints.Add(corners[i]);
            _red.positionCount = redPoints.Count;
            _red.SetPositions(redPoints.ToArray());
        }
        else
        {
            _red.positionCount = 0;
        }
    }
}
