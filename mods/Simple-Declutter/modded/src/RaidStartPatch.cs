using SPT.Reflection.Patching;
using Comfort.Common;
using EFT;
using HarmonyLib;
using System.Reflection;
using EFT.UI;
using UnityEngine.SceneManagement;
using System.IO;

namespace SimpleDeclutter.Patches
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

            Plugin.isOnMap = true;

            Plugin.LogSource.LogInfo($"Plugin run clutter search...");

            // Build declutter list
            StaticManager.BeginCoroutine(Plugin.GetAllGameObjectsInSceneCoroutine());
            StaticManager.BeginCoroutine(Plugin.GetValidDeclutterTargets());

            Plugin.ApplyDeclutter();
            Plugin.ApplyFrameSavers();
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