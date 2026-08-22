using System.Collections.Generic;
using System.Reflection;
using EFT;
using SPT.Reflection.Patching;
using UnityEngine;
using VisceralCombat.Ragdolls.Classes;

namespace VisceralCombat.Ragdolls.Patches;

public class GrenadeDeadBodiesPatch : ModulePatch
{
	protected override MethodBase GetTargetMethod()
	{
		return typeof(Grenade).GetMethod("Explosion", BindingFlags.Static | BindingFlags.Public);
	}

	[PatchPostfix]
	private static void Postfix(IExplosiveItem grenadeItem, Vector3 grenadePosition)
	{
		if (grenadeItem == null || VisceralEntry.Instance == null) return;
		float maxDist = grenadeItem.MaxExplosionDistance;

		Collider[] colliders = Physics.OverlapSphere(grenadePosition, maxDist, LayerMasksDataAbstractClass.HitMask);
		if (colliders == null || colliders.Length == 0) return;

		HashSet<Transform> awakenedRoots = new HashSet<Transform>();
		HashSet<Rigidbody> processedRigidbodies = new HashSet<Rigidbody>();
		float forceMultiplier = grenadeItem.GetStrength * 0.5f * VisceralEntry.Instance.GrenadeExplIntensity.Value;

		for (int i = 0; i < colliders.Length; i++)
		{
			Collider col = colliders[i];
			if (col == null) continue;

			Transform root = col.transform.root;
			if (root != null && awakenedRoots.Add(root))
			{
				RagdollHelperClass.WakeCorpse(col, 3.0f);
			}

			Rigidbody rb = col.attachedRigidbody ?? col.GetComponent<Rigidbody>() ?? col.GetComponentInParent<Rigidbody>();
			if (rb != null && !RagdollHelperClass.ParentIsDismembered(rb.transform))
			{
				if (processedRigidbodies.Add(rb))
				{
					rb.AddExplosionForce(forceMultiplier, grenadePosition, maxDist);
				}
			}
		}
	}
}
