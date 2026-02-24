using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text completionTimeText;
    [SerializeField] private string timeFormat = "Time: {0:F2}s";

    [Header("Behavior")]
    [SerializeField] private bool pauseGameOnDeath = true;
    [SerializeField] private bool hidePanelOnStart = true;

    private void Start()
    {
        Time.timeScale = 1f;

        if (hidePanelOnStart && gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }

        SubscribeToHealthEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromHealthEvents();
    }

    private void SubscribeToHealthEvents()
    {
        if (HealthManager.Instance != null)
        {
            HealthManager.Instance.OnPlayerDead += HandlePlayerDead;
        }
    }

    private void UnsubscribeFromHealthEvents()
    {
        if (HealthManager.Instance != null)
        {
            HealthManager.Instance.OnPlayerDead -= HandlePlayerDead;
        }
    }

    private void HandlePlayerDead()
    {
        if (completionTimeText != null)
        {
            completionTimeText.text = string.Format(timeFormat, Time.timeSinceLevelLoad);
        }

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (pauseGameOnDeath)
        {
            Time.timeScale = 0f;
        }
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;

        if (HealthManager.Instance != null)
        {
            HealthManager.Instance.ResetHealth();
        }

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
