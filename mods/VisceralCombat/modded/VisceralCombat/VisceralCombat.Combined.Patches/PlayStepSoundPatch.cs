using System.Reflection;
using EFT;
using SPT.Reflection.Patching;

namespace VisceralCombat.Combined.Patches;

public class PlayStepSoundPatch : ModulePatch
{
	protected override MethodBase GetTargetMethod()
	{
		return typeof(Player).GetMethod("PlayStepSound", BindingFlags.Instance | BindingFlags.Public);
	}

	[PatchPrefix]
	private static bool Prefix(Player __instance)
	{
		if (__instance == null || __instance.HealthController == null || (!__instance.HealthController.IsAlive && !VisceralCombat.Ragdolls.Classes.RagdollHelperClass.IsPlayerDowned(__instance)))
		{
			// Suppress PlayStepSound on dead players/bots to prevent NRE in method_68 via FikaPlayer listener
			return false;
		}
		return true;
	}
}
