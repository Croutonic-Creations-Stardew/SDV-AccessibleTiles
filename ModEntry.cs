using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AccessibleTiles.Integrations;
using AccessibleTiles.TrackingMode;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using xTile.Dimensions;
using xTile.Layers;
using xTile.ObjectModel;
using xTile.Tiles;

namespace AccessibleTiles {
    /// <summary>The mod entry point.</summary>
    public class ModEntry : Mod {

        private IModHelper helper;
        private Boolean is_warping = false;

        public Tracker trackingMode;

        private SButton? last_button = null;

        private Dictionary<SButton, int> key_map = new();

        private Boolean grid_movement_active;

        private ModConfig Config;

        public StardewAccessInterface? stardewAccess;

        public ConsoleUtil console;

        public bool movingWithTracker = false;

        /*********
        ** Public methods
        *********/
        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper) {

            this.helper = helper;
            this.console = new ConsoleUtil(this.Monitor);


            this.Config = helper.ReadConfig<ModConfig>();

            grid_movement_active = Config.GridModeActiveByDefault;

            helper.Events.GameLoop.UpdateTicked += this.UpdateTicked;
            helper.Events.GameLoop.OneSecondUpdateTicked += this.OneSecondUpdateTicked;
            helper.Events.Input.ButtonPressed += this.OnButtonPressed;
            helper.Events.GameLoop.SaveLoaded += GameLoop_SaveLoaded;
            helper.Events.Player.Warped += Player_Warped;

            key_map.Add(Config.GridUpKey, 0);
            key_map.Add(Config.GridRightKey, 1);
            key_map.Add(Config.GridDownKey, 2);
            key_map.Add(Config.GridLeftKey, 3);

            key_map.Add(SButton.DPadUp, 0);
            key_map.Add(SButton.DPadRight, 1);
            key_map.Add(SButton.DPadDown, 2);
            key_map.Add(SButton.DPadLeft, 3);

            trackingMode = new Tracker(this, Config.TrackingModeRead, Config.TrackingModeCycleUp, Config.TrackingModeCycleDown, Config.TrackingModeGetTile);

        }

        private void Player_Warped(object sender, WarpedEventArgs e) {
            if (this.is_warping) {
                this.is_warping = false;
            }
            trackingMode.ScanArea(e.NewLocation, clear_focus: true);
        }


        private void GameLoop_SaveLoaded(object sender, SaveLoadedEventArgs e) {
            Boolean stardewAccessLoaded = helper.ModRegistry.IsLoaded("shoaib.stardewaccess");
            if (stardewAccessLoaded) {
                stardewAccess = helper.ModRegistry.GetApi<StardewAccessInterface>("shoaib.stardewaccess");
            }
            trackingMode.ScanArea(Game1.player.currentLocation);
        }

        private void OnButtonPressed(object sender, ButtonPressedEventArgs e) {
            // ignore if player hasn't loaded a save yet, make sure no interface is open, and make sure player can move
            if (!Context.IsWorldReady || Game1.activeClickableMenu != null || !Game1.player.CanMove)
                return;

            // print button presses to the console window
            //this.Monitor.Log($"{Game1.player.Name} pressed {e.Button}.", LogLevel.Debug);

            int direction = -1;

            if (key_map.ContainsKey(e.Button)) {
                direction = key_map[e.Button];
                if (Game1.player.controller != null) {
                    ClearPathfindingController();
                }

            }

            if (e.Button == Config.GridCenterPlayerKey) {
                CenterPlayer();
            }

            if (e.Button == Config.MovementTypeToggle) {
                grid_movement_active = !grid_movement_active;

                if (stardewAccess != null) {
                    if (grid_movement_active) {
                        stardewAccess.Say("Activated Grid Movement", true);
                    } else {
                        stardewAccess.Say("Deactivated Grid Movement", true);
                    }
                }
            }

            if (direction != -1) {

                if (grid_movement_active) {
                    last_button = e.Button;
                    HandleArrowMovement(direction);
                }

            }

            trackingMode.OnButtonPressed(sender, e);

        }

        private void ClearPathfindingController() {
            Game1.player.controller = null;
            movingWithTracker = false;
        }

        private void CenterPlayer() {

            var position = Game1.player.Position;

            position.X = (int)Math.Round(position.X / Game1.tileSize) * Game1.tileSize;
            position.Y = (int)Math.Round(position.Y / Game1.tileSize) * Game1.tileSize;

            Game1.player.Position = position;

        }

        private bool hasCheckedBed = false;

