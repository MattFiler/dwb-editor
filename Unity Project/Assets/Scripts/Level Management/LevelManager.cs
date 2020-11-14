/*
 * *************************************************************
 * PLEASE DO NOT EDIT THIS SCRIPT WITHOUT CONTACTING THE AUTHORS
 * *************************************************************
 * 
 * The main level data manager. Allows the ability to load/unload a level.
 * Also contains metadata for the current level, and the ability to get metadata for ALL levels.
 * 
 * AUTHORS:
 *  - Matt Filer
*/

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

/* An object for storing metadata about a level */
public class LevelMetadata
{
    public LevelMetadata() { }
    public LevelMetadata(string name, int guid)
    {
        _levelName = name;
        _levelGUID = guid;
    }

    //Key level metadata: name, ID, and if it's UGC
    public string levelName { get { return _levelName; } }
    public int levelGUID { get { return _levelGUID; } }
    public string screenshotPath { get { return _screenshotPath; } }
    public bool hasScreenshot { get { return _screenshotPath != ""; } }

    //If the map is currently loaded, this is true
    public bool isLoaded { get { return _isLoaded; } }

    //Campaign data: if this is a campaign level, get next level ID, and if it's the intro level
    public bool isCampaignLevel { get { return (nextLevelGUID != -1); } }
    public bool isFirstCampaignLevel { get { return _isFirst; } }
    public bool isLastCampaignLevel { get { return _isLast; } }
    public int nextLevelGUID { get { return _nextLevelGUID; } }

    //Counts
    public int hazardCount { get { return _hazardCount; } }
    public int goonCount { get { return _goonCount; } }
    public AbilityCounts abilityCount { get {
            if (_abilityCounts == null) return new AbilityCounts(5, 5, 5, 5); //should only get this on v10s
            else return _abilityCounts; 
    } }

    //FOR USE ONLY BY LEVELMANAGER: C# IS CRAP AND DOESN'T HAVE FRIEND LINKS
    public void SetLoadState(bool _loaded) { _isLoaded = _loaded; }
    public void SetFirstLevel(bool _first) { _isFirst = _first; }
    public void SetLastLevel(bool _last) { _isLast = _last; }
    public void SetNextLevel(int _next) { _nextLevelGUID = _next; }
    public void SetGoonCount(int _goon) { _goonCount = _goon; }
    public void SetHazardCount(int _hazard) { _hazardCount = _hazard; }
    public void SetAbilityCounts(AbilityCounts _info) { _abilityCounts = _info; }
    public void SetScreenshotPath(string _path) { _screenshotPath = _path; }

    //--
    private string _levelName = "";
    private int _levelGUID = -1;
    private bool _isLoaded = false;
    private int _nextLevelGUID = -1;
    private bool _isFirst = false;
    private bool _isLast = false;
    private int _goonCount = 75;
    private int _hazardCount = -1;
    private string _screenshotPath = "";
    private AbilityCounts _abilityCounts;
}

public class AbilityCounts
{
    public AbilityCounts(int _rally, int _medical, int _fire, int _marshal)
    {
        Rally = _rally;
        Medical = _medical;
        FirePhone = _fire;
        Marshal = _marshal;
    }
    public int Rally;
    public int Medical;
    public int FirePhone;
    public int Marshal;
}

public class LinkedLevel
{
    public LinkedLevel(int _1, int _2, bool _first, bool _last)
    {
        GUID_1 = _1;
        GUID_2 = _2;
        IS_FIRST = _first;
        IS_LAST = _last;
    }
    public int GUID_1;
    public int GUID_2;
    public bool IS_FIRST;
    public bool IS_LAST;
}

public class LevelManager : MonoSingleton<LevelManager>
{
    [SerializeField] private GameObject worldTileObject = null;
    [SerializeField] private GameObject worldPropObject = null;
    public GameObject WorldTileObject { get { return worldTileObject; } }
    public GameObject WorldPropObject { get { return worldPropObject; } }

    public bool IsInEditor = false;

    public delegate void LevelEvent();
    public LevelEvent LevelLoadCompleted;
    public LevelEvent LevelUnloadCompleted;

    private LevelMetadata metadata = new LevelMetadata();
    public LevelMetadata CurrentMeta { get {
        //if (!metadata.isLoaded) Debug.LogWarning("Returning metadata for non-loaded level. Likely will be issues.");
        return metadata;
    } }
    private bool loadedInDebug = true;
    public bool LoadedInDebugMode { get { return loadedInDebug; } }

