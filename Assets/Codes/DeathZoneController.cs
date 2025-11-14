using UnityEngine;
using UnityEngine.SceneManagement;

public class DeathZoneController : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other){
        if (other.CompareTag("Player")){
            ReloadCurrentScene();
        }
    }

    void ReloadCurrentScene(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
