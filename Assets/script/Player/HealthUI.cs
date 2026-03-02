using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private Image[] healthImages;

    private void Start()
    {
        if (HealthManager.Instance != null)
        {
            HealthManager.Instance.OnHealthChanged += UpdateHealthUI;
            HealthManager.Instance.OnPlayerDead += OnPlayerDead;
            UpdateHealthUI(HealthManager.Instance.GetCurrentHealth());
        }
    }

    private void OnDestroy()
    {
        if (HealthManager.Instance != null)
        {
            HealthManager.Instance.OnHealthChanged -= UpdateHealthUI;
            HealthManager.Instance.OnPlayerDead -= OnPlayerDead;
        }
    }

    private void UpdateHealthUI(int currentHealth)
    {
        if (healthText != null)
        {
            healthText.text = $"life: {currentHealth}/3";
        }

        if (healthImages != null && healthImages.Length > 0)
        {
            for (int i = 0; i < healthImages.Length; i++)
            {
                if (healthImages[i] != null)
                {
                    healthImages[i].enabled = (i < currentHealth);
                }
            }
        }
    }

    private void OnPlayerDead()
    {
        Debug.Log("Game Over - Player has no health left!");
    }
}
