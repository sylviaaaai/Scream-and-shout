using UnityEngine;

public class HealthManager : MonoBehaviour
{
    public static HealthManager Instance { get; private set; }

    [Header("Health")]
    [SerializeField] private int currentHealth = 3;
    [SerializeField] private int maxHealth = 3;

    [Header("Damage Animation")]
    [SerializeField] private string damageAnimationTrigger = "IsDead";
    [SerializeField] private string playerObjectName = "Player";

    public event System.Action<int> OnHealthChanged;
    public event System.Action OnPlayerDead;

    private Animator playerAnimator;

    private void Awake()
    {
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
        ResolvePlayerAnimator();
        OnHealthChanged?.Invoke(currentHealth);
    }

    public void TakeDamage(int damage = 1)
    {
        if (damage <= 0)
        {
            return;
        }

        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0);

        // Play once for every damage event.
        PlayDamageAnimation();
        OnHealthChanged?.Invoke(currentHealth);

        Debug.Log($"Player health: {currentHealth}/{maxHealth}");

        if (currentHealth <= 0)
        {
            OnPlayerDead?.Invoke();
            Debug.LogWarning("Player dead!");
        }
    }

    public void Heal(int healAmount = 1)
    {
        currentHealth += healAmount;
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        OnHealthChanged?.Invoke(currentHealth);
        Debug.Log($"Player healed: {currentHealth}/{maxHealth}");
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public bool IsAlive()
    {
        return currentHealth > 0;
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        OnHealthChanged?.Invoke(currentHealth);
    }

    private void ResolvePlayerAnimator()
    {
        GameObject player = GameObject.Find(playerObjectName);
        if (player != null)
        {
            playerAnimator = player.GetComponent<Animator>();
        }
    }

    private void PlayDamageAnimation()
    {
        if (string.IsNullOrWhiteSpace(damageAnimationTrigger))
        {
            return;
        }

        if (playerAnimator == null)
        {
            ResolvePlayerAnimator();
        }

        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger(damageAnimationTrigger);
        }
    }
}
