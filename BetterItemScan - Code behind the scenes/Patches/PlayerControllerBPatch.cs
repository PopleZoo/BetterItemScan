using System;
using System.Collections.Generic;
using System.Reflection;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace BetterItemScan.Patches
{
	[HarmonyPatch]
	public class PlayerControllerBPatch_A
	{
		static LineRenderer lineRenderer;
		static GameObject lineObject;
		static float maxDistance = 80f;

		[HarmonyPrefix]
		[HarmonyPatch(typeof(HUDManager), ("AssignNewNodes"))]
		static bool AssignNewNodes_patch(HUDManager __instance, PlayerControllerB playerScript)
		{
			HudManagerPatch_UI.scannedNodeObjects.Clear();

			if (lineRenderer == null)
			{
				lineObject = new GameObject("LineObject");
				lineRenderer = lineObject.AddComponent<LineRenderer>();
				lineRenderer.positionCount = 2;
				lineObject.SetActive(false);
			}

            //fetching Private Vars associated with this method
            FieldInfo fieldInfo_1 = typeof(HUDManager).GetField("nodesOnScreen", BindingFlags.NonPublic | BindingFlags.Instance);
			var _nodesOnScreen = fieldInfo_1.GetValue(__instance) as List<ScanNodeProperties>;

			FieldInfo fieldInfo_2 = typeof(HUDManager).GetField("scannedScrapNum", BindingFlags.NonPublic | BindingFlags.Instance);
			var _scannedScrapNum = fieldInfo_2.GetValue(__instance);

			FieldInfo fieldInfo_3 = typeof(HUDManager).GetField("scanNodesHit", BindingFlags.NonPublic | BindingFlags.Instance);
			var _scanNodesHit = fieldInfo_3.GetValue(__instance) as RaycastHit[];
			//fetching private method 'AttemptScanNode'
			MethodInfo methodInfo = typeof(HUDManager).GetMethod("AttemptScanNode", BindingFlags.NonPublic | BindingFlags.Instance);

			int num = Physics.SphereCastNonAlloc(new Ray(playerScript.gameplayCamera.transform.position + playerScript.gameplayCamera.transform.forward * 20f, playerScript.gameplayCamera.transform.forward), BetterItemScanModBase.ItemScanRadius.Value, _scanNodesHit, maxDistance, 4194304);

			Vector3 origin = new Vector3(playerScript.gameplayCamera.transform.position.x, playerScript.gameplayCamera.transform.position.y - 2f, playerScript.gameplayCamera.transform.position.z) + playerScript.gameplayCamera.transform.forward;
			Vector3 direction = playerScript.gameplayCamera.transform.forward;
			

			if (BetterItemScanModBase.ShowDebugMode.Value)
			{
				lineObject.SetActive(true);
				// Set the start and end points of the line
				lineRenderer.SetPosition(0, origin);
				lineRenderer.SetPosition(1, origin + direction * maxDistance);

				lineRenderer.startWidth = lineRenderer.endWidth = BetterItemScanModBase.ItemScanRadius.Value;
			}
			if (num > __instance.scanElements.Length)
			{
				num = __instance.scanElements.Length;
			}
			_nodesOnScreen.Clear();
			_scannedScrapNum = 0;
			if (num > __instance.scanElements.Length)
			{
				for (int i = 0; i < num; i++)
				{
					ScanNodeProperties component = _scanNodesHit[i].transform.gameObject.GetComponent<ScanNodeProperties>();
					GrabbableObject GrabbableObjectcomponent = _scanNodesHit[i].transform.parent.gameObject.GetComponent<GrabbableObject>();
					if (GrabbableObjectcomponent.itemProperties.isScrap)
					{
                        HudManagerPatch_UI.scannedNodeObjects.Add(component);
                    }
                    if (component.nodeType == 1 || component.nodeType == 2)
					{
						object[] parameters = { component, i, playerScript };
						methodInfo.Invoke(__instance, parameters);
					}
				}
			}
			if (_nodesOnScreen.Count < __instance.scanElements.Length)
			{
				for (int j = 0; j < num; j++)
				{
					ScanNodeProperties component = _scanNodesHit[j].transform.gameObject.GetComponent<ScanNodeProperties>();
					object[] parameters = { component, j, playerScript };
					methodInfo.Invoke(__instance, parameters);
				}
			}
			return false;
		}

	}[HarmonyPatch]
	public class PlayerControllerBPatch_B
	{
		[HarmonyPrefix]
		[HarmonyPatch(typeof(HUDManager), ("AttemptScanNode"))]
		static bool AttemptScanNode_patch(HUDManager __instance, ScanNodeProperties node, int i, PlayerControllerB playerScript)
		{
			MethodInfo MeetsScanNodeRequirements = typeof(HUDManager).GetMethod("MeetsScanNodeRequirements", BindingFlags.NonPublic | BindingFlags.Instance);
			MethodInfo AssignNodeToUIElement = typeof(HUDManager).GetMethod("AssignNodeToUIElement", BindingFlags.NonPublic | BindingFlags.Instance);

			FieldInfo fieldInfo = typeof(HUDManager).GetField("nodesOnScreen", BindingFlags.NonPublic | BindingFlags.Instance);
			var nodesOnScreen = fieldInfo.GetValue(__instance) as List<ScanNodeProperties>;
			FieldInfo fieldInfo_ = typeof(HUDManager).GetField("scannedScrapNum", BindingFlags.NonPublic | BindingFlags.Instance);
			var scannedScrapNum = (int)fieldInfo_.GetValue(__instance);
			FieldInfo fieldInfo__ = typeof(HUDManager).GetField("playerPingingScan", BindingFlags.NonPublic | BindingFlags.Instance);
			var playerPingingScan = (float)fieldInfo__.GetValue(__instance);

			object[] parameters = { node, playerScript };
			object[] parameter = { node };

			if (!(bool)MeetsScanNodeRequirements.Invoke(__instance, parameters))
				return false;
			if (node.nodeType == 2)
				++scannedScrapNum;
			if (!nodesOnScreen.Contains(node))
				nodesOnScreen.Add(node);
				if (node.creatureScanID==-1) HudManagerPatch_UI.scannedNodeObjects.Add(node);
			if ((double)playerPingingScan < 0.0)
				return false;
			AssignNodeToUIElement.Invoke(__instance, parameter);
			return false;

		}

	}
	[HarmonyPatch]
	public class PlayerControllerBPatch_C
	{
		[HarmonyPostfix]
		[HarmonyPatch(typeof(HUDManager), ("Update"))]
		static void Update_patch(HUDManager __instance)
		{
			FieldInfo fieldInfo_1 = typeof(HUDManager).GetField("addingToDisplayTotal", BindingFlags.NonPublic | BindingFlags.Instance);
			var addingToDisplayTotal = fieldInfo_1.GetValue(__instance);
			__instance.totalValueText.transform.parent.gameObject.SetActive(false);
			addingToDisplayTotal = false;
		}

	}
	
}