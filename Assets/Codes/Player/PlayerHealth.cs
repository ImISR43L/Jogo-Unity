using UnityEngine;
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Configurações de Vida")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float invincibilityTime = 2f; 
    
    // Tornamos pública para debug, mas o controle ideal é via métodos
    public bool isInvincible = false;

    [Header("Referências")]
    public HealthBar healthBar;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    // --- NOVO MÉTODO ---
    // Permite que o Dash ative/desative a invencibilidade manualmente
    public void SetInvincible(bool state)
    {
        isInvincible = state;
    }
    // -------------------

    public void TakeDamage(float damage, Vector2 knockbackDirection, float knockbackForce = 10f)
    {
        if (isInvincible) return;

        currentHealth -= damage;
        Debug.Log("Player levou dano! Vida atual: " + currentHealth);
        if (healthBar != null) healthBar.UpdateHealthBar(currentHealth, maxHealth);

        rb.linearVelocity = Vector2.zero; 
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityRoutine());
        }
    }

    IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);
            yield return new WaitForSeconds(invincibilityTime);
            spriteRenderer.color = originalColor;
        }
        else
        {
            yield return new WaitForSeconds(invincibilityTime);
        }

        isInvincible = false;
    }

    void Die()
    {
        Debug.Log("Player morreu!");
        gameObject.SetActive(false);
    }
}