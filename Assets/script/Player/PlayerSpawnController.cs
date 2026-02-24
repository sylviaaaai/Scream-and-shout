using UnityEngine;

public class PlayerSpawnController : MonoBehaviour
{
    [Header("Spawn Points")]
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform respawnPoint;
    [SerializeField] private string startPointTag = "StartPoint";
    [SerializeField] private bool autoRespawnOnDeath = true;

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        ResolveStartPoint();

        if (respawnPoint == null)
        {
            respawnPoint = startPoint;
        }

        if (HealthManager.Instance != null)
        {
            HealthManager.Instance.OnPlayerDead += HandlePlayerDead;
        }
    }

    private void OnDestroy()
    {
        if (HealthManager.Instance != null)
        {
            HealthManager.Instance.OnPlayerDead -= HandlePlayerDead;
        }
    }

    public void SetRespawnPoint(Transform newRespawnPoint)
    {
        if (newRespawnPoint != null)
        {
            respawnPoint = newRespawnPoint;
        }
    }

    public void RespawnAtCheckpoint()
    {
        TeleportTo(respawnPoint != null ? respawnPoint : startPoint);
    }

    public void RespawnAtStartAndResetLife()
    {
        if (HealthManager.Instance != null)
        {
            HealthManager.Instance.ResetHealth();
        }

        if (startPoint != null)
        {
            respawnPoint = startPoint;
        }

        TeleportTo(startPoint != null ? startPoint : respawnPoint);
    }

    private void HandlePlayerDead()
    {
        if (!autoRespawnOnDeath)
        {
            return;
        }

        RespawnAtStartAndResetLife();
    }

    private void ResolveStartPoint()
    {
        if (startPoint != null)
        {
            return;
        }

        GameObject startObj = GameObject.FindWithTag(startPointTag);
        if (startObj != null)
        {
            startPoint = startObj.transform;
            return;
        }

        Debug.LogWarning($"PlayerSpawnController cannot find StartPoint with tag '{startPointTag}'.");
    }

    private void TeleportTo(Transform target)
    {
        if (target == null)
        {
            Debug.LogWarning("PlayerSpawnController teleport target is null.");
            return;
        }

        transform.position = target.position;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }
}
