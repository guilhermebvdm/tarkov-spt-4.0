// Decompiled with JetBrains decompiler
// Type: RealismMod.COIDeltaPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class COIDeltaPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (Weapon).GetMethod("get_CenterOfImpactDelta", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Prefix(Weapon __instance, ref float __result)
  {
    if (!Utils.PlayerIsReady || ((Item) __instance)?.Owner == null || __instance == null || ((IContainer) ((Item) __instance).Owner)?.ID == null || !(((IContainer) ((Item) __instance).Owner).ID == Singleton<GameWorld>.Instance.MainPlayer.ProfileId))
      return true;
    __result = WeaponStats.COIDelta;
    return false;
  }
}
