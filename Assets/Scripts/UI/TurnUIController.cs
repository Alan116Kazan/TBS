using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

/// <summary>
/// ��������� UI ����������, ���������� � ����� ������.
/// ����������, ��� ������ ���, � ��������� ��������� ���.
/// </summary>
public class TurnUIController : MonoBehaviour
{
    [SerializeField] private Button endTurnButton; // ������ "��������� ���"
    [SerializeField] private Text turnText;        // ��������� ���� ��� ����������� ������� ����

    // ID ���������� ������� (����� ������)
    private ulong _myId;

    private void Start()
    {
        // �������� ID ���������� ������� �� NetworkManager
        _myId = NetworkManager.Singleton.LocalClientId;

        // ������������� �� ���� �� ������ ���������� ����
        endTurnButton.onClick.AddListener(OnEndTurnClicked);
    }

    private void OnEnable()
    {
        // ������������� �� ������� ������ ����
        GameEvents.OnTurnStarted += HandleTurnStarted;
    }

    private void OnDisable()
    {
        // ������������ �� ������� ��� ���������� �������
        GameEvents.OnTurnStarted -= HandleTurnStarted;
    }

    /// <summary>
    /// ���������� ������� ������ ����.
    /// ��������� UI, ���������, ��� ������ ���, � ����������/������������ ������.
    /// </summary>
    /// <param name="activePlayerId">ID ������, ��� ��� �������</param>
    private void HandleTurnStarted(ulong activePlayerId)
    {
        bool isMyTurn = activePlayerId == _myId;

        // ��������� ����� � UI
        turnText.text = isMyTurn ? "��� ���" : "�������� ���� ���������...";

        // �������� ������ "��������� ���", ���� ������ ��� ���������� ������
        endTurnButton.interactable = isMyTurn;
    }

    /// <summary>
    /// ���������� ��� ������� ������ "��������� ���".
    /// ���������� ������ �� ������ ��� ��������� ����.
    /// </summary>
    private void OnEndTurnClicked()
    {
        // ��������� �������� ����� ��������� ���� �� �������
        TurnManager.Instance?.EndTurnServerRpc();
    }
}
