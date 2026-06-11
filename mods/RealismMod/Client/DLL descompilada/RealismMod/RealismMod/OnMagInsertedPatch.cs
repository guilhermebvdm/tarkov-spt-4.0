// Decompiled with JetBrains decompiler
// Type: RealismMod.OnMagInsertedPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class OnMagInsertedPatch : ModulePatch
{
  private static FieldInfo _playerField;

  protected virtual MethodBase GetTargetMethod()
  {
    OnMagInsertedPatch._playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    return (MethodBase) typeof (Player.FirearmController).GetMethod("method_50", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(ref Player.FirearmController __instance)
  {
    Player player = (Player) OnMagInsertedPatch._playerField.GetValue((object) __instance);
    if (!player.IsYourPlayer)
      return;
    PlayerState.IsMagReloading = false;
    PlayerState.IsQuickReloading = false;
    player.HandsAnimator.SetAnimationSpeed(1f);
    if (PluginConfig.EnableReloadLogging.Value)
    {
      ModulePatch.Logger.LogWarning((object) "===OnMagInsertedPatch/method_50===");
      ModulePatch.Logger.LogWarning((object) "=============");
    }
  }
}
