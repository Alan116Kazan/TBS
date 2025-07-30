using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class UnitSelectionManager : NetworkBehaviour
{
    // Singleton для удобного глобального доступа
    public static UnitSelectionManager Instance { get; private set; }

    // Словарь с выбором юнитов для каждого игрока (key — clientId)
    private readonly Dictionary<ulong, PlayerUnitSelectionData> _playerSelections = new();

    [SerializeField] private string _gameSceneName = "SampleScene"; // Имя сцены игры для загрузки

    private void Awake()
    {
        // Реализация паттерна Singleton: оставляем только один экземпляр
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject); // Менеджер не уничтожается при смене сцены
    }

    // Метод, вызываемый клиентом для отправки своего выбора серверу
    public void SubmitSelection(PlayerUnitSelectionData selection)
    {
        if (NetworkManager.Singleton.IsClient)
        {
            // Отправляем данные на сервер через ServerRpc
            SubmitSelectionServerRpc(selection.SlowUnitCount, selection.FastUnitCount);
        }
    }

    // Серверный метод, принимающий выбор от клиентов
    [ServerRpc(RequireOwnership = false)]
    private void SubmitSelectionServerRpc(int longRange, int shortRange, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId; // Получаем Id отправителя

        // Сохраняем выбор игрока в словарь
        _playerSelections[clientId] = new PlayerUnitSelectionData
        {
            SlowUnitCount = longRange,
            FastUnitCount = shortRange
        };

        Debug.Log($"[Server] Игрок {clientId} выбрал: дальнобойных={longRange}, ближних={shortRange}");

        // Как только получили выбор от двух игроков — загружаем игровую сцену
        if (_playerSelections.Count >= 2)
        {
            NetworkManager.SceneManager.LoadScene(_gameSceneName, LoadSceneMode.Single);
        }
    }

    // Получить выбор юнитов для конкретного клиента
    public PlayerUnitSelectionData GetSelectionForClient(ulong clientId)
    {
        return _playerSelections.TryGetValue(clientId, out var data) ? data : null;
    }
}
