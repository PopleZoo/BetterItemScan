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
        private const string modVersion = "2.1.0.0";
        private readonly Harmony harmony = new Harmony(modGUID);
        public static BetterItemScanModBase Instance;
        public ManualLogSource mls;

        //config variables
        public static ConfigEntry<float> ItemScanRadius;
        public static ConfigEntry<float> AdjustScreenPositionXaxis;
        public static ConfigEntry<float> AdjustScreenPositionYaxis;
        public static ConfigEntry<float> ItemScaningUICooldown;
        public static ConfigEntry<float> FontSize;
        public static ConfigEntry<bool> ShowDebugMode;
        public static ConfigEntry<bool> ShowShipTotalOnShipOnly;
        public static ConfigEntry<bool> ShowTotalOnShipOnly;
        public static ConfigEntry<bool> CalculateForQuota;
        public static ConfigEntry<string> ItemTextColorHex;
        public static ConfigEntry<string> ItemTextCalculatorColorHex;

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
            BetterItemScanModBase.ShowShipTotalOnShipOnly = Config.Bind("Settings", "ShowShipTotalOnShipOnly", false, "Whether or not to show the ship's total only in the ship");
            BetterItemScanModBase.ShowTotalOnShipOnly = Config.Bind("Settings", "ShowTotalOnShipOnly", false, "Whether or not to show the total scanned in the ship only");
            BetterItemScanModBase.CalculateForQuota = Config.Bind("Settings", "CalculateForQuota", true, "Whether or not to calculate scanned items to see which meet the quota and if any do");
            BetterItemScanModBase.ItemScanRadius = Config.Bind("Settings", "ItemScanRadius", 20f, "The default width is 20");
            BetterItemScanModBase.FontSize = Config.Bind("Settings", "FontSize", 20f, "The default font size is 20, to make/see this change you may have to do this manually in the config file itself in the bepinex folder");
            BetterItemScanModBase.ItemTextColorHex = Config.Bind("Settings", "ItemTextColorHex", "#78FFAE", "The default text color for items that have been scanned, value must be a hexadecimal");
            BetterItemScanModBase.ItemTextCalculatorColorHex = Config.Bind("Settings", "ItemTextCalculatorColorHex", "#FF3333", "The default text color for items that meet the criteria, value must be a hexadecimal");
            BetterItemScanModBase.ItemScaningUICooldown = Config.Bind("Settings", "ItemScaningUICooldown", 3f, "The default value is 5f, how long the ui stays on screen");
            BetterItemScanModBase.AdjustScreenPositionXaxis = Config.Bind("Settings", "AdjustScreenPositionXaxis", 0f, "The default value is 0, you will add or take away from its original position");
            BetterItemScanModBase.AdjustScreenPositionYaxis = Config.Bind("Settings", "AdjustScreenPositionYaxis", 0f, "The default value is 0, you will add or take away from its original position");
        }

    }
}
