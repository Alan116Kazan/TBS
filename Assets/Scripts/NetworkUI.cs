using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class NetworkUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button clientButton;
    [SerializeField] private Text statusText;

    private NetworkManager net => NetworkManager.Singleton;

    private void Start()
    {
        hostButton.onClick.AddListener(OnHostClicked);
        clientButton.onClick.AddListener(OnClientClicked);
    }

    private void OnHostClicked()
    {
        SetButtonsInteractable(false);
        statusText.text = "Ожидание второго игрока...";

        if (net.StartHost())
        {
            net.OnClientConnectedCallback += OnClientConnected;
            ShowUnitSelectionUI();
        }
        else
        {
            statusText.text = "Ошибка запуска хоста.";
            SetButtonsInteractable(true);
        }
    }

    private void OnClientClicked()
    {
        SetButtonsInteractable(false);
        statusText.text = "Подключение к хосту...";

        if (net.StartClient())
        {
            net.OnClientConnectedCallback += OnConnectedToHost;
        }
        else
        {
            statusText.text = "Ошибка подключения к хосту.";
            SetButtonsInteractable(true);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!net.IsHost) return;

        if (net.ConnectedClients.Count >= 2)
        {
            net.OnClientConnectedCallback -= OnClientConnected;
            Debug.Log("[Host] Оба игрока подключились. Ждём выбора юнитов.");
        }
    }

    private void OnConnectedToHost(ulong clientId)
    {
        if (!net.IsClient) return;

        net.OnClientConnectedCallback -= OnConnectedToHost;
        Debug.Log("[Client] Подключено к хосту. Открываем выбор юнитов.");
        ShowUnitSelectionUI();
    }

    private void ShowUnitSelectionUI()
    {
        UnitSelectionUI ui = FindObjectOfType<UnitSelectionUI>();
        if (ui != null) ui.Show();
        else Debug.LogWarning("UnitSelectionUI не найден в сцене.");
    }

    private void SetButtonsInteractable(bool interactable)
    {
        hostButton.interactable = interactable;
        clientButton.interactable = interactable;
    }
}