        private void HandleArrowMovement(int direction) {

            //stop if player is using their tool, otherwise the player may glitch out.
            if (Game1.player.UsingTool || this.is_warping) {
                return;
            }

            //first, center player to current tile
            CenterPlayer();

            Game1.player.CanMove = false;

            if (Game1.player.FacingDirection != direction) {
                Game1.player.faceDirection(direction);
                Game1.playSound("dwop");
                return;
            }

            var position = Game1.player.Position;
            var X = (int)Math.Round(position.X / Game1.tileSize);
            var Y = (int)Math.Round(position.Y / Game1.tileSize);

            switch (direction) {
                case 0:
                    Y -= 1;
                    break;
                case 1:
                    X += 1;
                    break;
                case 2:
                    Y += 1;
                    break;
                case 3:
                    X -= 1;
                    break;
            }

            position.X = X * Game1.tileSize;
            position.Y = Y * Game1.tileSize;

            var tile_vector = new Vector2(X, Y);

            var location = Game1.player.currentLocation;

            if (!IsColliding(tile_vector)) {
                Game1.player.Position = position;
                location.playTerrainSound(tile_vector);
                Game1.player.CanMove = true;
                hasCheckedBed = false;
            } else {

                //check if player is trying to warp
                Warp warp = location.isCollidingWithWarpOrDoor(new Microsoft.Xna.Framework.Rectangle((int)position.X, (int)position.Y, Game1.tileSize, Game1.tileSize));
                if (warp != null) {
                    Game1.playSound("doorClose");
                    Game1.player.warpFarmer(warp);
                    this.is_warping = true;
                }

                //check if player is trying to collide with an object
                if (Game1.currentLocation.isObjectAtTile(X, Y)) {

                    StardewValley.Object obj = Game1.currentLocation.getObjectAtTile(X, Y);
                    String object_name = obj.DisplayName;

                    if (object_name.ToLower().Contains("bed")) {
                        if (hasCheckedBed == false) {
                            Vector2 bed_position = obj.TileLocation;
                            bed_position.Y += 1;
                            bed_position *= Game1.tileSize;
                            //bed_position.X += 50;
                            Game1.player.Position = bed_position;
                            location.createQuestionDialogue(Game1.content.LoadString("Strings\\Locations:FarmHouse_Bed_GoToSleep"), location.createYesNoResponses(), "Sleep");
                            hasCheckedBed = true;
                        }
                    }

                }

            }

        }

        public bool IsColliding(Vector2 tile_vector) {

            var location = Game1.player.currentLocation;

            int X = (int)tile_vector.X;
            int Y = (int)tile_vector.Y;

            Boolean pass_feature_check = true;
            Boolean force_pass = false;
            if (location.resourceClumps.Count > 0) {
                for (int i = 0; i < location.resourceClumps.Count; i++) {
                    ResourceClump feature = location.resourceClumps[i];

                    if (feature.occupiesTile(X, Y)) {
                        pass_feature_check = false;
                    }
                }
            }

            //detect animal collision
            List<FarmAnimal>? farmAnimals = null;
            if (location is Farm) {
                farmAnimals = (location as Farm).getAllFarmAnimals();
            } else if (location is AnimalHouse) {
                farmAnimals = (location as AnimalHouse).animals.Values.ToList();
            }
            if (farmAnimals != null) {
                foreach (FarmAnimal animal in farmAnimals) {
                    if (animal.getTileLocation() == tile_vector) {
                        force_pass = true;
                    }
                }
            }

            if (location.objects.Count() > 0 && location.objects.ContainsKey(tile_vector)) {
                StardewValley.Object feature = location.objects[tile_vector];

                if (feature.name.ToLower().Contains("gate")) {
                    force_pass = true;
                }
            }

            //check tile indexes?
            int back_index = location.getTileIndexAt(X, Y, "Back");
            string passable = location.doesTileHaveProperty(X, Y, "Passable", "Back");
            //console.Debug();

            if (back_index == 107 || back_index == 362 || back_index == 1274 || back_index == 1244) {
                force_pass = true;
                Game1.playSound("woodyStep");
            }

            bool answer = !(!location.isTileOccupiedIgnoreFloors(tile_vector) &&
                location.isTilePassable(new Location(X, Y), Game1.viewport) &&
                !location.isWaterTile(X, Y) &&
                !location.isOpenWater(X, Y) &&
                !location.isTileOccupiedForPlacement(tile_vector) &&
                pass_feature_check ||
                location.isCropAtTile(X, Y) ||
                force_pass);

            //console.Debug(answer.ToString() + " - " + back_index.ToString());
            console.Debug($"Check {X},{Y}");

            return answer;
        }

        private int reset_on_tick_count = 15;
        private int held_for_ticks = 0;
        private int moved_for_ticks = 0;

        private void UpdateTicked(object sender, UpdateTickedEventArgs e) {

            if (!Context.IsWorldReady)
                return;

            if (last_button != null) {

                var buttonState = this.Helper.Input.GetState((SButton)last_button);
                if (buttonState == SButtonState.Held || buttonState == SButtonState.Pressed) {
                    Game1.player.CanMove = false;
                    held_for_ticks++;

                    int tick_count = reset_on_tick_count;

#if DEBUG
                    var ctrlState = this.Helper.Input.GetState(SButton.LeftControl);
                    if (ctrlState == SButtonState.Held) {
                        tick_count /= 2;
                    }
#endif

                    if (held_for_ticks > tick_count) {
                        HandleArrowMovement(key_map[(SButton)last_button]);
                        held_for_ticks = 0;
                    }

                } else {
                    last_button = null;
                    held_for_ticks = 0;
                    Game1.player.CanMove = true;
                }

            }

            if (movingWithTracker) {
                moved_for_ticks++;

                if (moved_for_ticks > reset_on_tick_count) {
                    Game1.currentLocation.playTerrainSound(Game1.player.getTileLocation(), Game1.player);
                    moved_for_ticks = 0;
                }
            } else {
                moved_for_ticks = 0;
                movingWithTracker = false;

                if (trackingMode.controlled_npcs.Any()) {
                    Task ignore = trackingMode.UnhaltNPCS();
                }
            }

            if (trackingMode.controlled_npcs.Any() && Game1.player.controller != null && Game1.activeClickableMenu != null) {
                Game1.player.controller = null;
                movingWithTracker = false;
                Task ignore = trackingMode.UnhaltNPCS();
            }

        }

        private void OneSecondUpdateTicked(object sender, OneSecondUpdateTickedEventArgs e) {

            if (!Context.IsWorldReady)
                return;

            /*if(movingWithTracker) {
                console.Debug("Play sound");
                Game1.currentLocation.playTerrainSound(Game1.player.getTileLocation(), Game1.player);
            }*/

        }

    }
}