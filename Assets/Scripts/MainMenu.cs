using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string deckSelectSceneName;
    public string tutorialSceneName;

    public void loadDeckSelect()
    {
        SceneManager.LoadScene(deckSelectSceneName);
    }

    public void loadTutorial()
    {
        SceneManager.LoadScene(tutorialSceneName);
    }
}
