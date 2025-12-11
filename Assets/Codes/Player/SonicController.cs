using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class SonicController : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    [SerializeField] private float acceleration = 40f;
    [SerializeField] private float deceleration = 25f;
    [SerializeField] private float maxSpeed = 12f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float bounceForce = 10f; 
    [SerializeField] private float airDrag = 2f;

    [Header("Detecção de Chão")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckHeight = 0.2f;

    [Header("Visuais")]
    [SerializeField] private Transform visual;
    [SerializeField] private Animator anim;

    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    private PlayerHealth playerHealth; // Referência ao script de vida
    
    private float horizontalInput;
    private bool isGrounded;
    private Vector2 groundNormal;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        playerHealth = GetComponent<PlayerHealth>(); // Busca o script automaticamente
        
        if (anim == null && visual != null) anim = visual.GetComponent<Animator>();
    }

    private void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        UpdateVisuals();
    }

    private void FixedUpdate()
    {
        CheckGround();
        ApplyMovement();
    }

    // --- LÓGICA DE COMBATE UNIFICADA ---
    // --- LÓGICA DE COMBATE CORRIGIDA ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // Verifica todos os pontos de contato
            foreach (ContactPoint2D point in collision.contacts)
            {
                // Verifica se o contato veio de baixo (Sonic está em cima)
                if (point.normal.y > 0.5f)
                {
                    // 1. Matar Inimigo
                    Destroy(collision.gameObject);
                    
                    // 2. Quicar
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, bounceForce);
                    
                    // 3. RETORNAR IMEDIATAMENTE
                    return; 
                }
            }
            
            // Se chegou aqui, é DANO.
            if (playerHealth != null)
            {
                // Calcula a direção do empurrão (do inimigo para o Sonic)
                Vector2 knockbackDirection = (transform.position - collision.transform.position).normalized;
                float knockbackForce = 5f; // Força padrão do empurrão
                
                // Agora passamos os 3 argumentos que o erro pediu:
                // 1. Dano (1)
                // 2. Direção (knockbackDirection)
                // 3. Força (knockbackForce)
                playerHealth.TakeDamage(1f, knockbackDirection, knockbackForce); 
            }
        }
    }
    // -----------------------------------

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
        RaycastHit2D hit = Physics2D.BoxCast(boxCenter, boxSize, 0f, Vector2.down, 0.1f, groundLayer);

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
                rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
            }
        }
        else
        {
            if (horizontalInput != 0)
            {
                rb.AddForce(new Vector2(horizontalInput * (acceleration * 0.5f), 0), ForceMode2D.Force);
            }
            else
            {
                rb.linearVelocity = new Vector2(Mathf.Lerp(rb.linearVelocity.x, 0, airDrag * Time.fixedDeltaTime), rb.linearVelocity.y);
            }
        }

        if (Mathf.Abs(rb.linearVelocity.x) > maxSpeed)
        {
            rb.linearVelocity = new Vector2(Mathf.Sign(rb.linearVelocity.x) * maxSpeed, rb.linearVelocity.y);
        }
    }
}