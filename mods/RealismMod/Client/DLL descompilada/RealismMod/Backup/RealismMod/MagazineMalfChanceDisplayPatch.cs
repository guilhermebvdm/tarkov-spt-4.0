// Decompiled with JetBrains decompiler
// Type: RealismMod.MagazineMalfChanceDisplayPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class MagazineMalfChanceDisplayPatch : ModulePatch
{
  private static string[] malfChancesKeys = new string[6]
  {
    "Malfunction/NoneChance",
    "Malfunction/VeryLowChance",
    "Malfunction/LowChance",
    "Malfunction/MediumChance",
    "Malfunction/HighChance",
    "Malfunction/VeryHighChance"
  };

  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (MagazineItemClass).GetMethod("method_40", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Prefix(MagazineItemClass __instance, ref string __result)
  {
    float malfunctionChance = __instance.MalfunctionChance;
    string str = "";
    float num = malfunctionChance;
    if ((double) num > 0.0)
    {
      if ((double) num > 0.05000000074505806)
      {
        if ((double) num > 0.20000000298023224)
        {
          if ((double) num > 0.60000002384185791)
          {
            if ((double) num > 1.2000000476837158)
            {
              if ((double) num > 1.2000000476837158)
                str = MagazineMalfChanceDisplayPatch.malfChancesKeys[5];
            }
            else
              str = MagazineMalfChanceDisplayPatch.malfChancesKeys[4];
          }
          else
            str = MagazineMalfChanceDisplayPatch.malfChancesKeys[3];
        }
        else
          str = MagazineMalfChanceDisplayPatch.malfChancesKeys[2];
      }
      else
        str = MagazineMalfChanceDisplayPatch.malfChancesKeys[1];
    }
    else
      str = MagazineMalfChanceDisplayPatch.malfChancesKeys[0];
    __result = GClass2112.Localized(str, (string) null);
    return false;
  }
}
