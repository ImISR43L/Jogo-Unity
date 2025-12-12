using UnityEngine;
using UnityEngine.SceneManagement; // Necessário para reiniciar a cena
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    [Header("Status")]
    public float maxHealth = 100f;
    public float currentHealth;
    public bool isInvincible = false;

    [Header("Configurações")]
    public float invincibilityTime = 2f; // Tempo imune após levar dano
    public float respawnDelay = 2f;      // Tempo de espera após morrer

    [Header("Referências")]
    public HealthBar healthBar;
    private Rigidbody2D rb;
    private SpriteRenderer[] visuals; // Array para controlar visual (caso tenha filhos)

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        // Pega os renderizadores do próprio objeto e dos filhos
        visuals = GetComponentsInChildren<SpriteRenderer>();
    }

    public void TakeDamage(float damage, Vector2 knockbackDirection, float knockbackForce = 10f)
    {
        // 1. Bloqueia dano se já estiver morto ou invencível
        if (currentHealth <= 0 || isInvincible) return;

        // 2. Aplica Dano
        currentHealth -= damage;
        Debug.Log("Player levou dano! Vida: " + currentHealth);
        
        if (healthBar != null) healthBar.UpdateHealthBar(currentHealth, maxHealth);

        // 3. Aplica Empurrão (Knockback)
        rb.linearVelocity = Vector2.zero; // Zera velocidade anterior para impacto seco
        rb.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        // 4. Verifica Morte ou Invencibilidade
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(InvincibilityRoutine());
        }
    }

    void Die()
    {
        Debug.Log("Player morreu! Reiniciando em " + respawnDelay + " segundos...");
        
        // Inicia a rotina de Respawn em vez de desativar o objeto imediatamente
        StartCoroutine(RespawnRoutine());
    }

    // --- Rotina de Respawn ---
    IEnumerator RespawnRoutine()
    {
        // A. Desabilita Controle e Física
        rb.simulated = false; // Para de cair/colidir
        rb.linearVelocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false; // Inimigos param de bater

        // Tenta desligar o script de movimento (SonicController)
        MonoBehaviour movement = GetComponent<SonicController>();
        if (movement != null) movement.enabled = false;

        // B. Desabilita Visual (Esconde o personagem)
        foreach (var sprite in visuals) sprite.enabled = false;

        // C. Espera o tempo definido (2 segundos)
        yield return new WaitForSeconds(respawnDelay);

        // D. Reinicia a Cena Atual
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // --- Rotina de Invencibilidade ---
    IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        
        // Pisca o personagem enquanto estiver invencível
        float timer = 0;
        while (timer < invincibilityTime)
        {
            foreach (var sprite in visuals) sprite.enabled = !sprite.enabled;
            yield return new WaitForSeconds(0.1f);
            timer += 0.1f;
        }

        // Garante que volta a ficar visível e vulnerável
        foreach (var sprite in visuals) sprite.enabled = true;
        isInvincible = false;
    }
}