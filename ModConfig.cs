using StardewModdingAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessibleTiles {
    public class ModConfig {

        /*
        * Grid Movement Keys
        */
        public Boolean GridMovementActive { get; set; } = true;

        public KeybindList ToggleGridMovementKey { get; set; } = KeybindList.Parse("I");

        public KeybindList GridMovementOverrideKey { get; set; } = KeybindList.Parse("LeftControl");

        /*
         * Object Tracker Keys
         */
        public KeybindList OTCycleUpCategory { get; set; } = KeybindList.Parse("LeftControl + PageUp");
        public KeybindList OTCycleDownCategory { get; set; } = KeybindList.Parse("LeftControl + PageDown");
        public KeybindList OTCycleUpObject { get; set; } = KeybindList.Parse("PageUp");
        public KeybindList OTCycleDownObject { get; set; } = KeybindList.Parse("PageDown");

        public KeybindList OTMoveToSelectedObject { get; set; } = KeybindList.Parse("LeftControl + Home");
        public KeybindList OTReadSelectedObject { get; set; } = KeybindList.Parse("Home");

        public KeybindList OTReadSelectedObjectTileLocation { get; set; } = KeybindList.Parse("End");

        public KeybindList OTCancelAutoWalking { get; set; } = KeybindList.Parse("Escape");

        public KeybindList OTSwitchSortingMode { get; set; } = KeybindList.Parse("OemTilde");

        public string OTReadSelectedObjectText { get; set; } = "{object} is at {objectX},{objectY}, player at {playerX},{playerY}";

        public string OTReadSelectedObjectTileText { get; set; } = "{object} is at {objectX},{objectY}, player at {playerX},{playerY}";

    }       
}
