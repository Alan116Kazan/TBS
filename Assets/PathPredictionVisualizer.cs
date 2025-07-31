//using UnityEngine;
//using UnityEngine.AI;
//using System.Collections.Generic;

///// <summary>
///// Отвечает за визуализацию прогнозируемого пути движения юнита.
///// </summary>
//public class PathPredictionVisualizer : MonoBehaviour
//{
//    [SerializeField] private LineRenderer greenLineRenderer;
//    [SerializeField] private LineRenderer redLineRenderer;

//    public void DrawPrediction(UnitController unit, Vector3 target)
//    {
//        if (unit == null || !greenLineRenderer || !redLineRenderer) return;

//        NavMeshAgent agent = unit.NavAgent;
//        if (agent == null) return;

//        NavMeshPath path = new NavMeshPath();
//        if (!agent.CalculatePath(target, path)) return;

//        Vector3[] corners = path.corners;
//        float moveLimit = unit.RemainingMoveDistance;

//        float totalLength = 0f;
//        int splitIndex = corners.Length;
//        float overshoot = 0f;

//        for (int i = 1; i < corners.Length; i++)
//        {
//            float segment = Vector3.Distance(corners[i - 1], corners[i]);
//            if (totalLength + segment >= moveLimit)
//            {
//                splitIndex = i;
//                overshoot = moveLimit - totalLength;
//                break;
//            }
//            totalLength += segment;
//        }

//        List<Vector3> greenPoints = new();
//        for (int i = 0; i < splitIndex; i++) greenPoints.Add(corners[i]);

//        if (splitIndex < corners.Length)
//        {
//            Vector3 dir = (corners[splitIndex] - corners[splitIndex - 1]).normalized;
//            greenPoints.Add(corners[splitIndex - 1] + dir * overshoot);
//        }

//        greenLineRenderer.positionCount = greenPoints.Count;
//        greenLineRenderer.SetPositions(greenPoints.ToArray());

//        if (splitIndex < corners.Length)
//        {
//            List<Vector3> redPoints = new() { greenPoints[^1] };
//            for (int i = splitIndex; i < corners.Length; i++)
//                redPoints.Add(corners[i]);

//            redLineRenderer.positionCount = redPoints.Count;
//            redLineRenderer.SetPositions(redPoints.ToArray());
//        }
//        else
//        {
//            redLineRenderer.positionCount = 0;
//        }
//    }

//    public void Clear()
//    {
//        if (greenLineRenderer) greenLineRenderer.positionCount = 0;
//        if (redLineRenderer) redLineRenderer.positionCount = 0;
//    }
//}
