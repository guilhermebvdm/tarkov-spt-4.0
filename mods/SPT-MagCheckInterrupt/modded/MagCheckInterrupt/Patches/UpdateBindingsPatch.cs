using System.Reflection;
using MagCheckInterrupt.Utils;
using SPT.Reflection.Patching;

namespace MagCheckInterrupt.Patches;

public class UpdateBindingsPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(InputBindingsDataClass).GetMethod(nameof(InputBindingsDataClass.UpdateBindings));
    }

    [PatchPostfix]
    public static void Postfix(InputBindingsDataClass __instance)
    {
        KeybindsUtil.UpdateKeys(__instance);
    }
}
