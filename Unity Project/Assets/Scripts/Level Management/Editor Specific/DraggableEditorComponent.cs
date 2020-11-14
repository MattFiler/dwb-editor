/*
 * *************************************************************
 * PLEASE DO NOT EDIT THIS SCRIPT WITHOUT CONTACTING THE AUTHORS
 * *************************************************************
 * 
 * An object within the level editor that can be dragged into the world.
 * LevelTileEntity should be used for actual in-world objects.
 * 
 * This is for DRAGGABLE tile types only (events: e.g. a vending machine).
 * 
 * AUTHORS:
 *  - Matt Filer
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DraggableEditorComponent : MonoBehaviour
{
    private PropRotation rotationOverride = PropRotation.FACING_FRONT;
    private PropTypes objectType;
    private bool spawnedWorldObject = true;

    private SingleTouchInstance uiElementInteraction;
    private Vector3 uiElementSpawn;
    private GameObject draggableObject;

    /* Setup component on start */
    private void Start()
    {
        if (!gameObject.GetComponent<BoxCollider2D>()) gameObject.AddComponent<BoxCollider2D>();
        if (!gameObject.GetComponent<Image>()) gameObject.AddComponent<Image>();

        gameObject.layer = 8; //Must match TouchManager

        draggableObject = Instantiate(gameObject, gameObject.transform) as GameObject;
        draggableObject.GetComponent<DraggableEditorComponent>().enabled = false;
        uiElementInteraction = new SingleTouchInstance(draggableObject);
        uiElementSpawn = gameObject.transform.position;
        draggableObject.SetActive(true);
    }

    /* Check for touch input, and handle appropriately */
    bool shownInvalidMarkers = false;
    private void Update()
    {
        //Touch interaction
        if (uiElementInteraction.IsTouchDown())
        {
            draggableObject.transform.SetParent(EditorModeButtonControllers.Instance.gameObject.transform); //HACKY!
            uiElementInteraction.MoveToTouch();
            spawnedWorldObject = false;

            //Show validity markers
            if (!shownInvalidMarkers)
            {
                for (int i = 0; i < LevelManager.Instance.Grid.AllTiles.Count; i++)
                {
                    if (LevelManager.Instance.Grid.AllTiles[i].IsOccupied && !TileIsValidForUs(LevelManager.Instance.Grid.AllTiles[i]))
                    {
                        LevelManager.Instance.Grid.AllTiles[i].Occupier.ShowInvalidMarker(true);
                    }
                }
                shownInvalidMarkers = true;
            }
        }
        else
        {
            if (!spawnedWorldObject)
            {
                Vector3 inWorldPos = Camera.main.ScreenToWorldPoint(draggableObject.transform.position + new Vector3(uiElementInteraction.Rect.width/2, uiElementInteraction.Rect.height/2, 0));
                inWorldPos.z = 0;
                if (LevelManager.Instance.Grid.GetTileAtPosition(inWorldPos) == null) Destroy(this);
                List<Tile> worldTileList = LevelManager.Instance.Grid.GetNeighboursFromBounds(LevelManager.Instance.Grid.GetTileAtPosition(inWorldPos), PropProperties.GetSpriteSet(objectType).GetByRotation(rotationOverride).bounds);

                //Validity check
                bool tilesAreValid = true;
                for (int i = 0; i < worldTileList.Count; i++)
                {
                    if (!(worldTileList[i].IsOccupied && TileIsValidForUs(worldTileList[i]))) {
                        tilesAreValid = false;
                        break;
                    }
                }

                //If the tiles are valid, spawn us in them
                if (tilesAreValid)
                {
                    GameObject inWorldObject = Instantiate(LevelManager.Instance.WorldPropObject, worldTileList[0].Position, Quaternion.identity) as GameObject;
                    inWorldObject.GetComponent<LevelPropEntity>().Initialise(objectType, worldTileList, rotationOverride);
                }
                //Otherwise, we can't occupy the tile - the user must delete the existing tile
                else
                {
                    Debug.LogWarning("Tile either isn't occupied or doesn't allow props!");
                    //TODO: nice animation here
                }
                spawnedWorldObject = true;
                
                //Hide the validity markers
                for (int i = 0; i < LevelManager.Instance.Grid.AllTiles.Count; i++)
                {
                    if (LevelManager.Instance.Grid.AllTiles[i].IsOccupied) LevelManager.Instance.Grid.AllTiles[i].Occupier.ShowInvalidMarker(false);
                }
                shownInvalidMarkers = false;
            }

            //Hacky fix for new animated UI
            draggableObject.transform.SetParent(gameObject.transform);
            draggableObject.transform.position = gameObject.transform.position;
        }
    }
    private bool TileIsValidForUs(Tile worldTile) //Assumes occupied tile
    {
        return TileProperties.AllowsProps(worldTile.Occupier.Type) && //That tile must allow props
               (int)TileProperties.GetUseage(worldTile.Occupier.Type) == (int)PropProperties.GetPlacementLocation(objectType); //And it must match our intended location
    }

    /* Set prop type & rotation */
    public void Initialise(PropTypes _type, PropRotation _rot)
    {
        rotationOverride = _rot;
        objectType = _type;
        gameObject.GetComponent<Image>().sprite = PropProperties.GetSpriteSet(objectType).GetByRotation(rotationOverride).sprite;
        gameObject.SetActive((gameObject.GetComponent<Image>().sprite != null));
        if (draggableObject) draggableObject.GetComponent<DraggableEditorComponent>().Initialise(_type, _rot);
    }
}
