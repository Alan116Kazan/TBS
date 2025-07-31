using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

/// <summary>
/// �������� ������ ������ �������� ����� ������� ����.
/// �������� �� �������� ������ ������� ������� � �������� ������� �����,
/// ����� ��� ������ ������� �����.
/// </summary>
public class UnitSelectionStart : NetworkBehaviour
{
    // Singleton ��� �������� ����������� ������� � ��������� ������
    public static UnitSelectionStart Instance { get; private set; }

    // ������� ��� �������� ������ ������ ������ ������� ������:
    // ���� � clientId ������, �������� � ������ � ����������� ��������� ������
    private readonly Dictionary<ulong, PlayerUnitSelectionData> _playerSelections = new();

    // ��� ����� � �����, ������� ���������� ��������� ����� ������ ������
    [SerializeField] private string _gameSceneName = "SampleScene";

    private void Awake()
    {
        // ���������� �������� Singleton:
        // ���� ��� ���� ���������, ���������� ����������� ������
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // �������� �� ������������ ��� ����� ����
        }
        else
        {
            Destroy(gameObject);
            return; // ���������� ����������, ����� �������� ������������
        }
    }

    /// <summary>
    /// ����� ��� �������� ������ ������ � ������� �� ������.
    /// �������� ��������� ����� � ����������� ������.
    /// </summary>
    /// <param name="selection">������ ������ ������ ������</param>
    public void SubmitSelection(PlayerUnitSelectionData selection)
    {
        // ���������, ��� ����� ���������� �� �������
        if (NetworkManager.Singleton.IsClient)
        {
            // �������� ServerRpc ��� �������� ������ �� ������
            SubmitSelectionServerRpc(selection.SlowUnitCount, selection.FastUnitCount);
        }
    }

    /// <summary>
    /// ��������� �����, ����������� ����� ������ �� �������.
    /// ��������� ������ � ������� � ��� ��������� �� ���� �������
    /// ��������� ������� �����.
    /// </summary>
    /// <param name="longRange">���������� ��������� (������������) ������</param>
    /// <param name="shortRange">���������� ������� (�������) ������</param>
    /// <param name="rpcParams">���������� � ������ RPC (�������������)</param>
    [ServerRpc(RequireOwnership = false)]
    private void SubmitSelectionServerRpc(int longRange, int shortRange, ServerRpcParams rpcParams = default)
    {
        // �������� ClientId ����������� �������
        ulong clientId = rpcParams.Receive.SenderClientId;

        // ������� ��� ��������� ������ ������ ��� ������� �������
        _playerSelections[clientId] = new PlayerUnitSelectionData
        {
            SlowUnitCount = longRange,
            FastUnitCount = shortRange
        };

        Debug.Log($"[Server] ����� {clientId} ������: ������������={longRange}, �������={shortRange}");

        // ���� ����� ������� ������� 2 ������, ��������� ������� �����
        if (_playerSelections.Count >= 2)
        {
            NetworkManager.SceneManager.LoadScene(_gameSceneName, LoadSceneMode.Single);
        }
    }

    /// <summary>
    /// �������� ������ ������ ������ ��� ����������� ������� �� ��� ClientId.
    /// </summary>
    /// <param name="clientId">������������� �������</param>
    /// <returns>������ ������ ������ ��� null, ���� ������ ���</returns>
    public PlayerUnitSelectionData GetSelectionForClient(ulong clientId)
    {
        return _playerSelections.TryGetValue(clientId, out var data) ? data : null;
    }
}
