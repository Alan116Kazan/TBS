using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

/// <summary>
/// ��������� UI ����������, ���������� � ����� ������.
/// ����������, ��� ������ ���, � ��������� ��������� ���.
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

        // ������� ����� �������� UI �� �������� ������
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
        Debug.Log($"TurnUIController: ��� ������ {activePlayerId}, ��� ID {_myId}");

        bool isMyTurn = activePlayerId == _myId;
        turnText.text = isMyTurn ? "��� ���" : "�������� ���� ���������...";
        endTurnButton.interactable = isMyTurn;
    }

    private void OnEndTurnClicked()
    {
        TurnManager.Instance?.EndTurnServerRpc();
    }
}

