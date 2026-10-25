using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    [Header("Ataque del Enemigo")]
    [Tooltip("Cantidad de vida que quita al golpear al PJ (editable)")]
    public float damage = 5f;

    [Tooltip("Tiempo de espera entre cada golpe al estar en contacto continuo (segundos)")]
    public float attackRate = 1.0f;

    private float nextDamageTime = 0f;

    // Daño al tocar con collider físico regular
    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamagePlayer(collision.gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (Time.time >= nextDamageTime)
        {
            TryDamagePlayer(collision.gameObject);
        }
    }

    // Daño si alguno de los colliders es un Trigger (para máxima compatibilidad)
    private void OnTriggerEnter2D(Collider2D collider)
    {
        TryDamagePlayer(collider.gameObject);
    }

    private void OnTriggerStay2D(Collider2D collider)
    {
        if (Time.time >= nextDamageTime)
        {
            TryDamagePlayer(collider.gameObject);
        }
    }

    private void TryDamagePlayer(GameObject target)
    {
        if (target.CompareTag("Player") || target.GetComponent<PlayerHealth>() != null)
        {
            PlayerHealth playerHealth = target.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                nextDamageTime = Time.time + attackRate;
            }
        }
    }
}
