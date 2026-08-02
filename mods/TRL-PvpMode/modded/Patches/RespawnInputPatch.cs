using System;
using System.Reflection;
using EFT;
using EFT.Communications;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace TarkovRedLine.PvpMode.Patches
{
    /// <summary>
    /// Lê a tecla de renascer enquanto o jogador está caído e conta o tempo de pressão.
    ///
    /// Por que não usar a leitura do Fika: o componente `Bleedout` sai de `Update` antes de
    /// `CheckForKeys()` quando o prazo é zero (Bleedout.cs:79-86), então pendurar a tecla nele
    /// quebraria justamente a configuração "sem limite" (item 001, R-05).
    ///
    /// ⚠️ `UpdateTick` roda para TODO jogador e bot do mapa. O filtro `IsYourPlayer` é a primeira
    /// linha e tudo o mais fica atrás dele (AP-02) — este é o único ponto do mod em caminho quente.
    /// </summary>
    public class RespawnInputPatch : ModulePatch
    {
        private static float _holdElapsed;
        private static bool _wasHolding;

        public static void Reset()
        {
            _holdElapsed = 0f;
            _wasHolding = false;
        }

        protected override MethodBase GetTargetMethod()
            // ref: Assembly-CSharp/EFT/Player.cs — UpdateTick é o tique por quadro do Player
            => AccessTools.Method(typeof(Player), nameof(Player.UpdateTick));

        [PatchPostfix]
        private static void Postfix(Player __instance)
        {
            try
            {
                if (!__instance.IsYourPlayer) return;
                if (!RaidState.IsActive) return;

                var deltaTime = Time.deltaTime;

                // A proteção corre mesmo com o jogador de pé — é justamente depois do respawn.
                RespawnService.Tick(__instance, deltaTime);

                if (__instance.ActiveHealthController is not Fika.Core.Main.ClientClasses.ClientHealthController { Downed: true })
                {
                    // De pé: qualquer pressão acumulada perde a validade.
                    if (_wasHolding) Reset();
                    return;
                }

                var holding = Input.GetKey(Settings.RESPAWN_KEY.Value);

                if (!holding)
                {
                    // Soltar antes do fim cancela sem gastar nada.
                    if (_wasHolding) Reset();
                    return;
                }

                _wasHolding = true;
                _holdElapsed += deltaTime;

                if (_holdElapsed < Mathf.Max(0.1f, Settings.RESPAWN_HOLD_TIME.Value)) return;

                Reset();

                if (!RaidState.HasLifeAvailable)
                {
                    ShowNoLivesLeft();
                    return;
                }

                RespawnService.TryRespawn(__instance);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[TRL-PvpMode] RespawnInputPatch: {ex}");
                Reset();
            }
        }

        private static float _nextNoLivesNotice;

        /// <summary>Avisa no máximo uma vez a cada 3s — o jogador pode ficar segurando a tecla.</summary>
        private static void ShowNoLivesLeft()
        {
            if (Time.time < _nextNoLivesNotice) return;
            _nextNoLivesNotice = Time.time + 3f;

            try
            {
                NotificationManagerClass.DisplayMessageNotification(
                    "Sem vidas restantes — esta morte e definitiva.",
                    ENotificationDurationType.Default,
                    ENotificationIconType.Alert,
                    Color.red);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError($"[TRL-PvpMode] ShowNoLivesLeft: {ex.Message}");
            }
        }
    }
}
