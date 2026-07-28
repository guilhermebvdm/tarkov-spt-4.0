using System;
using System.Collections.Generic;
using System.Reflection;
using EFT;
using SPT.Reflection.Patching;
using UnityEngine;
using VisceralCombat.Ragdolls.Classes;

namespace VisceralCombat.Ragdolls.Patches;

public class CreateCorpsePatch : ModulePatch
{
	private static string[] TargetBones = new string[6] { "thigh", "calf", "foot", "spine3", "forearm", "head" };

	private static List<string> shitColliders = new List<string>
	{
		"PelvisBack", "SpineLowerChest", "RightSideChestDown", "LeftSideChestDown", "Plate_Granit_SSAPI_side_left_high", "Plate_Granit_SSAPI_side_right_high", "Plate_Granit_SSAPI_side_left_low", "Plate_Granit_SSAPI_side_right_low", "Plate_Korund_side_left_high", "Plate_Korund_side_left_low",
		"Plate_Korund_side_right_high", "Plate_Korund_side_right_low", "Plate_Korund_chest", "Plate_Granit_SAPI_chest", "Plate_Granit_SAPI_back", "Plate_6B13_back", "SpineTopChest", "LeftSideChestTop", "RightSideChestTop", "NeckForward",
		"NeckBackward", "Parietal", "BackHead", "EarsL", "EarsR", "Eyes", "Jaw"
	};

	protected override MethodBase GetTargetMethod()
	{
		return typeof(Player).GetMethod("CreateCorpse", BindingFlags.Instance | BindingFlags.Public, null, Array.Empty<Type>(), null);
	}

	[PatchPostfix]
	private static void Postfix(Player __instance)
	{
		foreach (Transform item in Utils.EnumerateHierarchyCore(__instance.Transform.Original))
		{
			if (shitColliders.Contains(((Object)item).name) && __instance.IsAI)
			{
				((Component)item).gameObject.SetActive(false);
			}
		}
	}
}
