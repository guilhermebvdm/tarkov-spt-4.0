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
            new RequestHandlerGetJsonPatch().Enable();
            new RequestHandlerPostJsonPatch().Enable();
            new RequestHandlerPutJsonPatch().Enable();
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
                // Retorna false para impedir que o VOIP do Fika envie dados,
                // já que usaremos o nosso NetworkManager com Opus.
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
                // Impede o recebimento de dados do VOIP nativo do Fika
                return false;
            }
        }

        // --- Patches para detectar o momento seguro de inicialização no menu ---

        internal class RequestHandlerGetJsonPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(SPT.Common.Http.RequestHandler), "GetJson");
            }

            [PatchPostfix]
            static void Postfix(string path)
            {
                if (path != null && path.IndexOf("/hip/load", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (Core.VoipController.Instance != null)
                    {
                        Core.VoipController.Instance.OnHipLoadCompleted();
                    }
                }
            }
        }

        internal class RequestHandlerPostJsonPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(SPT.Common.Http.RequestHandler), "PostJson");
            }

            [PatchPostfix]
            static void Postfix(string path)
            {
                if (path != null && path.IndexOf("/hip/load", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (Core.VoipController.Instance != null)
                    {
                        Core.VoipController.Instance.OnHipLoadCompleted();
                    }
                }
            }
        }

        internal class RequestHandlerPutJsonPatch : ModulePatch
        {
            protected override MethodBase GetTargetMethod()
            {
                return AccessTools.Method(typeof(SPT.Common.Http.RequestHandler), "PutJson");
            }

            [PatchPostfix]
            static void Postfix(string path)
            {
                if (path != null && path.IndexOf("/hip/load", System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    if (Core.VoipController.Instance != null)
                    {
                        Core.VoipController.Instance.OnHipLoadCompleted();
                    }
                }
            }
        }
    }
}