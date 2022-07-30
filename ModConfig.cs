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
        public KeybindList ObjectTrackerCycleUpCategory { get; set; } = KeybindList.Parse("LeftControl + PageUp");
        public KeybindList ObjectTrackerCycleDownCategory { get; set; } = KeybindList.Parse("LeftControl + PageDown");
        public KeybindList ObjectTrackerCycleUpObject { get; set; } = KeybindList.Parse("PageUp");
        public KeybindList ObjectTrackerCycleDownObject { get; set; } = KeybindList.Parse("PageDown");

        public KeybindList ObjectTrackerMoveToSelectedObject { get; set; } = KeybindList.Parse("LeftControl + Home");
        public KeybindList ObjectTrackerReadSelectedObject { get; set; } = KeybindList.Parse("Home");

        public KeybindList ObjectTrackerReadSelectedObjectTileLocation { get; set; } = KeybindList.Parse("End");

        public KeybindList ObjectTrackerCancelAutoWalking { get; set; } = KeybindList.Parse("Escape");

        public KeybindList ObjectTrackerSwitchSortingMode { get; set; } = KeybindList.Parse("OemTilde");

    }
}
