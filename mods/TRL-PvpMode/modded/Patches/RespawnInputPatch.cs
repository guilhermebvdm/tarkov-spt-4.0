using System;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.Communications;
using EFT.UI;
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
        private static bool _announcedDowned;

        /// <summary>
        /// Progresso do segurar, de 0 a 1. Lido pelo indicador de tela para desenhar a barra —
        /// sem realimentacao visual o jogador nao tem como saber se a tecla esta chegando.
        /// </summary>
        public static float HoldProgress { get; private set; }

        public static void Reset()
        {
            _holdElapsed = 0f;
            _wasHolding = false;
            HoldProgress = 0f;
            _announcedDowned = false;
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
                    if (_wasHolding || _announcedDowned) Reset();
                    return;
                }

                // Uma linha por queda: dá para conferir no log qual tecla o mod está esperando,
                // sem depender da memória do jogador nem de adivinhação no meio da raid.
                if (!_announcedDowned)
                {
                    _announcedDowned = true;
                    Plugin.Log.LogInfo(
                        $"[TRL-PvpMode] Caido. Segure [{Settings.RESPAWN_KEY.Value}] por " +
                        $"{Settings.RESPAWN_HOLD_TIME.Value}s para renascer.");
                }

                // Sem as guardas de contexto, digitar no chat ou no console com a tecla rebindada
                // para uma letra gastaria uma vida. O próprio Fika guarda os mesmos três estados
                // antes de ler teclado (ClientPacketSender.cs:44-47) — code review 002, D-07.
                // Input.GetKey primeiro: e a checagem mais barata e falha na maioria dos quadros.
                var holding = Settings.RESPAWN_KEY.Value.IsPressed() && !IsTypingSomewhere(__instance);

                if (!holding)
                {
                    // Soltar antes do fim cancela sem gastar nada.
                    if (_wasHolding) Reset();
                    return;
                }

                _wasHolding = true;
                _holdElapsed += deltaTime;

                var required = Mathf.Max(0.1f, Settings.RESPAWN_HOLD_TIME.Value);
                HoldProgress = Mathf.Clamp01(_holdElapsed / required);

                if (_holdElapsed < required) return;

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

        /// <summary>
        /// O jogador está escrevendo em algum lugar? Mesmos três estados que o Fika consulta antes
        /// de ler teclado. O chat é resolvido por reflexão porque o tipo é interno do Fika.
        /// </summary>
        private static bool IsTypingSomewhere(Player player)
        {
            try
            {
                if (player.IsInventoryOpened) return true;

                var preloader = MonoBehaviourSingleton<PreloaderUI>.Instance;
                if (preloader != null && preloader.Console != null && preloader.Console.IsConsoleVisible)
                    return true;

                return FikaBridge.IsChatActive();
            }
            catch
            {
                // Em dúvida, deixa passar: bloquear o renascimento é pior que o risco de digitar.
                return false;
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
