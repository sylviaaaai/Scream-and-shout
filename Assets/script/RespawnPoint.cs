using UnityEngine;

public class RespawnPoint : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool oneTimeOnly;
    private bool activated;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (activated && oneTimeOnly)
        {
            return;
        }

        if (!other.CompareTag(playerTag))
        {
            return;
        }

        PlayerSpawnController spawnController = other.GetComponent<PlayerSpawnController>();
        if (spawnController == null)
        {
            return;
        }

        spawnController.SetRespawnPoint(transform);
        activated = true;
        Debug.Log($"Respawn point updated: {name}");
    }
}
