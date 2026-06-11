// Decompiled with JetBrains decompiler
// Type: RealismMod.ArmorLevelDisplayPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class ArmorLevelDisplayPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (GClass2936).GetMethod("FormatArmorClassIcon", BindingFlags.Public | BindingFlags.Static);
  }

  [SPT.Reflection.Patching.PatchPrefix]
  private static bool PatchPrefix(GClass2936 __instance, ref string __result, int armorClass)
  {
    __result = "Lvl " + armorClass.ToString();
    return false;
  }
}
