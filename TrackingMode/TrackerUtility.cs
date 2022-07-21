using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

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
    }
}