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
            // Resolve real singletons for bot spawning
            if (TRLDynamicSpawn.Helpers.FikaHelper.IsClient())
            {
                Plugin.LogSource.LogInfo("[TRL-DynamicSpawn] Guest client detected in FIKA coop. Skipping DynamicSpawnManager injection.");
                return;
            }

            if (!Singleton<IBotGame>.Instantiated)

            {
                Plugin.LogSource.LogError("[TRL-DynamicSpawn] Cannot inject DynamicSpawnManager: IBotGame is not instantiated yet.");
                return;
            }

            var botGame = Singleton<IBotGame>.Instance;
            var botsController = botGame?.BotsController;
            if (botsController == null)
            {
                Plugin.LogSource.LogError("[TRL-DynamicSpawn] Cannot inject DynamicSpawnManager: BotsController is null.");
                return;
            }

            var botCreator = botsController.BotSpawner?.BotCreator;
            if (botCreator == null)
            {
                Plugin.LogSource.LogError("[TRL-DynamicSpawn] Cannot inject DynamicSpawnManager: BotCreator is null.");
                return;
            }

            // Inject our Maestro into the GameWorld
            var spawnManager = __instance.gameObject.AddComponent<DynamicSpawnManager>();
            spawnManager.Init(__instance, botCreator, botsController);

            Plugin.LogSource.LogInfo("[TRL-DynamicSpawn] Maestro injected into GameWorld successfully!");
        }
    }
}
