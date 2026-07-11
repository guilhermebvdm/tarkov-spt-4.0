using BepInEx;
using System;

namespace TRLFixes
{
    [BepInPlugin("com.trl.fixes", "TRL Fixes", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
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
        }
    }
}
