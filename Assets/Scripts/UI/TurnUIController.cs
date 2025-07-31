using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

/// <summary>
/// Управляет UI элементами, связанными с ходом игрока.
/// Отображает, чей сейчас ход, и позволяет закончить ход.
/// </summary>
public class TurnUIController : MonoBehaviour
{
    [SerializeField] private Button endTurnButton;
    [SerializeField] private Text turnText;

    private ulong _myId;

    private void Start()
    {
        _myId = NetworkManager.Singleton.LocalClientId;
        endTurnButton.onClick.AddListener(OnEndTurnClicked);

        // Попытка сразу обновить UI по текущему игроку
        if (TurnManager.Instance != null)
        {
            HandleTurnStarted(TurnManager.Instance.CurrentPlayerId);
        }
    }

    private void OnEnable()
    {
        GameEvents.OnTurnStarted += HandleTurnStarted;
    }

    private void OnDisable()
    {
        GameEvents.OnTurnStarted -= HandleTurnStarted;
    }

    private void HandleTurnStarted(ulong activePlayerId)
    {
        Debug.Log($"TurnUIController: ход игрока {activePlayerId}, мой ID {_myId}");

        bool isMyTurn = activePlayerId == _myId;
        turnText.text = isMyTurn ? "Ваш ход" : "Ожидайте хода соперника...";
        endTurnButton.interactable = isMyTurn;
    }

    private void OnEndTurnClicked()
    {
        TurnManager.Instance?.EndTurnServerRpc();
    }
}

