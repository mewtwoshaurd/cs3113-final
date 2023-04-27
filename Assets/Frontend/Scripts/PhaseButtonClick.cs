using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PhaseButtonClick : MonoBehaviour
{
    // Start is called before the first frame update
    Camera cam;
    public LayerMask targetLayer;

    BoxCollider _coll;
    GameManager _gm;
    bool isSelected = false;
    int unitId;
    void Start()
    {
        _coll = GetComponent<BoxCollider>();
        _gm = (GameManager)FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            int id = touch.fingerId;
            if (touch.phase == TouchPhase.Began){
                //Debug.Log(EventSystem.current.IsPointerOverGameObject(id));
                if (EventSystem.current.IsPointerOverGameObject(id) && (_gm.hasPhaseChanged())){
                    //Debug.Log("buttonClicked");
                    _gm.IncrementPhase();
                }
            }
            
        }
    }
}
