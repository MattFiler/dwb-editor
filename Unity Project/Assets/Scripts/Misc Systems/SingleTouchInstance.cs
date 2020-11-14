/*
 * *************************************************************
 * PLEASE DO NOT EDIT THIS SCRIPT WITHOUT CONTACTING THE AUTHORS
 * *************************************************************
 * 
 * A wrapper for TouchManager to allow some extra functionality per touch.
 * Awaiting some fixes to TouchManager to become feature complete (check TODOs).
 * 
 * AUTHORS:
 *  - Matt Filer
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleTouchInstance
{
    private Touch thisTouch = new Touch();
    private GameObject thisGO;
    private Rect thisGORect;     //For UI elements
    private Bounds thisGOBounds; //For in-world objects

    public Touch Touch { get { return thisTouch; } }
    public GameObject GameObject { get { return thisGO; } }
    public Rect Rect { get { return thisGORect; } }
    public Bounds Bound { get { return thisGOBounds; } }

    /* Initialise the touch instance with a GameObject to track */
    public SingleTouchInstance(GameObject _trackable)
    {
        thisGO = _trackable;
        if (thisGO.GetComponent<RectTransform>()) thisGORect = thisGO.GetComponent<RectTransform>().rect;
        if (thisGO.GetComponent<SpriteRenderer>()) thisGOBounds = thisGO.GetComponent<SpriteRenderer>().bounds;
    }

    /* Is the touch that interacted with this GameObject currently down? */
    public bool IsTouchDown()
    {
        if (!TouchManager.Instance.IsTouched(thisGO, ref thisTouch)) return false; 
        return (thisTouch.phase == TouchPhase.Began ||
                thisTouch.phase == TouchPhase.Moved ||
                thisTouch.phase == TouchPhase.Stationary);
    }

    /* Return the position of the last touch to interact with the GameObject */
    public Vector3 GetTouchPosition(bool worldPoint = false)
    {
        if (worldPoint) return Camera.main.ScreenToWorldPoint(thisTouch.position);
        return thisTouch.position;
    }

    /* Move to the GameObject to the active touch position (if one is active) */
    public void MoveToTouch(bool worldPoint = false)
    {
        if (!IsTouchDown()) return;
        if (worldPoint)
        {
            thisGO.transform.position = thisTouch.position - new Vector2(thisGOBounds.size.x / 2, thisGOBounds.size.y / 2);
            return;
        }
        thisGO.transform.position = thisTouch.position - new Vector2(thisGORect.width / 2, thisGORect.height / 2);
    }
}
