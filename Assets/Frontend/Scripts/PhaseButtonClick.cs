using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhaseButtonClick : MonoBehaviour
{
    // Start is called before the first frame update
    Camera cam;
    public LayerMask targetLayer;

    BoxCollider _coll;
    GameManager _gm;
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
            Debug.Log("Touch Input");
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                Debug.Log(_gm.IsTouched(touch,_coll));
                Debug.Log("clicked!");
            }
        }
    }
}
