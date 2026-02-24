using UnityEngine;

public class RestartButton : MonoBehaviour
{
    [SerializeField] private PlayerSpawnController playerSpawnController;
    [SerializeField] private string playerObjectName = "Player";

    public void RestartFromButton()
    {
        if (playerSpawnController == null)
        {
            ResolvePlayerSpawnController();
        }

        if (playerSpawnController == null)
        {
            Debug.LogError("RestartButton cannot find PlayerSpawnController.");
            return;
        }

        playerSpawnController.RespawnAtStartAndResetLife();
        ResetAllPlatforms();
    }

    private void ResetAllPlatforms()
    {
        Platform1move[] platforms = FindObjectsOfType<Platform1move>();
        for (int i = 0; i < platforms.Length; i++)
        {
            platforms[i].ResetPlatformState();
        }

        Debug.Log($"RestartButton reset {platforms.Length} platform(s).");
    }

    private void ResolvePlayerSpawnController()
    {
        GameObject player = GameObject.Find(playerObjectName);
        if (player != null)
        {
            playerSpawnController = player.GetComponent<PlayerSpawnController>();
        }
    }
}
