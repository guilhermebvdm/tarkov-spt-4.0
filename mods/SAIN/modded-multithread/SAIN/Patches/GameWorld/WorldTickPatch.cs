using System.Reflection;
using HarmonyLib;
using SAIN.Components;
using SPT.Reflection.Patching;

namespace SAIN.Patches.GameWorld;

/// <summary>
/// This patch makes sure that whenever a new world tick happens, it is also sent to SAIN's <see cref="GameWorldComponent">GameWorldComponent</see>
/// </summary>
public class WorldTickPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return AccessTools.Method(typeof(EFT.GameWorld), nameof(EFT.GameWorld.DoWorldTick));
    }

    [PatchPostfix]
    public static void Patch(float dt)
    {
        if (ModDetection.ProjectFikaLoaded && ModDetection.FikaInterop.IsClient())
        {
            return;
        }

        var gameWorldComponent = GameWorldComponent.Instance;

        if (gameWorldComponent != null)
        {
            gameWorldComponent.WorldTick(dt);
        }
    }
}
