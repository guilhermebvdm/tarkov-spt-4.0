// Decompiled with JetBrains decompiler
// Type: RealismMod.BallisticsController
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Comfort.Common;
using EFT;
using EFT.AssetsManager;
using EFT.InventoryLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

#nullable disable
namespace RealismMod;

public static class BallisticsController
{
  public static EBodyPartColliderType[] HeadCollidors;
  public static EBodyPartColliderType[] ArmCollidors;
  public static EBodyPartColliderType[] FaceSpallProtectionCollidors;
  public static EBodyPartColliderType[] LegSpallProtectionCollidors;
  private static List<EBodyPart> _spallingBodyParts;
  private static List<ArmorComponent> _preAllocatedArmorComponents;

  public static void CalcAfterPenStats(
    float actualDurability,
    float armorClass,
    float templateDurability,
    ref float damage,
    ref float penetration,
    float factor = 1f)
  {
    float num1 = (float) (1.0 - (double) armorClass / 80.0 * ((double) actualDurability / (double) templateDurability));
    float num2 = Mathf.Clamp(num1, 0.85f, 1f);
    float num3 = Mathf.Clamp(num1, 0.85f, 1f) * factor;
    damage *= num2;
    penetration *= num3;
  }

  public static void ModifyDamageByHitZone(
    float maxHP,
    EBodyPartColliderType hitPart,
    EBodyHitZone hitZone,
    ref DamageInfoStruct di)
  {
    switch (hitZone)
    {
      case EBodyHitZone.AZone:
        di.Damage *= 2.1f * PluginConfig.GlobalDamageModifier.Value;
        di.HeavyBleedingDelta *= 1.5f;
        di.LightBleedingDelta *= 2f;
        break;
      case EBodyHitZone.CZone:
        di.Damage *= PluginConfig.GlobalDamageModifier.Value;
        di.HeavyBleedingDelta *= 1f;
        di.LightBleedingDelta *= 1f;
        break;
      case EBodyHitZone.DZone:
        di.Damage *= 0.77f * PluginConfig.GlobalDamageModifier.Value;
        di.HeavyBleedingDelta *= 0.8f;
        di.LightBleedingDelta *= 0.5f;
        break;
      case EBodyHitZone.Heart:
        di.Damage = maxHP * PluginConfig.GlobalDamageModifier.Value;
        di.HeavyBleedingDelta *= 10f;
        di.LightBleedingDelta *= 10f;
        break;
      case EBodyHitZone.Spine:
        di.Damage = maxHP * 0.9f * PluginConfig.GlobalDamageModifier.Value;
        di.HeavyBleedingDelta *= 1.15f;
        di.LightBleedingDelta *= 1.5f;
        break;
      default:
        switch (hitPart - 1)
        {
          case 0:
          case 20:
            di.Damage *= 1f * PluginConfig.GlobalDamageModifier.Value;
            di.HeavyBleedingDelta *= 1.15f;
            di.LightBleedingDelta *= 1.15f;
            return;
          case 1:
            return;
          case 2:
          case 22:
            di.Damage *= 1.1f * PluginConfig.GlobalDamageModifier.Value;
            di.HeavyBleedingDelta *= 1.1f;
            di.LightBleedingDelta *= 1.1f;
            return;
          case 3:
          case 5:
            di.Damage *= 0.9f * PluginConfig.GlobalDamageModifier.Value;
            di.HeavyBleedingDelta *= 0.65f;
            di.LightBleedingDelta *= 0.65f;
            return;
          case 4:
          case 6:
            di.Damage *= 0.7f * PluginConfig.GlobalDamageModifier.Value;
            di.HeavyBleedingDelta *= 0.4f;
            di.LightBleedingDelta *= 0.5f;
            return;
          case 7:
          case 9:
            di.Damage *= 1.1f * PluginConfig.GlobalDamageModifier.Value;
            di.HeavyBleedingDelta *= 1.3f;
            di.LightBleedingDelta *= 1.3f;
            return;
          case 8:
          case 10:
            di.Damage *= 0.8f * PluginConfig.GlobalDamageModifier.Value;
            di.HeavyBleedingDelta *= 0.6f;
            di.LightBleedingDelta *= 0.7f;
            return;
          case 11:
            di.Damage *= 0.9f;
            di.HeavyBleedingDelta *= 0.9f;
            di.LightBleedingDelta *= 0.9f;
            return;
          case 12:
            di.Damage *= 1.1f;
            di.HeavyBleedingDelta *= 1.15f;
            di.LightBleedingDelta *= 1.15f;
            return;
          case 13:
            return;
          case 14:
            return;
          case 15:
            di.Damage *= 0.85f;
            di.HeavyBleedingDelta *= 0.95f;
            di.LightBleedingDelta *= 0.95f;
            return;
          case 16 /*0x10*/:
          case 17:
            di.Damage *= 0.85f;
            di.HeavyBleedingDelta *= 1.15f;
            di.LightBleedingDelta *= 1.15f;
            return;
          case 18:
          case 19:
            di.Damage *= 1.15f * PluginConfig.GlobalDamageModifier.Value;
            di.HeavyBleedingDelta *= 1.25f;
            di.LightBleedingDelta *= 1.25f;
            return;
          case 21:
          case 25:
            di.Damage *= 0.9f * PluginConfig.GlobalDamageModifier.Value;
            di.HeavyBleedingDelta *= 0.95f;
            di.LightBleedingDelta *= 0.95f;
            return;
          case 23:
          case 24:
            di.Damage *= 0.9f * PluginConfig.GlobalDamageModifier.Value;
            di.HeavyBleedingDelta *= 0.9f;
            di.LightBleedingDelta *= 0.9f;
            return;
          default:
            return;
        }
    }
  }

