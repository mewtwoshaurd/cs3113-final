using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    List<Card> deck = new List<Card>();

    List<Card> enemies = new List<Card>();

    public GameObject[] playerslots;

    public GameObject[] handslots;

    public TMPro.TextMeshProUGUI PhaseButton;

    public GameObject[] enemyslots;

    public GameObject cardPrefab;

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
                Debug.Log(hand);
            }
        }

        GenerateHand(hand);

        if(phaseNum==0){
            PhaseButton.text = "Next";
        }


        Game.EndPhase();

    }

    // Update is called once per frame
    void Update()
    {
    }

    public bool IsTouched(Touch touch, BoxCollider collider)
    {
        Ray _ray = Camera.main.ScreenPointToRay(new Vector3(touch.position.x, touch.position.y, 0));
        RaycastHit _hit;

        return collider.Raycast(_ray, out _hit, 1000.0f);
    }

    public int IsTouchingPlayerSlot(Touch touch)
    {
        BoxCollider _coll;
        GameObject slot;
        int slotid = -1;
        for(int i = 0; i < playerslots.Length; i++)
        {
            slot = playerslots[i];
            _coll = slot.GetComponent<BoxCollider>();
            if(IsTouched(touch, _coll))
            {
                slotid = i;
                break;
            }
        }
        return slotid;
    }

    /*public void UpdateGameSlot(int slotid)
    {

        events = Game.PlayUnit();
        Debug.Log(events[0]);
    }*/

    void GenerateHand(List<Card> hand)
    {
        int handslotid = 0;
        GameObject currentCard;
        foreach (Card c in hand)
        {
            currentCard = Instantiate(cardPrefab, handslots[handslotid].transform.position, Quaternion.identity);
            //currentCard.SetUnitId(c.id);
            handslotid += 1;
        }
    }
}
