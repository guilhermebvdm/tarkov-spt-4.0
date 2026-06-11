// Decompiled with JetBrains decompiler
// Type: RealismMod.BreathIsAudiblePatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class BreathIsAudiblePatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (BasePhysicalClass).GetMethod("get_BreathIsAudible", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Prefix(BasePhysicalClass __instance, ref bool __result)
  {
    if (__instance.iobserverToPlayerBridge_0.iPlayer.IsAI)
      return true;
    __result = !__instance.HoldingBreath && (__instance.StaminaParameters.StaminaExhaustionStartsBreathSound && __instance.Stamina.Exhausted || __instance.Oxygen.Exhausted || Plugin.RealHealthController.HasOverdosed);
    return false;
  }
}
