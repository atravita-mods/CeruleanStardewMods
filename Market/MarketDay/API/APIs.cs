﻿using System;
using MarketDay.src.API;
using StardewModdingAPI;

namespace MarketDay.API
{
    /// <summary>
    /// This class is used to register external APIs and hold the instances of those APIs to be accessed
    /// by the rest of the mod
    /// </summary>
    class APIs
    {
        internal static IJsonAssetsApi JsonAssets;
        internal static IBFAVApi BFAV;
        internal static IConditionsApi Conditions;

        internal static readonly Lazy<IDynamicGameAssetsApi> dgaApi = new(
            () => MarketDay.helper.ModRegistry.GetApi<IDynamicGameAssetsApi>("spacechase0.DynamicGameAssets")
        );

        /// <summary>
        /// Register the API for Json Assets
        /// </summary>
        public static void RegisterJsonAssets()
        {
            JsonAssets = MarketDay.helper.ModRegistry.GetApi<IJsonAssetsApi>("spacechase0.JsonAssets");

            if (JsonAssets == null)
            {
                MarketDay.Log("Json Assets API not detected. This is only an issue if you're using cystom Json Assets items and shops trying to sell them, as custom items will not appear in shops.",
                    LogLevel.Info);
            }

        }

        /// <summary>
        /// Registers the API for Better Farm Animal Variety, and check if it has been disabled in the user's options.
        /// If so, set it to null
        /// </summary>
        public static void RegisterBFAV()
        {
            BFAV = MarketDay.helper.ModRegistry.GetApi<IBFAVApi>("Paritee.BetterFarmAnimalVariety");

            if (BFAV == null)
            {
                MarketDay.Log("BFAV API not detected. This is only an issue if you're using custom BFAV animals and a custom shop that's supposed to sell them, as custom animals will not appear in those shops.",
                    LogLevel.Info);
            }
            else if (!BFAV.IsEnabled())
            {
                BFAV = null;
                MarketDay.Log("BFAV is installed but not enabled. This is only an issue if you're using custom BFAV animals and a custom shop that's supposed to sell them, as custom animals will not appear in those shops",
                    LogLevel.Info);
            }
        }

        /// <summary>
        /// Register the API for Expanded Preconditions Utility
        /// </summary>
        public static void RegisterExpandedPreconditionsUtility()
        {
            Conditions = MarketDay.helper.ModRegistry.GetApi<IConditionsApi>("Cherry.ExpandedPreconditionsUtility");

            if (Conditions == null)
            {
                MarketDay.Log("Expanded Preconditions Utility API not detected. Something went wrong, please check that your installation of Expanded Preconditions Utility is valid",
                    LogLevel.Error);
                return;
            }

            Conditions.Initialize(MarketDay.VerboseLogging, "ceruleandeep.MarketDay");

        }

        public static void RegisterFAVR()
        {
            //TODO: when FAVR is released, start deprecating support for BFAV
        }

    }
}
