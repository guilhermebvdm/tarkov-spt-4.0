using System;
using System.Reflection;
using SPT.Reflection.Patching;
using EFT;
using HarmonyLib;
using UnityEngine;

namespace CameraRotationMod.Patches
{
    public class PhysicalInertiaPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(BasePhysicalClass), nameof(BasePhysicalClass.OnWeightUpdated));
        }

        [PatchPostfix]
        private static void Postfix(BasePhysicalClass __instance)
        {
            if (Plugin._InertiaMultiplier != null && Plugin._WalkSpeedMultiplier != null && Plugin._SprintSpeedMultiplier != null)
            {
                // Item 007: Cálculo Dinâmico de Inércia baseado no Peso (Curva Exponencial)
                // Peso varia de ~1kg a ~70kg (limite de overencumbered)
                float weight = Mathf.Clamp(__instance.PreviousWeight, 1f, 70f);
                float weightRatio = weight / 70f;
                
                // Aplicamos uma curva exponencial (weightRatio ^ 1.5) para que o peso
                // tenha pouco efeito no início e cresça mais rapidamente no final.
                // Mapeamos para um debuff local de 1.0x (leve) até 3.0x (pesado)
                float curve = Mathf.Pow(weightRatio, 1.5f);
                float weightMultiplier = Mathf.Lerp(1.0f, 3.0f, curve);

                // Multiplicamos o valor nativo pelo nosso multiplicador da curva E pelo slider do F12
                __instance.Inertia *= (weightMultiplier * Plugin._InertiaMultiplier.Value);
                
                // Limites de velocidade continuam respeitando os sliders
                __instance.WalkSpeedLimit *= Plugin._WalkSpeedMultiplier.Value;
            }
        }
    }
}
