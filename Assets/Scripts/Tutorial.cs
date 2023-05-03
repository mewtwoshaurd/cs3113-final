using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Tutorial : MonoBehaviour
{
    [SerializeField] string deckSelectSceneName;
    [SerializeField] GameObject sfxBox;

    public void StartGame()
    {
        GameObject.Instantiate(sfxBox);
        SceneManager.LoadScene(deckSelectSceneName);
    }
}
