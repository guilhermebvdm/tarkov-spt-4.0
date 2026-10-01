using System.Reflection;
using Comfort.Common;
using EFT;
using HarmonyLib;
using SkillsExtended.Helpers;
using SkillsExtended.Skills.Core;
using SPT.Reflection.Patching;

namespace SkillsExtended.Skills.SilentOps.Patches;

public class MeleeSpeedPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        // ref: ObjectInHandsAnimator.cs:214 (SetMeleeSpeed)
        return AccessTools.Method(typeof(ObjectInHandsAnimator), nameof(ObjectInHandsAnimator.SetMeleeSpeed));
    }

    [PatchPrefix]
    private static void Prefix(ObjectInHandsAnimator __instance, ref float speed)
    {
        if (!SkillsExtendedPlugin.SkillData.SilentOps.Enabled)
        {
            return;
        }

        if (!Singleton<GameWorld>.Instantiated)
        {
            return;
        }

        // ref: AUD-01-16 — Player.HandsAnimator (EFT/Player.cs:24721) não tem referência inversa embutida
        // no ObjectInHandsAnimator, então resolvemos o dono varrendo GameWorld.AllAlivePlayersList
        // (EFT/GameWorld.cs:556) em vez de assumir sempre o MainPlayer.
        Player owner = null;
        var players = Singleton<GameWorld>.Instance.AllAlivePlayersList;
        for (var i = 0; i < players.Count; i++)
        {
            if (players[i].HandsAnimator == __instance)
            {
                owner = players[i];
                break;
            }
        }

        var skillManager = owner?.Skills;
        if (skillManager?.SkillManagerExtended == null)
        {
            return;
        }

        speed *= 1 + skillManager.SkillManagerExtended.SilentOpsIncMeleeSpeedBuff;
    }
}
