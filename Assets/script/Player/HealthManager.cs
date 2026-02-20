using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthManager : MonoBehaviour
{
    public static HealthManager Instance { get; private set; }
    
    private int currentHealth = 3;
    private int maxHealth = 3;
    
    // 生命值改变事件（用于UI更新）
    public event System.Action<int> OnHealthChanged;
    // 玩家死亡事件
    public event System.Action OnPlayerDead;
    
    private Animator playerAnimator;

    private void Awake()
    {
        // 单例模式 - 确保只有一个HealthManager实例
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // 获取玩家的Animator组件
        GameObject player = GameObject.Find("Player"); // 根据你的玩家对象名修改
        if (player != null)
        {
            playerAnimator = player.GetComponent<Animator>();
        }
        
        OnHealthChanged?.Invoke(currentHealth);
    }

    /// <summary>
    /// 扣除生命值
    /// </summary>
    public void TakeDamage(int damage = 1)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);
        
        OnHealthChanged?.Invoke(currentHealth);
        
        Debug.Log($"玩家生命值: {currentHealth}/{maxHealth}");
        
        if (currentHealth <= 0)
        {
            // 播放死亡动画
            if (playerAnimator != null)
            {
                playerAnimator.SetTrigger("IsDead");
            }
            
            OnPlayerDead?.Invoke();
            Debug.LogWarning("玩家死亡!");
        }
    }

    /// <summary>
    /// 治疗生命值
    /// </summary>
    public void Heal(int healAmount = 1)
    {
        currentHealth += healAmount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);
        
        OnHealthChanged?.Invoke(currentHealth);
        
        Debug.Log($"玩家生命值恢复: {currentHealth}/{maxHealth}");
    }

    /// <summary>
    /// 获取当前生命值
    /// </summary>
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    /// <summary>
    /// 获取最大生命值
    /// </summary>
    public int GetMaxHealth()
    {
        return maxHealth;
    }

    /// <summary>
    /// 检查玩家是否活着
    /// </summary>
    public bool IsAlive()
    {
        return currentHealth > 0;
    }

    /// <summary>
    /// 重置生命值（新游戏时）
    /// </summary>
    public void ResetHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth);
    }
}
