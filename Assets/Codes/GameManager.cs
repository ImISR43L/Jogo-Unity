using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Configurações de HUD (Jogo)")]
    public TextMeshProUGUI hudCoinText; // Arraste o texto do contador de moedas aqui

    [Header("Configurações de Tela de Vitória")]
    public GameObject winScreenPanel; 
    public TextMeshProUGUI scoreText; 

    private int coinCount = 0;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
        
        if (winScreenPanel != null) winScreenPanel.SetActive(false);
    }

    private void Start()
    {
        // Inicializa o texto com 00
        UpdateHudText();
    }

    public void AddCoin()
    {
        coinCount++;
        UpdateHudText();
    }

    private void UpdateHudText()
    {
        if (hudCoinText != null)
        {
            // Exemplo de formato: "x 5"
            hudCoinText.text = "Moedas: " + coinCount.ToString();
        }
    }

    public void TriggerWin()
    {
        StartCoroutine(WinSequence());
    }

    private IEnumerator WinSequence()
    {
        int finalScore = coinCount * 1000;

        if (winScreenPanel != null)
        {
            winScreenPanel.SetActive(true);

            Image panelImage = winScreenPanel.GetComponent<Image>();
            if (panelImage != null) panelImage.color = Color.black; 

            if (scoreText != null)
            {
                Color originalColor = scoreText.color;
                scoreText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);
                scoreText.text = "PONTUAÇÃO FINAL:\n" + finalScore.ToString();
            }
        }

        // Desativa o HUD para limpar a tela na vitória (opcional)
        if (hudCoinText != null) hudCoinText.gameObject.SetActive(false);

        yield return new WaitForSeconds(2f);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}