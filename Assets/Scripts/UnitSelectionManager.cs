using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class UnitSelectionManager : NetworkBehaviour
{
    public static UnitSelectionManager Instance { get; private set; }

    private readonly Dictionary<ulong, PlayerUnitSelectionData> _playerSelections = new();

    [SerializeField] private string _gameSceneName = "SampleScene";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public void SubmitSelection(PlayerUnitSelectionData selection)
    {
        if (NetworkManager.Singleton.IsClient)
        {
            SubmitSelectionServerRpc(selection.SlowUnitCount, selection.FastUnitCount);
        }
    }

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

        if (_playerSelections.Count >= 2)
        {
            NetworkManager.SceneManager.LoadScene(_gameSceneName, LoadSceneMode.Single);
        }
    }

    public PlayerUnitSelectionData GetSelectionForClient(ulong clientId)
    {
        return _playerSelections.TryGetValue(clientId, out var data) ? data : null;
    }
}
