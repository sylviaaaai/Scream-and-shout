using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerDead : MonoBehaviour
{
    [SerializeField] private string[] trapTags = { "traps", "Traps", "trap", "Trap" };
    [SerializeField] private LayerMask trapLayers;
    [SerializeField] private float damageCooldown = 0.5f;
    [SerializeField] private bool respawnOnTrapDamage = true;
    [SerializeField] private bool respawnOnOutOfBoundsDamage = true;
    [SerializeField] private TilemapRenderer backgroundRenderer;
    [SerializeField] private string backgroundObjectName = "Background";
    [SerializeField] private float outOfBackgroundPadding = 0.2f;
    [SerializeField] private PlayerSpawnController playerSpawnController;

    private float lastDamageTime = -1f;
    private bool hasDamagedOutOfBounds;

    private void Start()
    {
        ResolveBackgroundRenderer();
        ResolveSpawnController();
    }

    private void Update()
    {
        CheckOutOfBackgroundRange();
    }

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
        if (!IsTrapObject(other))
        {
            Debug.Log($"Ignored non-trap hit: {other.name}, root={other.transform.root.name}, layer={LayerMask.LayerToName(other.layer)}");
            return;
        }

        TakeDamage("Player hit trap", respawnOnTrapDamage);
    }

    private bool IsTrapObject(GameObject other)
    {
        if (other == null)
        {
            return false;
        }

        if (HasTrapLayerInHierarchy(other.transform))
        {
            return true;
        }

        if (HasTrapTagInHierarchy(other.transform))
        {
            return true;
        }

        return false;
    }

    private bool HasTrapLayerInHierarchy(Transform t)
    {
        Transform current = t;
        while (current != null)
        {
            if (IsInTrapLayer(current.gameObject))
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private bool HasTrapTagInHierarchy(Transform t)
    {
        Transform current = t;
        while (current != null)
        {
            if (HasTrapTag(current.gameObject))
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }

    private bool IsInTrapLayer(GameObject obj)
    {
        int layerMaskValue = 1 << obj.layer;
        return (trapLayers.value & layerMaskValue) != 0;
    }

    private bool HasTrapTag(GameObject obj)
    {
        if (trapTags == null)
        {
            return false;
        }

        for (int i = 0; i < trapTags.Length; i++)
        {
            string tagName = trapTags[i];
            if (!string.IsNullOrWhiteSpace(tagName) && obj.CompareTag(tagName))
            {
                return true;
            }
        }

        return false;
    }

    private void TakeDamage(string reason, bool respawnAtCheckpointIfAlive)
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
        Debug.LogWarning($"{reason}, health reduced by 1.");

        if (respawnAtCheckpointIfAlive && HealthManager.Instance.IsAlive() && playerSpawnController != null)
        {
            playerSpawnController.RespawnAtCheckpoint();
        }
    }

    private void CheckOutOfBackgroundRange()
    {
        if (backgroundRenderer == null)
        {
            return;
        }

        Bounds bounds = backgroundRenderer.bounds;
        Vector3 pos = transform.position;
        bool outOfRange = pos.y < bounds.min.y - outOfBackgroundPadding;

        if (outOfRange)
        {
            if (!hasDamagedOutOfBounds)
            {
                hasDamagedOutOfBounds = true;
                TakeDamage("Player fell out of background range", respawnOnOutOfBoundsDamage);
            }

            return;
        }

        hasDamagedOutOfBounds = false;
    }

    private void ResolveBackgroundRenderer()
    {
        if (backgroundRenderer != null)
        {
            return;
        }

        GameObject backgroundObject = GameObject.Find(backgroundObjectName);
        if (backgroundObject != null)
        {
            backgroundRenderer = backgroundObject.GetComponent<TilemapRenderer>();
        }

        if (backgroundRenderer == null)
        {
            Debug.LogWarning(
                $"PlayerDead cannot find TilemapRenderer on '{backgroundObjectName}'. " +
                "Out-of-background damage check is disabled."
            );
        }
    }

    private void ResolveSpawnController()
    {
        if (playerSpawnController == null)
        {
            playerSpawnController = GetComponent<PlayerSpawnController>();
        }
    }
}
