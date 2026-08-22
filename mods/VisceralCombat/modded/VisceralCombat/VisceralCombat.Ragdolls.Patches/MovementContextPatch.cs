using System;
using System.Reflection;
using EFT;
using SPT.Reflection.Patching;

namespace VisceralCombat.Ragdolls.Patches;

public class MovementContextPatch : ModulePatch
{
	private static readonly FieldInfo _playerField = typeof(MovementContext).GetField("_player", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

	protected override MethodBase GetTargetMethod()
	{
		return typeof(MovementContext).GetMethod("ProcessStateEnter", BindingFlags.Instance | BindingFlags.Public);
	}

	[PatchPrefix]
	private static bool Prefix(MovementContext __instance)
	{
		if (__instance == null) return true;

		Player player = _playerField?.GetValue(__instance) as Player;
		if (player != null && (player.HealthController == null || (!player.HealthController.IsAlive && !VisceralCombat.Ragdolls.Classes.RagdollHelperClass.IsPlayerDowned(player))))
		{
			// Cancela o processamento de mudancas de estado de movimento para jogadores realmente mortos,
			// prevenindo chamadas de audio nulo (DefaultPlay) e NullReferenceException durante agonia
			return false;
		}
		return true;
	}
}
