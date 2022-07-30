using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessibleTiles.Integrations {
    public class ModIntegrations {

        public StardewAccessInterface? StardewAccess { get; set; }

        public IGenericModConfigMenuApi? ConfigMenu { get; set; }

        public ModIntegrations(IModHelper helper) {

            if (helper.ModRegistry.IsLoaded("shoaib.stardewaccess")) {
                this.StardewAccess = helper.ModRegistry.GetApi<StardewAccessInterface>("shoaib.stardewaccess");
            }

            if (helper.ModRegistry.IsLoaded("spacechase0.GenericModConfigMenu")) {
                this.ConfigMenu = helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
                this.BuildConfigMenu();
            }

        }

        public void SRSay(string text, Boolean interrupt = true) {
            if(StardewAccess != null) {
                StardewAccess.Say(text, interrupt);
            }
        }

        private void BuildConfigMenu() {

        }

    }
}
