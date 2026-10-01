using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace SAIN.Patches.Fixes;

/// <summary>
/// This patch was added because BSG added a method that checks if people are headshot within a certain distance for certain bots
/// And if so, will stop that damage entirely.
/// </summary>
public class RemoveSkillIssueController : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BotOwner), nameof(BotOwner.ShouldApplyDamage));
    }

    [PatchTranspiler]
    public static IEnumerable<CodeInstruction> Transpile()
    {
        return [new CodeInstruction(OpCodes.Ldc_I4_1), new CodeInstruction(OpCodes.Ret)];
    }
}
