using System.ComponentModel;
using EFT.InputSystem;

namespace MagCheckInterrupt.Utils;

public static class KeybindsUtil
{
    private static KeyBindingClass _reloadKeybind;
    private static KeyBindingClass _checkKeybind;

    public static void UpdateKeys(InputBindingsDataClass keys)
    {
        foreach (var key in keys.Gclass2408_0)
        {
            if (key is not KeyBindingClass keybind) continue;

            switch (keybind.GameKey)
            {
                case EGameKey.ReloadWeapon:
                    _reloadKeybind = keybind;
                    continue;
                case EGameKey.CheckAmmo:
                    _checkKeybind = keybind;
                    continue;
            }
        }
    }

    public static bool AreCheckAndReloadKeysConflicting()
    {
        return _reloadKeybind.KeyCombinationState_0.GetKeysStatus(out var reloadStatus)
               && (reloadStatus == EKeyPress.Down || reloadStatus == EKeyPress.Hold)
               && _checkKeybind.KeyCombinationState_0.GetKeysStatus(out var checkStatus)
               && (checkStatus == EKeyPress.Down || checkStatus == EKeyPress.Hold);
    }

    public enum EReloadMode
    {
        [Description("Press to Reload")]
        Press,

        [Description("Release to Reload")]
        Release,
    }
}
