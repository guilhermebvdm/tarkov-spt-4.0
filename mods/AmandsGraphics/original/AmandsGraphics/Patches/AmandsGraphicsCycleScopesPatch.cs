using System.Reflection;
using AmandsGraphics.Enums;
using EFT.Animations;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;

namespace AmandsGraphics.Patches;

public sealed class AmandsGraphicsCycleScopesPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(ProceduralWeaponAnimation).GetMethod(nameof(ProceduralWeaponAnimation.CycleScopes));
    }

    [PatchPostfix]
    public static void PatchPostFix(ref ProceduralWeaponAnimation __instance)
    {
        var currentScope = __instance.CurrentScope;

        if (AmandsGraphicsClass.Player != null && AmandsGraphicsClass.Player.ProceduralWeaponAnimation == __instance)
        {
            if (__instance.CurrentScope != null)
            {
                if (currentScope.ScopePrefabCache != null)
                {
                    SightComponent Mod = currentScope.Mod;

                    if (Mod != null && Mod.SelectedScopeIndex == 0)
                    {
                        AmandsGraphicsClass.aimingMode = EAimingMode.Sight;
                    }
                    else
                    {
                        AmandsGraphicsClass.aimingMode = EAimingMode.IronSight;
                    }
                }
                else
                {
                    AmandsGraphicsClass.aimingMode = EAimingMode.IronSight;
                }
            }
        }
    }
}
