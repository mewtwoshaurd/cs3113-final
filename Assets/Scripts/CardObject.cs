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
    public GameObject textureUtil;
    GameManager _gm;

    public GameObject parentCard = null;
    public int unitId;
    bool isSelected = false;

    Touch touch;
    bool isPlayed = false;

    //bool itemSelected = false;
    int slotid = -1;

    int permaSlotId = -1;
    //int enemyCardSlot = -1;
    bool inHand = false;

    bool isOfUnit = false;

    public GameObject item = null;
    int enemyCardId = 0;
    UnitType utype;

    ItemType itype;
    public int health;
    public TMPro.TextMeshProUGUI _health;
    public TMPro.TextMeshProUGUI _attack;

    public TMPro.TextMeshProUGUI _deltaHP;

    public TMPro.TextMeshProUGUI _deltaHM;

    public TMPro.TextMeshProUGUI _deltaAP;

    public TMPro.TextMeshProUGUI _deltaAM;
    List<GameEvent> events = new List<GameEvent>();
    public TMPro.TextMeshProUGUI _name;
    public TMPro.TextMeshProUGUI _ability;

    public TMP_FontAsset positive;

    public TMP_FontAsset negative;

    public static bool attacking = false;

    //bool isEmitting = false;
    
    //float waitTime = 0.5f;
    SoundEmitter soundEmitter;

    int phaseNum = 0;
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _coll = GetComponent<BoxCollider>();
        _gm = (GameManager)FindObjectOfType<GameManager>();
        soundEmitter = GameObject.FindGameObjectWithTag("SoundEmitter").GetComponent<SoundEmitter>();
        _deltaHP.text = "";
        _deltaHM.text = "";
        _deltaAP.text = "";
        _deltaAM.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        if(isSelected)
        {
            StartCoroutine(waitForHighlight());
        }
        else
        {
            _mr.material.DisableKeyword("_EMISSION");
        }
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
            touch = Input.GetTouch(0);
            touchPos = touch.position;
            isTouching = (touch.phase == TouchPhase.Began);
        }
