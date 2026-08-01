using SPT.Reflection.Patching;
using EFT;
using HarmonyLib;
using System.Reflection;
using UnityEngine.SceneManagement;
using PlayerLives.Features;
using PlayerLives.Helpers;

namespace PlayerLives.Patches
{
    public class RaidStartPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));
        }


        [PatchPostfix]
        private static void PatchPostfix(GameWorld __instance)
        {
            var gameWorld = __instance;
            if (gameWorld == null || gameWorld.MainPlayer == null || IsInHideout()) return;

            // Drop every per-player entry left over from the last raid.
            RevivalFeatures.ResetState();

            // Raid start, reset number of lives
            Plugin.CurrentLives = Settings.PLAYER_LIVES.Value;
            Plugin.GaveUp = false;
            Plugin.shownDeathNotification = false;
            Plugin.shownFikaReviveNotification = false;

            Plugin.LogSource.LogInfo($"Raid started, setting lives to {Plugin.CurrentLives}");
        }

        private static bool IsInHideout()
        {
            // Check if "bunker_2" is one of the active scene names
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (scene.name == "bunker_2")
                {
                    //EFT.UI.ConsoleScreen.LogError("bunker_2 loaded, not running de-cluttering.");
                    return true;
                }
            }
            //EFT.UI.ConsoleScreen.LogError("bunker_2 not loaded, de-cluttering.");
            return false;
        }
    }
}
