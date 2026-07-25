using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace TarkovIRL;

public class StaminaRegenRatePatch : ModulePatch
{
  protected override MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (PlayerPhysicalClass).GetMethod("method_21", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(PlayerPhysicalClass __instance, ref float __result)
  {
    float num = PlayerMotionController.IsAugmentedBreath ? -4f : 0.0f;
    __result += num;
  }
}

