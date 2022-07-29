using StardewModdingAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessibleTiles {
    class ModConfig {

        public Boolean GridMovementActive { get; set; } = true;

        public KeybindList ToggleGridMovementKey { get; set; } = KeybindList.Parse("I");

        public KeybindList GridMovementOverrideKey { get; set; } = KeybindList.Parse("LeftControl");

    }
}
