/*
 * *************************************************************
 * PLEASE DO NOT EDIT THIS SCRIPT WITHOUT CONTACTING THE AUTHORS
 * *************************************************************
 * 
 * The main touch input system manager.
 * Allows ability to track touches on GameObjects, supports multi-touch.
 * 
 * AUTHORS:
 *  - Evan Diamond
 *  - Toby Jones
 *  - Matt Filer
*/

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;


//////////////////////////////////////////
// TOUCH MANAGER - TO CHECK FOR A TOUCH //
/// //////////////////////////////////////
/* Touch touch = new Touch(); 
 * if (TouchManager.Instance.IsTouched(GAMEOBJECT, ref touch))
 * {
 *      CODE HERE
 * }
 * 
 * Wrappers -
 * TouchBegin - check for touch has begun
 * TouchEnded - check for touch has begun
 * TouchHeld - check for touch is held (moved, stationary)
 * 
 * You can check if a touch is pressed or released using touch.phase
 * GetButtonDown = TouchPhase.Began
 * GetButtonUp = TouchPhase.Ended
 * GetButton = TouchPhase.Stationary
 * 
 * MAKE SURE THE CAMERA IS ORTHOGRAPHIC
 * MAKE SURE THE GAMEOBJECT HAS THE TOUCHABLE LAYER OR THE LAYER HAS BEEN ADDED TO THE MASK
 */

class TouchedObject
{
    public GameObject gameObject;
    public bool isUI = false;
}

public class TouchManager : MonoSingleton<TouchManager>
{
    private Dictionary<GameObject, Touch> touchedObjects = new Dictionary<GameObject, Touch>();
    private Dictionary<int, TouchedObject> lastFrameTouches = new Dictionary<int, TouchedObject>();

    [Tooltip("Number of touches needed to for a camera")]
    [SerializeField] private int cameraTouches = 2;
    
    /*[HideInInspector] */public bool cameraToggle = true;

    /* Setup contact filter for layer filtering */
    [SerializeField] private LayerMask validTouchLayers;
    private ContactFilter2D cf;

#region Wrappers
    /* Returns true if the queried object is being touched */
    public bool IsTouched(GameObject queryObject)
    {
        return touchedObjects.ContainsKey(queryObject);
    }

    /* Returns true if the queried object is being touched */
    public bool IsTouched(GameObject queryObject, ref Touch touch)
    {
        if (touchedObjects.ContainsKey(queryObject))
        {
            touch = touchedObjects[queryObject];
            return true;
        }

        return false;
    }

    /* Returns true if the queried object is being touched */
    public bool IsTouched(GameObject queryObject, ref Vector3 position)
    {
        if (touchedObjects.ContainsKey(queryObject))
        {
            position = touchedObjects[queryObject].position;
            return true;
        }

        return false;
    }

    /* Returns true if a touch has began on queried object */
    public bool TouchBegin(GameObject queryObject)
    {
        Touch touch = new Touch();
        if (IsTouched(queryObject, ref touch)) return touch.phase == TouchPhase.Began;
        return false;
    }

    /* Returns true if a touch has began on queried object */
    public bool TouchBegin(GameObject queryObject, ref Touch touch)
    {
        if(IsTouched(queryObject, ref touch)) return touch.phase == TouchPhase.Began;
        return false;
    }

    /* Returns true if a touch has began on queried object */
    public bool TouchBegin(GameObject queryObject, ref Vector3 position)
    {
        Touch touch = new Touch();
        if (IsTouched(queryObject, ref touch))
        {
            position = touch.position;
            return touch.phase == TouchPhase.Began;
        }

        return false;
    }

    /* Returns true if a touch has ended on queried object */
    public bool TouchEnded(GameObject queryObject, bool includeCancled = true)
    {
        Touch touch = new Touch();
        if (IsTouched(queryObject, ref touch)) return includeCancled ? (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended) : touch.phase == TouchPhase.Ended;
        return false;
    }

    /* Returns true if a touch has ended on queried object */
    public bool TouchEnded(GameObject queryObject, ref Touch touch, bool includeCancelled = true)
    {
        if (IsTouched(queryObject, ref touch)) return includeCancelled ? (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended) : touch.phase == TouchPhase.Ended;
        return false;
    }

    /* Returns true if a touch has ended on queried object */
    public bool TouchEnded(GameObject queryObject, ref Vector3 position, bool includeCancelled = true)
    {
        Touch touch = new Touch();
        if (IsTouched(queryObject, ref touch))
        {
            position = touch.position;
            return includeCancelled ? (touch.phase == TouchPhase.Canceled || touch.phase == TouchPhase.Ended) : touch.phase == TouchPhase.Ended;
        }

        return false;
    }

