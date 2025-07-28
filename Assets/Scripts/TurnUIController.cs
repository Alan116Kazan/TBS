using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class TurnUIController : MonoBehaviour
{
    [SerializeField] private Button endTurnButton;
    [SerializeField] private Text turnText;

    private void Start()
    {
        endTurnButton.onClick.AddListener(OnEndTurnClicked);
    }

    private void Update()
    {
        if (!NetworkManager.Singleton.IsClient) return;

        ulong myId = NetworkManager.Singleton.LocalClientId;
        bool isMyTurn = TurnManager.Instance != null && TurnManager.Instance.IsPlayerTurn(myId);

        // Обновляем UI
        turnText.text = isMyTurn ? "Ваш ход" : "Ожидайте хода соперника...";
        endTurnButton.interactable = isMyTurn;
    }

    private void OnEndTurnClicked()
    {
        if (TurnManager.Instance != null)
        {
            TurnManager.Instance.EndTurnServerRpc();
        }
    }
}
