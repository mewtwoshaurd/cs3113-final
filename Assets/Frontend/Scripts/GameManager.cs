using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGame : MonoBehaviour
{
    List<Card> deck = new List<Card>();
    
    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < 20; i++)
        {
            deck.Add(Card.UnitCard(UnitType.BaseGame));
        }
        Game.StartEncounter(deck, UnitType.BaseGame);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
