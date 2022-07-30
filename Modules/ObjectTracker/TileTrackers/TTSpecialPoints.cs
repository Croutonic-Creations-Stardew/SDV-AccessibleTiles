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
    internal class TTSpecialPoints : TileTrackerBase {

        public TTSpecialPoints(object? arg) : base(arg) {
            
        }

        public override void FindObjects(object? arg) {

            var specialPoints = (arg as ModEntry).Helper.ModContent.Load<Dictionary<string, List<object>>>("assets/SpecialPoints.json");

            Farmer player = Game1.player;

            string category = "special";

            //get objects from current location
            if(specialPoints.ContainsKey(player.currentLocation.Name)) {
                foreach(object obj in specialPoints[player.currentLocation.Name]) {

                    SpecialPoint sPoint = Newtonsoft.Json.JsonConvert.DeserializeObject<SpecialPoint>(Newtonsoft.Json.JsonConvert.SerializeObject(obj));

                    string object_category = sPoint.category_override != null ? sPoint.category_override : category;

                    if (sPoint.requiresQuest != null) {
                        if (!player.hasQuest(sPoint.requiresQuest.Value)) {
                            continue;
                        }
                    }

                    if (sPoint.extraChecksHook != null) {
                        switch (sPoint.extraChecksHook) {
                            case 1:
                                //Shadow Guy's Hiding Bush
                                if (!player.hasMagnifyingGlass) continue;
                                break;
                            case 2:
                                //Haley's Bracelet
                                if (!(player.currentLocation.currentEvent != null && player.currentLocation.currentEvent.playerControlTargetTile == new Point(53, 8))) continue;
                                break;
                        }
                    }

                    AddFocusableObject(object_category, sPoint.name, new(sPoint.xPos, sPoint.yPos));

                }
            };

            base.FindObjects(arg);
        }

    }

    public class SpecialPoint {
        public string name { get; set; }
        public string? category_override { get; set; } = null;
        public int xPos { get; set; }
        public int yPos { get; set; }
        public int? requiresQuest { get; set; } = null;
        public int? extraChecksHook { get; set; } = null;
    }
}
