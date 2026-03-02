using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMenuButton : MonoBehaviour
{
    [SerializeField] private string menuSceneName = "Menu";

    public void BackToMenu()
    {
        SceneManager.LoadScene(menuSceneName);
    }
}
