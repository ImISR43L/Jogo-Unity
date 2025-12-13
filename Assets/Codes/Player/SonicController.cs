using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class SonicController : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    [SerializeField] private float acceleration = 40f;
    [SerializeField] private float deceleration = 25f;
    [SerializeField] private float maxSpeed = 12f;
    [SerializeField] private float jumpForce = 15f; 
    [SerializeField] private float airDrag = 2f;

    [Header("Configurações de Dash")]
    [SerializeField] private float dashSpeed = 25f;
    [SerializeField] private float dashDuration = 0.3f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private KeyCode dashKey = KeyCode.LeftShift;

    [Header("Detecção de Chão")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckHeight = 0.2f;

    [Header("Visuais")]
    [SerializeField] private Transform visual;
    [SerializeField] private Animator anim;

    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    private PlayerHealth playerHealth;
    
    private float horizontalInput;
    private bool isGrounded;
    private Vector2 groundNormal;

    private float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;

    // Variáveis de Dash
    private bool isDashing;
    private bool canDash = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        playerHealth = GetComponent<PlayerHealth>();
        
        if (anim == null && visual != null) anim = visual.GetComponent<Animator>();
    }

    private void Update()
    {
        // Impede inputs de movimento durante o Dash
        if (isDashing) return;

        horizontalInput = Input.GetAxisRaw("Horizontal");

        // Input de Dash
        if (Input.GetKeyDown(dashKey) && canDash)
        {
            StartCoroutine(DashCoroutine());
        }

        // Buffer de Pulo
        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        // Execução do Pulo
        if (jumpBufferCounter > 0 && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0;
            isGrounded = false;
        }

        UpdateVisuals();
    }

    private void FixedUpdate()
    {
        // Se estiver no Dash, a física padrão de movimento é ignorada
        if (isDashing) return;

        CheckGround();
        ApplyMovement();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Se estiver no Dash (Invencível) -> Mata o Inimigo
            if (isDashing)
            {
                Destroy(collision.gameObject);
            }
            // Se NÃO estiver no Dash -> Toma 10 de Dano (Independente do lado da colisão)
            else
            {
                if (playerHealth != null)
                {
                    Vector2 knockbackDirection = (transform.position - collision.transform.position).normalized;
                    knockbackDirection = new Vector2(knockbackDirection.x, 0.5f).normalized;
                    
                    // VALOR ALTERADO: 1f -> 10f
                    playerHealth.TakeDamage(10f, knockbackDirection, 8f);
                }
            }
        }
    }

    private IEnumerator DashCoroutine()
    {
        isDashing = true;
        canDash = false;

        // 1. Ativa a invencibilidade suprema no Health
        // Isso impede dano de qualquer fonte (colisão, espinhos, projéteis)
        if (playerHealth != null) playerHealth.SetInvincible(true);

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;

        float direction = visual.localScale.x; 
        if (horizontalInput != 0) direction = Mathf.Sign(horizontalInput);

        rb.linearVelocity = new Vector2(direction * dashSpeed, 0f);

        if (anim != null) anim.SetBool("isJumping", true);

        // Aguarda a duração do movimento
        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        rb.linearVelocity = Vector2.zero; 
        isDashing = false; // O ataque (dano no inimigo) para aqui

        // 2. SEGURANÇA EXTRA (Grace Period):
        // Mantém a invencibilidade por mais 0.2s APÓS parar o movimento.
        // Isso evita que você tome dano se parar exatamente dentro de um inimigo.
        yield return new WaitForSeconds(0.2f);

        // Desativa a invencibilidade manual
        if (playerHealth != null) playerHealth.SetInvincible(false);

        // Cooldown
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void UpdateVisuals()
    {
        if (visual == null || anim == null) return;

        bool isMovingOpposite = horizontalInput != 0 && Mathf.Sign(horizontalInput) != Mathf.Sign(rb.linearVelocity.x);
        bool isFastEnough = Mathf.Abs(rb.linearVelocity.x) > 2f;
        bool isStopping = isGrounded && isMovingOpposite && isFastEnough;

        if (horizontalInput > 0.01f) visual.localScale = new Vector3(1, 1, 1);
        else if (horizontalInput < -0.01f) visual.localScale = new Vector3(-1, 1, 1);

        // Se estiver Dashing, forçamos o estado de "Pulo" (giro) ou outra animação específica
        anim.SetBool("isJumping", !isGrounded || isDashing);
        anim.SetBool("isStopping", isStopping);
        anim.SetBool("isRunning", Mathf.Abs(horizontalInput) > 0f && isGrounded && !isStopping && !isDashing);
    }

    private void CheckGround()
    {
        Vector2 boxSize = new Vector2(col.size.x * 0.9f, groundCheckHeight);
        Vector2 boxCenter = (Vector2)transform.position + col.offset + (Vector2.down * (col.size.y / 2f));
        
        RaycastHit2D hit = Physics2D.BoxCast(boxCenter, boxSize, 0f, Vector2.down, 0.05f, groundLayer);

        isGrounded = hit.collider != null;

        if (isGrounded) groundNormal = hit.normal;
        else groundNormal = Vector2.up;
    }

    private void ApplyMovement()
    {
        if (isGrounded)
        {
            Vector2 slopeDirection = new Vector2(groundNormal.y, -groundNormal.x);
            
            if (horizontalInput != 0)
            {
                rb.AddForce(slopeDirection * horizontalInput * acceleration, ForceMode2D.Force);
            }
            else
            {
                float newX = Mathf.Lerp(rb.linearVelocity.x, 0, deceleration * Time.fixedDeltaTime);
                rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
            }
        }
        else
        {
            // No Ar
            if (horizontalInput != 0)
            {
                rb.AddForce(new Vector2(horizontalInput * (acceleration * 0.5f), 0), ForceMode2D.Force);
            }
            else
            {
                float newX = Mathf.Lerp(rb.linearVelocity.x, 0, airDrag * Time.fixedDeltaTime);
                rb.linearVelocity = new Vector2(newX, rb.linearVelocity.y);
            }
        }

        // Clamp apenas horizontal
        if (Mathf.Abs(rb.linearVelocity.x) > maxSpeed)
        {
            float limitedX = Mathf.Sign(rb.linearVelocity.x) * maxSpeed;
            rb.linearVelocity = new Vector2(limitedX, rb.linearVelocity.y);
        }
    }
}