    /* Returns true if a touch is active or stationary on queried object*/
    public bool TouchHeld(GameObject queryObject)
    {
        Touch touch = new Touch();
        if (IsTouched(queryObject, ref touch)) return touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary;
        return false;
    }

    /* Returns true if a touch is active or stationary on queried object*/
    public bool TouchHeld(GameObject queryObject, ref Touch touch)
    {
        if (IsTouched(queryObject, ref touch)) return touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary;
        return false;
    }

    /* Returns true if a touch is active or stationary on queried object*/
    public bool TouchHeld(GameObject queryObject, ref Vector3 position)
    {
        Touch touch = new Touch();
        if (IsTouched(queryObject, ref touch))
        {
            position = touch.position;
            return touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary;
        }

        return false;
    }

    /* Returns true if there are touches that aren't touching anything (used for camera)*/
    public bool CameraTouch()
    {
        if (LevelManager.Instance.IsInEditor && !cameraToggle) return false;

        int touchCount = 0;
        foreach (Touch touch in getAllTouches())
        {
            if (!touchedObjectContains(touch))
            {
                ++touchCount;
                //if (touchCount >= cameraTouches) return true;
            }
        }
        return touchCount >= cameraTouches;
    }

    /* Returns true if there are touches that aren't touching anything (used for camera)*/
    public bool CameraTouch(ref List<Vector3> positions, bool inWorldSpace = false)
    {
        if (LevelManager.Instance.IsInEditor && !cameraToggle) return false;

        int touchCount = 0;
        foreach (Touch touch in getAllTouches())
        {
            if (!touchedObjectContains(touch))
            {
                ++touchCount;
                positions.Add(inWorldSpace? Camera.main.ScreenToWorldPoint(touch.position) :(Vector3)touch.position);
                //if (touchCount >= cameraTouches) return true;
            }
        }
        return touchCount >= cameraTouches;
    }
#endregion

    void Start()
    {
        cf = new ContactFilter2D();
        cf.layerMask = validTouchLayers;
        cf.useTriggers = true;
    }

    /* Track inputs on update */
    void Update()
    {
       
        Dictionary<int, TouchedObject> currentFrameTouches = new Dictionary<int, TouchedObject>();

        touchedObjects.Clear();

        List<Touch> allTouches = getAllTouches();

        /*Check prior touched gameobjects to see if they're still touched*/
        removePreviousTouches(ref allTouches, ref currentFrameTouches);

        /*Adds removes or updates touchedObjects based on touches touch phase */
        foreach (Touch touch in allTouches)
        {
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    checkForTouchedObjects(touch, ref currentFrameTouches);
                    break;

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    if(touchedObjectContains(touch))
                    {
                        touchedObjects.Remove(getTouchedObjectsKey(touch));
                    }
                    break;

                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    if (lastFrameTouches.ContainsKey(touch.fingerId) && !touchedObjects.ContainsKey(lastFrameTouches[touch.fingerId].gameObject))
                    {
                        Touch newTouch = touch;
                        newTouch.position = lastFrameTouches[touch.fingerId].isUI ? touch.position : (Vector2)Camera.main.ScreenToWorldPoint(touch.position);
                        touchedObjects.Add(lastFrameTouches[touch.fingerId].gameObject, newTouch);
                        currentFrameTouches.Add(touch.fingerId, lastFrameTouches[touch.fingerId]);
                    }
                    break;
            }
        }

