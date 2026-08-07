using System;
using System.Reflection;
using EFT.Interactive;
using SPT.Reflection.Patching;
using UnityEngine;
using Random = UnityEngine.Random;
using Object = UnityEngine.Object;

namespace VisceralCombat.Ragdolls.Patches;

public class PhysicalItemsPatch : ModulePatch
{
	protected override MethodBase GetTargetMethod()
	{
		return typeof(LootItem).GetMethod("IsRigidbodyDone", BindingFlags.Instance | BindingFlags.Public, null, Array.Empty<Type>(), null);
	}

	private static int _deadbodyLayer = -1;

	[PatchPrefix]
	private static bool Prefix(LootItem __instance, ref bool __result)
	{
		if (VisceralEntry.Instance != null && VisceralEntry.Instance.ItemForce.Value)
		{
			if (_deadbodyLayer < 0)
			{
				_deadbodyLayer = LayerMask.NameToLayer("Deadbody");
			}
			__instance.gameObject.layer = _deadbodyLayer;
		}
		return true;
	}
}
