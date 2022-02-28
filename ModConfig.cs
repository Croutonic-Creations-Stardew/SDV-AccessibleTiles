using StardewModdingAPI;
using StardewModdingAPI.Utilities;

namespace AccessibleTiles {
    /// <summary>The mod configuration model.</summary>
    public class ModConfig {
        /*********
        ** Accessors
        *********/
        /// <summary>The default values.</summary>
        static ModConfig Defaults { get; } = new ModConfig();

        /****
         * * SETTINGS
        ****/
        public bool GridModeActiveByDefault = true;

        /****
        ** Keyboard buttons
        ****/
        /// <summary>The button which opens the menu.</summary>
        public SButton GridCenterPlayerKey { get; set; } = SButton.Home;
        public SButton GridUpKey { get; set; } = SButton.W;
        public SButton GridRightKey { get; set; } = SButton.D;
        public SButton GridDownKey { get; set; } = SButton.S;
        public SButton GridLeftKey { get; set; } = SButton.A;
        public SButton MovementTypeToggle { get; set; } = SButton.I;

        //tracker
        public SButton TrackingModeRead { get; set; } = SButton.Home;
        public SButton TrackingModeCycleUp { get; set; } = SButton.PageUp;
        public SButton TrackingModeCycleDown { get; set; } = SButton.PageDown;
        public SButton TrackingModeGetTile { get; set; } = SButton.End;

        public SButton TrackingToggleSortingMode { get; set; } = SButton.Scroll;


    }
}