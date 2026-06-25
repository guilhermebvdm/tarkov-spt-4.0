using Comfort.Common;
using EFT;
using EFT.Animations;
using EFT.CameraControl;
using SPT.Reflection.Patching;
using System.Reflection;

namespace hazelify.StanceSync.Patches
{
    public class PlayerCameraControllerPatchPrefix : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(PlayerCameraController).GetMethod(nameof(PlayerCameraController.LateUpdate));
        }

        [PatchPrefix]
        public static void Prefix(PlayerCameraController __instance)
        {
            var localPlayer = Singleton<GameWorld>.Instance.MainPlayer;

            // check if camera is null or if healthcontroller is literally not existing
            if (localPlayer.CameraPosition == null || !localPlayer.HealthController.IsAlive)
                return;

            var _firearmController = localPlayer.HandsController as Player.FirearmController;

            // check for the plugin sync options, this feature only works if "Reset Sync" is enabled, but regular Sync is required also
            if (Plugin.enableSync.Value && Plugin.enableResetSync.Value)
            {
                // if the firearmcontroller doesn't exist, well... tough shit, no bueno!
                if (_firearmController != null)
                {
                    // if leaning left but tilt is 0, trigger shoulder swapping back to default (toggle)
                    if (localPlayer.MovementContext.LeftStanceEnabled && localPlayer.MovementContext.Tilt == 0f)
                    {
                        Player mainPlayer = Singleton<GameWorld>.Instance.MainPlayer;
                        var pwa = Singleton<GameWorld>.Instance.MainPlayer.ProceduralWeaponAnimation;
                        if (pwa == null) return;

                        var currentOptic = pwa.CurrentScope;

                        bool isAiming = (_firearmController.IsAiming && mainPlayer.PointOfView == EPointOfView.FirstPerson);

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
        }
    }
}
