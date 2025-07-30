using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class UnitSelectionHandler : MonoBehaviour
{
    // ������ �� ������� ������ ��� ����� �� ����
    private Camera _mainCamera;

    // ������� ��������� ���� �������
    private UnitController _selectedUnit;

    // ����, ��������� ��� ���� ��� �����
    private UnitController _attackTarget;

    // ������������� ����� ��� ����������� (��� ������������ ����)
    private Vector3? _predictedTarget;

    // ����� ���������� ������� ����� ���� (��� ������������� �������� �����)
    private float _lastRightClickTime;
    private const float _doubleClickThreshold = 0.3f; // �������� ��� �������� ����� � ��������

    // ��� ����������� ������ � ������ ��������� ��������� ���������� ���������� �������� ���������� �����
    private float _lastReportedDistance = -1f;

    [SerializeField] private LineRenderer greenLineRenderer; // ����� �������� ����� ��� ����, ������� ���� ����� ������
    [SerializeField] private LineRenderer redLineRenderer;   // ����� �������� ����� ��� ������������� (������� ��������) ����

    private void Start()
    {
        _mainCamera = Camera.main;  // �������� �������� ������
        ClearPrediction();          // ���������� ����� ������������ ����
    }

    private void Update()
    {
        // ���������, ��� ������ ��������� � �����
        if (NetworkManager.Singleton?.IsConnectedClient != true) return;

        ulong myId = NetworkManager.Singleton.LocalClientId;

        // ���������, ����� �� ��������� ����� ������
        if (!TurnManager.Instance.IsPlayerTurn(myId)) return;

        // ��������� ������� ����
        if (Input.GetMouseButtonDown(0)) HandleLeftClick();     // ����� ������ � �����
        else if (Input.GetMouseButtonDown(1) && _selectedUnit != null) HandleRightClick(); // ������ � �������� (����� ��� ��������)

        // ���������� ����� ����������� ���������� �������� ���������� ����� (������� � ������� ��� ���������)
        if (_selectedUnit)
        {
            float currentDistance = _selectedUnit.RemainingMoveDistance;
            if (Mathf.Abs(currentDistance - _lastReportedDistance) > 0.01f)
            {
                Debug.Log($"���������� ����������: {currentDistance:F2} �");
                _lastReportedDistance = currentDistance;
            }
        }
        else
        {
            _lastReportedDistance = -1f;
        }
    }

    // ��������� ������ ����� � ����� ����� ��� ����� ���������
    private void HandleLeftClick()
    {
        if (!Physics.Raycast(_mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit)) return;

        // ���� �������� �� ����� ������
        if (hit.collider.TryGetComponent(out UnitController unit) &&
            unit.OwnerClientId == NetworkManager.Singleton.LocalClientId)
        {
            // ���� ���� ��� �������� ��� (�������� � �� ����� ���������)
            if (unit.HasAttacked && unit.RemainingMoveDistance <= 0f)
            {
                Debug.Log("���� ���� ��� �������� ���.");
                return;
            }

            // ���� ������ ����� ���� � ��������� ���������
            if (_selectedUnit != unit)
            {
                _selectedUnit?.SetSelected(false); // ������� ��������� � �����������
                ClearAttackTarget();               // ���������� ���� �����

                _selectedUnit = unit;
                _selectedUnit.SetSelected(true);  // �������� ����� ����
            }
        }
        else
        {
            // ���� �� �� ������ ����� � ������� ���������, ���������� ���� � ��������
            _selectedUnit?.SetSelected(false);
            _selectedUnit = null;
            ClearPrediction();
            ClearAttackTarget();
        }
    }

    // ��������� ������� ����� � ����� ��� �����������
    private void HandleRightClick()
    {
        if (!Physics.Raycast(_mainCamera.ScreenPointToRay(Input.mousePosition), out RaycastHit hit)) return;
        if (_selectedUnit == null) return;

        // ���� ���� ��� �������� � �������� �� ������� ����� � ��������� ��������� �����
        if (_selectedUnit.HasAttacked && hit.collider.TryGetComponent<UnitController>(out UnitController clickedUnit))
        {
            if (clickedUnit.OwnerClientId != NetworkManager.Singleton.LocalClientId)
            {
                Debug.Log("���� ���� ��� �������� � �� ����� ��������� �����.");
                return;
            }
        }

        // ��������� ����� �� ����� (���� ��� �����)
        if (hit.collider.TryGetComponent<UnitController>(out UnitController clickedUnit2))
        {
            if (clickedUnit2.OwnerClientId == NetworkManager.Singleton.LocalClientId)
            {
                // ���� �� ������ ����� � ����� ���� �����
                ClearAttackTarget();
                return;
            }

            // ���� �� ���������� ����� � ��������� ������ �����
            if (!_selectedUnit.IsTargetInRange(clickedUnit2.transform.position))
            {
                Debug.Log("���� ��� ������� �����");
                ClearAttackTarget();
                return;
            }

            if (_selectedUnit.HasAttacked)
            {
                Debug.Log("���� ���� ��� ��������.");
                return;
            }

            // ���� ���� ��� �� ������� ��� ������� ������ � �������� � � ���� �������������
            if (_attackTarget != clickedUnit2)
            {
                ClearAttackTarget();

                _attackTarget = clickedUnit2;
                _attackTarget.SetAttackTargetSelected(true);

                Debug.Log("���� ������� ��� �����. ��������� ���� ��� �������������.");
                return;
            }
            else
            {
                // ��������� ���� � ������������ ����� � �������� ��������� RPC
                _selectedUnit.TryAttackServerRpc(_attackTarget.transform.position);
                Debug.Log($"����� �� ���� {_attackTarget.name}!");

                ClearAttackTarget();
                ClearPrediction();
            }
        }
        else
        {
            // ���� �� ������� ����� � ����������� �����

            ClearAttackTarget();

            // ���� ����� ����� �������� ���������� �� ���������� � ��������� ������������ ����
            if (!_predictedTarget.HasValue || Vector3.Distance(_predictedTarget.Value, hit.point) > 0.5f)
            {
                _predictedTarget = hit.point;
                DrawPrediction(hit.point);
                _lastRightClickTime = Time.time;
                return;
            }

            // ��������� ������� ���� (��� ������ ����� ������ �� ����� �����) � ������������ �����������
            if (Time.time - _lastRightClickTime < _doubleClickThreshold)
            {
                _selectedUnit.TryMoveServerRpc(hit.point);
                ClearPrediction();
            }
            else
            {
                _lastRightClickTime = Time.time;
            }
        }
    }

    // ������������ ��������������� ���� �����������
    private void DrawPrediction(Vector3 target)
    {
        if (!_selectedUnit || !greenLineRenderer || !redLineRenderer) return;

        NavMeshAgent agent = _selectedUnit.NavAgent;
        if (agent == null) return;

        NavMeshPath path = new NavMeshPath();
        if (!agent.CalculatePath(target, path)) return;

        Vector3[] corners = path.corners;
        float moveLimit = _selectedUnit.RemainingMoveDistance;

        float totalLength = 0f;
        int splitIndex = corners.Length;
        float overshoot = 0f;

        // ���� �����, ��� ���� ��������� ���������� ���������� ��������
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

        // ������� ����� ���� � ��������� ��� �����������
        List<Vector3> greenPoints = new List<Vector3>();
        for (int i = 0; i < splitIndex; i++)
            greenPoints.Add(corners[i]);

        if (splitIndex < corners.Length)
        {
            Vector3 dir = (corners[splitIndex] - corners[splitIndex - 1]).normalized;
            greenPoints.Add(corners[splitIndex - 1] + dir * overshoot);
        }

        greenLineRenderer.positionCount = greenPoints.Count;
        greenLineRenderer.SetPositions(greenPoints.ToArray());

        // ������� ����� ���� � ����������� (������� �������)
        if (splitIndex < corners.Length)
        {
            List<Vector3> redPoints = new List<Vector3> { greenPoints[^1] };
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

    // ����� ������������ ����
    private void ClearPrediction()
    {
        _predictedTarget = null;
        if (greenLineRenderer) greenLineRenderer.positionCount = 0;
        if (redLineRenderer) redLineRenderer.positionCount = 0;
    }

    // ����� ��������� ���� �����
    private void ClearAttackTarget()
    {
        if (_attackTarget != null)
        {
            _attackTarget.SetAttackTargetSelected(false);
            _attackTarget = null;
        }
    }
}
