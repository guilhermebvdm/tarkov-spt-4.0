// Decompiled with JetBrains decompiler
// Type: RealismMod.HealCostDisplayFullPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using SPT.Reflection.Patching;
using System;
using System.Reflection;
using System.Text;

#nullable disable
namespace RealismMod;

public class HealCostDisplayFullPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (GClass1372).GetMethod("GetFullStringValue", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Prefix(GClass1372 __instance, string displayName, ref string __result)
  {
    if (GClass834.IsZero(__instance.Delay) && GClass834.IsZero(__instance.Duration) && __instance.Cost == 0)
      __result = string.Empty;
    StringBuilder stringBuilder = new StringBuilder();
    stringBuilder.Append(GClass2112.Localized(displayName, (string) null));
    if ((double) __instance.Delay > 1.0)
      stringBuilder.Append($"\n{GClass2112.Localized("Delay", (string) null)} {__instance.Delay}{GClass2112.Localized("sec", (string) null)}");
    if ((double) __instance.Duration > 0.0)
      stringBuilder.Append($"\n{GClass2112.Localized("Duration", (string) null)} {(ValueType) (float) ((double) __instance.Duration + 1.0)}{GClass2112.Localized("sec", (string) null)}");
    if (__instance.Cost > 0)
      stringBuilder.Append($"\n{(__instance.Cost + 1).ToString()} HP");
    __result = stringBuilder.ToString();
    return false;
  }
}
