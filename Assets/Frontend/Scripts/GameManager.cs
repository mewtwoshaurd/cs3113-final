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

    public TMPro.TextMeshProUGUI PhaseButton;
    public TMPro.TextMeshProUGUI PhaseText;

    public GameObject[] enemyslots;

    public GameObject cardPrefab;

    public int phaseNum;

    public Button PhaseButtonUI;

    private bool PhaseChanged = false;

    public int turnNum = 1;
    

    List<Card> hand = new List<Card>();
    List<GameEvent> events = new List<GameEvent>();
    // Start is called before the first frame update
    void Start()
    {	   
        for(int i = 0; i < 20; i++)
        {
            deck.Add(Card.UnitCard(UnitType.Dog));
        }
        events = Game.StartEncounter(deck, UnitType.Dog);
        foreach(GameEvent e in events)
        {
            if (e.eventType == EventType.EncounterStarted)
            {
                enemies = (List<Card>)e.data[0];
                //Debug.Log(enemies);
            }
            if (e.eventType == EventType.HandGiven)
            {
                hand = (List<Card>)e.data[0];
                //Debug.Log(hand);
            }
            print(e);
        }

        GenerateHand(hand);
        GenerateEnemies(enemies);
        for(int i =0; i<hand.Count;i++){
            int id = hand[i].id;
            Game.PlayUnit(id);
        }

        events = Game.EndPhase();
        foreach(GameEvent e in events){
            print(e);
        }

        events = Game.EndPhase();
        foreach(GameEvent e in events){
            print(e);
        }

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
        if(phaseNum==0 && !PhaseChanged){
            PhaseButton.text = "NEXT";
            StartCoroutine(PhaseTextChange("TURN "+turnNum,4f,"CARD PHASE",4f));
            
        }
        if(phaseNum==1 && !PhaseChanged){
            PhaseButton.text = "END";
            StartCoroutine(PhaseTextChange("ATTACK PHASE",4f));
        }
        if(phaseNum==2 && !PhaseChanged){
            PhaseButton.text = "NEXT";
            StartCoroutine(PhaseTextChange("ENEMY PHASE",4f));
            foreach (Card enemy in enemies){
                Game.AttackUnit(enemy.id,hand[0].id);   
                turnNum++; 
            }
        }

    }

    public void IncrementPhase(){
        try{
            events = Game.EndPhase();
            if(phaseNum<2){
                phaseNum++;
            }
            else{
                phaseNum=0;
            }
            PhaseChanged = !PhaseChanged;
        }
        catch(Exception e){
            PhaseText.text = "Play Card to Continue";
        }
		
	}

    public bool hasPhaseChanged(){
        return PhaseChanged;
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

    // public void UpdateGameSlot(int slotid)
    // {
    //     events = Game.PlayUnit(slotid);
    // }


    void GenerateEnemies(List<Card> enemies){
        int enemySlotId = 0;
        GameObject currentCard;
        foreach(Card enemy in enemies){
            //print("enemy "+ enemySlotId);
            currentCard = Instantiate(cardPrefab,new Vector3(enemyDeck.transform.position.x,enemyDeck.transform.position.y,enemyDeck.transform.position.z), Quaternion.identity);
            StartCoroutine(Lerp(currentCard,new Vector3(enemyslots[enemySlotId].transform.position.x,enemyslots[enemySlotId].transform.position.y,enemyslots[enemySlotId].transform.position.z - .01f),1f));
            enemySlotId++;
        }
    }
    void GenerateHand(List<Card> hand)
    {
        int handslotid = 0;
        GameObject currentCard;
        foreach (Card c in hand)
        {
            currentCard = Instantiate(cardPrefab, new Vector3(handslots[handslotid].transform.position.x, handslots[handslotid].transform.position.y, handslots[handslotid].transform.position.z), Quaternion.identity);
            currentCard.GetComponent<CardObject>().SetUnitId(c.id);
            currentCard.tag = "PlayerCard";
            currentCard.GetComponent<CardObject>().SetInHand(true);
            StartCoroutine(Lerp(currentCard,new Vector3(handslots[handslotid].transform.position.x,handslots[handslotid].transform.position.y,handslots[handslotid].transform.position.z - .01f),1f));
            handslotid += 1;
        }
    }

    void TakeHand()
    {
        GameObject[] inHand = GameObject.FindGameObjectsWithTag("PlayerCard");
        foreach (GameObject c in inHand)
        {
            if(c.GetComponent<CardObject>().GetInHand())
            {
                Destroy(c);
            }
        }
    }
    
    IEnumerator PhaseTextChange(string PhaseName, float FirstWait, String NextText = "",float SecondWait = 0f)
    {
        PhaseChanged=true;
        PhaseText.text = PhaseName;
        yield return new WaitForSeconds(FirstWait);
        PhaseText.text = NextText;
        yield return new WaitForSeconds(SecondWait);
        PhaseText.text = "";
        //PhaseChanged = false;
    }

    IEnumerator Lerp(GameObject obj,Vector3 end,float duration)
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

}
