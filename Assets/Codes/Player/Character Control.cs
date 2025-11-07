using UnityEngine;

public class CharacterControl : MonoBehaviour
{
    private Rigidbody2D rb2d;
    public float moveSpeed = 5f;
    private float moveX;
    public float jumpForce = 10f;
    public Transform groundCheck;
    public LayerMask groundLayer;
    private bool isGrounded;
    public Transform visual;
    private Animator anim;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        anim = visual.GetComponent<Animator>();
    }
    void Update()
    {
        moveX = Input.GetAxisRaw("Horizontal");
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
        
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            Jump();
        }
        Move();

        anim.SetBool("isRunning", Mathf.Abs(moveX) > 0f && isGrounded);
        if (moveX > 0.01)
        {
            visual.localScale = new Vector3(1, 1, 1);
        }
        else if (moveX < -0.01f)
        {
            visual.localScale = new Vector3(-1, 1, 1);
        }
        
        anim.SetBool("isJumping", Mathf.Abs(rb2d.linearVelocity.y) > 0f && !isGrounded);
    }

    void Move()
    {
        rb2d.linearVelocity = new Vector2(moveX * moveSpeed, rb2d.linearVelocity.y);
    }
    void Jump()
    {
        rb2d.linearVelocity = new Vector2(rb2d.linearVelocity.x, jumpForce);
    }
}
