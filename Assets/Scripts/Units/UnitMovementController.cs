using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// ���������� ������������ ����� � �������������� NavMeshAgent.
/// ��������� ��������� IMovable � ������� ������������� ����� NetworkVariable.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class UnitMovementController : NetworkBehaviour, IMovable
{
    private NavMeshAgent _agent;               // ��������� NavMeshAgent ��� ���������
    private UnitController _unitController;    // ������ �� �������� ���������� �����

    // ������� ����������, ������ ���������� ���������� ��� ������������ � ������� ����
    // ������ � ������ �� �������, ������ � ��� ���� ��������
    private readonly NetworkVariable<float> remainingMoveDistance = new(
        writePerm: NetworkVariableWritePermission.Server,
        readPerm: NetworkVariableReadPermission.Everyone);

    /// <summary>
    /// ���������� ���������� ��� ������������ � ������� ����.
    /// </summary>
    public float RemainingMoveDistance => remainingMoveDistance.Value;

    /// <summary>
    /// ��������� ������ � NavMeshAgent (��������, ��� ������ �����������).
    /// </summary>
    public NavMeshAgent Agent => _agent;

    /// <summary>
    /// ������������� ���������� � ��������� � UnitController.
    /// ����������� NavMeshAgent � ���������� ���������� ���������� �� �������.
    /// </summary>
    public void Initialize(UnitController controller)
    {
        _unitController = controller;
        _agent = GetComponent<NavMeshAgent>();

        _agent.autoBraking = true;       // �������������� ���������� ������ ��� ���������� ����
        _agent.updatePosition = true;    // ��������� ������� �������
        _agent.updateRotation = true;    // ��������� ������� �������

        if (IsServer)
            remainingMoveDistance.Value = _unitController.MaxMoveDistance; // ������������� �������� �������� �� �������
    }

    /// <summary>
    /// �������� ����������� ����� � ��������� �������.
    /// �������� ��������� RPC ��� ���������� ������.
    /// </summary>
    public void TryMove(Vector3 targetPosition)
    {
        TryMoveServerRpc(targetPosition);
    }

    /// <summary>
    /// ��������� RPC ��� ����������� �����.
    /// ���������, ��� ��� ����������� ������ � �������� ��������.
    /// ��������� ���� � ������������ �� ����������� ����������.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void TryMoveServerRpc(Vector3 targetPosition, ServerRpcParams rpcParams = default)
    {
        // ���������, ��� ������ ��� � ���� �� ���������� ��������
        if (!TurnManager.Instance.IsPlayerTurn(_unitController.OwnerId) || RemainingMoveDistance <= 0f)
            return;

        NavMeshPath path = new NavMeshPath();
        // ���������, �������� �� ��������� ���� � ����, � ���� �� �� ������� �� ���� �����
        if (!_agent.CalculatePath(targetPosition, path) || path.corners.Length < 2)
            return;

        float totalDistance = 0f;
        Vector3 finalPosition = path.corners[0]; // �������� � ������� �������

        // �������� �� ��������� ����, �������� ����������
        for (int i = 1; i < path.corners.Length; i++)
        {
            float segment = Vector3.Distance(path.corners[i - 1], path.corners[i]);

            // ���� ��������� ������� ��������� ���������� ����������, ������������ ��������
            if (totalDistance + segment > RemainingMoveDistance)
            {
                float remaining = RemainingMoveDistance - totalDistance;
                Vector3 direction = (path.corners[i] - path.corners[i - 1]).normalized;
                finalPosition = path.corners[i - 1] + direction * remaining;
                totalDistance = RemainingMoveDistance;
                break;
            }

            totalDistance += segment;
            finalPosition = path.corners[i];
        }

        // ������������� �������� ����� �������� NavMeshAgent
        _agent.SetDestination(finalPosition);

        // ��������� ���������� ���������� ��������
        remainingMoveDistance.Value = Mathf.Max(0f, RemainingMoveDistance - totalDistance);
    }

    /// <summary>
    /// ���������� ��������� �������� (��������, � ������ ������ ����).
    /// �������� ��������� RPC.
    /// </summary>
    public void ResetMovement()
    {
        ResetMovementServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResetMovementServerRpc()
    {
        // ��������������� ������������ ���������� ��������
        remainingMoveDistance.Value = _unitController.MaxMoveDistance;
    }
}
