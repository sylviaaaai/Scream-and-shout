using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void GameStart()
    {
        GameModeConfig.UseVoiceSpeed = false;
        SceneManager.LoadScene(1);
    }

    public void GameExit()
    {
        GameModeConfig.UseVoiceSpeed = true;
        SceneManager.LoadScene(1);
    }
}
