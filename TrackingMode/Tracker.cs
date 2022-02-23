using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.Linq;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace AccessibleTiles.TrackingMode {
    public class Tracker {
        
        private ModEntry mod;
        //public GameLocation? last_location;

        String? focus_name;
        String? focus_type;

        SortedList<string, SortedList<string, SpecialObject>> focusable = new();

        private SButton read;
        private SButton cycleup;
        private SButton cycledown;
        private SButton readtile;

        public string[] categories = { "Mining", "Objects", "Crops", "Animals", "Entrances", "Characters", "Resource Clumps", "Bundles", "P O I", "Resources", "FarmBuildings" };

        public Tracker(ModEntry mod, SButton read, SButton cycleup, SButton cycledown, SButton readtile) {
            this.mod = mod;
            this.read = read;
            this.cycleup = cycleup;
            this.cycledown = cycledown;
            this.readtile = readtile;
        }

        public void ScanArea(GameLocation location) {
            ScanArea(location, false);
        }


        public void ScanArea(GameLocation location, Boolean? clear_focus) {

            if (mod.stardewAccess == null) {
                return;
            }

            focusable.Clear();

            TrackCategory("Objects", TrackerUtility.GetObjects(mod));
            TrackCategory("Resource Clumps", TrackerUtility.GetResourceClumps(mod));
            TrackCategory("Animals", TrackerUtility.GetAnimals(mod));
            TrackCategory("Mining", TrackerUtility.GetMining(mod));
            TrackCategory("Crops", TrackerUtility.GetCrops(mod));
            TrackCategory("Resources", TrackerUtility.GetResources(mod));
            TrackCategory("Bundles", TrackerUtility.GetBundles());
            TrackCategory("Characters", TrackerUtility.GetCharacters());
            TrackCategory("Entrances", TrackerUtility.GetEntrances());
            TrackCategory("FarmBuildings", TrackerUtility.GetBuildings());
            TrackCategory("P O I", TrackerUtility.GetPOIs());

            if (focus_name == null || focus_type == null || (bool)clear_focus) {
                clearFocus();
            }


        }
        private void TrackCategory(string name, SortedList<string, SpecialObject> objects) {
            if (objects.Count() > 0) {
                focusable.Add(name, objects);
            }
        }

        public void clearFocus() {
            foreach(string cat in categories) {
                if(focusable.ContainsKey(cat)) {
                    focus_type = cat;
                    object focus = focusable[focus_type].Values[0];
                    focus_name = (focus as SpecialObject).name;
                    return;
                }
            }
        }

        public void ChangeCategory(SButton button) {

            if (focus_type != null && focusable.ContainsKey(focus_type)) {
                int index = focusable.IndexOfKey(focus_type);
                index += button == this.cycledown ? 1 : -1;

                if (index >= 0 && index < focusable.Count) {

                    focus_type = focusable.Keys[index];
                    mod.console.Debug("Change Category: " + focus_type);

                    object focus = focusable[focus_type].Values[0];
                    focus_name = (focus as SpecialObject).name;

                }
                mod.stardewAccess.Say(focus_type + " Category,  Focus - " + focus_name, true);

            } else {
                mod.console.Debug(focusable.ToArray().ToString());
                mod.stardewAccess.Say("No Categories Found", true);
            }
        }
        
        internal void OnButtonPressed(object sender, ButtonPressedEventArgs e) {

            if (mod.stardewAccess == null || Game1.activeClickableMenu != null) {
                return;
            }

            if (e.Button == this.read) {
                if (e.IsDown(SButton.LeftControl) || e.IsDown(SButton.RightControl)) {
                    ReadCurrentFocus(false, true);
                } else {
                    ReadCurrentFocus(false, false);
                }
            }

            if (e.Button == this.readtile) {
                ReadCurrentFocus(true, false);
            }

            if (e.Button == this.cycleup || e.Button == this.cycledown) {
                if(e.IsDown(SButton.LeftControl) || e.IsDown(SButton.RightControl)) {
                    ChangeCategory(e.Button);
                } else {
                    ChangeFocus(e.Button);
                }
                
            }

        }

        private void ChangeFocus(SButton? button, int? key, string? extra_details) {

            if (focusable.Count() < 1 || !focusable.ContainsKey(focus_type)) {
                mod.stardewAccess.Say("Nothing Found.", true);
                return;
            }

            SortedList<string, SpecialObject> local_focusable = focusable[focus_type];

            if (extra_details == null) {
                extra_details = "";
            }

            string extra_details_end = "";

            if (key == null) {
                if (button == null) {
                    return;
                }
                int direction = button == this.cycleup ? -1 : 1;
                key = local_focusable.IndexOfKey(focus_name) + direction;
            }

            if (key < 0 || key > local_focusable.Count - 1) {
                //end of list
                extra_details_end += "End of list, ";
            } else {
                object focus = local_focusable.Values[(int)key];
                focus_name = (focus as SpecialObject).name;
            }

            mod.stardewAccess.Say(extra_details + $"{focus_name} focused, " + extra_details_end, true);
            mod.console.Debug(extra_details + $"Focused on {focus_name}., " + extra_details_end);
        }

        private void ChangeFocus(SButton button) {
            this.ChangeFocus(button, null, null);
        }

        private void ReadCurrentFocus(bool tileOnly, bool autopath) {
            if (focus_name != null && focus_type != null) {

                mod.console.Debug("run scan");
                ScanArea(Game1.player.currentLocation);

                if(!focusable.ContainsKey(focus_type)) {
                    clearFocus();
                    return;
                }

                SortedList<string, SpecialObject> local_focusable = focusable[focus_type];

                if (!local_focusable.ContainsKey(focus_name)) {
                    this.ChangeFocus(null, 0, $"Can't find {focus_name}, ");
                    return;
                }

                Vector2 position = Game1.player.getTileLocation();

                object focus = local_focusable[focus_name];
                Vector2 location = (focus as SpecialObject).TileLocation;
                //str += $"{focus_name} at {focus.TileLocation.X}-{focus.TileLocation.Y}, ";

                if(focus == null || location == Vector2.Zero) {
                    mod.console.Debug("focus is null or location is zero. " + location.ToString());
                    return;
                }

                Vector2 tileXY = location;

                location.X += Game1.tileSize / 4;
                location.Y += Game1.tileSize / 4;

                string direction = TrackerUtility.GetDirection(Game1.player.getTileLocation(), tileXY);
                double distance = Math.Round(TrackerUtility.GetDistance(Game1.player.getTileLocation(), tileXY));

                //Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(346, 400, 8, 8), 10f, 1, 50, tileXY, flicker: false, flipped: false, layerDepth: 999, 0f, Color.White, 4f, 0f, 0f, 0f));
                autopath = false;
                if(autopath) {
                    Vector2? closest_tile = GetClosestTile(tileXY);
                    if (closest_tile != null) {
                        Game1.player.controller = new PathFindController(Game1.player, Game1.currentLocation, new Point((int)closest_tile.Value.X, (int)closest_tile.Value.Y), -1);
                    }
                } else {
                    if (tileOnly) {
                        mod.stardewAccess.Say($"{focus_name} is at {tileXY.X}-{tileXY.Y}, player is at {position.X}-{position.Y}", true);
                    } else {
                        mod.stardewAccess.Say($"{focus_name} is {direction} {distance} tiles, at {tileXY.X}-{tileXY.Y}, player is at {position.X}-{position.Y}", true);
                    }
                }
                
                mod.console.Debug($"{focus_name} is {direction} {distance} tiles, at {tileXY.X}-{tileXY.Y}, player is at {position.X}-{position.Y}");
            } else {
                mod.console.Debug("no focus no type");
            }

        }

        private Vector2? GetClosestTile(Vector2 tileXY) {

            mod.console.Debug("Finding tile...");

            int radius = 1;

            Vector2 topLeft = new(tileXY.X - radius, tileXY.Y - radius);
            Vector2 bottomRight = new(tileXY.X + radius, tileXY.Y + radius);

            float currentX = topLeft.X;
            float currentY = topLeft.Y;

            Vector2? closest_tile = null;

            for(int i = 0; i < 9 * radius; i++) {
                if (currentX == tileXY.X && currentY == tileXY.Y) continue;

                Vector2 tile = new(currentX, currentY);

                if (!mod.IsColliding(tile)) {
                    if(closest_tile != null) {
                        tile = TrackerUtility.GetClosest(tile, (Vector2)closest_tile);
                    }
                    closest_tile = tile;
                }

                currentX++;

                if(currentX > bottomRight.X) {
                    currentX = topLeft.X;
                    currentY++;
                }
            }

            return closest_tile;
        }
    }

}

public class SpecialObject {

    public string name;
    public Vector2 TileLocation;

    public SpecialObject(string name, Vector2 location) {
        this.name = name;
        this.TileLocation = location;
    }
}
