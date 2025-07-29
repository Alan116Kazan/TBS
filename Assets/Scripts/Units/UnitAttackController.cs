using Unity.Netcode;
using UnityEngine;

public class UnitAttackController : NetworkBehaviour
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
        {
            hasAttacked.Value = false;
        }
    }

    public bool IsTargetInRange(Vector3 targetPosition)
    {
        return Vector3.Distance(_unitController.transform.position, targetPosition) <= _unitController.AttackRange;
    }

    [ServerRpc(RequireOwnership = false)]
    public void TryAttackServerRpc(Vector3 targetPosition, ServerRpcParams rpcParams = default)
    {
        if (!TurnManager.Instance.IsPlayerTurn(_unitController.OwnerId) || HasAttacked) return;
        if (!IsTargetInRange(targetPosition)) return;

        Debug.Log($"Юнит {_unitController.name} атакует цель на позиции {targetPosition}");
        hasAttacked.Value = true;

        // Здесь можно добавить логику нанесения урона, анимации и т.п.
    }

    [ServerRpc(RequireOwnership = false)]
    public void ResetAttackServerRpc()
    {
        hasAttacked.Value = false;
    }
}
