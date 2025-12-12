using UnityEngine;

public class FinishZone : MonoBehaviour
{
    private bool levelFinished = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Garante que só ativa uma vez e apenas pelo Player
        if (other.CompareTag("Player") && !levelFinished)
        {
            levelFinished = true;
            if (GameManager.instance != null)
            {
                GameManager.instance.TriggerWin();
            }
        }
    }
}