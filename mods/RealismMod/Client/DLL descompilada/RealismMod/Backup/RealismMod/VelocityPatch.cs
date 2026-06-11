// Decompiled with JetBrains decompiler
// Type: RealismMod.VelocityPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class VelocityPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (GClass3451).GetMethod("Initialize", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPostfix]
  private static void Prefix(GClass3451 __instance)
  {
    float num1 = (float) AccessTools.Field(typeof (GClass3451), "float_3").GetValue((object) __instance);
    float num2;
    AccessTools.Field(typeof (GClass3451), "float_3").SetValue((object) __instance, (object) (num2 = num1 * PluginConfig.DragModifier.Value));
  }
}
