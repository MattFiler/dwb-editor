using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DWB_Toolkit
{
    /* Container to handle wall tile sprites */
    class WallTileSprites
    {
        public void Load(string formattedTileName)
        {
            BinaryReader reader = new BinaryReader(File.OpenRead(FilePaths.PathToUnityStreaming + "/TILECONFIGS/" + formattedTileName + ".DWB"));
            reader.BaseStream.Position += 4;
            int verticalCount = reader.ReadInt32();
            int horizontalCount = reader.ReadInt32();
            reader.Close();

            string prefix = Environment.CurrentDirectory + "/" + FilePaths.PathToUnityTileResources + formattedTileName + "/";

            for (int i = 0; i < verticalCount; i++)
            {
                Verticals.Add(prefix + "VERTICAL_" + i + ".png");
            }
            for (int i = 0; i < horizontalCount; i++)
            {
                Horizontals.Add(prefix + "HORIZONTAL_" + i + ".png");
            }
        }

        public List<string> Verticals = new List<string>();
        public List<string> Horizontals = new List<string>();
    }

    /* Container to handle floor tile sprites */
    class FloorTileSprites
    {
        public void Load(string formattedTileName)
        {

            BinaryReader reader = new BinaryReader(File.OpenRead(FilePaths.PathToUnityStreaming + "/TILECONFIGS/" + formattedTileName + ".DWB"));
            reader.BaseStream.Position += 4;
            int fillerCount = reader.ReadInt32();
            reader.Close();

            string prefix = Environment.CurrentDirectory + "/" + FilePaths.PathToUnityTileResources + formattedTileName + "/";

            CORNER_NorthEast = prefix + "CORNER_NORTH_EAST.png";
            CORNER_SouthEast = prefix + "CORNER_SOUTH_EAST.png";
            CORNER_NorthWest = prefix + "CORNER_NORTH_WEST.png";
            CORNER_SouthWest = prefix + "CORNER_SOUTH_WEST.png";

            EDGING_North = prefix + "EDGE_NORTH.png";
            EDGING_East = prefix + "EDGE_EAST.png";
            EDGING_South = prefix + "EDGE_SOUTH.png";
            EDGING_West = prefix + "EDGE_WEST.png";

            for (int i = 0; i < fillerCount; i++)
            {
                Fillers.Add(prefix + "FILL_" + i + ".png");
            }
        }

        public string CORNER_NorthEast = "";
        public string CORNER_SouthEast = "";
        public string CORNER_NorthWest = "";
        public string CORNER_SouthWest = "";

        public string EDGING_North = "";
        public string EDGING_East = "";
        public string EDGING_South = "";
        public string EDGING_West = "";

        public List<string> Fillers = new List<string>();
    }

    /* Container to handle prop sprites */
    class PropSprites
    {
        public string Front = "";
        public string Left = "";
        public string Right = "";
        public string Back = "";
        public string EditorUI = "";
    }
}
