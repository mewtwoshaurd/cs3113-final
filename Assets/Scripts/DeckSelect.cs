using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DeckSelect : MonoBehaviour
{
    [SerializeField] CardToTextureUtility cttu;
    [SerializeField] string encounterSceneName;

    [SerializeField] RawImage[] cardImages;
    [SerializeField] Image[] buttons;
    [SerializeField] Color unpressedColor;
    [SerializeField] Color pressedColor;

    [SerializeField] GameObject sfxBox;

    int currentlySelected = 0;

    public void SelectFirst()
    {
        currentlySelected = 1;

        buttons[0].color = pressedColor;
        buttons[1].color = unpressedColor;
        buttons[2].color = unpressedColor;

        foreach (RawImage cardImage in cardImages)
        {
            cardImage.enabled = true;
        }

        cardImages[0].texture = cttu.getUnitTexture(UnitType.Dog);
        cardImages[1].texture = cttu.getUnitTexture(UnitType.Dog);
        cardImages[2].texture = cttu.getUnitTexture(UnitType.Dog);
        cardImages[3].texture = cttu.getItemTexture(ItemType.Apple);
        cardImages[4].texture = cttu.getItemTexture(ItemType.Apple);
        cardImages[5].texture = cttu.getItemTexture(ItemType.Dagger);

        GameObject.Instantiate(sfxBox);
    }

    public void SelectSecond()
    {
        currentlySelected = 2;

        buttons[0].color = unpressedColor;
        buttons[1].color = pressedColor;
        buttons[2].color = unpressedColor;

        foreach (RawImage cardImage in cardImages)
        {
            cardImage.enabled = true;
        }

        cardImages[0].texture = cttu.getUnitTexture(UnitType.HedgeHog);
        cardImages[1].texture = cttu.getUnitTexture(UnitType.HedgeHog);
        cardImages[2].texture = cttu.getUnitTexture(UnitType.Bee);
        cardImages[3].texture = cttu.getItemTexture(ItemType.SmokeBomb);
        cardImages[4].texture = cttu.getItemTexture(ItemType.SmokeBomb);
        cardImages[5].texture = cttu.getItemTexture(ItemType.Dagger);

        GameObject.Instantiate(sfxBox);
    }

    public void SelectThird()
    {
        currentlySelected = 3;

        buttons[0].color = unpressedColor;
        buttons[1].color = unpressedColor;
        buttons[2].color = pressedColor;

        foreach (RawImage cardImage in cardImages)
        {
            cardImage.enabled = true;
        }

        cardImages[0].texture = cttu.getUnitTexture(UnitType.Bat);
        cardImages[1].texture = cttu.getUnitTexture(UnitType.Bat);
        cardImages[2].texture = cttu.getUnitTexture(UnitType.Spider);
        cardImages[3].texture = cttu.getItemTexture(ItemType.Coffee);
        cardImages[4].texture = cttu.getItemTexture(ItemType.SmokeBomb);
        cardImages[5].texture = cttu.getItemTexture(ItemType.Star);

        GameObject.Instantiate(sfxBox);
    }

    public void Ok()
    {
        if (currentlySelected != 1 && currentlySelected != 2 && currentlySelected != 3)
        {
            return;
        }

        List<Card> playerDeck = null;
        switch (currentlySelected)
        {
            case 1:
                playerDeck = new List<Card> {
                    Card.UnitCard(UnitType.Dog),
                    Card.UnitCard(UnitType.Dog),
                    Card.UnitCard(UnitType.Dog),
                    Card.ItemCard(ItemType.Apple),
                    Card.ItemCard(ItemType.Apple),
                    Card.ItemCard(ItemType.Dagger)
                };
                break;
            case 2:
                playerDeck = new List<Card> {
                    Card.UnitCard(UnitType.HedgeHog),
                    Card.UnitCard(UnitType.HedgeHog),
                    Card.UnitCard(UnitType.Bee),
                    Card.ItemCard(ItemType.SmokeBomb),
                    Card.ItemCard(ItemType.SmokeBomb),
                    Card.ItemCard(ItemType.Dagger)
                };
                break;
            case 3:
                playerDeck = new List<Card> {
                    Card.UnitCard(UnitType.Bat),
                    Card.UnitCard(UnitType.Bat),
                    Card.UnitCard(UnitType.Spider),
                    Card.ItemCard(ItemType.Coffee),
                    Card.ItemCard(ItemType.SmokeBomb),
                    Card.ItemCard(ItemType.Star)
                };
                break;
        }

        DeckManager.playerDeck = playerDeck;

        SceneManager.LoadScene(encounterSceneName);

        GameObject.Instantiate(sfxBox);
    }
}