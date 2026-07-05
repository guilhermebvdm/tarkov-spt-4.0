using System.Reflection;
using EFT;
using HarmonyLib;
using SAIN.Components;
using SAIN.Components.Helpers;
using SPT.Reflection.Patching;

namespace SAIN.Patches.BotHearing;

/// <summary>
/// This patch hooks into the base sound player and passes certain events on to SAIN to use as hearing for bots
/// </summary>
public class SoundClipNameCheckerPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(BaseSoundPlayer), nameof(BaseSoundPlayer.SoundEventHandler));
    }

    [PatchPrefix]
    public static void PatchPrefix(string soundName, BaseSoundPlayer.IObserverToPlayerBridge ___playersBridge)
    {
        if (BotManagerComponent.Instance != null)
        {
            Player player = ___playersBridge.iPlayer as Player;
            SAINSoundTypeHandler.AISoundFileChecker(soundName, player);
        }
    }
}
