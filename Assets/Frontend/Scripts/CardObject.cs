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
            Debug.Log("Touch Input");
            Touch touch = Input.GetTouch(0);
            slotid = _gm.IsTouchingPlayerSlot(touch);
            if (touch.phase == TouchPhase.Began && (_gm.IsTouched(touch, _coll) && !isPlayed))
            {
                Debug.Log("selected!");
                isSelected = true;
            }
            else if (touch.phase == TouchPhase.Began && isSelected && (slotid >= 0) &&!isPlayed)
            {
                Debug.Log("play!");
                Transform slotTrans = _gm.playerslots[slotid].transform;
                Vector3 newPos = new Vector3(slotTrans.position.x, slotTrans.position.y, slotTrans.position.z + cardOffset);
                transform.position = newPos;
                isSelected = false;
                isPlayed = true;
                //_gm.UpdateGameSlot(slotid);
            }
            else if (touch.phase == TouchPhase.Began && !(_gm.IsTouched(touch, _coll)) &&!isPlayed)
            {
                Debug.Log("unselected!");
                isSelected = false;
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
}