    private LevelGrid theGrid;
    public LevelGrid Grid { get { return theGrid; } } //The main level grid, used for accessing tile/position data.
    private void Start()
    {
        theGrid = GetComponent<LevelGrid>();
        StartCoroutine(RefreshLevelData());
    }

    /* Load the level */
    bool validateNextTick = false;
    MemoryStream levelData = null;
    public IEnumerator Load(int level_guid)
    {
        loadedInDebug = false;

        AsyncPropAndTileSetup.Instance.LoadConfigs();
        while (!TileProperties.Loaded || !PropProperties.Loaded)
            yield return new WaitForEndOfFrame();

        if (!didTriggerMetaUpdate) StartCoroutine(RefreshLevelData());
        while (!hasUpdatedMetas)
            yield return new WaitForEndOfFrame();

        Unload();

        //Find our metadata
        LevelMetadata ourMeta = null;
        foreach (LevelMetadata meta in LevelMetadata)
        {
            if (meta.levelGUID == level_guid)
            {
                ourMeta = meta;
                break;
            }
        }
        if (ourMeta == null)
        {
            Debug.LogError("Cannot find requested level GUID!");
            yield break;
        }
        metadata = ourMeta;
        metadata.SetLoadState(false);
        Debug.Log("Loading level: " + metadata.levelName + " (GUID " + metadata.levelGUID + ")");
        int invisibleProps = 0;
        int invisibleTiles = 0;

        string configFile = Application.streamingAssetsPath + "/LEVELS/" + metadata.levelName + ".dwb";
#if !UNITY_WEBGL
        if (File.Exists(configFile)) //It's ok not to exist if we're in editor
        {
#endif
            //Load our level config
            levelData = null;
            StartCoroutine(FileLoader.Instance.LoadAsBytes(configFile, LoadCallback));
            while (levelData == null || !levelData.CanRead)
                yield return new WaitForEndOfFrame();

            //Read our level config
            BinaryReader reader = new BinaryReader(levelData);
            int versionNum = reader.ReadInt32();
            if (!(versionNum == LevelFileVersion.VERSION_NUM)) //This can allow through back-supported config versions
            {
                Debug.LogError("Tried to load an outdated level file!");
                reader.Close();
                yield break;
            }
            if (versionNum != LevelFileVersion.VERSION_NUM)
            {
                Debug.LogWarning("This level uses file version " + versionNum + " (latest is " + LevelFileVersion.VERSION_NUM + "). While this version is supported, please re-save the level in editor ASAP to keep updated with the latest features.");
            }
            int tileCount = reader.ReadInt32();
            Vector2 gridDims = new Vector2(reader.ReadInt16(), reader.ReadInt16());
            if (gridDims != theGrid.GridTileDims)
            {
                Debug.LogError("Grid resizing is unsupported! This level is invalid.");
                reader.Close();
                yield break;
            }

            //Load occupiers
            int tileOccupierCount = reader.ReadInt32();
            for (int i = 0; i < tileOccupierCount; i++)
            {
                Tile parentTile = theGrid.AllTiles[reader.ReadInt32()];
                int enumIndex = reader.ReadInt16();

                if (!Enum.IsDefined(typeof(TileTypes), enumIndex))
                {
                    Debug.LogError("ERROR! When loading, a tile's occupier was of a type that no longer exists.");
                    continue;
                }

                //Populate the tile's occupier
                GameObject parentTileObject = null;
                parentTileObject = Instantiate(worldTileObject, parentTile.Position, Quaternion.identity) as GameObject;
                TileTypes tileType = (TileTypes)enumIndex;
#if UNITY_EDITOR
                if (!TileProperties.IsVisibleInEditor(tileType)) invisibleTiles++;
#endif
                parentTileObject.GetComponent<LevelTileEntity>().Initialise(tileType, parentTile, false);
            }

            //Load props
            int tilePropCount = reader.ReadInt32();
            for (int i = 0; i < tilePropCount; i++)
            {
                if (versionNum == 13) reader.BaseStream.Position += 4;
                int enumIndex = reader.ReadInt16();
                PropRotation rotation = (PropRotation)reader.ReadInt16();

                int propTileCount = reader.ReadInt32();
                List<Tile> propTiles = new List<Tile>();
                for (int x = 0; x < propTileCount; x++)
                {
                    propTiles.Add(theGrid.AllTiles[reader.ReadInt32()]);
                }

                if (!Enum.IsDefined(typeof(PropTypes), enumIndex))
                {
                    Debug.LogError("ERROR! When loading, a tile's prop was of a type that no longer exists.");
                    continue;
                }

                PropTypes propType = (PropTypes)enumIndex;
                GameObject parentTileDecoratorObject = Instantiate(worldPropObject, propTiles[0].Position, Quaternion.identity) as GameObject;
#if UNITY_EDITOR
                if (!PropProperties.IsVisibleInEditor(propType)) invisibleProps++;
#endif
                parentTileDecoratorObject.GetComponent<LevelPropEntity>().Initialise(propType, propTiles, rotation, false);
            }

            //Load some metadata
            metadata.SetGoonCount(reader.ReadInt32());
            metadata.SetHazardCount(reader.ReadInt32());

            reader.Close();

            //Refresh all sprites to get the right contexts
            for (int i = 0; i < Grid.AllTiles.Count; i++)
            {
                if (Grid.AllTiles[i].IsOccupied) Grid.AllTiles[i].Occupier.RefreshSprite();
            }

            metadata.SetLoadState(true);
#if !UNITY_WEBGL
        }
        else
        {
            //User is just starting to create this level
            metadata.SetLoadState(true);
            LevelEditor.Instance.SaveLevel();
        }
#endif

        //Show/hide grid depending on editor state
        GetComponent<SpriteRenderer>().enabled = IsInEditor;

        if (invisibleProps == 0 && invisibleTiles == 0) Debug.Log("Level loaded without warnings!");
        else Debug.LogWarning("Level loaded successfully, but contains " + invisibleProps + " deprecated props and " + invisibleTiles + " deprecated tiles. Consider updating this content.");
        LevelLoadCompleted?.Invoke();

        validateNextTick = true;
    }
    private void LoadCallback(byte[] content)
    {
        levelData = new MemoryStream(content);
    }
    private void Update()
    {
        if (!validateNextTick) return;
        bool[] validity = LevelIsValid();
        if (!(validity[0] && validity[1] && validity[2] && validity[3]) && !IsInEditor) 
        {
            Debug.LogError("Level failed validation check! Please open this in the level editor and fix the issues.");
            Unload();
#if UNITY_EDITOR
            SceneHandler.Instance.LoadScene("ErrorWarning");
#else
            SceneHandler.Instance.LoadScene("MainMenu");
#endif
        }
        validateNextTick = false;
    }

