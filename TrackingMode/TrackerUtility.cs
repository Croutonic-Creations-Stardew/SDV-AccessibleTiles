using Microsoft.Xna.Framework;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile;
using Object = StardewValley.Object;

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

        public static Dictionary<(int, int), string> bundle_locations = new() {
            { (14, 5), "Pantry" },
            { (14, 23), "Crafts Room" },
            { (40, 10), "Fish Tank" },
            { (63, 14), "Boiler Room" },
            { (55, 6), "Vault" },
            { (46, 11), "Bulletin Board" },
        };

        public static SpecialObject GetClosest(SpecialObject item1, SpecialObject item2) {

            Vector2 player_tile = Game1.player.getTileLocation();

            double collide_distance = GetDistance(player_tile, item2.TileLocation);
            double new_distance = GetDistance(player_tile, item1.TileLocation);

            if (new_distance < collide_distance) {
                return item1;
            }
            return item2;
        }

        public static Vector2 GetClosest(Vector2 item1, Vector2 item2) {

            Vector2 player_tile = Game1.player.getTileLocation();

            double collide_distance = GetDistance(player_tile, item2);
            double new_distance = GetDistance(player_tile, item1);

            if (new_distance < collide_distance) {
                return item1;
            }
            return item2;
        }

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

        private static void AddObject(ref SortedList<string, SpecialObject> list, SpecialObject sObject) {
            if(list.ContainsKey(sObject.name)) {
                sObject = GetClosest(sObject, (SpecialObject)list[sObject.name]);
            }
            list[sObject.name] = sObject;
            //return list;
        }

        public static SortedList<string, SpecialObject> GetObjects(ModEntry mod) {

            GameLocation location = Game1.currentLocation;

            SortedList<string, SpecialObject> detected_objects = new();
            foreach (Vector2 position in location.objects.Keys) {

                StardewValley.Object obj = location.objects[position];
                string name = mod.stardewAccess.GetNameAtTile(position);
                if(name != null) {

                    if(obj.type == "Crafting" && obj.bigCraftable) {

                        string[] trackable_machines = { "bee house", "cask", "press", "keg", "machine", "maker", "preserves jar", "bone mill", "kiln", "crystalarium", "furnace", "geode crusher", "tapper", "lightning rod", "incubator", "wood chipper", "worm bin", "loom" };
                        
                        foreach(string machine in trackable_machines) {
                            if(name.ToLower().Contains(machine)) {

                                if(obj.MinutesUntilReady > 0) {
                                    name = $"Running {name}";
                                } else if(obj.readyForHarvest) {
                                    name = $"Harvestable {name}";
                                } else {
                                    name = $"Empty {name}";
                                }
                            }
                        }

                    }

                    SpecialObject sObject = new SpecialObject(name, position);
                    AddObject(ref detected_objects, sObject);
                }
                
            }

            return detected_objects;
        }

        public static SortedList<string, SpecialObject> GetResources(ModEntry mod) {
            SortedList<string, SpecialObject> detected_objects = new();
            foreach (TerrainFeature feature in Game1.currentLocation.terrainFeatures.Values) {
                if (feature is not HoeDirt) {
                    string name = mod.stardewAccess.GetNameAtTile(feature.currentTileLocation);
                    if (name != null) {
                        SpecialObject sObject = new SpecialObject(name, feature.currentTileLocation);
                        AddObject(ref detected_objects, sObject);
                    }
                }
            }
            foreach (LargeTerrainFeature feature in Game1.currentLocation.largeTerrainFeatures) {
                string name = mod.stardewAccess.GetNameAtTile(feature.tilePosition);
                AddObject(ref detected_objects, new(name, feature.tilePosition));
            }
            return detected_objects;
        }

        public static SortedList<string, SpecialObject> GetCrops(ModEntry mod) {
            SortedList<string, SpecialObject> detected_objects = new();
            foreach (TerrainFeature feature in Game1.currentLocation.terrainFeatures.Values) {
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
                            names.Add("Unfertilized Soil");
                        }
                    }
                    for (int i = 0; i < names.Count; i++) {
                        string name = names[i];
                        AddObject(ref detected_objects, new(name, feature.currentTileLocation));
                    }
                }
            }
            return detected_objects;
        }

        public static SortedList<string, SpecialObject> GetBuildings() {

            GameLocation location = Game1.currentLocation;
            SortedList<string, SpecialObject> detected_objects = new();

            if (location is Farm) {

                List<string> name_counts = new();

                foreach (Building building in ((Farm)location).buildings) {

                    string name = building.nameOfIndoorsWithoutUnique;
<<<<<<< Updated upstream
                    if (name == null || name == "null") name = "unknown";//continue;
                    name = name.TrimEnd(new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' });

                    int count = 1;
                    foreach (string c_name in name_counts) {
                        if (c_name == name) count++;
                    }

                    name_counts.Add(name);

                    name += $" {count}";

                    //human door
                    Point door = building.getPointForHumanDoor();
                    SpecialObject sObject = new SpecialObject(name + " Door", new(door.X, door.Y));

                    AddObject(ref detected_objects, sObject);

                    //animal door
                    if (building.animalDoor != new Point(-1, -1)) {

                        door = Vector2.Round(building.getRectForAnimalDoor().Location.ToVector2() / Game1.tileSize).ToPoint();
                        sObject = new SpecialObject(name + " Animal Door", new(door.X, door.Y));

                        AddObject(ref detected_objects, sObject);
                    }
                }
<<<<<<< Updated upstream

            }

            return detected_objects;

        }

        public static SortedList<string, SpecialObject> GetMining(ModEntry mod) {
            SortedList<string, SpecialObject> detected_objects = new();

            if (Game1.currentLocation is not Mine or MineShaft && !Game1.currentLocation.Name.ToLower().Contains("undergroundmine")) return detected_objects;

            foreach (Object feature in Game1.currentLocation.objects.Values) {
                string name = mod.stardewAccess.GetNameAtTile(feature.TileLocation);
                

                if(name == null) {
                    continue;
                }

                SpecialObject sObject = new(name, feature.TileLocation);
                if (detected_objects.ContainsKey(name)) {
                    sObject = GetClosest(sObject, (SpecialObject)detected_objects[name]);
                }
                detected_objects[name] = sObject;
            }

            Map map = Game1.currentLocation.Map;
            for (int yTile = 0; yTile < map.GetLayer("Buildings").LayerHeight; ++yTile) {
                for (int xTile = 0; xTile < map.GetLayer("Buildings").LayerWidth; ++xTile) {
                    if (map.GetLayer("Buildings").Tiles[xTile, yTile] != null) {

                        string? name;

                        name = mod.stardewAccess.GetNameAtTile(new(xTile, yTile));

                        if (name == null) {
                            continue;
                        }

                        SpecialObject sObject = new SpecialObject(name, new(xTile, yTile));
                        AddObject(ref detected_objects, sObject);

                    }
                }
            }

            return detected_objects;
        }

        public static SortedList<string, SpecialObject> GetResourceClumps(ModEntry mod) {

            GameLocation location = Game1.currentLocation;

            SortedList<string, SpecialObject> detected_objects = new();

            List<object> types = new();
            types.Add(location.resourceClumps);
            if (location is Woods) {
                types.Add(((Woods)location).stumps);
            }

            foreach (object type in types) {
                if (type is NetCollection<ResourceClump>) {
                    foreach (ResourceClump obj in (NetCollection<ResourceClump>)type) {
                        string name = mod.stardewAccess.GetNameAtTile(obj.tile);//GetClumpTileIndexName(obj.parentSheetIndex);

                        SpecialObject sObject = new SpecialObject(name, obj.tile);
                        AddObject(ref detected_objects, sObject);

                    }
                }
                if (type is NetObjectList<ResourceClump>) {
                    foreach (ResourceClump obj in (NetObjectList<ResourceClump>)type) {
                        string name = mod.stardewAccess.GetNameAtTile(obj.tile);//GetClumpTileIndexName(obj.parentSheetIndex);

                        SpecialObject sObject = new SpecialObject(name, obj.tile);
                        AddObject(ref detected_objects, sObject);

                    }
                }
            }

            return detected_objects;
        }

        public static SortedList<string, SpecialObject> GetAnimals(ModEntry mod) {

            GameLocation location = Game1.currentLocation;

            SortedList<string, SpecialObject> animals = new();

            List<FarmAnimal>? farmAnimals = null;

            if (location is Farm)
                farmAnimals = (location as Farm).getAllFarmAnimals();
            else if (location is AnimalHouse)
                farmAnimals = (location as AnimalHouse).animals.Values.ToList();

            if (farmAnimals != null) {
                foreach (FarmAnimal animal in farmAnimals) {
                    mod.console.Debug(animal.getMoodMessage());

                    string mood = animal.getMoodMessage();
                    string mood_text = "";
                    if(mood.ToLower().Contains("thin")) {
                        mood_text = " the Hungry ";
                    }

                    SpecialObject sObject = new SpecialObject($"{animal.displayName} {mood_text}{animal.shortDisplayType()}", animal.getTileLocation());
                    AddObject(ref animals, sObject);
                }
            }

            return animals;
        }

        public static SortedList<string, SpecialObject> GetBundles() {

            CommunityCenter communityCenter = (Game1.currentLocation as CommunityCenter);
            SortedList<string, SpecialObject> bundles = new();

            if (communityCenter == null) return bundles;

            foreach (var (tile, name) in bundle_locations) {
                if (communityCenter.shouldNoteAppearInArea(CommunityCenter.getAreaNumberFromName(name))) {
                    bundles.Add(name, new SpecialObject(name, new(tile.Item1, tile.Item2)));
                }
            }

            return bundles;
        }

        public static SortedList<string, SpecialObject> GetCharacters() {

            GameLocation location = Game1.currentLocation;

            SortedList<string, SpecialObject> characters = new();
            foreach (NPC npc in location.getCharacters()) {
                SpecialObject sObject = new SpecialObject(npc.displayName, npc.getTileLocation());
                AddObject(ref characters, sObject);
            }

                foreach (NPC npc in location.currentEvent.actors) {
                    SpecialObject sObject = new SpecialObject(npc.displayName, npc.getTileLocation());
                    AddObject(ref characters, sObject);
                }
            }
            

            return characters;
        }

        public static SortedList<string, SpecialObject> GetPOIs() {

            GameLocation location = Game1.currentLocation;
            SortedList<string, SpecialObject> points = new();

            if (location.Name == "BeachNightMarket") {
                AddObject(ref points, new("Outdoor Decorations Shop", new(55, 31)));
                AddObject(ref points, new("Coffee Vendor", new(14, 37)));
                AddObject(ref points, new("Home Teleport", new(32, 34)));
                AddObject(ref points, new("Travelling Cart", new(39, 30)));
                AddObject(ref points, new("Famous Painter", new(43, 34)));
                AddObject(ref points, new("Random Shop", new(48, 34)));
            }
            if (location is Submarine) {
                AddObject(ref points, new("Captain", new(2, 9)));
            }
            if (location is BusStop) {
                AddObject(ref points, new("Bus Ticket Station", new(7, 11)));
                AddObject(ref points, new("Bus Stop Minecart", new(4, 3)));
            }
            if (location is Town) {
                AddObject(ref points, new("Daily Quest Board", new(42, 53)));
                AddObject(ref points, new("Special Orders Board", new(62, 93)));
                if (location.currentEvent != null) {
                    string event_name = location.currentEvent.FestivalName;
                    if (event_name == "Egg Festival") {
                        AddObject(ref points, new(event_name + " Shop", new(21, 55)));
                    }
                    if (event_name == "Flower Dance") {
                        AddObject(ref points, new(event_name + " Shop", new(28, 37)));
                    }
                    if (event_name == "Luau") {
                        AddObject(ref points, new(event_name + " Soup Pot", new(35, 13)));
                    }
                    if (event_name == "Spirit's Eve") {
                        AddObject(ref points, new(event_name + " Shop", new(25, 49)));
                    }
                    if (event_name == "Stardew Valley Fair") {
                        event_name = "Fair";
                        AddObject(ref points, new(event_name + " Shop", new(16, 52)));
                        AddObject(ref points, new("Slingshot Game", new(23, 62)));
                        AddObject(ref points, new("Purchase Star Tokens", new(34, 65)));
                        AddObject(ref points, new("Spin The Wheel", new(33, 70)));
                        AddObject(ref points, new("Fishing Challenge", new(23, 70)));
                        AddObject(ref points, new("Fortune Teller", new(47, 87)));
                        AddObject(ref points, new("Grange Display", new(38, 59)));
                        AddObject(ref points, new("Strength Game", new(30, 56)));
                        AddObject(ref points, new("Free Burgers", new(26, 33)));
                    }
                    if (event_name == "Festival of Ice") {
                        AddObject(ref points, new("Travelling Cart", new(55, 31)));
                    }
                    if (event_name == "Feast of the Winter Star") {
                        AddObject(ref points, new(event_name + " Shop", new(18, 61)));
                    }
                }
            }
            if (location.Name == "Saloon") {
                AddObject(ref points, new("Gus's Refridgerator", new(18, 16)));
            }
            if (location.Name == "ArchaeologyHouse") {
                AddObject(ref points, new("Museum Box", new(5, 9)));
            }
            if (location is Beach) {
                AddObject(ref points, new("Willy's Barrel", new(37, 31)));
            }

            return points;

        }

        public static SortedList<string, SpecialObject> GetEntrances(ModEntry mod) {

            GameLocation location = Game1.currentLocation;

            SortedList<string, SpecialObject> doors = new();
            Dictionary<string, int> added_names = new();

            foreach (Point point in location.doors.Keys) {
                string str = location.doors[point];

                if(added_names.ContainsKey(str)) {
                    added_names[str]++;
                    str += $" {added_names[str]}";
                } else {
                    added_names.Add(str, 1);
                }

                doors[str] = new SpecialObject(str, new(point.X, point.Y));
            }

            foreach (Warp point in location.warps) {
                string str = point.TargetName;
                if (str.ToLower() == "desert") continue;

                if (added_names.ContainsKey(str)) {

                    //make sure this warp is not directly next to an existing one

                    bool add = true;

                    int number = added_names[str];

                    string name = str;
                    if(number > 1) {
                        name += $" {number}";
                    }
                    SpecialObject previous_warp = doors[name];

                    if (mod.IsColliding(new(point.X, point.Y))) {
                        add = false;
                    }

                    for (int i = -5; i < 5; i++) {
                        if(add) {
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

                
            }

            return doors;
        }

    }
}
