// Decompiled with JetBrains decompiler
// Type: RealismMod.HCApplyDamagePatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.HealthSystem;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class HCApplyDamagePatch : ModulePatch
{
  private static FieldInfo _playerField;
  private static EDamageType[] _acceptedDamageTypes;

  protected virtual MethodBase GetTargetMethod()
  {
    HCApplyDamagePatch._playerField = AccessTools.Field(typeof (ActiveHealthController), "Player");
    return (MethodBase) typeof (ActiveHealthController).GetMethod("ApplyDamage", BindingFlags.Instance | BindingFlags.Public);
  }

  private static void CancelRegen()
  {
    Plugin.RealHealthController.CancelPassiveRegen = true;
    Plugin.RealHealthController.CurrentPassiveRegenBlockDuration = Plugin.RealHealthController.BlockPassiveRegenBaseDuration;
  }

  private static void HandlePassiveRegenTimer(float damage, EDamageType damageType)
  {
    if (damageType != 512 /*0x0200*/ && damageType != 4 && damageType != 4096 /*0x1000*/ && damageType != 4194304 /*0x400000*/ && damageType != 32768 /*0x8000*/ && damageType != 16384 /*0x4000*/ && damageType != 1048576 /*0x100000*/ && damageType != 131072 /*0x020000*/ && damageType != 65536 /*0x010000*/ && damageType != 262144 /*0x040000*/ && damageType != 64 /*0x40*/ && damageType != 1024 /*0x0400*/ && damageType != 16 /*0x10*/ && damageType != 256 /*0x0100*/ && damageType != 2097152 /*0x200000*/ && damageType != 524288 /*0x080000*/ && (double) damage <= 5.0)
      return;
    HCApplyDamagePatch.CancelRegen();
  }

  [PatchPrefix]
  private static void Prefix(
    ActiveHealthController __instance,
    EBodyPart bodyPart,
    ref float damage,
    DamageInfoStruct damageInfo)
  {
    Player player = (Player) HCApplyDamagePatch._playerField.GetValue((object) __instance);
    if (!player.IsYourPlayer)
      return;
    EDamageType damageType = damageInfo.DamageType;
    float current = ((GClass2814<ActiveHealthController.GClass2813>) __instance).GetBodyPartHealth(bodyPart, false).Current;
    float maximum = ((GClass2814<ActiveHealthController.GClass2813>) __instance).GetBodyPartHealth(bodyPart, false).Maximum;
    float num1 = current / maximum;
    HCApplyDamagePatch.HandlePassiveRegenTimer(damage, damageType);
    if (damageType == 65536 /*0x010000*/ || damageType == 131072 /*0x020000*/ || damageType == 1048576 /*0x100000*/)
      damage = 0.0f;
    else if ((double) current <= 10.0 && (bodyPart == null || bodyPart == 1) && damageType == 16384 /*0x4000*/)
    {
      damage = 0.0f;
    }
    else
    {
      float num2 = player.Skills.VitalityBuffSurviobilityInc.Value;
      float num3 = player.Skills.StressPain.Value;
      int delay = (int) Math.Round(15.0 * (1.0 - (double) num2), 2);
      float tickRate = (float) Math.Round(0.2199999988079071 * (1.0 + (double) num2), 2);
      float num4 = 17f * num2;
      float num5 = 7.5f * num2;
      if (damageType == 65536 /*0x010000*/)
        Plugin.RealHealthController.DmgeTracker.TotalDehydrationDamage += damage;
      else if (damageType == 131072 /*0x020000*/)
        Plugin.RealHealthController.DmgeTracker.TotalExhaustionDamage += damage;
      else if (damageType == 2 && (double) damage <= (double) num4)
        Plugin.RealHealthController.DoPassiveRegen(tickRate, bodyPart, player, delay, damage, damageType);
      else if (damageType == 8)
        Plugin.RealHealthController.DoPassiveRegen(tickRate, bodyPart, player, delay, damage * 0.75f, damageType);
      else if (damageType == 8192 /*0x2000*/ && (double) damage <= (double) num5)
        Plugin.RealHealthController.DoPassiveRegen(tickRate, bodyPart, player, delay, damage * 0.75f, damageType);
      else if (damageType == 32768 /*0x8000*/ || damageType == 16384 /*0x4000*/)
      {
        Plugin.RealHealthController.DmgeTracker.UpdateDamage(damageType, bodyPart, damage);
      }
      else
      {
        if (damageType == 512 /*0x0200*/ || damageType == 4 || damageType == 2048 /*0x0800*/ || damageType == 2 && (double) damage >= (double) num4 + 2.0 || damageType == 8192 /*0x2000*/ && (double) damage >= (double) num5 + 2.0)
          Plugin.RealHealthController.RemoveEffectsOfType(EHealthEffectType.HealthRegen);
        if (damageType == 512 /*0x0200*/ || damageType == 8192 /*0x2000*/ || damageType == 1024 /*0x0400*/ || damageType == 4096 /*0x1000*/)
        {
          float num6 = (float) Math.Round(20.0 * (1.0 + (double) num3 / 2.0), 2);
          float num7 = (float) Math.Round(25.0 * (1.0 - (double) num3 / 2.0), 2);
          float negativeEffectStrength = (float) Math.Round(0.949999988079071 * (1.0 - (double) num3 / 2.0), 2);
          Plugin.RealHealthController.TryAddAdrenaline(player, num7, num7, negativeEffectStrength);
        }
      }
    }
  }

  static HCApplyDamagePatch()
  {
    // ISSUE: unable to decompile the method.
  }
}
