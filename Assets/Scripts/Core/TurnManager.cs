using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// ��������� ������ ������� � ������� ����.
/// ��������� ������������ ����� �������� � ������ �������� ��������� ������.
/// </summary>
public class TurnManager : NetworkBehaviour
{
    // �������� ��� �������� ������� � ��������� ����� �� ������ �������
    public static TurnManager Instance { get; private set; }

    // ������ ���� ������������������ ������ � ����
    private readonly List<UnitController> registeredUnits = new();

    // ������� ����������, �������� ID �������, ��� ������ ���
    // �������� �����, ���������� ������ ������
    private NetworkVariable<ulong> currentClientId = new(0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private void Awake()
    {
        // ���������� �������� �������� � ���� ���� ������ ���������, ���������� �������
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // ������������� �� ��������� �������� �������, ����� ���������� ��������� �������
        currentClientId.OnValueChanged += (_, newValue) =>
        {
            // ���������� ������� ������ ���� ��� ����� ����������
            GameEvents.TriggerTurnStarted(newValue);
        };
    }

    /// <summary>
    /// ���������, �������� �� ������ ����� ������ � ��������� clientId.
    /// </summary>
    public bool IsPlayerTurn(ulong clientId) => currentClientId.Value == clientId;

    /// <summary>
    /// ������������ ����� � ������ ����������� ������.
    /// </summary>
    public void RegisterUnit(UnitController unit)
    {
        if (!registeredUnits.Contains(unit))
            registeredUnits.Add(unit);
    }

    /// <summary>
    /// ��������� RPC ��� ��������� ���� ������� �������.
    /// ���������� ��������, ����� ��������, ��� ��� ��� ��������.
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void EndTurnServerRpc()
    {
        // �������� ������� ��������� ���� ��� ���� ����������
        GameEvents.TriggerTurnEnded(currentClientId.Value);

        // �������� ID ���������� ������ � ������ ���
        ulong nextClientId = GetNextClientId();
        currentClientId.Value = nextClientId;
    }

    /// <summary>
    /// ������ ��������� ���������� ������� (������) �� ������ ������������.
    /// ���������� ID ���������� �������, ��������� �� ��������.
    /// ���� ������ ���, ���������� ��������.
    /// </summary>
    private ulong GetNextClientId()
    {
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (client.ClientId != currentClientId.Value)
                return client.ClientId;
        }

        // ���� ��� ������� �������, ���������� �������� (��������, ���� ����� � ����)
        return currentClientId.Value;
    }
}
