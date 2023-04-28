using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardObject : MonoBehaviour
{
    // Start is called before the first frame update
    Rigidbody _rigidbody;
    Camera cam;
    public LayerMask targetLayer;

    BoxCollider _coll;
    //public float speed = 1;
    //public float maxSpeed = 10;
    public float cardOffset = -0.1f;
    GameManager _gm;

    int unitId;

    bool isSelected = false;

    bool isPlayed = false;

    int slotid = -1;

    bool inHand = false;

    int phaseNum=0;
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _coll = GetComponent<BoxCollider>();
        _gm = (GameManager)FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            slotid = _gm.IsTouchingPlayerSlot(touch);
            phaseNum = _gm.currentPhase();
            switch(phaseNum){
            case 0:
                
                if(Input.touchCount > 0){
                    Touch touch0 = Input.GetTouch(0);
                    slotid = _gm.IsTouchingPlayerSlot(touch0);
                    if (touch0.phase == TouchPhase.Began && (_gm.IsTouched(touch0, _coll) && !isPlayed)){
                        //Debug.Log("selected!");
                        isSelected = true;
                    }
                    else if (touch0.phase == TouchPhase.Began && isSelected && (slotid >= 0) &&!isPlayed)
                    {
                        //Debug.Log("play!");
                        Transform slotTrans = _gm.playerslots[slotid].transform;
                        Vector3 newPos = new Vector3(slotTrans.position.x, slotTrans.position.y, slotTrans.position.z + cardOffset);
                        transform.position = newPos;
                        isSelected = false;
                        isPlayed = true;
                        //_gm.UpdateGameSlot(slotid);
                    }
                    else if (touch0.phase == TouchPhase.Began && !(_gm.IsTouched(touch0, _coll)) &&!isPlayed)
                    {
                        //Debug.Log("unselected!");
                        isSelected = false;
                    }
                    
                }
                break;
            case 1:
                isSelected = false;
                if(Input.touchCount > 0)
                {
                    Touch touch1 = Input.GetTouch(0);
                    slotid = _gm.IsTouchingPlayerSlot(touch1);
                    if (touch1.phase == TouchPhase.Began && (_gm.IsTouched(touch1, _coll) && !isPlayed)){
                        isSelected = true;
                        StartCoroutine(_gm.PhaseTextChange("pick an enemy to attack",1f));
                    }
                    if(touch1.phase == TouchPhase.Began && isSelected){
                        touch1 = Input.GetTouch(0);
                        slotid = _gm.IsTouchingPlayerSlot(touch1);
                        print(slotid); 
                    }

                }
                break;
            case 2:
                break;
            }
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
}
