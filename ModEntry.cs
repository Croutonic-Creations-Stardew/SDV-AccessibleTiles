using StardewModdingAPI;
using StardewModdingAPI.Events;
using AccessibleTiles.Integrations;
using AccessibleTiles.Modules.GridMovement;
using StardewValley;
using System;
using AccessibleTiles.Modules.ObjectTracker;

namespace AccessibleTiles {
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod {

        public ModConfig Config;
        public ModIntegrations Integrations;

        private GridMovement GridMovement;
        private ObjectTracker ObjectTracker;

        public Boolean IsUsingPathfinding = false;

        public int? LastGridMovementDirection = null;
        public InputButton? LastGridMovementButtonPressed = null;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper) {

            this.Config = this.Helper.ReadConfig<ModConfig>();
            this.GridMovement = new GridMovement(this);
            this.ObjectTracker = new ObjectTracker(this, this.Config);

            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.Input.ButtonsChanged += Input_ButtonsChanged;
            helper.Events.Player.Warped += Player_Warped;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;

        }

        public void Output(string text, bool say = false) {
            this.Monitor.Log(text + (!say ? " (Not Read)" : ""), say ? LogLevel.Info : LogLevel.Debug);
            if(say) {
                Integrations.SRSay(text);
            }
        }

        public ModConfig GetModConfig() {
            return this.Config;
        }

        private void GameLoop_UpdateTicked(object sender, UpdateTickedEventArgs e) {
            if(LastGridMovementButtonPressed != null) {

                

                SButton button = LastGridMovementButtonPressed.Value.ToSButton();
                if (Game1.activeClickableMenu == null && !GridMovement.is_moving && !this.Config.GridMovementOverrideKey.IsDown()  && (this.Helper.Input.IsDown(button) || this.Helper.Input.IsSuppressed(button))) {
                    GridMovement.HandleGridMovement(LastGridMovementDirection.Value, LastGridMovementButtonPressed.Value);
                }
            }
        }

        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e) {
            ObjectTracker.GetLocationObjects();
        }

        private void Player_Warped(object sender, WarpedEventArgs e) {
            GridMovement.PlayerWarped(sender, e);
            ObjectTracker.GetLocationObjects();
        }

        private void Input_ButtonsChanged(object sender, ButtonsChangedEventArgs e) {

            if (Game1.activeClickableMenu != null) return;

            if(this.Config.ToggleGridMovementKey.JustPressed()) {
                this.Config.GridMovementActive = !this.Config.GridMovementActive;

                string output = "Grid Movement Status: " + (this.Config.GridMovementActive ? "Active" : "Inactive");
                Output(output, true);

            } else {

                ObjectTracker.HandleKeys(sender, e);

            }

        }

        private void Input_ButtonPressed(object sender, ButtonPressedEventArgs e) {

            if (Game1.player.controller != null) {

                if(this.Config.OTCancelAutoWalking.JustPressed()) {
                    Game1.player.controller.endBehaviorFunction(Game1.player, Game1.currentLocation);
                }

                this.Helper.Input.Suppress(e.Button);
                return;

            }

            if (GridMovement.is_warping == true) {
                this.Helper.Input.Suppress(e.Button);
            }

            if (Game1.activeClickableMenu == null && !this.Config.GridMovementOverrideKey.IsDown() && e.Button.TryGetStardewInput(out InputButton button)) {
                if (this.Config.GridMovementActive) {
                    foreach (InputButton Button in Game1.options.moveUpButton) {
                        if (button.Equals(Button)) {
                            GridMovement.HandleGridMovement(0, Button);
                            this.Helper.Input.Suppress(e.Button);
                        }
                    }
                    foreach (InputButton Button in Game1.options.moveRightButton) {
                        if (button.Equals(Button)) {
                            GridMovement.HandleGridMovement(1, Button);
                            this.Helper.Input.Suppress(e.Button);
                        }
                    }
                    foreach (InputButton Button in Game1.options.moveDownButton) {
                        if (button.Equals(Button)) {
                            GridMovement.HandleGridMovement(2, Button);
                            this.Helper.Input.Suppress(e.Button);
                        }
                    }
                    foreach (InputButton Button in Game1.options.moveLeftButton) {
                        if (button.Equals(Button)) {
                            GridMovement.HandleGridMovement(3, Button);
                            this.Helper.Input.Suppress(e.Button);
                        }
                    }
                }
            }

        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e) {

            this.Integrations = new ModIntegrations(this);

        }

    }
}