#endif

        //slotid = _gm.IsTouchingPlayerSlot(touchPos);
        phaseNum = _gm.currentPhase();
        switch (phaseNum)
        {
            case 0:
                if ((tag == "PlayerCard")){
                    slotid = _gm.IsTouchingPlayerSlot(touchPos);
                    int playerCardId;
                    Vector3 playerCardLoc;
                    _gm.IsTouchingPlayerCard(touch, out playerCardId, out playerCardLoc);
                    if (isTouching && (_gm.IsTouched(touchPos, _coll) && !isPlayed))
                    {
                        //Debug.Log("selected!");
                        isSelected = true;
                    }
                    else if (isOfUnit && isTouching && isSelected && (slotid >= 0) && !isPlayed && _gm.IsGameSlotOpen(slotid))
                    {
                        //Debug.Log("play!");
                        Transform slotTrans = _gm.playerslots[slotid].transform;
                        Vector3 newPos = new Vector3(slotTrans.position.x, slotTrans.position.y, slotTrans.position.z + cardOffset);
                        transform.position = newPos;
                        isSelected = false;
                        isPlayed = true;
                        inHand = false;
                        Game.PlayUnit(unitId);
                        soundEmitter.PlayCardSound();
                        _gm.UpdateGameSlot(slotid, isPlayed);
                        permaSlotId = slotid;
                    }
                    else if (isTouching && !(_gm.IsTouched(touchPos, _coll)) && !isPlayed)
                    {
                        //Debug.Log("unselected!");
                        isSelected = false;
                    }
                    if (!isOfUnit && (_gm.IsTouched(touchPos, _coll)))
                    {
                        //Debug.Log("touching Item");
                        //transform.position = playerCardLoc;
                        isSelected = false;
                        isPlayed = true;
                        inHand = false;
                        _gm.selectedItem = gameObject;
                        soundEmitter.PlayCardSound();
                    }
                    if(isOfUnit && isPlayed &&(_gm.IsTouched(touchPos, _coll)) && item==null && (_gm.selectedItem!=null)){

                        _gm.attachItemToCard(gameObject);
                    }
                }
                
                break;
            case 1:
                if ((tag == "PlayerCard"))
                {
                    //slotid = _gm.IsTouchingPlayerSlot(touchPos);

                    if (isOfUnit && isTouching && !(_gm.IsTouched(touchPos, _coll)) && isSelected)
                    {
                        //print("selecting enemy");
                        
                        enemyCardId = _gm.IsTouchingEnemyCard(touchPos);
                        if (enemyCardId >= 0)
                        {
                            //Debug.Log(unitId);
                            //Debug.Log(enemyCardId);
                            _gm.PlayerAttackEvent(unitId, enemyCardId);
                            Color defaultColor = new Color(1f, 1f, 1f, 1f);
                            highlight(defaultColor);
                            //_gm.AttackResults(events);
                            //print("attacked");
                            //attacksPerTurn--;
                            //print(attacksPerTurn);
                            attacking = false;
                        }
                        isSelected = false;
                    }

                    else if (isOfUnit && isTouching && (_gm.IsTouched(touchPos, _coll)) && !attacking && !isSelected)
                    {
                        //Debug.Log("selecting player");
                        Color yellowHighlight = new Color(1f,1f,0f,1f);
                        highlight(yellowHighlight);
                        isSelected = true;
                        attacking = true;
                    }
                    break;
                }
                break;
            case 2:
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

    public void SetType(UnitType newtypeU, ItemType newtypeI)
    {
        _mr = GetComponent<Renderer>();
        Material mat;
        //Debug.Log(newtypeU);
        //Debug.Log(newtypeI);
        if(newtypeI == ItemType.NotApplicable)
        {
            _health.text = CardDicts.unitHealthDict[newtypeU].ToString();
            utype = newtypeU;
            health = CardDicts.unitHealthDict[newtypeU];
            _attack.text = CardDicts.unitDamageDict[newtypeU].ToString();
            mat = textureUtil.GetComponent<CardToTextureUtility>().getUnitMat(newtypeU);
            _mr.material = mat;
            isOfUnit = true;
        }
        else
        {
            _health.text = "";
            //health
            itype = newtypeI;
            _attack.text = "";
            mat = textureUtil.GetComponent<CardToTextureUtility>().getItemMat(newtypeI);
            _mr.material = mat;
        }
        /*var _mrcopy = _mr.materials;
        if (newtype == UnitType.Bat)
        {
            _mrcopy[0] = mats[0];
            //_name.text = "Bat";

        }
        else if (newtype == UnitType.Bee)
        {
            _mrcopy[0] = mats[1];
            //_name.text = "Bee";
        }
        else if (newtype == UnitType.Dog)
        {
            _mrcopy[0] = mats[2];
            //_name.text = "Dog";
        }
        else if (newtype == UnitType.Spider)
        {
            _mrcopy[0] = mats[3];
            //_name.text = "Spider";
        }
        _mr.materials = _mrcopy;*/
    }

    public void UpdateStats(int healthChange, int damageChange)
    {
        _health.text = healthChange.ToString();
        _attack.text = damageChange.ToString();
    }

    public IEnumerator takeDamage(float timeColorChange)
    {
        var _renderer = GetComponent<Renderer>();
        Color damamgeColor = new Color(1f, 0f, 0f, 1f);
        Color defaultColor = new Color(1f, 1f, 1f, 1f);
        _renderer.material.color = damamgeColor;
        yield return new WaitForSeconds(timeColorChange);
        _renderer.material.color = defaultColor;
    }

    public void highlight(Color newColor){
        var _renderer = GetComponent<Renderer>();
        _renderer.material.color = newColor;
    }


    public IEnumerator waitForHighlight()
    {
        yield return StartCoroutine(selectionhighlight());
    }
    public IEnumerator selectionhighlight()
    {
        _mr.material.EnableKeyword("_EMISSION");
        _mr.material.SetColor("_EmissionColor", Color.white);
        yield return new WaitForSeconds(1.0f);
        //Debug.Log("does this work?");
        _mr.material.DisableKeyword("_EMISSION");
        yield return new WaitForSeconds(3.0f);
        //Debug.Log("heheh");
    }

    public int GetUnitSlot()
    {
        return permaSlotId;
    }

    public UnitType GetUnitType()
    {
        return utype;
    }

    public ItemType GetItemType()
    {
        return itype;
    }

    public bool IsPlayed()
    {
        return isPlayed;
    }
    
    public IEnumerator changeHealth(int deltaHealth)
    {
        Vector3 currPos = new Vector3(_deltaHP.transform.position.x, _deltaHP.transform.position.y, _deltaHP.transform.position.z);
        Vector3 finalPos = new Vector3(_deltaHP.transform.position.x, _deltaHP.transform.position.y + 0.2f, _deltaHP.transform.position.z);
        //Debug.Log("Is this thing on?");
        if(deltaHealth > 0)
        {
            _deltaHP.text = deltaHealth.ToString();
            yield return StartCoroutine(_gm.Lerp(_deltaHP.gameObject, finalPos, 2.0f));
            _deltaHP.text = "";
            _deltaHP.transform.position = currPos;
        }
        else
        {
            _deltaHM.text = deltaHealth.ToString();
            yield return StartCoroutine(_gm.Lerp(_deltaHM.gameObject, finalPos, 2.0f));
            _deltaHM.text = "";
            _deltaHM.transform.position = currPos;
        }
    }
}
