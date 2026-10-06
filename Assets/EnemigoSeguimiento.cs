using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    public Transform player;
    public float speed = 3f;

    void Update()
    {
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;

        transform.position += (Vector3)(direction * speed * Time.deltaTime);
    }
}