using Unity.Netcode;
using UnityEngine;

/// <summary>
/// �������� �� �������� ����� �����.
/// ��������� ��������� ����������� �����, � ����������� � �������������� ��������� ����� ����.
/// </summary>
public class UnitAttackController : NetworkBehaviour
{
    // ������ �� �������� ���������� �����
    private UnitController _unitController;

    // ������� ����, ������������ � �������� �� ���� � ������� ����.
    // �������� ���������������� � ��������� (read: Everyone), �� ���������� ������ �������� (write: Server).
    private readonly NetworkVariable<bool> hasAttacked = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    /// <summary>
    /// ��������� �������� ��� ��������, �������� �� ���� �����.
    /// </summary>
    public bool HasAttacked => hasAttacked.Value;

    /// <summary>
    /// ������������� ����������� � ��������� ������ �� UnitController.
    /// ����� ��������� ����� ��� ������������� �� �������.
    /// </summary>
    public void Initialize(UnitController controller)
    {
        _unitController = controller;

        if (IsServer)
        {
            hasAttacked.Value = false;
        }
    }

    /// <summary>
    /// ���������, ��������� �� ���� � ������� ����� �����.
    /// </summary>
    public bool IsTargetInRange(Vector3 targetPosition)
    {
        return Vector3.Distance(_unitController.transform.position, targetPosition) <= _unitController.AttackRange;
    }

    /// <summary>
    /// ��������� ����� ��� ���������� �����.
    /// ���������, ���:
    /// - ������ ��� ���������������� ������;
    /// - ���� ��� �� ��������;
    /// - ���� ��������� � ������� �����.
    /// ����� ����� �������� ����� ��� ��� ������������.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void TryAttackServerRpc(Vector3 targetPosition, ServerRpcParams rpcParams = default)
    {
        if (!TurnManager.Instance.IsPlayerTurn(_unitController.OwnerId) || HasAttacked) return;
        if (!IsTargetInRange(targetPosition)) return;

        Debug.Log($"���� {_unitController.name} ������� ���� �� ������� {targetPosition}");

        hasAttacked.Value = true;

        // TODO: ���� ����� ��������:
        // - ����� �������� �����
        // - ��������� ����� ����
        // - ����������� ������� (��������, OnAttack)
    }

    /// <summary>
    /// ����� ��������� ����� �����. ���������� ��� ������ ������ ����.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void ResetAttackServerRpc()
    {
        hasAttacked.Value = false;
    }
}
