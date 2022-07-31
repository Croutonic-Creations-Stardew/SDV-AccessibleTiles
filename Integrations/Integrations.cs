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
                text: () => "Grid Movement Settings"
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
                text: () => "Object Tracker Settings"
            );

            ConfigMenu.AddKeybindList(
                Mod.ModManifest,
                name: () => "Cycle Up Category",
                getValue: () => config.OTCycleUpCategory,
                setValue: value => config.OTCycleUpCategory = value
            );

            ConfigMenu.AddKeybindList(
                Mod.ModManifest,
                name: () => "Cycle Down Category",
                getValue: () => config.OTCycleDownCategory,
                setValue: value => config.OTCycleDownCategory = value
            );

            ConfigMenu.AddKeybindList(
                Mod.ModManifest,
                name: () => "Cycle Up Object",
                getValue: () => config.OTCycleUpObject,
                setValue: value => config.OTCycleUpObject = value
            );

            ConfigMenu.AddKeybindList(
                Mod.ModManifest,
                name: () => "Cycle Down Object",
                getValue: () => config.OTCycleDownObject,
                setValue: value => config.OTCycleDownObject = value
            );

            ConfigMenu.AddKeybindList(
                Mod.ModManifest,
                name: () => "Read Selected Object",
                getValue: () => config.OTReadSelectedObject,
                setValue: value => config.OTReadSelectedObject = value
            );

            ConfigMenu.AddKeybindList(
                Mod.ModManifest,
                name: () => "Read Selected Object Tile",
                getValue: () => config.OTReadSelectedObjectTileLocation,
                setValue: value => config.OTReadSelectedObjectTileLocation = value
            );

            ConfigMenu.AddKeybindList(
                Mod.ModManifest,
                name: () => "Move To Selected Object",
                getValue: () => config.OTMoveToSelectedObject,
                setValue: value => config.OTMoveToSelectedObject = value
            );

            ConfigMenu.AddKeybindList(
                Mod.ModManifest,
                name: () => "Cancel Auto Walking",
                getValue: () => config.OTCancelAutoWalking,
                setValue: value => config.OTCancelAutoWalking = value
            );

            ConfigMenu.AddKeybindList(
                Mod.ModManifest,
                name: () => "Switch Sorting Mode",
                getValue: () => config.OTSwitchSortingMode,
                setValue: value => config.OTSwitchSortingMode = value
            );

            //screen reader
            if (StardewAccess != null) {
                ConfigMenu.AddSectionTitle(
                    Mod.ModManifest,
                    text: () => "Screen Reader Settings"
                );

                ConfigMenu.AddTextOption(
                    Mod.ModManifest,
                    name: () => "Read Object Text Template",
                    getValue: () => config.OTReadSelectedObjectText,
                    setValue: value => config.OTReadSelectedObjectText = value
                );

                ConfigMenu.AddTextOption(
                    Mod.ModManifest,
                    name: () => "Read Object Text Template",
                    getValue: () => config.OTReadSelectedObjectText,
                    setValue: value => config.OTReadSelectedObjectText = value
                );
            }

        }

    }
}
