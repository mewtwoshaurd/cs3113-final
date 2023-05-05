using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PhaseButtonClick : MonoBehaviour
{
    // Start is called before the first frame update
    Camera cam;
    public LayerMask targetLayer;

    CapsuleCollider _coll;
    GameManager _gm;

    bool prevMouseDown = false;

    //bool isSelected = false;
    int unitId;
    void Start()
    {
        _coll = GetComponent<CapsuleCollider>();
        _gm = (GameManager)FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        bool isTouching = false;
        Vector3 touchPos = Vector3.zero;
#if !UNITY_ANDROID || UNITY_EDITOR
        if (!Input.GetMouseButton(0) && prevMouseDown)
        {
            touchPos = Input.mousePosition;
            isTouching = _gm.IsTouched(touchPos, _coll);
        }

        prevMouseDown = Input.GetMouseButton(0);
#else
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            touchPos = touch.position;
            isTouching = (touch.phase == TouchPhase.Ended) && _gm.IsTouched(touchPos,_coll);          
        }
#endif
        if (isTouching)
        {
            _gm.IncrementPhase();
        }
    }
}
