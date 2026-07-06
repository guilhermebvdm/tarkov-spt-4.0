using Comfort.Common;
using EFT;
using HarmonyLib;
using System.Reflection;
using TRLDynamicSpawn.Components;
using SPT.Reflection.Patching;

namespace TRLDynamicSpawn.Patches
{
    public class DynamicSpawnManagerPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));
        }

        [PatchPostfix]
        private static void PatchPostfix(GameWorld __instance)
        {
            // Inject our Maestro into the GameWorld
            var spawnManager = __instance.gameObject.AddComponent<DynamicSpawnManager>();
            
            // Note: In SPT, BotsController is usually attached to GameWorld or accessible via singleton
            // IBotCreator is usually resolved via Dependency Injection or Singletons.
            // For now, passing nulls, you must adapt this to grab the real Fika/SPT singletons.
            spawnManager.Init(__instance, null, null);

            Plugin.LogSource.LogInfo("[TRL-DynamicSpawn] Maestro injected into GameWorld successfully!");
        }
    }
}
