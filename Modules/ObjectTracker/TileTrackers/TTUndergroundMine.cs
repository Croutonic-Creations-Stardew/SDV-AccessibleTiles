using AccessibleTiles.Integrations;
using AccessibleTiles.Modules.ObjectTracker.Categories;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using xTile;

namespace AccessibleTiles.Modules.ObjectTracker.TileTrackers {
    internal class TTUndergroundMine : TileTrackerBase {

        public ModEntry mod;

        public TTUndergroundMine(object? arg = null) : base(arg) {
            
        }

        public override void FindObjects(object? arg = null) {

            if (Game1.currentLocation is not Mine or MineShaft && !Game1.currentLocation.Name.ToLower().Contains("undergroundmine")) {
                return;
            }

            Dictionary<string, object> args = (Dictionary<string, object>)arg;

            StardewAccessInterface access = (StardewAccessInterface)args["access"];
            ModEntry mod = (ModEntry)args["mod"];

            GameLocation location = Game1.player.currentLocation;

            Dictionary<string, SpecialObject> detected_objects = new();

            foreach (StardewValley.Object feature in Game1.currentLocation.objects.Values) {

                string name = access.GetNameAtTile(feature.TileLocation);

                if (name == null) {
                    continue;
                }

                string category = "mine items";
                if(name.ToLower().Contains("ladder") || name.ToLower().Contains("elevator")) {
                    category = "mine navigation";
                }

                AddFocusableObject(category, name, feature.TileLocation);
            }

            Map map = Game1.currentLocation.Map;
            for (int yTile = 0; yTile < map.GetLayer("Buildings").LayerHeight; ++yTile) {
                for (int xTile = 0; xTile < map.GetLayer("Buildings").LayerWidth; ++xTile) {
                    if (map.GetLayer("Buildings").Tiles[xTile, yTile] != null) {

                        string? name;

                        name = access.GetNameAtTile(new(xTile, yTile));

                        if (name == null) {
                            continue;
                        }

                        string category = "mine items";
                        if (name.ToLower().Contains("ladder") || name.ToLower().Contains("elevator")) {
                            category = "mine navigation";
                        }

                        AddFocusableObject(category, name, new(xTile, yTile));
                    }
                }
            }

            base.FindObjects(arg);

        }

    }

}
