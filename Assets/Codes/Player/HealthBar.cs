using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    private Image healthBarFill;

    void Start()
    {
        healthBarFill = this.GetComponent<Image>();
    }

    public void UpdateHealthBar(float currentHealth, float maxHealth)
    {
        healthBarFill.fillAmount = currentHealth / maxHealth;
    }
}
