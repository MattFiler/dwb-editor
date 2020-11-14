using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DWB_Toolkit
{
    class TileFileInterface
    {
        private static List<TileData> tileData = new List<TileData>();
        private static int CURRENT_VERSION = 10;

        /* Load tile data */
        public static bool LoadData()
        {
            if (!File.Exists(FilePaths.PathToTileSaveFile)) return false;
            tileData.Clear();
            BinaryReader reader = new BinaryReader(File.OpenRead(FilePaths.PathToTileSaveFile));
            if (reader.ReadInt32() != CURRENT_VERSION)
            {
                reader.Close();
                return false;
            }
            int typeCount = reader.ReadInt32();
            for (int i = 0; i < typeCount; i++)
            {
                TileData thisData = new TileData();
                thisData.tileID = reader.ReadInt32();
                thisData.tileName = reader.ReadString();
                thisData.tileDesc = reader.ReadString();
                thisData.isPathable = reader.ReadBoolean();
                thisData.isFlammable = reader.ReadBoolean();
                thisData.allowProps = reader.ReadBoolean();
                thisData.tileType = reader.ReadInt16();
                thisData.tileUseage = reader.ReadInt16();
                thisData.zBias = reader.ReadInt16();
                thisData.hideInEditor = reader.ReadBoolean();
                tileData.Add(thisData);
            }
            reader.Close();
            return true;
        }

        /* Get tile data */
        public static List<TileData> GetData()
        {
            return tileData;
        }

        /* Remove an entry at index */
        public static void RemoveDataAt(int _i)
        {
            tileData.RemoveAt(_i);
        }
        
        /* Save tile data */
        public static void SaveData()
        {
            //Tile properties
            BinaryWriter writer = new BinaryWriter(File.OpenWrite(FilePaths.PathToTileSaveFile));
            writer.BaseStream.SetLength(0);
            writer.Write(CURRENT_VERSION);
            writer.Write(tileData.Count);
            for (int i = 0; i < tileData.Count; i++)
            {
                if (tileData[i].tileID == -1)
                {
                    if (i == 0) tileData[i].tileID = 0;
                    else tileData[i].tileID = tileData[i - 1].tileID + 1;
                }
                writer.Write(tileData[i].tileID);
                writer.Write(tileData[i].tileName);
                writer.Write(tileData[i].tileDesc);
                writer.Write(tileData[i].isPathable);
                writer.Write(tileData[i].isFlammable);
                writer.Write(tileData[i].allowProps);
                writer.Write((Int16)tileData[i].tileType);
                writer.Write((Int16)tileData[i].tileUseage);
                writer.Write((Int16)tileData[i].zBias);
                writer.Write(tileData[i].hideInEditor);
            }
            writer.Close();

            //Tile type enum for in script
            List<string> enumFile = new List<string>(File.ReadAllLines(FilePaths.PathToTileEnumFile));
            List<string> enumFileEdit = new List<string>();
            for (int i = 0; i < enumFile.Count; i++)
            {
                enumFileEdit.Add(enumFile[i]);
                if (enumFile[i].Contains("**START_ENUMS**"))
                {
                    for (int x = 0; x < tileData.Count; x++)
                    {
                        enumFileEdit.Add(tileData[x].tileName + " = " + tileData[x].tileID + ", ");
                    }
                    enumFileEdit.Add("}");
                    break;
                }
            }
            File.WriteAllLines(FilePaths.PathToTileEnumFile, enumFileEdit);
        }
    }
}
