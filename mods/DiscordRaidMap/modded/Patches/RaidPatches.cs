using System;
using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace DiscordRaidMap.Patches
{
    internal class GameStartedPatch : ModulePatch
    {
        internal static event Action<GameWorld> OnGameStarted;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnGameStarted));
        }

        [PatchPostfix]
        private static void PatchPostfix(GameWorld __instance)
        {
            OnGameStarted?.Invoke(__instance);
        }
    }

    internal class GameWorldOnDestroyPatch : ModulePatch
    {
        internal static event Action OnRaidEnd;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GameWorld), nameof(GameWorld.OnDestroy));
        }

        [PatchPrefix]
        private static void PatchPrefix()
        {
            OnRaidEnd?.Invoke();
        }
    }
}
