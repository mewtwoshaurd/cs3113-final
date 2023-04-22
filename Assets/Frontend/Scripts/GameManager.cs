using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    List<Card> deck = new List<Card>();

    List<Card> enemies = new List<Card>();

    public GameObject[] playerslots;

    public TMPro.TextMeshProUGUI PhaseButton;

    public GameObject[] enemyslots;

    public int phaseNum;

    List<Card> hand = new List<Card>();
    List<GameEvent> events = new List<GameEvent>();
    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < 20; i++)
        {
            deck.Add(Card.UnitCard(UnitType.BaseGame));
        }
        events = Game.StartEncounter(deck, UnitType.BaseGame);
        foreach(GameEvent e in events)
        {
            if (e.eventType == EventType.EncounterStarted)
            {
                enemies = (List<Card>)e.data[0];
                Debug.Log(enemies);
            }
            if (e.eventType == EventType.HandGiven)
            {
                hand = (List<Card>)e.data[0];
            }
        }

        if(phaseNum==0){
            PhaseButton.text = "Next";
        }

    }

    // Update is called once per frame
    void Update()
    {
    }
}
