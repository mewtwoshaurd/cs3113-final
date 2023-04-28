using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardObject : MonoBehaviour
{
    // Start is called before the first frame update
    Rigidbody _rigidbody;
    Camera cam;
    public LayerMask targetLayer;

    BoxCollider _coll;

    Renderer _mr;
    //public float speed = 1;
    //public float maxSpeed = 10;
    public float cardOffset = -0.1f;

    public Material[] mats;

    GameManager _gm;

    public int unitId;

    bool isSelected = false;

    bool isPlayed = false;

    int slotid = -1;
    int enemyCardSlot = -1;

    bool inHand = false;

    int enemyCardId = 0;

    UnitType type;

    public int health;
    public TMPro.TextMeshProUGUI _health;

    public TMPro.TextMeshProUGUI _attack;

    List<GameEvent> events = new List<GameEvent>();
    public TMPro.TextMeshProUGUI _name;

    public TMPro.TextMeshProUGUI _ability;

    public int attacksPerTurn = 4;

    public static bool attacking = false;

    int phaseNum = 0;
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _coll = GetComponent<BoxCollider>();
        _gm = (GameManager)FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        bool isTouching = false;
        Vector3 touchPos = Vector3.zero;
#if !UNITY_ANDROID || UNITY_EDITOR
        if (Input.GetMouseButton(0))
        {
            touchPos = Input.mousePosition;
            isTouching = true;
        }
#else
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            touchPos = touch.position;
            isTouching = (touch.phase == TouchPhase.Began);
        }
#endif


        slotid = _gm.IsTouchingPlayerSlot(touchPos);
        phaseNum = _gm.currentPhase();
        switch (phaseNum)
        {
            case 0:
                slotid = _gm.IsTouchingPlayerSlot(touchPos);
                if (isTouching && (_gm.IsTouched(touchPos, _coll) && !isPlayed))
                {
                    //Debug.Log("selected!");
                    isSelected = true;
                }
                else if (isTouching && isSelected && (slotid >= 0) && !isPlayed && _gm.IsGameSlotOpen(slotid))
                {
                    //Debug.Log("play!");
                    Transform slotTrans = _gm.playerslots[slotid].transform;
                    Vector3 newPos = new Vector3(slotTrans.position.x, slotTrans.position.y, slotTrans.position.z + cardOffset);
                    transform.position = newPos;
                    isSelected = false;
                    isPlayed = true;
                    Game.PlayUnit(unitId);
                    _gm.UpdateGameSlot(slotid, isPlayed);
                }
                else if (isTouching && !(_gm.IsTouched(touchPos, _coll)) && !isPlayed)
                {
                    //Debug.Log("unselected!");
                    isSelected = false;
                }
                break;
            case 1:
                print("isSelected " + isSelected);
                slotid = _gm.IsTouchingPlayerSlot(touchPos);
                print("slotId " + slotid);

                /*if (attacksPerTurn == 0)
                {
                    _gm.PhaseTextChange("NO MORE ATTACKS", 1f);
                }*/
                if (isTouching && !(_gm.IsTouched(touchPos, _coll)) && isSelected && attacksPerTurn != 0)
                {
                    print("playerid " + unitId);
                    enemyCardId = _gm.IsTouchingEnemyCard(touchPos);
                    print("enemyID " + enemyCardId);
                    if (enemyCardId >= 0)
                    {
                        _gm.AttackEvent(unitId, enemyCardId);
                        //_gm.AttackResults(events);
                        print("attacked");
                        //attacksPerTurn--;
                        //print(attacksPerTurn);
                        attacking = false;
                    }
                    isSelected = false;
                }

                else if (isTouching && (_gm.IsTouched(touchPos, _coll)) && attacksPerTurn != 0 && !attacking && !isSelected)
                {
                    isSelected = true;
                    StartCoroutine(_gm.PhaseTextChange("pick an enemy to attack", 1f));
                    print("picking player");
                    attacking = true;
                }
                break;
            
            case 2:
                attacksPerTurn = 1;
                break;
        }
    }

    public void SetUnitId(int id)
    {
        unitId = id;
    }

    public int GetUnitId()
    {
        return unitId;
    }

    public void SetInHand(bool hand)
    {
        inHand = hand;
    }

    public bool GetInHand()
    {
        return inHand;
    }

    public void SetUnitType(UnitType newtype)
    {
        type = newtype;
        _mr = GetComponent<Renderer>();
        _health.text = CardDicts.unitHealthDict[newtype].ToString();
        health = CardDicts.unitHealthDict[newtype];
        _attack.text = CardDicts.unitDamageDict[newtype].ToString();
        var _mrcopy = _mr.materials;
        if (newtype == UnitType.Bat)
        {
            _mrcopy[0] = mats[0];
            _name.text = "Bat";

        }
        else if (newtype == UnitType.Bee)
        {
            _mrcopy[0] = mats[1];
            _name.text = "Bee";
        }
        else if (newtype == UnitType.Dog)
        {
            _mrcopy[0] = mats[2];
            _name.text = "Dog";
        }
        else if (newtype == UnitType.Spider)
        {
            _mrcopy[0] = mats[3];
            _name.text = "Spider";
        }
        _mr.materials = _mrcopy;
    }

    public void UpdateStats(int healthChange, int damageChange)
    {
        print("Doing Thing");
        _health.text = healthChange.ToString();
        _attack.text = damageChange.ToString();
    }
}
