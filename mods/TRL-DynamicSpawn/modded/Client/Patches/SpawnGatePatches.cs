using System;
using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using TRLDynamicSpawn.Components;
using TRLDynamicSpawn.Helpers;

namespace TRLDynamicSpawn.Patches
{
    /// <summary>
    /// Refuses the vanilla continuous scav spawner (EFT/NonWavesSpawnScenario.cs:160; group variant GClass1876.cs:51 =
    /// NonWaveGroupScenario) at its FIRST step, before any profile is created/chosen. Previously the refusal happened in
    /// TryToSpawnInZoneAndDelay (Patches.cs, TryToSpawnInZoneAndDelayPatch), after BotCreationDataClass.Create +
    /// ChooseProfile had already run — the 10 s metronome measured in V1 (163 refused attempts with 0 bots alive).
    /// marksman (vanilla snipers) is deliberately allowed through (NR-2). BotHalloweenEvent.cs:176 bypasses this entry
    /// (calls BotSpawner directly) and still hits the old backstop.
    /// ref: Assembly-CSharp/EFT/BotsController.cs:536 (public void ActivateBotsWithoutWave(int count, IGetProfileData data))
    /// ref: AUD-01-08
    /// </summary>
    public class ActivateBotsWithoutWavePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
            => AccessTools.Method(typeof(BotsController), nameof(BotsController.ActivateBotsWithoutWave), new[] { typeof(int), typeof(IGetProfileData) });

        [PatchPrefix]
        private static bool Prefix(IGetProfileData data)
        {
            try
            {
                if (FikaHelper.IsClient()) return true;                         // guest: vanilla untouched
                if (DynamicSpawnManager.IsGeneratingDynamicWave) return true;   // defensive; the mod does not use this entry
                if (!(data is BotProfileDataClass bp)) return true;            // unknown provider: let vanilla decide

                var role = bp.WildSpawnType_0;                                  // ref: BotProfileDataClass.cs:16 (public field, already used at Patches.cs ChooseProfilePatch)
                if (role != WildSpawnType.assault && role != WildSpawnType.cursedAssault) return true;

                if (Settings.enableDebugLogs.Value)                             // gate BEFORE formatting (AUD-01-07)
                    Plugin.LogSource.LogInfo($"[TRLDynamicSpawn] Refused vanilla continuous spawn ({role}) before profile creation.");
                return false;
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[TRL-DynamicSpawn] ActivateBotsWithoutWavePatch: {ex}");
                return true;
            }
        }
    }
}
