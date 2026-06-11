// Decompiled with JetBrains decompiler
// Type: RealismMod.HealCostDisplayShortPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using SPT.Reflection.Patching;
using System;
using System.Reflection;
using System.Text;

#nullable disable
namespace RealismMod;

public class HealCostDisplayShortPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (GClass1372).GetMethod("GetStringValue", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Prefix(GClass1372 __instance, ref string __result)
  {
    StringBuilder stringBuilder = new StringBuilder();
    if ((double) __instance.Delay > 1.0)
      stringBuilder.Append($"{GClass2112.Localized("Del.", (string) null)} {__instance.Delay}{GClass2112.Localized("sec", (string) null)}");
    if ((double) __instance.Duration > 0.0)
    {
      if (stringBuilder.Length > 0)
        stringBuilder.Append(" / ");
      stringBuilder.Append($"{GClass2112.Localized("Dur.", (string) null)} {__instance.Duration}{GClass2112.Localized("sec", (string) null)}");
    }
    if (__instance.Cost > 0)
    {
      if (stringBuilder.Length > 0)
        stringBuilder.Append(" / ");
      stringBuilder.Append((__instance.Cost + 1).ToString() + " HP");
    }
    if (__instance.HealthPenaltyMax == 69)
      stringBuilder.Append($"(<color=#54C1FFFF>{(ValueType) (float) -(double) __instance.FadeOut}</color>)");
    __result = stringBuilder.ToString();
    return false;
  }
}
