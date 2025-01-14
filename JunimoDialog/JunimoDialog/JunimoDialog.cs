﻿using System;
using System.IO;
using ContentPatcher;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;

namespace JunimoDialog
{
    public class JunimoDialog : Mod
    {
        internal static ModConfig Config;
        internal static IMonitor SMonitor;
        internal static Random jdRandom;

        /// <summary>The mod entry point, called after the mod is first loaded.</summary>
        /// <param name="helper">Provides simplified APIs for writing mods.</param>
        public override void Entry(IModHelper helper)
        {
            // Only run if the master game
            if (!Game1.IsMasterGame)
            {
                return;
            }

            SMonitor = Monitor;
            jdRandom = new Random();
            
            Helper.Events.GameLoop.GameLaunched += OnLaunched;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.Saving += OnSaving;

            Harmony harmony = new Harmony("ceruleandeep.JunimoDialog");
            harmony.PatchAll();
        }

        /// <summary>Raised after a the game is saved</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        void OnSaving(object sender, SavingEventArgs e)
        {
            Helper.WriteConfig(Config);
        }

        /// <summary>Raised after the player loads a save slot and the world is initialised.</summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event arguments.</param>
        void OnSaveLoaded(object sender, EventArgs e)
        {
            // reload the config to pick up any changes made in GMCM on the title screen
            Config = Helper.ReadConfig<ModConfig>();
        }

        private void OnLaunched(object sender, GameLaunchedEventArgs e)
        {
            Config = Helper.ReadConfig<ModConfig>();
            Helper.WriteConfig(Config);
            
            Dialog.Initialize(ModManifest, Helper);
            Dialog.cp_api = Helper.ModRegistry.GetApi<IContentPatcherAPI>("Pathoschild.ContentPatcher");
            Dialog.Load(Path.Combine(Helper.DirectoryPath, "Assets"));

            foreach (var contentPack in Helper.ContentPacks.GetOwned())
            {
                Dialog.Load(contentPack.DirectoryPath);
            }
            
            var api = Helper.ModRegistry.GetApi<GenericModConfigMenuAPI>("spacechase0.GenericModConfigMenu");
            if (api is null) return;
            api.RegisterModConfig(ModManifest, () => Config = new ModConfig(), () => Helper.WriteConfig(Config));
            api.SetDefaultIngameOptinValue(ModManifest, true);

            api.RegisterSimpleOption(ModManifest, "Happy", "", () => Config.Happy, val => Config.Happy = val);
            api.RegisterSimpleOption(ModManifest, "Grumpy", "", () => Config.Grumpy, val => Config.Grumpy = val);
            api.RegisterClampedOption(ModManifest, "Dialog Chance", "Dialog Chance", () => Config.DialogChance, val => Config.DialogChance = val, 0.0f, 1.0f, 0.05f);
            api.RegisterClampedOption(ModManifest, "Junimo Language Chance", "Chance for dialog to appear in Junimo language", () => Config.JunimoTextChance, val => Config.JunimoTextChance = val, 0.0f, 1.0f, 0.50f);
            api.RegisterSimpleOption(ModManifest, "Extra debug output", "", () => Config.ExtraDebugOutput, val => Config.ExtraDebugOutput = val);
        }
    }
}