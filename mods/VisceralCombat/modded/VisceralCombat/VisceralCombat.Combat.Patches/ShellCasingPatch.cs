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

	private const int MaxActiveCasings = 50;
	private static readonly System.Collections.Generic.Queue<AmmoPoolObject> ActiveCasings = new System.Collections.Generic.Queue<AmmoPoolObject>();

	[PatchPrefix]
	private static bool Prefix(AmmoPoolObject __instance)
	{
		if (VisceralEntry.Instance != null && VisceralEntry.Instance.NeverDeleteShells.Value)
		{
			if (!ActiveCasings.Contains(__instance))
			{
				ActiveCasings.Enqueue(__instance);
				if (ActiveCasings.Count > MaxActiveCasings)
				{
					AmmoPoolObject oldest = ActiveCasings.Dequeue();
					if (oldest != null)
					{
						return true; // permite que o Update do SPT limpe a cápsula antiga
					}
				}
			}
			return false;
		}
		return true;
	}
}
