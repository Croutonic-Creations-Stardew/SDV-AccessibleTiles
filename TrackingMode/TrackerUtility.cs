using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AccessibleTiles.TrackingMode {
    internal static class TrackerUtility {

        static readonly double tan_Pi_div_8 = Math.Sqrt(2.0) - 1.0;

        public enum Direction {
            North, South, East, West, Northeast, Northwest, Southeast, Southwest
        }

        static Dictionary<Direction, string> directions = new() {
            { Direction.North, "North" },
            { Direction.South, "South" },
            { Direction.East, "East" },
            { Direction.West, "West" },
            { Direction.Northeast, "Northeast" },
            { Direction.Northwest, "Northwest" },
            { Direction.Southeast, "Southeast" },
            { Direction.Southwest, "Southwest" }
        };

        public static double GetDistance(Vector2 player, Vector2 point) {
            return Math.Sqrt(Math.Pow(((double)point.X - (double)player.X), 2) + Math.Pow(((double)point.Y - (double)player.Y), 2));
        }

        public static Vector2? get_theater_entrance() {
            if (Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheater")) {
                if (Utility.doesMasterPlayerHaveMailReceivedButNotMailForTomorrow("ccMovieTheaterJoja")) {
                    return new Vector2(55, 20);
                } else {
                    return new Vector2(98, 51);
                }
            } else {
                return null;
            }
        }

        public static Dictionary<string, SpecialObject> GetAnimals(ModEntry mod, GameLocation location) {

            Dictionary<string, SpecialObject> animals = new();

            List<FarmAnimal>? farmAnimals = null;

            if (location is Farm)
                farmAnimals = (location as Farm).getAllFarmAnimals();
            else if (location is AnimalHouse)
                farmAnimals = (location as AnimalHouse).animals.Values.ToList();

            if (farmAnimals != null) {
                foreach (FarmAnimal animal in farmAnimals) {
                    string mood = animal.getMoodMessage();
                    string mood_text = "";
                    if (mood.ToLower().Contains("thin")) {
                        mood_text = " the Hungry ";
                    }

                    SpecialObject sObject = new SpecialObject($"{animal.displayName} {mood_text}{animal.shortDisplayType()}", animal.getTileLocation());
                    animals.Add(sObject.name, sObject);
                }
            }

            return animals;
        }

        public static string GetDirection(Vector2 start, Vector2 end) {

            double dx = end.X - start.X;
            double dy = start.Y - end.Y;

            if (Math.Abs(dx) > Math.Abs(dy)) {
                if (Math.Abs(dy / dx) <= tan_Pi_div_8) {
                    return directions[dx > 0 ? Direction.East : Direction.West];
                } else if (dx > 0) {
                    return directions[dy > 0 ? Direction.Northeast : Direction.Southeast];
                } else {
                    return directions[dy > 0 ? Direction.Northwest : Direction.Southwest];
                }
            } else if (Math.Abs(dy) > 0) {
                if (Math.Abs(dx / dy) <= tan_Pi_div_8) {
                    return directions[dy > 0 ? Direction.North : Direction.South];
                } else if (dy > 0) {
                    return directions[dx > 0 ? Direction.Northeast : Direction.Northwest];
                } else {
                    return directions[dx > 0 ? Direction.Southeast : Direction.Southwest];
                }
            } else {
                return $"Current Tile";
            }

        }

        public static List<string> get_directions(string[] map) {
            List<string> directions = new();

            int direction_count = 0;
            string? last_direction = map[0];
            for (int i = 0; i < map.Length; i++) {

                string direction = map[i];

                if (last_direction == direction) direction_count++;

                if (last_direction != direction || i == map.Length - 1) {
                    string units = direction_count > 1 ? "Tiles" : "Tile";
                    directions.Add($"{direction_count} {units} {last_direction}");
                    last_direction = direction;
                    direction_count = 1;
                }
            }

            return directions;
        }

        public static Dictionary<string, SpecialObject> GetEntrances(ModEntry mod) {

            GameLocation location = Game1.currentLocation;

            Dictionary<string, SpecialObject> doors = new();
            Dictionary<string, int> added_names = new();

            foreach (Point point in location.doors.Keys) {
                string str = location.doors[point];

                if (added_names.ContainsKey(str)) {
                    added_names[str]++;
                    str += $" {added_names[str]}";
                } else {
                    added_names.Add(str, 1);
                }

                doors[str] = new SpecialObject(str, new(point.X, point.Y), new(point.X, point.Y + 1));
            }

            foreach (Warp point in location.warps) {
                string str = point.TargetName;
                if (str.ToLower() == "desert") continue;

                if (added_names.ContainsKey(str)) {

                    //make sure this warp is not directly next to an existing one

                    bool add = true;

                    int number = added_names[str];

                    string name = str;
                    if (number > 1) {
                        name += $" {number}";
                    }
                    SpecialObject previous_warp = doors[name];

                    if (mod.IsColliding(new(point.X, point.Y))) {
                        add = false;
                    }

                    for (int i = -5; i < 5; i++) {
                        if (add) {
                            if (previous_warp.TileLocation.X == point.X && previous_warp.TileLocation.Y == point.Y + i) {
                                add = false;
                            }
                            if (previous_warp.TileLocation.Y == point.Y && previous_warp.TileLocation.X == point.X + i) {
                                add = false;
                            }
                        }
                    }

                    if (add) {
                        added_names[str]++;
                        str += $" {added_names[str]}";
                        doors[str] = new SpecialObject(str, new(point.X, point.Y));
                    }

                } else {
                    added_names.Add(str, 1);
                    doors[str] = new SpecialObject(str, new(point.X, point.Y));
                }

                if (str.ToLower().Contains("sunroom")) {

                    Vector2 pathfind_override = doors[str].TileLocation;
                    pathfind_override.Y += 1;

                    doors[str].PathfindingOverride = pathfind_override;
                }


            }

            return doors;
        }
    }
}