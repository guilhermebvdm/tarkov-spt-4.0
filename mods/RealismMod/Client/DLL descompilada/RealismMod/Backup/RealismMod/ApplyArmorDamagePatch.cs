// Decompiled with JetBrains decompiler
// Type: RealismMod.ApplyArmorDamagePatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;

#nullable disable
namespace RealismMod;

public class ApplyArmorDamagePatch : ModulePatch
{
  private static void playRicochetSound(Vector3 pos, int rndNum)
  {
    float num = CameraClass.Instance.Distance(pos);
    string str;
    switch (rndNum)
    {
      case 0:
        str = "ric_1.wav";
        break;
      case 1:
        str = "ric_2.wav";
        break;
      default:
        str = "ric_3.wav";
        break;
    }
    string key = str;
    Singleton<BetterAudio>.Instance.PlayAtPoint(pos, Plugin.RealismAudioController.HitAudioClips[key], num, (BetterAudio.AudioSourceGroupType) 2, 40, 4.25f, (EOcclusionTest) 2, (AudioMixerGroup) null, false);
  }

  private static void playArmorHitSound(EArmorMaterial mat, Vector3 pos, bool isHelm, int rndNum)
  {
    float num1 = CameraClass.Instance.Distance(pos);
    float num2 = 0.5f * PluginConfig.ArmorCloseHitSoundMulti.Value;
    float num3 = 3f * PluginConfig.ArmorFarHitSoundMulti.Value;
    float num4 = 30f;
    if (mat == 1)
    {
      string key;
      if ((double) num1 >= (double) num4)
      {
        string str;
        switch (rndNum)
        {
          case 0:
            str = "impact_dist_1.wav";
            break;
          case 1:
            str = "impact_dist_2.wav";
            break;
          default:
            str = "impact_dist_3.wav";
            break;
        }
        key = str;
      }
      else if (!isHelm)
      {
        string str;
        switch (rndNum)
        {
          case 0:
            str = "aramid_1.wav";
            break;
          case 1:
            str = "aramid_2.wav";
            break;
          default:
            str = "aramid_3.wav";
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
            str = "uhmwpe_1.wav";
            break;
          case 1:
            str = "uhmwpe_2.wav";
            break;
          default:
            str = "uhmwpe_3.wav";
            break;
        }
        key = str;
      }
      Singleton<BetterAudio>.Instance.PlayAtPoint(pos, Plugin.RealismAudioController.HitAudioClips[key], num1, (BetterAudio.AudioSourceGroupType) 2, 100, (double) num1 >= (double) num4 ? num3 : num2, (EOcclusionTest) 2, (AudioMixerGroup) null, false);
    }
    else if (mat == 6)
    {
      string key;
      if ((double) num1 >= (double) num4)
      {
        string str;
        switch (rndNum)
        {
          case 0:
            str = "ceramic_dist_1.wav";
            break;
          case 1:
            str = "ceramic_dist_2.wav";
            break;
          default:
            str = "ceramic_dist_3.wav";
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
            str = "ceramic_1.wav";
            break;
          case 1:
            str = "ceramic_2.wav";
            break;
          default:
            str = "ceramic_3.wav";
            break;
        }
        key = str;
      }
      Singleton<BetterAudio>.Instance.PlayAtPoint(pos, Plugin.RealismAudioController.HitAudioClips[key], num1, (BetterAudio.AudioSourceGroupType) 2, 100, (double) num1 >= (double) num4 ? num3 * 1.5f : num2 * 1.5f, (EOcclusionTest) 2, (AudioMixerGroup) null, false);
    }
    else if (mat == null || mat == 2)
    {
      string key;
      if ((double) num1 >= (double) num4)
      {
        string str;
        switch (rndNum)
        {
          case 0:
            str = "impact_dist_1.wav";
            break;
          case 1:
            str = "impact_dist_2.wav";
            break;
          default:
            str = "impact_dist_4.wav";
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
            str = "uhmwpe_1.wav";
            break;
          case 1:
            str = "uhmwpe_2.wav";
            break;
          default:
            str = "uhmwpe_3.wav";
            break;
        }
        key = str;
      }
      Singleton<BetterAudio>.Instance.PlayAtPoint(pos, Plugin.RealismAudioController.HitAudioClips[key], num1, (BetterAudio.AudioSourceGroupType) 2, 100, (double) num1 >= (double) num4 ? num3 : num2, (EOcclusionTest) 2, (AudioMixerGroup) null, false);
    }
    else if (mat == 3 || mat == 5)
    {
      string key;
      if ((double) num1 >= (double) num4)
      {
        string str;
        switch (rndNum)
        {
          case 0:
            str = "metal_dist_1.wav";
            break;
          case 1:
            str = "metal_dist_2.wav";
            break;
          default:
            str = "metal_dist_3.wav";
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
            str = "metal_1.wav";
            break;
          case 1:
            str = "metal_2.wav";
            break;
          default:
            str = "metal_3.wav";
            break;
        }
        key = str;
      }
      Singleton<BetterAudio>.Instance.PlayAtPoint(pos, Plugin.RealismAudioController.HitAudioClips[key], num1, (BetterAudio.AudioSourceGroupType) 2, 100, (double) num1 >= (double) num4 ? num3 * 0.75f : num2 * 0.75f, (EOcclusionTest) 2, (AudioMixerGroup) null, false);
    }
    else if (mat == 7)
    {
      string key;
      if ((double) num1 >= (double) num4)
      {
        string str;
        switch (rndNum)
        {
          case 0:
            str = "impact_dist_3.wav";
            break;
          case 1:
            str = "impact_dist_4.wav";
            break;
          default:
            str = "impact_dist_2.wav";
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
            str = "glass_1.wav";
            break;
          case 1:
            str = "glass_2.wav";
            break;
          default:
            str = "glass_3.wav";
            break;
        }
        key = str;
      }
      Singleton<BetterAudio>.Instance.PlayAtPoint(pos, Plugin.RealismAudioController.HitAudioClips[key], num1, (BetterAudio.AudioSourceGroupType) 2, 100, (double) num1 >= (double) num4 ? num3 : num2, (EOcclusionTest) 2, (AudioMixerGroup) null, false);
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
            str = "impact_dist_4.wav";
            break;
          case 1:
            str = "impact_dist_1.wav";
            break;
          default:
            str = "impact_dist_3.wav";
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
            str = "impact_1.wav";
            break;
          case 1:
            str = "impact_2.wav";
            break;
          default:
            str = "impact_3.wav";
            break;
        }
        key = str;
      }
      Singleton<BetterAudio>.Instance.PlayAtPoint(pos, Plugin.RealismAudioController.HitAudioClips[key], num1, (BetterAudio.AudioSourceGroupType) 2, 100, (double) num1 >= (double) num4 ? num3 : num2, (EOcclusionTest) 2, (AudioMixerGroup) null, false);
    }
  }

  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (ArmorComponent).GetMethod("ApplyDamage", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Prefix(
    ArmorComponent __instance,
    ref DamageInfoStruct damageInfo,
    ref float __result,
    EBodyPartColliderType colliderType,
    EArmorPlateCollider armorPlateCollider,
    bool damageInfoIsLocal,
    List<ArmorComponent> armorComponents)
  {
    if (PluginConfig.EnableBallisticsLogging.Value)
    {
      ModulePatch.Logger.LogWarning((object) "===========ARMOR DAMAGE BEFORE=============== ");
      ModulePatch.Logger.LogWarning((object) ("Pen " + damageInfo.PenetrationPower.ToString()));
      ModulePatch.Logger.LogWarning((object) ("Damage " + damageInfo.Damage.ToString()));
      ModulePatch.Logger.LogWarning((object) "========================== ");
    }
    Player playerByProfileId = damageInfo.Player != null ? Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(damageInfo.Player.iPlayer.ProfileId) : (Player) null;
    if (Utils.IsZombie(playerByProfileId))
      return true;
    EDamageType damageType = damageInfo.DamageType;
    if (damageType == 4096 /*0x1000*/ || damageType == 2048 /*0x0800*/)
      return true;
    if (!GClass2855.IsWeaponInduced(damageType) && damageType != 32 /*0x20*/)
    {
      __result = 0.0f;
      return false;
    }
    if (Object.op_Inequality((Object) playerByProfileId, (Object) null))
      __instance.TryShatter(playerByProfileId, damageInfoIsLocal);
    bool isHelm = BallisticsController.ArmorHasSpecificColliders(__instance, BallisticsController.HeadCollidors);
    bool flag1 = __instance.Template.ArmorMaterial == 5 && !isHelm;
    MongoID? nullable1 = damageInfo.BlockedBy;
    MongoID? nullable2 = MongoID.op_Implicit(((GClass3175) __instance).Item.Id);
    int num1;
    if ((nullable1.HasValue == nullable2.HasValue ? (nullable1.HasValue ? (MongoID.op_Inequality(nullable1.GetValueOrDefault(), nullable2.GetValueOrDefault()) ? 1 : 0) : 0) : 1) != 0)
    {
      nullable2 = damageInfo.DeflectedBy;
      nullable1 = MongoID.op_Implicit(((GClass3175) __instance).Item.Id);
      num1 = nullable2.HasValue == nullable1.HasValue ? (nullable2.HasValue ? (MongoID.op_Inequality(nullable2.GetValueOrDefault(), nullable1.GetValueOrDefault()) ? 1 : 0) : 0) : 1;
    }
    else
      num1 = 0;
    bool flag2 = num1 != 0;
    float damage = damageInfo.Damage;
    float num2 = 1f;
    if ((double) __instance.Repairable.Durability <= 0.0 && !flag1)
    {
      __result = 0.0f;
      return false;
    }
    AmmoTemplate itemTemplate = damageType == 1024 /*0x0400*/ ? (AmmoTemplate) null : Utils.GetItemTemplate<AmmoTemplate>(MongoID.op_Implicit(damageInfo.SourceId));
    float armorDamage;
    float num3;
    if (damageType == 1024 /*0x0400*/)
    {
      Weapon weapon = damageInfo.Weapon as Weapon;
      bool flag3 = !damageInfo.Player.IsAI && WeaponStats.HasBayonet && weapon.WeapClass != "Knife";
      armorDamage = damageInfo.ArmorDamage;
      num3 = (flag3 ? damageInfo.Damage : damageInfo.Damage * 2f) * 100f;
    }
    else
    {
      armorDamage = (float) itemTemplate.ArmorDamage;
      if ((double) damageInfo.ArmorDamage <= 1.0)
      {
        num3 = itemTemplate.BulletMassGram * itemTemplate.InitialSpeed;
      }
      else
      {
        num2 = damageInfo.ArmorDamage / itemTemplate.InitialSpeed;
        num3 = itemTemplate.BulletMassGram * damageInfo.ArmorDamage;
      }
    }
    if (itemTemplate != null && itemTemplate.ProjectileCount < 2 && PluginConfig.EnableHitSounds.Value && Object.op_Inequality((Object) damageInfo.HittedBallisticCollider, (Object) null) && !(((IContainer) ((GClass3175) __instance).Item.Owner).ID == Singleton<GameWorld>.Instance.MainPlayer.ProfileId))
    {
      nullable1 = damageInfo.DeflectedBy;
      nullable2 = MongoID.op_Implicit(((GClass3175) __instance).Item.Id);
      if (nullable1.HasValue == nullable2.HasValue && (!nullable1.HasValue || MongoID.op_Equality(nullable1.GetValueOrDefault(), nullable2.GetValueOrDefault())))
        ApplyArmorDamagePatch.playRicochetSound(((Component) damageInfo.HittedBallisticCollider).transform.position, Random.Range(0, 2));
      else
        ApplyArmorDamagePatch.playArmorHitSound(__instance.Template.ArmorMaterial, ((Component) damageInfo.HittedBallisticCollider).transform.position, isHelm, Random.Range(0, 2));
    }
    nullable2 = damageInfo.DeflectedBy;
    nullable1 = MongoID.op_Implicit(((GClass3175) __instance).Item.Id);
    if (nullable2.HasValue == nullable1.HasValue && (!nullable2.HasValue || MongoID.op_Equality(nullable2.GetValueOrDefault(), nullable1.GetValueOrDefault())))
    {
      num3 *= 0.25f;
      armorDamage *= 0.25f;
      damageInfo.ArmorDamage *= 0.25f;
      damageInfo.PenetrationPower *= 0.25f;
    }
    float bluntThroughput = __instance.Template.BluntThroughput;
    float factor = 1f;
    if (((GClass3175) __instance).Item.CurrentAddress is GClass3184 currentAddress && currentAddress.Slot is GClass2929 slot && ((Slot) slot).BluntDamageReduceFromSoftArmor)
    {
      bluntThroughput *= 0.6f;
      factor = 0.7f;
    }
    float num4 = damageInfo.PenetrationPower / 1.8f;
    float num5 = Mathf.Exp(Mathf.Log10(num3) / 5f * 12f) / 100f;
    float num6 = __instance.Repairable.Durability / (float) __instance.Repairable.TemplateDurability;
    float num7 = (float) (__instance.ArmorClass * __instance.ArmorClass);
    float num8 = Singleton<BackendConfigSettingsClass>.Instance.ArmorMaterials[__instance.Template.ArmorMaterial].Destructibility;
    if (((__instance.Template.ArmorMaterial == 5 ? 1 : (__instance.Template.ArmorMaterial == 3 ? 1 : 0)) & (isHelm ? 1 : 0)) != 0)
      num8 = 0.25f;
    float num9 = Mathf.Clamp(num7 * Mathf.Pow(num6, 0.5f), 10f, num7);
    float num10 = Mathf.Clamp(1f - Mathf.InverseLerp(1f, 100f, num9), 0.1f, 1f);
    float num11 = Mathf.Clamp(1f - Mathf.InverseLerp(1f, 100f, num9 - num4), 0.1f, 1f);
    float num12 = Mathf.Clamp(1f - Mathf.InverseLerp(1f, 100f, num7 - num4), 0.1f, 1f);
    float num13 = __instance.Template.ArmorMaterial != 5 || isHelm ? Mathf.Clamp(num5 * num11 * bluntThroughput, 0.4f, damageInfo.Damage) : Mathf.Clamp(num5 * num12 * bluntThroughput, 0.4f, damageInfo.Damage);
    float num14 = num5 * num8 * armorDamage * num10;
    if (damageType == 1024 /*0x0400*/)
    {
      if ((double) damageInfo.PenetrationPower > (double) __instance.ArmorClass * 100.0 * (double) num6)
      {
        if (PluginConfig.EnableBallisticsLogging.Value)
          ModulePatch.Logger.LogWarning((object) "Melee Penetrated");
        damageInfo.Damage *= 0.75f;
        damageInfo.PenetrationPower *= 0.75f * factor;
      }
      else
      {
        if (PluginConfig.EnableBallisticsLogging.Value)
          ModulePatch.Logger.LogWarning((object) "Melee Blocked");
        damageInfo.Damage = isHelm ? num13 : num13 + damageInfo.Damage / 10f;
        damageInfo.BleedBlock = true;
        damageInfo.HeavyBleedingDelta = -1f;
        damageInfo.LightBleedingDelta = -1f;
      }
    }
    else if (flag2)
    {
      if (armorPlateCollider > 0)
        damageInfo.Damage = 0.0f;
      BallisticsController.CalcAfterPenStats(Mathf.Max(__instance.Repairable.Durability - num14, 1f), (float) __instance.ArmorClass, (float) __instance.Repairable.TemplateDurability, ref damageInfo.Damage, ref damageInfo.PenetrationPower, factor);
    }
    else
      damageInfo.Damage = num13;
    if (!flag2)
    {
      damageInfo.BleedBlock = true;
      damageInfo.HeavyBleedingDelta = -1f;
      damageInfo.LightBleedingDelta = -1f;
    }
    damageInfo.StaminaBurnRate = (float) ((double) num13 / 100.0 * 2.0);
    float num15 = Math.Max(num14 * PluginConfig.ArmorDurabilityModifier.Value, 0.1f);
    __instance.ApplyDurabilityDamage(num15, armorComponents);
    __result = num15;
    damageInfo.Damage = Mathf.Min(damageInfo.Damage, damage);
    if (PluginConfig.EnableBallisticsLogging.Value)
    {
      ModulePatch.Logger.LogWarning((object) "===========ARMOR DAMAGE AFTER=============== ");
      ModulePatch.Logger.LogWarning((object) ("Momentum " + num3.ToString()));
      ModulePatch.Logger.LogWarning((object) ("Momentum Factor " + num5.ToString()));
      ModulePatch.Logger.LogWarning((object) ("Pen " + damageInfo.PenetrationPower.ToString()));
      ModulePatch.Logger.LogWarning((object) ("Armor Damage " + armorDamage.ToString()));
      ModulePatch.Logger.LogWarning((object) ("Speed Factor " + num2.ToString()));
      ModulePatch.Logger.LogWarning((object) ("Class " + __instance.ArmorClass.ToString()));
      ModulePatch.Logger.LogWarning((object) ("Throughput " + bluntThroughput.ToString()));
      ModulePatch.Logger.LogWarning((object) ("Material " + __instance.Template.ArmorMaterial.ToString()));
      ModulePatch.Logger.LogWarning((object) ("Material Descructibility " + num8.ToString()));
      ModulePatch.Logger.LogWarning((object) ("Dura percent " + num6.ToString()));
      ModulePatch.Logger.LogWarning((object) ("armor Factor Damage = " + num11.ToString()));
      ModulePatch.Logger.LogWarning((object) ("armor Factor Dura = " + num10.ToString()));
      ModulePatch.Logger.LogWarning((object) ("Durability Loss " + num15.ToString()));
      ModulePatch.Logger.LogWarning((object) ("Damage " + damageInfo.Damage.ToString()));
      ModulePatch.Logger.LogWarning((object) "========================== ");
    }
    return false;
  }
}
