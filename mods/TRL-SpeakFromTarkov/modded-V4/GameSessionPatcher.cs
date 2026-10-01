using BepInEx.Logging;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

namespace TRL_SpeakFromTarkov
{
    internal static class GameSessionPatcher
    {
        private static ManualLogSource Log => VoIPPlugin.Log;

        public static void Init()
        {
            Log.LogInfo("[SFT] Aplicando patches de sessao...");
            new PlayerOnDeadPatch().Enable();
            new GameWorldDisposePatch().Enable();
            Log.LogInfo("[SFT] Patches aplicados.");
        }

        // AUD-03-11: PlayerInitPatch removido. EFT.Player.Init é `async Task`
        // (Assembly-CSharp/EFT/Player.cs:28592) e faz `await JobScheduler.Yield()` ANTES de
        // `Profile = profile;` — um Postfix Harmony num método async dispara nesse primeiro yield,
        // não ao final da inicialização lógica. Sem bug ativo até aqui (o guard só lia
        // IsYourPlayer), mas frágil pra qualquer lógica futura. Substituído por um poll seguro em
        // VoipController.Update() (guardado por _voipCaptureStarted), usando
        // GameWorld.MainPlayer — só preenchido em RegisterPlayer (GameWorld.cs:2277) quando
        // IsYourPlayer, ou seja, depois do Init estar de fato completo.

        internal class PlayerOnDeadPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(EFT.Player), nameof(EFT.Player.OnDead));
            }

            [PatchPostfix]
            static void Postfix(EFT.Player __instance)
            {
                if (__instance.IsYourPlayer && Core.VoipController.Instance != null)
                {
                    Core.VoipController.Instance.SetPlayerStatus(true);
                }
            }
        }

        internal class GameWorldDisposePatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.Dispose));
            }

            [PatchPrefix]
            static void Prefix()
            {
                if (Core.VoipController.Instance != null)
                {
                    Core.VoipController.Instance.SetGameStateChannel(false);
                    Core.VoipController.Instance.ResetVoipCaptureStartedFlag(); // AUD-03-11
                }
            }
        }

        // --- Patches para silenciar o Fika VOIP e evitar travamento no carregamento da raid ---
        // AUD-03-15: FikaVoipSendPatch/FikaVoipReceivePatch removidos — miravam
        // FikaVOIPClient.SendVoiceData/NetworkReceivedPacket, mas FikaClientInitializeVoipPatch/
        // FikaServerInitializeVoipPatch (abaixo) já bloqueiam InitializeVOIP() inteiro quando
        // EnableMod=true, e é lá que FikaVOIPClient é instanciado — os dois patches removidos
        // nunca eram exercitados em nenhum dos dois estados do mod (código morto confirmado).

        internal class FikaClientInitializeVoipPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(Fika.Core.Networking.FikaClient), "InitializeVOIP");
            }

            [PatchPrefix]
            static bool Prefix(ref System.Threading.Tasks.Task __result)
            {
                if (VoIPPlugin.EnableMod != null && VoIPPlugin.EnableMod.Value)
                {
                    __result = System.Threading.Tasks.Task.CompletedTask;
                    return false; // Ignora o carregamento lento do Dissonance Scene e o loop de espera do VOIPClient
                }
                return true;
            }
        }

        internal class FikaServerInitializeVoipPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(Fika.Core.Networking.FikaServer), "InitializeVOIP");
            }

            [PatchPrefix]
            static bool Prefix(ref System.Threading.Tasks.Task __result)
            {
                if (VoIPPlugin.EnableMod != null && VoIPPlugin.EnableMod.Value)
                {
                    __result = System.Threading.Tasks.Task.CompletedTask;
                    return false; // Ignora o carregamento lento do Dissonance Scene e o loop de espera do VOIPServer
                }
                return true;
            }
        }

        internal class FikaFixVoipAudioDevicePatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                var type = AccessTools.TypeByName("Fika.Core.Main.GameMode.BaseGameController");
                return AccessTools.Method(type, "FixVOIPAudioDevice");
            }

            [PatchPrefix]
            static bool Prefix(ref System.Collections.IEnumerator __result)
            {
                if (VoIPPlugin.EnableMod != null && VoIPPlugin.EnableMod.Value)
                {
                    __result = EmptyEnumerator();
                    return false; // Silencia FixVOIPAudioDevice do FIKA que acessava DissonanceComms.Instance causando NRE
                }
                return true;
            }

            private static System.Collections.IEnumerator EmptyEnumerator()
            {
                yield break;
            }
        }

        internal class FikaObservedPlayerInitVoipPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                var type = AccessTools.TypeByName("Fika.Core.Main.Players.ObservedPlayer");
                return AccessTools.Method(type, "InitVoip");
            }

            [PatchPrefix]
            static bool Prefix()
            {
                if (VoIPPlugin.EnableMod != null && VoIPPlugin.EnableMod.Value)
                {
                    return false; // Silencia o InitVoip do ObservedPlayer que chamava Dissonance
                }
                return true;
            }
        }

        internal class FikaPlayerInitVoipPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                var type = AccessTools.TypeByName("Fika.Core.Main.Players.FikaPlayer");
                return AccessTools.Method(type, "InitVoip");
            }

            [PatchPrefix]
            static bool Prefix()
            {
                if (VoIPPlugin.EnableMod != null && VoIPPlugin.EnableMod.Value)
                {
                    return false; // Silencia o InitVoip do FikaPlayer
                }
                return true;
            }
        }

        internal class BoundSlotViewRefreshSelectViewPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                var type = AccessTools.TypeByName("EFT.UI.DragAndDrop.BoundSlotView");
                return AccessTools.Method(type, "RefreshSelectView");
            }

            [PatchFinalizer]
            static System.Exception? Finalizer(System.Exception? __exception)
            {
                // AUD-03-18: antes suprimia QUALQUER exceção incondicionalmente — agora só a
                // NullReferenceException que motivou o patch (spawn de arma no coop); qualquer
                // outra exceção volta a subir normalmente, em vez de ser silenciada pra sempre
                // nesse método usado por toda a UI de drag-and-drop do jogo.
                if (__exception is System.NullReferenceException)
                {
                    // Absorve NRE de BoundSlotView durante o spawn da raid no coop, permitindo que o ciclo de equipar a arma termine com sucesso
                    return null;
                }
                return __exception;
            }
        }
    }
}