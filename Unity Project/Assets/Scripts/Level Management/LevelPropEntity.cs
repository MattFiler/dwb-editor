/*
 * *************************************************************
 * PLEASE DO NOT EDIT THIS SCRIPT WITHOUT CONTACTING THE AUTHORS
 * *************************************************************
 * 
 * An object within the world placed on top of an occupied tile in the grid.
 * In editor this is spawned from LevelEditorComponent.
 * 
 * AUTHORS:
 *  - Matt Filer
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelPropEntity : MonoBehaviour
{
    private PropSprites spriteSet;                     //Our available sprites
    private List<Tile> gridTile = new List<Tile>();    //The tile(s) we occupy
    public List<Tile> Tiles { get { return gridTile; } }
    private PropTypes propType = (PropTypes)0;         //Defaults to type 0 with placeholder sprite - call SetTileType before using!
    public PropTypes Type { get { return propType; } }
    private PropRotation propRotation;                 //This prop's rotation (can be changed at runtime, but will only update collider, not occupiers
    public static Dictionary<PropRotation, Vector2> propRotationVector = new Dictionary<PropRotation, Vector2>()
    {
        {PropRotation.FACING_FRONT, Vector2.down},
        {PropRotation.FACING_LEFT, Vector2.left},
        {PropRotation.FACING_RIGHT, Vector2.right},
        {PropRotation.FACING_BACK, Vector2.up},
    };
    public PropRotation Rotation { get { return propRotation; } }
    public Vector2 RotationVector { get { return propRotationVector[propRotation]; } }
    public int RenderOrder { get { return ourSprite.sortingOrder; } }

    private GameObject childObject;
    private SpriteRenderer ourSprite;
    public PolygonCollider2D ourCollider { get; private set; }

    public bool EnableRotationOnTouch = false; //This is deprecated - but keeping the functionality in logic for now, because reasons. Don't rely on it!

    /* Set the prop type & grid tile object (should really only set on init) */
    bool hasInitialised = false;
    public void Initialise(PropTypes _type, List<Tile> _tiles, PropRotation _rotation = PropRotation.FACING_FRONT, bool doSpawnAnim = true)
    {
        if (_tiles.Count == 0)
        {
            Debug.LogError("Failed to init prop! No tiles have been passed for us to occupy!");
            Destroy(this.gameObject);
            return;
        }

        gridTile = _tiles; //_tiles[0] should be our origin. The others can be in any order!
        propType = _type;

        childObject = gameObject.transform.GetChild(0).gameObject;
        ourSprite = childObject.GetComponent<SpriteRenderer>();
        ourCollider = GetComponent<PolygonCollider2D>();

        if (!doSpawnAnim) GetComponent<Animator>().speed = 9999;

        hasInitialised = true;
        spriteSet = PropProperties.GetSpriteSet(propType);

        //Try and apply ourselves as the prop to our passed tile(s)
        for (int i = 0; i < gridTile.Count; i++)
        {
            gridTile[i].SetProp(null, true, false); //We have to force animations off, else the coroutine time throws off the execution order. TODO: nicer fix?
        }
        for (int i = 0; i < gridTile.Count; i++)
        {
            if (!gridTile[i].SetProp(this))
            {
                Debug.LogWarning("Failed to init prop! Most likely dumb user has tried to decorate an unoccupied tile.");
                foreach (Tile aTile in gridTile) aTile.SetProp(null);
                Destroy(this.gameObject);
                return;
            }
            if (i == 0) gridTile[i].SetPropOrigin();
        }

        //Set rotation-based sprite/collider and sorting
        SetRotation(_rotation);
        int offset = (int)(LevelManager.Instance.Grid.GridTileDims.y - _tiles[0].GridPos.y);
        ourSprite.sortingOrder = (5 * offset) + offset + TileProperties.GetZOffset(_tiles[0].Occupier.Type) + PropProperties.GetZOffset(propType) + 3;

        //For neatness in the editor, parent to the grid
        this.gameObject.transform.parent = LevelManager.Instance.Grid.gameObject.transform;

        //Remove touchable layer if out of editor
        if (!LevelManager.Instance.IsInEditor)
        {
            gameObject.layer = 0;
            gameObject.transform.GetChild(0).gameObject.layer = 0;

            //If scriptable object, try to assign script
            if (PropProperties.IsScriptedObject(propType))
            {
                try
                {
                    string componentType = PropProperties.GetScriptClassName(propType);
                    gameObject.AddComponent(System.Type.GetType(componentType));
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Failed to assign script to scripted object!");
                    Debug.LogError(e.Message);
                }
            }
        }
    }
    public void Initialise(PropTypes _type, Tile _tile, PropRotation _rotation = PropRotation.FACING_FRONT, bool doSpawnAnim = true)
    {
        List<Tile> tileList = new List<Tile>();
        tileList.Add(_tile);
        Initialise(_type, tileList, _rotation, doSpawnAnim);
    }

    /* This should only be called if you know what you're doing :) */
    public void DestroyMe(bool doAnimation = true)
    {
        if (!doAnimation)
        {
            ActualDestruction();
            return;
        }
        GetComponent<Animator>().speed = 1;
        GetComponent<Animator>().SetBool("isDespawning", true);
    }
    protected void ActualDestruction()
    {
        foreach (Tile aTile in gridTile) aTile.SetProp(null, false);
        Destroy(this.gameObject); //https://i.kym-cdn.com/entries/icons/mobile/000/028/731/cover2.jpg
    }

    /* Rotate the prop */
    public void RotateMe()
    {
        switch (propRotation)
        {
            case PropRotation.FACING_LEFT:
                SetRotation(PropRotation.FACING_FRONT);
                break;
            case PropRotation.FACING_FRONT:
                SetRotation(PropRotation.FACING_RIGHT);
                break;
            case PropRotation.FACING_RIGHT:
                SetRotation(PropRotation.FACING_BACK);
                break;
            case PropRotation.FACING_BACK:
                SetRotation(PropRotation.FACING_LEFT);
                break;
        }
    }

    /* Set specific rotation */
    public void SetRotation(PropRotation rotation)
    {
        SpriteBounds propBounds = null;
        if (ourCollider) ourCollider.enabled = false;

        //Set rotation (if invalid try next)
        propRotation = rotation;
        switch (propRotation)
        {
            case PropRotation.FACING_LEFT:
                if (spriteSet.leftFacing.sprite == null)
                {
                    RotateMe();
                    return;
                }
                ourSprite.sprite = spriteSet.leftFacing.sprite;
                propBounds = spriteSet.leftFacing.bounds;
                break;
            case PropRotation.FACING_FRONT:
                if (spriteSet.frontFacing.sprite == null)
                {
                    RotateMe();
                    return;
                }
                ourSprite.sprite = spriteSet.frontFacing.sprite;
                propBounds = spriteSet.frontFacing.bounds;
                break;
            case PropRotation.FACING_RIGHT:
                if (spriteSet.rightFacing.sprite == null)
                {
                    RotateMe();
                    return;
                }
                ourSprite.sprite = spriteSet.rightFacing.sprite;
                propBounds = spriteSet.rightFacing.bounds;
                break;
            case PropRotation.FACING_BACK:
                if (spriteSet.backFacing.sprite == null)
                {
                    RotateMe();
                    return;
                }
                ourSprite.sprite = spriteSet.backFacing.sprite;
                propBounds = spriteSet.backFacing.bounds;
                break;
        }

        //Set collider
        if (!ourCollider) return;
        List<Vector2> colliderPoints = new List<Vector2>(); //All values should be /200 because of sprite pixels to unity units conversion
        colliderPoints.Add(new Vector2((float)propBounds.from_origin_west / 200.0f, (float)propBounds.from_origin_north / 200.0f));
        colliderPoints.Add(new Vector2(-((float)propBounds.from_origin_east / 200.0f), (float)propBounds.from_origin_north / 200.0f));
        colliderPoints.Add(new Vector2(-((float)propBounds.from_origin_east / 200.0f), -((float)propBounds.from_origin_south / 200.0f)));
        colliderPoints.Add(new Vector2((float)propBounds.from_origin_west / 200.0f, -((float)propBounds.from_origin_south / 200.0f)));
        ourCollider.pathCount = 1;
        ourCollider.SetPath(0, colliderPoints.ToArray());
        ourCollider.enabled = true;
    }

    /* Allow interaction in editor */
    bool wasTouchedLastFrame = false;
    bool hasResetAnimSpeed = false;
    private void Update()
    {
        if (hasInitialised && !hasResetAnimSpeed)
        {
            GetComponent<Animator>().speed = 1;
            hasResetAnimSpeed = true;
        }

        if (!EnableRotationOnTouch) return;
        if (!LevelManager.Instance.IsInEditor) return;
        if (LevelEditor.Instance.Mode == LevelEditorMode.DELETING_PROPS) return; //Don't rotate when being touched for deletion

        if (TouchManager.Instance.IsTouched(childObject)) wasTouchedLastFrame = true;
        else if (wasTouchedLastFrame)
        {
            RotateMe();
            wasTouchedLastFrame = false;
        }
    }

    private static Dictionary<PropRotation, PropRotation> oppositeRotation = new Dictionary<PropRotation, PropRotation>()
    {
        {PropRotation.FACING_FRONT, PropRotation.FACING_BACK},
        {PropRotation.FACING_LEFT, PropRotation.FACING_RIGHT},
        {PropRotation.FACING_RIGHT, PropRotation.FACING_LEFT},
        {PropRotation.FACING_BACK, PropRotation.FACING_FRONT},
    };
    public static PropRotation GetOppositeRotation(PropRotation rotation) { return oppositeRotation[rotation]; }
}

public enum PropRotation
{
    FACING_FRONT,
    FACING_LEFT,
    FACING_RIGHT,
    FACING_BACK,
}