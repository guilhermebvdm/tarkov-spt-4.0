// Decompiled with JetBrains decompiler
// Type: RealismMod.PenetrationUIPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class PenetrationUIPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (GClass3555).GetConstructor(new Type[1]
    {
      typeof (float)
    });
  }

  private static string GetStringValues(int armorClass, float penetrationPower)
  {
    ArmorResistanceStruct resistanceStruct = GClass638.RealResistance(100f, 100f, armorClass, penetrationPower);
    float penetrationChance = ((ArmorResistanceStruct) ref resistanceStruct).GetPenetrationChance(penetrationPower);
    return (armorClass >= 10 ? $"Lvl {armorClass.ToString()} " : $"Lvl {armorClass.ToString()} ") + GClass3555.smethod_0(penetrationChance);
  }

  [PatchPostfix]
  private static void PatchPrefix(GClass3555 __instance, float penetrationPower)
  {
    List<GClass3554> gclass3554List = new List<GClass3554>();
    for (int armorClass = 1; armorClass <= 10; ++armorClass)
      gclass3554List.Add(new GClass3554(PenetrationUIPatch.GetStringValues(armorClass, penetrationPower), (string) null));
    AccessTools.Field(typeof (GClass3555), "Lines").SetValue((object) __instance, (object) gclass3554List.ToArray());
  }
}
