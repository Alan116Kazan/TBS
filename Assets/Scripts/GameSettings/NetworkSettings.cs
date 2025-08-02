using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

/// <summary>
/// ��������� UI ��� ����������� � ������� ����.
/// ��������� ��������� ���� ��� ���� ��� ������,
/// ���������� ������ ����������� � ��������� ����� � ����� ��� ����������.
/// </summary>
public class NetworkSettings : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Text statusText;

    private NetworkManager _net => NetworkManager.Singleton;

    private const string GameSceneName = "SampleScene";

    #region Unity Lifecycle

    private void Start()
    {
        hostButton.onClick.AddListener(OnHostClicked);
        clientButton.onClick.AddListener(OnClientClicked);
    }

    #endregion

    #region UI �����������

    /// <summary>
    /// ��������� ���� (������ + ������ �� ��������� ������).
    /// ������������� �� ������� ����������� ��������.
    /// </summary>
    private void OnHostClicked()
    {
        SetButtonsInteractable(false);
        statusText.text = "������ �����...";

        if (_net.StartHost())
        {
            _net.OnClientConnectedCallback += OnClientConnected;
            statusText.text = "�������� ������� ������...";
        }
        else
        {
            statusText.text = "������ ������� �����.";
            SetButtonsInteractable(true);
        }
    }

    /// <summary>
    /// ��������� ������� � �������� ������������ � �����.
    /// ������������� �� ������� ��������� �����������.
    /// </summary>
    private void OnClientClicked()
    {
        SetButtonsInteractable(false);
        statusText.text = "����������� � �����...";

        if (_net.StartClient())
        {
            _net.OnClientConnectedCallback += OnConnectedToHost;
        }
        else
        {
            statusText.text = "������ ����������� � �����.";
            SetButtonsInteractable(true);
        }
    }

    #endregion

    #region ��������� ������� �������

    /// <summary>
    /// ���������� �� ����� ��� ����������� ������ �������.
    /// ���� ������������ 2 � ����� �������, ��������� ����� � �����.
    /// </summary>
    private void OnClientConnected(ulong clientId)
    {
        if (!_net.IsHost) return;

        if (_net.ConnectedClients.Count >= 2)
        {
            _net.OnClientConnectedCallback -= OnClientConnected;
            LoadGameScene();
        }
    }

    /// <summary>
    /// ���������� �� ������� ��� �������� ����������� � �����.
    /// ��������� �������� ������� �����.
    /// </summary>
    private void OnConnectedToHost(ulong clientId)
    {
        if (!_net.IsClient) return;

        _net.OnClientConnectedCallback -= OnConnectedToHost;
        LoadGameScene();
    }

    #endregion

    #region �������� �����

    /// <summary>
    /// ��������� ����� � �����.
    /// ���� ���������� �������� � �������������,
    /// ������� ���� ������������� � ���� �������������.
    /// </summary>
    private void LoadGameScene()
    {
        if (_net.IsHost)
        {
            _net.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
        }
    }

    #endregion

    #region ��������������� ������

    /// <summary>
    /// �������� ��� ��������� ��������������� ������ UI.
    /// </summary>
    private void SetButtonsInteractable(bool interactable)
    {
        hostButton.interactable = interactable;
        clientButton.interactable = interactable;
    }

    #endregion
}
