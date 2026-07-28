using System;
using System.Reflection;
using EFT.Interactive;
using SPT.Reflection.Patching;
using UnityEngine;

namespace VisceralCombat.Ragdolls.Patches;

public class PhysicalItemsPatch : ModulePatch
{
	protected override MethodBase GetTargetMethod()
	{
		return typeof(LootItem).GetMethod("IsRigidbodyDone", BindingFlags.Instance | BindingFlags.Public, null, Array.Empty<Type>(), null);
	}

	[PatchPrefix]
	private static bool Prefix(LootItem __instance, ref bool __result)
	{
		if (VisceralEntry.Instance.ItemForce.Value)
		{
			((Component)__instance).gameObject.layer = LayerMask.NameToLayer("Deadbody");
			__result = false;
			return false;
		}
		return true;
	}
}
