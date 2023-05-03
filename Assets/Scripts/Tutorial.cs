using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Tutorial : MonoBehaviour
{
    [SerializeField] string deckSelectSceneName;
    [SerializeField] GameObject sfxBox;
    [SerializeField] List<GameObject> tutorialPages;
    [SerializeField] TMP_Text buttonText;
    int currentPage = 0;

    public void Start()
    {
        tutorialPages[currentPage].SetActive(true);
        for (int i = 1; i < tutorialPages.Count; i++)
        {
            tutorialPages[i].SetActive(false);
        }
    }


    public void NextButton()
    {
        currentPage++;
        GameObject.Instantiate(sfxBox);
        if (currentPage >= tutorialPages.Count)
        {
            SceneManager.LoadScene(deckSelectSceneName);
            return;
        }
        tutorialPages[currentPage - 1].SetActive(false);
        tutorialPages[currentPage].SetActive(true);

        if (currentPage == tutorialPages.Count - 1)
        {
            buttonText.text = "Start Game";
        }
    }

}
