using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.AI;

public class UnitSelectionHandler : MonoBehaviour
{
    private Camera mainCamera;
    private UnitController selectedUnit;

    private Vector3? predictedTarget;
    private float lastRightClickTime;
    private const float doubleClickThreshold = 0.3f;

    private float lastReportedDistance = -1f;

    [SerializeField] private LineRenderer greenLineRenderer;
    [SerializeField] private LineRenderer redLineRenderer;

    private void Start()
    {
        mainCamera = Camera.main;
        ClearPrediction();
    }

    private void Update()
    {
        if (NetworkManager.Singleton?.IsConnectedClient != true) return;
        if (!TurnManager.Instance.IsPlayerTurn(NetworkManager.Singleton.LocalClientId)) return;

        if (Input.GetMouseButtonDown(0)) LeftClick();
        else if (Input.GetMouseButtonDown(1) && selectedUnit != null) RightClick();

        if (selectedUnit != null)
        {
            float currentDistance = selectedUnit.RemainingMoveDistance;
            if (Mathf.Abs(currentDistance - lastReportedDistance) > 0.01f)
            {
                Debug.Log($"Оставшееся расстояние: {currentDistance:F2} м");
                lastReportedDistance = currentDistance;
            }
        }
        else
        {
            lastReportedDistance = -1f;
        }
    }

    private void LeftClick()
    {
        if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
        {
            var unit = hit.collider.GetComponent<UnitController>();
            if (unit != null && unit.OwnerClientId == NetworkManager.Singleton.LocalClientId)
            {
                selectedUnit?.SetSelected(false);
                selectedUnit = unit;
                selectedUnit.SetSelected(true);
                return;
            }
        }

        selectedUnit?.SetSelected(false);
        selectedUnit = null;
        ClearPrediction();
    }

    private void RightClick()
    {
        if (!Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit)) return;

        Vector3 clickedPoint = hit.point;

        if (!predictedTarget.HasValue || Vector3.Distance(predictedTarget.Value, clickedPoint) > 0.5f)
        {
            predictedTarget = clickedPoint;
            DrawPrediction(clickedPoint);
            lastRightClickTime = Time.time;
            return;
        }

        if (Time.time - lastRightClickTime < doubleClickThreshold)
        {
            selectedUnit.TryMoveServerRpc(clickedPoint);
            ClearPrediction();
        }
        else
        {
            lastRightClickTime = Time.time;
        }
    }

    private void DrawPrediction(Vector3 target)
    {
        if (selectedUnit == null || greenLineRenderer == null || redLineRenderer == null) return;

        var agent = selectedUnit.GetComponent<NavMeshAgent>();
        if (agent == null) return;

        NavMeshPath path = new NavMeshPath();
        if (!agent.CalculatePath(target, path)) return;

        float remainingDistance = selectedUnit.RemainingMoveDistance;

        Vector3[] corners = path.corners;

        float pathLength = 0f;
        int splitIndex = corners.Length - 1;
        float splitDistanceOnSegment = 0f;

        for (int i = 1; i < corners.Length; i++)
        {
            float segmentLength = Vector3.Distance(corners[i - 1], corners[i]);
            if (pathLength + segmentLength >= remainingDistance)
            {
                splitIndex = i;
                splitDistanceOnSegment = remainingDistance - pathLength;
                break;
            }
            pathLength += segmentLength;
        }

        List<Vector3> greenPoints = new List<Vector3>();
        for (int i = 0; i < splitIndex; i++)
            greenPoints.Add(corners[i]);

        if (splitIndex < corners.Length)
        {
            Vector3 dir = (corners[splitIndex] - corners[splitIndex - 1]).normalized;
            Vector3 splitPoint = corners[splitIndex - 1] + dir * splitDistanceOnSegment;
            greenPoints.Add(splitPoint);
        }

        List<Vector3> redPoints = new List<Vector3>();
        if (splitIndex < corners.Length)
        {
            redPoints.Add(greenPoints[greenPoints.Count - 1]);
            for (int i = splitIndex; i < corners.Length; i++)
                redPoints.Add(corners[i]);
        }

        greenLineRenderer.positionCount = greenPoints.Count;
        greenLineRenderer.SetPositions(greenPoints.ToArray());

        redLineRenderer.positionCount = redPoints.Count;
        redLineRenderer.SetPositions(redPoints.ToArray());
    }

    private void ClearPrediction()
    {
        predictedTarget = null;

        if (greenLineRenderer != null)
            greenLineRenderer.positionCount = 0;

        if (redLineRenderer != null)
            redLineRenderer.positionCount = 0;
    }
}
