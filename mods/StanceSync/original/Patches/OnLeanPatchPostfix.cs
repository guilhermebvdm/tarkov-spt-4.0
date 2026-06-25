using Comfort.Common;
using EFT;
using SPT.Reflection.Patching;
using System.Reflection;
using static EFT.Player;

namespace hazelify.StanceSync.Patches;

public class OnLeanPatchPostfix : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(Player).GetMethod(nameof(Player.method_3));
    }

    [PatchPostfix]
    private static void Postfix(Player __instance, float dir)
    {
        // if core sync option is disabled, or if the instance is not our player, or if "MovementContext" does not exist, skip
        if (!Plugin.enableSync.Value || !__instance.IsYourPlayer || __instance.MovementContext == null)
            return;

        var _firearmController = __instance.HandsController as Player.FirearmController;

        // if leaning left and direction is more than 0, trigger shoulder swapping
        if ((__instance.MovementContext.LeftStanceEnabled && dir > 0f) || (!__instance.MovementContext.LeftStanceEnabled && dir < 0f))
        {
            var pwa = Singleton<GameWorld>.Instance.MainPlayer.ProceduralWeaponAnimation;
            if (pwa == null) return;

            var currentOptic = pwa.CurrentScope;
            bool isAiming = (_firearmController.IsAiming && __instance.PointOfView == EPointOfView.FirstPerson);

            if (isAiming && Plugin.disableSyncWhileADS.Value)
            {
                if (!Plugin.disableSyncWithOptic.Value || currentOptic.IsOptic)
                {
                    return;
                }
            }

            _firearmController.ChangeLeftStance();
        }
    }
}