// Decompiled with JetBrains decompiler
// Type: RealismMod.Audio.RegisterShotPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod.Audio;

public class RegisterShotPatch : ModulePatch
{
  private const float SuppressorWithSubsFactor = 0.25f;
  private const float SubsonicFactor = 0.85f;
  private const float SpeedOfSound = 335f;
  private static FieldInfo playerField;

  protected virtual MethodBase GetTargetMethod()
  {
    RegisterShotPatch.playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    return (MethodBase) typeof (Player.FirearmController).GetMethod("RegisterShot", BindingFlags.Instance | BindingFlags.Public);
  }

  private static float CalcAmmoFactor(float ammoRec)
  {
    return Mathf.Clamp((float) ((double) ammoRec / 100.0 + 1.0), 0.8f, 2f);
  }

  private static float CalcVelocityFactor(float speedFactor)
  {
    return (float) (((double) speedFactor - 1.0) * -2.0 + 1.0);
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(Player.FirearmController __instance, EftBulletClass shot)
  {
    Player player = (Player) RegisterShotPatch.playerField.GetValue((object) __instance);
    if (((IWeapon) __instance.Weapon).IsUnderbarrelWeapon)
      return;
    Weapon weapon = __instance.Weapon;
    if (player.IsYourPlayer)
    {
      float num = RegisterShotPatch.CalcVelocityFactor(weapon.SpeedFactor) * RegisterShotPatch.CalcAmmoFactor((float) (shot.Ammo as AmmoItemClass).ammoRec);
      if ((double) shot.InitialSpeed * (double) weapon.SpeedFactor <= 335.0)
        num *= __instance.IsSilenced ? 0.25f : 0.85f;
      DeafenController.AmmoDeafFactor = Mathf.Clamp(num, 0.5f, 2f);
    }
    else
    {
      Vector3 position = Singleton<GameWorld>.Instance.MainPlayer.Transform.position;
      float num1 = Vector3.Distance(player.Transform.position, position);
      if ((double) num1 <= 15.0)
      {
        float num2 = RegisterShotPatch.CalcVelocityFactor(weapon.SpeedFactor);
        float num3 = StatCalc.CaliberLoudnessFactor(weapon.AmmoCaliber);
        if ((double) shot.InitialSpeed * (double) weapon.SpeedFactor <= 335.0)
          num2 *= __instance.IsSilenced ? 0.25f : 0.85f;
        DeafenController.BotFiringDeafFactor = Mathf.Clamp((float) ((double) (num3 * num2) * (-(double) num1 / 100.0 + 1.0) * 1.25), 1f, 5f);
        DeafenController.IsBotFiring = true;
        DeafenController.BotTimer = 0.0f;
        DeafenController.IncreaseDeafeningShotAt();
      }
    }
  }
}
