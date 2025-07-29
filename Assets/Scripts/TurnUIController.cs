using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class TurnUIController : MonoBehaviour
{
    [SerializeField] private Button endTurnButton;
    [SerializeField] private Text turnText;

    private ulong _myId;
    private bool _lastIsMyTurn = false;

    private void Start()
    {
        _myId = NetworkManager.Singleton.LocalClientId;
        endTurnButton.onClick.AddListener(OnEndTurnClicked);
        UpdateUI(); // начальная инициализация
    }

    private void Update()
    {
        if (!NetworkManager.Singleton.IsClient) return;

        bool isMyTurn = TurnManager.Instance?.IsPlayerTurn(_myId) == true;

        if (isMyTurn != _lastIsMyTurn)
        {
            _lastIsMyTurn = isMyTurn;
            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        bool isMyTurn = _lastIsMyTurn;
        turnText.text = isMyTurn ? "Ваш ход" : "Ожидайте хода соперника...";
        endTurnButton.interactable = isMyTurn;
    }

    private void OnEndTurnClicked()
    {
        TurnManager.Instance?.EndTurnServerRpc();
    }
}
