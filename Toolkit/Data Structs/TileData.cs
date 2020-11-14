using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DWB_Toolkit
{
    /* Tile data to save: this should really match the TileData class in-game, at "TileProperties.cs" */
    class TileData : DataStruct
    {
        public int tileID = -1;

        public string tileName = "";
        public string tileDesc = "";

        public bool isPathable = false;
        public bool isFlammable = false;
        public bool allowProps = false;

        //public bool hideInEditor = false; - inherited now

        public int zBias = 0;

        public Int16 tileType = 0; //maps to enum 
        public Int16 tileUseage = 0; //maps to enum 
    }
}
