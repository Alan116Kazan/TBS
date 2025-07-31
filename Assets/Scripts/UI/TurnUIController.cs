using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

/// <summary>
/// Управляет UI элементами, связанными с ходом игрока.
/// Отображает, чей сейчас ход, и позволяет закончить ход.
/// </summary>
public class TurnUIController : MonoBehaviour
{
    [SerializeField] private Button endTurnButton; // Кнопка "Завершить ход"
    [SerializeField] private Text turnText;        // Текстовое поле для отображения статуса хода

    // ID локального клиента (этого игрока)
    private ulong _myId;

    private void Start()
    {
        // Получаем ID локального клиента из NetworkManager
        _myId = NetworkManager.Singleton.LocalClientId;

        // Подписываемся на клик по кнопке завершения хода
        endTurnButton.onClick.AddListener(OnEndTurnClicked);
    }

    private void OnEnable()
    {
        // Подписываемся на событие начала хода
        GameEvents.OnTurnStarted += HandleTurnStarted;
    }

    private void OnDisable()
    {
        // Отписываемся от события при отключении объекта
        GameEvents.OnTurnStarted -= HandleTurnStarted;
    }

    /// <summary>
    /// Обработчик события начала хода.
    /// Обновляет UI, показывая, чей сейчас ход, и активирует/деактивирует кнопку.
    /// </summary>
    /// <param name="activePlayerId">ID игрока, чей ход начался</param>
    private void HandleTurnStarted(ulong activePlayerId)
    {
        bool isMyTurn = activePlayerId == _myId;

        // Обновляем текст в UI
        turnText.text = isMyTurn ? "Ваш ход" : "Ожидайте хода соперника...";

        // Включаем кнопку "Завершить ход", если сейчас ход локального игрока
        endTurnButton.interactable = isMyTurn;
    }

    /// <summary>
    /// Вызывается при нажатии кнопки "Завершить ход".
    /// Отправляет запрос на сервер для окончания хода.
    /// </summary>
    private void OnEndTurnClicked()
    {
        // Безопасно вызываем метод окончания хода на сервере
        TurnManager.Instance?.EndTurnServerRpc();
    }
}
