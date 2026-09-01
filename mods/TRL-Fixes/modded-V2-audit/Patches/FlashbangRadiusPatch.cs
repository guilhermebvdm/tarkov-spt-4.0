using HarmonyLib;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;
using Comfort.Common;
using System;

namespace TRLFixes.Patches
{
    public class FlashbangRadiusPatch
    {
        public void Enable()
        {
            try
            {
                var harmony = new Harmony("com.trl.fixes.flashbangradius");
                var originalMethod = AccessTools.Method(typeof(EFT.Grenade), "Explosion");
                var postfixMethod = AccessTools.Method(typeof(FlashbangRadiusPatch), nameof(Postfix));
                
                if (originalMethod != null)
                {
                    harmony.Patch(originalMethod, postfix: new HarmonyMethod(postfixMethod));
                    Plugin.Log?.LogInfo("TRL-Fixes: Hook no EFT.Grenade.Explosion (Raio da Flashbang) aplicado com sucesso!");
                }
                else
                {
                    Plugin.Log?.LogError("TRL-Fixes: Erro ao encontrar EFT.Grenade.Explosion");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"TRL-Fixes: Erro ao aplicar FlashbangRadiusPatch: {ex}");
            }
        }

        public static void Postfix(IExplosiveItem grenadeItem, Vector3 grenadePosition)
        {
            try
            {
                // Verifica se a granada tem componente de cegueira (Z > 0)
                if (grenadeItem == null || grenadeItem.Blindness == Vector3.zero) 
                    return;
                
                var gameWorld = Singleton<GameWorld>.Instance;
                if (gameWorld == null || gameWorld.AllAlivePlayersList == null)
                    return;

                foreach (var player in gameWorld.AllAlivePlayersList)
                {
                    // Queremos atingir apenas IA ativa e viva
                    if (player == null || !player.IsAI || player.HealthController == null || !player.HealthController.IsAlive)
                        continue;

                    BotOwner botOwner = player.AIData?.BotOwner;
                    if (botOwner == null || botOwner.BotState != EBotState.Active)
                        continue;

                    // Null safety defensivo em PlayerBones e Head (AUD-01-02)
                    if (player.PlayerBones?.Head == null)
                        continue;

                    Vector3 headPosition = player.PlayerBones.Head.position;

                    // Checa distância (20 metros)
                    float distance = Vector3.Distance(grenadePosition, headPosition);
                    if (distance > 20f)
                        continue;

                    Vector3 directionToExplosion = (grenadePosition - headPosition).normalized;
                    float dot = Vector3.Dot(directionToExplosion, player.LookDirection);

                    // Tolerância maior para o ângulo: > -0.3f (visão periférica / ligeiramente atrás do ombro)
                    if (dot > -0.3f)
                    {
                        // Raycast ignorando obstáculos pequenos, testando apenas paredes e terreno (HighPolyWithTerrainMaskAI)
                        if (!Physics.Linecast(headPosition, grenadePosition, LayerMaskClass.HighPolyWithTerrainMaskAI))
                        {
                            if (botOwner.FlashGrenade != null && !botOwner.FlashGrenade.IsFlashed)
                            {
                                // Resgate defensivo do coeficiente de flashbang
                                float coef = botOwner.Settings?.FileSettings?.Grenade?.FLASH_GRENADE_TIME_COEF ?? 1f;
                                float time = grenadeItem.Blindness.z * coef;
                                
                                // Força a IA a reconhecer o efeito de cegueira de forma absoluta
                                botOwner.FlashGrenade.AddBlindEffect(time, grenadePosition);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log?.LogError($"TRL-Fixes: Exceção no Postfix de FlashbangRadiusPatch: {ex.Message}");
            }
        }
    }
}
