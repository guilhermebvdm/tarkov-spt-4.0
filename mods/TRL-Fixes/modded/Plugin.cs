using BepInEx;
using BepInEx.Logging;
using System;

namespace TRLFixes
{
    // ref: CR-01-01 (review 01 do item 002) — SoftDependency do Fika é OBRIGATÓRIA para os patches
    // que resolvem tipos do Fika por nome (Patch_FikaReviveHitbox). Sem ela a ordem de carga do
    // BepInEx é indeterminada: se este plugin subir antes do Fika, AccessTools.TypeByName devolve
    // null, o patch é dispensado e o log diz "Fika nao encontrado" — falha SILENCIOSA que se
    // disfarça de "Fika não instalado" e enganaria o diagnóstico do próximo teste.
    // Padrão já usado no repo por DiscordRaidMap e MOAR-Client.
    [BepInPlugin("com.trl.fixes", "TRL Fixes", "1.1.1")]
    [BepInDependency("com.fika.core", BepInDependency.DependencyFlags.SoftDependency)]
    public class Plugin : BaseUnityPlugin
    {
        /// <summary>ref: CR-01-02 — log via ManualLogSource do BepInEx, não UnityEngine.Debug:
        /// Debug.Log escapa da infraestrutura de log do SPT (sem prefixo do plugin, sem nível).
        /// Estático para os patches usarem sem carregar referência ao MonoBehaviour.</summary>
        internal static ManualLogSource Log;

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
                new Patches.Patch_PoolManagerCreateItem().Enable();
                Logger.LogInfo("TRL-Fixes: Patch_PoolManagerCreateItem ativado com sucesso.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"TRL-Fixes: Falha ao carregar Patch_PoolManagerCreateItem: {ex.Message}");
            }

            try
            {
                new Patches.Patch_FikaReviveHitbox().Enable();
                Logger.LogInfo("TRL-Fixes: Patch_FikaReviveHitbox ativado com sucesso.");
            }
            catch (Exception ex)
            {
                Logger.LogError($"TRL-Fixes: Falha ao carregar Patch_FikaReviveHitbox: {ex.Message}");
            }
        }
    }
}
