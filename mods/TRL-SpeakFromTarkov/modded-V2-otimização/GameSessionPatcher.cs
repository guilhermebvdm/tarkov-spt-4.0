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
            new PlayerInitPatch().Enable();
            new PlayerOnDeadPatch().Enable();
            new GameWorldDisposePatch().Enable();
            Log.LogInfo("[SFT] Patches aplicados.");
        }

        internal class PlayerInitPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(EFT.Player), nameof(EFT.Player.Init));
            }

            [PatchPostfix]
            static void Postfix(EFT.Player __instance)
            {
                if (__instance.IsYourPlayer && Core.VoipController.Instance != null)
                {
                    Core.VoipController.Instance.SetGameStateChannel(true);
                    Core.VoipController.Instance.StartVoipCapture();
                }
            }
        }

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
                }
            }
        }

        // --- Patches para silenciar o Fika VOIP e evitar travamento no carregamento da raid ---

        internal class FikaVoipSendPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(Fika.Core.Networking.VOIP.FikaVOIPClient), "SendVoiceData");
            }

            [PatchPrefix]
            static bool Prefix()
            {
                if (VoIPPlugin.EnableMod != null && !VoIPPlugin.EnableMod.Value) return true; // Deixa o FIKA rodar
                return false;
            }
        }

        internal class FikaVoipReceivePatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(Fika.Core.Networking.VOIP.FikaVOIPClient), "NetworkReceivedPacket");
            }

            [PatchPrefix]
            static bool Prefix()
            {
                if (VoIPPlugin.EnableMod != null && !VoIPPlugin.EnableMod.Value) return true; // Deixa o FIKA rodar
                return false;
            }
        }

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

        internal class FikaVoipControllerUpdatePatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                var type = AccessTools.TypeByName("Fika.Core.Networking.VOIP.FikaVOIPController");
                return AccessTools.Method(type, "Update");
            }

            [PatchPrefix]
            static bool Prefix()
            {
                if (VoIPPlugin.EnableMod != null && VoIPPlugin.EnableMod.Value)
                {
                    return false; // Silencia o tick do VOIP do FIKA, evitando NRE em MicrophoneFailState
                }
                return true;
            }
        }
    }
}