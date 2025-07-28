using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class NetworkUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Text statusText;
    [SerializeField] private string gameSceneName = "SampleScene";

    private void Start()
    {
        hostButton.onClick.AddListener(OnHostClicked);
        clientButton.onClick.AddListener(OnClientClicked);
    }

    private void OnHostClicked()
    {
        hostButton.interactable = false;
        clientButton.interactable = false;

        statusText.text = "�������� ������� ������...";
        NetworkManager.Singleton.StartHost();

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnClientClicked()
    {
        hostButton.interactable = false;
        clientButton.interactable = false;

        statusText.text = "����������� � �����...";
        NetworkManager.Singleton.StartClient();
    }

    private void OnClientConnected(ulong clientId)
    {
        if (NetworkManager.Singleton.ConnectedClients.Count >= 2 && NetworkManager.Singleton.IsHost)
        {
            // ��������� ������� ����� � ����
            NetworkManager.Singleton.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
        }
    }
}
