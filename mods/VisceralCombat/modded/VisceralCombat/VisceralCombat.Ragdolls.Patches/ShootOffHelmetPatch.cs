using System;
using System.Reflection;
using EFT;
using EFT.InventoryLogic;
using Fika.Core.Main.Utils;
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
		// ref: CR-02-02 - ShootOffHelmet é exclusivamente para bots VIVOS (arrancar capacete em combate).
		// Se o tiro foi fatal, method_35 já disparou o SetupCorpseSyncPacket do Fika antes deste Postfix;
		// executar ThrowItem após a morte aqui causava duplicação do capacete no cadáver.
		// Em coop, bots são autoritativamente gerenciados pelo host (FikaServer) ou solo SPT.
		if (__instance.HealthController == null || !__instance.HealthController.IsAlive) return;
		if (!(FikaBackendUtils.IsServer || FikaBackendUtils.IsSinglePlayer)) return;

		if (VisceralEntry.Instance == null || !VisceralEntry.Instance.IsCategoryActive(VisceralEntry.Instance.ShootHelmetOff) || !__instance.IsAI) return; // ref: item 005
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
