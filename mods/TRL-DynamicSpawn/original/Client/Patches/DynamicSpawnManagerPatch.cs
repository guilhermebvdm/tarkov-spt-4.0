using Comfort.Common;
using EFT;
using HarmonyLib;
using System.Collections;
using System.Reflection;
using TRLDynamicSpawn.Components;
using SPT.Reflection.Patching;
using UnityEngine;

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
            if (__instance == null) return;

            // Resolve real singletons for bot spawning
            if (TRLDynamicSpawn.Helpers.FikaHelper.IsClient())
            {
                Plugin.LogSource.LogInfo("[TRL-DynamicSpawn] Guest client detected in FIKA coop. Skipping DynamicSpawnManager injection.");
                return;
            }

            // Evitar injeção duplicada se o componente já existir no GameWorld
            if (__instance.GetComponent<DynamicSpawnManager>() != null)
            {
                return;
            }

            if (TryInjectImmediate(__instance))
            {
                return;
            }

            // Se o IBotGame/BotsController ainda não foi instanciado, inicia Coroutine de aguardo com timeout
            Plugin.LogSource.LogInfo("[TRL-DynamicSpawn] IBotGame/BotsController ainda não instanciado. Aguardando inicialização assíncrona...");
            __instance.StartCoroutine(WaitForBotGameAndInjectCoroutine(__instance));
        }

        private static bool TryInjectImmediate(GameWorld gameWorld)
        {
            if (!Singleton<IBotGame>.Instantiated) return false;

            var botGame = Singleton<IBotGame>.Instance;
            var botsController = botGame?.BotsController;
            if (botsController == null) return false;

            var botCreator = botsController.BotSpawner?.BotCreator;
            if (botCreator == null) return false;

            if (gameWorld.GetComponent<DynamicSpawnManager>() != null) return true;

            var spawnManager = gameWorld.gameObject.AddComponent<DynamicSpawnManager>();
            spawnManager.Init(gameWorld, botCreator, botsController);

            Plugin.LogSource.LogInfo("[TRL-DynamicSpawn] Maestro injected into GameWorld successfully!");
            return true;
        }

        private static IEnumerator WaitForBotGameAndInjectCoroutine(GameWorld gameWorld)
        {
            float timeout = 30f;
            float elapsedTime = 0f;

            while (elapsedTime < timeout)
            {
                if (gameWorld == null) yield break;

                if (TryInjectImmediate(gameWorld))
                {
                    yield break;
                }

                yield return new WaitForSeconds(0.5f);
                elapsedTime += 0.5f;
            }

            Plugin.LogSource.LogError("[TRL-DynamicSpawn] Timeout (30s) aguardando IBotGame/BotsController. Falha ao injetar DynamicSpawnManager.");
        }
    }
}
