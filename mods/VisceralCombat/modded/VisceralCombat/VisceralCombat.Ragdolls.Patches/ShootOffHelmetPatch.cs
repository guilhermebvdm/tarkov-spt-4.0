using System;
using System.Reflection;
using EFT;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using UnityEngine;
using Random = UnityEngine.Random;

namespace VisceralCombat.Ragdolls.Patches;

public class ShootOffHelmetPatch : ModulePatch
{
	protected override MethodBase GetTargetMethod()
	{
		return typeof(Player).GetMethod("ApplyDamageInfo", BindingFlags.Instance | BindingFlags.Public, null, new Type[] { typeof(DamageInfoStruct), typeof(EBodyPart), typeof(EBodyPartColliderType), typeof(float) }, null);
	}

	[PatchPostfix]
	private static void Postfix(Player __instance, DamageInfoStruct damageInfo, EBodyPart bodyPartType, EBodyPartColliderType colliderType, float absorbed)
	{
		if (VisceralEntry.Instance == null || !VisceralEntry.Instance.ShootHelmetOff.Value || !__instance.IsAI) return;
		if (bodyPartType != EBodyPart.Head) return;

		// Verify helmet / head area hit
		if (colliderType != EBodyPartColliderType.HeadCommon &&
		    colliderType != EBodyPartColliderType.ParietalHead &&
		    colliderType != EBodyPartColliderType.BackHead &&
		    colliderType != EBodyPartColliderType.Eyes &&
		    colliderType != EBodyPartColliderType.Ears &&
		    colliderType != EBodyPartColliderType.Jaw &&
		    colliderType != EBodyPartColliderType.NeckFront &&
		    colliderType != EBodyPartColliderType.NeckBack)
		{
			return;
		}

		float num = Random.Range(0f, 100f);
		if (num <= VisceralEntry.Instance.HelmetShootOffChance.Value)
		{
			Slot slot = __instance.Inventory?.Equipment?.GetSlot(EquipmentSlot.Headwear);
			if (slot?.ContainedItem != null && __instance.InventoryController is TraderControllerClass controller)
			{
				controller.ThrowItem(slot.ContainedItem, false, null);
			}
		}
	}
}
