using System.Reflection;
using SPT.Reflection.Patching;
using UnityEngine;
using Object = UnityEngine.Object;

namespace VisceralCombat.Ragdolls.Patches;

public class AttachWeaponPatch : ModulePatch
{
	protected override MethodBase GetTargetMethod()
	{
		return typeof(RagdollClass).GetMethod("AttachWeapon", BindingFlags.Instance | BindingFlags.Public);
	}

	[PatchPostfix]
	private static void Postfix(RagdollClass __instance, Rigidbody weaponRigidbody)
	{
		SpringJoint component = ((Component)weaponRigidbody).gameObject.GetComponent<SpringJoint>();
		if ((Object)(object)component != (Object)null)
		{
			Object.Destroy((Object)(object)component);
		}
		else
		{
			((Component)weaponRigidbody).gameObject.SetActive(false);
		}
	}
}
