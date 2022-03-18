﻿using System;
using System.Linq;
using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Characters;
using StardewValley.Objects;
using StardewValley.TerrainFeatures;
using StardewModdingAPI;
using System.Collections.Generic;
using BetterJunimos.Abilities;
using StardewValley.Buildings;
using SObject = StardewValley.Object;

namespace BetterJunimosForestry.Abilities {
    public class PlantTreesAbility : IJunimoAbility {
        private readonly IMonitor Monitor;

        internal PlantTreesAbility(IMonitor Monitor) {
            this.Monitor = Monitor;
        }

        public string AbilityName() {
            return "PlantTrees";
        }

        public bool IsActionAvailable(GameLocation farm, Vector2 pos, Guid guid) {
            string mode = Util.GetModeForHut(Util.GetHutFromId(guid));
            if (mode != Modes.Forest) return false;

            JunimoHut hut = Util.GetHutFromId(guid);
            
            Vector2 up = new Vector2(pos.X, pos.Y + 1);
            Vector2 right = new Vector2(pos.X + 1, pos.Y);
            Vector2 down = new Vector2(pos.X, pos.Y - 1);
            Vector2 left = new Vector2(pos.X - 1, pos.Y);

            Vector2[] positions = { up, right, down, left };
            foreach (Vector2 nextPos in positions) {
                if (!Util.IsWithinRadius(hut, pos)) continue;
                if (ShouldPlantWildTreeHere(farm, hut, nextPos)) return true;
            }
            return false;

            //return farm.terrainFeatures.ContainsKey(pos) && farm.terrainFeatures[pos] is HoeDirt hd && hd.crop == null &&
            //    !farm.objects.ContainsKey(pos);
        }

        // is this tile plantable? 
        internal bool ShouldPlantWildTreeHere(GameLocation farm, JunimoHut hut, Vector2 pos) {
            if (Util.BlocksDoor(hut, pos)) return false;

            // Monitor.Log($"    ShouldPlantWildTreeHere: {pos.X} {pos.Y} pattern {ModEntry.Config.WildTreePattern} in pattern {IsTileInPattern(pos)} plantable {Plantable(farm, pos)}", LogLevel.Debug);
            // is this tile in the planting pattern?
            if (!IsTileInPattern(pos)) {
                // Monitor.Log($"        ShouldPlantWildTreeHere: no, {pos.X} {pos.Y} not in pattern", LogLevel.Debug);
                return false;
            }

            if (!Plantable(farm, pos)) {
                // Monitor.Log($"        ShouldPlantWildTreeHere: no, {pos.X} {pos.Y} not plantable", LogLevel.Debug);
                return false;
            }
            
            // would a tree here restrict passage?
            // if (ModEntry.Config.WildTreePattern == "tight" || ModEntry.Config.WildTreePattern == "impassable") return true;
            // for (int x = -1; x < 2; x++) {
            //     for (int y = -1; y < 2; y++) {
            //         Vector2 v = new Vector2(pos.X + x, pos.Y + y);
            //         if (!Plantable(farm, v)) {
            //             Monitor.Log($"ShouldPlantWildTreeHere: {pos.X} {pos.Y} is not plantable so not planting here", LogLevel.Debug);
            //             return false;
            //         }
            //     }
            // }

            // Monitor.Log($"        ShouldPlantWildTreeHere: yes, {pos.X} {pos.Y} plantable", LogLevel.Debug);
            return true;
        }

        internal static bool IsTileInPattern(Vector2 pos) {
            if (ModEntry.Config.WildTreePattern == "tight") {
                return pos.X % 2 == 0;
            }

            if (ModEntry.Config.WildTreePattern == "loose") {
                return pos.X % 2 == 0 && pos.Y % 2 == 0;
            }

            if (ModEntry.Config.WildTreePattern == "fruity-tight") {
                return pos.X % 3 == 0 && pos.Y % 3 == 0;
            }

            if (ModEntry.Config.WildTreePattern == "fruity-loose") {
                if (pos.X % 4 == 2) return pos.Y % 2 == 0;
                if (pos.X % 4 == 0) return pos.Y % 2 == 0;
                return false;
            }

            throw new ArgumentOutOfRangeException($"Pattern '{ModEntry.Config.WildTreePattern}' not recognized");
        }

        // is this tile plantable?
        private bool Plantable(GameLocation farm, Vector2 pos) {
            if (farm.isTileOccupied(pos)) return false;  // is something standing on it? an impassable building? a terrain feature?
            if (Util.IsHoed(farm, pos)) return false;
            if (Util.IsOccupied(farm, pos)) return false;
            if (Util.SpawningTreesForbidden(farm, pos)) return false;
            if (!Util.CanBeHoed(farm, pos)) return false;
            return true;
        }
        
        public bool PerformAction(GameLocation farm, Vector2 pos, JunimoHarvester junimo, Guid guid) {
            JunimoHut hut = Util.GetHutFromId(guid);
            Chest chest = hut.output.Value;
            Item foundItem;
            foundItem = chest.items.FirstOrDefault(item => item != null && Util.WildTreeSeeds.Keys.Contains(item.ParentSheetIndex));
            if (foundItem == null) return false;

            Vector2 up = new Vector2(pos.X, pos.Y + 1);
            Vector2 right = new Vector2(pos.X + 1, pos.Y);
            Vector2 down = new Vector2(pos.X, pos.Y - 1);
            Vector2 left = new Vector2(pos.X - 1, pos.Y);

            Vector2[] positions = { up, right, down, left };
            foreach (Vector2 nextPos in positions) {
                if (!Util.IsWithinRadius(Util.GetHutFromId(guid), nextPos)) continue;
                if (ShouldPlantWildTreeHere(farm, hut, nextPos)) {
                    bool success = Plant(farm, nextPos, foundItem.ParentSheetIndex);
                    if (success) {
                        //Monitor.Log($"PerformAction planted {foundItem.Name} at {nextPos.X} {nextPos.Y}", LogLevel.Info);
                        Util.RemoveItemFromChest(chest, foundItem);
                        return true;
                    }
                }
            }
            return false;
        }

        private bool Plant(GameLocation farm, Vector2 pos, int index) {
            if (farm.terrainFeatures.Keys.Contains(pos)) {
                return false;
            }

            Tree tree = new Tree(Util.WildTreeSeeds[index], ModEntry.Config.PlantWildTreesSize);
            farm.terrainFeatures.Add(pos, tree);

            if (Utility.isOnScreen(Utility.Vector2ToPoint(pos), 64, farm)) {
                farm.playSound("stoneStep");
                farm.playSound("dirtyHit");
            }

            ++Game1.stats.SeedsSown;
            return true;
        }


        public List<int> RequiredItems() {
            return Util.WildTreeSeeds.Keys.ToList();
        }
        
        
        /* older API compat */
        public bool IsActionAvailable(Farm farm, Vector2 pos, Guid guid) {
            return IsActionAvailable((GameLocation) farm, pos, guid);
        }
        
        public bool PerformAction(Farm farm, Vector2 pos, JunimoHarvester junimo, Guid guid) {
            return PerformAction((GameLocation) farm, pos, junimo, guid);
        }
    }
}