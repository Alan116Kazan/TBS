using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

/// <summary>
/// Управляет UI, связанным с ходом игрока.
/// Показывает, чей сейчас ход, и активирует кнопку "Завершить ход" для текущего игрока.
/// </summary>
public class TurnUIController : MonoBehaviour
{
    [SerializeField] private Button endTurnButton; // Кнопка для завершения хода
    [SerializeField] private Text turnText;        // Текст, отображающий состояние хода

    private ulong _myId;               // Идентификатор локального игрока
    private bool _lastIsMyTurn = false; // Для отслеживания изменений состояния хода и обновления UI

    private void Start()
    {
        // Получаем ClientId локального игрока из NetworkManager
        _myId = NetworkManager.Singleton.LocalClientId;

        // Подписываемся на нажатие кнопки завершения хода
        endTurnButton.onClick.AddListener(OnEndTurnClicked);

        // Обновляем UI при старте
        UpdateUI();
    }

    private void Update()
    {
        // Работаем только на клиенте (не на сервере)
        if (!NetworkManager.Singleton.IsClient) return;

        // Проверяем, принадлежит ли ход локальному игроку
        bool isMyTurn = TurnManager.Instance?.IsPlayerTurn(_myId) == true;

        // Если состояние хода изменилось, обновляем UI
        if (isMyTurn != _lastIsMyTurn)
        {
            _lastIsMyTurn = isMyTurn;
            UpdateUI();
        }
    }

    /// <summary>
    /// Обновляет UI, показывая текст и активируя/деактивируя кнопку в зависимости от хода.
    /// </summary>
    private void UpdateUI()
    {
        bool isMyTurn = _lastIsMyTurn;

        // Текст показывает, чей сейчас ход
        turnText.text = isMyTurn ? "Ваш ход" : "Ожидайте хода соперника...";

        // Кнопка активна только для игрока, чей сейчас ход
        endTurnButton.interactable = isMyTurn;
    }

    /// <summary>
    /// Обработчик нажатия кнопки "Завершить ход".
    /// Вызывает серверный RPC для передачи хода следующему игроку.
    /// </summary>
    private void OnEndTurnClicked()
    {
        TurnManager.Instance?.EndTurnServerRpc();
    }
}
