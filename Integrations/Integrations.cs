using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccessibleTiles.Integrations {
    public class ModIntegrations {

        private readonly ModEntry Mod;
        public StardewAccessInterface? StardewAccess { get; set; }

        public IGenericModConfigMenuApi? ConfigMenu { get; set; }

        public ModIntegrations(ModEntry mod) {

            this.Mod = mod;

            if (mod.Helper.ModRegistry.IsLoaded("shoaib.stardewaccess")) {
                this.StardewAccess = mod.Helper.ModRegistry.GetApi<StardewAccessInterface>("shoaib.stardewaccess");
            }

            if (mod.Helper.ModRegistry.IsLoaded("spacechase0.GenericModConfigMenu")) {
                this.ConfigMenu = mod.Helper.ModRegistry.GetApi<IGenericModConfigMenuApi>("spacechase0.GenericModConfigMenu");
                this.BuildConfigMenu();
            }

        }

        public void SRSay(string text, Boolean interrupt = true) {
            if(StardewAccess != null) {
                StardewAccess.Say(text, interrupt);
            }
        }

        private void BuildConfigMenu() {

            ModConfig config = Mod.Config;

            ConfigMenu.Register(
                Mod.ModManifest,
                reset: () => Mod.Config = new ModConfig(),
                save: () => Mod.Helper.WriteConfig(config)
            );

            //grid movement
            ConfigMenu.AddSectionTitle(
                Mod.ModManifest,
                text: () => "Grid Movement"
            );

            ConfigMenu.AddBoolOption(
                Mod.ModManifest,
                name: () => "Grid Movement Active",
                getValue: () => config.GridMovementActive,
                setValue: value => config.GridMovementActive = value
            );

            ConfigMenu.AddKeybindList(
                Mod.ModManifest,
                name: () => "Toggle Grid Movement",
                getValue: () => config.ToggleGridMovementKey,
                setValue: value => config.ToggleGridMovementKey = value
            );

            ConfigMenu.AddKeybindList(
                Mod.ModManifest,
                name: () => "Grid Movement Override",
                getValue: () => config.GridMovementOverrideKey,
                setValue: value => config.GridMovementOverrideKey = value
            );

            //tracker
            ConfigMenu.AddSectionTitle(
                Mod.ModManifest,
                text: () => "Object Tracker"
            );

            ConfigMenu.AddKeybindList(
                Mod.ModManifest,
                name: () => "Cycle Up Category",
                getValue: () => config.ObjectTrackerCycleUpCategory,
                setValue: value => config.ObjectTrackerCycleUpCategory = value
            );

            ConfigMenu.AddKeybindList(
                Mod.ModManifest,
                name: () => "Cycle Down Category",
                getValue: () => config.ObjectTrackerCycleDownCategory,
                setValue: value => config.ObjectTrackerCycleDownCategory = value
            );

            ConfigMenu.AddKeybindList(
                Mod.ModManifest,
                name: () => "Cycle Up Object",
                getValue: () => config.ObjectTrackerCycleUpObject,
                setValue: value => config.ObjectTrackerCycleUpObject = value
            );

            ConfigMenu.AddKeybindList(
                Mod.ModManifest,
                name: () => "Cycle Down Object",
                getValue: () => config.ObjectTrackerCycleDownObject,
                setValue: value => config.ObjectTrackerCycleDownObject = value
            );

            ConfigMenu.AddKeybindList(
                Mod.ModManifest,
                name: () => "Read Selected Object",
                getValue: () => config.ObjectTrackerReadSelectedObject,
                setValue: value => config.ObjectTrackerReadSelectedObject = value
            );

            ConfigMenu.AddKeybindList(
                Mod.ModManifest,
                name: () => "Read Selected Object Tile",
                getValue: () => config.ObjectTrackerReadSelectedObjectTileLocation,
                setValue: value => config.ObjectTrackerReadSelectedObjectTileLocation = value
            );

            ConfigMenu.AddKeybindList(
                Mod.ModManifest,
                name: () => "Move To Selected Object",
                getValue: () => config.ObjectTrackerMoveToSelectedObject,
                setValue: value => config.ObjectTrackerMoveToSelectedObject = value
            );

            ConfigMenu.AddKeybindList(
                Mod.ModManifest,
                name: () => "Cancel Auto Walking",
                getValue: () => config.ObjectTrackerCancelAutoWalking,
                setValue: value => config.ObjectTrackerCancelAutoWalking = value
            );

            ConfigMenu.AddKeybindList(
                Mod.ModManifest,
                name: () => "Switch Sorting Mode",
                getValue: () => config.ObjectTrackerSwitchSortingMode,
                setValue: value => config.ObjectTrackerSwitchSortingMode = value
            );

        }

    }
}
