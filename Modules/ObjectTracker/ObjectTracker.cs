using Microsoft.Xna.Framework;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace AccessibleTiles.Modules.ObjectTracker {
    internal class ObjectTracker {

        private Boolean sortByProxy = true;

        private readonly ModEntry Mod;
        private readonly ModConfig ModConfig;

        private TrackedObjects TrackedObjects;

        //private Dictionary<NPC, int> haltedNPCs = new();
        private Vector2? LastTargetedTile = null;

        public string SelectedCategory;
        public string SelectedObject;


        //stop player from moving too fast
        int msBetweenCheckingPathfindingController = 1000;
        Timer checkPathingTimer = new Timer();

        int pathfindingRetryAttempts = 0;

        //stop player from moving too fast
        Timer footstepTimer = new Timer();

        public ObjectTracker(ModEntry mod, ModConfig config) {
            this.Mod = mod;
            this.ModConfig = config;

            //set is_moving after x time to allow the next grid movement
            checkPathingTimer.Interval = msBetweenCheckingPathfindingController;
            checkPathingTimer.Elapsed += checkPathingTimer_Elapsed;

            footstepTimer.Interval = mod.GridMovement.minMillisecondsBetweenSteps + 50;
            footstepTimer.Elapsed += footstepTimer_Elapsed;
        }

        private void footstepTimer_Elapsed(object sender, ElapsedEventArgs e) {
            Farmer player = Game1.player;
            player.currentLocation.playTerrainSound(player.getTileLocation());
        }

        private void checkPathingTimer_Elapsed(object sender, ElapsedEventArgs e) {

            Farmer player = Game1.player;
            GameLocation location = Game1.currentLocation;

            if (player.controller != null && (Game1.activeClickableMenu == null || Game1.IsMultiplayer)) {
                if (player.controller.timerSinceLastCheckPoint > 500) {

                    if (IsFocusValid() && pathfindingRetryAttempts < 5) {
                        pathfindingRetryAttempts++;

                        this.Mod.Output($"Attempting to restart pathfinding attempt {pathfindingRetryAttempts}");

                        if(pathfindingRetryAttempts == 1) {
                            //move around NPC?
                            Dictionary<string, SpecialObject>? characters = TrackedObjects.GetObjects()["characters"];

                            if(characters != null) {
                                foreach (var kvp in characters) {

                                    NPC character = kvp.Value.character;

                                    if(character.getTileLocation() == player.getTileLocation() && !character.IsInvisible) {
                                        character.IsInvisible = true;

                                        //runs after x millseconds, according to function
                                        Task ignore = SetCharacterVisible(character);
                                    }
                                }
                            }

                        }
                        
                    } else {
                        pathfindingRetryAttempts = 0;
                        player.controller.endBehaviorFunction(player, location);
                        GetLocationObjects(reset_focus: true);

                        this.Mod.Output("Pathfinding forcibly stopped. Target Lost.", true);
                        player.controller = null;
                    }
                }
            }
        }

        private async Task SetCharacterVisible(NPC npc) {
            await Task.Delay(100);
            npc.IsInvisible = false;
        }
        public void HandleKeys(object sender, ButtonsChangedEventArgs e) {

            if (ModConfig.OTCycleUpCategory.JustPressed()) {
                CycleCategory(back: true);
            } else if (ModConfig.OTCycleDownCategory.JustPressed()) {
                CycleCategory();
            } else if (ModConfig.OTCycleUpObject.JustPressed()) {
                CycleObjects(back: true);
            } else if (ModConfig.OTCycleDownObject.JustPressed()) {
                CycleObjects();
            } else if (ModConfig.OTReadSelectedObject.JustPressed()) {
                GetLocationObjects(reset_focus: false);
                ReadCurrentlySelectedObject();
            } else if(ModConfig.OTSwitchSortingMode.JustPressed()) {
                this.sortByProxy = !this.sortByProxy;

                this.Mod.Output("Sort By Proximity: " + (sortByProxy ? "Enabled" : "Disabled"), true);
                GetLocationObjects(reset_focus: false);

            }

            if (ModConfig.OTMoveToSelectedObject.JustPressed()) {
                GetLocationObjects(reset_focus: false);
                MoveToCurrentlySelectedObject();
            } else if (ModConfig.OTReadSelectedObjectTileLocation.JustPressed()) {
                GetLocationObjects(reset_focus: false);
                ReadCurrentlySelectedObject(readTileOnly: true);
            }

        }

        private Boolean IsFocusValid() {
            SortedList<string, Dictionary<string, SpecialObject>> objects = TrackedObjects.GetObjects();
            if (objects.ContainsKey(SelectedCategory) && objects[SelectedCategory].ContainsKey(SelectedObject)) {
                return true;
            }
            return false;
        }

        private void MoveToCurrentlySelectedObject() {

            this.Mod.Output($"Attempt pathfinding.", true);
            pathfindingRetryAttempts = 0;

            if (this.IsFocusValid()) {
                ReadCurrentlySelectedObject();
            }

            Farmer player = Game1.player;
            SpecialObject sObject = GetCurrentlySelectedObject();

            Vector2 playerTile = player.getTileLocation();
            Vector2 sObjectTile = sObject.TileLocation;

            Vector2? closestTile = null;
            if (sObject.PathfindingOverride != null) {
                closestTile = Utility.GetClosestTilePath((Vector2)sObject.PathfindingOverride);
            } else {
                closestTile = Utility.GetClosestTilePath(sObjectTile);
            }

            if (closestTile != null) {

                /*if(sObject.character != null) {
                    haltedNPCs.Add(sObject.character, sObject.character.speed);
                    sObject.character.speed = 0;
                }*/

                this.Mod.Output($"Moving to {closestTile.Value.X}-{closestTile.Value.Y}.", true);
                StartPathfinding(player, Game1.currentLocation, closestTile.Value.ToPoint());
                

            } else {

                this.Mod.Output("Could not find path to object.", true);

            }

        }

        private void StartPathfinding(Farmer player, GameLocation location, Point targetTile, int direction = -1) {

            LastTargetedTile = targetTile.ToVector2();

            footstepTimer.Stop();
            checkPathingTimer.Stop();
            footstepTimer.Start();

            checkPathingTimer.Start();

            player.controller = new PathFindController(player, location, targetTile, direction, (Character farmer, GameLocation location) => {
                this.StopPathfinding();
            });

        }

        private void StopPathfinding() {

            Farmer player = Game1.player;
            footstepTimer.Stop();

            ReadCurrentlySelectedObject();
            Utility.FixCharacterMovement();
            player.controller = null;
            checkPathingTimer.Stop();

            //Task unhalt = UnhaltNPCS();

            if(LastTargetedTile != null) {
                string faceDirection = Utility.GetDirection(player.getTileLocation(), LastTargetedTile.Value);
                if (faceDirection == "North") {
                    player.faceDirection(0);
                }
                if (faceDirection == "East") {
                    player.faceDirection(1);
                }
                if (faceDirection == "South") {
                    player.faceDirection(2);
                }
                if (faceDirection == "West") {
                    player.faceDirection(3);
                }
            }

        }

        /*private async Task UnhaltNPCS() {
            await Task.Delay(3000);
            foreach (var npcs in haltedNPCs) {
                npcs.Key.speed = npcs.Value;
            }
            haltedNPCs.Clear();
        }*/

        private void ReadCurrentlySelectedObject(bool readTileOnly = false) {

            SortedList<string, Dictionary<string, SpecialObject>> objects = TrackedObjects.GetObjects();

            if (!(objects.ContainsKey(SelectedCategory) && objects[SelectedCategory].ContainsKey(SelectedObject))) {
                this.Mod.Output($"No Object Selected", true);
                return;
            }

            Farmer player = Game1.player;
            SpecialObject sObject = GetCurrentlySelectedObject();

            Vector2 playerTile = player.getTileLocation();
            Vector2 sObjectTile = sObject.TileLocation;

            string direction = Utility.GetDirection(playerTile, sObject.TileLocation);
            string distance = Utility.GetDistance(playerTile, sObject.TileLocation).ToString();

            this.Mod.Output(ReplacePlaceholders(readTileOnly ? this.ModConfig.OTReadSelectedObjectTileText : this.ModConfig.OTReadSelectedObjectText, playerTile, sObjectTile, direction, distance), true);

        }

        private string ReplacePlaceholders(string s, Vector2 playerTile, Vector2 sObjectTile, string direction, string distance) {
            StringBuilder sb = new StringBuilder(s);

            sb.Replace("{object}", SelectedObject);

            sb.Replace("{objectX}", $"{sObjectTile.X}");
            sb.Replace("{objectY}", $"{sObjectTile.Y}");
            sb.Replace("{playerX}", $"{playerTile.X}");
            sb.Replace("{playerY}", $"{playerTile.Y}");
            sb.Replace("{direction}", $"{direction}");
            sb.Replace("{distance}", $"{distance}");

            return sb.ToString().ToLower();
        }

        private void CycleCategory(bool back = false) {

            SortedList<string, Dictionary<string, SpecialObject>> objects = TrackedObjects.GetObjects();

            string[] object_keys = objects.Keys.ToArray();

            if (!object_keys.Contains(SelectedCategory)) {
                this.Mod.Output("No Categories Found", true);
            }

            string suffix_text = Utility.DoCycle(ref SelectedCategory, object_keys, back);
            this.SetFocusedObjectToFirstInCategory();

            if (suffix_text.Length > 0) {
                suffix_text = ", " + suffix_text;
            }

            this.Mod.Output($"{SelectedCategory}, {SelectedObject}" + suffix_text, true);

        }

        private void CycleObjects(bool back = false) {

            SortedList<string, Dictionary<string, SpecialObject>> objects = TrackedObjects.GetObjects();

            string[] categories = objects.Keys.ToArray();

            if (!categories.Contains(SelectedCategory)) {
                this.Mod.Output("No Categories Found", true);
            }

            string[] object_keys = objects[SelectedCategory].Keys.ToArray();

            string suffix_text = Utility.DoCycle(ref SelectedObject, object_keys, back);

            if(suffix_text.Length > 0) {
                suffix_text = ", " + suffix_text;
            }

            this.Mod.Output($"{SelectedObject}, {SelectedCategory}" + suffix_text, true);

        }

        private SpecialObject? GetCurrentlySelectedObject() {
            return TrackedObjects.GetObjects()[SelectedCategory][SelectedObject];
        }
        private void SetFocusedObjectToFirstInCategory() {

            var objects = TrackedObjects.GetObjects();

            if(objects.ContainsKey(SelectedCategory)) {
                Dictionary<string, SpecialObject> cat_objects = objects[SelectedCategory];
                SelectedObject = cat_objects.Keys.ToArray()[0];
            }

        }

        private void SetDefaultCategoryAndFocusedObject() {

            var objects = TrackedObjects.GetObjects();

            if(TrackedObjects.GetObjects().Count() < 1) {
                this.Mod.Output("No objects found.");
            } else {

                SelectedCategory = objects.Keys[0];
                
                Dictionary<string, SpecialObject> cat_objects = objects[SelectedCategory];
                SelectedObject = cat_objects.Keys.ToArray()[0];

                this.Mod.Output($"Category: {SelectedCategory} | Object: {SelectedObject}");

            }
        }

        internal void GetLocationObjects(bool reset_focus = true) {
            TrackedObjects tracked_objects = new TrackedObjects(this.Mod);
            tracked_objects.FindObjectsInArea(!this.sortByProxy);
            this.TrackedObjects = tracked_objects;

            if(!reset_focus) {
                if(!tracked_objects.GetObjects().ContainsKey(SelectedCategory)) {
                    reset_focus = true;
                }
            }

            if(reset_focus) {
                this.SetDefaultCategoryAndFocusedObject();
            }

            if(!tracked_objects.GetObjects()[SelectedCategory].ContainsKey(SelectedObject) && tracked_objects.GetObjects().ContainsKey(SelectedCategory)) {
                this.SetFocusedObjectToFirstInCategory();
            }

        }
    }
}
