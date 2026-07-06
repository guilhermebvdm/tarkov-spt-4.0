using BepInEx.Configuration;
using UnityEngine;

namespace MOAR.Helpers;

public static class UIUtils
{
    /// <summary>
    /// KeyboardShortcut default behavior is awful and doesn't allow other buttons to be pressed during
    /// </summary>
    public static bool BetterIsPressed(this KeyboardShortcut key)
    {
        if (!Input.GetKey(key.MainKey))
        {
            return false;
        }

        foreach (var modifier in key.Modifiers)
        {
            if (!Input.GetKey(modifier))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// KeyboardShortcut default behavior is awful and doesn't allow other buttons to be pressed during
    /// </summary>
    public static bool BetterIsDown(this KeyboardShortcut key)
    {
        if (!Input.GetKeyDown(key.MainKey))
        {
            return false;
        }

        foreach (var modifier in key.Modifiers)
        {
            if (!Input.GetKey(modifier))
            {
                return false;
            }
        }

        return true;
    }
}
