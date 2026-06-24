using Comfort.Common;
using EFT;
using EFT.Console.Core;
using EFT.InventoryLogic;
using System.Collections.Generic;
using EFT.HealthSystem;
using UnityEngine;
using UnderFire;

namespace UnderFire
{
    public class DebugCommands
    {
        [ConsoleCommand("apply_effects")]
        public static void ApplyEffects([ConsoleArgument(0, "Duration")] int delay, [ConsoleArgument(0, "Delay")] int duration)
        {
            if (Plugin.activeHealthController == null)
                GetBuffs();
            Plugin.ApplyAdrenalineEffects(Plugin.activeHealthController);
            Plugin.isAdrenalineActive = true;
        }

        [ConsoleCommand("reset_flag")]
        public static void ResetFlag()
        {
            Plugin.isAdrenalineActive = false;
        }

        [ConsoleCommand("get_buffs")]
        public static void GetBuffs()
        {
            if (!Singleton<GameWorld>.Instantiated) return;
            if (Plugin.gameWorld == null) Plugin.gameWorld = Singleton<GameWorld>.Instance;
            if (Plugin.player == null) Plugin.player = Plugin.gameWorld.MainPlayer;
            if (Plugin.activeHealthController == null) Plugin.activeHealthController = Plugin.player.ActiveHealthController;
            Plugin.logSource.LogWarning($"Getting Buffs...");
            string[] buffNames = Plugin.activeHealthController.ActiveBuffsNames();
            foreach (string buffName in buffNames)
            {
                Plugin.logSource.LogWarning($"Buff name is: {buffName}");
            }
        }
    }
}