  private static float GetBleedFactor(EBodyPart part)
  {
    switch ((int) part)
    {
      case 0:
        return 0.4f;
      case 3:
      case 4:
        return 0.25f;
      case 5:
      case 6:
        return 0.5f;
      default:
        return 1f;
    }
  }

  public static void PlayBodyHitSound(EBodyPart part, Vector3 pos, int rndNum)
  {
    float num1 = CameraClass.Instance.Distance(pos);
    float num2 = 0.4f * PluginConfig.FleshHitSoundMulti.Value;
    float num3 = 2f * PluginConfig.FleshHitSoundMulti.Value;
    float num4 = 30f;
    if (part == 0)
    {
      Singleton<BetterAudio>.Instance.PlayAtPoint(pos, Plugin.RealismAudioController.HitAudioClips["headshot.wav"], num1, (BetterAudio.AudioSourceGroupType) 2, 100, num2 * 0.6f, (EOcclusionTest) 2, (AudioMixerGroup) null, false);
    }
    else
    {
      string key;
      if ((double) num1 >= (double) num4)
      {
        string str;
        switch (rndNum)
        {
          case 0:
            str = "flesh_dist_1.wav";
            break;
          case 1:
            str = "flesh_dist_2.wav";
            break;
          default:
            str = "flesh_dist_2.wav";
            break;
        }
        key = str;
      }
      else
      {
        string str;
        switch (rndNum)
        {
          case 0:
            str = "flesh_1.wav";
            break;
          case 1:
            str = "flesh_2.wav";
            break;
          default:
            str = "flesh_3.wav";
            break;
        }
        key = str;
      }
      Singleton<BetterAudio>.Instance.PlayAtPoint(pos, Plugin.RealismAudioController.HitAudioClips[key], num1, (BetterAudio.AudioSourceGroupType) 2, 100, (double) num1 >= (double) num4 ? num3 : num2, (EOcclusionTest) 2, (AudioMixerGroup) null, false);
    }
  }

  public static EBodyHitZone GetBodyHitZone(
    Player player,
    EBodyPartColliderType partHit,
    DamageInfoStruct damageInfo)
  {
    EBodyHitZone bodyHitZone = EBodyHitZone.Unknown;
    if (partHit == 1 || partHit == 26 || partHit == 22 || partHit == 21)
    {
      Collider collider1 = damageInfo.HitCollider;
      if (Object.op_Equality((Object) damageInfo.HitCollider, (Object) null))
      {
        List<Collider> colliders = ((AssetPoolObject) ((Component) player).GetComponent<PlayerPoolObject>()).Colliders;
        if (colliders == null || colliders.Count <= 0)
          return bodyHitZone;
        int count = colliders.Count;
        for (int index = 0; index < count; ++index)
        {
          Collider collider2 = colliders[index];
          BodyPartCollider component = ((Component) collider2).GetComponent<BodyPartCollider>();
          if (Object.op_Inequality((Object) component, (Object) null) && component.BodyPartColliderType == partHit)
          {
            Utils.Logger.LogWarning((object) component.BodyPartColliderType);
            collider1 = collider2;
            break;
          }
        }
        if (Object.op_Equality((Object) collider1, (Object) null))
          return bodyHitZone;
      }
      Vector3 localPoint = ((Component) collider1).transform.InverseTransformPoint(damageInfo.HitPoint);
      EHitOrientation hitOrientation = HitZones.GetHitOrientation(damageInfo.HitNormal, ((Component) collider1).transform);
      bodyHitZone = HitZones.GetHitBodyZone(localPoint, hitOrientation, partHit);
      if (PluginConfig.EnableBallisticsLogging.Value)
      {
        Utils.Logger.LogWarning((object) "=========Hitzone Damage Info==========");
        Utils.Logger.LogWarning((object) ("hit collider = " + partHit.ToString()));
        Utils.Logger.LogWarning((object) ("hit orientation = " + hitOrientation.ToString()));
        Utils.Logger.LogWarning((object) ("hit zone = " + bodyHitZone.ToString()));
        Utils.Logger.LogWarning((object) ("damage before = " + damageInfo.Damage.ToString()));
        Utils.Logger.LogWarning((object) ("x = " + localPoint.x.ToString()));
        Utils.Logger.LogWarning((object) ("y = " + localPoint.y.ToString()));
        Utils.Logger.LogWarning((object) ("z = " + localPoint.z.ToString()));
        Utils.Logger.LogWarning((object) "===================");
      }
    }
    return bodyHitZone;
  }

