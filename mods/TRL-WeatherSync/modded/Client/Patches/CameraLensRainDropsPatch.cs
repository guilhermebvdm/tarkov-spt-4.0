using System;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace TRLWeatherSync.Patches;

public class CameraLensRainDropsPatch : ModulePatch
{
    private static ManualLogSource Log => TRLWeatherSyncPlugin.Log;
    private static bool _hasLoggedError;

    protected override MethodBase GetTargetMethod()
    {
        // ref: Assembly-CSharp/GClass986.cs:114
        return AccessTools.Method(typeof(GClass986), nameof(GClass986.Update));
    }

    [PatchPrefix]
    private static bool Prefix(GClass986 __instance, float dt)
    {
        if (!TRLWeatherSyncPlugin.EnableLensDropsTuning.Value)
        {
            return true;
        }

        try
        {
            bool isCameraUnderRain = RainController.IsCameraUnderRain;
            float intensity = __instance.Intensity;

            if (intensity > 0.05f && isCameraUnderRain && __instance.Transform_0 != null)
            {
                // pitchDot: -1 (olhando 100% chão), 0 (horizonte), +1 (olhando 100% céu)
                float pitchDot = Vector3.Dot(__instance.Transform_0.forward, Vector3.up);

                float forwardMultiplier = TRLWeatherSyncPlugin.LensRainDropsForwardRate.Value;
                float skyMultiplier = TRLWeatherSyncPlugin.LensRainDropsLookUpMultiplier.Value;

                float num;
                if (pitchDot > 0f)
                {
                    // Olhando para cima: interpola do horizonte até o multiplicador de céu
                    num = Mathf.Lerp(0.6f * forwardMultiplier, skyMultiplier, pitchDot);
                }
                else if (pitchDot > -0.15f)
                {
                    // Transição suave ao redor do horizonte
                    float t = Mathf.InverseLerp(-0.15f, 0f, pitchDot);
                    num = Mathf.Lerp(0f, 0.6f * forwardMultiplier, t);
                }
                else
                {
                    // Olhando para baixo: zera completamente novas gotas
                    num = 0f;
                }

                // Acumula tempo de spawn
                __instance.Float_6 += dt * UnityEngine.Random.Range(0.7f, 3f) * num;

                if (__instance.Float_6 > __instance.Float_1 && num > 0.1f)
                {
                    // Permite mais gotas por ciclo quando olhando para cima (clamp entre 1 e 12)
                    int maxAllowed = Mathf.Clamp(Mathf.RoundToInt(__instance.Int_0 * (pitchDot > 0f ? (1f + pitchDot * 1.5f) : 1f)), 1, 12);
                    int numDrops = Mathf.CeilToInt(UnityEngine.Random.Range(1, maxAllowed + 1) * num * Mathf.Max(intensity, 0.35f));

                    for (int i = 0; i < numDrops; i++)
                    {
                        if (__instance.Queue_0.Count > 0)
                        {
                            __instance.method_1(Mathf.Clamp01(num));

                            // Limita o tempo de vida da gota recém-criada
                            if (__instance.List_0.Count > 0)
                            {
                                var lastDrop = __instance.List_0[__instance.List_0.Count - 1];
                                float maxLifetime = TRLWeatherSyncPlugin.LensRainDropsMaxLifetime.Value;
                                if (lastDrop.Lifetime > maxLifetime)
                                {
                                    lastDrop.Lifetime = maxLifetime;
                                }
                            }
                        }
                    }
                    __instance.Float_6 = 0f;
                }
            }

            // Atualização e renderização das gotas existentes
            if (__instance.List_0.Count != 0)
            {
                float updateDt = dt;
                // Secagem acelerada se olhando para baixo
                if (TRLWeatherSyncPlugin.LensRainDropsLookDownDrain.Value && __instance.Transform_0 != null)
                {
                    float pitchDot = Vector3.Dot(__instance.Transform_0.forward, Vector3.up);
                    if (pitchDot < -0.2f)
                    {
                        // Acelera em 3.5x o tempo de vida restante das gotas ativas
                        updateDt = dt * 3.5f;
                    }
                }

                __instance.method_0(updateDt);
            }

            return false;
        }
        catch (Exception ex)
        {
            if (!_hasLoggedError)
            {
                _hasLoggedError = true;
                Log.LogError($"[TRL-WeatherSync] Falha no patch de gotas da lente (GClass986.Update): {ex}");
            }
            return true; // Fallback vanilla se ocorrer exceção
        }
    }
}
