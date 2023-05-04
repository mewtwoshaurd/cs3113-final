using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    List<Card> deck = new List<Card>();

    List<Card> enemies = new List<Card>();

    //public Transform enemyDeck;

    public Transform playerDeck;
    public GameObject[] playerslots;

    public GameObject[] handslots;

    public GameObject enemyPrefab;
    public GameObject damagePrefab;

    public GameObject selectedItem = null;

    public TMPro.TextMeshPro PhaseButton;
    public TMPro.TextMeshProUGUI PhaseText;

    public TMPro.TextMeshProUGUI TurnText;

    public float animationTime = 3.0f;
    public TMPro.TextMeshProUGUI WarningText;

    public TMPro.TextMeshProUGUI WinLoseText;

    public GameObject[] enemyslots;

    public GameObject[] enemyCardObjs = new GameObject[5];

    int[] playableSlots = new int[5];
    public GameObject cardPrefab;

    public int phaseNum;

    public bool transitioning = false;
    private bool PhaseChanged = false;

    public int turnNum = 1;

    public TMPro.TextMeshProUGUI AbilityActivated;

    public TMPro.TextMeshProUGUI ItemActivated;
    List<Card> hand = new List<Card>();
    List<GameEvent> events = new List<GameEvent>();

    public bool attackedThisTurn = false;

    public UnitType[] currImplementedUnits = new UnitType[] { UnitType.Dog, UnitType.Bee, UnitType.Bat, UnitType.Spider };
    public ItemType[] currImplementedItems = new ItemType[] { ItemType.SmokeBomb, ItemType.Apple};

    public SoundEmitter soundEmitter;

    void Start()
    {   
        AbilityActivated.text = "";
        ItemActivated.text = "";
        for (int i = 0; i < 5; i++)
        {
            playableSlots[i] = 0;
        }
        UnitType randtypeU;
        for (int i = 0; i < 10; i++)
        {
            int randu = UnityEngine.Random.Range(0, 4);
            randtypeU = currImplementedUnits[randu];
            deck.Add(Card.UnitCard(randtypeU));
        }
        //ItemType randtypeI;
        for (int i = 0; i < 10; i++)
        {
            /*int randi = UnityEngine.Random.Range(0, 1);
            Debug.Log(randi);
            randtypeI = currImplementedItems[randi];*/
            deck.Add(Card.ItemCard(ItemType.Apple));
        }
        events = Game.StartEncounter(deck, UnitType.Dog);
        foreach (GameEvent e in events)
        {
            if (e.eventType == EventType.EncounterStarted)
            {
                enemies = (List<Card>)e.data[0];
                //print("enemies Count " + enemies.Count);
                foreach (Card en in enemies)
                {
                    //print("Enemy id " + en.id);
                }
                //Debug.Log(enemies);
            }
            if (e.eventType == EventType.HandGiven)
            {
                hand = (List<Card>)e.data[0];
                foreach (Card card in hand)
                {
                    Debug.Log(card.cardType);
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

    void Update()
    {
        print("phaseChanged: " + PhaseChanged);
        print("phase: " + phaseNum);
        updateTurnText();
        if (phaseNum == 0 && !PhaseChanged && !transitioning)
        {
            PhaseButton.text = "NEXT";
            StartCoroutine(PhaseTextChange("CARD PHASE"));
            //attackedThisTurn = false;
            print("phaseNum: " + phaseNum);
        }
        if (phaseNum == 1 && !PhaseChanged && !transitioning)
        {
            PhaseButton.text = "END";
            StartCoroutine(PhaseTextChange("ATTACK PHASE"));
            print("phaseNum: " + phaseNum);
        }
        if (phaseNum == 2 && !PhaseChanged && !transitioning)
        {
            PhaseButton.text = "NEXT";
            StartCoroutine(PhaseTextChange("ENEMY PHASE"));
            print("phaseNum: " + phaseNum);
        }

    }

    public void IncrementPhase()
    {
        soundEmitter.PlayButtonSound();
        events = Game.EndPhase();
        foreach (GameEvent e in events)
        {
            if (e.eventType == EventType.Error)
            {
                GiveWarning("You Must Play a Card to Move On", 2f);
            }
        }
        if (phaseNum < 2)
        {
            phaseNum++;
        }
        if (phaseNum == 2)
        {
            //Debug.Log("Is this thing on?");
            turnNum++;
            EnemyAttack(events);
            PhaseChanged=true;
            PhaseChanged=true;
            phaseNum=0;
        }
        if (phaseNum == 0)
        {
            TakeHand();
            //Debug.Log("We're so back.");
            GenerateHand(hand);
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

    public bool IsTouched(Vector3 mousePos, Collider collider)
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

    public void EnemyAttack(List<GameEvent> events)
    {
        GameObject[] playercards = GameObject.FindGameObjectsWithTag("PlayerCard");
        GameObject[] enemycards = GameObject.FindGameObjectsWithTag("EnemyCard");
        int attackerId;
        int defenderId;
        foreach (GameEvent e in events)
        {
            //Debug.Log(e.eventType);
            if (e.eventType == EventType.UnitAttacked)
            {
                attackerId = (int)e.data[0];
                defenderId = (int)e.data[1];
                DisplayEnemyAttack(attackerId, defenderId);
            }
            if (e.eventType == EventType.UnitStatChanged)
            {
                int idToCheck = (int)e.data[0];
                foreach (GameObject go in playercards)
                {
                    var card = go.GetComponent<CardObject>();
                    if (idToCheck == card.GetUnitId())
                    {
                        card.UpdateStats((int)(e.data[3]), (int)(e.data[4]));
                        break;
                    }
                }
                foreach (GameObject go in enemycards)
                {
                    //Debug.Log("Selected Stats to Change");
                    var card = go.GetComponent<CardObject>();
                    //Debug.Log("You asshole.... " + go.GetInstanceID());
                    if (idToCheck == card.GetUnitId())
                    {
                        card.UpdateStats((int)(e.data[3]), (int)(e.data[4]));
                        break;
                    }
                }
            }
            if (e.eventType == EventType.UnitDied)
            {
                int idToCheck = (int)e.data[0];
                foreach (GameObject go in playercards)
                {
                    var card = go.GetComponent<CardObject>();
                    if (idToCheck == card.GetUnitId())
                    {
                        int handSlotToRemove = card.GetUnitSlot();
                        //Debug.Log("Play slot please... " + handSlotToRemove.ToString());
                        UpdateGameSlot(handSlotToRemove, false);
                        Destroy(go, animationTime);
                        soundEmitter.PlaySoundWithDelay(GameSound.Death, animationTime);
                    }
                }
                foreach (GameObject go in enemycards)
                {
                    var card = go.GetComponent<CardObject>();
                    if (idToCheck == card.GetUnitId())
                    {
                        Destroy(go, animationTime);
                        soundEmitter.PlaySoundWithDelay(GameSound.Death, animationTime);
                    }
                }
            }
            if (e.eventType == EventType.UnitAbilityActivation)
            {
                Debug.Log("Did anything even happen?");
                int idToCheck = (int)e.data[0];
                UnitType unitAbility = UnitType.NotApplicable;
                string abilityName = null;
                foreach (GameObject go in playercards)
                {
                    var card = go.GetComponent<CardObject>();
                    if (idToCheck == card.GetUnitId())
                    {
                        Debug.Log("found!");
                        unitAbility = card.GetUnitType();
                        Debug.Log("found: " + unitAbility.ToString());
                        break;
                    }
                }
                foreach (GameObject go in enemycards)
                {
                    var card = go.GetComponent<CardObject>();
                    if (idToCheck == card.GetUnitId())
                    {
                       unitAbility = card.GetUnitType();
                       break;
                    }
                }
                if(unitAbility == UnitType.NotApplicable)
                {
                    Debug.Log("Something went wrong...");
                    continue;
                }
                else
                {
                    Debug.Log("Found ability!");
                    abilityName = ToAbility(CardDicts.unitAbilityDict[unitAbility]);
                }
                
                if(abilityName == null)
                {
                    continue;
                }
                else
                {
                    Debug.Log("Where's the text!");
                    StartCoroutine(AbilityActivates(abilityName));
                }
            }
            if (e.eventType == EventType.EncounterEnded)
            {
                print((bool)e.data[0]);
                if((bool)e.data[0]){
                    print("won");
                    WinLoseText.text = "YOU WIN";
                    StartCoroutine(DisplayEndEncounter(true));
                }
                if(!((bool)e.data[0])){
                    print("lose");
                    WinLoseText.text = "YOU LOSE";
                    StartCoroutine(DisplayEndEncounter(false));
                }

                //StartCoroutine(DisplayEndOfEncounter((bool)e.data[0]));
            }
            if (e.eventType == EventType.Error)
            {
                Debug.Log("Everything sucks and I am sad");
            }
            if (e.eventType == EventType.HandGiven)
            {
                hand = (List<Card>)e.data[0];
            }
        }
        IncrementPhase();
    }

    

    public void attachItemToCard(GameObject card){
        print("should lerp");
        if(selectedItem.GetComponent<CardObject>().parentCard==null &&card.GetComponent<CardObject>().item==null){
            StartCoroutine(Lerp(selectedItem,new Vector3(card.transform.position.x,card.transform.position.y-2f,card.transform.position.z-.02f),1f));
            Game.AttachItem(card.GetComponent<CardObject>().GetUnitId(), selectedItem.GetComponent<CardObject>().GetUnitId());
            selectedItem.GetComponent<CardObject>().parentCard = card;
            card.GetComponent<CardObject>().item = selectedItem;
        }
        
    }
    IEnumerator DisplayEndEncounter(bool won){
        if(won){
            WinLoseText.color = new Color(0f,1f,0f,1f);
            WinLoseText.text = "YOU WON";
            yield return new WaitForSeconds(2f);
            SceneManager.LoadScene(2);    
        }
        else{
            WinLoseText.color = new Color(1f,0f,0f,1f);
            WinLoseText.text = "YOU LOSE";
            yield return new WaitForSeconds(2f);
            SceneManager.LoadScene(1);
        }
    }

    public void DisplayEnemyAttack(int attackerId, int defenderId)
    {
        GameObject[] playercards = GameObject.FindGameObjectsWithTag("PlayerCard");
        GameObject[] enemycards = GameObject.FindGameObjectsWithTag("EnemyCard");
        Vector3 attackerPos = new Vector3(0, 0, 0);
        Vector3 defenderPos = new Vector3(0, 0, 0);
        GameObject attacker = null;
        GameObject defender = null;
        foreach (GameObject card in playercards)
        {
            if (card.GetComponent<CardObject>().GetUnitId() == defenderId)
            {
                defender = card;
                defenderPos = card.transform.position;
                print("card found 1");
                break;
            }
        }
        foreach (GameObject card in enemycards)
        {
            if (card.GetComponent<CardObject>().GetUnitId() == attackerId)
            {
                attacker = card;
                print("card found 2");
                break;
            }
        }
        if (attacker != null && defender != null)
        {
            print("both found");
            print(attacker.transform.position);
            attackerPos = attacker.transform.position;
            StartCoroutine(attackAnimation(attacker, attackerPos, defender));
        }

    }

    public void DisplayPlayerAttack(int attackerId, int defenderId)
    {
        GameObject[] playercards = GameObject.FindGameObjectsWithTag("PlayerCard");
        GameObject[] enemycards = GameObject.FindGameObjectsWithTag("EnemyCard");
        Vector3 attackerPos = new Vector3(0, 0, 0);
        Vector3 defenderPos = new Vector3(0, 0, 0);
        GameObject attacker = null;
        GameObject defender = null;
        foreach (GameObject card in enemycards)
        {
            if (card.GetComponent<CardObject>().GetUnitId() == defenderId)
            {
                defender = card;
                defenderPos = card.transform.position;
                print("card found 1");
                break;
            }
        }
        foreach (GameObject card in playercards)
        {
            print(card);
            if (card != null)
            {
                if (card.GetComponent<CardObject>().GetUnitId() == attackerId)
                {
                    attacker = card;
                    print("card found 2");
                    break;
                }
            }
        }
        if (attacker != null && defender != null)
        {
            print("both found");
            print(attacker.transform.position);
            attackerPos = attacker.transform.position;
            StartCoroutine(attackAnimation(attacker, attackerPos, defender));
        }

    }

    IEnumerator attackAnimation(GameObject attacker, Vector3 attackerPos, GameObject defender)
    {
        print("running animation");
        StartCoroutine(Lerp(attacker, new Vector3(attackerPos.x, attackerPos.y, attackerPos.z - 1f), 1f));
        yield return new WaitForSeconds(1f);
        StartCoroutine(Lerp(attacker, new Vector3(defender.transform.position.x, defender.transform.position.y + 2f, defender.transform.position.z - .01f), .25f));
        Instantiate(damagePrefab, (defender.transform.position), Quaternion.Euler(0, 0, 0));
        soundEmitter.PlayAttackSound();
        yield return new WaitForSeconds(.25f);
        StartCoroutine(Lerp(attacker, attackerPos, .5f));
        yield return null;
    }

    //IEnumerator attachAnimation(GameObject )

    public void PlayerAttackEvent(int attackerId, int defenderId)
    {
        //print("slot id :" + attackerId);
        //print("defenderId :" + defenderId);
        /*if(attackedThisTurn)
        {
            Debug.Log("Avoiding this error wooo!");
            return;
        }*/
        //attackedThisTurn = true;
        bool attackSuccessful = true;
        GameObject[] playercards = GameObject.FindGameObjectsWithTag("PlayerCard");
        GameObject[] enemycards = GameObject.FindGameObjectsWithTag("EnemyCard");
        events = Game.AttackUnit(attackerId, defenderId);
        foreach (GameEvent e in events)
        {
            Debug.Log(e.eventType);
            if (e.eventType == EventType.UnitStatChanged)
            {
                int idToCheck = (int)e.data[0];
                foreach (GameObject go in playercards)
                {
                    var card = go.GetComponent<CardObject>();
                    if (idToCheck == card.GetUnitId())
                    {
                        card.UpdateStats((int)(e.data[3]), (int)(e.data[4]));
                        break;
                    }
                }
                foreach (GameObject go in enemycards)
                {
                    //Debug.Log("Selected Stats to Change");
                    var card = go.GetComponent<CardObject>();
                    //Debug.Log("You asshole.... " + go.GetInstanceID());
                    if (idToCheck == card.GetUnitId())
                    {
                        //Debug.Log("Health before: " + (e.data[3]));
                        //Debug.Log("Health delta: " + (e.data[1]));
                        //Debug.Log("Attack before: " + e.data[4]);
                        //Debug.Log("Attack delta: " + e.data[2]);
                        card.UpdateStats((int)(e.data[3]), (int)(e.data[4]));
                        break;
                    }
                }
            }
            if (e.eventType == EventType.UnitDied)
            {
                int idToCheck = (int)e.data[0];
                foreach (GameObject go in playercards)
                {
                    var card = go.GetComponent<CardObject>();
                    if (idToCheck == card.GetUnitId())
                    {
                        int handSlotToRemove = card.GetUnitSlot();
                        Destroy(go, animationTime);
                        UpdateGameSlot(handSlotToRemove, false);
                        soundEmitter.PlaySoundWithDelay(GameSound.Death, animationTime);
                        break;
                    }
                }
                foreach (GameObject go in enemycards)
                {
                    var card = go.GetComponent<CardObject>();
                    if (idToCheck == card.GetUnitId())
                    {
                        Destroy(go, animationTime);
                        break;
                    }
                }
            }
            if (e.eventType == EventType.UnitAbilityActivation)
            {
                Debug.Log("Did anything even happen?");
                int idToCheck = (int)e.data[0];
                UnitType unitAbility = UnitType.NotApplicable;
                string abilityName = null;
                foreach (GameObject go in playercards)
                {
                    var card = go.GetComponent<CardObject>();
                    if (idToCheck == card.GetUnitId())
                    {
                        unitAbility = card.GetUnitType();
                        break;
                    }
                }
                foreach (GameObject go in enemycards)
                {
                    var card = go.GetComponent<CardObject>();
                    if (idToCheck == card.GetUnitId())
                    {
                       unitAbility = card.GetUnitType();
                       break;
                    }
                }
                if(unitAbility == UnitType.NotApplicable)
                {
                    continue;
                }
                else
                {
                    Debug.Log("Found ability!");
                    abilityName = ToAbility(CardDicts.unitAbilityDict[unitAbility]);
                }
                
                if(abilityName == null)
                {
                    continue;
                }
                else
                {
                    Debug.Log("Where's the text!");
                    StartCoroutine(AbilityActivates(abilityName));
                }
            }
            if (e.eventType == EventType.EncounterEnded)
            {
                print((bool)e.data[0]);
                if((bool)e.data[0]){
                    print("won");
                    StartCoroutine(DisplayEndEncounter(true));
                }
                if(!((bool)e.data[0])){
                    print("lose");
                    WinLoseText.text = "YOU LOSE";
                    //SceneManager.LoadScene(1);
                    StartCoroutine(DisplayEndEncounter(false));
                }
                //StartCoroutine(DisplayEndOfEncounter((bool)e.data[0]));
            }
            if (e.eventType == EventType.Error)
            {
                attackSuccessful = false;
                Debug.Log("Everything sucks and I am sad");
            }
        }
        if (attackSuccessful)
        {
            DisplayPlayerAttack(attackerId, defenderId);
        }

        //events = Game.EndPhase();
    }

    public void updateTurnText()
    {
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
            if (enemyCardObjs[i] != null)
            {
                card = enemyCardObjs[i];
                _coll = card.GetComponent<BoxCollider>();
                if (IsTouched(touch, _coll) && card.tag == "EnemyCard")
                {
                    cardId = card.GetComponent<CardObject>().GetUnitId();
                    break;
                }
            }
        }
        return cardId;
    }

    public void IsTouchingPlayerCard(Touch touch, out int cardId, out Vector3 cardLoc)
    {
        BoxCollider _coll;
        cardId = -1;
        cardLoc = new Vector3();
        GameObject[] playercards = GameObject.FindGameObjectsWithTag("PlayerCard");
        foreach (GameObject go in playercards)
        {
            var card = go.GetComponent<CardObject>();
            if (card.IsPlayed())
            {
                _coll = go.GetComponent<BoxCollider>();
                if (IsTouched(touch, _coll))
                {
                    cardId = card.GetUnitId();
                    cardLoc = go.GetComponent<Rigidbody>().transform.position;
                    break;
                }
            }
        }
    }


    public int IsTouchingEnemyCard(Vector3 mousePos)
    {
        BoxCollider _coll;
        GameObject card;
        int cardId = -1;
        for (int i = 0; i < enemyCardObjs.Length; i++)
        {
            if (enemyCardObjs[i] != null)
            {
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
            currentCard = Instantiate(enemyPrefab, new Vector3(enemyslots[enemySlotId].transform.position.x, enemyslots[enemySlotId].transform.position.y, enemyslots[enemySlotId].transform.position.z), Quaternion.Euler(0, 0, 0));
            //Debug.Log(enemy.id);
            //Debug.Log(currentCard);
            currentCard.GetComponent<CardObject>().SetUnitId(enemy.id);
            //print("enemy cardobj id: "+currentCard.GetComponent<CardObject>().unitId);
            currentCard.tag = "EnemyCard";
            currentCard.GetComponent<CardObject>().SetType(enemy.unitType, enemy.itemType);
            enemyCardObjs[enemySlotId] = currentCard;
            StartCoroutine(Lerp(currentCard, new Vector3(enemyslots[enemySlotId].transform.position.x, enemyslots[enemySlotId].transform.position.y, enemyslots[enemySlotId].transform.position.z - .1f), 1f));
            enemySlotId++;
        }
    }

    void GenerateHand(List<Card> hand)
    {
        int handslotid = 0;
        GameObject currentCard;
        soundEmitter.PlayCardSound();
        foreach (Card c in hand)
        {
            currentCard = Instantiate(cardPrefab, new Vector3(playerDeck.position.x, playerDeck.position.y, playerDeck.position.z), Quaternion.Euler(0, 0, 0));
            currentCard.tag = "PlayerCard";
            currentCard.GetComponent<CardObject>().SetUnitId(c.id);
            currentCard.GetComponent<CardObject>().SetInHand(true);
            Debug.Log("In Generate Hand: " + c.unitType + "," + c.itemType);
            currentCard.GetComponent<CardObject>().SetType(c.unitType, c.itemType);
            StartCoroutine(Lerp(currentCard, new Vector3(handslots[handslotid].transform.position.x, handslots[handslotid].transform.position.y, handslots[handslotid].transform.position.z - .01f), 1f));
            handslotid += 1;
        }
    }

    void TakeHand()
    {
        GameObject[] inHand = GameObject.FindGameObjectsWithTag("PlayerCard");
        soundEmitter.PlayCardSound();
        foreach (GameObject c in inHand)
        {
            if (c.GetComponent<CardObject>().GetInHand())
            {
                Instantiate(damagePrefab);
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
        print("lerping" + obj + " " + end);
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

    public string ToAbility(AbilityType abilityT)
    {
        if(abilityT == AbilityType.None)
        {
            return "None";
        }
        else if(abilityT == AbilityType.Bloodsucker)
        {
            return "Bloodsucker";
        }
        else if(abilityT == AbilityType.Lazy)
        {
            return "Lazy";
        }
        else if(abilityT == AbilityType.Swarm)
        {
            return "Swarm";
        }
        else if(abilityT == AbilityType.Spikey)
        {
            return "Spikey";
        }
        else if(abilityT == AbilityType.Wild)
        {
            return "Wild";
        }
        else if(abilityT == AbilityType.Curse)
        {
            return "Curse";
        }
        else if(abilityT == AbilityType.Intimidate)
        {
            return "Intimidate";
        }
        else
        {
            return null;
        }
    }

    public IEnumerator AbilityActivates(string ability)
    {
        soundEmitter.PlayAbilitySound();
        AbilityActivated.text = "ABILITY: ";
        AbilityActivated.text += ability;
        yield return new WaitForSeconds(2.0f);
        AbilityActivated.text = "";
    }
    public IEnumerator ItemActivates(string item)
    {
        soundEmitter.PlayAbilitySound();
        ItemActivated.text = "ITEM: ";
        ItemActivated.text += item;
        yield return new WaitForSeconds(2.0f);
        ItemActivated.text = "";
    }

}