  public static void ModifyDamageByZone(
    Player player,
    EBodyPartColliderType partHit,
    float maxHp,
    bool isBluntDamage,
    ref DamageInfoStruct damageInfo)
  {
    if (isBluntDamage)
      return;
    EBodyHitZone bodyHitZone = BallisticsController.GetBodyHitZone(player, partHit, damageInfo);
    BallisticsController.ModifyDamageByHitZone(maxHp, partHit, bodyHitZone, ref damageInfo);
  }

  public static bool ShouldDoSpalling(
    bool isBuckshot,
    AmmoTemplate ammoTemp,
    DamageInfoStruct damageInfo,
    EBodyPart bodyPartType,
    bool isBluntDamage)
  {
    return !isBuckshot && ammoTemp != null && damageInfo.DamageType != 1024 /*0x0400*/ && isBluntDamage && (bodyPartType == 1 || bodyPartType == 2);
  }

  public static void GetKineticEnergy(
    DamageInfoStruct damageInfo,
    AmmoTemplate ammoTemp,
    ref float KE)
  {
    if (damageInfo.DamageType == 1024 /*0x0400*/)
    {
      Weapon weapon = damageInfo.Weapon as Weapon;
      float num = !damageInfo.Player.IsAI && WeaponStats.HasBayonet && weapon.WeapClass != "Knife" ? damageInfo.Damage : damageInfo.Damage * 2f;
      KE = num * 50f;
    }
    else
      KE = (double) damageInfo.ArmorDamage > 1.0 ? (float) (0.5 * (double) ammoTemp.BulletMassGram * (double) damageInfo.ArmorDamage * (double) damageInfo.ArmorDamage / 1000.0) : (float) (0.5 * (double) ammoTemp.BulletMassGram * (double) ammoTemp.InitialSpeed * (double) ammoTemp.InitialSpeed / 1000.0);
  }

  public static bool ArmorHasSpecificColliders(
    ArmorComponent armorComp,
    EBodyPartColliderType[] colliders)
  {
    bool flag = false;
    EBodyPartColliderType[] armorColliders = armorComp.Template.ArmorColliders;
    int num = ((IEnumerable<EBodyPartColliderType>) armorColliders).Count<EBodyPartColliderType>();
    for (int index = 0; index < num; ++index)
    {
      if (((IEnumerable<EBodyPartColliderType>) colliders).Contains<EBodyPartColliderType>((EBodyPartColliderType) (int) armorColliders[index]))
      {
        flag = true;
        break;
      }
    }
    return flag;
  }

  public static int ArmorColliderCount(ArmorComponent armorComp, EBodyPartColliderType[] colliders)
  {
    int num1 = 0;
    EBodyPartColliderType[] armorColliders = armorComp.Template.ArmorColliders;
    int num2 = ((IEnumerable<EBodyPartColliderType>) armorColliders).Count<EBodyPartColliderType>();
    for (int index = 0; index < num2; ++index)
    {
      if (((IEnumerable<EBodyPartColliderType>) colliders).Contains<EBodyPartColliderType>((EBodyPartColliderType) (int) armorColliders[index]))
        ++num1;
    }
    return num1;
  }

