using UnityEngine;
using System.Collections; // Necessário para usar Coroutines

public class PlayerHealth : MonoBehaviour
{
    [Header("Configurações de Vida")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float invincibilityTime = 2f; // Tempo que fica imune após dano
    private bool isInvincible = false;

    [Header("Referências")]
    public HealthBar healthBar;
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer; // Para piscar o personagem

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        
        // Tenta achar o SpriteRenderer no objeto ou nos filhos (para o efeito visual)
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void TakeDamage(float damage, Vector2 knockbackDirection, float knockbackForce = 10f)
    {
        // 1. Se já estiver invencível, ignora o dano e sai da função
        if (isInvincible) return;

        // 2. Aplica o Dano
        currentHealth -= damage;
        Debug.Log("Player levou dano! Vida atual: " + currentHealth);
        healthBar.UpdateHealthBar(currentHealth, maxHealth);

        // 3. Aplica o Empurrão (Knockback)
        // Zeramos a velocidade antes para o empurrão ser consistente
        rb.linearVelocity = Vector2.zero; 
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        // 4. Checa Morte
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // 5. Ativa a Invencibilidade
            StartCoroutine(InvincibilityRoutine());
        }
    }

    // Rotina temporária (Timer)
    IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        
        // Efeito visual (Piscar) - Opcional, mas útil
        if (spriteRenderer != null)
        {
            Color originalColor = spriteRenderer.color;
            // Reduz a opacidade para 50%
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);
            
            yield return new WaitForSeconds(invincibilityTime);
            
            // Restaura a cor original
            spriteRenderer.color = originalColor;
        }
        else
        {
            // Se não tiver sprite, apenas espera o tempo
            yield return new WaitForSeconds(invincibilityTime);
        }

        isInvincible = false;
    }

    void Die()
    {
        Debug.Log("Player morreu!");
        // Opcional: Tocar som de morte ou reiniciar fase aqui
        gameObject.SetActive(false);
    }
}