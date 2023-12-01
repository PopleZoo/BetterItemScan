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
        private const string modVersion = "1.0.0.1";
        private readonly Harmony harmony = new Harmony(modGUID);
        public static BetterItemScanModBase Instance;
        public ManualLogSource mls;

        //config variables
        public static ConfigEntry<float> Config_ItemScanRadius;
        public static ConfigEntry<bool> ShowDebugMode;
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
            BetterItemScanModBase.Config_ItemScanRadius = Config.Bind("Radius for scanning items", "Radius for scanning items", 20f, "The default value is 20");
        }

    }
}
