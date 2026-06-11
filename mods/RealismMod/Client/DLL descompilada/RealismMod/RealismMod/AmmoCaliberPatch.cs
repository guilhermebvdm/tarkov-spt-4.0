// Decompiled with JetBrains decompiler
// Type: RealismMod.AmmoCaliberPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class AmmoCaliberPatch : ModulePatch
{
  private static string[] _multiCals = new string[5]
  {
    "556x45NATO",
    "762x35",
    "762x51",
    "762x39",
    "366TKM"
  };

  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (Weapon).GetMethod("get_AmmoCaliber", BindingFlags.Instance | BindingFlags.Public);
  }

  private static string GetCaliber(IEnumerable<Mod> mods)
  {
    mods.Count<Mod>();
    foreach (Mod mod in mods)
    {
      string modType = TemplateStats.GetDataObj<WeaponMod>(TemplateStats.WeaponModStats, MongoID.op_Implicit(((Item) mod).TemplateId)).ModType;
      if (Utils.IsBarrel(mod) && ((IEnumerable<string>) AmmoCaliberPatch._multiCals).Contains<string>(modType))
        return modType;
    }
    return (string) null;
  }

  [PatchPrefix]
  private static bool Prefix(Weapon __instance, ref string __result)
  {
    if (Utils.PlayerIsReady && ((IContainer) ((Item) __instance)?.Owner)?.ID != Singleton<GameWorld>.Instance?.MainPlayer?.ProfileId)
      return true;
    string caliber = AmmoCaliberPatch.GetCaliber(__instance.Mods);
    if (caliber == null)
      return true;
    __result = caliber;
    return false;
  }
}
