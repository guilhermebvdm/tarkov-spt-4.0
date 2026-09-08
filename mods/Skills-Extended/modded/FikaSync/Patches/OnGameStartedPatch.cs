using System.Reflection;
using EFT;
using HarmonyLib;
using SkillsExtended.Skills.LockPicking;
using SkillsExtendedFika.Controllers;
using SPT.Reflection.Patching;

namespace SkillsExtendedFika.Patches;

public class OnGameStartedPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));
    }

    [PatchPostfix]
    public static void PatchPostfix(GameWorld __instance)
    {
        // ref: AUD-01-15 — sem isto, DoorAttempts nunca é limpo no headless (o lado Plugin/OnGameStarted.cs
        // que faz isso tem guard `IsFikaHeadless => return`, então nunca roda lá; em sessões headless longas,
        // IDs de porta de mapas diferentes se acumulavam indefinidamente nesse Dictionary estático).
        LockPickingHelpers.DoorAttempts.Clear();
        LockPickingFikaController.GetDoors();
    }
}