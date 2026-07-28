using System.Reflection;
using EFT;
using SPT.Reflection.Patching;

namespace VisceralCombat.Combined.Patches;

public class KillClientPatch : ModulePatch
{
	protected override MethodBase GetTargetMethod()
	{
		return typeof(Player).GetMethod("ApplyDamageInfo");
	}

	[PatchPostfix]
	private static void Postfix(Player __instance, DamageInfoStruct damageInfo, EBodyPart bodyPartType)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0003: Unknown result type (might be due to invalid IL or missing references)
		KillPatch.Postfix(__instance, damageInfo, bodyPartType);
	}
}
