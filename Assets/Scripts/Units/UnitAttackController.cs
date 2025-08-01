using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Контроллер атаки юнита, реализует интерфейс IAttackable.
/// Управляет состоянием атаки и проверками в сетевой игре.
/// </summary>
public class UnitAttackController : NetworkBehaviour, IAttackable
{
    private UnitController _unitController;

    private readonly NetworkVariable<bool> hasAttacked = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    public bool HasAttacked => hasAttacked.Value;

    public void Initialize(UnitController controller)
    {
        _unitController = controller;

        if (IsServer)
            hasAttacked.Value = false;

        // Подписка на клиенте
        hasAttacked.OnValueChanged += (prev, next) =>
        {
            if (IsClient)
                GameEvents.TriggerUnitAttacked(_unitController);
        };
    }


    public bool IsTargetInRange(Vector3 targetPosition)
    {
        return Vector3.Distance(_unitController.transform.position, targetPosition) <= _unitController.AttackRange;
    }

    public void TryAttack(Vector3 targetPosition)
    {
        TryAttackServerRpc(targetPosition);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TryAttackServerRpc(Vector3 targetPosition, ServerRpcParams rpcParams = default)
    {
        if (!TurnManager.Instance.IsPlayerTurn(_unitController.OwnerId) || HasAttacked)
            return;

        if (!IsTargetInRange(targetPosition))
            return;

        Debug.Log($"Unit {_unitController.name} attacks target at {targetPosition}");

        hasAttacked.Value = true;
        GameEvents.TriggerUnitAttacked(_unitController);

        // TODO: здесь можно добавить урон, анимацию и эффекты
    }

    public void ResetAttack()
    {
        ResetAttackServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResetAttackServerRpc()
    {
        hasAttacked.Value = false;
        GameEvents.TriggerUnitAttacked(_unitController);
    }
}
