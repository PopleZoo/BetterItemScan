using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;

namespace BetterItemScan
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class BetterItemScanModBase : BaseUnityPlugin
    {
        private const string modGUID = "PopleZoo.BetterItemScan";
        private const string modName = "Better Item Scan";
        private const string modVersion = "2.0.0.0";
        private readonly Harmony harmony = new Harmony(modGUID);
        public static BetterItemScanModBase Instance;
        public ManualLogSource mls;

        //config variables
        public static ConfigEntry<float> ItemScanRadius;
        public static ConfigEntry<float> AdjustScreenPositionXaxis;
        public static ConfigEntry<float> AdjustScreenPositionYaxis;
        public static ConfigEntry<bool> ShowDebugMode;
        public static ConfigEntry<bool> ShowOnShipOnly;
        public static ConfigEntry<bool> CalculateForQuota;
        private void Awake()
        {
            if (BetterItemScanModBase.Instance == null) { BetterItemScanModBase.Instance = this; };
            mls = BepInEx.Logging.Logger.CreateLogSource(modGUID);
            mls.LogInfo("Plugin BetterItemScan is loaded!");
            this.LoadConfigs();
            harmony.PatchAll();
        }
        private void LoadConfigs()
        {
            BetterItemScanModBase.ShowDebugMode = Config.Bind("Settings", "ShowDebugMode", false, "Shows the change in width of the area to scan");
            BetterItemScanModBase.ShowOnShipOnly = Config.Bind("Settings", "ShowOnShipOnly", false, "Whether or not to show the ship's total just in the ship");
            BetterItemScanModBase.CalculateForQuota = Config.Bind("Settings", "CalculateForQuota", false, "Whether or not to calculate scanned items to see which meet the quota");
            BetterItemScanModBase.ItemScanRadius = Config.Bind("Settings", "ItemScanRadius", 20f, "The default value is 20");
            BetterItemScanModBase.AdjustScreenPositionXaxis = Config.Bind("Settings", "AdjustScreenPositionXaxis", 0f, "The default value is 0, you will add or take away from its original position");
            BetterItemScanModBase.AdjustScreenPositionYaxis = Config.Bind("Settings", "AdjustScreenPositionYaxis", 0f, "The default value is 0, you will add or take away from its original position");
        }

    }
}
