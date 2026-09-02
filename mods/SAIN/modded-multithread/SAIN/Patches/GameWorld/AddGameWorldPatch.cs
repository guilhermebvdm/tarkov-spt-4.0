using System;
using System.Reflection;
using EFT;
using HarmonyLib;
using SAIN.Components;
using SPT.Reflection.Patching;
using UnityEngine;

namespace SAIN.Patches.GameWorld;

/// <summary>
/// This patch makes sure that whenever a new game is ran, various required components from SAIN are also initialized
/// </summary>
public class AddGameWorldPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(GameWorldUnityTickListener), nameof(GameWorldUnityTickListener.Create));
    }

    [PatchPostfix]
    public static void PatchPostfix(GameObject gameObject, EFT.GameWorld gameWorld)
    {
        if (gameWorld is HideoutGameWorld)
        {
            return;
        }

        if (ModDetection.ProjectFikaLoaded && ModDetection.FikaInterop.IsClient())
        {
            Logger.LogInfo("Skipping SAIN gameworld initialization - player is a fika client");
            return;
        }

        try
        {
            if (GameWorldComponent.Instance != null)
            {
                Logger.LogWarning($"Old SAIN Gameworld is not null! Destroying...");
                GameWorldComponent.Instance.DestroyComponent();
            }
            GameWorldComponent gameWorldComponent = gameWorld.gameObject.AddComponent<GameWorldComponent>();
            BotManagerComponent botController = gameWorld.gameObject.AddComponent<BotManagerComponent>();
            gameWorldComponent.Init(gameWorld, botController);
            botController.Activate(gameWorldComponent);
        }
        catch (Exception ex)
        {
            Logger.LogError($" SAIN Init Gameworld Error: {ex}");
        }
    }
}
