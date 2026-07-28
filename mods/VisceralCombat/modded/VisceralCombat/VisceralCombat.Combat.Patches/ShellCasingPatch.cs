using System.Reflection;
using EFT.AssetsManager;
using SPT.Reflection.Patching;

namespace VisceralCombat.Combat.Patches;

public class ShellCasingPatch : ModulePatch
{
	protected override MethodBase GetTargetMethod()
	{
		return typeof(AmmoPoolObject).GetMethod("Update", BindingFlags.Instance | BindingFlags.Public);
	}

	[PatchPrefix]
	private static bool Prefix(AmmoPoolObject __instance)
	{
		return !VisceralEntry.Instance.NeverDeleteShells.Value;
	}
}
