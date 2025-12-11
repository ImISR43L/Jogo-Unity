using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class SonicController : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    [SerializeField] private float acceleration = 40f;     // Aumentei para resposta mais rápida
    [SerializeField] private float deceleration = 25f;     // Freio mais forte
    [SerializeField] private float maxSpeed = 12f;
    [SerializeField] private float jumpForce = 12f;
    [SerializeField] private float airDrag = 2f;           // NOVO: Fricção no ar para evitar deslize infinito

    [Header("Detecção de Chão (Ground Check)")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckHeight = 0.2f; // Altura da caixa de detecção
    [SerializeField] private Transform visual;
    [SerializeField] private Animator anim;

    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    private float horizontalInput;
    private bool isGrounded;
    private Vector2 groundNormal;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<CapsuleCollider2D>();
        if (anim == null && visual != null) anim = visual.GetComponent<Animator>();
    }

    private void Update()
    {
        // Coleta de Input
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // Pulo
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

    private void UpdateVisuals()
    {
        if (visual == null || anim == null) return;

        // Detectar Frenagem (Skid)
        bool isMovingOpposite = horizontalInput != 0 && Mathf.Sign(horizontalInput) != Mathf.Sign(rb.linearVelocity.x);
        bool isFastEnough = Mathf.Abs(rb.linearVelocity.x) > 2f;
        bool isStopping = isGrounded && isMovingOpposite && isFastEnough;

        // Flip (Vira o personagem)
        if (horizontalInput > 0.01f) visual.localScale = new Vector3(1, 1, 1);
        else if (horizontalInput < -0.01f) visual.localScale = new Vector3(-1, 1, 1);

        // Atualiza Animator
        anim.SetBool("isJumping", !isGrounded);
        anim.SetBool("isStopping", isStopping);
        anim.SetBool("isRunning", Mathf.Abs(horizontalInput) > 0f && isGrounded && !isStopping);
    }

    private void CheckGround()
    {
        // Usa BoxCast em vez de Raycast: cria uma caixa na base do colisor
        // Isso é muito mais estável para plataformas e evita "quicar" e perder o chão
        Vector2 boxSize = new Vector2(col.size.x * 0.9f, groundCheckHeight);
        Vector2 boxCenter = (Vector2)transform.position + col.offset + (Vector2.down * (col.size.y / 2f));
        
        RaycastHit2D hit = Physics2D.BoxCast(boxCenter, boxSize, 0f, Vector2.down, 0.1f, groundLayer);

        isGrounded = hit.collider != null;

        if (isGrounded)
        {
            groundNormal = hit.normal;
            // Desenha a caixa verde no editor para facilitar visualização
            Debug.DrawRay(boxCenter, Vector2.down * 0.5f, Color.green);
        }
        else
        {
            groundNormal = Vector2.up;
            Debug.DrawRay(boxCenter, Vector2.down * 0.5f, Color.red);
        }
    }

    private void ApplyMovement()
    {
        if (isGrounded)
        {
            // Movimento no chão
            Vector2 slopeDirection = new Vector2(groundNormal.y, -groundNormal.x);
            float moveDirection = horizontalInput; // Input direto (sem inversão)

            if (horizontalInput != 0)
            {
                // Acelerando
                rb.AddForce(slopeDirection * moveDirection * acceleration, ForceMode2D.Force);
            }
            else
            {
                // Parando (Sem input) -> Aplica Desaceleração forte
                rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
            }
        }
        else
        {
            // Movimento no Ar
            if (horizontalInput != 0)
            {
                rb.AddForce(new Vector2(horizontalInput * (acceleration * 0.5f), 0), ForceMode2D.Force);
            }
            else
            {
                // NOVO: Se não apertar nada no ar, aplica uma pequena fricção (Air Drag)
                // Isso impede que ele continue deslizando para sempre se der um pulinho acidental
                rb.linearVelocity = new Vector2(
                    Mathf.Lerp(rb.linearVelocity.x, 0, airDrag * Time.fixedDeltaTime), 
                    rb.linearVelocity.y
                );
            }
        }

        // Limite de Velocidade (Clamp) Global
        if (Mathf.Abs(rb.linearVelocity.x) > maxSpeed)
        {
            rb.linearVelocity = new Vector2(Mathf.Sign(rb.linearVelocity.x) * maxSpeed, rb.linearVelocity.y);
        }
    }
}