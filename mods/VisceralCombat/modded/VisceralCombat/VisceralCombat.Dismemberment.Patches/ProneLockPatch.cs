using System.Reflection;
using EFT;
using SPT.Reflection.Patching;
using VisceralCombat.Dismemberment.Classes;

namespace VisceralCombat.Dismemberment.Patches;

/// <summary>
/// Intercepts BotLay.IsLay = false for bots with LivingDismembermentController to prevent standing up near obstacles.
/// </summary>
public class ProneLockPatch : ModulePatch
{
	protected override MethodBase GetTargetMethod()
	{
		PropertyInfo prop = typeof(BotLay).GetProperty("IsLay", BindingFlags.Instance | BindingFlags.Public);
		return prop?.GetSetMethod();
	}

	[PatchPrefix]
	private static bool Prefix(BotLay __instance, ref bool value)
	{
		if (!value && __instance != null)
		{
			Player player = __instance.BotOwner_0?.GetPlayer;
			if (player != null && player.GetComponent<LivingDismembermentController>() != null)
			{
				// Abort standing up for dismembered living bots!
				return false;
			}
		}
		return true;
	}
}

/// <summary>
/// Intercepts BotMover.DoProne(false) for bots with LivingDismembermentController to prevent standing up near obstacles.
/// </summary>
public class ProneMoverDoPronePatch : ModulePatch
{
	protected override MethodBase GetTargetMethod()
	{
		return typeof(BotMover).GetMethod("DoProne", BindingFlags.Instance | BindingFlags.Public);
	}

	[PatchPrefix]
	private static bool Prefix(BotMover __instance, ref bool val)
	{
		if (!val && __instance != null)
		{
			Player player = __instance.BotOwner_0?.GetPlayer;
			if (player != null && player.GetComponent<LivingDismembermentController>() != null)
			{
				// Abort standing up for dismembered living bots!
				return false;
			}
		}
		return true;
	}
}
