using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelEndTrigger : MonoBehaviour
{
    [Header("Trigger Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool pauseGameOnWin = true;

    [Header("UI References")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TMP_Text completionTimeText;
    [SerializeField] private string timeFormat = "Completion Time: {0:F2}s";

    private float levelStartTime;
    private bool levelCompleted;

    private void Start()
    {
        levelStartTime = Time.time;

        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (levelCompleted)
        {
            return;
        }

        if (!other.CompareTag(playerTag))
        {
            return;
        }

        levelCompleted = true;

        float completionTime = Time.time - levelStartTime;
        if (completionTimeText != null)
        {
            completionTimeText.text = string.Format(timeFormat, completionTime);
        }

        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        if (pauseGameOnWin)
        {
            Time.timeScale = 0f;
        }
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
