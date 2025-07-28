using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class UnitSelectionManager : NetworkBehaviour
{
    public static UnitSelectionManager Instance { get; private set; }

    private readonly Dictionary<ulong, PlayerUnitSelectionData> playerSelections = new();

    [SerializeField] private string gameSceneName = "SampleScene";

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
            SubmitSelectionServerRpc(selection.shortMoveLongRangeCount, selection.longMoveShortRangeCount);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitSelectionServerRpc(int longRange, int shortRange, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        playerSelections[clientId] = new PlayerUnitSelectionData
        {
            shortMoveLongRangeCount = longRange,
            longMoveShortRangeCount = shortRange
        };

        Debug.Log($"[Server] Игрок {clientId} выбрал: дальнобойных={longRange}, ближних={shortRange}");

        if (playerSelections.Count >= 2)
        {
            NetworkManager.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
        }
    }

    public PlayerUnitSelectionData GetSelectionForClient(ulong clientId)
    {
        return playerSelections.TryGetValue(clientId, out var data) ? data : null;
    }
}
