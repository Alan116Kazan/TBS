using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using UnityEngine.SceneManagement;

/// <summary>
/// Управляет UI для подключения в сетевой игре.
/// Позволяет запустить игру как хост или клиент,
/// отображает статус подключения и загружает сцену с игрой при готовности.
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

    #region UI Обработчики

    /// <summary>
    /// Запускает хост (сервер + клиент на локальной машине).
    /// Подписывается на события подключения клиентов.
    /// </summary>
    private void OnHostClicked()
    {
        SetButtonsInteractable(false);
        statusText.text = "Запуск хоста...";

        if (_net.StartHost())
        {
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
    /// Запускает клиента и пытается подключиться к хосту.
    /// Подписывается на событие успешного подключения.
    /// </summary>
    private void OnClientClicked()
    {
        SetButtonsInteractable(false);
        statusText.text = "Подключение к хосту...";

        if (_net.StartClient())
        {
            _net.OnClientConnectedCallback += OnConnectedToHost;
        }
        else
        {
            statusText.text = "Ошибка подключения к хосту.";
            SetButtonsInteractable(true);
        }
    }

    #endregion

    #region Обработка сетевых событий

    /// <summary>
    /// Вызывается на хосте при подключении нового клиента.
    /// Если подключилось 2 и более игроков, загружает сцену с игрой.
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
    /// Вызывается на клиенте при успешном подключении к хосту.
    /// Запускает загрузку игровой сцены.
    /// </summary>
    private void OnConnectedToHost(ulong clientId)
    {
        if (!_net.IsClient) return;

        _net.OnClientConnectedCallback -= OnConnectedToHost;
        LoadGameScene();
    }

    #endregion

    #region Загрузка сцены

    /// <summary>
    /// Загружает сцену с игрой.
    /// Хост инициирует загрузку и синхронизацию,
    /// клиенты ждут синхронизации и сами переключаются.
    /// </summary>
    private void LoadGameScene()
    {
        if (_net.IsHost)
        {
            _net.SceneManager.LoadScene(GameSceneName, LoadSceneMode.Single);
        }
    }

    #endregion

    #region Вспомогательные методы

    /// <summary>
    /// Включает или отключает интерактивность кнопок UI.
    /// </summary>
    private void SetButtonsInteractable(bool interactable)
    {
        hostButton.interactable = interactable;
        clientButton.interactable = interactable;
    }

    #endregion
}
