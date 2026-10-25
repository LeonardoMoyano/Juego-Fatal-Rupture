using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Salud")]
    [Tooltip("Salud máxima del jugador")]
    public float maxHealth = 100f;
    [Tooltip("Salud actual del jugador")]
    public float currentHealth;

    [Header("Efectos al recibir daño")]
    [Tooltip("Parpadeo rojo al recibir golpe")]
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
    /// Aplica daño al jugador
    /// </summary>
    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        Debug.Log($"[PJ] Daño recibido: {amount}. Vida restante: {currentHealth}/{maxHealth}");

        if (flashOnDamage && spriteRenderer != null)
        {
            CancelInvoke(nameof(ResetColor));
            spriteRenderer.color = damageFlashColor;
            Invoke(nameof(ResetColor), 0.15f);
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
        Debug.Log("<color=red><b>[PJ] El jugador ha muerto.</b></color>");
        
        // Deshabilitar movimiento si existe
        PlayerMovement movement = GetComponent<PlayerMovement>();
        if (movement != null) movement.enabled = false;

        PlayerAttack attack = GetComponent<PlayerAttack>();
        if (attack != null) attack.enabled = false;

        // Mostrar menú YOU ARE DEAD
        if (GameOverUI.Instance != null)
        {
            GameOverUI.Instance.ShowDeathScreen();
        }
    }

    /// <summary>
    /// Cura al jugador cierta cantidad de vida
    /// </summary>
    public void Heal(float amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Clamp(currentHealth + amount, 0f, maxHealth);
        Debug.Log($"[PJ] Curado: {amount}. Vida actual: {currentHealth}/{maxHealth}");
    }
}
