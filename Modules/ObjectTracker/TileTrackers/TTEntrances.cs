using AccessibleTiles.Integrations;
using AccessibleTiles.Modules.ObjectTracker.Categories;
using Microsoft.Xna.Framework;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessibleTiles.Modules.ObjectTracker.TileTrackers {
    internal class TTEntrances : TileTrackerBase {

        public TTEntrances(object? arg) : base(arg) {
            
        }

        public override void FindObjects(object? arg) {

            string category = "entrances";
            GameLocation location = Game1.player.currentLocation;

            Dictionary<string, int> added_names = new();

            foreach (Point point in location.doors.Keys) {
                string str = location.doors[point];

                if (added_names.ContainsKey(str)) {
                    added_names[str]++;
                    str += $" {added_names[str]}";
                } else {
                    added_names.Add(str, 1);
                }

                AddFocusableObject(category, str, point.ToVector2());
            }

            foreach (Warp point in location.warps) {
                string name = point.TargetName;
                if (name.ToLower() == "desert") continue;

                if (added_names.ContainsKey(name)) {

                    bool add = true;
                    int number = added_names[name];

                    if (number > 1) {
                        name += $" {number}";
                    }
                    SpecialObject previous_warp = this.Objects[category][name];

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
                        added_names[name]++;
                        name += $" {added_names[name]}";
                        AddFocusableObject(category, name, new(point.X, point.Y));
                    }

                } else {
                    added_names.Add(name, 1);
                    AddFocusableObject(category, name, new(point.X, point.Y));
                }

                if (name.ToLower().Contains("sunroom")) {

                    Vector2 pathfind_override = this.Objects[category][name].TileLocation;
                    pathfind_override.Y += 1;

                    this.Objects[category][name].PathfindingOverride = pathfind_override;

                }


            }

            base.FindObjects(arg);
        }

    }

}
