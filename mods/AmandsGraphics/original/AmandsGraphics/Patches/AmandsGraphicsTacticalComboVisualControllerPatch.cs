using System.Reflection;
using AmandsGraphics.Enums;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace AmandsGraphics.Patches;

public sealed class AmandsGraphicsTacticalComboVisualControllerPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(TacticalComboVisualController).GetMethod(nameof(TacticalComboVisualController.UpdateBeams));
    }

    [PatchPostfix]
    private static void PatchPostFix(ref TacticalComboVisualController __instance)
    {
        if (AmandsGraphicsPlugin.Flashlight.Value == EEnabledFeature.On && AmandsGraphicsClass.Player != null && Vector3.Distance(__instance.transform.position, AmandsGraphicsClass.Player.Position) < 5f && AmandsGraphicsClass.Player.HandsController != null && __instance.transform.IsChildOf(AmandsGraphicsClass.Player.HandsController.WeaponRoot))
        {
            foreach (Light light in Traverse.Create(__instance).Field("light_0").GetValue<Light[]>())
            {
                if (!AmandsGraphicsClass.registeredLights.ContainsKey(light))
                {
                    AmandsGraphicsClass.registeredLights.Add(light, light.range);
                }

                if (AmandsGraphicsPlugin.AmandsGraphicsClass.GraphicsMode)
                {
                    light.range = AmandsGraphicsClass.registeredLights[light] * AmandsGraphicsPlugin.FlashlightRange.Value;
                }

                VolumetricLight volumetricLight = light.GetComponent<VolumetricLight>();
                if (volumetricLight != null)
                {
                    if (!AmandsGraphicsClass.registeredVolumetricLights.ContainsKey(volumetricLight))
                    {
                        AmandsGraphicsClass.registeredVolumetricLights.Add(volumetricLight, volumetricLight.ExtinctionCoef);
                    }
                    if (AmandsGraphicsPlugin.AmandsGraphicsClass.GraphicsMode)
                    {
                        volumetricLight.ExtinctionCoef = AmandsGraphicsPlugin.FlashlightExtinctionCoef.Value;
                        if (volumetricLight.VolumetricMaterial != null)
                        {
                            volumetricLight.VolumetricMaterial.SetVector("_VolumetricLight", new Vector4(volumetricLight.ScatteringCoef, volumetricLight.ExtinctionCoef, AmandsGraphicsPlugin.FlashlightRange.Value, 1f - volumetricLight.SkyboxExtinctionCoef));
                        }
                    }
                }
            }
        }
    }
}
