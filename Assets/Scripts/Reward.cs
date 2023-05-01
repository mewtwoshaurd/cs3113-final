using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Reward : MonoBehaviour
{
    [SerializeField] string encounterSceneName;
    [SerializeField] CardToTextureUtility cttu;

    [SerializeField] RawImage card;

    UnitType unitReward;
    ItemType itemReward;
    bool giveUnit;

    void Start()
    {
        List<UnitType> units = new List<UnitType>() {
            UnitType.Bat,
            UnitType.Bee,
            UnitType.Dog,
            UnitType.Gorilla,
            UnitType.HedgeHog,
            UnitType.Lion,
            UnitType.Monkey,
            UnitType.Spider
        };
        List<ItemType> items = new List<ItemType>() {
            ItemType.Apple,
            ItemType.Dagger,
            ItemType.SmokeBomb,
            ItemType.Star,
            ItemType.Coffee
        };

        giveUnit = Random.Range(0, 4) == 0;
        unitReward = units[Random.Range(0, units.Count)];
        itemReward = items[Random.Range(0, items.Count)];

        if (giveUnit)
            card.texture = cttu.getUnitTexture(unitReward);
        else
            card.texture = cttu.getItemTexture(itemReward);
    }

    public void Accept()
    {
        if (giveUnit)
            DeckManager.playerDeck.Add(Card.UnitCard(unitReward));
        else
            DeckManager.playerDeck.Add(Card.ItemCard(itemReward));

        SceneManager.LoadScene(encounterSceneName);
    }

    public void Decline()
    {
        SceneManager.LoadScene(encounterSceneName);
    }
}
