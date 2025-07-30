using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Отвечает за механику атаки юнита.
/// Управляет проверкой доступности атаки, её выполнением и синхронизацией состояния через сеть.
/// </summary>
public class UnitAttackController : NetworkBehaviour
{
    // Ссылка на основной контроллер юнита
    private UnitController _unitController;

    // Сетевое поле, определяющее — атаковал ли юнит в текущем ходу.
    // Значение синхронизируется с клиентами (read: Everyone), но изменяется только сервером (write: Server).
    private readonly NetworkVariable<bool> hasAttacked = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    /// <summary>
    /// Публичное свойство для проверки, совершал ли юнит атаку.
    /// </summary>
    public bool HasAttacked => hasAttacked.Value;

    /// <summary>
    /// Инициализация контроллера с передачей ссылки на UnitController.
    /// Сброс состояния атаки при инициализации на сервере.
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
    /// Проверяет, находится ли цель в радиусе атаки юнита.
    /// </summary>
    public bool IsTargetInRange(Vector3 targetPosition)
    {
        return Vector3.Distance(_unitController.transform.position, targetPosition) <= _unitController.AttackRange;
    }

    /// <summary>
    /// Серверный метод для выполнения атаки.
    /// Проверяет, что:
    /// - сейчас ход соответствующего игрока;
    /// - юнит ещё не атаковал;
    /// - цель находится в радиусе атаки.
    /// После этого помечает юнита как уже атаковавшего.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void TryAttackServerRpc(Vector3 targetPosition, ServerRpcParams rpcParams = default)
    {
        if (!TurnManager.Instance.IsPlayerTurn(_unitController.OwnerId) || HasAttacked) return;
        if (!IsTargetInRange(targetPosition)) return;

        Debug.Log($"Юнит {_unitController.name} атакует цель на позиции {targetPosition}");

        hasAttacked.Value = true;

        // TODO: сюда стоит добавить:
        // - вызов анимации атаки
        // - нанесение урона цели
        // - уведомления ивентов (например, OnAttack)
    }

    /// <summary>
    /// Сброс состояния атаки юнита. Вызывается при начале нового хода.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void ResetAttackServerRpc()
    {
        hasAttacked.Value = false;
    }
}
