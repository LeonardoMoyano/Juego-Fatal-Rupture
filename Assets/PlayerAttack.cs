using UnityEngine;

/// <summary>
/// Sistema de Ataque del Jugador:
/// - Soporta golpe a puño limpio (daño base).
/// - Si el jugador recoge un arma contundente (ej: "Muleta", "Tubo", "Bate"),
///   consulta automáticamente al InventarioManager:
///   -> Aumenta el daño
///   -> Aumenta el alcance del golpe
/// </summary>
public class PlayerAttack : MonoBehaviour
{
    [Header("Ataque Base (Puño Limpio)")]
    [Tooltip("Daño por cada golpe a puño")]
    public float punchDamage = 15f;

    [Tooltip("Distancia o alcance del puñetazo hacia adelante")]
    public float punchRange = 1.2f;

    [Tooltip("Radio del área de impacto")]
    public float attackRadius = 0.6f;

    [Tooltip("Tiempo de espera mínimo entre cada golpe (segundos)")]
    public float attackCooldown = 0.35f;

    [Tooltip("Tecla o botón para golpear")]
    public KeyCode attackKey = KeyCode.Mouse0; // Clic izquierdo del mouse

    [Header("Configuración de Armas Contundentes")]
    [Tooltip("Nombre exacto del ítem en el inventario que sirve como arma (ej: Muleta)")]
    public string weaponItemName = "Muleta";

    [Tooltip("Daño que hace el jugador cuando tiene la Muleta en el inventario")]
    public float weaponDamage = 40f;

    [Tooltip("Alcance mayor del golpe al usar la Muleta")]
    public float weaponRange = 2.2f;

    [Header("Capas de Detección")]
    [Tooltip("Capas de enemigos (si es Nothing golpea a cualquier objeto con EnemyHealth)")]
    public LayerMask enemyLayers;

    private float nextAttackTime = 0f;

    void Update()
    {
        if (Input.GetKeyDown(attackKey) && Time.time >= nextAttackTime)
        {
            Attack();
            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void Attack()
    {
        // 1. Comprobar si el jugador tiene la muleta en el inventario
        bool hasWeapon = false;
        if (InventarioManager.Instance != null)
        {
            hasWeapon = InventarioManager.Instance.ObtenerCantidad(weaponItemName) > 0;
        }

        // 2. Elegir estadísticas (Puño vs Muleta)
        float currentDamage = hasWeapon ? weaponDamage : punchDamage;
        float currentRange = hasWeapon ? weaponRange : punchRange;

        // 3. Punto de impacto hacia donde apunta el jugador (transform.up)
        Vector2 attackPoint = (Vector2)transform.position + (Vector2)transform.up * currentRange;

        // 4. Detectar colisiones
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
            if (hit.gameObject == gameObject) continue;

            EnemyHealth enemyHealth = hit.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(currentDamage);
                hitAnyone = true;
            }
        }

        if (hitAnyone)
        {
            string weaponText = hasWeapon ? $"con {weaponItemName.ToUpper()}" : "a puño limpio";
            Debug.Log($"<color=orange>[Ataque PJ] ¡Golpe {weaponText}! Daño: {currentDamage}</color>");
        }
        else
        {
            Debug.Log($"[Ataque PJ] Golpe al aire ({(hasWeapon ? weaponItemName : "Puño")}).");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        bool hasWeapon = false;
        if (InventarioManager.Instance != null && !string.IsNullOrEmpty(weaponItemName))
        {
            hasWeapon = InventarioManager.Instance.ObtenerCantidad(weaponItemName) > 0;
        }

        float range = hasWeapon ? weaponRange : punchRange;
        Vector2 attackPoint = (Vector2)transform.position + (Vector2)transform.up * range;
        Gizmos.DrawWireSphere(attackPoint, attackRadius);
    }
}
