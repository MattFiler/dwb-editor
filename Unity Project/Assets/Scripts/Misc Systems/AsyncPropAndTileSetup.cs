/*
 * *************************************************************
 * PLEASE DO NOT EDIT THIS SCRIPT WITHOUT CONTACTING THE AUTHORS
 * *************************************************************
 * 
 * This is the new tile & prop property loading system required to function on WebGL.
 * This script needs to be called before accessing any TileProperties or PropProperties values.
 * Also as an aside, as this is all async ".Loaded" should be checked on the Properties classes before using them!
 * 
 * AUTHORS:
 *  - Matt Filer
*/

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class AsyncPropAndTileSetup : MonoSingleton<AsyncPropAndTileSetup>
{
    /* Trigger async load of prop/tile data if not loaded already */
    private void Start()
    {
        LoadConfigs();
    }
    public void LoadConfigs()
    {
        if (!TileProperties.Loaded) StartCoroutine(FileLoader.Instance.LoadAsBytes(Application.streamingAssetsPath + "/TILEDATA.DWB", LoadTileConfig));
        if (!PropProperties.Loaded) StartCoroutine(FileLoader.Instance.LoadAsBytes(Application.streamingAssetsPath + "/PROPDATA.DWB", LoadPropConfig));
    }

    /* LOAD TILE DATA */
    private void LoadTileConfig(byte[] content)
    {
        BinaryReader reader = new BinaryReader(new MemoryStream(content));
        StartCoroutine(LoadTileConfigAsync(reader));
    }
    private MemoryStream tempMemStream = null;
    private IEnumerator LoadTileConfigAsync(BinaryReader reader)
    {
        List<TileData> tileData = new List<TileData>();
        int versionNum = reader.ReadInt32();
        int entryCount = reader.ReadInt32();
        for (int i = 0; i < entryCount; i++)
        {
            TileData thisData = new TileData();
            bool loadedOK = true;

            thisData.tileID = reader.ReadInt32();
            thisData.tileName = (TileTypes)thisData.tileID;
            thisData.tileNameString = reader.ReadString();
            if (thisData.tileName.ToString() != thisData.tileNameString) loadedOK = false;
            thisData.tileDesc = reader.ReadString();
            thisData.isPathable = reader.ReadBoolean();
            thisData.isFlammable = reader.ReadBoolean();
            thisData.allowProps = reader.ReadBoolean();
            thisData.tileType = (TileVariant)reader.ReadInt16();
            thisData.tileUseage = (TileUseage)reader.ReadInt16();
            thisData.zOffset = reader.ReadInt16();
            thisData.hideInEditor = reader.ReadBoolean();

            if (!loadedOK) continue;

            string folderPath = "TILES/" + thisData.tileName.ToString() + "/";
            thisData.spriteSet.editorUI = Resources.Load<Sprite>(folderPath + "EDITOR_UI");
            if (thisData.tileType == TileVariant.FLOOR)
            {
                thisData.spriteSet.floor_cornerNorthEast = Resources.Load<Sprite>(folderPath + "CORNER_NORTH_EAST");
                thisData.spriteSet.floor_cornerSouthEast = Resources.Load<Sprite>(folderPath + "CORNER_SOUTH_EAST");
                thisData.spriteSet.floor_cornerNorthWest = Resources.Load<Sprite>(folderPath + "CORNER_NORTH_WEST");
                thisData.spriteSet.floor_cornerSouthWest = Resources.Load<Sprite>(folderPath + "CORNER_SOUTH_WEST");

                thisData.spriteSet.floor_edgeNorth = Resources.Load<Sprite>(folderPath + "EDGE_NORTH");
                thisData.spriteSet.floor_edgeEast = Resources.Load<Sprite>(folderPath + "EDGE_EAST");
                thisData.spriteSet.floor_edgeSouth = Resources.Load<Sprite>(folderPath + "EDGE_SOUTH");
                thisData.spriteSet.floor_edgeWest = Resources.Load<Sprite>(folderPath + "EDGE_WEST");

                tempMemStream = null;
                StartCoroutine(FileLoader.Instance.LoadAsBytes(Application.streamingAssetsPath + "/TILECONFIGS/" + thisData.tileName.ToString() + ".DWB", LoadTileConfigAsyncLoader));
                while (tempMemStream == null || !tempMemStream.CanRead)
                    yield return new WaitForEndOfFrame();

                BinaryReader reader2 = new BinaryReader(tempMemStream);
                reader2.BaseStream.Position += 4;
                int fillerCount = reader2.ReadInt32();
                reader2.Close();

                for (int x = 0; x < fillerCount; x++)
                {
                    thisData.spriteSet.floor_fillers.Add(Resources.Load<Sprite>(folderPath + "FILL_" + x));
                }
            }
            else if (thisData.tileType == TileVariant.WALL)
            {
                tempMemStream = null;
                StartCoroutine(FileLoader.Instance.LoadAsBytes(Application.streamingAssetsPath + "/TILECONFIGS/" + thisData.tileName.ToString() + ".DWB", LoadTileConfigAsyncLoader));
                while (tempMemStream == null || !tempMemStream.CanRead)
                    yield return new WaitForEndOfFrame();

                BinaryReader reader2 = new BinaryReader(tempMemStream);
                reader2.BaseStream.Position += 4;
                int verticalCount = reader2.ReadInt32();
                int horizontalCount = reader2.ReadInt32();
                reader2.Close();

                for (int x = 0; x < verticalCount; x++)
                {
                    thisData.spriteSet.wall_verticals.Add(Resources.Load<Sprite>(folderPath + "VERTICAL_" + x));
                }
                for (int x = 0; x < horizontalCount; x++)
                {
                    thisData.spriteSet.wall_horizontals.Add(Resources.Load<Sprite>(folderPath + "HORIZONTAL_" + x));
                }
            }

            tileData.Add(thisData);
        }
        reader.Close();

        TileProperties.SetData(tileData);
    }
    private void LoadTileConfigAsyncLoader(byte[] content)
    {
        tempMemStream = new MemoryStream(content);
    }

    /* LOAD PROP DATA */
    private void LoadPropConfig(byte[] content)
    {
        BinaryReader reader = new BinaryReader(new MemoryStream(content));
        StartCoroutine(LoadPropConfigAsync(reader));
    }
    private SpriteBounds tempBounds = null;
    private IEnumerator LoadPropConfigAsync(BinaryReader reader)
    {
        List<PropData> propData = new List<PropData>();
        int versionNum = reader.ReadInt32();
        int entryCount = reader.ReadInt32();
        for (int i = 0; i < entryCount; i++)
        {
            PropData thisData = new PropData();
            bool loadedOK = true;

            thisData.propID = reader.ReadInt32();
            thisData.propName = (PropTypes)thisData.propID;
            thisData.propNameString = reader.ReadString();
            if (thisData.propName.ToString() != thisData.propNameString) loadedOK = false;
            thisData.propDesc = reader.ReadString();
            thisData.isWaypoint = reader.ReadBoolean();
            if (thisData.isWaypoint)
            {
                thisData.waypointFor = (PropWaypointUser)reader.ReadInt16();
                thisData.waypointType = (PropWaypointType)reader.ReadInt16();
            }
            thisData.isScripted = reader.ReadBoolean();
            if (thisData.isScripted)
            {
                thisData.scriptName = reader.ReadString();
            }
            thisData.isPOI = reader.ReadBoolean();
            if (thisData.isPOI)
            {
                thisData.poiType = (PoiType)reader.ReadInt16();
                thisData.poiGoonCount = reader.ReadInt32();
            }
            thisData.placement = (reader.ReadBoolean()) ? PropPlacement.INTERIOR : PropPlacement.EXTERIOR;
            thisData.isUnpathable = reader.ReadBoolean();
            thisData.hideInEditor = reader.ReadBoolean();

            string folderPath = "PROPS/" + thisData.propName.ToString() + "/";
            string streamingPath = "/PROPCONFIGS/" + thisData.propName.ToString() + "/";
            thisData.spriteSet.frontFacing.sprite = Resources.Load<Sprite>(folderPath + "FRONT_FACING");
            if (thisData.spriteSet.frontFacing.sprite != null)
            {
                tempBounds = null;
                StartCoroutine(GetSpriteBounds(streamingPath + "FRONT_FACING"));
                while (tempBounds == null)
                    yield return new WaitForEndOfFrame();
                thisData.spriteSet.frontFacing.bounds = tempBounds;
            }
            thisData.spriteSet.leftFacing.sprite = Resources.Load<Sprite>(folderPath + "LEFT_FACING");
            if (thisData.spriteSet.leftFacing.sprite != null)
            {
                tempBounds = null;
                StartCoroutine(GetSpriteBounds(streamingPath + "LEFT_FACING"));
                while (tempBounds == null)
                    yield return new WaitForEndOfFrame();
                thisData.spriteSet.leftFacing.bounds = tempBounds;
            }
            thisData.spriteSet.rightFacing.sprite = Resources.Load<Sprite>(folderPath + "RIGHT_FACING");
            if (thisData.spriteSet.rightFacing.sprite != null)
            {
                tempBounds = null;
                StartCoroutine(GetSpriteBounds(streamingPath + "RIGHT_FACING"));
                while (tempBounds == null)
                    yield return new WaitForEndOfFrame();
                thisData.spriteSet.rightFacing.bounds = tempBounds;
            }
            thisData.spriteSet.backFacing.sprite = Resources.Load<Sprite>(folderPath + "BACK_FACING");
            if (thisData.spriteSet.backFacing.sprite != null)
            {
                tempBounds = null;
                StartCoroutine(GetSpriteBounds(streamingPath + "BACK_FACING"));
                while (tempBounds == null)
                    yield return new WaitForEndOfFrame();
                thisData.spriteSet.backFacing.bounds = tempBounds;
            }
            thisData.spriteSet.editorUISprite = Resources.Load<Sprite>(folderPath + "EDITOR_UI");
            tempBounds = null;
            StartCoroutine(GetSpriteBounds(streamingPath + thisData.propName.ToString()));
            while (tempBounds == null)
                yield return new WaitForEndOfFrame();
            thisData.spriteSet.completeSpriteBounds = tempBounds;

            thisData.zOffset = reader.ReadInt16();

            if (loadedOK) propData.Add(thisData);
        }
        reader.Close();

        PropProperties.SetData(propData);
    }
    private IEnumerator GetSpriteBounds(string path)
    {
        tempBounds = null;
        byte[] content = { };
        string filePath = Application.streamingAssetsPath + path + ".dwb";
        if (filePath.Contains("://") || filePath.Contains(":///"))
        {
            WWW www = new WWW(filePath);
            yield return www;

            while (!www.isDone)
                yield return new WaitForEndOfFrame();

            content = www.bytes;
        }
        else
        {
            content = File.ReadAllBytes(filePath);
        }

        SpriteBounds bounds = new SpriteBounds();
        BinaryReader reader = new BinaryReader(new MemoryStream(content));
        bounds.from_origin_north = reader.ReadInt32();
        bounds.from_origin_east = reader.ReadInt32();
        bounds.from_origin_south = reader.ReadInt32();
        bounds.from_origin_west = reader.ReadInt32();
        //Older config files don't have this, so skip if they don't!
        if (reader.BaseStream.Length == 32)
        {
            bounds.tile_excess_north = reader.ReadSingle();
            bounds.tile_excess_east = reader.ReadSingle();
            bounds.tile_excess_south = reader.ReadSingle();
            bounds.tile_excess_west = reader.ReadSingle();
        }
        reader.Close();
        tempBounds = bounds;
    }
}
