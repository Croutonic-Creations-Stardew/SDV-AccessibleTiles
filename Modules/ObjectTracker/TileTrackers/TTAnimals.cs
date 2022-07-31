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
    internal class TTAnimals : TileTrackerBase {

        public TTAnimals(object? arg = null) : base(arg) {
            
        }

        public override void FindObjects(object? arg = null) {

            string category = "animals";
            GameLocation location = Game1.player.currentLocation;

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

                    AddFocusableObject(category, $"{animal.displayName} {mood_text}{animal.shortDisplayType()}", animal.getTileLocation());
                }
            }

            base.FindObjects(arg);
        }

    }

}
