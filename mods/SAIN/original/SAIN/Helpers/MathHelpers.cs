using System;
using UnityEngine;

namespace SAIN.Helpers;

public static class MathHelpers
{
    public static float ClampObject(object value, float min, float max)
    {
        if (value != null)
        {
            Type type = value.GetType();
            if (type == typeof(float))
            {
                return Mathf.Clamp((float)value, min, max);
            }
            else if (type == typeof(int))
            {
                return Mathf.Clamp(Mathf.RoundToInt((float)value), Mathf.RoundToInt(min), Mathf.RoundToInt(max));
            }
            else
            {
                Logger.LogError($"{type}");
            }
        }
        else
        {
            Logger.LogError($"Null!?");
        }
        return default;
    }
}
