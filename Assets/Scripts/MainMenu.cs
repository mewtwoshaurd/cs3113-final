using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string deckSelectSceneName;
    public string tutorialSceneName;
    public GameObject musicBox;
    public GameObject sfxBox;

    void Start()
    {
        DontDestroyOnLoad(musicBox);
    }

    public void loadDeckSelect()
    {
        SceneManager.LoadScene(deckSelectSceneName);
        Instantiate(sfxBox);
    }

    public void loadTutorial()
    {
        SceneManager.LoadScene(tutorialSceneName);
        Instantiate(sfxBox);
    }
}
