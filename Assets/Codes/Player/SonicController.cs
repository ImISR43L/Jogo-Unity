using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class SonicController : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    [SerializeField] private float acceleration = 40f;
    [SerializeField] private float deceleration = 25f;
    [SerializeField] private float maxSpeed = 12f;
    [SerializeField] private float jumpForce = 15f; 
    [SerializeField] private float bounceForce = 12f;
    [SerializeField] private float airDrag = 2f;

    [Header("Detecção de Chão")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckHeight = 0.2f;

    [Header("Visuais")]
    [SerializeField] private Transform visual;
    [SerializeField] private Animator anim;

    [Header("Combate")]
    [SerializeField] private float minAttackSpeed = 8f;

    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    private PlayerHealth playerHealth;
    
    private float horizontalInput;
    private bool isGrounded;
    private Vector2 groundNormal;

    private float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;

    private Vector2 velocitySnapshot;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        playerHealth = GetComponent<PlayerHealth>();
        
        if (anim == null && visual != null) anim = visual.GetComponent<Animator>();
    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

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
        // 2. Tira uma "foto" da velocidade ANTES de qualquer colisão ou cálculo de física acontecer neste frame
        velocitySnapshot = rb.linearVelocity;

        CheckGround();
        ApplyMovement();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            ContactPoint2D contact = collision.GetContact(0);
            
            // Verifica se bateu por cima
            bool hitFromAbove = contact.normal.y > 0.5f;

            // 3. Verifica a velocidade usando o SNAPSHOT (a velocidade real que ele tinha antes de bater)
            // Usamos o valor absoluto (.Abs) para funcionar tanto para direita quanto para esquerda
            bool isSpeedAttack = Mathf.Abs(velocitySnapshot.x) >= minAttackSpeed;

            // CONDIÇÃO DE VITÓRIA: Bateu por cima OU estava correndo muito rápido
            if (hitFromAbove || isSpeedAttack)
            {
                Destroy(collision.gameObject);
                
                // Só damos o pulinho (bounce) se o ataque foi por cima.
                // Se foi ataque de velocidade (lateral), ele continua correndo sem pular.
                if (hitFromAbove)
                {
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0); 
                    rb.AddForce(Vector2.up * bounceForce, ForceMode2D.Impulse);
                }
            }
            // CONDIÇÃO DE DANO: Falhou nos testes acima
            else
            {
                if (playerHealth != null)
                {
                    Vector2 knockbackDirection = (transform.position - collision.transform.position).normalized;
                    
                    // Empurrão levemente para cima para não prender no chão
                    knockbackDirection = new Vector2(knockbackDirection.x, 0.5f).normalized;
                    
                    playerHealth.TakeDamage(1f, knockbackDirection, 8f);
                }
            }
        }
    }

    private void UpdateVisuals()
    {
        if (visual == null || anim == null) return;

        bool isMovingOpposite = horizontalInput != 0 && Mathf.Sign(horizontalInput) != Mathf.Sign(rb.linearVelocity.x);
        bool isFastEnough = Mathf.Abs(rb.linearVelocity.x) > 2f;
        bool isStopping = isGrounded && isMovingOpposite && isFastEnough;

        if (horizontalInput > 0.01f) visual.localScale = new Vector3(1, 1, 1);
        else if (horizontalInput < -0.01f) visual.localScale = new Vector3(-1, 1, 1);

        anim.SetBool("isJumping", !isGrounded);
        anim.SetBool("isStopping", isStopping);
        anim.SetBool("isRunning", Mathf.Abs(horizontalInput) > 0f && isGrounded && !isStopping);
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
                // CORREÇÃO: Aplica desaceleração APENAS no eixo X
                // Mantemos o Y original para não cancelar o início do pulo
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