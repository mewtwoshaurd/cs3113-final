using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    List<Card> deck = new List<Card>();

    List<Card> enemies = new List<Card>();

    public GameObject[] playerslots;

    public GameObject[] handslots;

    public TMPro.TextMeshProUGUI PhaseButton;
    public TMPro.TextMeshProUGUI PhaseText;

    public GameObject[] enemyslots;

    public GameObject cardPrefab;

    public int phaseNum;

    public Button PhaseButtonUI;

    private bool PhaseChanged = false;
    

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

        

        
        Game.EndPhase();

    }

    // Update is called once per frame
    void Update()
    {
        Button btn = PhaseButtonUI.GetComponent<Button>();
        btn.onClick.AddListener(IncrementPhase);
        
        if(phaseNum==0 && !PhaseChanged){
            PhaseButton.text = "NEXT";
            StartCoroutine(PhaseTextChange("CARD PHASE",2.5f));
            print("phase text change complete");
            PhaseChanged=true;
        }
        if(phaseNum==1 && !PhaseChanged){
            PhaseButton.text = "END";
            StartCoroutine(PhaseTextChange("ATTACK PHASE",2.5f));
            PhaseChanged=true;
        }
        if(phaseNum==2 && !PhaseChanged){
            PhaseButton.text = "NEXT";
            StartCoroutine(PhaseTextChange("ENEMY CARD PHASE",2.5f));
            PhaseChanged=true;
        }
        if(phaseNum==3 && !PhaseChanged){
            PhaseButton.text = "NEXT";
            StartCoroutine(PhaseTextChange("ENEMY ATTACK PHASE",2.5f));
            StartCoroutine(WaitXSeconds(1));
            PhaseChanged=true;

        }
    }

    void IncrementPhase(){
		if(phaseNum<3){
            phaseNum++;
        }
        else{
            phaseNum=0;
        }
        PhaseChanged = false;
	}

    public bool IsTouched(Touch touch, BoxCollider collider)
    {
        Ray _ray = Camera.main.ScreenPointToRay(new Vector3(touch.position.x, touch.position.y, 0));
        Debug.Log(_ray);
        RaycastHit _hit;
        print(_hit);

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
    
    IEnumerator PhaseTextChange(string PhaseName, float seconds)
    {
        PhaseText.text = PhaseName;
        yield return new WaitForSeconds(seconds);
        PhaseText.text = "";
    }
    IEnumerator WaitXSeconds(float x){
        yield return new WaitForSeconds(x);
    }


}
