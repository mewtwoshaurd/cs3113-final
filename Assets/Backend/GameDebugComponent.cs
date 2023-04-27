using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameDebugComponent : MonoBehaviour
{
    void Start()
    {
        List<Card> cards = new List<Card>();
        for (int i = 0; i < 6; i++)
        {
            cards.Add(Card.UnitCard(UnitType.Camel));
        }

        PrintEvents(Game.StartEncounter(cards, UnitType.Dog));
        PrintEvents(Game.EndPhase());
        PrintEvents(Game.PlayUnit(1));
        PrintEvents(Game.EndPhase());
        PrintEvents(Game.AttackUnit(1, 6));
        PrintEvents(Game.AttackUnit(1, 6));
    }

    void PrintEvents(List<GameEvent> events)
    {
        foreach (GameEvent e in events)
        {
            Debug.Log(e);
        }
    }
}
