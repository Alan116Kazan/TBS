using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;  // Для загрузки сцен

/// <summary>
/// Отвечает за UI для подключения в сетевой игре.
/// Позволяет запустить игру как хост или как клиент, отображает статус подключения.
/// Автоматически загружает сцену с игрой при подключении нужного количества игроков.
/// </summary>
public class NetworkUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button hostButton;   // Кнопка запуска хоста
    [SerializeField] private Button clientButton; // Кнопка запуска клиента
    [SerializeField] private Text statusText;     // Текст для отображения статуса подключения

    // Удобный геттер для синглтона NetworkManager
    private NetworkManager _net => NetworkManager.Singleton;

    // Имя сцены с игрой, которую надо загрузить после подключения
    private const string gameSceneName = "SampleScene";

    private void Start()
    {
        // Подписываем кнопки на свои методы
        hostButton.onClick.AddListener(OnHostClicked);
        clientButton.onClick.AddListener(OnClientClicked);
    }

    /// <summary>
    /// Обработчик нажатия кнопки запуска хоста.
    /// Запускает сервер + клиента на этой же машине.
    /// </summary>
    private void OnHostClicked()
    {
        SetButtonsInteractable(false);
        statusText.text = "Запуск хоста...";

        if (_net.StartHost())
        {
            // Подписываемся на событие подключения клиентов
            _net.OnClientConnectedCallback += OnClientConnected;
            statusText.text = "Ожидание второго игрока...";
        }
        else
        {
            statusText.text = "Ошибка запуска хоста.";
            SetButtonsInteractable(true);
        }
    }

    /// <summary>
    /// Обработчик нажатия кнопки запуска клиента.
    /// </summary>
    private void OnClientClicked()
    {
        SetButtonsInteractable(false);
        statusText.text = "Подключение к хосту...";

        if (_net.StartClient())
        {
            // Подписываемся на событие подключения к хосту
            _net.OnClientConnectedCallback += OnConnectedToHost;
        }
        else
        {
            statusText.text = "Ошибка подключения к хосту.";
            SetButtonsInteractable(true);
        }
    }

    /// <summary>
    /// Вызывается на хосте при подключении клиента.
    /// Проверяет, что подключились минимум два игрока, и запускает игру.
    /// </summary>
    private void OnClientConnected(ulong clientId)
    {
        if (!_net.IsHost) return;

        if (_net.ConnectedClients.Count >= 2)
        {
            // Отписываемся, чтобы не сработало повторно
            _net.OnClientConnectedCallback -= OnClientConnected;

            LoadGameScene();
        }
    }

    /// <summary>
    /// Вызывается на клиенте при успешном подключении к хосту.
    /// </summary>
    private void OnConnectedToHost(ulong clientId)
    {
        if (!_net.IsClient) return;

        _net.OnClientConnectedCallback -= OnConnectedToHost;

        LoadGameScene();
    }

    /// <summary>
    /// Запускает загрузку сцены с игрой.
    /// На хосте — инициирует загрузку и синхронизацию с клиентами.
    /// Клиенты ждут синхронизации и сами переключаются.
    /// </summary>
    private void LoadGameScene()
    {
        if (_net.IsHost)
        {
            _net.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
        }
        else if (_net.IsClient)
        {
            // Клиенты ничего не делают, Netcode сам переключает сцену
        }
    }

    /// <summary>
    /// Включает или отключает интерактивность кнопок UI.
    /// </summary>
    private void SetButtonsInteractable(bool interactable)
    {
        hostButton.interactable = interactable;
        clientButton.interactable = interactable;
    }
}
