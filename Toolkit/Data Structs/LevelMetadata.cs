using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DWB_Toolkit
{
    public class LevelMetadata
    {
        public LevelMetadata() { }
        public LevelMetadata(string name, int guid)
        {
            levelName = name;
            levelGUID = guid;
        }
        public string levelName = "";
        public int levelGUID = -1;
        public bool isOfficialMap = true;
        public bool isLoaded = false;
        public int nextLevelGUID = -1;
        public bool isFirstLevel = false;
        public bool isLastLevel = false;
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
}
