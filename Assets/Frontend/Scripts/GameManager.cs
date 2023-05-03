using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    List<Card> deck = new List<Card>();

    List<Card> enemies = new List<Card>();

    //public Transform enemyDeck;
    public GameObject[] playerslots;

    public GameObject RulesCanvas;

    public GameObject[] handslots;

    public GameObject enemyPrefab;

    public TMPro.TextMeshPro PhaseButton;
    public TMPro.TextMeshProUGUI PhaseText;

    public TMPro.TextMeshProUGUI TurnText;

    
    public TMPro.TextMeshProUGUI WarningText;

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
        RulesCanvas.transform.position = new Vector3(100000000,100000000,0);
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
                //print("enemies Count " + enemies.Count);
                foreach(Card en in enemies){
                    //print("Enemy id " + en.id);
                }
                //Debug.Log(enemies);
            }
            if (e.eventType == EventType.HandGiven)
            {
                hand = (List<Card>)e.data[0];
                foreach(Card card in hand){
                    //print("player id " + card.id);
                }
            }
            //print(e);
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
            StartCoroutine(PhaseTextChange( "CARD PHASE"));

        }
        if (phaseNum == 1 && !PhaseChanged && !transitioning)
        {
            PhaseButton.text = "END";
            StartCoroutine(PhaseTextChange("ATTACK PHASE"));
        }
        if (phaseNum == 2 && !PhaseChanged && !transitioning)
        {
            PhaseButton.text = "NEXT";
            StartCoroutine(PhaseTextChange("ENEMY PHASE"));
            
        }

    }

    public void IncrementPhase()
    {
        events = Game.EndPhase();
        foreach(GameEvent e in events){
            if(e.eventType == EventType.Error){
                GiveWarning("You Must Play a Card to Move On",2f);
            }
        }
        if(phaseNum == 2){
            EnemyAttack(events);
        }
        if (phaseNum < 2)
        {
            phaseNum++;
        }
        else
        {
            phaseNum = 0;
        }
        PhaseChanged = !PhaseChanged;
        //print("try successful");

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

    public bool IsTouched(Touch touch, CapsuleCollider collider)
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

    public void EnemyAttack(List<GameEvent> events){
        GameObject[] playercards = GameObject.FindGameObjectsWithTag("PlayerCard");
        GameObject[] enemycards = GameObject.FindGameObjectsWithTag("EnemyCard");

        foreach(GameEvent e in events){
            if(e.eventType==EventType.UnitAttacked){
                print(e.data);
            }
        }
    }

    public void DisplayEnemyAttack(int attackerId, int defenderId){
        GameObject[] playercards = GameObject.FindGameObjectsWithTag("PlayerCard");
        GameObject[] enemycards = GameObject.FindGameObjectsWithTag("EnemyCard");
        Vector3 attackerPos = new Vector3(0,0,0);
        Vector3 defenderPos = new Vector3(0,0,0);
        GameObject attacker = null;
        GameObject defender = null;
        foreach(GameObject card in playercards){
            if(card.GetComponent<CardObject>().GetUnitId() == defenderId){
                defender = card;
                defenderPos = card.transform.position;
                print("card found 1");
                break;
            }
        }
        foreach(GameObject card in enemycards){
            if(card.GetComponent<CardObject>().GetUnitId() == attackerId){
                attacker = card;
                print("card found 2");
                break;
            }
        }        
        if(attacker!=null && defender != null){
            print("both found");
            print(attacker.transform.position);
            attackerPos = attacker.transform.position;
            StartCoroutine(attackAnimation(attacker,attackerPos,defender));
        }
        
    }

    IEnumerator attackAnimation(GameObject attacker, Vector3 attackerPos, GameObject defeneder){
        print("running animation");
        StartCoroutine(Lerp(attacker,new Vector3(attackerPos.x,attackerPos.y,attackerPos.z-1f),10f));
        new WaitForSeconds(1f);
        StartCoroutine(Lerp(attacker,defender.transform.position,10f));
        new WaitForSeconds(1f);
        StartCoroutine(Lerp(attacker,attackerPos,10f));
        yield return null;
    }

    public void AttackEvent(int attackerId, int defenderSlot)
    {
        //print("slot id :" + attackerId);
        //print("defenderslot :" + defenderSlot);
        GameObject[] playercards = GameObject.FindGameObjectsWithTag("PlayerCard");
        GameObject[] enemycards = GameObject.FindGameObjectsWithTag("EnemyCard");
        events = Game.AttackUnit(attackerId,defenderSlot);
        DisplayEnemyAttack(defenderSlot, attackerId);
        //Debug.Log(events);
        foreach (GameEvent e in events)
        {
            //Debug.Log(e.eventType);
            if(e.eventType==EventType.UnitStatChanged)
            {
                //Debug.Log("Changing Stats");
                int idToCheck = (int)e.data[0];
                foreach(GameObject go in playercards)
                {
                    //Debug.Log("Selected Stats to Change");
                    var card = go.GetComponent<CardObject>();
                    if(idToCheck == card.GetUnitId())
                    {
                        /*Debug.Log(e.data[3]);
                        Debug.Log(e.data[4]);*/
                        card.UpdateStats((int)(e.data[3])+(int)(e.data[1]), (int)(e.data[3])+(int)(e.data[2]));
                    }
                }
                foreach(GameObject go in enemycards)
                {
                    //Debug.Log("Selected Stats to Change");
                    var card = go.GetComponent<CardObject>();
                    if(idToCheck == card.GetUnitId())
                    {
                        /*Debug.Log(e.data[3]);
                        Debug.Log(e.data[4]);*/
                        card.UpdateStats((int)(e.data[3])+(int)(e.data[1]), (int)(e.data[4])+(int)(e.data[2]));
                    }
                }
            }
            if(e.eventType == EventType.UnitDied)
            {
                int idToCheck = (int)e.data[0];
                foreach(GameObject go in playercards)
                {
                    var card = go.GetComponent<CardObject>();
                    if(idToCheck == card.GetUnitId())
                    {
                        Destroy(card);
                    }
                }
                foreach(GameObject go in enemycards)
                {
                    //Debug.Log("Selected Stats to Change");
                    var card = go.GetComponent<CardObject>();
                    if(idToCheck == card.GetUnitId())
                    {
                        Destroy(card);
                    }
                }
            }
            if(e.eventType == EventType.EncounterEnded)
            {
                Debug.Log("joever");
            }
        }
        events = Game.EndPhase();
        foreach (GameEvent e in events)
        {
            //Debug.Log(e.eventType);
            if(e.eventType==EventType.UnitStatChanged)
            {
                //Debug.Log("Changing Stats");
                int idToCheck = (int)e.data[0];
                foreach(GameObject go in playercards)
                {
                    //Debug.Log("Selected Stats to Change");
                    var card = go.GetComponent<CardObject>();
                    if(idToCheck == card.GetUnitId())
                    {
                        /*Debug.Log(e.data[3]);
                        Debug.Log(e.data[4]);*/
                        card.UpdateStats((int)(e.data[3])+(int)(e.data[1]), (int)(e.data[3])+(int)(e.data[2]));
                    }
                }
                foreach(GameObject go in enemycards)
                {
                    //Debug.Log("Selected Stats to Change");
                    var card = go.GetComponent<CardObject>();
                    if(idToCheck == card.GetUnitId())
                    {
                        /*Debug.Log(e.data[3]);
                        Debug.Log(e.data[4]);*/
                        card.UpdateStats((int)(e.data[3])+(int)(e.data[1]), (int)(e.data[4])+(int)(e.data[2]));
                    }
                }
            }
            if(e.eventType == EventType.UnitDied)
            {
                int idToCheck = (int)e.data[0];
                foreach(GameObject go in playercards)
                {
                    var card = go.GetComponent<CardObject>();
                    if(idToCheck == card.GetUnitId())
                    {
                        //Print("Should kill.")
                        Destroy(card);
                    }
                }
                foreach(GameObject go in enemycards)
                {
                    //Debug.Log("Selected Stats to Change");
                    var card = go.GetComponent<CardObject>();
                    if(idToCheck == card.GetUnitId())
                    {
                        Destroy(card);
                    }
                }
            }
            if(e.eventType == EventType.EncounterEnded)
            {
                Debug.Log("joever");
            }
        }
    }

    public void updateTurnText(){
        TurnText.text = "Turn: " + (turnNum);
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
                if(IsTouched(touch, _coll) && card.tag == "EnemyCard")
                {
                    cardId = card.GetComponent<CardObject>().GetUnitId();
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
            if(enemyCardObjs[i]!=null){
                card = enemyCardObjs[i];
                _coll = card.GetComponent<BoxCollider>();
                if (IsTouched(mousePos, _coll) && card.tag == "EnemyCard")
                {
                    cardId = card.GetComponent<CardObject>().GetUnitId();
                    break;
                }
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
        //print(enemies.Count);
        //print(enemyslots.Length);
        foreach (Card enemy in enemies)
        {
            //print("enemy "+ enemySlotId);
            currentCard = Instantiate(enemyPrefab, new Vector3(enemyslots[enemySlotId].transform.position.x, enemyslots[enemySlotId].transform.position.y, enemyslots[enemySlotId].transform.position.z), Quaternion.Euler(0,0,180)); 
            //Debug.Log(enemy.id);
            //Debug.Log(currentCard);
            currentCard.GetComponent<CardObject>().SetUnitId(enemy.id);
            //print("enemy cardobj id: "+currentCard.GetComponent<CardObject>().unitId);
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
            print("cardobj id: "+c.id);
            currentCard.tag = "PlayerCard";
            currentCard.GetComponent<CardObject>().SetInHand(true);
            //Debug.Log(c.unitType);
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

    public IEnumerator PhaseTextChange(string PhaseName)
    {
        transitioning = true;
        PhaseChanged = true;
        PhaseText.text = PhaseName;
        yield return new WaitForSeconds(0);
        //PhaseChanged = false;
        transitioning = false;
    }

    public IEnumerator GiveWarning(string warning, float WaitTime)
    {
        WarningText.text = warning;
        yield return new WaitForSeconds(WaitTime);
        WarningText.text = "";

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