        lastFrameTouches = currentFrameTouches;
    }

    /*Returns all current touches, including the mouse*/
    private static List<Touch> getAllTouches()
    {
        List<Touch> allTouches = new List<Touch>(Input.touches);
        /*Spoof mouse input as a touch*/
        bool mouseClicked = false;
        Touch mouseTouch = getMouseTouch(out mouseClicked);
        if (mouseClicked && allTouches.Count == 0)
        {
            mouseTouch.fingerId = -1;
            allTouches.Add(mouseTouch);
        }

        return allTouches;
    }

    /*Check prior touched gameobjects to see if they're still touched*/
    private void removePreviousTouches(ref List<Touch> allTouches, ref Dictionary<int, TouchedObject> currentFrameTouches)
    {
        List<Touch> removeList = new List<Touch>();
        foreach (Touch touch in allTouches)
        {
            if (lastFrameTouches.ContainsKey(touch.fingerId))
            {
                bool hitUI = false;
                /*For ui*/
                foreach (RaycastResult result in UiRaycast(touch.position))
                {
                    if (result.gameObject == lastFrameTouches[touch.fingerId].gameObject)
                    {
                        addTouchedGameObject(touch, result.gameObject, result.screenPosition, ref currentFrameTouches, true);
                        removeList.Add(touch);
                        //break;
                    }
                }

                if (hitUI) continue;
                /*For non ui*/
                foreach (RaycastHit2D hit in Raycast(touch.position))
                {
                    if (hit.collider != null && hit.collider.gameObject == lastFrameTouches[touch.fingerId].gameObject)
                    {
                        addTouchedGameObject(touch, hit.collider.gameObject, hit.point, ref currentFrameTouches, false);
                        removeList.Add(touch);
                        //break;
                    }
                }
            }
        }

        /*Remove already active touches*/
        foreach (Touch t in removeList)
        {
            allTouches.Remove(t);
        }
    }

    /* Spoof a mouse input as a physical touch */
    private static Touch getMouseTouch(out bool mouseClicked)
    {
        Touch mouseTouch = new Touch();
        mouseTouch.position = Input.mousePosition;

        mouseClicked = false;
        if (Input.GetMouseButtonDown(0))
        {
            //First press
            mouseTouch.phase = TouchPhase.Began;
            mouseClicked = true;
        }
        else if (Input.GetMouseButton(0))
        {
            //Holding down
            mouseClicked = true;
            mouseTouch.phase = TouchPhase.Stationary;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            //Released press
            mouseTouch.phase = TouchPhase.Ended;
            mouseClicked = true;
        }

        return mouseTouch;
    }

    /* Add an object to the touched list */
    private void addTouchedGameObject(Touch touch, GameObject gameObject, Vector2 position, ref Dictionary<int, TouchedObject> currentFrameTouches, bool isUI)
    {
        //Debug.Log("Adding object: " + touchedObject + " touch finger id:" + touch.fingerId);

        touch.position = position;
        touchedObjects.Add(gameObject, touch);

        TouchedObject touchedObject = new TouchedObject();
        touchedObject.gameObject = gameObject;
        touchedObject.isUI = isUI;
        if (!currentFrameTouches.ContainsKey(touch.fingerId)) currentFrameTouches.Add(touch.fingerId, touchedObject);
    }

    /*Raycast from postion to non ui 2d objects in the scene */
    RaycastHit2D[] Raycast(Vector2 position)
    {
        RaycastHit2D[] hits = new RaycastHit2D[10];
        if (Camera.main) Physics2D.Raycast(Camera.main.ScreenPointToRay(position).origin, Vector3.forward, cf, hits);
        return hits;
    }

    /*Raycast from postion to ui objects in the scene */
    List<RaycastResult> UiRaycast(Vector2 position)
    {
        var eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = position;
        List<RaycastResult> raycastResults = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, raycastResults);
        return raycastResults;
    }

    /*Adds or removes gameobjects from touchedObjects based on touch poition*/
    private void checkForTouchedObjects(Touch touch, ref Dictionary<int, TouchedObject> currentFrameTouches)
    {
        bool hitUI = false;
        /*For ui*/
        foreach (RaycastResult result in UiRaycast(touch.position))
        {
            if (((1 << result.gameObject.layer) & validTouchLayers) != 0)
            {
                if (!touchedObjects.ContainsKey(result.gameObject))
                {
                    addTouchedGameObject(touch, result.gameObject, touch.position, ref currentFrameTouches, true);
                    hitUI = true;
                    //break;
                }
            }
        }

        //Disables non ui in when camera is enabled in the editor
        if (hitUI || (LevelManager.Instance.IsInEditor && cameraToggle)) return;

        /*For non ui*/
        foreach (RaycastHit2D hit in Raycast(touch.position))
        {
            if (hit.collider != null && ((1 << hit.collider.gameObject.layer) & validTouchLayers) != 0)
            {
                if (!touchedObjects.ContainsKey(hit.collider.gameObject))
                {
                    addTouchedGameObject(touch, hit.collider.gameObject, Camera.main.ScreenToWorldPoint(touch.position), ref currentFrameTouches, false);
                    //break;
                }
            }
        }
    }

    /*Checks if touch is already in touchedObjects based on fingerId (not the most solid method)*/
    private bool touchedObjectContains(Touch touch)
    {
        foreach(KeyValuePair<GameObject, Touch> kv in touchedObjects)
        {
            if (kv.Value.fingerId == touch.fingerId) return true;
        }

        return false;
    }

    /*Returns Gameobject (key) that is mapped to touch (value) - I couldn't see a better way of doing this*/
    private GameObject getTouchedObjectsKey(Touch touch)
    {
        foreach (KeyValuePair<GameObject, Touch> kv in touchedObjects)
        {
            if (kv.Value.fingerId == touch.fingerId) return kv.Key;
        }

        return null;
    }
}
