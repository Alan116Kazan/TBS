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
    [SerializeField] private Button hostButton;   // Кнопка запуска хоста (сервер + клиент на одной машине)
    [SerializeField] private Button clientButton; // Кнопка запуска клиента (подключение к хосту)
    [SerializeField] private Text statusText;     // Текст для отображения текущего статуса подключения

    // Удобный геттер для синглтона NetworkManager — центра управления сетью в Netcode
    private NetworkManager _net => NetworkManager.Singleton;

    // Имя сцены с игрой, которая будет загружена после успешного подключения
    private const string gameSceneName = "SampleScene";

    private void Start()
    {
        // Подписываем обработчики на нажатия кнопок
        hostButton.onClick.AddListener(OnHostClicked);
        clientButton.onClick.AddListener(OnClientClicked);
    }

    /// <summary>
    /// Обработчик нажатия кнопки запуска хоста.
    /// Запускает сервер и клиента на этой же машине.
    /// </summary>
    private void OnHostClicked()
    {
        SetButtonsInteractable(false); // Блокируем кнопки, чтобы избежать повторных нажатий
        statusText.text = "Запуск хоста...";

        if (_net.StartHost()) // Попытка запустить хост
        {
            // Подписываемся на событие подключения клиентов к хосту
            _net.OnClientConnectedCallback += OnClientConnected;
            statusText.text = "Ожидание второго игрока...";
        }
        else
        {
            // Если запуск не удался — показываем ошибку и разблокируем кнопки
            statusText.text = "Ошибка запуска хоста.";
            SetButtonsInteractable(true);
        }
    }

    /// <summary>
    /// Обработчик нажатия кнопки запуска клиента.
    /// </summary>
    private void OnClientClicked()
    {
        SetButtonsInteractable(false); // Блокируем кнопки
        statusText.text = "Подключение к хосту...";

        if (_net.StartClient()) // Попытка подключиться к хосту
        {
            // Подписываемся на событие успешного подключения к хосту
            _net.OnClientConnectedCallback += OnConnectedToHost;
        }
        else
        {
            // Ошибка подключения — показываем сообщение и разблокируем кнопки
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
        if (!_net.IsHost) return; // Обработчик должен работать только на хосте

        if (_net.ConnectedClients.Count >= 2)
        {
            // Отписываемся, чтобы избежать повторных вызовов
            _net.OnClientConnectedCallback -= OnClientConnected;

            LoadGameScene(); // Загружаем сцену с игрой
        }
    }

    /// <summary>
    /// Вызывается на клиенте при успешном подключении к хосту.
    /// </summary>
    private void OnConnectedToHost(ulong clientId)
    {
        if (!_net.IsClient) return; // Должен выполняться только на клиенте

        // Отписываемся от события, чтобы не повторять обработку
        _net.OnClientConnectedCallback -= OnConnectedToHost;

        LoadGameScene(); // Загружаем сцену с игрой (клиенты сами переключаются с помощью Netcode)
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
            // Хост инициирует загрузку сцены и синхронизацию с клиентами
            _net.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
        }
        else if (_net.IsClient)
        {
            // Клиенты ничего не делают — Netcode самостоятельно переключит их сцену
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
