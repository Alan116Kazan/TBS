using UnityEngine;

public class SpawnZone : MonoBehaviour
{
    [SerializeField] private Vector2 size = new(3f, 3f);

    public Vector3 GetRandomSpawnPosition()
    {
        float halfWidth = size.x * 0.5f;
        float halfDepth = size.y * 0.5f;

        float x = Random.Range(-halfWidth, halfWidth);
        float z = Random.Range(-halfDepth, halfDepth);

        return transform.position + new Vector3(x, 0f, z);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(size.x, 0.01f, size.y));
    }

    public Vector2 Size => size; // если понадобится доступ к размеру извне
}
