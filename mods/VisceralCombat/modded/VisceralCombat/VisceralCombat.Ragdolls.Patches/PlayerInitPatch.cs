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
	private static async void Postfix(Player __instance, Task __result)
	{
		if (!__instance.IsYourPlayer)
		{
			await __result;
			if (VisceralEntry.Instance.UseActiveRagdolls.Value)
			{
				Utils.SetupPuppetMaster(__instance);
				QuickLogger.Log(ELogType.Log, "Setup Puppet Master for " + __instance.Profile.Nickname);
			}
		}
	}
}
