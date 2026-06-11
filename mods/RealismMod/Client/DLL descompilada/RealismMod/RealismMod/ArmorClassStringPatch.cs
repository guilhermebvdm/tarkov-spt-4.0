// Decompiled with JetBrains decompiler
// Type: RealismMod.ArmorClassStringPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class ArmorClassStringPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (ArmorComponent.Class2165).GetMethod("method_3", BindingFlags.Instance | BindingFlags.Public);
  }

  private static string GetItemClass(CompositeArmorComponent x)
  {
    Gear dataObj = TemplateStats.GetDataObj<Gear>(TemplateStats.GearStats, MongoID.op_Implicit(((GClass3175) x).Item.TemplateId));
    return $"{GClass2112.Localized(((GClass3175) x).Item.ShortName, (string) null)}: {dataObj.ArmorClass}";
  }

  [SPT.Reflection.Patching.PatchPrefix]
  private static bool PatchPrefix(ArmorComponent.Class2165 __instance, ref string __result)
  {
    CompositeArmorComponent[] array = GClass3176.GetItemComponentsInChildren<CompositeArmorComponent>(__instance.item, true).ToArray<CompositeArmorComponent>();
    Gear dataObj = TemplateStats.GetDataObj<Gear>(TemplateStats.GearStats, MongoID.op_Implicit(((GClass3175) __instance.armorComponent_0).Item.TemplateId));
    if (array.Length > 1)
      __result = GClass1835.CastToStringValue<string>(((IEnumerable<CompositeArmorComponent>) array).Select<CompositeArmorComponent, string>(new Func<CompositeArmorComponent, string>(ArmorClassStringPatch.GetItemClass)), "\n", true);
    __result = dataObj.ArmorClass;
    return false;
  }
}
