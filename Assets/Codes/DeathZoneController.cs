using UnityEngine;

public class DeathZoneController : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other){
        if (other.CompareTag("Player")){
            ReloadCurrentScene();
        }
    }

    void ReloadCurrentScene(){
        sceneManager.LoadScene(sceneManager.GetActiveScene().name);
    }
}
