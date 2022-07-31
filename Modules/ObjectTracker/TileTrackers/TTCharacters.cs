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
    internal class TTCharacters : TileTrackerBase {

        public TTCharacters(object? arg = null) : base(arg) {
            
        }

        public override void FindObjects(object? arg = null) {

            string category = "characters";
            GameLocation location = Game1.player.currentLocation;

            foreach (NPC npc in location.getCharacters()) {
                AddFocusableObject(category, npc.displayName, npc.getTileLocation(), character: npc);
            }

            if (location.isTemp()) {
                foreach (NPC npc in location.currentEvent.actors) {
                    AddFocusableObject(category, npc.displayName, npc.getTileLocation(), character: npc);
                }
            }

            base.FindObjects(arg);
        }

    }

}
