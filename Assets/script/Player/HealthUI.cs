using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private TMP_Text healthText;
    [SerializeField] private Image[] healthImages; // 用心形或其他图片显示生命值

    private void Start()
    {
        // 订阅生命值改变事件
        if (HealthManager.Instance != null)
        {
            HealthManager.Instance.OnHealthChanged += UpdateHealthUI;
            HealthManager.Instance.OnPlayerDead += OnPlayerDead;
            UpdateHealthUI(HealthManager.Instance.GetCurrentHealth());
        }
    }

    private void OnDestroy()
    {
        // 取消订阅
        if (HealthManager.Instance != null)
        {
            HealthManager.Instance.OnHealthChanged -= UpdateHealthUI;
            HealthManager.Instance.OnPlayerDead -= OnPlayerDead;
        }
    }

    /// <summary>
    /// 更新UI显示
    /// </summary>
    private void UpdateHealthUI(int currentHealth)
    {
        // 更新文本显示
        if (healthText != null)
        {
            healthText.text = $"life: {currentHealth}/3";
        }

        // 更新图片显示（如果有的话）
        if (healthImages != null && healthImages.Length > 0)
        {
            for (int i = 0; i < healthImages.Length; i++)
            {
                if (healthImages[i] != null)
                {
                    // 如果生命值大于当前索引，显示图片；否则隐藏
                    healthImages[i].enabled = (i < currentHealth);
                }
            }
        }
    }

    /// <summary>
    /// 玩家死亡时的处理
    /// </summary>
    private void OnPlayerDead()
    {
        Debug.Log("Game Over - Player has no health left!");
        // 这里可以显示游戏结束UI、停止游戏等
        // Time.timeScale = 0; // 暂停游戏
    }
}
