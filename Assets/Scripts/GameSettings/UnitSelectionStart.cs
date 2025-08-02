using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Менеджер выбора юнитов игроками перед началом игры.
/// Хранит выбор каждого клиента и загружает игровую сцену,
/// когда все игроки завершили выбор.
/// </summary>
public class UnitSelectionStart : NetworkBehaviour
{
    /// <summary>
    /// Singleton для глобального доступа к менеджеру выбора.
    /// </summary>
    public static UnitSelectionStart Instance { get; private set; }

    /// <summary>
    /// Словарь выбора юнитов для каждого игрока: ключ — clientId, значение — данные выбора.
    /// </summary>
    private readonly Dictionary<ulong, PlayerUnitSelectionData> _playerSelections = new();

    [Header("Настройки")]
    [Tooltip("Имя игровой сцены, загружаемой после выбора юнитов")]
    [SerializeField] private string gameSceneName = "SampleScene";

    #region Unity Lifecycle

    private void Awake()
    {
        // Реализация паттерна Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Менеджер сохраняется между сценами
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    #endregion

    #region Публичные методы

    /// <summary>
    /// Вызывается клиентом для отправки выбора юнитов на сервер.
    /// </summary>
    /// <param name="selection">Данные выбора юнитов игрока</param>
    public void SubmitSelection(PlayerUnitSelectionData selection)
    {
        if (NetworkManager.Singleton.IsClient)
        {
            SubmitSelectionServerRpc(selection.SlowUnitCount, selection.FastUnitCount);
        }
    }

    /// <summary>
    /// Получить данные выбора юнитов для конкретного клиента.
    /// </summary>
    /// <param name="clientId">Идентификатор клиента</param>
    /// <returns>Данные выбора или null, если выбор отсутствует</returns>
    public PlayerUnitSelectionData GetSelectionForClient(ulong clientId)
    {
        return _playerSelections.TryGetValue(clientId, out var data) ? data : null;
    }

    #endregion

    #region Server RPC

    /// <summary>
    /// Серверный метод, принимающий выбор юнитов от клиента.
    /// Сохраняет данные и загружает игровую сцену при готовности.
    /// </summary>
    /// <param name="longRange">Количество медленных (дальнобойных) юнитов</param>
    /// <param name="shortRange">Количество быстрых (ближних) юнитов</param>
    [ServerRpc(RequireOwnership = false)]
    private void SubmitSelectionServerRpc(int longRange, int shortRange, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        _playerSelections[clientId] = new PlayerUnitSelectionData
        {
            SlowUnitCount = longRange,
            FastUnitCount = shortRange
        };

        Debug.Log($"[Server] Игрок {clientId} выбрал: дальнобойных={longRange}, ближних={shortRange}");

        // Запускаем игру, когда выбор сделали минимум два игрока
        if (_playerSelections.Count >= 2)
        {
            NetworkManager.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
        }
    }

    #endregion
}
