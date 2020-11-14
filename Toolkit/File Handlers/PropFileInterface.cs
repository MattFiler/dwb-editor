using System;
using System.Collections.Generic;
using System.IO;

namespace DWB_Toolkit
{
    class PropFileInterface
    {
        private static List<PropData> propData = new List<PropData>();
        private static int CURRENT_VERSION = 9;

        /* Load prop data */
        public static bool LoadData()
        {
            if (!File.Exists(FilePaths.PathToPropSaveFile)) return false;
            propData.Clear();
            BinaryReader reader = new BinaryReader(File.OpenRead(FilePaths.PathToPropSaveFile));
            int versionNum = reader.ReadInt32();
            if (versionNum != CURRENT_VERSION)
            {
                reader.Close();
                return false;
            }
            int typeCount = reader.ReadInt32();
            for (int i = 0; i < typeCount; i++)
            {
                PropData thisData = new PropData();
                thisData.propID = reader.ReadInt32();
                thisData.propName = reader.ReadString();
                thisData.propDesc = reader.ReadString();
                thisData.isWaypoint = reader.ReadBoolean();
                if (thisData.isWaypoint)
                {
                    thisData.waypointFor = reader.ReadInt16();
                    thisData.waypointType = reader.ReadInt16();
                }
                thisData.isEventSpawn = reader.ReadBoolean();
                if (thisData.isEventSpawn)
                {
                    thisData.eventType = reader.ReadString();
                }
                thisData.isPOI = reader.ReadBoolean();
                if (thisData.isPOI)
                {
                    thisData.poiType = reader.ReadInt16();
                    thisData.poiGoonCount = reader.ReadInt32();
                }
                thisData.isInside = reader.ReadBoolean();
                thisData.makesTileUnpathable = reader.ReadBoolean();
                thisData.hideInEditor = reader.ReadBoolean();
                thisData.zBias = reader.ReadInt16();
                propData.Add(thisData);
            }
            reader.Close();
            return true;
        }

        /* Get prop data */
        public static List<PropData> GetData()
        {
            return propData;
        }

        /* Remove an entry at index */
        public static void RemoveDataAt(int _i)
        {
            propData.RemoveAt(_i);
        }
        
        /* Save prop data */
        public static void SaveData()
        {
            //Prop properties
            BinaryWriter writer = new BinaryWriter(File.OpenWrite(FilePaths.PathToPropSaveFile));
            writer.BaseStream.SetLength(0);
            writer.Write(CURRENT_VERSION);
            writer.Write(propData.Count);
            for (int i = 0; i < propData.Count; i++)
            {
                if (propData[i].propID == -1)
                {
                    if (i == 0) propData[i].propID = 0;
                    else propData[i].propID = propData[i - 1].propID + 1;
                }
                writer.Write(propData[i].propID);
                writer.Write(propData[i].propName);
                writer.Write(propData[i].propDesc);
                writer.Write(propData[i].isWaypoint);
                if (propData[i].isWaypoint)
                {
                    writer.Write((Int16)propData[i].waypointFor);
                    writer.Write((Int16)propData[i].waypointType);
                }
                writer.Write(propData[i].isEventSpawn);
                if (propData[i].isEventSpawn)
                {
                    writer.Write(propData[i].eventType);
                }
                writer.Write(propData[i].isPOI);
                if (propData[i].isPOI)
                {
                    writer.Write((Int16)propData[i].poiType);
                    writer.Write(propData[i].poiGoonCount);
                }
                writer.Write(propData[i].isInside);
                writer.Write(propData[i].makesTileUnpathable);
                writer.Write(propData[i].hideInEditor);
                writer.Write((Int16)propData[i].zBias);
            }
            writer.Close();

            //Prop type enum for in script
            List<string> enumFile = new List<string>(File.ReadAllLines(FilePaths.PathToPropEnumFile));
            List<string> enumFileEdit = new List<string>();
            for (int i = 0; i < enumFile.Count; i++)
            {
                enumFileEdit.Add(enumFile[i]);
                if (enumFile[i].Contains("**START_ENUMS**"))
                {
                    for (int x = 0; x < propData.Count; x++)
                    {
                        enumFileEdit.Add(propData[x].propName + " = " + propData[x].propID + ",");
                    }
                    enumFileEdit.Add("}");
                    break;
                }
            }
            File.WriteAllLines(FilePaths.PathToPropEnumFile, enumFileEdit);
        }
    }
}
