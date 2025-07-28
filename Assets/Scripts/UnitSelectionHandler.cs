using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class UnitSelectionHandler : MonoBehaviour
{
    private Camera mainCamera;
    private UnitController selectedUnit;
    private UnitController attackTarget;

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

        ulong myId = NetworkManager.Singleton.LocalClientId;
        if (!TurnManager.Instance.IsPlayerTurn(myId)) return;

        if (Input.GetMouseButtonDown(0)) HandleLeftClick();
        else if (Input.GetMouseButtonDown(1) && selectedUnit != null) HandleRightClick();

        if (selectedUnit)
        {
            float currentDistance = selectedUnit.RemainingMoveDistance;
            if (Mathf.Abs(currentDistance - lastReportedDistance) > 0.01f)
            {
                Debug.Log($"���������� ����������: {currentDistance:F2} �");
                lastReportedDistance = currentDistance;
            }
        }
        else
        {
            lastReportedDistance = -1f;
        }
    }

    private void HandleLeftClick()
    {
        if (!Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit)) return;

        if (hit.collider.TryGetComponent(out UnitController unit) &&
            unit.OwnerClientId == NetworkManager.Singleton.LocalClientId)
        {
            // ������ ������ �����, ���� �� ��������� �������� ��� (�������� � �������� ��������)
            if (unit.HasAttacked && unit.RemainingMoveDistance <= 0f)
            {
                Debug.Log("���� ���� ��� �������� ���.");
                return;
            }

            if (selectedUnit != unit)
            {
                selectedUnit?.SetSelected(false);
                ClearAttackTarget();

                selectedUnit = unit;
                selectedUnit.SetSelected(true);
            }
        }
        else
        {
            selectedUnit?.SetSelected(false);
            selectedUnit = null;
            ClearPrediction();
            ClearAttackTarget();
        }
    }

    private void HandleRightClick()
    {
        if (!Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit)) return;
        if (selectedUnit == null) return;

        // ��������� �������� ���� ����� �����, �� �� ��������� ��������� �����
        if (selectedUnit.HasAttacked && hit.collider.TryGetComponent<UnitController>(out UnitController clickedUnit))
        {
            if (clickedUnit.OwnerClientId != NetworkManager.Singleton.LocalClientId)
            {
                Debug.Log("���� ���� ��� �������� � �� ����� ��������� �����.");
                return; // �� ��� �������� ���������
            }
        }

        if (hit.collider.TryGetComponent<UnitController>(out UnitController clickedUnit2))
        {
            // �������� �� ������ ����� � ������� ���� �����
            if (clickedUnit2.OwnerClientId == NetworkManager.Singleton.LocalClientId)
            {
                ClearAttackTarget();
                return;
            }

            // �������� �� ���������� ����� � ��������� ������ �����
            if (!selectedUnit.IsTargetInRange(clickedUnit2.transform.position))
            {
                Debug.Log("���� ��� ������� �����");
                ClearAttackTarget();
                return;
            }

            // ���� ���� ��� �������� � �� ��������� ��������� �����
            if (selectedUnit.HasAttacked)
            {
                Debug.Log("���� ���� ��� ��������.");
                return;
            }

            // ���� ����� ���� � �������� �
            if (attackTarget != clickedUnit2)
            {
                ClearAttackTarget();

                attackTarget = clickedUnit2;
                attackTarget.SetAttackTargetSelected(true);

                Debug.Log("���� ������� ��� �����. ��������� ���� ��� �������������.");
                return;
            }
            else
            {
                // ��������� ���� � �����
                selectedUnit.TryAttackServerRpc(attackTarget.transform.position);
                Debug.Log($"����� �� ���� {attackTarget.name}!");

                ClearAttackTarget();
                // �� ���������� selection � ���� ����� ��������� ������ ����� �����
                ClearPrediction();
            }
        }
        else
        {
            // ���� �� ������� ����� � �����������
            ClearAttackTarget();

            if (!predictedTarget.HasValue || Vector3.Distance(predictedTarget.Value, hit.point) > 0.5f)
            {
                predictedTarget = hit.point;
                DrawPrediction(hit.point);
                lastRightClickTime = Time.time;
                return;
            }

            if (Time.time - lastRightClickTime < doubleClickThreshold)
            {
                selectedUnit.TryMoveServerRpc(hit.point);
                ClearPrediction();
            }
            else
            {
                lastRightClickTime = Time.time;
            }
        }
    }

    private void DrawPrediction(Vector3 target)
    {
        if (!selectedUnit || !greenLineRenderer || !redLineRenderer) return;
        if (!selectedUnit.TryGetComponent(out NavMeshAgent agent)) return;

        NavMeshPath path = new NavMeshPath();
        if (!agent.CalculatePath(target, path)) return;

        Vector3[] corners = path.corners;
        float moveLimit = selectedUnit.RemainingMoveDistance;

        float totalLength = 0f;
        int splitIndex = corners.Length;
        float overshoot = 0f;

        for (int i = 1; i < corners.Length; i++)
        {
            float segment = Vector3.Distance(corners[i - 1], corners[i]);
            if (totalLength + segment >= moveLimit)
            {
                splitIndex = i;
                overshoot = moveLimit - totalLength;
                break;
            }
            totalLength += segment;
        }

        List<Vector3> greenPoints = new();
        for (int i = 0; i < splitIndex; i++)
            greenPoints.Add(corners[i]);

        if (splitIndex < corners.Length)
        {
            Vector3 dir = (corners[splitIndex] - corners[splitIndex - 1]).normalized;
            greenPoints.Add(corners[splitIndex - 1] + dir * overshoot);
        }

        greenLineRenderer.positionCount = greenPoints.Count;
        greenLineRenderer.SetPositions(greenPoints.ToArray());

        if (splitIndex < corners.Length)
        {
            List<Vector3> redPoints = new() { greenPoints[^1] };
            for (int i = splitIndex; i < corners.Length; i++)
                redPoints.Add(corners[i]);

            redLineRenderer.positionCount = redPoints.Count;
            redLineRenderer.SetPositions(redPoints.ToArray());
        }
        else
        {
            redLineRenderer.positionCount = 0;
        }
    }

    private void ClearPrediction()
    {
        predictedTarget = null;
        if (greenLineRenderer) greenLineRenderer.positionCount = 0;
        if (redLineRenderer) redLineRenderer.positionCount = 0;
    }

    private void ClearAttackTarget()
    {
        if (attackTarget != null)
        {
            attackTarget.SetAttackTargetSelected(false);
            attackTarget = null;
        }
    }
}
