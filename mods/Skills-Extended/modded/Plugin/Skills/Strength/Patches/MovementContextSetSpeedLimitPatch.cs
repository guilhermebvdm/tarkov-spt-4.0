using System.Collections.Generic;
using System.Reflection;
using EFT;
using EFT.Interactive;
using HarmonyLib;
using SkillsExtended.Helpers;
using SkillsExtended.Skills.Core;
using SkillsExtended.Utils;
using SPT.Reflection.Patching;

namespace SkillsExtended.Skills.Strength.Patches;

public class MovementContextSetSpeedLimitPatch : ModulePatch
{
	protected override MethodBase GetTargetMethod()
	{
		return AccessTools.Method(typeof(MovementContext), nameof(MovementContext.method_0));
	}

	[PatchPrefix]
	public static bool Prefix(MovementContext __instance, Player ____player)
		// ____player = campo privado `_player` de MovementContext (Assembly-CSharp/EFT/MovementContext.cs:157),
		// mesma convenção já usada em Plugin/Skills/FirstAid/Patches/CanWalkPatch.cs.
	{
		var skillData = SkillsExtendedPlugin.SkillData;
		if (!skillData.Strength.Enabled)
		{
			return true;
		}

		// ref: AUD-01-02 — só aplica a skill do jogador local a movimento do próprio jogador local.
		if (____player == null || !____player.IsYourPlayer)
		{
			return true; // não é o jogador local — deixa o método nativo (MovementContext.method_0) rodar
		}

		var skillManager = GameUtils.GetSkillManager();
		if (skillManager == null)
		{
			return true;
		}

		var skillMgrExt = skillManager.SkillManagerExtended;

		MovementContext.Struct333 gStruct;
		gStruct.movementContext_0 = __instance;
		gStruct.conditions = EPhysicalCondition.None;

		var flag = false;
		foreach (var collider in __instance.EnteredObstacles)
		{
			gStruct.conditions |= collider.ConditionsMask;
			flag |= collider.HasSwampSpeedLimit;
		}
		
		__instance.method_28(EPhysicalCondition.ProneDisabled, ref gStruct);
		__instance.method_28(EPhysicalCondition.ProneMovementDisabled, ref gStruct);

		var bushSpeedElite = skillMgrExt.StrengthBushSpeedIncBuffElite;
		if (!bushSpeedElite)
		{
			__instance.method_28(EPhysicalCondition.SprintDisabled, ref gStruct);
			__instance.method_28(EPhysicalCondition.JumpDisabled, ref gStruct);
		}
		
		if (__instance.PhysicalConditionIs(EPhysicalCondition.SprintDisabled))
		{
			__instance.EnableSprint(false);
		}

		if (flag)
		{
			var speedLimit = bushSpeedElite.Value 
				? 1f
				: 0.2f * (1 + skillMgrExt.StrengthBushSpeedIncBuff);

#if DEBUG
			Logger.LogDebug($"Collider speed limit: {speedLimit} :: IsElite {bushSpeedElite.Value}");
#endif
			
			__instance.AddStateSpeedLimit(speedLimit, Player.ESpeedLimit.Swamp);
			return false;
		}
		
		__instance.RemoveStateSpeedLimit(Player.ESpeedLimit.Swamp);
		return false;
	}
}