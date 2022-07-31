using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Vector2 = Microsoft.Xna.Framework.Vector2;
using System.Text;
using System.Threading.Tasks;
using StardewValley;

namespace AccessibleTiles.Modules.ObjectTracker.Categories {
    internal class TileTrackerBase {

        private SortedList<string, Dictionary<string, SpecialObject>> Objects = new();

        public TileTrackerBase(object? arg) {
            this.FindObjects(arg);
        }

        public virtual void FindObjects(object? arg) {
            
        }

        public Boolean HasObjects() {
            return Objects.Any();
        }

        public SortedList<string, Dictionary<string, SpecialObject>> GetObjects() {
            return Objects;
        }

        public void AddFocusableObject(string category, string name, Vector2 tile, NPC character = null) {

            if (!Objects.ContainsKey(category)) {
                Objects.Add(category, new());
            }

            SpecialObject sObject = new SpecialObject(name, tile);

            if(character != null) {
                sObject.character = character;
            }

            if (!Objects.GetValueOrDefault(category).ContainsKey(name))
                Objects.GetValueOrDefault(category).Add(name, sObject);

        }

    }
}
