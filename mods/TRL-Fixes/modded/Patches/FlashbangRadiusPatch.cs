using HarmonyLib;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;
using System.Collections.Generic;
using Comfort.Common;

namespace TRLFixes.Patches
{
    public class FlashbangRadiusPatch
    {
        public void Enable()
        {
            var harmony = new Harmony("com.trl.fixes.flashbangradius");
            
            var originalMethod = AccessTools.Method(typeof(EFT.Grenade), "Explosion");
            var postfixMethod = AccessTools.Method(typeof(FlashbangRadiusPatch), nameof(Postfix));
            
            if (originalMethod != null)
            {
                harmony.Patch(originalMethod, postfix: new HarmonyMethod(postfixMethod));
                Debug.Log("TRL-Fixes: Hook no EFT.Grenade.Explosion (Raio da Flashbang) aplicado com sucesso!");
            }
            else
            {
                Debug.LogError("TRL-Fixes: Erro ao encontrar EFT.Grenade.Explosion");
            }
        }

        public static void Postfix(IExplosiveItem grenadeItem, Vector3 grenadePosition)
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
                if (!player.IsAI || player.HealthController == null || !player.HealthController.IsAlive)
                    continue;

                BotOwner botOwner = player.AIData?.BotOwner;
                if (botOwner == null || botOwner.BotState != EBotState.Active)
                    continue;

                // Checa distância (20 metros)
                float distance = Vector3.Distance(grenadePosition, player.PlayerBones.Head.position);
                if (distance > 20f)
                    continue;

                Vector3 directionToExplosion = (grenadePosition - player.PlayerBones.Head.position).normalized;
                float dot = Vector3.Dot(directionToExplosion, player.LookDirection);

                // Tolerância maior para o ângulo: > -0.3f (visão periférica / ligeiramente atrás do ombro)
                if (dot > -0.3f)
                {
                    // Raycast ignorando obstáculos pequenos, testando apenas paredes e terreno (HighPolyWithTerrainMaskAI)
                    if (!Physics.Linecast(player.PlayerBones.Head.position, grenadePosition, LayerMaskClass.HighPolyWithTerrainMaskAI))
                    {
                        if (!botOwner.FlashGrenade.IsFlashed)
                        {
                            // O cálculo do tempo de cegueira usa o tempo original da granada (Blindness.z)
                            float time = grenadeItem.Blindness.z * botOwner.Settings.FileSettings.Grenade.FLASH_GRENADE_TIME_COEF;
                            
                            // Força a IA a reconhecer o efeito de cegueira de forma absoluta
                            botOwner.FlashGrenade.AddBlindEffect(time, grenadePosition);
                        }
                    }
                }
            }
        }
    }
}
