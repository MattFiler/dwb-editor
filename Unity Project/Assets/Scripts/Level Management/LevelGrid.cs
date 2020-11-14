/*
 * *************************************************************
 * PLEASE DO NOT EDIT THIS SCRIPT WITHOUT CONTACTING THE AUTHORS
 * *************************************************************
 * 
 * Scripts for both the level grid and tiles within the grid.
 * Access this through LevelManager.Instance.Grid.
 * 
 * AUTHORS:
 *  - Matt Filer
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

/* A tile on the grid */
public class Tile
{
    private Vector2 _dims;             //The width and height of the tile
    private Vector2 _pos;              //The centre of the tile in world space
    private bool _isOccupied;          //If something is in this tile, it is occupied
    private LevelTileEntity _occupier; //If occupied, this should be set to the object in the space
    private bool _hasProp;             //If the tile has a prop (only possible if also occupied)
    private LevelPropEntity _prop;     //If tile has prop, this should be set to the object
    private Vector2 _gridPos;          //The X and Y position of the tile in the grid
    private int _index;                //The index in the grid
    private bool _isOriginOfProp;      //If this tile is the origin of a prop, this is true

    public Vector2 Dimensions { get { return _dims; } }
    public Vector2 Position { get { return _pos; } }

    public bool IsOccupied { get {
        if (_isOccupied && _occupier == null) { UnityEngine.Debug.LogWarning("Occupier has been unsafely deleted - check scripts!"); _isOccupied = false; }
        return _isOccupied;
    } }
    public LevelTileEntity Occupier { get { return _occupier; } }

    public bool HasProp { get {
        if (_hasProp && _prop == null) { UnityEngine.Debug.LogWarning("Prop has been unsafely deleted - check scripts!"); _hasProp = false; }
        return _hasProp;
    } }
    public LevelPropEntity Prop { get { return _prop; } }
    public bool HasPropOrigin { get { return _isOriginOfProp; } }

    public Vector2 GridPos { get { return _gridPos; } }
    public int Index { get { return _index; } }

    /* Create tile */
    public Tile(Vector2 dimensions, Vector2 worldPosition, Vector2 gridPosition, int index)
    {
        _dims = dimensions;
        _pos = worldPosition;
        _gridPos = gridPosition;
        _isOccupied = false;
        _occupier = null;
        _hasProp = false;
        _prop = null;
        _index = index;
        _isOriginOfProp = false;
    }

    /* Set the tile as occupied, and reset the decorator (if it exists) */
    public bool SetOccupied(LevelTileEntity inhabitant, bool destroyPrevious = true, bool refreshSprites = false)
    {
        SetProp(null, destroyPrevious);
        if (_occupier && destroyPrevious) _occupier.DestroyMe();
        _isOccupied = (inhabitant != null);
        _occupier = inhabitant;

        if (refreshSprites)
        {
            for (int i = 0; i < LevelManager.Instance.Grid.AllTiles.Count; i++)
            {
                if (LevelManager.Instance.Grid.AllTiles[i].IsOccupied) LevelManager.Instance.Grid.AllTiles[i].Occupier.RefreshSprite();
            }
        }

        return true;
    }

    /* Set the prop on the tile (must be occupied first) */
    public bool SetProp(LevelPropEntity prop, bool destroyPrevious = true, bool destroyPreviousWithAnimation = true)
    {
        if (prop != null && !IsOccupied) return false;
        if (_prop && destroyPrevious) _prop.DestroyMe(destroyPreviousWithAnimation);
        _hasProp = (prop != null);
        _prop = prop;
        _isOriginOfProp = false;
        return true;
    }

    /* Set this tile as the prop's origin (the centre of the prop's occupiers) - do this after calling SetProp */
    public void SetPropOrigin()
    {
        _isOriginOfProp = true;
    }

    /* Work out if a specified point is within the tile */
    public bool Contains(Vector2 point)
    {
        for (float x = _pos.x - (_dims.x / 2) - 0.5f; x < _pos.x + (_dims.x / 2) + 0.5f; x++)
        {
            if (Math.Round(x) != Math.Round(point.x)) continue;
            for (float y = _pos.y - (_dims.y / 2) - 0.5f; y < _pos.y + (_dims.y / 2) + 0.5f; y++)
            {
                if (Math.Round(y) == Math.Round(point.y)) return true;
            }
            return false;
        }
        return false;
    }

    /* Work out if we have any neighbours */
    public bool HasNeighbours(LevelGrid playspace)
    {
        foreach (Tile nTile in playspace.GetTileNeighbours(this)) if (nTile != null && nTile._isOccupied) return true;
        return false;
    }

    /* Work out if we're at the top of the board */
    public bool IsAtTop(LevelGrid playspace)
    {
        return (GridPos.y == 0);
    }
}

