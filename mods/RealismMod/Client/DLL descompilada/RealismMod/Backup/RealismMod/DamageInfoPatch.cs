// Decompiled with JetBrains decompiler
// Type: RealismMod.DamageInfoPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using SPT.Reflection.Patching;
using System;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class DamageInfoPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (DamageInfoStruct).GetConstructor(new Type[2]
    {
      typeof (EDamageType),
      typeof (EftBulletClass)
    });
  }

  [PatchPrefix]
  private static bool Prefix(
    ref DamageInfoStruct __instance,
    EDamageType damageType,
    EftBulletClass shot)
  {
    __instance.DamageType = damageType;
    __instance.Damage = shot.Damage;
    __instance.PenetrationPower = shot.PenetrationPower;
    __instance.HitCollider = shot.HitCollider;
    __instance.Direction = shot.Direction;
    __instance.HitPoint = shot.HitPoint;
    __instance.HitNormal = shot.HitNormal;
    __instance.HittedBallisticCollider = shot.HittedBallisticCollider;
    __instance.Player = shot.Player;
    __instance.Weapon = shot.Weapon;
    __instance.FireIndex = shot.FireIndex;
    __instance.DelayedDamage = shot.DelayedDamage;
    __instance.DeflectedBy = shot.DeflectedBy;
    __instance.BlockedBy = shot.BlockedBy;
    __instance.MasterOrigin = shot.MasterOrigin;
    __instance.IsForwardHit = shot.IsForwardHit;
    __instance.SourceId = MongoID.op_Implicit(shot.Ammo.TemplateId);
    int num;
    if (__instance.DamageType != 8192 /*0x2000*/ && __instance.DamageType != 512 /*0x0200*/)
    {
      MongoID? blockedBy = __instance.BlockedBy;
      num = !string.IsNullOrEmpty(blockedBy.HasValue ? MongoID.op_Implicit(blockedBy.GetValueOrDefault()) : (string) null) ? 1 : 0;
    }
    else
      num = 1;
    if (num != 0)
    {
      if (PluginConfig.EnableGeneralLogging.Value)
        ModulePatch.Logger.LogWarning((object) ("================shot velocity============== " + shot.VelocityMagnitude.ToString()));
      __instance.ArmorDamage = shot.VelocityMagnitude;
    }
    else
      __instance.ArmorDamage = shot.ArmorDamage;
    if (shot.Ammo is AmmoItemClass ammo)
    {
      __instance.StaminaBurnRate = ammo.StaminaBurnRate;
      __instance.HeavyBleedingDelta = ammo.HeavyBleedingDelta;
      __instance.LightBleedingDelta = ammo.LightBleedingDelta;
    }
    else
    {
      __instance.LightBleedingDelta = 0.0f;
      __instance.HeavyBleedingDelta = 0.0f;
      __instance.StaminaBurnRate = 0.0f;
      if (__instance.Weapon is KnifeItemClass weapon)
        __instance.StaminaBurnRate = weapon.KnifeComponent.Template.StaminaBurnRate;
    }
    __instance.DidBodyDamage = 0.0f;
    __instance.DidArmorDamage = 0.0f;
    __instance.OverDamageFrom = new EBodyPart?();
    __instance.BodyPartColliderType = (EBodyPartColliderType) -1;
    __instance.BleedBlock = false;
    return false;
  }
}
