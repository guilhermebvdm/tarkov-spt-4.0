using BepInEx;
using System;

namespace TRLFixes
{
    // TRL Fixes v1.5.0: Correções essenciais de IA, combate e estabilidade do jogo base EFT.
    // Nota: Patches específicos do FIKA (inventário, revive, slot views, UI main thread, empty hands)
    // foram graduados e integrados nativamente ao código-fonte do FIKA v2.3.10, eliminando
    // double-patching e interceptações redundantes via Harmony.
    [BepInPlugin("com.trl.fixes", "TRL Fixes", "1.5.0")]
    public class Plugin : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;
            Logger.LogInfo("TRL-Fixes: Carregando patches do jogo base e IA...");

            try
            {
                new Patches.FlashbangBotPatch().Enable();
                Logger.LogInfo("TRL-Fixes: FlashbangBotPatch ativado com sucesso.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"TRL-Fixes: Falha ao carregar FlashbangBotPatch: {ex.Message}");
            }

            try
            {
                new Patches.FlashbangRadiusPatch().Enable();
                Logger.LogInfo("TRL-Fixes: FlashbangRadiusPatch ativado com sucesso.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"TRL-Fixes: Falha ao carregar FlashbangRadiusPatch: {ex.Message}");
            }

            try
            {
                new Patches.PickupAimingSafetyPatch().Enable();
                Logger.LogInfo("TRL-Fixes: PickupAimingSafetyPatch ativado com sucesso.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"TRL-Fixes: Falha ao carregar PickupAimingSafetyPatch: {ex.Message}");
            }

            try
            {
                new Patches.DynamicMapsSafetyPatch().Enable();
                Logger.LogInfo("TRL-Fixes: DynamicMapsSafetyPatch ativado com sucesso.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"TRL-Fixes: Falha ao carregar DynamicMapsSafetyPatch: {ex.Message}");
            }

            try
            {
                new Patches.BotMountWeaponFixPatch().Enable();
                new Patches.GClass81ShallUseNowPatch().Enable();
                new Patches.BotStationaryWeaponDataMethod4Patch().Enable();
                new Patches.BotStationaryWeaponDataDropCurWeaponPatch().Enable();
                new Patches.PlayerOperateStationaryWeaponPatch().Enable();
                Logger.LogInfo("TRL-Fixes: BotMountWeaponFixPatch (EFT Base) ativado com sucesso.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"TRL-Fixes: Falha ao carregar BotMountWeaponFixPatch: {ex.Message}");
            }

            try
            {
                new Patches.BotWeaponManagerSafetyPatch().Enable();
                Logger.LogInfo("TRL-Fixes: BotWeaponManagerSafetyPatch ativado com sucesso.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"TRL-Fixes: Falha ao carregar BotWeaponManagerSafetyPatch: {ex.Message}");
            }
        }
    }
}
