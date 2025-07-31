using Unity.Netcode;
using UnityEngine;

public class AttackHandler
{
    private UnitController _attackTarget;

    public void HandleAttack(UnitController selected, UnitController target)
    {
        if (selected.HasAttacked || !selected.IsTargetInRange(target.transform.position)) return;

        if (_attackTarget != target)
        {
            ClearTarget();
            _attackTarget = target;
            _attackTarget.SetAttackTargetSelected(true);
            Debug.Log("Цель выбрана для атаки. Повторите клик для подтверждения.");
        }
        else
        {
            selected.TryAttack(_attackTarget.transform.position);

            if (selected.IsServer)
            {
                _attackTarget.Die(); // Сервер (хост) убивает напрямую
            }
            else
            {
                _attackTarget.RequestDieServerRpc(); // Клиент отправляет запрос на сервер
            }

            Debug.Log($"Атака по цели {_attackTarget.name}!");
            ClearTarget();
        }
    }


    public void ClearTarget()
    {
        if (_attackTarget != null)
        {
            _attackTarget.SetAttackTargetSelected(false);
            _attackTarget = null;
        }
    }
}
