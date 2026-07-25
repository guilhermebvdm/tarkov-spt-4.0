using BepInEx.Logging;
using EFT;
using EFT.HealthSystem;
using Fika.Core.Coop.Components;
using Fika.Core.Coop.Players;
using HarmonyLib;
using System.Reflection;

namespace SpeakFromTarkov
{
    internal static class GameSessionPatcher
    {
        private static Harmony _harmony;
        private static ManualLogSource Log => VoIPPlugin.Log;

        public static void Init(Harmony harmony)
        {
            _harmony = harmony;
            Log.LogInfo("[SFT] Aplicando patches...");
            _harmony.PatchAll(typeof(PlayerInitPatch));
            _harmony.PatchAll(typeof(CoopPlayerOnDeadPatch));
            _harmony.PatchAll(typeof(HealthControllerDiePatch));
            _harmony.PatchAll(typeof(GameWorldDisposePatch));
            Log.LogInfo("[SFT] Patches aplicados.");
        }

        [HarmonyPatch(typeof(EFT.Player), "Init")]
        internal class PlayerInitPatch
        {
            [HarmonyPostfix]
            static void Postfix(EFT.Player __instance)
            {
                if (__instance.IsYourPlayer)
                {
                    VoiceChatManager.Instance.SetGameStateChannel(true);
                }
            }
        }

        [HarmonyPatch]
        internal class CoopPlayerOnDeadPatch
        {
            static MethodBase TargetMethod()
            {
                return typeof(CoopPlayer).GetMethod("OnDead", BindingFlags.Instance | BindingFlags.Public);
            }

            [HarmonyPostfix]
            static void Postfix(CoopPlayer __instance)
            {
                if (__instance.IsYourPlayer)
                {
                    VoiceChatManager.Instance.SetPlayerStatus(true);
                }
            }
        }

        [HarmonyPatch(typeof(ActiveHealthController), "Kill")]
        internal class HealthControllerDiePatch
        {
            [HarmonyPostfix]
            static void Postfix()
            {
                VoiceChatManager.Instance.SetPlayerStatus(true);
            }
        }

        [HarmonyPatch(typeof(GameWorld), "Dispose")]
        internal class GameWorldDisposePatch
        {
            [HarmonyPrefix]
            static void Prefix()
            {
                VoiceChatManager.Instance.SetGameStateChannel(false);
                NetworkManager.Instance.SetSessionActive(false);
            }
        }
    }
}