    /* Unload the currently loaded level */
    public void Unload()
    {
        metadata.SetLoadState(false);
        metadata = new LevelMetadata();
        theGrid.ClearGrid();
        LevelUnloadCompleted?.Invoke();
    }

    /* Is the currently loaded level valid? */
    public bool[] LevelIsValid()
    {
        bool[] validityCheck = { false, false, false, false };
        if (!metadata.isLoaded) return validityCheck;
        Tile battlebusSpawn = null;
        Tile goonEntryDoor = null;
        Tile goonExitDoor = null;
        foreach (Tile thisTile in Grid.AllTiles)
        {
            if (thisTile.HasPropOrigin &&
                PropProperties.GetWaypointTarget(thisTile.Prop.Type) == PropWaypointUser.GOON_BATTLEBUS &&
                PropProperties.GetWaypointType(thisTile.Prop.Type) == PropWaypointType.MIDDLE)
            {
                battlebusSpawn = thisTile;
                validityCheck[0] = true;
                if (validityCheck[1] && validityCheck[3]) break;
            }
            else if (thisTile.HasPropOrigin &&
                PropProperties.GetWaypointTarget(thisTile.Prop.Type) == PropWaypointUser.GOONS &&
                PropProperties.GetWaypointType(thisTile.Prop.Type) == PropWaypointType.START)
            {
                goonEntryDoor = thisTile;
                validityCheck[3] = true;
                if (validityCheck[0] && validityCheck[1]) break;
            }
            else if (thisTile.HasPropOrigin &&
                PropProperties.GetWaypointTarget(thisTile.Prop.Type) == PropWaypointUser.GOONS &&
                PropProperties.GetWaypointType(thisTile.Prop.Type) == PropWaypointType.END)
            {
                goonExitDoor = thisTile;
                validityCheck[1] = true;
                if (validityCheck[0] && validityCheck[3]) break;
            }
        }
        //TODO: This is where you'd implement your pathfinding check!
        if (battlebusSpawn != null && goonEntryDoor != null) validityCheck[2] = true/*AStarPathfinding.IsPathValid(battlebusSpawn.Position, goonEntryDoor.Position)*/;
        if (validityCheck[2] && goonEntryDoor != null && goonExitDoor != null) validityCheck[2] = true/*AStarPathfinding.IsPathValid(goonEntryDoor.Position, goonExitDoor.Position)*/;
        return validityCheck;
    }

