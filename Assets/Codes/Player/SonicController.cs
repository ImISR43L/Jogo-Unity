using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CapsuleCollider2D))]
public class SonicController : MonoBehaviour
{
    [Header("Configurações de Movimento")]
    [SerializeField] private float acceleration = 15f;     // Taxa de ganho de velocidade
    [SerializeField] private float deceleration = 20f;     // Frenagem ao soltar input
    [SerializeField] private float maxSpeed = 12f;         // Velocidade terminal no solo
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float groundCheckDistance = 0.6f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private float horizontalInput;
    private bool isGrounded;
    private Vector2 groundNormal;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        // Coleta de Input
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // Lógica de Pulo
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    private void FixedUpdate()
    {
        CheckGround();
        ApplyMovement();
    }

    private void CheckGround()
    {
        // Raycast para detectar solo e obter a normal (ângulo da superfície)
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        
        isGrounded = hit.collider != null;
        
        if (isGrounded)
        {
            groundNormal = hit.normal;
            // Visualização da normal no editor
            Debug.DrawRay(hit.point, groundNormal, Color.green);
        }
        else
        {
            groundNormal = Vector2.up;
        }
    }

    private void ApplyMovement()
    {
        if (isGrounded)
        {
            // Calcula o vetor de movimento paralelo à superfície
            // Perpendicular à normal do solo (-normal.y, normal.x)
            Vector2 slopeDirection = new Vector2(groundNormal.y, -groundNormal.x);
            
            // Inverte se o input for para a esquerda
            float moveDirection = -horizontalInput; 

            // Aplica força na direção da inclinação
            if (horizontalInput != 0)
            {
                // Aceleração
                rb.AddForce(slopeDirection * moveDirection * acceleration, ForceMode2D.Force);
            }
            else
            {
                // Fricção/Desaceleração manual (counter-force)
                rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
            }

            // Clampar a velocidade máxima (magnitude) para não acumular força infinita
            if (rb.linearVelocity.magnitude > maxSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
            }
        }
        else
        {
            // Controle aéreo simples (opcional: reduzir controle no ar para mais realismo)
            rb.AddForce(new Vector2(horizontalInput * (acceleration * 0.5f), 0), ForceMode2D.Force);
            
            // Clamp horizontal no ar
            float clampedX = Mathf.Clamp(rb.linearVelocity.x, -maxSpeed, maxSpeed);
            rb.linearVelocity = new Vector2(clampedX, rb.linearVelocity.y);
        }
    }
}