using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardObject : MonoBehaviour
{
    // Start is called before the first frame update
    Rigidbody _rigidbody;
    Camera cam;
    public LayerMask targetLayer;

    public float maxSpeed = 10;
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.touchCount > 0)
        {
            Debug.Log("Touch Input");
            Touch touch = Input.GetTouch(0);
            if((Physics2D.OverlapPoint(cam.ScreenToWorldPoint(Input.mousePosition),targetLayer)) && (touch.phase == TouchPhase.Moved)){
              
                _rigidbody.AddForce(touch.deltaPosition);
                _rigidbody.velocity = Vector3.ClampMagnitude(_rigidbody.velocity, maxSpeed);
            }
        }
    }
}
