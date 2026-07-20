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
            // Tenta resolver singletons de IA do servidor
            var botGame = Singleton<IBotGame>.Instantiated ? Singleton<IBotGame>.Instance : null;
            var botsController = botGame?.BotsController;
            var botCreator = botsController?.BotSpawner?.BotCreator;

            // Injeta o spawn manager de qualquer forma para permitir o HUD em clientes convidados.
            // Se botsController for nulo (cliente Fika convidado), o manager ativará apenas a UI visual.
            var spawnManager = __instance.gameObject.AddComponent<DynamicSpawnManager>();
            spawnManager.Init(__instance, botCreator, botsController);

            Plugin.LogSource.LogInfo("[TRL-DynamicSpawn] Maestro injected into GameWorld successfully!");
        }
    }
}
