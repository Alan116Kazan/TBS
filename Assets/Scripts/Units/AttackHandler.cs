using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Обрабатывает выбор и выполнение атаки юнитом.
/// </summary>
public class AttackHandler
{
    private UnitController _attackTarget;

    /// <summary>
    /// Обрабатывает выбор цели и выполнение атаки.
    /// Первый клик — выбор цели, второй — подтверждение и атака.
    /// </summary>
    /// <param name="selected">Юнит, совершающий атаку.</param>
    /// <param name="target">Целевой юнит.</param>
    public void HandleAttack(UnitController selected, UnitController target)
    {
        // Проверка: уже атаковал или цель вне зоны поражения
        if (selected.HasAttacked || !selected.IsTargetInRange(target.transform.position))
            return;

        // Если цель новая — выделить её
        if (_attackTarget != target)
        {
            ClearTarget();
            _attackTarget = target;
            _attackTarget.SetAttackTargetSelected(true);

            Debug.Log("Цель выбрана для атаки. Повторите клик для подтверждения.");
        }
        else
        {
            // Подтверждение и выполнение атаки
            selected.TryAttack(_attackTarget.transform.position);

            if (selected.IsServer)
            {
                _attackTarget.Die();
            }
            else
            {
                _attackTarget.RequestDieServerRpc();
            }

            Debug.Log($"Атака по цели {_attackTarget.name}!");
            ClearTarget();
        }
    }

    /// <summary>
    /// Снимает выделение цели и сбрасывает её.
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
