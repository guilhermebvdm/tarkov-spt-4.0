using System.Reflection;
using EFT;
using EFT.InventoryLogic;
using MagCheckInterrupt.Components;
using MagCheckInterrupt.Utils;
using SPT.Reflection.Patching;
using static EFT.Player.FirearmController;

namespace MagCheckInterrupt.Patches;

/// <summary>
/// This patch replaces mag check <see cref="UtilityOperation"/> with <see cref="MagCheckReloadOperation"/>.
/// </summary>
public class RunUtilityOpPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(IdlingOperation).GetMethod(nameof(IdlingOperation.RunUtilityOperation));
    }

    [PatchPrefix]
    public static bool Prefix(IdlingOperation __instance, UtilityOperation.EUtilityType utilityType)
    {
        if (utilityType != UtilityOperation.EUtilityType.CheckMagazine) return true;
        if (__instance.Player_0.IsAI) return true;

        if (!WeaponUsesExternalMag(__instance.Weapon_0))
        {
            // Show ammo details since we skip it through `AmmoDetailsPatch`
            if (__instance.Player_0.FirstPersonPointOfView)
            {
                AmmoDetailsPatch.ShowLastAmmoDetail();
            }

            return true;
        }

        LoggerUtil.Debug("RunUtilityOpPatch::Prefix Initiate MagCheckReloadOperation");

        __instance.Player_0.BodyAnimatorCommon.SetFloat(PlayerAnimator.RELOAD_FLOAT_PARAM_HASH, 1f);
        __instance.State = EOperationState.Finished;
        __instance.FirearmController_0.InitiateOperation<MagCheckReloadOperation>().Start(utilityType);
        return false;
    }

    private static bool WeaponUsesExternalMag(Weapon weapon)
    {
        return weapon.ReloadMode == Weapon.EReloadMode.ExternalMagazine
               || weapon.ReloadMode == Weapon.EReloadMode.ExternalMagazineWithInternalReloadSupport;
    }
}
