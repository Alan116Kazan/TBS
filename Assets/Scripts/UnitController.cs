using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public class UnitController : NetworkBehaviour
{
    private NavMeshAgent agent;

    [SerializeField] private GameObject selectionCircle;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        SetSelected(false); // —брос на старте
    }

    [ServerRpc]
    public void SetDestinationServerRpc(Vector3 targetPosition)
    {
        agent.SetDestination(targetPosition);
    }

    public void SetSelected(bool isSelected)
    {
        if (selectionCircle != null)
            selectionCircle.SetActive(isSelected);
    }
}
