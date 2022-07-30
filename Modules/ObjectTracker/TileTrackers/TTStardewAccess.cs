using AccessibleTiles.Integrations;
using AccessibleTiles.Modules.ObjectTracker.Categories;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessibleTiles.Modules.ObjectTracker.TileTrackers {
    internal class TTStardewAccess : TileTrackerBase {

        public TTStardewAccess(object? arg) : base(arg) {
            
        }

        public override void FindObjects(object? arg) {

            if (arg == null) return;

            Dictionary<Vector2, (string name, string category)> scannedTiles = (arg as StardewAccessInterface).SearchLocation();

            /* Categorise the scanned tiles into groups
             *
             * This method uses breadth first search so the first item is the closest item, no need to reorder or check for closest item
             */
            foreach (var tile in scannedTiles) {

                //ignore some of stardew access categories
                if (tile.Value.category == "animal") continue;

                AddFocusableObject(tile.Value.category, tile.Value.name, tile.Key);
            }

            base.FindObjects(arg);
        }

    }
}
