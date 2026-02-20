using UnityEngine;

public class PlayerDead : MonoBehaviour
{
    [SerializeField] private string trapTag = "traps";
    [SerializeField] private float damageCooldown = 0.5f;
    private float lastDamageTime = -1f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TryDamage(collision.gameObject, "Trigger");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamage(collision.gameObject, "Collision");
    }

    private void TryDamage(GameObject other, string hitType)
    {
        Debug.Log($"PlayerDead hit by {hitType}: {other.name}, tag={other.tag}");
        if (!other.CompareTag(trapTag))
        {
            return;
        }
        TakeDamageFromTrap();
    }

    private void TakeDamageFromTrap()
    {
        if (Time.time - lastDamageTime < damageCooldown)
        {
            return;
        }

        lastDamageTime = Time.time;

        if (HealthManager.Instance == null)
        {
            Debug.LogError("HealthManager.Instance is null. Add HealthManager to an active GameObject.");
            return;
        }

        HealthManager.Instance.TakeDamage(1);
        Debug.LogWarning("Player hit trap, health reduced by 1.");
    }
}
