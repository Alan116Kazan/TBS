using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Управляет ходами игроков в сетевой игре.
/// Реализует переключение между игроками и хранит текущего активного игрока.
/// </summary>
public class TurnManager : NetworkBehaviour
{
    // Синглтон для удобного доступа к менеджеру ходов из других классов
    public static TurnManager Instance { get; private set; }

    // Список всех зарегистрированных юнитов в игре
    private readonly List<UnitController> registeredUnits = new();

    // Сетевая переменная, хранящая ID клиента, чей сейчас ход
    // Читается всеми, записывает только сервер
    private NetworkVariable<ulong> currentClientId = new(0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private void Awake()
    {
        // Реализация паттерна синглтон — если есть другой экземпляр, уничтожаем текущий
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // Подписываемся на изменение текущего клиента, чтобы оповестить остальные системы
        currentClientId.OnValueChanged += (_, newValue) =>
        {
            // Генерируем событие начала хода для новых слушателей
            GameEvents.TriggerTurnStarted(newValue);
        };
    }

    /// <summary>
    /// Проверяет, является ли сейчас ходом игрока с указанным clientId.
    /// </summary>
    public bool IsPlayerTurn(ulong clientId) => currentClientId.Value == clientId;

    /// <summary>
    /// Регистрирует юнита в списке управляемых юнитов.
    /// </summary>
    public void RegisterUnit(UnitController unit)
    {
        if (!registeredUnits.Contains(unit))
            registeredUnits.Add(unit);
    }

    /// <summary>
    /// Серверный RPC для окончания хода текущим игроком.
    /// Вызывается клиентом, чтобы сообщить, что его ход завершён.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void EndTurnServerRpc()
    {
        // Вызываем событие окончания хода для всех слушателей
        GameEvents.TriggerTurnEnded(currentClientId.Value);

        // Получаем ID следующего игрока и меняем ход
        ulong nextClientId = GetNextClientId();
        currentClientId.Value = nextClientId;
    }

    /// <summary>
    /// Логика получения следующего клиента (игрока) по списку подключённых.
    /// Возвращает ID следующего клиента, отличного от текущего.
    /// Если других нет, возвращает текущего.
    /// </summary>
    private ulong GetNextClientId()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.ClientId != currentClientId.Value)
                return client.ClientId;
        }

        // Если нет другого клиента, возвращаем текущего (например, один игрок в сети)
        return currentClientId.Value;
    }
}
