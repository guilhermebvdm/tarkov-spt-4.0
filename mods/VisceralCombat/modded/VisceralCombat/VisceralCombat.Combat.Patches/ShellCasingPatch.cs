using System.Collections.Generic;
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
	private static readonly Queue<AmmoPoolObject> ActiveCasings = new Queue<AmmoPoolObject>();
	private static readonly HashSet<AmmoPoolObject> ActiveCasingsSet = new HashSet<AmmoPoolObject>();

	public static void ClearCasings()
	{
		ActiveCasings.Clear();
		ActiveCasingsSet.Clear();
	}

	[PatchPrefix]
	private static bool Prefix(AmmoPoolObject __instance)
	{
		if (VisceralEntry.Instance != null && VisceralEntry.Instance.NeverDeleteShells != null && VisceralEntry.Instance.NeverDeleteShells.Value)
		{
			if (ActiveCasingsSet.Add(__instance))
			{
				ActiveCasings.Enqueue(__instance);
				if (ActiveCasings.Count > MaxActiveCasings)
				{
					AmmoPoolObject oldest = ActiveCasings.Dequeue();
					if (oldest != null)
					{
						ActiveCasingsSet.Remove(oldest);
						return true; // allows native SPT pooling to clean up old casing
					}
				}
			}
			return false;
		}
		return true;
	}
}
