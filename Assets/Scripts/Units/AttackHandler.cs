using Unity.Netcode;
using UnityEngine;

/// <summary>
/// ������������ ����� � ���������� ����� ������.
/// </summary>
public class AttackHandler
{
    private UnitController _attackTarget;

    /// <summary>
    /// ������������ ����� ���� � ���������� �����.
    /// ������ ���� � ����� ����, ������ � ������������� � �����.
    /// </summary>
    /// <param name="selected">����, ����������� �����.</param>
    /// <param name="target">������� ����.</param>
    public void HandleAttack(UnitController selected, UnitController target)
    {
        // ��������: ��� �������� ��� ���� ��� ���� ���������
        if (selected.HasAttacked || !selected.IsTargetInRange(target.transform.position))
            return;

        // ���� ���� ����� � �������� �
        if (_attackTarget != target)
        {
            ClearTarget();
            _attackTarget = target;
            _attackTarget.SetAttackTargetSelected(true);

            Debug.Log("���� ������� ��� �����. ��������� ���� ��� �������������.");
        }
        else
        {
            // ������������� � ���������� �����
            selected.TryAttack(_attackTarget.transform.position);

            if (selected.IsServer)
            {
                _attackTarget.Die();
            }
            else
            {
                _attackTarget.RequestDieServerRpc();
            }

            Debug.Log($"����� �� ���� {_attackTarget.name}!");
            ClearTarget();
        }
    }

    /// <summary>
    /// ������� ��������� ���� � ���������� �.
    /// </summary>
    public void ClearTarget()
    {
        if (_attackTarget != null)
        {
            _attackTarget.SetAttackTargetSelected(false);
            _attackTarget = null;
        }
    }
}
