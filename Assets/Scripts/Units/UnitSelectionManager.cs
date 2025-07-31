using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

/// <summary>
/// Менеджер выбора юнитов игроками перед началом игры.
/// Отвечает за хранение выбора каждого клиента и загрузку игровой сцены,
/// когда все игроки сделали выбор.
/// </summary>
public class UnitSelectionManager : NetworkBehaviour
{
    // Singleton для удобного глобального доступа к менеджеру выбора
    public static UnitSelectionManager Instance { get; private set; }

    // Словарь для хранения данных выбора юнитов каждого игрока:
    // ключ — clientId игрока, значение — объект с количеством выбранных юнитов
    private readonly Dictionary<ulong, PlayerUnitSelectionData> _playerSelections = new();

    // Имя сцены с игрой, которую необходимо загрузить после выбора юнитов
    [SerializeField] private string _gameSceneName = "SampleScene";

    private void Awake()
    {
        // Реализация паттерна Singleton:
        // Если уже есть экземпляр, уничтожаем дублирующий объект
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Менеджер не уничтожается при смене сцен
        }
        else
        {
            Destroy(gameObject);
            return; // Прекращаем выполнение, чтобы избежать дублирования
        }
    }

    /// <summary>
    /// Метод для отправки выбора юнитов с клиента на сервер.
    /// Вызывает серверный метод с параметрами выбора.
    /// </summary>
    /// <param name="selection">Данные выбора юнитов игрока</param>
    public void SubmitSelection(PlayerUnitSelectionData selection)
    {
        // Проверяем, что вызов происходит на клиенте
        if (NetworkManager.Singleton.IsClient)
        {
            // Вызываем ServerRpc для передачи данных на сервер
            SubmitSelectionServerRpc(selection.SlowUnitCount, selection.FastUnitCount);
        }
    }

    /// <summary>
    /// Серверный метод, принимающий выбор юнитов от клиента.
    /// Сохраняет данные в словарь и при получении от всех игроков
    /// загружает игровую сцену.
    /// </summary>
    /// <param name="longRange">Количество медленных (дальнобойных) юнитов</param>
    /// <param name="shortRange">Количество быстрых (ближних) юнитов</param>
    /// <param name="rpcParams">Информация о вызове RPC (автоматически)</param>
    [ServerRpc(RequireOwnership = false)]
    private void SubmitSelectionServerRpc(int longRange, int shortRange, ServerRpcParams rpcParams = default)
    {
        // Получаем ClientId отправителя запроса
        ulong clientId = rpcParams.Receive.SenderClientId;

        // Создаем или обновляем запись выбора для данного клиента
        _playerSelections[clientId] = new PlayerUnitSelectionData
        {
            SlowUnitCount = longRange,
            FastUnitCount = shortRange
        };

        Debug.Log($"[Server] Игрок {clientId} выбрал: дальнобойных={longRange}, ближних={shortRange}");

        // Если выбор сделали минимум 2 игрока, загружаем игровую сцену
        if (_playerSelections.Count >= 2)
        {
            NetworkManager.SceneManager.LoadScene(_gameSceneName, LoadSceneMode.Single);
        }
    }

    /// <summary>
    /// Получить данные выбора юнитов для конкретного клиента по его ClientId.
    /// </summary>
    /// <param name="clientId">Идентификатор клиента</param>
    /// <returns>Данные выбора игрока или null, если выбора нет</returns>
    public PlayerUnitSelectionData GetSelectionForClient(ulong clientId)
    {
        return _playerSelections.TryGetValue(clientId, out var data) ? data : null;
    }
}
