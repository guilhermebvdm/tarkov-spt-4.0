using BepInEx;
using System;

namespace TRLFixes
{
    // SoftDependency do FIKA: o FixFikaReviveRagdollPatch resolve o tipo do FIKA por NOME
    // (AccessTools.TypeByName). Sem declarar a dependência, a ordem de carga do BepInEx é
    // indeterminada — se este plugin subir antes do FIKA o tipo não resolve, o patch é dispensado
    // e o log diz "FIKA nao detectado". Falha SILENCIOSA disfarçada de "FIKA não instalado", que
    // custaria uma sessão de teste inteira. Soft = ordena a carga se presente, não exige.
    // GUID confirmado: fika-plugin/Fika.Core/FikaPlugin.cs:40. Mesmo padrão de DiscordRaidMap e MOAR-Client.
    [BepInPlugin("com.trl.fixes", "TRL Fixes", "1.2.2")]
    [BepInDependency("com.fika.core", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public static BepInEx.Logging.ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;
            Logger.LogInfo("TRL-Fixes: Carregando patches...");

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
                new Patches.FixFikaReviveRagdollPatch().Enable();
                Logger.LogInfo("TRL-Fixes: FixFikaReviveRagdollPatch ativado com sucesso.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"TRL-Fixes: Falha ao carregar FixFikaReviveRagdollPatch: {ex.Message}");
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
                new Patches.FikaMainThreadUISafetyPatch().Enable();
                Logger.LogInfo("TRL-Fixes: FikaMainThreadUISafetyPatch ativado com sucesso.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"TRL-Fixes: Falha ao carregar FikaMainThreadUISafetyPatch: {ex.Message}");
            }

            try
            {
                new Patches.BotMountWeaponFixPatch().Enable();
                new Patches.GClass81ShallUseNowPatch().Enable();
                new Patches.BotStationaryWeaponDataMethod4Patch().Enable();
                new Patches.FikaPlayerOperateStationaryWeaponPatch().Enable();
                new Patches.PlayerOperateStationaryWeaponPatch().Enable();
                Logger.LogInfo("TRL-Fixes: BotMountWeaponFixPatch ativado com sucesso.");
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
