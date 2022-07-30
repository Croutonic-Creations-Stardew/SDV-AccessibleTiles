﻿using AccessibleTiles.Modules.ObjectTracker.Categories;
using AccessibleTiles.Modules.ObjectTracker.TileTrackers;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessibleTiles.Modules.ObjectTracker {
     class TrackedObjects {

        private readonly ModEntry Mod;

        /// <summary>
        /// {column:[{Name, SpecialObject}]}
        /// </summary>
        private SortedList<string, Dictionary<string, SpecialObject>> Objects = new();
        
        public TrackedObjects(ModEntry mod) {
            this.Mod = mod;
        }

        public SortedList<string, Dictionary<string, SpecialObject>> GetObjects() {
            return Objects;
        }

        public void FindObjectsInArea() {
            TTStardewAccess StardewAccessObjects = new TTStardewAccess(this.Mod.Integrations.StardewAccess);
            if (StardewAccessObjects.HasObjects()) {
                this.AddObjects(StardewAccessObjects.GetObjects());
            }
        }

        private void AddObjects(SortedList<string, Dictionary<string, SpecialObject>> objectsToAdd) {
            foreach(var kvp in objectsToAdd) {

                string category = kvp.Key;

                if (!Objects.ContainsKey(category)) {
                    Objects.Add(category, new());
                }

                foreach(var obj in kvp.Value) {
                    if (!Objects.GetValueOrDefault(category).ContainsKey(obj.Key))
                        Objects.GetValueOrDefault(category).Add(obj.Key, obj.Value);
                }

                
            }
        }

    }
}