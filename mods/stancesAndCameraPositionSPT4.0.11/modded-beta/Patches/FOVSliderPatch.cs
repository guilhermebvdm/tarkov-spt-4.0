using SPT.Reflection.Patching;
using EFT.UI;
using EFT.UI.Settings;
using System.Reflection;
using HarmonyLib;

namespace CameraRotationMod.Patches
{
    /// <summary>
    /// Patch to extend the FOV slider range in GameSettingsTab.
    /// Re-binds the FOV NumberSlider with extended min/max values.
    /// </summary>
    public class FOVSliderPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GameSettingsTab), nameof(GameSettingsTab.Show));
        }

        [PatchPostfix]
        private static void PatchPostfix(ref NumberSlider ____fov, GClass1085 ___gclass1085_0)
        {
            if (!Plugin._FOVExpandEnabled.Value)
                return;

            // Re-bind the FOV slider with extended range
            SettingsTab.BindNumberSliderToSetting(
                ____fov, 
                ___gclass1085_0.FieldOfView, 
                Plugin._FOVMinRange.Value, 
                Plugin._FOVMaxRange.Value
            );
        }
    }
}
