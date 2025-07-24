using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class NetworkUI : MonoBehaviour
{
    [SerializeField] private Button _hostButton;
    [SerializeField] private Button _clientButton;

    private void Start()
    {
        // Защита от отсутствия NetworkManager
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager.Singleton is null. Ensure NetworkManager is in the scene.");
            return;
        }

        // Назначаем корректные обработчики кнопок
        _hostButton.onClick.AddListener(StartHost);
        _clientButton.onClick.AddListener(StartClient);

        // Подписываемся на событие подключения клиента
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client connected: {clientId}");
    }

    private void StartHost()
    {
        if (NetworkManager.Singleton.StartHost())
        {
            Debug.Log("Started as Host");
        }
        else
        {
            Debug.LogError("Failed to start as Host.");
        }
    }

    private void StartClient()
    {
        if (NetworkManager.Singleton.StartClient())
        {
            Debug.Log("Started as Client");
        }
        else
        {
            Debug.LogError("Failed to start as Client.");
        }
    }
}
