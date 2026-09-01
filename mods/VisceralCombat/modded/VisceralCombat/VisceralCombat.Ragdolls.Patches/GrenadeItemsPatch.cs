using System.Collections.Generic;
using System.Reflection;
using EFT;
using EFT.Interactive;
using SPT.Reflection.Patching;
using UnityEngine;

namespace VisceralCombat.Ragdolls.Patches;

public class GrenadeItemsPatch : ModulePatch
{
	protected override MethodBase GetTargetMethod()
	{
		return typeof(Grenade).GetMethod("Explosion", BindingFlags.Static | BindingFlags.Public);
	}

	[PatchPostfix]
	private static void Postfix(IExplosiveItem grenadeItem, Vector3 grenadePosition)
	{
		if (grenadeItem == null || VisceralEntry.Instance == null) return;
		if (VisceralEntry.Instance.ItemForce == null || !VisceralEntry.Instance.ItemForce.Value) return;

		float maxDist = grenadeItem.MaxExplosionDistance;
		int defaultLayerMask = 1 << LayerMask.NameToLayer("Default");
		Collider[] colliders = Physics.OverlapSphere(grenadePosition, maxDist, defaultLayerMask);
		if (colliders == null || colliders.Length == 0) return;

		HashSet<Rigidbody> processedItems = new HashSet<Rigidbody>();
		float forceMultiplier = grenadeItem.GetStrength * 0.5f * VisceralEntry.Instance.GrenadeExplIntensity.Value;

		for (int i = 0; i < colliders.Length; i++)
		{
			Collider col = colliders[i];
			if (col == null) continue;

			Rigidbody rb = col.attachedRigidbody ?? col.GetComponent<Rigidbody>();
			if (rb != null && processedItems.Add(rb))
			{
				if (rb.gameObject.GetComponent<ObservedLootItem>() != null)
				{
					rb.AddExplosionForce(forceMultiplier, grenadePosition, maxDist);
				}
			}
		}
	}
}
