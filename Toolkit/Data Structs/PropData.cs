using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DWB_Toolkit
{
    /* Tile data to save: this should really match the PropData class in-game, at "PropProperties.cs" */
    class PropData : DataStruct
    {
        public int propID = -1;

        public string propName = "";
        public string propDesc = "";

        public bool isWaypoint = false;
        public Int16 waypointType = 0; //maps to enum 
        public Int16 waypointFor = 0; //maps to enum 

        public bool isEventSpawn = false;
        public string eventType = ""; //maps to a .cs file

        public bool isPOI = false;
        public Int16 poiType = 0; //maps to enum
        public int poiGoonCount = 0;

        public int zBias = 0;

        public bool isInside = true;
        public bool makesTileUnpathable = false;
        //public bool hideInEditor = false; - inherited now
    }
}
