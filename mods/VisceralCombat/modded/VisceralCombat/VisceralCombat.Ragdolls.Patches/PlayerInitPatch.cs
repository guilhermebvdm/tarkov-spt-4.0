using System.Reflection;
using System.Threading.Tasks;
using EFT;
using SPT.Reflection.Patching;
using VisceralCombat.Ragdolls.Classes;

namespace VisceralCombat.Ragdolls.Patches;

public class PlayerInitPatch : ModulePatch
{
	protected override MethodBase GetTargetMethod()
	{
		return typeof(Player).GetMethod("Init");
	}

	[PatchPostfix]
	private static void Postfix(Player __instance, Task __result)
	{
		// ref: item 005 (§5.5) — checagem ANTES de agendar o ContinueWith, não só dentro dele
		if (VisceralEntry.Instance == null || !VisceralEntry.Instance.VisceralCombatEnabled.Value) return;

		if (!__instance.IsYourPlayer && __result != null)
		{
			__result.ContinueWith(_ =>
			{
				if (VisceralEntry.Instance != null && VisceralEntry.Instance.IsCategoryActive(VisceralEntry.Instance.UseActiveRagdolls)) // ref: item 005
				{
					Utils.SetupPuppetMaster(__instance);
					QuickLogger.Log(ELogType.Log, "Setup Puppet Master for " + __instance.Profile.Nickname);
				}
			}, TaskScheduler.FromCurrentSynchronizationContext());
		}
	}
}
