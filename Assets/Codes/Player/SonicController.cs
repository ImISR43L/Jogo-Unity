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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // --- LÓGICA POR POSIÇÃO (Mais robusta) ---
            
            // 1. Identificar alturas
            // Pega a altura dos pés do Sonic (limite inferior do colisor)
            float sonicFeetY = col.bounds.min.y;
            // Pega a altura do centro do inimigo
            float enemyCenterY = collision.collider.bounds.center.y;

            // 2. Regra do Stomp
            // Se os pés estão acima do centro do inimigo, consideramos que foi um pulo na cabeça
            // Adicionamos uma pequena margem (0.1f) para ser mais permissivo
            bool isStomp = sonicFeetY > (enemyCenterY + 0.1f);

            // Extra: Impede que o Sonic mate o inimigo enquanto sobe (cabeçada por baixo)
            if (rb.linearVelocity.y > 0.1f) isStomp = false;

            if (isStomp)
            {
                // -- VITÓRIA (Stomp) --
                Destroy(collision.gameObject);
                
                // Aplica o pulo (Bounce)
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, bounceForce);
            }
            else
            {
                // -- DERROTA (Dano) --
                if (playerHealth != null)
                {
                    Vector2 knockbackDirection = (transform.position - collision.transform.position).normalized;
                    // Força padrão de empurrão = 5f (pode ajustar aqui)
                    playerHealth.TakeDamage(10f, knockbackDirection, 5f); 
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