  public static void GetArmorComponents(
    Player player,
    DamageInfoStruct damageInfo,
    EBodyPart bodyPartType,
    ref ArmorComponent armor,
    ref int faceProtectionCount,
    ref bool doSpalling,
    ref bool hasArmArmor,
    ref bool hasLegProtection)
  {
    BallisticsController._preAllocatedArmorComponents.Clear();
    player.Inventory.GetPutOnArmorsNonAlloc(BallisticsController._preAllocatedArmorComponents);
    foreach (ArmorComponent allocatedArmorComponent in BallisticsController._preAllocatedArmorComponents)
    {
      MongoID? nullable1 = MongoID.op_Implicit(((GClass3175) allocatedArmorComponent).Item.Id);
      MongoID? nullable2 = damageInfo.BlockedBy;
      int num;
      if ((nullable1.HasValue == nullable2.HasValue ? (nullable1.HasValue ? (MongoID.op_Equality(nullable1.GetValueOrDefault(), nullable2.GetValueOrDefault()) ? 1 : 0) : 1) : 0) == 0)
      {
        nullable2 = MongoID.op_Implicit(((GClass3175) allocatedArmorComponent).Item.Id);
        nullable1 = damageInfo.DeflectedBy;
        num = nullable2.HasValue == nullable1.HasValue ? (nullable2.HasValue ? (MongoID.op_Equality(nullable2.GetValueOrDefault(), nullable1.GetValueOrDefault()) ? 1 : 0) : 1) : 0;
      }
      else
        num = 1;
      if (num != 0)
      {
        armor = allocatedArmorComponent;
        if (!doSpalling && bodyPartType != 3 && bodyPartType != 4)
          break;
      }
      if (doSpalling || bodyPartType == 3 || bodyPartType == 4)
      {
        if (BallisticsController.ArmorHasSpecificColliders(allocatedArmorComponent, BallisticsController.ArmCollidors))
          hasArmArmor = true;
        if (!doSpalling)
          break;
      }
      if (doSpalling)
      {
        if (BallisticsController.ArmorHasSpecificColliders(allocatedArmorComponent, BallisticsController.LegSpallProtectionCollidors))
          hasLegProtection = true;
        faceProtectionCount += BallisticsController.ArmorColliderCount(allocatedArmorComponent, BallisticsController.FaceSpallProtectionCollidors);
      }
    }
    BallisticsController._preAllocatedArmorComponents.Clear();
  }

