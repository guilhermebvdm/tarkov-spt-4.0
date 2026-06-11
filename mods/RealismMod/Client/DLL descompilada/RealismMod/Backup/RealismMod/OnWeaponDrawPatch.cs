// Decompiled with JetBrains decompiler
// Type: RealismMod.OnWeaponDrawPatch
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

public class OnWeaponDrawPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (SkillManager).GetMethod("OnWeaponDraw", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(SkillManager __instance, Item item)
  {
    if (item == null || ((IContainer) item.Owner)?.ID == null || !(((IContainer) item.Owner).ID == Singleton<GameWorld>.Instance.MainPlayer.ProfileId) || StanceController.CurrentStance != EStance.PistolCompressed)
      return;
    StanceController.DidWeaponSwap = true;
  }
}
