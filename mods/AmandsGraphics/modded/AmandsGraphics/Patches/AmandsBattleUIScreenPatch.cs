using System.Reflection;
using SPT.Reflection.Patching;
using EFT.UI;

namespace AmandsGraphics.Patches;

public sealed class AmandsBattleUIScreenPatch : ModulePatch
{
    protected override MethodBase GetTargetMethod()
    {
        return typeof(EftBattleUIScreen).GetMethods(BindingFlags.Instance | BindingFlags.Public).First(x => x.Name == "Show" && x.GetParameters()[0].Name == "owner");
    }

    [PatchPostfix]
    public static void PatchPostFix(ref EftBattleUIScreen __instance)
    {
        if (AmandsGraphicsClass.ActiveUIScreen == __instance.gameObject)
        {
            return;
        }

        AmandsGraphicsClass.ActiveUIScreen = __instance.gameObject;
        AmandsGraphicsClass.DestroyGameObjects();
        AmandsGraphicsClass.CreateGameObjects(__instance.transform);
    }
}
