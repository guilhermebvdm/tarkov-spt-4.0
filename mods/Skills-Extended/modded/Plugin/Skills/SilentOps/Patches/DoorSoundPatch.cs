using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.Interactive;
using HarmonyLib;
using SkillsExtended.Helpers;
using SkillsExtended.Skills.Core;
using SkillsExtended.Utils;
using SPT.Reflection.Patching;
using UnityEngine;

namespace SkillsExtended.Skills.SilentOps.Patches;

public class DoorSoundPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(WorldInteractiveObject), nameof(WorldInteractiveObject.PlaySound));
    }

    [PatchPrefix]
    private static bool Prefix(WorldInteractiveObject __instance, EDoorState state)
    {
        // Door sounds don't exist on the headless
        if (!SkillsExtendedPlugin.SkillData.SilentOps.Enabled || SkillsExtendedInfo.IsFikaHeadless)
        {
            return true;
        }

        // ref: AUD-01-01 — só aplica o bônus quando a interação em andamento é do jogador local.
        // Sem dado (porta não rastreada por DoorInteractionTrackerPatch) = fail-safe pro som nativo.
        if (!DoorInteractionTrackerPatch.LastInteractionIsLocal.TryGetValue(__instance.Id, out var isLocal) || !isLocal)
        {
            return true;
        }

        var skillManager = GameUtils.GetSkillManager();
        if (skillManager == null)
        {
            return true;
        }

        if (__instance.OpenSound.Length != 0 && state == EDoorState.Open)
        {
            PlayDoorOpenSound(__instance, skillManager);
        }

        if (__instance.SqueakSound.Length != 0)
        {
            PlayDoorSqueakSound(__instance, skillManager);
        }

        return false;
    }

    private static void PlayDoorOpenSound(WorldInteractiveObject door, SkillManager skillManager)
    {
        var openSound = door.OpenSound[Random.Range(0, door.OpenSound.Length)];
        var bonus = 1f - skillManager.SkillManagerExtended.SilentOpsReduceVolumeBuff;

        if (openSound)
        {
            Singleton<BetterAudio>.Instance.PlayAtPoint(
                door.transform.position,
                openSound,
                CameraClass.Instance.Distance(door.transform.position),
                BetterAudio.AudioSourceGroupType.Collisions,
                35,
                Random.Range(0.8f * bonus, 1f * bonus),
                EOcclusionTest.Fast);
        }
    }

    private static void PlayDoorSqueakSound(WorldInteractiveObject door, SkillManager skillManager)
    {
        var squeakSound = door.SqueakSound[Random.Range(0, door.SqueakSound.Length)];
        var bonus = 1f - skillManager.SkillManagerExtended.SilentOpsReduceVolumeBuff;

        if (squeakSound)
        {
            Singleton<BetterAudio>.Instance.PlayAtPoint(
                door.transform.position,
                squeakSound,
                CameraClass.Instance.Distance(door.transform.position),
                BetterAudio.AudioSourceGroupType.Collisions,
                35,
                Random.Range(0.8f * bonus, 1f * bonus),
                EOcclusionTest.Fast);
        }
    }
}