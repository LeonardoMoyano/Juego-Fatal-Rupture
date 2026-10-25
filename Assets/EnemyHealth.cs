using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Salud del Enemigo")]
    [Tooltip("Salud máxima del enemigo (editable)")]
    public float maxHealth = 60f;
    [Tooltip("Salud actual")]
    public float currentHealth;

    [Header("Efectos al recibir daño")]
    [Tooltip("Parpadeo blanco o rojo al ser golpeado")]
    public bool flashOnDamage = true;
    public Color damageFlashColor = Color.red;

    private SpriteRenderer spriteRenderer;
    private Color originalColor = Color.white;
    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    /// <summary>
    /// Aplica daño al enemigo
    /// </summary>
    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log($"[Enemigo '{gameObject.name}'] Recibió daño: {amount}. Vida restante: {currentHealth}/{maxHealth}");

        if (flashOnDamage && spriteRenderer != null)
        {
            CancelInvoke(nameof(ResetColor));
            spriteRenderer.color = damageFlashColor;
            Invoke(nameof(ResetColor), 0.12f);
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void ResetColor()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"<color=green>[Enemigo '{gameObject.name}'] Enemigo eliminado.</color>");

        // Desactivar movimiento del enemigo si lo tiene
        EnemyFollow follow = GetComponent<EnemyFollow>();
        if (follow != null) follow.enabled = false;

        // Destruir el objeto enemigo
        Destroy(gameObject);
    }
}
