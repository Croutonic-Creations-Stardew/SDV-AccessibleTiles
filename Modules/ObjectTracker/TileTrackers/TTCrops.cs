using AccessibleTiles.Integrations;
using AccessibleTiles.Modules.ObjectTracker.Categories;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessibleTiles.Modules.ObjectTracker.TileTrackers {
    internal class TTCrops : TileTrackerBase {

        public TTCrops(object? arg = null) : base(arg) {
            
        }

        public override void FindObjects(object? arg = null) {

            string category = "crops";
            GameLocation location = Game1.player.currentLocation;

            foreach (TerrainFeature feature in location.terrainFeatures.Values) {
                if (feature is HoeDirt) {
                    HoeDirt dirt = (HoeDirt)feature;
                    List<string> names = new();

                    if (dirt.crop != null) {
                        string cropName = Game1.objectInformation[dirt.crop.indexOfHarvest].Split('/')[0];
                        if (dirt.readyForHarvest()) {
                            names.Add("Harvestable " + cropName);
                        }
                        if (dirt.state.Value != HoeDirt.watered) {
                            names.Add("Unwatered " + cropName);
                        }
                        if (dirt.crop.dead) {
                            names.Add("Dead " + cropName);
                        }
                    } else {
                        if (dirt.fertilizer.Value == HoeDirt.noFertilizer) {
                            names.Add("Unfertilized Unplanted Soil");
                        } else {
                            names.Add("Fertilized Unplanted Soil");
                        }
                    }
                    for (int i = 0; i < names.Count; i++) {
                        string name = names[i];
                        AddFocusableObject(category, name, feature.currentTileLocation);
                    }
                }
            }

            base.FindObjects(arg);
        }

    }

}
