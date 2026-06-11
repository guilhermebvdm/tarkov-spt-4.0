// Decompiled with JetBrains decompiler
// Type: RealismMod.ApplyCorpseImpulsePatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class ApplyCorpseImpulsePatch : ModulePatch
{
  private static FieldInfo lastDamField;
  private static FieldInfo corpseField;
  private static FieldInfo corpseAppliedForceField;

  protected virtual MethodBase GetTargetMethod()
  {
    ApplyCorpseImpulsePatch.lastDamField = AccessTools.Field(typeof (Player), "LastDamageInfo");
    ApplyCorpseImpulsePatch.corpseField = AccessTools.Field(typeof (Player), "Corpse");
    ApplyCorpseImpulsePatch.corpseAppliedForceField = AccessTools.Field(typeof (Player), "_corpseAppliedForce");
    return (MethodBase) typeof (Player).GetMethod("ApplyCorpseImpulse", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Prefix(Player __instance)
  {
    if (Utils.IsZombie(__instance))
      return true;
    DamageInfoStruct damageInfoStruct = (DamageInfoStruct) ApplyCorpseImpulsePatch.lastDamField.GetValue((object) __instance);
    Corpse corpse = (Corpse) ApplyCorpseImpulsePatch.corpseField.GetValue((object) __instance);
    float num = damageInfoStruct.DamageType != 512 /*0x0200*/ ? (damageInfoStruct.DamageType != 4 ? 5f : 150f) : -Mathf.Max(1f, (float) (0.5 * (double) Utils.GetItemTemplate<AmmoTemplate>(MongoID.op_Implicit(damageInfoStruct.SourceId)).BulletMassGram * (double) damageInfoStruct.ArmorDamage * (double) damageInfoStruct.ArmorDamage / 1000.0) / 1000f) * PluginConfig.RagdollForceModifier.Value;
    ApplyCorpseImpulsePatch.corpseAppliedForceField.SetValue((object) __instance, (object) num);
    corpse.Ragdoll.ApplyImpulse(damageInfoStruct.HitCollider, damageInfoStruct.Direction, damageInfoStruct.HitPoint, num);
    return false;
  }
}