/* Maps to the array order returned from GetNeighboursToTile */
enum TileNeighbour
{
    LEFT,
    TOP_LEFT,
    TOP,
    TOP_RIGHT,
    RIGHT,
    BOTTOM_RIGHT,
    BOTTOM,
    BOTTOM_LEFT
}

/* The grid, made up of tiles, which handles finding correct placement positions */
public class LevelGrid : MonoBehaviour
{
    [SerializeField] private float TileSize = 1;  //The tile size is used for width and height

    private List<Tile> _tiles = new List<Tile>(); //All tiles in the grid
    private int _horizontalTileCount = -1;        //The number of tiles across the grid horizontally
    private int _verticalTileCount = -1;          //The number of tiles across the grid vertically

    public List<Tile> AllTiles { get { return _tiles; } }
    public Vector2 GridTileDims { get { return new Vector2(_horizontalTileCount, _verticalTileCount); } }

#if UNITY_EDITOR
    [SerializeField] private bool ShowExtraInfoInGizmo = false;
    [SerializeField] private bool HighlightUnpathableTilesInGizmo = false;
    [SerializeField] private bool HighlightTilesWithPropsInGizmo = false;
#endif

    /* Call to create grid both in game and in editor for debugging */
    void Start()
    {
        CreateGrid();
    }

    void OnDrawGizmosSelected()
    {
#if UNITY_EDITOR
        for (int i = 0; i < _tiles.Count; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(_tiles[i].Position, _tiles[i].Dimensions);
            Vector2 drawPos = _tiles[i].Position + new Vector2(-0.25f, 0.25f);
            Handles.Label(drawPos, "X: " + _tiles[i].GridPos.x + "\nY: " + _tiles[i].GridPos.y);
            if (ShowExtraInfoInGizmo)
            {
                if (_tiles[i].IsOccupied) Handles.Label(drawPos, "\n\nOCCUPIED");
                if (_tiles[i].HasPropOrigin) Handles.Label(drawPos, "\n\n\n\nHAS PROP ORIGIN");
            }
            if (_tiles[i].HasProp)
            {
                Gizmos.color = Color.green;
                if (HighlightTilesWithPropsInGizmo && _tiles[i].HasPropOrigin) Gizmos.DrawCube(_tiles[i].Position, _tiles[i].Dimensions);
                Gizmos.color = Color.yellow;
                if (HighlightTilesWithPropsInGizmo && !_tiles[i].HasPropOrigin) Gizmos.DrawCube(_tiles[i].Position, _tiles[i].Dimensions);
                if (ShowExtraInfoInGizmo) Handles.Label(drawPos, "\n\n\nHAS PROP");
            }
            if ((_tiles[i].HasProp && PropProperties.IsUnpathable(_tiles[i].Prop.Type)) ||
                (_tiles[i].IsOccupied && !TileProperties.IsPathable(_tiles[i].Occupier.Type)))
            {
                Gizmos.color = Color.red;
                if (HighlightUnpathableTilesInGizmo) Gizmos.DrawCube(_tiles[i].Position, _tiles[i].Dimensions);
                if (ShowExtraInfoInGizmo) Handles.Label(drawPos, "\n\n\n\n\nTILE IS UNPATHABLE");
            }
        }
#endif
    }

    /* Create the tiles within the grid */
    private void CreateGrid()
    {
        if (TileSize.ToString() == "") return;

        //Get grid position and size
        SpriteRenderer spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        Vector2 gridDims = new Vector2(spriteRenderer.bounds.size.x, spriteRenderer.bounds.size.y);
        Vector2 gridPos = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);
        Vector2 gridTopLeft = new Vector2(gridPos.x - (gridDims.x / 2), gridPos.y - (gridDims.y / 2));

