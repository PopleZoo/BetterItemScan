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
        private const string modVersion = "1.0.0.3";
        private readonly Harmony harmony = new Harmony(modGUID);
        public static BetterItemScanModBase Instance;
        public ManualLogSource mls;

        //config variables
        public static ConfigEntry<float> ItemScanRadius;
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
            BetterItemScanModBase.ShowDebugMode = Config.Bind("Bool for showing Radius of the scan in game", "Show DebugMode", false, "shows the change in width of the area to scan");
            BetterItemScanModBase.ShowOnShipOnly = Config.Bind("Bool for showing the Total Ship amount only on the Ship", "Show On Ship Only", false, "whether or not to show the ship's total just in the ship");
            BetterItemScanModBase.CalculateForQuota = Config.Bind("Bool for showing the which scanned items will meet the quota, if your scanned items meet the quota an '*' will mark it", "Calculate For Quota", false, "whether or not to calculate scanned items to see which meet the quota");
            BetterItemScanModBase.ItemScanRadius = Config.Bind("Radius for scanning items", "Radius for scanning items", 20f, "The default value is 20");
        }

    }
}
