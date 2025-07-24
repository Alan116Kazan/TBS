using UnityEngine;

public class SpawnZone : MonoBehaviour
{
    [SerializeField] private Vector2 size = new Vector2(3, 3);

    public Vector3 GetRandomSpawnPosition()
    {
        float x = Random.Range(-size.x / 2f, size.x / 2f);
        float z = Random.Range(-size.y / 2f, size.y / 2f);
        Vector3 localOffset = new Vector3(x, 0f, z);
        return transform.position + localOffset;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(size.x, 0.1f, size.y));
    }
}
