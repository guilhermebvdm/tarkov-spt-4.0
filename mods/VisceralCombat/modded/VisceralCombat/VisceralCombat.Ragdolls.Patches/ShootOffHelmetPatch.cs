using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.Ballistics;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using UnityEngine;

namespace VisceralCombat.Ragdolls.Patches;

public class ShootOffHelmetPatch : ModulePatch
{
	protected override MethodBase GetTargetMethod()
	{
		return typeof(Player).GetMethod("ReceiveDamage", BindingFlags.Instance | BindingFlags.Public);
	}

	[PatchPostfix]
	private static void Postfix(Player __instance, EBodyPart part, MaterialType special)
	{
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_0028: Invalid comparison between Unknown and I4
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0032: Invalid comparison between Unknown and I4
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0037: Invalid comparison between Unknown and I4
		if (!VisceralEntry.Instance.ShootHelmetOff.Value || !__instance.IsAI || (int)part != 0 || ((int)special != 34 && (int)special != 36))
		{
			return;
		}
		float num = Random.Range(0f, 100f);
		if (num <= VisceralEntry.Instance.HelmetShootOffChance.Value)
		{
			Slot slot = __instance.Inventory.Equipment.GetSlot((EquipmentSlot)11);
			if (slot.ContainedItem != null)
			{
				((TraderControllerClass)__instance.InventoryController).ThrowItem(slot.ContainedItem, false, (Callback)null);
			}
		}
	}
}
