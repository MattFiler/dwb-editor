/*
 * *************************************************************
 * PLEASE DO NOT EDIT THIS SCRIPT WITHOUT CONTACTING THE AUTHORS
 * *************************************************************
 * 
 * Functionality to get prop properties.
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

/* A representation of stored prop data (should match TileManager really, with some exceptions) */
public class PropData
{
    public int propID;
    public PropTypes propName;
    public string propNameString;
    public string propDesc;
    public bool isWaypoint;
    public PropWaypointType waypointType = (PropWaypointType)0;
    public PropWaypointUser waypointFor = (PropWaypointUser)0;
    public bool isScripted;
    public string scriptName;
    public bool isPOI;
    public PoiType poiType = (PoiType)0;
    public int poiGoonCount;
    public PropPlacement placement = (PropPlacement)0;
    public bool isUnpathable;
    public bool hideInEditor;
    public int zOffset;
    public Sprite frontFacingSprite;
    public Sprite sideFacingSprite;
    public PropSprites spriteSet = new PropSprites();
}
public class PropSprites
{
    public PropSprite GetByRotation(PropRotation _rot)
    {
        switch (_rot)
        {
            case PropRotation.FACING_FRONT:
                return frontFacing;
            case PropRotation.FACING_LEFT:
                return leftFacing;
            case PropRotation.FACING_RIGHT:
                return rightFacing;
            case PropRotation.FACING_BACK:
                return backFacing;
            default:
                return null;
        }
    }

    public PropSprite frontFacing = new PropSprite();
    public PropSprite leftFacing = new PropSprite();
    public PropSprite rightFacing = new PropSprite();
    public PropSprite backFacing = new PropSprite();

    public Sprite editorUISprite;
    public SpriteBounds completeSpriteBounds = new SpriteBounds();
}
public class PropSprite
{
    public Sprite sprite;
    public SpriteBounds bounds;
}

public class PropProperties
{
    private static List<PropData> propData = new List<PropData>();
    private static bool hasLoaded = false;
    public static bool Loaded { get { return hasLoaded; } }

    /* ONLY TO BE USED BY AsyncPropAndTileSetup!!!! */
    public static void SetData(List<PropData> data)
    {
        propData = data;
        hasLoaded = true;
    }

    /* Get the description of a prop */
    public static string GetDescription(PropTypes type)
    {
        PropData thisTile = GetProp(type);
        if (thisTile == null) return "";
        return thisTile.propDesc;
    }

    /* Get the sprite of a tile */
    public static PropSprites GetSpriteSet(PropTypes type)
    {
        PropData thisTile = GetProp(type);
        if (thisTile == null) return null;
        return thisTile.spriteSet;
    }

    /* Is the tile a scripted object? */
    public static bool IsScriptedObject(PropTypes type)
    {
        PropData thisTile = GetProp(type);
        if (thisTile == null) return false;
        return thisTile.isScripted;
    }

    /* Get the tile's script class as string */
    public static string GetScriptClassName(PropTypes type)
    {
        PropData thisTile = GetProp(type);
        if (thisTile == null) return "";
        return thisTile.scriptName;
    }

    /* Is the prop a waypoint? */
    public static bool IsWaypoint(PropTypes type)
    {
        PropData thisTile = GetProp(type);
        if (thisTile == null) return false;
        return thisTile.isWaypoint;
    }

    /* Get prop waypoint intended entity */
    public static PropWaypointUser GetWaypointTarget(PropTypes type)
    {
        PropData thisTile = GetProp(type);
        if (thisTile == null) return (PropWaypointUser)0;
        return (PropWaypointUser)thisTile.waypointFor;
    }

    /* Get prop waypoint type */
    public static PropWaypointType GetWaypointType(PropTypes type)
    {
        PropData thisTile = GetProp(type);
        if (thisTile == null) return (PropWaypointType)(-1);
        return (PropWaypointType)thisTile.waypointType;
    }

    /* Is this prop a POI? */
    public static bool IsPOI(PropTypes type)
    {
        PropData thisTile = GetProp(type);
        if (thisTile == null) return false;
        return thisTile.isPOI;
    }

    /* Get prop POI type */
    public static PoiType GetPOIType(PropTypes type)
    {
        PropData thisTile = GetProp(type);
        if (thisTile == null) return (PoiType)0;
        return thisTile.poiType;
    }

    /* Get prop POI goon count */
    public static int GetPOIGoonCount(PropTypes type)
    {
        PropData thisTile = GetProp(type);
        if (thisTile == null) return 0;
        return thisTile.poiGoonCount;
    }

    /* Get prop placement location */
    public static PropPlacement GetPlacementLocation(PropTypes type)
    {
        PropData thisTile = GetProp(type);
        if (thisTile == null) return (PropPlacement)0;
        return thisTile.placement;
    }

    /* Should this prop's tile be classed as unpathable? */
    public static bool IsUnpathable(PropTypes type)
    {
        PropData thisTile = GetProp(type);
        if (thisTile == null) return false;
        return thisTile.isUnpathable;
    }

    /* Show this prop show up in the editor UI? */
    public static bool IsVisibleInEditor(PropTypes type)
    {
        PropData thisTile = GetProp(type);
        if (thisTile == null) return false;
        return !thisTile.hideInEditor;
    }

    /* Get the props's z-offset */
    public static int GetZOffset(PropTypes type)
    {
        PropData thisTile = GetProp(type);
        if (thisTile == null) return 0;
        return thisTile.zOffset;
    }

    /* Get a tile object from enum */
    private static PropData GetProp(PropTypes type)
    {
        foreach (PropData aProp in propData)
        {
            if (aProp.propName == type)
            {
                return aProp;
            }
        }
        Debug.LogError("Cannot find requested prop '" + type + "' - it may have been deleted! Map should be updated.");
        return null;
    }
}

public class SpriteBounds
{
    public int from_origin_north = 0;
    public int from_origin_east = 0;
    public int from_origin_south = 0;
    public int from_origin_west = 0;

    public float tile_excess_north = 0.0f;
    public float tile_excess_east = 0.0f;
    public float tile_excess_south = 0.0f;
    public float tile_excess_west = 0.0f;
}