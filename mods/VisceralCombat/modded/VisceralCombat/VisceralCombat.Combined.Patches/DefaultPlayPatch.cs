using System;
using System.Reflection;
using EFT;
using SPT.Reflection.Patching;

namespace VisceralCombat.Combined.Patches;

public class DefaultPlayPatch : ModulePatch
{
	protected override MethodBase GetTargetMethod()
	{
		return typeof(Player).GetMethod("DefaultPlay", BindingFlags.Instance | BindingFlags.Public);
	}

	[PatchPrefix]
	private static bool Prefix(Player __instance)
	{
		if (__instance == null || __instance.HealthController == null || !__instance.HealthController.IsAlive)
		{
			// Suppress DefaultPlay movement audio events on dead players/bots to avoid NRE on missing BetterSource
			return false;
		}
		return true;
	}
}
