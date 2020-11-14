/*
 * *************************************************************
 * PLEASE DO NOT EDIT THIS SCRIPT WITHOUT CONTACTING THE AUTHORS
 * *************************************************************
 * 
 * An object within the world placed on the grid.
 * In editor this is spawned from LevelEditorComponent.
 * 
 * AUTHORS:
 *  - Matt Filer
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelTileEntity : MonoBehaviour
{
    private Tile gridTile;                                           //The tile we occupy
    private TileTypes tileType = (TileTypes)0;                       //Defaults to type 0 with placeholder sprite - call SetTileType before using!
    public TileTypes Type { get { return tileType; } }
    public int RenderOrder { get { return ourSprite.sortingOrder; } }

    private TileSprites spriteSet;
    private GameObject invalidMarker;
    private GameObject childObject;
    private SpriteRenderer ourSprite;
    private BoxCollider2D ourCollider;

    /* Set the tile type & grid tile object (should really only set on init) */
    bool hasInitialised = false;
    public void Initialise(TileTypes _type, Tile _tile, bool doSpawnAnim = true, bool refreshAllSprites = false)
    {
        gridTile = _tile;
        tileType = _type;

        childObject = gameObject.transform.GetChild(0).gameObject;
        ourSprite = childObject.GetComponent<SpriteRenderer>();
        ourCollider = childObject.GetComponent<BoxCollider2D>();

        if (!doSpawnAnim) GetComponent<Animator>().speed = 9999;

        hasInitialised = true;
        spriteSet = TileProperties.GetSpriteSet(tileType);

        //Try and occupy the tile we were given, this shouldn't really ever fail
        if (!gridTile.SetOccupied(this))
        {
            Debug.LogWarning("Failed to init tile! May be an issue with SetOccupied in grid.");
            Destroy(this.gameObject);
            return;
        }

        //Set tags for flocking AI
        if (!TileProperties.IsPathable(_type)) gameObject.layer = 30; //"Obstacle"
        else gameObject.layer = 0; //"Default"

        //Enable collision for walls
        ourCollider.enabled = (TileProperties.GetVariant(_type) == TileVariant.WALL);

        //For neatness in the editor, parent to the grid
        this.gameObject.transform.parent = LevelManager.Instance.Grid.gameObject.transform;

        //Refresh sprites
        if (refreshAllSprites)
        {
            for (int i = 0; i < LevelManager.Instance.Grid.AllTiles.Count; i++)
            {
                if (LevelManager.Instance.Grid.AllTiles[i].IsOccupied && LevelManager.Instance.Grid.AllTiles[i].Occupier.Type == tileType)
                {
                    LevelManager.Instance.Grid.AllTiles[i].Occupier.RefreshSprite();
                }
            }
        }
        else
        {
            RefreshSprite();
        }

        if (LevelManager.Instance.IsInEditor)
        {
            //If in editor, spawn an invalid marker for UI use
            invalidMarker = Instantiate(LevelEditor.Instance.InvalidTileMarker, this.gameObject.transform.position, Quaternion.identity) as GameObject;
            invalidMarker.transform.parent = this.gameObject.transform;
            invalidMarker.SetActive(false);
        }
        else
        {
            //If out of editor, remove touchable from our child - it's not needed
            gameObject.transform.GetChild(0).gameObject.layer = gameObject.layer;
        }
    }
    bool hasResetAnimSpeed = false;
    private void Update()
    {
        if (hasInitialised && !hasResetAnimSpeed)
        {
            GetComponent<Animator>().speed = 1;
            hasResetAnimSpeed = true;
        }
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
        Destroy(this.gameObject); //https://i.kym-cdn.com/entries/icons/mobile/000/028/731/cover2.jpg
    }

    /* Show invalid marker */
    public void ShowInvalidMarker(bool shouldShow)
    {
        if (!LevelManager.Instance.IsInEditor) return;
        invalidMarker.SetActive(shouldShow);
    }

    /* Refresh the sprite based on the context of the grid */
    int fixedFillerInt = -1;
    int fixedVerticalInt = -1;
    int fixedHorizontalInt = -1;
    public void RefreshSprite()
    {
        Tile[] myNeighbours = LevelManager.Instance.Grid.GetTileNeighbours(gridTile);

        int zOffset = (int)(LevelManager.Instance.Grid.GridTileDims.y - gridTile.GridPos.y);
        ourSprite.sortingOrder = (5 * zOffset) + zOffset + TileProperties.GetZOffset(tileType);

        switch (TileProperties.GetVariant(tileType))
        {
            case TileVariant.FLOOR:
                if (NoNeighbour(TileNeighbour.TOP, myNeighbours) && NoNeighbour(TileNeighbour.LEFT, myNeighbours) &&
                    SameNeighbour(TileNeighbour.BOTTOM, myNeighbours) && SameNeighbour(TileNeighbour.RIGHT, myNeighbours))
                {
                    ourSprite.sprite = spriteSet.floor_cornerNorthWest;
                }
                else if (NoNeighbour(TileNeighbour.TOP, myNeighbours) && NoNeighbour(TileNeighbour.RIGHT, myNeighbours) &&
                    SameNeighbour(TileNeighbour.BOTTOM, myNeighbours) && SameNeighbour(TileNeighbour.LEFT, myNeighbours))
                {
                    ourSprite.sprite = spriteSet.floor_cornerNorthEast;
                }
                else if (NoNeighbour(TileNeighbour.BOTTOM, myNeighbours) && NoNeighbour(TileNeighbour.LEFT, myNeighbours) &&
                    SameNeighbour(TileNeighbour.TOP, myNeighbours) && SameNeighbour(TileNeighbour.RIGHT, myNeighbours))
                {
                    ourSprite.sprite = spriteSet.floor_cornerSouthWest;
                }
                else if (NoNeighbour(TileNeighbour.BOTTOM, myNeighbours) && NoNeighbour(TileNeighbour.RIGHT, myNeighbours) &&
                    SameNeighbour(TileNeighbour.TOP, myNeighbours) && SameNeighbour(TileNeighbour.LEFT, myNeighbours))
                {
                    ourSprite.sprite = spriteSet.floor_cornerSouthEast;
                }
                else if (NoNeighbour(TileNeighbour.LEFT, myNeighbours) && SameNeighbour(TileNeighbour.RIGHT, myNeighbours))
                {
                    ourSprite.sprite = spriteSet.floor_edgeWest;
                }
                else if (NoNeighbour(TileNeighbour.RIGHT, myNeighbours) && SameNeighbour(TileNeighbour.LEFT, myNeighbours))
                {
                    ourSprite.sprite = spriteSet.floor_edgeEast;
                }
                else if (NoNeighbour(TileNeighbour.TOP, myNeighbours) && SameNeighbour(TileNeighbour.BOTTOM, myNeighbours))
                {
                    ourSprite.sprite = spriteSet.floor_edgeNorth;
                }
                else if (NoNeighbour(TileNeighbour.BOTTOM, myNeighbours) && SameNeighbour(TileNeighbour.TOP, myNeighbours))
                {
                    ourSprite.sprite = spriteSet.floor_edgeSouth;
                }
                else
                {
                    if (fixedFillerInt == -1) fixedFillerInt = Random.Range(0, spriteSet.floor_fillers.Count - 1);
                    ourSprite.sprite = spriteSet.floor_fillers[fixedFillerInt];
                    ourSprite.sortingOrder -= 1;
                }
                break;
            case TileVariant.WALL:
                if (SameNeighbour(TileNeighbour.BOTTOM, myNeighbours))
                {
                    if (fixedVerticalInt == -1) fixedVerticalInt = Random.Range(0, spriteSet.wall_verticals.Count - 1);
                    ourSprite.sprite = spriteSet.wall_verticals[fixedVerticalInt];
                }
                else
                {
                    if (fixedHorizontalInt == -1) fixedHorizontalInt = Random.Range(0, spriteSet.wall_horizontals.Count - 1);
                    ourSprite.sprite = spriteSet.wall_horizontals[fixedHorizontalInt];
                }
                break;
        }
    }
    private bool SameNeighbour(TileNeighbour neighbour, Tile[] myNeighbours)
    {
        return myNeighbours[(int)neighbour] != null && myNeighbours[(int)neighbour].IsOccupied && myNeighbours[(int)neighbour].Occupier.Type == tileType;
    }
    private bool NoNeighbour(TileNeighbour neighbour, Tile[] myNeighbours)
    {
        return myNeighbours[(int)neighbour] == null || !myNeighbours[(int)neighbour].IsOccupied || (myNeighbours[(int)neighbour].IsOccupied && myNeighbours[(int)neighbour].Occupier.Type != tileType);
    }
}