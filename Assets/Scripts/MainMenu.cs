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
    public GameObject forcedTutorialCanvas;

    public bool resetPlayerPrefs = false;

    void Start()
    {
        DontDestroyOnLoad(musicBox);

#if UNITY_EDITOR
        if (resetPlayerPrefs)
        {
            PlayerPrefs.DeleteAll();
        }
#endif
    }

    public void loadDeckSelect()
    {
        Instantiate(sfxBox);

        if (PlayerPrefs.GetInt("tutorial", 0) == 0)
        {
            forcedTutorialCanvas.SetActive(true);
            return;
        }
        SceneManager.LoadScene(deckSelectSceneName);
    }

    public void loadTutorial()
    {
        Instantiate(sfxBox);
        PlayerPrefs.SetInt("tutorial", 1);
        SceneManager.LoadScene(tutorialSceneName);

    }

    public void closeForcedTutorialCanvas()
    {
        Instantiate(sfxBox);
        forcedTutorialCanvas.SetActive(false);
    }
}
