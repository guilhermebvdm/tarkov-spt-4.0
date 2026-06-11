// Decompiled with JetBrains decompiler
// Type: RealismMod.ApplyDamageInfoPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Comfort.Common;
using EFT;
using EFT.HealthSystem;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class ApplyDamageInfoPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (Player).GetMethod("ApplyDamageInfo", BindingFlags.Instance | BindingFlags.Public);
  }

  private static void TryDoDisarm(
    Player player,
    float kineticEnergy,
    bool hitArmArmor,
    bool forearm)
  {
    Player.ItemHandsController handsController1 = player.HandsController as Player.ItemHandsController;
    if (Object.op_Inequality((Object) handsController1, (Object) null) && handsController1.CurrentCompassState)
    {
      handsController1.SetCompassState(false);
    }
    else
    {
      if (!Object.op_Equality((Object) player.MovementContext.StationaryWeapon, (Object) null) || player.HandsController.IsPlacingBeacon() || player.HandsController.IsInInteractionStrictCheck() || player.CurrentStateName == 9 || player.IsSprintEnabled || (double) Random.Range(1, 81) > (double) Mathf.Round(PluginConfig.DisarmBaseChance.Value * (float) (1.0 + (double) kineticEnergy / 1000.0) * (hitArmArmor ? 0.5f : 1f) * (forearm ? 1.5f : 1f)) || !Object.op_Inequality((Object) (player.HandsController as Player.FirearmController), (Object) null))
        return;
      Player.FirearmController handsController2 = player.HandsController as Player.FirearmController;
      if (handsController2.Item != null && ((TraderControllerClass) player.InventoryController).CanThrow((Item) handsController2.Item))
        ((TraderControllerClass) player.InventoryController).TryThrowItem((Item) handsController2.Item, (Callback) null, false);
    }
  }

  private static void TryDoKnockdown(
    Player player,
    float kineticEnergy,
    bool bonusChance,
    bool isPlayer)
  {
    if ((double) Random.Range(1, 51) > (double) Mathf.Round(PluginConfig.FallBaseChance.Value * (float) (1.0 + (double) kineticEnergy / 1000.0) * (bonusChance ? 2f : 1f)))
      return;
    player.ToggleProne();
    if (isPlayer && PluginConfig.CanDisarmPlayer.Value || !isPlayer && PluginConfig.CanDisarmBot.Value)
      ApplyDamageInfoPatch.TryDoDisarm(player, kineticEnergy * 0.25f, false, false);
  }

  private static void DisarmAndKnockdownCheck(
    Player player,
    DamageInfoStruct damageInfo,
    EBodyPart bodyPartType,
    EBodyPartColliderType partHit,
    float KE,
    bool hasArmArmor)
  {
    float num1 = (((GClass2814<ActiveHealthController.GClass2813>) player.ActiveHealthController).GetBodyPartHealth((EBodyPart) 7, false).Current - damageInfo.Damage) / ((GClass2814<ActiveHealthController.GClass2813>) player.ActiveHealthController).GetBodyPartHealth((EBodyPart) 7, false).Maximum;
    float num2 = ((GClass2814<ActiveHealthController.GClass2813>) player.ActiveHealthController).GetBodyPartHealth(bodyPartType, false).Current - damageInfo.Damage;
    bool flag1 = !player.IsInPronePose && (!player.IsYourPlayer && PluginConfig.CanFellBot.Value || player.IsYourPlayer && PluginConfig.CanFellPlayer.Value);
    bool flag2 = !player.IsYourPlayer && PluginConfig.CanDisarmBot.Value || player.IsYourPlayer && PluginConfig.CanDisarmPlayer.Value;
    bool forearm = partHit == 5 || partHit == 7;
    bool flag3 = partHit == 9 || partHit == 11;
    bool flag4 = partHit == 8 || partHit == 10;
    bool flag5 = player.IsYourPlayer && Plugin.RealHealthController.HasOverdosed && (double) damageInfo.Damage > 10.0;
    bool flag6 = damageInfo.DamageType == 2 && (double) damageInfo.Damage >= 15.0;
    bool flag7 = flag3 | flag4 && (double) num2 <= 5.0;
    bool flag8 = forearm && (double) num2 <= 5.0;
    bool flag9 = bodyPartType == null && (double) num2 > 0.0 && (double) num2 <= 12.0 && (double) damageInfo.Damage >= 5.0;
    bool bonusChance = flag3 || bodyPartType == 0;
    float num3 = flag6 ? 50000f : 1f;
    if (flag2 && flag8 | flag5 | flag6)
      ApplyDamageInfoPatch.TryDoDisarm(player, KE * num3, hasArmArmor, forearm);
    if (!flag1 || !(flag7 | flag9 | flag5 | flag6))
      return;
    ApplyDamageInfoPatch.TryDoKnockdown(player, KE * num3, bonusChance, player.IsYourPlayer);
  }

  [PatchPrefix]
  private static void Prefix(
    Player __instance,
    ref DamageInfoStruct damageInfo,
    EBodyPart bodyPartType)
  {
    if (PluginConfig.DevMode.Value && __instance.IsYourPlayer)
    {
      damageInfo.Damage = 0.0f;
    }
    else
    {
      int num;
      if (damageInfo.DamageType != 8192 /*0x2000*/)
      {
        MongoID? blockedBy = damageInfo.BlockedBy;
        num = !string.IsNullOrEmpty(blockedBy.HasValue ? MongoID.op_Implicit(blockedBy.GetValueOrDefault()) : (string) null) ? 1 : 0;
      }
      else
        num = 1;
      bool isBluntDamage = num != 0;
      bool flag = Utils.IsZombie(__instance);
      if (isBluntDamage)
      {
        damageInfo.BleedBlock = true;
        damageInfo.HeavyBleedingDelta = -1f;
        damageInfo.LightBleedingDelta = -1f;
      }
      if (((damageInfo.DamageType == 512 /*0x0200*/ ? 1 : (damageInfo.DamageType == 1024 /*0x0400*/ ? 1 : 0)) | (isBluntDamage ? 1 : 0)) == 0)
        return;
      EBodyPartColliderType partHit = damageInfo.BodyPartColliderType != -1 ? damageInfo.BodyPartColliderType : ((BodyPartCollider) damageInfo.HittedBallisticCollider).BodyPartColliderType;
      if (PluginConfig.EnableBodyHitZones.Value)
        BallisticsController.ModifyDamageByZone(__instance, partHit, ((GInterface355) __instance.HealthController).GetBodyPartHealth(bodyPartType, false).Maximum, isBluntDamage, ref damageInfo);
      float KE = 1f;
      AmmoTemplate itemTemplate = damageInfo.SourceId == null || damageInfo.DamageType == 1024 /*0x0400*/ ? (AmmoTemplate) null : Utils.GetItemTemplate<AmmoTemplate>(MongoID.op_Implicit(damageInfo.SourceId));
      BallisticsController.GetKineticEnergy(damageInfo, itemTemplate, ref KE);
      bool isBuckshot = itemTemplate != null && itemTemplate.ProjectileCount > 1;
      if (!flag && !isBuckshot && __instance.IsAI && Object.op_Inequality((Object) damageInfo.HittedBallisticCollider, (Object) null) && !((DamageInfoStruct) ref damageInfo).Blunt && PluginConfig.EnableHitSounds.Value)
        BallisticsController.PlayBodyHitSound(bodyPartType, ((Component) damageInfo.HittedBallisticCollider).transform.position, Random.Range(0, 2));
      bool doSpalling = !flag && BallisticsController.ShouldDoSpalling(isBuckshot, itemTemplate, damageInfo, bodyPartType, isBluntDamage);
      bool hasArmArmor = false;
      bool hasLegProtection = false;
      int faceProtectionCount = 0;
      ArmorComponent armor = (ArmorComponent) null;
      if (doSpalling)
        BallisticsController.GetArmorComponents(__instance, damageInfo, bodyPartType, ref armor, ref faceProtectionCount, ref doSpalling, ref hasArmArmor, ref hasLegProtection);
      if (!flag & doSpalling && armor != null && __instance?.ActiveHealthController != null && TemplateStats.GetDataObj<Gear>(TemplateStats.GearStats, MongoID.op_Implicit(((GClass3175) armor).Item.TemplateId)).CanSpall)
        BallisticsController.CalculatSpalling(__instance, ref damageInfo, KE, armor, itemTemplate, faceProtectionCount, hasArmArmor, hasLegProtection);
      if (!flag && __instance?.ActiveHealthController != null)
        ApplyDamageInfoPatch.DisarmAndKnockdownCheck(__instance, damageInfo, bodyPartType, partHit, KE, hasArmArmor);
      if (PluginConfig.EnableBallisticsLogging.Value)
      {
        ModulePatch.Logger.LogWarning((object) "==========Apply Damage Info=============== ");
        ModulePatch.Logger.LogWarning((object) ("Damage " + damageInfo.Damage.ToString()));
        ModulePatch.Logger.LogWarning((object) ("Pen " + damageInfo.PenetrationPower.ToString()));
        ModulePatch.Logger.LogWarning((object) "========================= ");
      }
    }
  }

  [PatchPostfix]
  private static void Postfix(Player __instance, ref DamageInfoStruct damageInfo)
  {
    int num;
    if (damageInfo.DamageType != 8192 /*0x2000*/)
    {
      MongoID? blockedBy = damageInfo.BlockedBy;
      num = !string.IsNullOrEmpty(blockedBy.HasValue ? MongoID.op_Implicit(blockedBy.GetValueOrDefault()) : (string) null) ? 1 : 0;
    }
    else
      num = 1;
    if (num == 0)
      return;
    damageInfo.BleedBlock = true;
    damageInfo.HeavyBleedingDelta = -1f;
    damageInfo.LightBleedingDelta = -1f;
  }
}
