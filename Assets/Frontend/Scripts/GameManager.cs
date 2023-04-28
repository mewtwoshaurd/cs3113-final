using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    List<Card> deck = new List<Card>();

    List<Card> enemies = new List<Card>();

    public Transform enemyDeck;
    public GameObject[] playerslots;

    public GameObject[] handslots;

    public GameObject enemyPrefab;

    public TMPro.TextMeshPro PhaseButton;
    public TMPro.TextMeshProUGUI PhaseText;

    public GameObject[] enemyslots;

    public GameObject[] enemyCardObjs = new GameObject[5];

    int[] playableSlots = new int[5];
    public GameObject cardPrefab;

    public int phaseNum;

    public bool transitioning = false;
    private bool PhaseChanged = false;

    public int turnNum = 1;


    List<Card> hand = new List<Card>();
    List<GameEvent> events = new List<GameEvent>();

    public UnitType[] currImplemented = new UnitType[] { UnitType.Dog, UnitType.Bee, UnitType.Bat, UnitType.Spider };
    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < 5; i++)
        {
            playableSlots[i] = 0;
        }
        UnitType randtype;
        for (int i = 0; i < 20; i++)
        {
            int rand = UnityEngine.Random.Range(0, 4);
            randtype = currImplemented[rand];
            deck.Add(Card.UnitCard(randtype));
        }
        events = Game.StartEncounter(deck, UnitType.Dog);
        foreach (GameEvent e in events)
        {
            if (e.eventType == EventType.EncounterStarted)
            {
                enemies = (List<Card>)e.data[0];
                print("enemies Count " + enemies.Count);
                //Debug.Log(enemies);
            }
            if (e.eventType == EventType.HandGiven)
            {
                hand = (List<Card>)e.data[0];
                Debug.Log(hand[0].unitType);
            }
            print(e);
        }

        GenerateHand(hand);
        GenerateEnemies(enemies);

        // events = Game.AttackUnit(hand[0].id,enemies[0].id);
        // foreach(GameEvent e in events){
        //     print(e);
        // }
    }

    // Update is called once per frame
    void Update()
    {
        //print("phaseChanged: " + PhaseChanged);
        //print("phase: " + phaseNum);
        if (phaseNum == 0 && !PhaseChanged && !transitioning)
        {
            PhaseButton.text = "NEXT";
            StartCoroutine(PhaseTextChange("TURN " + turnNum, 1.5f, "CARD PHASE", 1.5f));

        }
        if (phaseNum == 1 && !PhaseChanged && !transitioning)
        {
            PhaseButton.text = "END";
            StartCoroutine(PhaseTextChange("ATTACK PHASE", 1.5f));
        }
        if (phaseNum == 2 && !PhaseChanged && !transitioning)
        {
            PhaseButton.text = "NEXT";
            StartCoroutine(PhaseTextChange("ENEMY PHASE", 1.5f));
            turnNum++;
        }

    }

    public void IncrementPhase()
    {
        try
        {

            events = Game.EndPhase();
            if (phaseNum < 2)
            {
                phaseNum++;
            }
            else
            {
                phaseNum = 0;
            }
            PhaseChanged = !PhaseChanged;
            print("try successful");
        }
        catch (Exception e)
        {
            PhaseText.text = "Play Card to Continue";
            print(e.Message);
        }

    }

    public int currentPhase()
    {
        return phaseNum;
    }

    public bool hasPhaseChanged()
    {
        return PhaseChanged;
    }

    public bool IsTouched(Touch touch, BoxCollider collider)
    {
        Ray _ray = Camera.main.ScreenPointToRay(new Vector3(touch.position.x, touch.position.y, 0));
        RaycastHit _hit;

        return collider.Raycast(_ray, out _hit, 1000.0f);
    }


    public bool IsTouched(Vector3 mousePos, BoxCollider collider)
    {
        Ray _ray = Camera.main.ScreenPointToRay(new Vector3(mousePos.x, mousePos.y, 0));
        RaycastHit _hit;

        return collider.Raycast(_ray, out _hit, 1000.0f);
    }

    public void ChangeEnemyCardsHealth(GameObject card, int deltaHealth)
    {
        card.GetComponent<CardObject>().health += deltaHealth;
        card.GetComponent<CardObject>()._health.text = card.GetComponent<CardObject>().health.ToString();
    }


    public void AttackEvent(int attackedId, int defenderSlot)
    {
        print("enemyid " + enemyCardObjs[defenderSlot].GetComponent<CardObject>().unitId);
        events = Game.AttackUnit(attackedId, enemyCardObjs[defenderSlot].GetComponent<CardObject>().unitId);
        foreach (GameEvent e in events)
        {
            print(e);
            if(e.eventType==EventType.UnitStatChanged){
                print("e.data " + e.data);
            }
        }
    }
    public int IsTouchingPlayerSlot(Touch touch)
    {
        BoxCollider _coll;
        GameObject slot;
        int slotid = -1;
        for (int i = 0; i < playerslots.Length; i++)
        {
            slot = playerslots[i];
            _coll = slot.GetComponent<BoxCollider>();
            if (IsTouched(touch, _coll))
            {
                slotid = i;
                break;
            }
        }
        return slotid;
    }

    public int IsTouchingPlayerSlot(Vector3 mousePos)
    {
        BoxCollider _coll;
        GameObject slot;
        int slotid = -1;
        for (int i = 0; i < playerslots.Length; i++)
        {
            slot = playerslots[i];
            _coll = slot.GetComponent<BoxCollider>();
            if (IsTouched(mousePos, _coll))
            {
                slotid = i;
                break;
            }
        }
        return slotid;
    }

    public int IsTouchingEnemyCard(Touch touch)
    {
        BoxCollider _coll;
        GameObject card;
        int cardId = -1;
        for (int i = 0; i < enemyCardObjs.Length; i++)
        {
            if(enemyCardObjs[i]!=null){
                card = enemyCardObjs[i];
                _coll = card.GetComponent<BoxCollider>();
                if(IsTouched(touch, _coll))
                {
                    cardId = i;
                    break;
                }
            }
        }
        return cardId;

    }

    public int IsTouchingEnemyCard(Vector3 mousePos)
    {
        BoxCollider _coll;
        GameObject card;
        int cardId = -1;
        for (int i = 0; i < enemyCardObjs.Length; i++)
        {
            card = enemyCardObjs[i];
            _coll = card.GetComponent<BoxCollider>();
            if (IsTouched(mousePos, _coll))
            {
                cardId = i;
                break;
            }
        }

        if (cardId > -1)
        {
            return cardId;
        }
        else
        {
            return -1;
        }
    }

    void GenerateEnemies(List<Card> enemies)
    {
        int enemySlotId = 0;
        GameObject currentCard;
        print(enemies.Count);
        print(enemyslots.Length);
        foreach (Card enemy in enemies)
        {
            //print("enemy "+ enemySlotId);
            currentCard = Instantiate(enemyPrefab, enemyDeck.transform.position,Quaternion.Euler(0, 0, 180)); 
            currentCard.GetComponent<CardObject>().SetUnitId(enemy.id);
            currentCard.tag = "EnemyCard";
            currentCard.GetComponent<CardObject>().SetUnitType(enemy.unitType);
            enemyCardObjs[enemySlotId] = currentCard;
            StartCoroutine(Lerp(currentCard, new Vector3(enemyslots[enemySlotId].transform.position.x, enemyslots[enemySlotId].transform.position.y, enemyslots[enemySlotId].transform.position.z - .1f), 1f));
            enemySlotId++;
        }
    }
    void GenerateHand(List<Card> hand)
    {
        int handslotid = 0;
        GameObject currentCard;
        foreach (Card c in hand)
        {
            currentCard = Instantiate(cardPrefab, new Vector3(handslots[handslotid].transform.position.x, handslots[handslotid].transform.position.y, handslots[handslotid].transform.position.z), Quaternion.Euler(0, 0, 180));
            currentCard.GetComponent<CardObject>().SetUnitId(c.id);
            currentCard.tag = "PlayerCard";
            currentCard.GetComponent<CardObject>().SetInHand(true);
            Debug.Log(c.unitType);
            currentCard.GetComponent<CardObject>().SetUnitType(c.unitType);
            StartCoroutine(Lerp(currentCard, new Vector3(handslots[handslotid].transform.position.x, handslots[handslotid].transform.position.y, handslots[handslotid].transform.position.z - .01f), 1f));
            handslotid += 1;
        }
    }


    void TakeHand()
    {
        GameObject[] inHand = GameObject.FindGameObjectsWithTag("PlayerCard");
        foreach (GameObject c in inHand)
        {
            if (c.GetComponent<CardObject>().GetInHand())
            {
                Destroy(c);
            }
        }
    }

    public IEnumerator PhaseTextChange(string PhaseName, float FirstWait, String NextText = "", float SecondWait = 0f)
    {
        transitioning = true;
        PhaseChanged = true;
        PhaseText.text = PhaseName;
        yield return new WaitForSeconds(FirstWait);
        PhaseText.text = NextText;
        yield return new WaitForSeconds(SecondWait);
        PhaseText.text = "";
        //PhaseChanged = false;
        transitioning = false;
    }

    IEnumerator Lerp(GameObject obj, Vector3 end, float duration)
    {
        float time = 0;
        Vector3 start = obj.transform.position;
        while (time < duration)
        {
            obj.transform.position = Vector3.Lerp(start, end, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        obj.transform.position = end;
    }

    public void UpdateGameSlot(int slotid, bool isPlayed)
    {
        if (isPlayed == true)
        {
            playableSlots[slotid] = 1;
        }
        else
        {
            playableSlots[slotid] = 0;
        }
    }

    public bool IsGameSlotOpen(int slotid)
    {
        if (playableSlots[slotid] == 1)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

}
