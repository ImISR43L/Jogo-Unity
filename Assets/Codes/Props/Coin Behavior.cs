using UnityEngine;

public class CoinBehavior : MonoBehaviour
{
    // Variável de travamento para impedir coleta dupla
    private bool wasCollected = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Se já foi coletada, ignora qualquer outra colisão
        if (wasCollected) return;

        if (other.CompareTag("Player"))
        {
            wasCollected = true; // Trava a moeda imediatamente

            if (GameManager.instance != null)
            {
                GameManager.instance.AddCoin();
            }
            
            // Opcional: Desative o Sprite/Collider visualmente se quiser atrasar o Destroy (para tocar som, por exemplo)
            // GetComponent<Collider2D>().enabled = false;
            // GetComponent<SpriteRenderer>().enabled = false;

            Destroy(gameObject);
        }
    }
}