  public static void CalculatSpalling(
    Player player,
    ref DamageInfoStruct damageInfo,
    float KE,
    ArmorComponent armor,
    AmmoTemplate ammoTemp,
    int faceProtectionCount,
    bool hasArmArmor,
    bool hasLegProtection)
  {
    Gear dataObj = TemplateStats.GetDataObj<Gear>(TemplateStats.GearStats, MongoID.op_Implicit(((GClass3175) armor).Item.TemplateId));
    bool flag = armor.Template.ArmorMaterial == 5 || armor.Template.ArmorMaterial == 3;
    float damage = damageInfo.Damage;
    float num1 = damageInfo.ArmorDamage / ammoTemp.InitialSpeed;
    float num2 = ammoTemp.FragmentationChance * num1;
    float num3 = 0.8f;
    float num4 = 0.25f;
    float num5 = ammoTemp.RicochetChance * num1;
    float spallReduction = dataObj.SpallReduction;
    float num6 = (float) ammoTemp.ArmorDamage * num1;
    float penetrationPower = damageInfo.PenetrationPower;
    float num7 = armor.Repairable.Durability / (float) armor.Repairable.TemplateDurability;
    float num8 = Mathf.Min((float) armor.Repairable.TemplateDurability / Mathf.Max(armor.Repairable.Durability, 1f), 2f);
    float num9 = 10f + Mathf.Max(1f, (float) armor.ArmorClass * 10f * num7 - penetrationPower / 1.8f);
    float num10 = KE / num9;
    float num11 = (float) ((double) num10 * ((double) num2 + 1.0) * ((double) num5 + 1.0) * (double) spallReduction * (flag ? 1.0 - (double) num7 + 1.0 : 1.0));
    float num12 = Mathf.Clamp(num11 - damage, 7f, 35f * num8);
    float num13 = num12 / (float) BallisticsController._spallingBodyParts.Count;
    damageInfo.BleedBlock = false;
    if (PluginConfig.EnableBallisticsLogging.Value)
    {
      Utils.Logger.LogWarning((object) "===========SPALLING=============== ");
      Utils.Logger.LogWarning((object) ("Spall Reduction " + spallReduction.ToString()));
      Utils.Logger.LogWarning((object) ("Dura Percent " + num7.ToString()));
      Utils.Logger.LogWarning((object) ("Armor factorPercent " + num7.ToString()));
      Utils.Logger.LogWarning((object) ("Max Dura Factored Damage " + num10.ToString()));
      Utils.Logger.LogWarning((object) ("Factored Spalling Damage " + num11.ToString()));
      Utils.Logger.LogWarning((object) ("Max Spalling Damage " + num12.ToString()));
      Utils.Logger.LogWarning((object) ("Split Spalling Dmg " + num13.ToString()));
    }
    int count = Mathf.Max(1, Random.Range(1, BallisticsController._spallingBodyParts.Count + 1));
    foreach (EBodyPart part in BallisticsController._spallingBodyParts.OrderBy<EBodyPart, float>((Func<EBodyPart, float>) (x => Random.value)).Take<EBodyPart>(count))
    {
      if (part == 7)
        break;
      float num14 = num13;
      float bleedFactor = BallisticsController.GetBleedFactor(part);
      if (part == 0)
      {
        float num15 = Mathf.Clamp((float) (1.0 - (double) faceProtectionCount / 10.0), 0.1f, 1f);
        num14 = Mathf.Min(10f, num13 * num15);
        bleedFactor *= num15;
      }
      if (((part == 3 ? 1 : (part == 4 ? 1 : 0)) & (hasArmArmor ? 1 : 0)) != 0)
      {
        num14 *= 0.5f;
        bleedFactor *= 0.25f;
      }
      if (((part == 5 ? 1 : (part == 6 ? 1 : 0)) & (hasLegProtection ? 1 : 0)) != 0)
      {
        num14 *= 0.5f;
        bleedFactor *= 0.25f;
      }
      if (PluginConfig.EnableBallisticsLogging.Value)
      {
        Utils.Logger.LogWarning((object) "==== ");
        Utils.Logger.LogWarning((object) ("Part Hit " + part.ToString()));
        Utils.Logger.LogWarning((object) ("Damage " + num14.ToString()));
        Utils.Logger.LogWarning((object) "=== ");
      }
      damageInfo.HeavyBleedingDelta = num4 * bleedFactor;
      damageInfo.LightBleedingDelta = num3 * bleedFactor;
      double num16 = (double) player.ActiveHealthController.ApplyDamage(part, num14, damageInfo);
    }
  }

  private static void ModifyPlateHelper(
    Collider collider,
    BoxCollider boxCollider,
    string colliderName,
    string target,
    float x,
    float y,
    float z)
  {
    if (!colliderName.Contains(target))
      return;
    float num1 = boxCollider.size.x * x;
    float num2 = boxCollider.size.y * y;
    float num3 = boxCollider.size.z * z;
    boxCollider.size = new Vector3(num1, num2, num3);
    if (PluginConfig.EnableBallisticsLogging.Value)
      DebugGizmos.SingleObjects.VisualizeBoxCollider(boxCollider, ((Object) collider).name);
  }

  public static void ModifyPlateColliders(Player player)
  {
    List<Collider> colliders = ((AssetPoolObject) ((Component) player).GetComponent<PlayerPoolObject>()).Colliders;
    if (colliders == null || colliders.Count <= 0)
      return;
    int count = colliders.Count;
    for (int index = 0; index < count; ++index)
    {
      Collider collider = colliders[index];
      BoxCollider boxCollider1 = collider as BoxCollider;
      if (Object.op_Inequality((Object) boxCollider1, (Object) null))
      {
        string lower = ((Object) collider).name.ToLower();
        if (lower == "left" || lower == "right" || lower == "top")
        {
          BoxCollider boxCollider2 = boxCollider1;
          boxCollider2.size = Vector3.op_Multiply(boxCollider2.size, 0.0f);
        }
        BallisticsController.ModifyPlateHelper(collider, boxCollider1, lower, "_chest", 0.985f, 0.475f, 0.87f);
        BallisticsController.ModifyPlateHelper(collider, boxCollider1, lower, "_back", 0.78f, 0.58f, 0.84f);
        BallisticsController.ModifyPlateHelper(collider, boxCollider1, lower, "_side_", 0.82f, 1f, 0.7f);
        BallisticsController.ModifyPlateHelper(collider, boxCollider1, lower, "chesttop", 1.55f, 0.9f, 1f);
      }
    }
  }

  static BallisticsController()
  {
    // ISSUE: unable to decompile the method.
  }
}
