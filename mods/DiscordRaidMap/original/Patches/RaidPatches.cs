using System;
using System.Collections.Generic;
using System.Reflection;
using EFT;
using EFT.SynchronizableObjects;
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
            AirdropLandedPatch.Airdrops.Clear();
        }
    }

    internal class PlayerOnDeadPatch : ModulePatch
    {
        internal static event Action<Player> OnDead;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player.OnDead));
        }

        [PatchPostfix]
        private static void PatchPostfix(Player __instance)
        {
            if (!DiscordRaidMap.RaidMap.HostCheck.CanBroadcast())
            {
                return;
            }

            OnDead?.Invoke(__instance);
        }
    }

    internal class AirdropLandedPatch : ModulePatch
    {
        internal static event Action<AirdropSynchronizableObject> OnAirdropLanded;
        internal static readonly List<AirdropSynchronizableObject> Airdrops = [];

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(AirdropLogicClass), nameof(AirdropLogicClass.method_3));
        }

        [PatchPostfix]
        private static void PatchPostfix(AirdropLogicClass __instance)
        {
            if (!DiscordRaidMap.RaidMap.HostCheck.CanBroadcast())
            {
                return;
            }

            var airdrop = __instance?.AirdropSynchronizableObject_0;
            if (airdrop == null || Airdrops.Contains(airdrop))
            {
                return;
            }

            Airdrops.Add(airdrop);
            OnAirdropLanded?.Invoke(airdrop);
        }
    }
}
