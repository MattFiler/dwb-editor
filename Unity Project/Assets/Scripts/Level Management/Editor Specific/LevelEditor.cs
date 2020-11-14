/*
 * *************************************************************
 * PLEASE DO NOT EDIT THIS SCRIPT WITHOUT CONTACTING THE AUTHORS
 * *************************************************************
 * 
 * The core of the level editor, including functionality for creating and saving levels.
 * Level loading is performed by LevelManager, as this functionality is not editor specific.
 * 
 * AUTHORS:
 *  - Matt Filer
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LevelEditor : MonoSingleton<LevelEditor>
{
    private LevelEditorMode currentEditorMode = (LevelEditorMode)0;
    public LevelEditorMode Mode { get { return currentEditorMode; } }
    private TileTypes currentBrushType = (TileTypes)0;
    public TileTypes BrushType { get { return currentBrushType; } }

    [SerializeField] private GameObject invalidMarker;
    public GameObject InvalidTileMarker { get { return invalidMarker; } }

    /* Set in-editor state */
    private void Start()
    {
        LevelManager.Instance.IsInEditor = true;
        currentBrushType = (TileTypes)Enum.GetValues(typeof(TileTypes)).GetValue(0);
    }

    /* Create a new level instance */
    public int CreateLevel(string name)
    {
        //Pull the existing metadata, and work out our new GUID
        List<LevelMetadata> currentLevels = LevelManager.Instance.LevelMetadata;
        int guid = LevelManager.Instance.HistoricalMaxGUID + 1;

        //Add our new level metadata
        currentLevels.Add(new LevelMetadata(name, guid));

        //Write out our updated info
        BinaryWriter bankWriter = new BinaryWriter(File.OpenWrite(Application.streamingAssetsPath + "/LEVELS_MANIFEST.dwb"));
        bankWriter.BaseStream.SetLength(0);
        bankWriter.Write(guid);
        bankWriter.Write(currentLevels.Count);
        List<LinkedLevel> linkedLevels = new List<LinkedLevel>();
        for (int i = 0; i < currentLevels.Count; i++)
        {
            bankWriter.Write(currentLevels[i].levelName);
            bankWriter.Write(currentLevels[i].levelGUID);
            if (currentLevels[i].nextLevelGUID != -1) linkedLevels.Add(new LinkedLevel(currentLevels[i].levelGUID, currentLevels[i].nextLevelGUID, currentLevels[i].isFirstCampaignLevel, currentLevels[i].isLastCampaignLevel));
        }
        bankWriter.Write(linkedLevels.Count);
        for (int i = 0; i < linkedLevels.Count; i++)
        {
            bankWriter.Write(linkedLevels[i].GUID_1);
            bankWriter.Write(linkedLevels[i].GUID_2);
            bankWriter.Write(linkedLevels[i].IS_FIRST);
            bankWriter.Write(linkedLevels[i].IS_LAST);
        }
        bankWriter.Close();

        return guid;
    }

    /* Save the current level */
    public void SaveLevel(byte[] levelScreenshot = null, int maxGoonCount = 75, int hazardCount = 0, AbilityCounts abilityCounts = null)
    {
#if UNITY_WEBGL
        Debug.LogError("REQUESTED TO SAVE LEVEL IN WEBGL - THIS IS NOT SUPPORTED!");
        return;
#endif
        Debug.Log("Saving level...");
        if (!LevelManager.Instance.CurrentMeta.isLoaded)
        {
            Debug.LogError("Cannot save unloaded level!");
            return;
        }
        if (abilityCounts == null) abilityCounts = new AbilityCounts(5, 5, 5, 5);

        //Work out what tiles to write
        List<Tile> tiles = new List<Tile>();
        for (int i = 0; i < LevelManager.Instance.Grid.AllTiles.Count; i++)
            if (LevelManager.Instance.Grid.AllTiles[i].IsOccupied)
                tiles.Add(LevelManager.Instance.Grid.AllTiles[i]);

        //Write data
        BinaryWriter writer = new BinaryWriter(File.OpenWrite(Application.streamingAssetsPath + "/LEVELS/" + LevelManager.Instance.CurrentMeta.levelName + ".dwb"));
        writer.BaseStream.SetLength(0);

        /* Header Info */
        writer.Write(LevelFileVersion.VERSION_NUM);
        writer.Write(tiles.Count);
        writer.Write((Int16)LevelManager.Instance.Grid.GridTileDims.x);
        writer.Write((Int16)LevelManager.Instance.Grid.GridTileDims.y);

        /* Tile Occupier Info */
        int occupiedCount = 0;
        int occCountPos = (int)writer.BaseStream.Position;
        writer.Write(0);
        for (int i = 0; i < tiles.Count; i++)
        {
            if (!tiles[i].IsOccupied) continue;
            writer.Write(tiles[i].Index);
            writer.Write((Int16)tiles[i].Occupier.Type);
            occupiedCount++;
        }
        int occEndPos = (int)writer.BaseStream.Position;
        writer.BaseStream.Position = occCountPos;
        writer.Write(occupiedCount);
        writer.BaseStream.Position = occEndPos;

        /* Tile Prop Info */
        int propCount = 0;
        int prpCountPos = (int)writer.BaseStream.Position;
        writer.Write(0);
        for (int i = 0; i < tiles.Count; i++)
        {
            if (!tiles[i].HasProp && !tiles[i].HasPropOrigin) continue;
            writer.Write((Int16)tiles[i].Prop.Type);
            writer.Write((Int16)tiles[i].Prop.Rotation);
            writer.Write(tiles[i].Prop.Tiles.Count);
            for (int x = 0; x < tiles[i].Prop.Tiles.Count; x++)
            {
                writer.Write(tiles[i].Prop.Tiles[x].Index);
            }
            propCount++;
        }
        int prpEndPos = (int)writer.BaseStream.Position;
        writer.BaseStream.Position = prpCountPos;
        writer.Write(propCount);
        writer.BaseStream.Position = prpEndPos;

        /* Misc Metadata */
        writer.Write(maxGoonCount);
        writer.Write(hazardCount);
        writer.Write(abilityCounts.Rally);
        writer.Write(abilityCounts.Medical);
        writer.Write(abilityCounts.FirePhone);
        writer.Write(abilityCounts.Marshal);

        writer.Close();

        //Save screenshot if generated one
        if (levelScreenshot != null)
        {
            File.WriteAllBytes(Application.streamingAssetsPath + "/LEVELS/" + LevelManager.Instance.CurrentMeta.levelName + ".png", levelScreenshot);
        }

        Debug.Log("Saved level " + LevelManager.Instance.CurrentMeta.levelName + " with " + tiles.Count + " tiles and " + hazardCount + " hazards, which is GUID " + LevelManager.Instance.CurrentMeta.levelGUID + ".");
    }

    /* Sets the active editor mode */
    public void SetEditorMode(LevelEditorMode mode)
    {
        currentEditorMode = mode;
    }

    /* Set the active brush for painting */
    public void SetActiveBrush(TileTypes brush)
    {
        currentBrushType = brush;
    }

    /* Handle level tile painting in editor mode */
    Vector3 touchPos = new Vector3(0, 0, 0);
    private void Update()
    {
        switch (Mode)
        {
            //Tile instantiation mode
            case LevelEditorMode.PAINTING_TILES:
                if (TouchManager.Instance.IsTouched(LevelManager.Instance.Grid.gameObject, ref touchPos))
                {
                    Tile destinationTile = LevelManager.Instance.Grid.GetTileAtPosition(touchPos);
                    if (destinationTile == null) break;
                    //if (destinationTile.IsOccupied && destinationTile.Occupier.Type == currentBrushType) break;
                    if (destinationTile.IsOccupied) break;

                    GameObject inWorldObject = Instantiate(LevelManager.Instance.WorldTileObject, destinationTile.Position, Quaternion.identity) as GameObject;
                    inWorldObject.GetComponent<LevelTileEntity>().Initialise(currentBrushType, destinationTile, true, true);
                }
                break;

            //Prop deletion mode
            case LevelEditorMode.DELETING_PROPS:
                if (TouchManager.Instance.IsTouched(LevelManager.Instance.Grid.gameObject, ref touchPos))
                {
                    Tile destinationTile = LevelManager.Instance.Grid.GetTileAtPosition(touchPos);
                    if (destinationTile == null) break;

                    destinationTile.SetProp(null);
                }
                break;

            //Tile deletion mode
            case LevelEditorMode.DELETING_TILES:
                if (TouchManager.Instance.IsTouched(LevelManager.Instance.Grid.gameObject, ref touchPos))
                {
                    Tile destinationTile = LevelManager.Instance.Grid.GetTileAtPosition(touchPos);
                    if (destinationTile == null) break;

                    destinationTile.SetOccupied(null, true, true);
                }
                break;
        }
    }
}
