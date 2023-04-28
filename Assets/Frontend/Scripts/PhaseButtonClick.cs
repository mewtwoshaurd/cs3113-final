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

        if (isTouching)
        {
            //Debug.Log(EventSystem.current.IsPointerOverGameObject(id));
            if (_gm.IsTouched(touchPos, _coll) && (_gm.hasPhaseChanged()) && !(_gm.transitioning))
            {
                //Debug.Log("buttonClicked");
                _gm.IncrementPhase();
            }
        }
    }
}
