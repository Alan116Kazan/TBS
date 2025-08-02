using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

/// <summary>
/// ���������� UI ��� ����������� �������� ���� � ���������� ������� ���������� ����.
/// </summary>
public class TurnDisplayController : MonoBehaviour
{
    [SerializeField] private Text turnText;
    [SerializeField] private Button endTurnButton;

    private ulong _myId;

    private void Start()
    {
        _myId = NetworkManager.Singleton.LocalClientId;
        endTurnButton.onClick.AddListener(OnEndTurnClicked);

        // �������� UI ��� �������, ���� ���� ��� ��������
        if (TurnManager.Instance != null && TurnManager.Instance.IsTurnStarted)
        {
            HandleTurnStarted(TurnManager.Instance.CurrentPlayerId);
        }
        else
        {
            turnText.text = "�������� ������ ����...";
            endTurnButton.interactable = false;
        }
    }

    private void OnEnable()
    {
        GameEvents.OnTurnStarted += HandleTurnStarted;
        GameEvents.OnGameEnded += HandleGameEnded;
    }

    private void OnDisable()
    {
        GameEvents.OnTurnStarted -= HandleTurnStarted;
        GameEvents.OnGameEnded -= HandleGameEnded;
    }

    private void HandleTurnStarted(ulong activePlayerId)
    {
        bool isMyTurn = activePlayerId == _myId;
        turnText.text = isMyTurn ? "��� ���" : "�������� ���� ���������...";
        endTurnButton.interactable = isMyTurn;
    }

    private void HandleGameEnded(ulong winnerClientId)
    {
        endTurnButton.interactable = false;
    }

    private void OnEndTurnClicked()
    {
        TurnManager.Instance?.EndTurnServerRpc();
    }
}
