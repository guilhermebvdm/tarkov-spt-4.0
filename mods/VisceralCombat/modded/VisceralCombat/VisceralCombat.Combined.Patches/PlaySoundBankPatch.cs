using System.Reflection;
using EFT;
using SPT.Reflection.Patching;

namespace VisceralCombat.Combined.Patches;

public class PlaySoundBankPatch : ModulePatch
{
	protected override MethodBase GetTargetMethod()
	{
		return typeof(Player).GetMethod("PlaySoundBank", BindingFlags.Instance | BindingFlags.Public);
	}

	[PatchPrefix]
	private static bool Prefix(Player __instance)
	{
		if (__instance == null || __instance.HealthController == null || !__instance.HealthController.IsAlive)
		{
			// Suppress animation soundbank events for dead players/bots to avoid NRE on missing AudioSource
			return false;
		}
		return true;
	}
}
