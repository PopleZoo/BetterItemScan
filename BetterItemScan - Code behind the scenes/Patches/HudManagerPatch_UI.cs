
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private static float _displayTimeLeft = BetterItemScanModBase.ItemScaningUICooldown.Value;
        private static float Totalsum;
        private static float Totalship;
        public static List<ScanNodeProperties> scannedNodeObjects = new List<ScanNodeProperties>();
        public static List<string> meetQuotaItemNames = new List<string>();
        public static Dictionary<ScanNodeProperties, int> ItemsDictionary = new Dictionary<ScanNodeProperties, int>();

        public static int MeetQuota(List<int> items, int quota)
        {
            // Filter out items that are smaller than the quota
            items = items.Where(item => item >= quota).ToList();

            // If no items are left, return -1 to indicate that no item can meet the quota
            if (!items.Any())
            {
                return -1;
            }

            // Subtract each item from the quota and return the item that results in the smallest non-negative difference
            return items.Aggregate((a, b) => Math.Abs(quota - a) < Math.Abs(quota - b) ? a : b);
        }

        public static List<List<int>> FindCombinations(List<int> items, int quota)
        {
            List<List<int>> results = new List<List<int>>();
            int n = items.Count;
            int bestSumDifference = int.MaxValue;

            // Generate all subsets of the items
            for (int i = 0; i < (1 << n); i++)
            {
                List<int> subset = new List<int>();
                for (int j = 0; j < n; j++)
                {
                    if ((i & (1 << j)) > 0)
                    {
                        subset.Add(items[j]);
                    }
                }

                // Calculate the sum of the subset
                int subsetSum = subset.Sum();

                // If the sum of the subset is close to the quota, update the best result
                int sumDifference = Math.Abs(quota - subsetSum);
                if (sumDifference < bestSumDifference)
                {
                    bestSumDifference = sumDifference;
                    results.Clear();
                    results.Add(subset);
                }
                else if (sumDifference == bestSumDifference)
                {
                    results.Add(subset);
                }
            }

            // Sort the results by the size of the combination, ascending
            results.Sort((a, b) => a.Count.CompareTo(b.Count));

            return results;
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(HUDManager), "PingScan_performed")]
        private static void OnScan(HUDManager __instance, InputAction.CallbackContext context)
        {
            FieldInfo fieldInfo_2 = typeof(HUDManager).GetField("playerPingingScan", BindingFlags.NonPublic | BindingFlags.Instance);
            var playerPingingScan = (float)fieldInfo_2.GetValue(__instance);

            MethodInfo methodInfo = typeof(HUDManager).GetMethod("CanPlayerScan", BindingFlags.NonPublic | BindingFlags.Instance);

            if ((UnityEngine.Object)GameNetworkManager.Instance.localPlayerController == (UnityEngine.Object)null || !context.performed || !(bool)methodInfo.Invoke(__instance,null) || (double)playerPingingScan > -1.0)
                return;
            if (!(bool)(UnityEngine.Object)HudManagerPatch_UI._totalCounter)
                    HudManagerPatch_UI.CopyValueCounter();
                List<ScanNodeProperties> items = CalculateLootItems(); // Get the list of items
                meetQuotaItemNames.Clear();
                foreach (var item in items)
                {
                    ItemsDictionary.Add(item, item.scrapValue);
                }

                int quota = TimeOfDay.Instance.profitQuota;
                List<int> itemsValues = ItemsDictionary.Values.ToList();
                itemsValues.Sort();
                itemsValues.Reverse();
                int bestIndividualItem = MeetQuota(itemsValues, quota);
                List<List<int>> bestCombinations = FindCombinations(itemsValues, quota);

                foreach (List<int> combination in bestCombinations)
                {
                    Debug.Log(string.Join(", ", combination));
                }

                // Compare the best individual item to the best combinations
                if (bestIndividualItem != -1 && bestCombinations.Any())
                {
                    int bestCombinationSum = bestCombinations.First().Sum();
                    //Debug.Log($"Best Individual Item: {bestIndividualItem}");
                    //Debug.Log($"Best Combination Sum: {bestCombinationSum}");

                    // Determine the winner based on the difference between individual item and combination sum
                    int selectedValue = -1;

                    if (Math.Abs(quota - bestIndividualItem) <= Math.Abs(quota - bestCombinationSum))
                    {
                        selectedValue = bestIndividualItem;
                    }
                    else if (bestCombinations.Any())
                    {
                        // Select the first item in the best combination (you may want to refine the selection logic)
                        selectedValue = bestCombinations.First().First();
                    }

                    // Add the selected item to the meetQuotaItemNames list
                    if (selectedValue != -1)
                    {
                        ScanNodeProperties key = ItemsDictionary.FirstOrDefault(x => x.Value == selectedValue).Key;
                        if (key != null)
                        {
                            meetQuotaItemNames.Add(key.headerText);
                        }
                    }

                }
                else if (bestIndividualItem != -1)
                {
                    // No valid combinations, consider the best individual item
                    ScanNodeProperties key = ItemsDictionary.FirstOrDefault(x => x.Value == bestIndividualItem).Key;
                    if (key != null)
                    {
                        meetQuotaItemNames.Add(key.headerText);
                    }
                }

                ItemsDictionary.Clear();
                string itemList = "";
                foreach (var item in items)
                {
                    string itemText = $"{item.headerText}: ${item.scrapValue}";
                    if (!BetterItemScanModBase.CalculateForQuota.Value && meetQuotaItemNames.Contains(item.headerText))
                    {
                        meetQuotaItemNames.Remove(item.headerText);// -_- took way too long to remember this
                        itemText = "* " + itemText;
                    }
                    itemList += itemText + "\n";
                }

                HudManagerPatch_UI._textMesh.text = itemList;
                if (BetterItemScanModBase.ShowShipTotalOnShipOnly.Value)
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

                if (BetterItemScanModBase.ShowTotalOnShipOnly.Value)
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


                HudManagerPatch_UI._displayTimeLeft = BetterItemScanModBase.ItemScaningUICooldown.Value;
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
