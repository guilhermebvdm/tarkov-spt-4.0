// Decompiled with JetBrains decompiler
// Type: RealismMod.SetFireModePatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class SetFireModePatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (FirearmsAnimator).GetMethod("SetFireMode", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Prefix(
    FirearmsAnimator __instance,
    Weapon.EFireMode fireMode,
    bool skipAnimation = false)
  {
    ((ObjectInHandsAnimator) __instance).ResetLeftHand();
    skipAnimation = StanceController.CurrentStance == EStance.HighReady && PlayerState.IsSprinting || skipAnimation;
    WeaponAnimationSpeedControllerClass.SetFireMode(((ObjectInHandsAnimator) __instance).Animator, (float) fireMode);
    if (!skipAnimation)
      WeaponAnimationSpeedControllerClass.TriggerFiremodeSwitch(((ObjectInHandsAnimator) __instance).Animator);
    return false;
  }
}
