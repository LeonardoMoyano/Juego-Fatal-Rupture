using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Ataque a Mano Limpia")]
    [Tooltip("Daño por cada golpe a mano limpia")]
    public float punchDamage = 15f;

    [Tooltip("Distancia o alcance del golpe hacia adelante")]
    public float attackRange = 1.2f;

    [Tooltip("Radio del área de impacto del golpe")]
    public float attackRadius = 0.6f;

    [Tooltip("Tiempo de espera mínimo entre cada golpe (segundos)")]
    public float attackCooldown = 0.35f;

    [Tooltip("Tecla o botón para golpear")]
    public KeyCode attackKey = KeyCode.Mouse0; // Clic izquierdo del mouse

    [Header("Capas de Detección")]
    [Tooltip("Capas donde se encuentran los enemigos (opcional, si es Nothing golpea a cualquier objeto con EnemyHealth)")]
    public LayerMask enemyLayers;

    private float nextAttackTime = 0f;

    void Update()
    {
        if (Input.GetKeyDown(attackKey) && Time.time >= nextAttackTime)
        {
            Punch();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void Punch()
    {
        // En tu juego PlayerAim rota hacia el mouse (transform.up es el frente hacia donde apunta el PJ)
        Vector2 attackPoint = (Vector2)transform.position + (Vector2)transform.up * attackRange;

        // Detectar colliders en la zona del puñetazo
        Collider2D[] hitEnemies;
        if (enemyLayers.value != 0)
        {
            hitEnemies = Physics2D.OverlapCircleAll(attackPoint, attackRadius, enemyLayers);
        }
        else
        {
            hitEnemies = Physics2D.OverlapCircleAll(attackPoint, attackRadius);
        }

        bool hitAnyone = false;
        foreach (Collider2D hit in hitEnemies)
        {
            // Ignorar el propio collider del jugador
            if (hit.gameObject == gameObject) continue;

            EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(punchDamage);
                hitAnyone = true;
            }
        }

        if (hitAnyone)
        {
            Debug.Log($"<color=yellow>[Golpe PJ] ¡Golpe conectado! Daño infligido: {punchDamage}</color>");
        }
        else
        {
            Debug.Log("[Golpe PJ] Golpe al aire.");
        }
    }

    // Dibuja el radio de golpe en el editor de Unity (Scene view) para calibrarlo fácilmente
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector2 attackPoint = (Vector2)transform.position + (Vector2)transform.up * attackRange;
        Gizmos.DrawWireSphere(attackPoint, attackRadius);
    }
}
