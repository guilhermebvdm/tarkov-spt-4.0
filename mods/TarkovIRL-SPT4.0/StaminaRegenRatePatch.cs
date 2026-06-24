// Decompiled with JetBrains decompiler
// Type: TarkovIRL.StaminaRegenRatePatch
// Assembly: TarkovIRL, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C42939BD-7BF0-4586-ABE5-9D2EFC361A0B
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\TarkovIRL_WeaponsHandlingMod_1.0.0\BepInEx\plugins\TarkovIRL.dll

using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace TarkovIRL;

public class StaminaRegenRatePatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (PlayerPhysicalClass).GetMethod("method_21", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(PlayerPhysicalClass __instance, ref float __result)
  {
    float num = PlayerMotionController.IsAugmentedBreath ? -4f : 0.0f;
    __result += num;
  }
}
