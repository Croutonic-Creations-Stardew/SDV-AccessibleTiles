using StardewModdingAPI;
using StardewModdingAPI.Events;
using AccessibleTiles.Integrations;
using AccessibleTiles.Modules.GridMovement;
using StardewValley;
using System;

namespace AccessibleTiles {
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod {

        private ModConfig Config;
        private ModIntegrations Integrations;

        private GridMovement GridMovement;
        public Boolean log { get; set; }

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper) {

            this.Config = this.Helper.ReadConfig<ModConfig>();
            this.GridMovement = new GridMovement(this);

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;
            helper.Events.Player.Warped += GridMovement.PlayerWarped;

        }

        public void Log(string text) {
            this.Monitor.Log(text, LogLevel.Debug);
        }

        private void Input_ButtonsChanged(object sender, ButtonsChangedEventArgs e) {
            
            if(this.Config.ToggleGridMovementKey.JustPressed()) {
                this.Config.GridMovementActive = !this.Config.GridMovementActive;

                string output = "Grid Movement Status: " + (this.Config.GridMovementActive ? "Active" : "Inactive");
                Integrations.SRSay(output);
                Log(output);

            }

        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e) {

            if(GridMovement.is_warping == true) {
                this.Helper.Input.Suppress(e.Button);
            }

            if (!this.Config.GridMovementOverrideKey.IsDown() && e.Button.TryGetStardewInput(out InputButton button)) {
                if (this.Config.GridMovementActive) {
                    foreach (InputButton Button in Game1.options.moveUpButton) {
                        if (button.Equals(Button)) {
                            GridMovement.HandleGridMovement(0);
                            this.Helper.Input.Suppress(e.Button);
                        }
                    }
                    foreach (InputButton Button in Game1.options.moveRightButton) {
                        if (button.Equals(Button)) {
                            GridMovement.HandleGridMovement(1);
                            this.Helper.Input.Suppress(e.Button);
                        }
                    }
                    foreach (InputButton Button in Game1.options.moveDownButton) {
                        if (button.Equals(Button)) {
                            GridMovement.HandleGridMovement(2);
                            this.Helper.Input.Suppress(e.Button);
                        }
                    }
                    foreach (InputButton Button in Game1.options.moveLeftButton) {
                        if (button.Equals(Button)) {
                            GridMovement.HandleGridMovement(3);
                            this.Helper.Input.Suppress(e.Button);
                        }
                    }
                }
            }

        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e) {

            this.Integrations = new ModIntegrations(this.Helper);

        }

    }
}