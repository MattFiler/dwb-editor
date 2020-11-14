using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraMovement : MonoBehaviour
{

    public enum InputType
    {
        MOUSE_TOUCH = 0,
        WASD = 1,
        ARROW_KEYS = 2,
        NUMPAD = 3
    }

    enum Direction
    {
        UP = 0,
        RIGHT = 1,
        DOWN = 2,
        LEFT = 3
    }

    Camera cam;

    /* The type of Input for moving the camera */
    [SerializeField] InputType inputType = InputType.WASD;

    /* Speed of camera */
    [SerializeField] float moveSpeed = 15.0f;

    /* Speed of camera zoom */
    [SerializeField][Range(1.0f, 100.0f)]
    float scrollSensitivity = 10.0f;

    /* Used to store the distance between the touches of the previous update */
    float lastDistance = 0.0f;

    /* The gameobject that the camera will follow */
    GameObject focusedGameObject;

    /* If true it focus on a pawn and the camera will follow them around */
    bool focused = false;

    /* Used to check if the player has been touching the screen on the previous update */
    bool touching = false;

    /* Used to check if the player has been touching the screen on the previous update */
    bool moving = false;

    /* Used to check if the max and min of the level has been loaded */
    bool gotBounds = false;

    /* Used to set if the camera should focus on bus at the start of the level */
    [SerializeField] bool startFocusOnBus = false;

    /* Limits the camera bounds to the size fo the occupied tiles else it can be moved around outside the level */
    [SerializeField] bool limitBoundsToOccupiedTiles = false;

    /* The list of positions that are passed into the touch mananger and used to working out the position of fingers */
    List<Vector3> touchPositions = new List<Vector3>();

    /* The average position of the list of touches passed into the touch manager. */
    Vector3 avTouchPos = new Vector3();

    /* The average position of the list of touches passed into the touch manager from the previous update. */
    Vector3 lastAvTouchPos = new Vector3();

    /* The lower limit of the zoom, so it won't zoom in further than this. */
    [SerializeField][Range(1.0f, 3.0f)]
    float zoomLowerLimit = 1.5f;

    /* The upper limit of the zoom, so it won't zoom out further than this. */
    [SerializeField][Range(6.0f, 14.0f)] 
    float zoomUpperLimit = 8.0f;

    /*
     *  The maximum position of the the level grid or occupied tiles depending on the 'limitBoundsToOccupiedTiles', 
     *  used for the bounds of the camera
     */
    Vector3 gridMin = new Vector2(Mathf.Infinity, Mathf.Infinity);

    /*
     *  The minimum position of the the level grid or occupied tiles depending on the 'limitBoundsToOccupiedTiles', 
     *  used for the bounds of the camera
     */
    Vector3 gridMax = new Vector2(-Mathf.Infinity, -Mathf.Infinity);

    /* Used to store the average position of the fingers when they first touch the screen */
    Vector3 camStartTouchPos;

    Vector3 cameraVelo = Vector2.zero;

    [SerializeField]
    private Image screenBlocker;

    void Awake()
    {
        StartCoroutine(BlockTillLoaded());

        cam = GetComponent<Camera>();

        if (limitBoundsToOccupiedTiles)
        {
            //cam.transform.position = new Vector3(0.0f, 0.0f, -5.0f);
            cam.orthographicSize = 5.0f;
        }
    }

    private IEnumerator BlockTillLoaded()
    {
        if (!screenBlocker) yield break;

        screenBlocker.color = new Color(255, 255, 255, 255);
        screenBlocker.raycastTarget = true;

        while (LevelManager.Instance.CurrentMeta == null || !LevelManager.Instance.CurrentMeta.isLoaded)
            yield return new WaitForEndOfFrame();

        while (LevelManager.Instance.Grid == null)
        {
            yield return new WaitForEndOfFrame();
        }

        Vector3 startPos = Vector3.zero;

        foreach (var tile in LevelManager.Instance.Grid.AllTiles)
        {
            if (tile.HasProp && tile.Prop.Type == PropTypes.BUS_STOP)
            {
                startPos = tile.Position;
                startPos.z = -5.0f;
                break;
            }
        }

        ClampPositionToBounds(ref startPos);
        cam.transform.position = startPos;
        Color col = screenBlocker.color;

        yield return new WaitForSeconds(1.8f);

        for(int i = 0; i < 21; i++)
        {
            col.a = 1.0f - ((float)i / 20.0f);
            screenBlocker.color = col;
            yield return new WaitForSeconds(0.05f);
        }
        screenBlocker.color = new Color(0, 0, 0, 0);
        screenBlocker.raycastTarget = false;
    }


    // Update is called once per frame
    void LateUpdate()
    {
        if (TouchManager.Instance.cameraToggle)
            return;

        //if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    //CameraShake.Instance.Shake(CameraShake.Type.POS, CameraShake.Axis.X, 1.0f, 0.2f);
        //    CameraShake.Instance.Shake(CameraShake.Type.POS, CameraShake.Axis.ALL, 3.0f, 0.3f);
        //}

        //if (SceneHandler.Instance.GetCurrentSceneID() == SceneHandler.SceneID.MainMenu)
        //    return;

        if (!GotBounds())
            return;

        moving = false;
        switch (inputType)
        {
            case InputType.MOUSE_TOUCH:
                {
                    TouchInput();
                    break;
                }
            case InputType.WASD:
                {
                    KeyInput(KeyCode.W, KeyCode.S, KeyCode.A, KeyCode.D);
                    break;
                }
            case InputType.ARROW_KEYS:
                {
                    KeyInput(KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow);
                    break;
                }
            case InputType.NUMPAD:
                {
                    KeyInput(KeyCode.Keypad8, KeyCode.Keypad5, KeyCode.Keypad4, KeyCode.Keypad6);
                    break;
                }
        }

        UpdatePosition();
        if (moving)
            return;

        float size = cam.orthographicSize - Input.mouseScrollDelta.y * (scrollSensitivity / 100.0f);
        ZoomBounds(ref size);
        cam.orthographicSize = size;

        if (focused)
            FollowGameObject();


    }

    private void UpdatePosition()
    {
        Vector3 newPos = transform.position;
        newPos += cameraVelo * Time.deltaTime;
        if(cameraVelo.x > 0)
            ClampPositionToBounds(ref newPos, Direction.RIGHT);
        else if(cameraVelo.x < 0)
            ClampPositionToBounds(ref newPos, Direction.LEFT);
        if(cameraVelo.y > 0)
            ClampPositionToBounds(ref newPos, Direction.UP);
        else if(cameraVelo.y < 0)
            ClampPositionToBounds(ref newPos, Direction.DOWN);

        transform.position = newPos;

        // Decay move speed when no move input is detected
        if(!moving)
        {
            cameraVelo /= 1.1f;
            if (cameraVelo.sqrMagnitude < 1)
                cameraVelo = Vector3.zero;
        }
    }

    private bool GotBounds()
    {
        if (gotBounds)
            return gotBounds;

        if (LevelManager.Instance.CurrentMeta.isLoaded)
        {
            var tiles = LevelManager.Instance.Grid.AllTiles;
            foreach (var tile in tiles)
            {
                if (limitBoundsToOccupiedTiles)
                {
                    if (!tile.IsOccupied)
                        continue;
                }

                gridMin = new Vector2(gridMin.x > tile.Position.x ? tile.Position.x : gridMin.x,
                    gridMin.y > tile.Position.y ? tile.Position.y : gridMin.y);

                gridMax = new Vector2(gridMax.x < tile.Position.x ? tile.Position.x : gridMax.x,
                    gridMax.y < tile.Position.y ? tile.Position.y : gridMax.y);
            }

            var difference = gridMax - gridMin;
            if (limitBoundsToOccupiedTiles)
            {
                var pos = gridMin + (difference) / 2.0f;
                cam.transform.position = new Vector3(pos.x, pos.y, cam.transform.position.z);
            }

            difference.x = (difference.x / ((float)Screen.width / (float)Screen.height)) / 2.0f;
            difference.y = (difference.y) / 2.0f;
            zoomUpperLimit = (difference.x < difference.y ? difference.x : difference.y) - 0.2f;

            return gotBounds = true;
        }
        return false;
    }

    private void KeyInput(KeyCode up, KeyCode down, KeyCode left, KeyCode right)
    {
        Vector3 newPos = new Vector3(0, 0, cam.transform.position.z);
        if (Input.GetKey(up))
        {
            cameraVelo += new Vector3(0, 0.1f, 0) * moveSpeed;
            Unfocus();
            moving = true;
        }
        if (Input.GetKey(left))
        {
            cameraVelo += new Vector3(-0.1f, 0, 0) * moveSpeed;
            Unfocus();
            moving = true;
        }
        if (Input.GetKey(down))
        {
            cameraVelo += new Vector3(0, -0.1f, 0) * moveSpeed;
            Unfocus();
            moving = true;
        }
        if (Input.GetKey(right))
        {
            cameraVelo += new Vector3(0.1f, 0, 0) * moveSpeed;
            Unfocus();
            moving = true;
        }
    }

    private void TouchInput()
    {
        List<Vector3> tmpTouches = new List<Vector3>();
        if (TouchManager.Instance.CameraTouch(ref tmpTouches))
        {
            if (Pinching(tmpTouches))
                return;
            else if (!touching)
            {
                touching = true;
                touchPositions = tmpTouches;
                lastAvTouchPos = GetAverageTouchPos(touchPositions);
                camStartTouchPos = transform.position;
            }
            else
            {
                avTouchPos = GetAverageTouchPos(tmpTouches);
                Vector3 moveVector = (Camera.main.ScreenToWorldPoint(lastAvTouchPos) - Camera.main.ScreenToWorldPoint(avTouchPos));
                Vector3 finalPosition = Vector3.Lerp(camStartTouchPos, camStartTouchPos + moveVector * moveSpeed, 0.1f);
                cameraVelo = moveVector * moveSpeed * 7.0f;
            }
            touchPositions = tmpTouches;
            lastAvTouchPos = GetAverageTouchPos(touchPositions);
            Unfocus();
            moving = true;
        }
        else
        {
            touching = false;
        }
    }

    void ClampPositionToBounds(ref Vector3 camPosition)
    {
        Bounds bounds = OrthographicBounds(cam.orthographicSize);

        bounds.center = camPosition;

        float offset = 0.05f;

        if (bounds.max.y > gridMax.y - offset)
        {
            camPosition.y = gridMax.y - (bounds.size.y / 2.0f) - offset - 0.01f;
        }
        if (bounds.min.y < gridMin.y + offset)
        {
            camPosition.y = gridMin.y + (bounds.size.y / 2.0f) + offset + 0.01f;
        }
        if (bounds.max.x > gridMax.x - offset)
        {
            camPosition.x = gridMax.x - (bounds.size.x / 2.0f) - offset - 0.01f;
        }
        if (bounds.min.x < gridMin.x + offset)
        {
            camPosition.x = gridMin.x + (bounds.size.x / 2.0f) + offset + 0.01f;
        }
    }

    void ClampPositionToBounds(ref Vector3 camPosition, Direction dir)
    {
        Bounds bounds = OrthographicBounds(cam.orthographicSize);

        bounds.center = camPosition;

        float offset = 0.05f;

        if (dir == Direction.UP && bounds.max.y > gridMax.y - offset)
        {
            camPosition.y = gridMax.y - (bounds.size.y / 2.0f) - offset;
        }
        if (dir == Direction.DOWN && bounds.min.y < gridMin.y + offset)
        {
            camPosition.y = gridMin.y + (bounds.size.y / 2.0f) + offset;
        }
        if (dir == Direction.RIGHT && bounds.max.x > gridMax.x - offset)
        {
            camPosition.x = gridMax.x - (bounds.size.x / 2.0f) - offset;
        }
        if (dir == Direction.LEFT && bounds.min.x < gridMin.x + offset)
        {
            camPosition.x = gridMin.x + (bounds.size.x / 2.0f) + offset;
        }
    }

    void ZoomBounds(ref float size)
    {

        if (size < zoomLowerLimit)
        {
            size = zoomLowerLimit;
            return;
        }
        else if (size > zoomUpperLimit)
        {
            size = zoomUpperLimit;
            return;
        }

        Bounds bounds = OrthographicBounds(size);

        Vector3 camPosition = cam.transform.position;
        bounds.center = camPosition;

        float offset = 0.05f;

        if (bounds.max.y > gridMax.y - offset)
        {
            if (bounds.min.y < gridMin.y + offset)
            {
                size = cam.orthographicSize;
                var pos = gridMin + (gridMax - gridMin) / 2.0f;
                cam.transform.position = new Vector3(cam.transform.position.x, pos.y, cam.transform.position.z);
                return;
            }
            camPosition.y = gridMax.y - bounds.size.y / 2.0f - offset - 0.01f;
        }
        if (bounds.max.x > gridMax.x - offset)
        {
            if (bounds.min.x < gridMin.x + offset)
            {
                size = cam.orthographicSize;
                var pos = gridMin + (gridMax - gridMin) / 2.0f;
                cam.transform.position = new Vector3(pos.x, cam.transform.position.y, cam.transform.position.z);
                return;
            }
            camPosition.x = gridMax.x - bounds.size.x / 2.0f - offset - 0.01f;
        }
        if (bounds.min.y < gridMin.y + offset)
        {
            if (bounds.max.y > gridMax.y - offset)
            {
                size = cam.orthographicSize;
                var pos = gridMin + (gridMax - gridMin) / 2.0f;
                cam.transform.position = new Vector3(cam.transform.position.x, pos.y, cam.transform.position.z);
                return;
            }
            camPosition.y = gridMin.y + bounds.size.y / 2.0f + offset + 0.01f;
        }
        if (bounds.min.x < gridMin.x + offset)
        {
            if (bounds.max.x > gridMax.x - offset)
            {
                size = cam.orthographicSize;
                var pos = gridMin + (gridMax - gridMin) / 2.0f;
                cam.transform.position = new Vector3(pos.x, cam.transform.position.y, cam.transform.position.z);
                return;
            }
            camPosition.x = gridMin.x + bounds.size.x / 2.0f + offset + 0.01f;
        }

        cam.transform.position = camPosition;
    }

    Bounds OrthographicBounds(float size)
    {
        float screenAspect = (float)Screen.width / (float)Screen.height;
        float cameraHeight = size * 2;

        Bounds bounds = new Bounds(
            transform.position,
            new Vector3(cameraHeight * screenAspect, cameraHeight, 0));

        return bounds;
    }

    public void Focus(GameObject gO)
    {
        focusedGameObject = gO;
        focused = true;

        Vector3 dstPos = new Vector3(focusedGameObject.transform.position.x,
            focusedGameObject.transform.position.y,
            transform.position.z);

        FollowGameObject();
    }

    public void Unfocus()
    {
        focused = false;
        focusedGameObject = null;
    }

    public bool IsFocused()
    {
        return focused;
    }

    private void FollowGameObject()
    {
        if (!focusedGameObject)
        {
            Unfocus();
            return;
        }

        Vector3 newPos = new Vector3(focusedGameObject.transform.position.x,
            focusedGameObject.transform.position.y,
            transform.position.z);
        ClampPositionToBounds(ref newPos);
        transform.position = Vector3.Lerp(transform.position, newPos, moveSpeed * Time.deltaTime);

        //transform.position = newPos;
    }

    bool Pinching(List<Vector3> ts)
    {
        if (ts.Count < 2)
            return false;

        float distance = (ts[0] - ts[1]).magnitude;

        if (!touching)
        {
            lastDistance = distance;
            touching = true;
            return true;
        }

        float diff = distance - lastDistance;
        Debug.Log(diff);
        if (diff < 1.0f && diff > -1.0f)
            return true;

        float size = cam.orthographicSize - ((distance - lastDistance) / 100.0f);
        ZoomBounds(ref size);
        cam.orthographicSize = size;
        lastDistance = distance;
        return true;
    }

    Vector3 GetAverageTouchPos(List<Vector3> t)
    {
        Vector3 temp = new Vector3();

        foreach (var touch in t)
        {
            temp += touch;
        }

        temp /= t.Count;

        return temp;
    }

    public InputType GetInputType()
    {
        return inputType;
    }

    public void SetInputType(InputType _inputType)
    {
        inputType = _inputType;
    }
}