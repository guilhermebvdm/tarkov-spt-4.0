using System;
using System.Reflection;
using BepInEx.Logging;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace TRLWeatherSync.Patches;

public class RainDropVisibilityPatch : ModulePatch
{
    private static ManualLogSource Log => TRLWeatherSyncPlugin.Log;
    private static readonly int _sizePropertyId = Shader.PropertyToID("_Size");
    private static bool _hasLoggedError;

    protected override MethodBase GetTargetMethod()
    {
        // ref: Assembly-CSharp/RainFallDrops.cs:129
        return AccessTools.Method(typeof(RainFallDrops), nameof(RainFallDrops.method_2));
    }

    [PatchPostfix]
    private static void Postfix(Material ___material_0)
    {
        try
        {
            float minSize = TRLWeatherSyncPlugin.MinRainDropSize.Value;
            if (minSize <= 0f || ___material_0 == null)
            {
                return;
            }

            Vector4 currentSize = ___material_0.GetVector(_sizePropertyId);
            float aspect = (float)Screen.height / (float)Screen.width;
            float minWidth = minSize * aspect;
            float minLength = minSize;

            if (currentSize.x < minWidth || currentSize.y < minLength)
            {
                currentSize.x = Mathf.Max(currentSize.x, minWidth);
                currentSize.y = Mathf.Max(currentSize.y, minLength);
                ___material_0.SetVector(_sizePropertyId, currentSize);
            }
        }
        catch (Exception ex)
        {
            if (!_hasLoggedError)
            {
                _hasLoggedError = true;
                Log.LogError($"[TRL-WeatherSync] Falha ao ajustar tamanho da gota de chuva (RainFallDrops.method_2): {ex}");
            }
        }
    }
}
