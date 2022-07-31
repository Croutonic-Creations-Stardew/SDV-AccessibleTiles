using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessibleTiles.Modules.ObjectTracker {
    internal static class Utility {

        public static double GetDistance(Vector2 player, Vector2 point) {
            double value = Math.Sqrt(Math.Pow(((double)point.X - (double)player.X), 2) + Math.Pow(((double)point.Y - (double)player.Y), 2));
            return Math.Round(value);
        }

        public static string GetDirection(Vector2 start, Vector2 end) {

            double tan_Pi_div_8 = Math.Sqrt(2.0) - 1.0;

            double dx = end.X - start.X;
            double dy = start.Y - end.Y;

            if (Math.Abs(dx) > Math.Abs(dy)) {
                if (Math.Abs(dy / dx) <= tan_Pi_div_8) {
                    return dx > 0 ? "East" : "West";
                } else if (dx > 0) {
                    return dy > 0 ? "Northeast" : "Southeast";
                } else {
                    return dy > 0 ? "Northwest" : "Southwest";
                }
            } else if (Math.Abs(dy) > 0) {
                if (Math.Abs(dx / dy) <= tan_Pi_div_8) {
                    return dy > 0 ? "North" : "South";
                } else if (dy > 0) {
                    return dx > 0 ? "Northeast" : "Northwest";
                } else {
                    return dx > 0 ? "Southeast" : "Southwest";
                }
            } else {
                return "Current Tile";
            }

        }

        public static void FixCharacterMovement() {
            //ripped from the debug cm command
            Game1.player.isEating = false;
            Game1.player.CanMove = true;
            Game1.player.UsingTool = false;
            Game1.player.usingSlingshot = false;
            Game1.player.FarmerSprite.PauseForSingleAnimation = false;
            if (Game1.player.CurrentTool is FishingRod)
                (Game1.player.CurrentTool as FishingRod).isFishing = false;
            if (Game1.player.mount != null) {
                Game1.player.mount.dismount();
            }
        }

        public static string DoCycle(ref string valueToUpdate, string[] object_keys, bool back = false) {

            int count_keys = object_keys.Count();

            string output_suffix = "";

            if (back) {

                int key = 0;
                foreach (string object_key in object_keys) {

                    if (object_key == valueToUpdate) {
                        if (key == 0) {
                            output_suffix = "End of List.";
                            break;
                        }
                        valueToUpdate = object_keys[key - 1];
                        break;
                    }
                    key++;
                }

            } else {

                int key = 0;
                foreach (string object_key in object_keys) {

                    if (object_key == valueToUpdate) {
                        if (key == count_keys - 1) {
                            output_suffix = "End of List.";
                            break;
                        }
                        valueToUpdate = object_keys[key + 1];
                        break;
                    }
                    key++;
                }

            }

            return output_suffix;

        }

        public static Vector2? GetClosestTilePath(Vector2 tilePosition) {

            Vector2 topLeft = new(tilePosition.X - 1, tilePosition.Y - 1);
            Vector2 bottomRight = new(tilePosition.X + 1, tilePosition.Y + 1);

            Vector2? closestTile = null;
            double? closestTileDistance = null;
            double? ClosestTileDistanceToObject = null;

            //store the locations currently being checked. First check directly adjacent tiles
            Dictionary<string, Vector2> checks = new Dictionary<string, Vector2>() {
                { "top", Vector2.Add(topLeft, new(1, 0)) },
                { "right", Vector2.Add(topLeft, new(2, 1)) },
                { "bottom", Vector2.Add(topLeft, new(1, 2)) },
                { "left", Vector2.Add(topLeft, new(0, 1)) },
            };

            for (int i = 0; i < 2; i++) {

                if (i == 1) {

                    //if we hit this code, none of the blocks directly next to the object are navigatable.
                    //so clear the checks, and check the tiles diag from the object
                    checks = new Dictionary<string, Vector2>() {
                        { "topLeft", topLeft },
                        { "topRight", Vector2.Add(bottomRight, new(2, 0)) },
                        { "bottomRight", bottomRight },
                        { "bottomLeft", Vector2.Add(topLeft, new(0, 2)) },
                    };

                }

                foreach (var (qualifier, tile) in checks) {

                    PathFindController controller = new PathFindController(Game1.player, Game1.currentLocation, tile.ToPoint(), -1, eraseOldPathController: true);

                    if (controller.pathToEndPoint != null) {

                        int tile_distance = controller.pathToEndPoint.Count();
                        double distance_to_object = Utility.GetDistance(tilePosition, Game1.player.getTileLocation());

                        if (closestTileDistance == null) {
                            closestTile = tile;
                            closestTileDistance = tile_distance;
                            ClosestTileDistanceToObject = distance_to_object;
                        }

                        if (tile_distance <= closestTileDistance && distance_to_object <= ClosestTileDistanceToObject) {

                            if (closestTile == null) {
                                closestTile = tile;
                                continue;
                            }

                            closestTile = tile;
                            closestTileDistance = tile_distance;
                            ClosestTileDistanceToObject = distance_to_object;

                        }
                    }
                }

                if (closestTile != null) {
                    return closestTile;
                }

            }

            return null;
        }

    }
}
