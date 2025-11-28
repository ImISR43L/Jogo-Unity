using UnityEngine;

public class EnemyJump : MonoBehaviour
{
   public float jumpForce;

   void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Enemy") collision.gameObject.SendMessage("Jump", jumpForce);
    }
}