    /* Get metadata for all levels we can currently load */
    private int prevGUID = -1;
    public int HistoricalMaxGUID { get { return prevGUID; } }
    private List<LevelMetadata> levelMetas = new List<LevelMetadata>();
    private MemoryStream levelMemStream = null;
    private MemoryStream levelMemStreamInt = null;
    public List<LevelMetadata> LevelMetadata { get { return levelMetas; } }
    private bool hasUpdatedMetas = false;
    private bool didTriggerMetaUpdate = false;
    public bool LevelMetaUpdated { get { return hasUpdatedMetas; } }
    public IEnumerator RefreshLevelData()
    {
        Debug.Log("Refreshing level metadata...");
        didTriggerMetaUpdate = true;

        hasUpdatedMetas = false;
        levelMemStream = null;
        StartCoroutine(FileLoader.Instance.LoadAsBytes(Application.streamingAssetsPath + "/LEVELS_MANIFEST.dwb", RefreshLevelDataCallback));
        while (levelMemStream == null || !levelMemStream.CanRead)
            yield return new WaitForEndOfFrame();

        BinaryReader bankReader = new BinaryReader(levelMemStream);
        prevGUID = bankReader.ReadInt32();

        //Base metadata
        levelMetas = new List<LevelMetadata>(bankReader.ReadInt32());
        for (int i = 0; i < levelMetas.Capacity; i++) levelMetas.Add(new LevelMetadata(bankReader.ReadString(), bankReader.ReadInt32()));

        //Count metadata
        for (int x = 0; x < levelMetas.Count; x++)
        {
            string levelPath = Application.streamingAssetsPath + "/LEVELS/" + levelMetas[x].levelName + ".dwb";
#if !UNITY_WEBGL
            if (!File.Exists(levelPath)) continue; //This should only happen if called in the gap between editor instancing and creating a level file.
#endif

            levelMemStreamInt = null;
            StartCoroutine(FileLoader.Instance.LoadAsBytes(levelPath, RefreshLevelDataCallbackInt));
            while (levelMemStreamInt == null || !levelMemStreamInt.CanRead)
                yield return new WaitForEndOfFrame();

            BinaryReader thisLevelFile = new BinaryReader(levelMemStreamInt);
            int versionNum = thisLevelFile.ReadInt32();
            if (versionNum > 10) //Only levels above V10 have this info
            {
                thisLevelFile.BaseStream.Position = thisLevelFile.BaseStream.Length - (sizeof(int) * 6);
                levelMetas[x].SetGoonCount(thisLevelFile.ReadInt32());
                levelMetas[x].SetHazardCount(thisLevelFile.ReadInt32());
                levelMetas[x].SetAbilityCounts(new AbilityCounts(thisLevelFile.ReadInt32(), thisLevelFile.ReadInt32(), thisLevelFile.ReadInt32(), thisLevelFile.ReadInt32()));
            }
            thisLevelFile.Close();

#if !UNITY_WEBGL
            if (File.Exists(levelPath.Substring(0, levelPath.Length - 3) + "png"))
#endif
                levelMetas[x].SetScreenshotPath(levelPath.Substring(0, levelPath.Length - 3) + "png"); 
        }

        //Linked levels
        int linkedLevelCount = bankReader.ReadInt32();
        for (int i = 0; i < linkedLevelCount; i++)
        {
            int levelGUID = bankReader.ReadInt32();
            int nextLevelGUID = bankReader.ReadInt32();
            bool isFirstLevel = bankReader.ReadBoolean();
            bool isLastLevel = bankReader.ReadBoolean();
            for (int x = 0; x < levelMetas.Count; x++)
            {
                if (levelMetas[x].levelGUID == levelGUID)
                {
                    levelMetas[x].SetNextLevel(nextLevelGUID);
                    levelMetas[x].SetFirstLevel(isFirstLevel);
                    levelMetas[x].SetLastLevel(isLastLevel);
                    break;
                }
            }
        }

        bankReader.Close();
        hasUpdatedMetas = true;
    }
    public void RefreshLevelDataCallback(byte[] content)
    {
        levelMemStream = new MemoryStream(content);
    }
    public void RefreshLevelDataCallbackInt(byte[] content)
    {
        levelMemStreamInt = new MemoryStream(content);
    }
}
