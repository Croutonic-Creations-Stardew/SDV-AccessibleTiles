using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace AccessibleTiles.TrackingMode {
    public class Tracker {

        private ModEntry mod;
        //public GameLocation? last_location;

        String? focus_name;
        String? focus_type;

        public bool sort_by_proxy = false;
        public SortedList<string, Dictionary<string, SpecialObject>> focusable = new();

        private SButton read;
        private SButton cycleup;
        private SButton cycledown;
        private SButton readtile;
        private SButton sort_order_toggle;

        public string[] categories = { "Mining", "Objects", "Crops", "Animals", "Entrances", "Characters", "Resource Clumps", "Bundles", "P O I", "Resources", "FarmBuildings" };

        public Tracker(ModEntry mod) {
            this.mod = mod;
            this.read = mod.Config.TrackingModeRead;
            this.cycleup = mod.Config.TrackingModeCycleUp;
            this.cycledown = mod.Config.TrackingModeCycleDown;
            this.readtile = mod.Config.TrackingModeGetTile;
            this.sort_order_toggle = mod.Config.TrackingToggleSortingMode;
        }

        public void ScanArea(GameLocation location) {
            ScanArea(location, false);
        }


        public void ScanArea(GameLocation location, Boolean? clear_focus) {

            focusable.Clear();

            TrackCategory("FarmBuildings", TrackerUtility.GetBuildings());
            TrackCategory("Objects", TrackerUtility.GetObjects(mod)); //utilizes on FarmBuildings sometimes
            TrackCategory("Resource Clumps", TrackerUtility.GetResourceClumps(mod));
            TrackCategory("Animals", TrackerUtility.GetAnimals(mod));
            TrackCategory("Mining", TrackerUtility.GetMining(mod));
            TrackCategory("Crops", TrackerUtility.GetCrops(mod));
            TrackCategory("Resources", TrackerUtility.GetResources(mod));
            TrackCategory("Bundles", TrackerUtility.GetBundles());
            TrackCategory("Characters", TrackerUtility.GetCharacters());
            TrackCategory("Entrances", TrackerUtility.GetEntrances(mod));
            TrackCategory("P O I", TrackerUtility.GetPOIs(mod));
            TrackCategory("Doors", TrackerUtility.GetDoors());
            TrackCategory("Players", TrackerUtility.GetPlayers(mod));

            if(focusable.Count() > 0 && sort_by_proxy) {
                SortedList<string, Dictionary<string, SpecialObject>> tmp = new();
                foreach (string key in focusable.Keys) {
                    mod.console.Debug("Proxy...");
                    tmp[key] = focusable[key].Values.OrderBy(v => TrackerUtility.GetDistance(Game1.player.getTileLocation(), v.TileLocation)).ToDictionary(x => x.name, x => x);
                }
                foreach(string key in tmp.Keys) {
                    focusable[key] = tmp[key].Values.ToDictionary(x => x.name, x => x);
                }
                tmp.Clear();
            }
            

            if (focus_name == null || focus_type == null || (bool)clear_focus) {
                clearFocus();
            }


        }
        private void TrackCategory(string name, Dictionary<string, SpecialObject> objects) {
            if (objects.Count() > 0) {
                focusable.Add(name, objects);
            }
        }

        public void clearFocus() {
            foreach (string cat in categories) {
                if (focusable.ContainsKey(cat)) {
                    focus_type = cat;
                    object focus = focusable[focus_type].Values.First();
                    focus_name = (focus as SpecialObject).name;
                    return;
                }
            }
        }

        private void say(string text, bool force) {
            if(mod.stardewAccess != null) {
                mod.stardewAccess.Say(text, force);
            }
        }

        public void ChangeCategory(SButton button) {

            if (focus_type != null && focusable.ContainsKey(focus_type)) {
                int index = focusable.IndexOfKey(focus_type);
                index += button == this.cycledown ? 1 : -1;

                if (index >= 0 && index < focusable.Count) {

                    focus_type = focusable.Keys[index];
                    mod.console.Debug("Change Category: " + focus_type);

                    object focus = focusable[focus_type].Values.First();
                    focus_name = (focus as SpecialObject).name;

                }
                this.say(focus_type + " Category,  Focus - " + focus_name, true);

            } else {
                mod.console.Debug(focusable.ToArray().ToString());
                this.say("No Categories Found", true);
            }
        }

        internal void OnButtonPressed(object sender, ButtonPressedEventArgs e) {

            if (mod.stardewAccess == null || Game1.activeClickableMenu != null) {
                return;
            }

            if (e.Button == this.read) {
                if (e.IsDown(SButton.LeftControl) || e.IsDown(SButton.RightControl)) {
                    ReadCurrentFocus(false, true, false);
                } else {
                    ReadCurrentFocus(false, false, false);
                }
            }

            if (e.Button == this.readtile) {
                if (e.IsDown(SButton.LeftControl) || e.IsDown(SButton.RightControl)) {
                    ReadCurrentFocus(true, true, false);
                } else {
                    ReadCurrentFocus(true, false, false);
                }
                
            }

            if (e.Button == this.cycleup || e.Button == this.cycledown) {
                if (e.IsDown(SButton.LeftControl) || e.IsDown(SButton.RightControl)) {
                    ChangeCategory(e.Button);
                } else {
                    ChangeFocus(e.Button);
                }

            }

            if(e.Button == this.sort_order_toggle) {
                mod.console.Debug("Change Sorting... " + sort_by_proxy);
                bool prev_proxy = sort_by_proxy;
                foreach (string key in focusable.Keys) {
                    if (prev_proxy == true) {
                        sort_by_proxy = false;
                        this.say("Sorting by Name", true);
                    } else {
                        sort_by_proxy = true;
                        this.say("Sorting by Proximity", true);
                    }
                }
                mod.console.Debug("Proxy Sort: " + sort_by_proxy.ToString());
            }

        }

        private void ChangeFocus(SButton? button, int? key, string? extra_details) {

            if (focusable.Count() < 1 || !focusable.ContainsKey(focus_type)) {
                this.say("Nothing Found.", true);
                return;
            }

            Dictionary<string, SpecialObject> local_focusable = focusable[focus_type];

            if (extra_details == null) {
                extra_details = "";
            }

            string extra_details_end = "";

            if (key == null) {
                if (button == null) {
                    return;
                }
                int direction = button == this.cycleup ? -1 : 1;
                key = local_focusable.Keys.ToList().IndexOf(focus_name) + direction;
            }

            if (key < 0 || key > local_focusable.Count - 1) {
                //end of list
                extra_details_end += "End of list, ";
            } else {
                object focus = local_focusable.Values.ElementAt((int)key);
                focus_name = (focus as SpecialObject).name;
            }

            this.say(extra_details + $"{focus_name} focused, " + extra_details_end, true);
            mod.console.Debug(extra_details + $"Focused on {focus_name}., " + extra_details_end);
        }

        private void ChangeFocus(SButton button) {
            this.ChangeFocus(button, null, null);
        }

        public Dictionary<string, (NPC, int)> controlled_npcs = new();

        private void ReadCurrentFocus(bool tileOnly, bool autopath, bool faceDirection) {
            if (focus_name != null && focus_type != null) {

                Farmer player = Game1.player;

                mod.console.Debug("run scan");
                ScanArea(player.currentLocation);

                if (!focusable.ContainsKey(focus_type)) {
                    clearFocus();
                    return;
                }

                Dictionary<string, SpecialObject> local_focusable = focusable[focus_type];

                if (!local_focusable.ContainsKey(focus_name)) {
                    this.ChangeFocus(null, 0, $"Can't find {focus_name}, ");
                    return;
                }

                Vector2 position = player.getTileLocation();

                SpecialObject focus = local_focusable[focus_name];
                Vector2 location = focus.TileLocation;
                //str += $"{focus_name} at {focus.TileLocation.X}-{focus.TileLocation.Y}, ";

                if (focus == null || location == Vector2.Zero) {
                    mod.console.Debug("focus is null or location is zero. " + location.ToString());
                    return;
                }

                Vector2 tileXY = location;

                location.X += Game1.tileSize / 4;
                location.Y += Game1.tileSize / 4;

                string direction = TrackerUtility.GetDirection(player.getTileLocation(), tileXY);
                if (faceDirection) {
                    if (direction == "North") {
                        player.faceDirection(0);
                    }
                    if (direction == "East") {
                        player.faceDirection(1);
                    }
                    if (direction == "South") {
                        player.faceDirection(2);
                    }
                    if (direction == "West") {
                        player.faceDirection(3);
                    }
                }

                double distance = Math.Round(TrackerUtility.GetDistance(player.getTileLocation(), tileXY));

                //Game1.currentLocation.TemporarySprites.Add(new TemporaryAnimatedSprite("LooseSprites\\Cursors", new Rectangle(346, 400, 8, 8), 10f, 1, 50, tileXY, flicker: false, flipped: false, layerDepth: 999, 0f, Color.White, 4f, 0f, 0f, 0f));
                //autopath = false;
                if (focus.reachable != false) {
                    if (autopath) {
                        Vector2? closest_tile = null;
                        if (focus.PathfindingOverride != null) {
                            closest_tile = focus.PathfindingOverride;
                        } else {
                            closest_tile = GetClosestTile(tileXY);
                        }

                        mod.console.Debug($"closest tile: {closest_tile}");
                        if (closest_tile != null) {
                            Vector2 tile = (Vector2)closest_tile;

                            if (tileOnly) { //get directions

                                mod.console.Debug("Get Directions to " + tile);
                                string[] cardinal_directions = { };
                                PathFindController controller = new PathFindController(player, Game1.currentLocation, new Point((int)tile.X, (int)tile.Y), -1);

                                Vector2 last_tile = player.getTileLocation();
                                controller.pathToEndPoint.Pop(); //remove current tile
                                foreach (Point p in controller.pathToEndPoint) {
                                    Vector2 new_tile = new(p.X, p.Y);

                                    cardinal_directions = cardinal_directions.Append(TrackerUtility.GetDirection(last_tile, new_tile)).ToArray();
                                    last_tile = new_tile;
                                }

                                string directions = String.Join(" - ", TrackerUtility.get_directions(cardinal_directions));
                                say($"{focus_name} at {directions}");
                            } else {
                                player.UsingTool = false;
                                player.controller = new PathFindController(player, Game1.currentLocation, new Point((int)tile.X, (int)tile.Y), -1, (Character farmer, GameLocation location) => {
                                    direction = TrackerUtility.GetDirection(player.getTileLocation(), tile);
                                    ReadCurrentFocus(false, false, true);
                                    mod.movingWithTracker = false;
                                    Task ignore = UnhaltNPCS();
                                    player.canMove = true;
                                });
                                this.say($"moving near {focus_name}, to {tile.X}-{tile.Y}", true);
                            }
                        } else {
                            this.say($"Could not find path to {focus_name} at {tileXY.X}-{tileXY.Y}.", true);
                        }
                    } else {
                        if (tileOnly) {
                            this.say($"{focus_name} is at {tileXY.X}-{tileXY.Y}, player is at {position.X}-{position.Y}", true);
                        } else {
                            this.say($"{focus_name} is {direction} {distance} tiles, at {tileXY.X}-{tileXY.Y}, player is at {position.X}-{position.Y}", true);
                        }
                    }
                } else {
                    this.say(focus.unreachable_reason, true);
                    mod.console.Debug(focus.unreachable_reason);
                }


            }

        }

        private void say(string text) {
            if(mod.stardewAccess != null) {
                mod.stardewAccess.Say(text, true);
            }
            mod.console.Debug(text);
        }

        public async Task UnhaltNPCS() {
            await Task.Delay(3000);
            foreach (var key_value in controlled_npcs) {
                (NPC, int) npc = key_value.Value;
                npc.Item1.speed = npc.Item2;
            }
            controlled_npcs.Clear();
        }

        private Vector2? GetClosestTile(Vector2 tileXY) {

            int radius = 3;
            int layers = radius - 2;

            Vector2 topLeft = new(tileXY.X - layers, tileXY.Y - layers);
            Vector2 bottomRight = new(tileXY.X + layers, tileXY.Y + layers);

            float currentX = topLeft.X;
            float currentY = topLeft.Y;

            Vector2? closest_tile = null;
            double? closest_tile_distance = null;
            double? closest_tile_distance_to_object = null;

            for (int i = 0; i <= radius * radius; i++) {

                Vector2 tile = new(currentX, currentY);

                //mod.console.Debug($"{i}) {tile}");
                if (currentX == tileXY.X && currentY == tileXY.Y) {
                    currentX++;
                    continue;
                }

                //mod.console.Debug($"Check Tile: {tile}");

                if (!mod.IsColliding(tile)) {

                    PathFindController controller = new PathFindController(Game1.player, Game1.currentLocation, new Point((int)tile.X, (int)tile.Y), -1, eraseOldPathController: true);

                    if (controller.pathToEndPoint != null) {

                        int tile_distance = controller.pathToEndPoint.Count();
                        double distance_to_object = TrackerUtility.GetDistance(tileXY, tile);

                        if (closest_tile_distance == null) {
                            closest_tile = tile;
                            closest_tile_distance = tile_distance;
                            closest_tile_distance_to_object = distance_to_object;
                        }

                        if (tile_distance <= closest_tile_distance && distance_to_object <= closest_tile_distance_to_object) {

                            if (closest_tile == null) {
                                closest_tile = tile;
                                continue;
                            }

                            closest_tile = tile;
                            closest_tile_distance = tile_distance;
                            closest_tile_distance_to_object = distance_to_object;

                        }
                    }
                }

                currentX++;

                if (currentX > bottomRight.X) {
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
    public Vector2? PathfindingOverride;

    public NPC? character;
    public bool reachable = true;

    public string? unreachable_reason;

    public SpecialObject(string name) {
        this.name = name;
    }

    public SpecialObject(string name, Vector2 location) {
        this.name = name;
        this.TileLocation = location;
    }

    public SpecialObject(string name, Vector2 location, Vector2 path_override) {
        this.name = name;
        this.TileLocation = location;
        this.PathfindingOverride = path_override;
    }
}