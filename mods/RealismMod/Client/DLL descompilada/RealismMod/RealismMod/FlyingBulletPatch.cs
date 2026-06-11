// Decompiled with JetBrains decompiler
// Type: RealismMod.FlyingBulletPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class FlyingBulletPatch : ModulePatch
{
  private static FieldInfo _playerField;

  protected virtual MethodBase GetTargetMethod()
  {
    FlyingBulletPatch._playerField = AccessTools.Field(typeof (FlyingBulletSoundPlayer), "player_0");
    return (MethodBase) typeof (FlyingBulletSoundPlayer).GetMethod("method_3", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPostfix]
  private static void Postfix(FlyingBulletSoundPlayer __instance)
  {
    Player player = (Player) FlyingBulletPatch._playerField.GetValue((object) __instance);
    if (!player.IsYourPlayer)
      return;
    if (Plugin.ServerConfig.med_changes)
    {
      float num = player.Skills.StressPain.Value;
      float painkillerDuration = (float) Math.Round(12.0 * (1.0 + (double) num), 2);
      float negativeEffectDuration = (float) Math.Round(15.0 * (1.0 - (double) num), 2);
      float negativeEffectStrength = (float) Math.Round(0.75 * (1.0 - (double) num), 2);
      Plugin.RealHealthController.TryAddAdrenaline(player, painkillerDuration, negativeEffectDuration, negativeEffectStrength);
    }
    if (Plugin.ServerConfig.headset_changes)
    {
      DeafenController.IsBotFiring = true;
      DeafenController.BotTimer = 0.0f;
    }
  }
}
