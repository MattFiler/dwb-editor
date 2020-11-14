using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DWB_Toolkit
{
    class PropParams
    {
        /* This data should match the enum order found within "PropTypes.cs" in the game */
        public static string[] waypointTypes = { "NONE", "GOONS", "GOON_BATTLEBUS", "FIRE_ENGINE", "AMBULANCE" };
        /* This should match the PoiState enum in "PoiPoint.cs" in the game */
        public static string[] poiTypes = { "DEFAULT", "INTERACTABLE", "SITTABLE" };
    };
}
