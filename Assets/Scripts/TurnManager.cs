using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// �������� ����� � ��������� ������� ����.
/// �������� �� �������� �������� ������, ������������ ����� � ����� ��������� ������.
/// </summary>
public class TurnManager : NetworkBehaviour
{
    /// <summary>
    /// Singleton ��� �������� ����������� �������.
    /// </summary>
    public static TurnManager Instance { get; private set; }

    /// <summary>
    /// ���������������� ������� ������������� ������, ������� ����� � ������� ������.
    /// </summary>
    private readonly NetworkVariable<ulong> currentPlayerId = new(
        0, // �� ��������� 0 � ��������, ��� �������� ������ ���
        NetworkVariableReadPermission.Everyone, // ������ ��� �������
        NetworkVariableWritePermission.Server  // ������ ����� ������ ������
    );

    /// <summary>
    /// �������, ��� ���� � clientId ������, � �������� � ������ ������, ������������� ����� ������.
    /// </summary>
    private readonly Dictionary<ulong, List<UnitController>> playerUnits = new();

    private void Awake()
    {
        Instance = this;
    }

    private new void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    /// <summary>
    /// ��� ��������� ������� � ���� (OnNetworkSpawn) �� �������
    /// ������������� ������� ������������� ������� ��� ������� ��������.
    /// </summary>
    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        var firstClient = NetworkManager.Singleton.ConnectedClientsList.FirstOrDefault();
        if (firstClient != null)
        {
            currentPlayerId.Value = firstClient.ClientId;
        }
    }

    /// <summary>
    /// ������������ ����� �� ����������� ������.
    /// </summary>
    /// <param name="unit">����, ������������� ������</param>
    public void RegisterUnit(UnitController unit)
    {
        if (!playerUnits.TryGetValue(unit.OwnerClientId, out var units))
        {
            units = new List<UnitController>();
            playerUnits[unit.OwnerClientId] = units;
        }

        units.Add(unit);
    }

    /// <summary>
    /// ���������, ����������� �� ������ ��� ������ � clientId.
    /// </summary>
    public bool IsPlayerTurn(ulong clientId) => currentPlayerId.Value == clientId;

    /// <summary>
    /// RPC ��� ���������� ���� ������� �������.
    /// ���������� ��������, �������������� ��������.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void EndTurnServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong nextPlayerId = GetNextPlayerId();
        currentPlayerId.Value = nextPlayerId;

        ResetUnitsForPlayer(nextPlayerId);
    }

    /// <summary>
    /// ���������� ���������� ������ �� �������.
    /// ���� ���� ������ ����� � ���������� ��� clientId,
    /// ����� ���������� ��������.
    /// </summary>
    private ulong GetNextPlayerId()
    {
        // ���������� ���� ������� ����� ��������, � ���� ������� �����������
        foreach (var playerId in playerUnits.Keys.ToList())
        {
            if (playerId != currentPlayerId.Value)
                return playerId;
        }

        // ���� ������ ������� ���, ���������� �������� (��������� �����)
        return currentPlayerId.Value;
    }

    /// <summary>
    /// ���������� ��� ���� ������ ��� ���������� ������.
    /// �������� RPC �� ������ �����, ����� �������� �����, �������� � ���������.
    /// </summary>
    private void ResetUnitsForPlayer(ulong playerId)
    {
        if (!playerUnits.TryGetValue(playerId, out var units)) return;

        foreach (var unit in units)
        {
            unit.ResetTurnServerRpc();
        }
    }
}
