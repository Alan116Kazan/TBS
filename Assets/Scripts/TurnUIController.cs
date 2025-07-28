using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class TurnUIController : MonoBehaviour
{
    [SerializeField] private Button endTurnButton;
    [SerializeField] private Text turnText;

    private ulong myId;
    private bool lastIsMyTurn = false;

    private void Start()
    {
        myId = NetworkManager.Singleton.LocalClientId;
        endTurnButton.onClick.AddListener(OnEndTurnClicked);
        UpdateUI(); // начальная инициализация
    }

    private void Update()
    {
        if (!NetworkManager.Singleton.IsClient) return;

        bool isMyTurn = TurnManager.Instance?.IsPlayerTurn(myId) == true;

        if (isMyTurn != lastIsMyTurn)
        {
            lastIsMyTurn = isMyTurn;
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        bool isMyTurn = lastIsMyTurn;
        turnText.text = isMyTurn ? "Ваш ход" : "Ожидайте хода соперника...";
        endTurnButton.interactable = isMyTurn;
    }

    private void OnEndTurnClicked()
    {
        TurnManager.Instance?.EndTurnServerRpc();
    }
}