        //Create tiles within the grid dimensions
        int gridX = 0;
        int gridY = 0;
        _tiles.Clear();
        int index = 0;
        for (float y = gridTopLeft.y; y < gridTopLeft.y + gridDims.y; y += TileSize)
        {
            float startWidth = gridTopLeft.x;
            float targetWidth = gridTopLeft.x + gridDims.x;
            for (float x = startWidth; x < targetWidth; x += TileSize)
            {
                Tile newTile = new Tile(
                    new Vector2(TileSize, TileSize), 
                    new Vector2(x + (TileSize / 2), y + (TileSize / 2)), 
                    new Vector2(gridX, gridY),
                    index
                );
                _tiles.Add(newTile);
                gridX++;
                index++;
            }
            _horizontalTileCount = gridX;
            gridX = 0;
            gridY++;
        }
        _verticalTileCount = gridY;
    }

    /* Return a tile that is valid to land in within (or closest to) a given location */
    public Tile GetTileAtPosition(Vector2 location)
    {
        location -= _tiles[0].Position;
        int index = (int)((Mathf.Round(location.x) * TileSize) + ((Mathf.Round(location.y) * TileSize) * (float)_horizontalTileCount));

        if (index >= 0 && index < _tiles.Count)
            return _tiles[index];
        else
            UnityEngine.Debug.LogWarning("No suitable tile found at " + (location + _tiles[0].Position) + ". Make sure to handle this in your code!");
        return null;

        //Workout what tile covers the location
        //Tile containingTile = null;
        //foreach (Tile tile in _tiles)
        //{
        //    if (tile.Contains(location))
        //    {
        //        containingTile = tile;
        //        break;
        //    }
        //}
        //if (containingTile == null) UnityEngine.Debug.LogWarning("No suitable tile found. Make sure to handle this in your code!");
        //return containingTile;
    }

    /* Return a tile that matches the given grid position */
    public Tile GetTileAtGridPos(Vector2 gridpos)
    {
        foreach (Tile tile in _tiles)
        {
            if (tile.GridPos == gridpos)
            {
                return tile;
            }
        }
        UnityEngine.Debug.LogWarning("No tile at grid pos: " + gridpos + ". Make sure to handle this in your code!");
        return null;
    }

    /* Get the tiles that neighbour a given tile (if the neighbour would be off-grid, it is null) */
    public Tile[] GetTileNeighbours(Tile tile)
    {
        int i = tile.Index;
        int w = _horizontalTileCount;
        int h = _verticalTileCount;

        List<Tile> neighbourList = new List<Tile>();
        if (i - 1 < 0) { neighbourList.Add(null); } else { neighbourList.Add(_tiles[i - 1]); }                     //Left
        if (i + w - 1 >= _tiles.Count) { neighbourList.Add(null); } else { neighbourList.Add(_tiles[i + w - 1]); } //Top left
        if (i + w >= _tiles.Count) { neighbourList.Add(null); } else { neighbourList.Add(_tiles[i + w]); }         //Top
        if (i + w + 1 >= _tiles.Count) { neighbourList.Add(null); } else { neighbourList.Add(_tiles[i + w + 1]); } //Top right
        if (i + 1 >= _tiles.Count) { neighbourList.Add(null); } else { neighbourList.Add(_tiles[i + 1]); }         //Right
        if (i - w + 1 < 0) { neighbourList.Add(null); } else { neighbourList.Add(_tiles[i - w + 1]); }             //Bottom right
        if (i - w < 0) { neighbourList.Add(null); } else { neighbourList.Add(_tiles[i - w]); }                     //Bottom
        if (i - w - 1 < 0) { neighbourList.Add(null); } else { neighbourList.Add(_tiles[i - w - 1]); }             //Bottom left

        return neighbourList.ToArray();
    }

    /* Get the tiles that neighbour a given tile given bounds (if the neighbour would be off-grid, it is null) - **List contains original tile in index 0!! */
    public List<Tile> GetNeighboursFromBounds(Tile tile, SpriteBounds bounds)
    {
        int northBounds = (int)Math.Ceiling(bounds.tile_excess_north)+1;
        int eastBounds = (int)Math.Ceiling(bounds.tile_excess_east)+1;
        int southBounds = (int)Math.Ceiling(bounds.tile_excess_south)+1;
        int westBounds = (int)Math.Ceiling(bounds.tile_excess_west)+1;

        //Debug.Log("North: " + northBounds + ", South: " + southBounds + ", East: " + eastBounds + ", West: " + westBounds);

        /* There is definitely a more optimal way to do this, but as this function is called rarely, I'm using this method as it's easier to debug for my tiny brain */
        List<Vector2> posList = new List<Vector2>();
        for (int x = 0; x < eastBounds; x++)
        {
            for (int y = 0; y < northBounds; y++)
            {
                Vector2 newPos = tile.GridPos + new Vector2(x, y);
                if (!posList.Contains(newPos)) posList.Add(newPos);
            }
        }
        for (int x = 0; x < eastBounds; x++)
        {
            for (int y = 0; y < southBounds; y++)
            {
                Vector2 newPos = tile.GridPos + new Vector2(x, -y);
                if (!posList.Contains(newPos)) posList.Add(newPos);
            }
        }
        for (int x = 0; x < westBounds; x++)
        {
            for (int y = 0; y < northBounds; y++)
            {
                Vector2 newPos = tile.GridPos + new Vector2(-x, y);
                if (!posList.Contains(newPos)) posList.Add(newPos);
            }
        }
        for (int x = 0; x < westBounds; x++)
        {
            for (int y = 0; y < southBounds; y++)
            {
                Vector2 newPos = tile.GridPos + new Vector2(-x, -y);
                if (!posList.Contains(newPos)) posList.Add(newPos);
            }
        }
        List<Tile> neighbourList = new List<Tile>();
        foreach (Vector2 pos in posList)
        {
            neighbourList.Add(GetTileAtGridPos(pos));
            //Debug.Log(pos);
        }

        return neighbourList;
    }

    /* Clears all entries on the grid (use with caution!) */
    public void ClearGrid()
    {
        foreach (Tile aTile in _tiles)
        {
            aTile.SetOccupied(null);
            aTile.SetProp(null);
        }
    }

   
}
