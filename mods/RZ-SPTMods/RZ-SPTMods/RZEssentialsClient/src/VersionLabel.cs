// RemzDNB - 2026

using System.Reflection;
using EFT.UI;
using SPT.Reflection.Patching;

namespace RZEssentialsClient;

public class VersionLabelPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
        => typeof(LocalizedText).GetMethod("method_1",
               BindingFlags.Public | BindingFlags.Instance)!;

    [PatchPrefix]
    public static bool Prefix(LocalizedText __instance)
    {
        if (!ClientConfig.Instance.EnableVersionLabelOverride || string.IsNullOrEmpty(ClientConfig.Instance.VersionLabelText))
            return true;

        if (__instance.gameObject.name != "AlphaLabel")
            return true;
        
        __instance.method_2(ClientConfig.Instance.VersionLabelText);
        
        return false;
    }
}