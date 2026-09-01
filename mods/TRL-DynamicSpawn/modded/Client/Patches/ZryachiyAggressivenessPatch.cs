using System;
using System.Reflection;
using Comfort.Common;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace TRLDynamicSpawn.Patches
{
    /// <summary>
    /// Força agressividade total do Zryachiy em mapas diferentes do Lighthouse (Zryachiy adicional/não-nativo).
    /// Faz o Zryachiy tratar qualquer jogador ou PMC à vista como inimigo imediato.
    /// </summary>
    internal class ZryachiyAggressivenessPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ZyriachyBossLogicClass), nameof(ZyriachyBossLogicClass.IsEnemyNow));
        }

        [PatchPrefix]
        private static bool PatchPrefix(ZyriachyBossLogicClass __instance, IPlayer person, ref bool __result)
        {
            if (person == null) return true;

            try
            {
                var gameWorld = Singleton<GameWorld>.Instance;
                if (gameWorld != null && gameWorld.MainPlayer != null)
                {
                    string mapName = gameWorld.MainPlayer.Location?.ToLower() ?? "";

                    // Se a raid NÃO for no Lighthouse (Zryachiy não-nativo em Woods, Customs, Factory, etc.),
                    // forçamos agressividade total e imediata contra qualquer jogador/PMC!
                    if (!mapName.Contains("lighthouse"))
                    {
                        __result = true;
                        return false; // Bypassa a lógica passiva original do Zryachiy
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[TRL-DynamicSpawn] Error in ZryachiyAggressivenessPatch: {ex.Message}");
            }

            return true;
        }
    }

    /// <summary>
    /// Ao ativar o Zryachiy fora do Lighthouse, registra imediatamente todos os jogadores vivos da sala como inimigos ativos.
    /// </summary>
    internal class ZryachiyActivatePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ZyriachyBossLogicClass), nameof(ZyriachyBossLogicClass.Activate));
        }

        [PatchPostfix]
        private static void PatchPostfix(ZyriachyBossLogicClass __instance)
        {
            if (__instance == null || __instance.BotOwner_0 == null) return;

            try
            {
                var gameWorld = Singleton<GameWorld>.Instance;
                if (gameWorld != null && gameWorld.MainPlayer != null)
                {
                    string mapName = gameWorld.MainPlayer.Location?.ToLower() ?? "";
                    if (!mapName.Contains("lighthouse"))
                    {
                        Plugin.LogSource.LogInfo($"[TRL-DynamicSpawn] Non-Lighthouse Zryachiy activated on {mapName}: Enforcing 100% Total Aggressiveness!");

                        if (gameWorld.RegisteredPlayers != null)
                        {
                            foreach (var p in gameWorld.RegisteredPlayers)
                            {
                                if (p != null && p.Id != __instance.BotOwner_0.GetPlayer.Id)
                                {
                                    __instance.AddEnemy(p, EBotEnemyCause.zryachiyLogic);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource.LogError($"[TRL-DynamicSpawn] Error in ZryachiyActivatePatch: {ex.Message}");
            }
        }
    }
}
