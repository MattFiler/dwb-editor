using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DWB_Toolkit
{
    /* Only change these filepaths if you're also updating them in-script in game */
    class FilePaths
    {
        public static string PathToUnityStreaming = "Unity Project/Assets/StreamingAssets/";
        public static string PathToUnityScripts = "Unity Project/Assets/Scripts/Level Management/";

        public static string PathToTileEnumFile = PathToUnityScripts + "TileTypes.cs";
        public static string PathToPropEnumFile = PathToUnityScripts + "PropTypes.cs";

        public static string PathToUnityTileResources = "Unity Project/Assets/Resources/TILES/";
        public static string PathToUnityPropResources = "Unity Project/Assets/Resources/PROPS/";
        
        public static string PathToTileSaveFile = PathToUnityStreaming + "TILEDATA.DWB";
        public static string PathToPropSaveFile = PathToUnityStreaming + "PROPDATA.DWB";
    }
}
