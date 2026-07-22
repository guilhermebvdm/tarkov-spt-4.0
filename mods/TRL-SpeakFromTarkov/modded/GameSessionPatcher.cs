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

        // --- Patches para silenciar o Fika VOIP sem quebrar o Dissonance base ---

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

        // --- Patches para detectar o momento seguro de inicialização no menu ---

    }
}