using Unity.Netcode;
using UnityEngine;

public class UnitController : NetworkBehaviour
{
    [SerializeField] private float speed = 3f;

    private void Update()
    {
        if (!IsOwner) return;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 direction = new Vector3(horizontal, 0, vertical);
        if (direction != Vector3.zero)
        {
            RequestMoveServerRpc(direction.normalized, speed * Time.deltaTime);
        }
    }

    [ServerRpc]
    private void RequestMoveServerRpc(Vector3 direction, float distance)
    {
        transform.position += direction * distance;
    }
}
