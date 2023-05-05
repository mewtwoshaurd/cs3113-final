using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragDrop : MonoBehaviour
{
    Transform card;
    Camera cam;
    public LayerMask targetLayer;
    public LayerMask boxLayer;

    private void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
/*#if UNITY_STANDALONE || UNITY_EDITOR
        //Mouse Ver
        if (Input.GetMouseButtonDown(0))
        {
            TouchMouseBegan(Input.mousePosition);
        }

        if (card != null)
        {
            if (Input.GetMouseButton(0))
            {
                TouchMouseMoved(Input.mousePosition);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                TouchMouseEnded(Input.mousePosition);
            }
        }
#endif*/

#if UNITY_IPHONE || UNITY_ANDROID

        //Touch Ver
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                TouchMouseBegan(touch.position);
            }

            if (card != null)
            {
                if (touch.phase == TouchPhase.Moved)
                {
                    TouchMouseMoved(touch.position);
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    TouchMouseEnded(touch.position);
                }
            }
        }
#endif
    }

    void TouchMouseBegan(Vector2 touchPos)
    {
        Vector2 worldPos = cam.ScreenToWorldPoint(touchPos);
        if (Physics2D.OverlapPoint(worldPos, boxLayer))
        {
            card = Physics2D.OverlapPoint(worldPos, boxLayer).transform;
        }
    }

    void TouchMouseMoved(Vector2 touchPos)
    {
        card.position = (Vector2)cam.ScreenToWorldPoint(touchPos);
    }

    void TouchMouseEnded(Vector2 touchPos)
    {
        Vector2 worldPos = cam.ScreenToWorldPoint(touchPos);
        if (Physics2D.OverlapPoint(worldPos, targetLayer))
        {
            card.position = Physics2D.OverlapPoint(worldPos, targetLayer).transform.position;
        }
        card = null;
    }
}
