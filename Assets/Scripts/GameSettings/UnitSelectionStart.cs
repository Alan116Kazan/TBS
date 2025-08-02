using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// �������� ������ ������ �������� ����� ������� ����.
/// ������ ����� ������� ������� � ��������� ������� �����,
/// ����� ��� ������ ��������� �����.
/// </summary>
public class UnitSelectionStart : NetworkBehaviour
{
    /// <summary>
    /// Singleton ��� ����������� ������� � ��������� ������.
    /// </summary>
    public static UnitSelectionStart Instance { get; private set; }

    /// <summary>
    /// ������� ������ ������ ��� ������� ������: ���� � clientId, �������� � ������ ������.
    /// </summary>
    private readonly Dictionary<ulong, PlayerUnitSelectionData> _playerSelections = new();

    [Header("���������")]
    [Tooltip("��� ������� �����, ����������� ����� ������ ������")]
    [SerializeField] private string gameSceneName = "SampleScene";

    #region Unity Lifecycle

    private void Awake()
    {
        // ���������� �������� Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // �������� ����������� ����� �������
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    #endregion

    #region ��������� ������

    /// <summary>
    /// ���������� �������� ��� �������� ������ ������ �� ������.
    /// </summary>
    /// <param name="selection">������ ������ ������ ������</param>
    public void SubmitSelection(PlayerUnitSelectionData selection)
    {
        if (NetworkManager.Singleton.IsClient)
        {
            SubmitSelectionServerRpc(selection.SlowUnitCount, selection.FastUnitCount);
        }
    }

    /// <summary>
    /// �������� ������ ������ ������ ��� ����������� �������.
    /// </summary>
    /// <param name="clientId">������������� �������</param>
    /// <returns>������ ������ ��� null, ���� ����� �����������</returns>
    public PlayerUnitSelectionData GetSelectionForClient(ulong clientId)
    {
        return _playerSelections.TryGetValue(clientId, out var data) ? data : null;
    }

    #endregion

    #region Server RPC

    /// <summary>
    /// ��������� �����, ����������� ����� ������ �� �������.
    /// ��������� ������ � ��������� ������� ����� ��� ����������.
    /// </summary>
    /// <param name="longRange">���������� ��������� (������������) ������</param>
    /// <param name="shortRange">���������� ������� (�������) ������</param>
    [ServerRpc(RequireOwnership = false)]
    private void SubmitSelectionServerRpc(int longRange, int shortRange, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        _playerSelections[clientId] = new PlayerUnitSelectionData
        {
            SlowUnitCount = longRange,
            FastUnitCount = shortRange
        };

        Debug.Log($"[Server] ����� {clientId} ������: ������������={longRange}, �������={shortRange}");

        // ��������� ����, ����� ����� ������� ������� ��� ������
        if (_playerSelections.Count >= 2)
        {
            NetworkManager.SceneManager.LoadScene(gameSceneName, LoadSceneMode.Single);
        }
    }

    #endregion
}
