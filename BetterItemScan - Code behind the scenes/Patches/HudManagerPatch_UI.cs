
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BetterItemScan.Patches
{
    [HarmonyPatch]
    internal class HudManagerPatch_UI
    {
        private static GameObject _totalCounter;
        private static TextMeshProUGUI _textMesh;
        private static float _displayTimeLeft;
        private static float Totalsum;
        private static float Totalship;
        private const float DisplayTime = 5f;

        public static List<ScanNodeProperties> scannedNodeObjects = new List<ScanNodeProperties>();
        public static List<string> meetQuotaItemNames = new List<string>();
        public static string TickLabel = ((char)0x221A).ToString();

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HUDManager), "PingScan_performed")]
        private static void OnScan(HUDManager __instance, InputAction.CallbackContext context)
        {
            if (!(bool)(UnityEngine.Object)HudManagerPatch_UI._totalCounter)
                HudManagerPatch_UI.CopyValueCounter();

            List<ScanNodeProperties> items = CalculateLootItems(); // Get the list of items
            meetQuotaItemNames.Clear();
            int quota = TimeOfDay.Instance.profitQuota;
            items = items.OrderByDescending(item => item.scrapValue).ToList();
            int totalScrapValue = 0;
            bool hasGoneOverQuota=false;
            for (int i = 0; i < items.Count; i++)
            {
                if (totalScrapValue + items[i].scrapValue <= quota)
                {
                    totalScrapValue += items[i].scrapValue;
                    if (!items[i].headerText.Contains(TickLabel))
                    {
                        meetQuotaItemNames.Add(items[i].headerText);
                    }
                }
                else if (!hasGoneOverQuota && quota > totalScrapValue)
                {
                    hasGoneOverQuota = true;
                    totalScrapValue += items[i].scrapValue;
                    if (!items[i].headerText.Contains(TickLabel))
                    {
                        meetQuotaItemNames.Add(items[i].headerText);
                    }

                }
                else break;
            }

            string itemList = "";
            foreach (var item in items)
            {
                string itemText = $"{item.headerText}: ${item.scrapValue}";
                if (BetterItemScanModBase.CalculateForQuota.Value && meetQuotaItemNames.Contains(item.headerText))
                {
                    meetQuotaItemNames.Remove(item.headerText);// -_- took way too long to remember this
                    itemText = "* " + itemText;
                }
                itemList += itemText + "\n";
            }
            HudManagerPatch_UI._textMesh.text = itemList;
            if (BetterItemScanModBase.ShowTotalOnShipOnly.Value)
            {
                if (!GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom)
                {
                    HudManagerPatch_UI._textMesh.text += $"\nTotal Scanned: {Totalsum.ToString()}";
                }
                else
                {
                    HudManagerPatch_UI._textMesh.text += $"\nTotal Scanned: {Totalsum.ToString()} Ship Total: {Totalship.ToString()}";
                }
            }
            else HudManagerPatch_UI._textMesh.text += $"\nTotal Scanned: {Totalsum.ToString()} Ship Total: {Totalship.ToString()}";

            if (BetterItemScanModBase.ShowOnShipOnly.Value)
            {
                if (!GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom)
                {
                    HudManagerPatch_UI._textMesh.gameObject.SetActive(false);
                    __instance.totalValueText.transform.parent.gameObject.SetActive(true);
                }
                else
                {
                    HudManagerPatch_UI._textMesh.gameObject.SetActive(true);
                    __instance.totalValueText.transform.parent.gameObject.SetActive(false);

                }
            }
            

            HudManagerPatch_UI._displayTimeLeft = 5f;
            if (HudManagerPatch_UI._totalCounter.activeSelf)
                return;
            GameNetworkManager.Instance.StartCoroutine(HudManagerPatch_UI.ValueCoroutine());
        }

        private static List<ScanNodeProperties> CalculateLootItems()
        {
            List<GrabbableObject> list = ((IEnumerable<GrabbableObject>)GameObject.Find("/Environment/HangarShip").GetComponentsInChildren<GrabbableObject>()).Where<GrabbableObject>((Func<GrabbableObject, bool>)(obj => obj.name != "ClipboardManual" && obj.name != "StickyNoteItem")).ToList<GrabbableObject>();
            List<ScanNodeProperties> lootItems = scannedNodeObjects.Where(obj => obj.headerText != "ClipboardManual" && obj.headerText != "StickyNoteItem" && obj.scrapValue != 0).ToList();
            Totalsum = (float)lootItems.Sum<ScanNodeProperties>((Func<ScanNodeProperties, int>)(scrap => scrap.scrapValue));
            Totalship = (float)list.Sum<GrabbableObject>((Func<GrabbableObject, int>)(scrap => scrap.scrapValue));
            return lootItems;
        }
        
        private static IEnumerator ValueCoroutine()
        {
            HudManagerPatch_UI._totalCounter.SetActive(true);
            while ((double)HudManagerPatch_UI._displayTimeLeft > 0.0)
            {
                float displayTimeLeft = HudManagerPatch_UI._displayTimeLeft;
                HudManagerPatch_UI._displayTimeLeft = 0.0f;
                yield return (object)new WaitForSeconds(displayTimeLeft);
            }
            HudManagerPatch_UI._totalCounter.SetActive(false);
        }

        private static void CopyValueCounter()
        {
            GameObject gameObject = GameObject.Find("/Systems/UI/Canvas/IngamePlayerHUD/BottomMiddle/ValueCounter");
            if (!(bool)(UnityEngine.Object)gameObject)
                BetterItemScanModBase.Instance.mls.LogError((object)"Failed to find ValueCounter object to copy!");
            HudManagerPatch_UI._totalCounter = UnityEngine.Object.Instantiate<GameObject>(gameObject.gameObject, gameObject.transform.parent, false);
            Vector3 localPosition = HudManagerPatch_UI._totalCounter.transform.localPosition;
            float adjustedX = Mathf.Clamp(localPosition.x + BetterItemScanModBase.AdjustScreenPositionXaxis.Value + 50f, -6000f, Screen.width);
            float adjustedY = Mathf.Clamp(localPosition.y + BetterItemScanModBase.AdjustScreenPositionYaxis.Value - 80f, -6000f, Screen.height);
            HudManagerPatch_UI._totalCounter.transform.localPosition = new Vector3(adjustedX, adjustedY, localPosition.z);
            HudManagerPatch_UI._textMesh = HudManagerPatch_UI._totalCounter.GetComponentInChildren<TextMeshProUGUI>();

            // Set the anchor and pivot of the text's RectTransform to the top of the parent on the y-axis and center on the x-axis
            HudManagerPatch_UI._textMesh.alignment = TextAlignmentOptions.BottomLeft;
            RectTransform textRectTransform = HudManagerPatch_UI._textMesh.rectTransform;
            textRectTransform.anchorMin = new Vector2(0.5f, 2f); // Change the anchorMin to (0.5, 2) to position the child above the parent on the y-axis
            textRectTransform.anchorMax = new Vector2(0.5f, 2f); // Change the anchorMax to (0.5, 2) to position the child above the parent on the y-axis
            textRectTransform.pivot = new Vector2(0.5f, 0f); // Change the pivot to (0.5, 0) to position the child at the center of the parent on the x-axis

            // nicer positioning. smile
            Vector3 textLocalPosition = textRectTransform.localPosition;
            textRectTransform.localPosition = new Vector3(textLocalPosition.x, textLocalPosition.y - 140f, textLocalPosition.z);
        }





    }
}
