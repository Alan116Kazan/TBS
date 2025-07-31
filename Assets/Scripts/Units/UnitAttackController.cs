using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Контроллер атаки юнита, реализует интерфейс IAttackable.
/// Управляет состоянием атаки и проверками в сетевой игре.
/// </summary>
public class UnitAttackController : NetworkBehaviour, IAttackable
{
    private UnitController _unitController;

    // Сетевая переменная, указывающая, совершал ли юнит атаку в текущем ходе
    // Все могут читать, только сервер может писать
    private readonly NetworkVariable<bool> hasAttacked = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    // Свойство для проверки, был ли совершен удар
    public bool HasAttacked => hasAttacked.Value;

    /// <summary>
    /// Инициализация контроллера с привязкой к UnitController.
    /// </summary>
    public void Initialize(UnitController controller)
    {
        _unitController = controller;

        if (IsServer)
            hasAttacked.Value = false; // Сбрасываем состояние атаки при инициализации на сервере
    }

    /// <summary>
    /// Проверяет, находится ли цель в радиусе атаки юнита.
    /// </summary>
    public bool IsTargetInRange(Vector3 targetPosition)
    {
        return Vector3.Distance(_unitController.transform.position, targetPosition) <= _unitController.AttackRange;
    }

    /// <summary>
    /// Пытается выполнить атаку по позиции цели.
    /// Вызывает серверный RPC для обработки логики.
    /// </summary>
    public void TryAttack(Vector3 targetPosition)
    {
        TryAttackServerRpc(targetPosition);
    }

    /// <summary>
    /// Серверный RPC, выполняющий атаку.
    /// Проверяет, что ход принадлежит владельцу юнита и атака ещё не была совершена.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    private void TryAttackServerRpc(Vector3 targetPosition, ServerRpcParams rpcParams = default)
    {
        if (!TurnManager.Instance.IsPlayerTurn(_unitController.OwnerId) || HasAttacked)
            return; // Если не ход игрока или атака уже была — ничего не делаем

        if (!IsTargetInRange(targetPosition))
            return; // Если цель вне досягаемости — ничего не делаем

        Debug.Log($"Unit {_unitController.name} attacks target at {targetPosition}");

        hasAttacked.Value = true;

        // TODO: здесь можно добавить анимацию атаки, вычисление урона и т.п.
    }

    /// <summary>
    /// Сбрасывает состояние атаки (например, при начале нового хода).
    /// Вызывает серверный RPC.
    /// </summary>
    public void ResetAttack()
    {
        ResetAttackServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResetAttackServerRpc()
    {
        hasAttacked.Value = false;
    }
}
