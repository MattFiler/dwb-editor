/*
 * *************************************************************
 * PLEASE DO NOT EDIT THIS SCRIPT WITHOUT CONTACTING THE AUTHORS
 * *************************************************************
 * 
 * Functionality to get tile properties.
 * This loads data produced by TileManager. 
 * 
 * AUTHORS:
 *  - Matt Filer
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/* A representation of stored tile data (should match TileManager really, with some exceptions) */
public class TileData
{
    public int tileID;
    public TileTypes tileName;
    public string tileNameString;
    public string tileDesc;
    public bool isPathable;
    public bool isFlammable;
    public bool allowProps;
    public bool hideInEditor;
    public int zOffset;
    public TileVariant tileType;
    public TileUseage tileUseage;
    public TileSprites spriteSet = new TileSprites();
}
public class TileSprites
{
    public Sprite editorUI;

    /* FOR FLOOR TILES */
    public Sprite floor_cornerNorthEast;
    public Sprite floor_cornerSouthEast;
    public Sprite floor_cornerNorthWest;
    public Sprite floor_cornerSouthWest;
                  
    public Sprite floor_edgeNorth;
    public Sprite floor_edgeEast;
    public Sprite floor_edgeSouth;
    public Sprite floor_edgeWest;

    public List<Sprite> floor_fillers = new List<Sprite>();

    /* FOR WALL TILES */
    public List<Sprite> wall_verticals = new List<Sprite>();
    public List<Sprite> wall_horizontals = new List<Sprite>();
}

public class TileProperties
{
    private static List<TileData> tileData = new List<TileData>();
    private static bool hasLoaded = false;
    public static bool Loaded { get { return hasLoaded; } }

    /* ONLY TO BE USED BY AsyncPropAndTileSetup!!!! */
    public static void SetData(List<TileData> data)
    {
        tileData = data;
        hasLoaded = true;
    }

    /* Get the description of a tile */
    public static string GetDescription(TileTypes tile)
    {
        TileData thisTile = GetTile(tile);
        if (thisTile == null) return "";
        return thisTile.tileDesc;
    }

    /* Get the sprite of a tile */
    public static TileSprites GetSpriteSet(TileTypes tile)
    {
        TileData thisTile = GetTile(tile);
        if (thisTile == null) return null;
        return thisTile.spriteSet;
    }

    /* Is the tile type pathable? */
    public static bool IsPathable(TileTypes tile, List<TileTypes> validOverride = null)
    {
        TileData thisTile = GetTile(tile);
        if (thisTile == null) return false;
        if (validOverride != null && validOverride.Contains(tile)) return true;
        return thisTile.isPathable;
    }

    /* Is the tile type flammable? */
    public static bool IsFlammable(TileTypes tile)
    {
        TileData thisTile = GetTile(tile);
        if (thisTile == null) return false;
        return thisTile.isFlammable;
    }

    /* Does the tile allow props? */
    public static bool AllowsProps(TileTypes tile)
    {
        TileData thisTile = GetTile(tile);
        if (thisTile == null) return false;
        return thisTile.allowProps;
    }

    /* Get the tile variant */
    public static TileVariant GetVariant(TileTypes tile)
    {
        TileData thisTile = GetTile(tile);
        if (thisTile == null) return (TileVariant)0;
        return thisTile.tileType;
    }

    /* Get the tile location useage */
    public static TileUseage GetUseage(TileTypes tile)
    {
        TileData thisTile = GetTile(tile);
        if (thisTile == null) return (TileUseage)0;
        return thisTile.tileUseage;
    }

    /* Get the tile's z-offset */
    public static int GetZOffset(TileTypes tile)
    {
        TileData thisTile = GetTile(tile);
        if (thisTile == null) return 0;
        return thisTile.zOffset;
    }

    /* Should this tile show in the editor UI? */
    public static bool IsVisibleInEditor(TileTypes tile)
    {
        TileData thisTile = GetTile(tile);
        if (thisTile == null) return false;
        return !thisTile.hideInEditor;
    }

    /* Get a tile object from enum */
    private static TileData GetTile(TileTypes tile)
    {
        foreach (TileData aTile in tileData)
        {
            if (aTile.tileName == tile)
            {
                return aTile;
            }
        }
        Debug.LogError("Cannot find requested tile '" + tile + "' - it may have been deleted! Map should be updated.");
        return null;
    }
}
