// Decompiled with JetBrains decompiler
// Type: TarkovIRL.EfficiencyController
// Assembly: TarkovIRL, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C42939BD-7BF0-4586-ABE5-9D2EFC361A0B
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\TarkovIRL_WeaponsHandlingMod_1.0.0\BepInEx\plugins\TarkovIRL.dll

using EFT;
using EFT.HealthSystem;

using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace TarkovIRL;

internal static class EfficiencyController
{
  private static readonly float _LerpSpeed = 1f;
  private static readonly int _HeavyBleedHash = -1302691610;
  private static readonly int _LightBleedHash = -1302691609;
  private static readonly int _FreshWoundHash = -2109260636;
  private static readonly int _BoneBreakHash = -1302691612;
  private static readonly int _TremorsHash = -139892197;
  private static readonly int _PainHash = -139892193;
  private static readonly int _FatigueHash = -543176719;
  private static readonly int _ToxicationHash = -1705976133;
  private static readonly int[] _NominalEffectStates = new int[3]
  {
    -1705976135,
    1022907221,
    -139892192
  };
  private static readonly int[] _KnownEffectStates = new int[7]
  {
    -1302691609,
    -1302691612,
    -139892197,
    -139892193,
    -1302691610,
    -543176719,
    -1705976133
  };
  private static float _efficiencyLerpTarget = 0.0f;
  private static float _efficiencyLerpTargetWithoutWeight = 0.0f;
  private static float _efficiencyLerp = 0.0f;
  private static float _efficiencyLastFrame = 0.0f;
  private static float debugTimer = 0.0f;
  private static Dictionary<Injury, float> _injuryTimes = new Dictionary<Injury, float>();
  private static int _heavyBleedCountLastFrame = 0;
  private static int _lightBleedCountLastFrame = 0;
  private static int _boneBreakCountLastFrame = 0;

  public static void UpdateEfficiencyLerp(float dt)
  {
    float num = (double) EfficiencyController._efficiencyLerpTarget < (double) EfficiencyController._efficiencyLastFrame ? 1f / EfficiencyController._efficiencyLerpTarget : EfficiencyController._efficiencyLerpTarget;
    EfficiencyController._efficiencyLerp = Mathf.Lerp(EfficiencyController._efficiencyLerp, EfficiencyController._efficiencyLerpTarget, dt * EfficiencyController._LerpSpeed * PrimeMover.EfficiencyLerpMulti.Value * num);
    EfficiencyController._efficiencyLastFrame = EfficiencyController._efficiencyLerpTarget;
    EfficiencyController.debugTimer += dt;
    if ((double) EfficiencyController.debugTimer <= 0.20000000298023224)
      return;
    EfficiencyController.debugTimer = 0.0f;
  }

  private static void CheckInjuryChange(
    int boneBreakCount,
    int heavyBleedCount,
    int lightBleedCount)
  {
    if (boneBreakCount > EfficiencyController._boneBreakCountLastFrame)
    {
      int num = boneBreakCount - EfficiencyController._boneBreakCountLastFrame;
      for (int index = 0; index < num; ++index)
        EfficiencyController.AddInjuryToEffects(new Injury(Injury.EInjury.BONE_BREAK, Time.time));
    }
    if (boneBreakCount < EfficiencyController._boneBreakCountLastFrame)
      EfficiencyController.RemoveInjuryEffect(Injury.EInjury.BONE_BREAK);
    if (heavyBleedCount > EfficiencyController._heavyBleedCountLastFrame)
    {
      int num = heavyBleedCount - EfficiencyController._heavyBleedCountLastFrame;
      for (int index = 0; index < num; ++index)
        EfficiencyController.AddInjuryToEffects(new Injury(Injury.EInjury.HEAVY_BLEED, Time.time));
    }
    if (heavyBleedCount < EfficiencyController._heavyBleedCountLastFrame)
      EfficiencyController.RemoveInjuryEffect(Injury.EInjury.HEAVY_BLEED);
    if (lightBleedCount > EfficiencyController._lightBleedCountLastFrame)
    {
      int num = lightBleedCount - EfficiencyController._lightBleedCountLastFrame;
      for (int index = 0; index < num; ++index)
        EfficiencyController.AddInjuryToEffects(new Injury(Injury.EInjury.LIGHT_BLEED, Time.time));
    }
    if (lightBleedCount < EfficiencyController._lightBleedCountLastFrame)
      EfficiencyController.RemoveInjuryEffect(Injury.EInjury.LIGHT_BLEED);
    EfficiencyController._boneBreakCountLastFrame = boneBreakCount;
    EfficiencyController._heavyBleedCountLastFrame = heavyBleedCount;
    EfficiencyController._lightBleedCountLastFrame = lightBleedCount;
  }

  private static float ReturnInjuryEffectCoef()
  {
    float impactWeightPercent = 0.0f;
    foreach (KeyValuePair<Injury, float> injuryTime in EfficiencyController._injuryTimes)
    {
      float num = Mathf.Clamp01((Time.time - injuryTime.Key.TimeInflicted) / injuryTime.Key.TimeUntilEffect);
      impactWeightPercent += injuryTime.Key.InjuryWeight * num;
    }
    float injuryImpact = EfficiencyController.GetInjuryImpact(impactWeightPercent);
    TIRLUtils.Log($"total injury effect is ({injuryImpact})", false);
    return injuryImpact;
  }

  private static void AddInjuryToEffects(Injury injury)
  {
    EfficiencyController._injuryTimes.Add(injury, Time.time);
  }

  private static void RemoveInjuryEffect(Injury.EInjury injuryType)
  {
    Injury key = (Injury) null;
    foreach (KeyValuePair<Injury, float> injuryTime in EfficiencyController._injuryTimes)
    {
      if (key == null && injuryTime.Key.InjuryType == injuryType)
        key = injuryTime.Key;
      else if (key != null && injuryTime.Key.InjuryType == injuryType && (double) injuryTime.Key.TimeInflicted < (double) key.TimeInflicted)
        key = injuryTime.Key;
    }
    if (key == null)
      return;
    EfficiencyController._injuryTimes.Remove(key);
  }

  public static void UpdateEfficiency(Player player)
  {
    ValueStruct hydration = player.HealthController.Hydration;
    float normalized1 = hydration.Normalized;
    ValueStruct energy = player.HealthController.Energy;
    float normalized2 = energy.Normalized;
    float num1 = 1f - Mathf.Clamp01(player.Physical.Overweight);
    int multipleInstances1 = 0;
    int heavyBleedCount = 0;
    int lightBleedCount = 0;
    int boneBreakCount = 0;
    int multipleInstances2 = 0;
    int multipleInstances3 = 0;
    int multipleInstances4 = 0;
    int multipleInstances5 = 0;
    foreach (IEffect allEffect in player.HealthController.GetAllEffects((EBodyPart) 7))
    {
      if (!EfficiencyController.IsEffectKnown(allEffect, EfficiencyController._NominalEffectStates) && !EfficiencyController.IsEffectKnown(allEffect, EfficiencyController._KnownEffectStates))
        TIRLUtils.Log($"effect type {allEffect}({allEffect.Type.FullName.GetHashCode()}) on bodypart {allEffect.BodyPart}", false);
      if (allEffect.Type.FullName.GetHashCode() == EfficiencyController._HeavyBleedHash)
        ++heavyBleedCount;
      if (allEffect.Type.FullName.GetHashCode() == EfficiencyController._LightBleedHash)
        ++lightBleedCount;
      if (allEffect.Type.FullName.GetHashCode() == EfficiencyController._FreshWoundHash)
        ++multipleInstances1;
      if (allEffect.Type.FullName.GetHashCode() == EfficiencyController._BoneBreakHash)
        ++boneBreakCount;
      if (allEffect.Type.FullName.GetHashCode() == EfficiencyController._TremorsHash)
        ++multipleInstances2;
      if (allEffect.Type.FullName.GetHashCode() == EfficiencyController._PainHash)
        ++multipleInstances3;
      if (allEffect.Type.FullName.GetHashCode() == EfficiencyController._FatigueHash)
        ++multipleInstances4;
      if (allEffect.Type.FullName.GetHashCode() == EfficiencyController._ToxicationHash)
        ++multipleInstances5;
    }
    TIRLUtils.Log($"heavyBleedCount {heavyBleedCount}, lightBleedCount {lightBleedCount}, freshWoundCount {multipleInstances1}, boneBreakCount {boneBreakCount} , tremorCount {multipleInstances2}, painCount {multipleInstances3}, fatigueCount {multipleInstances4}, intoxCount {multipleInstances5}", false);
    EfficiencyController.CheckInjuryChange(boneBreakCount, heavyBleedCount, lightBleedCount);
    ValueStruct bodyPartHealth = player.HealthController.GetBodyPartHealth((EBodyPart) 7, false);
    float normalized3 = bodyPartHealth.Normalized;
    float normalValue1 = player.Physical.Stamina.NormalValue;
    float normalValue2 = player.Physical.HandsStamina.NormalValue;
    float injuryImpact1 = EfficiencyController.GetInjuryImpact(normalized1, 10f);
    float injuryImpact2 = EfficiencyController.GetInjuryImpact(normalized2, 10f);
    float injuryImpact3 = EfficiencyController.GetInjuryImpact(num1, PrimeMover.EfficiencyOverweightIpmact.Value);
    float injuryImpact4 = EfficiencyController.GetInjuryImpact(normalized3, 40f);
    float injuryImpact5 = EfficiencyController.GetInjuryImpact(normalValue1, 20f);
    float injuryImpact6 = EfficiencyController.GetInjuryImpact(normalValue2, 20f);
    float injuryImpact7 = EfficiencyController.GetInjuryImpact(5f, multipleInstances1);
    float injuryImpact8 = EfficiencyController.GetInjuryImpact(5f, multipleInstances2);
    float injuryImpact9 = EfficiencyController.GetInjuryImpact(5f, multipleInstances3);
    float injuryImpact10 = EfficiencyController.GetInjuryImpact(5f, multipleInstances4);
    float injuryImpact11 = EfficiencyController.GetInjuryImpact(10f, multipleInstances5);
    float num2 = EfficiencyController.ReturnInjuryEffectCoef() * injuryImpact7 * injuryImpact8 * injuryImpact9;
    float num3 = RealismWrapper.IsAdrenaline ? 1f : num2;
    float num4 = RealismWrapper.IsAdrenaline ? 1f : injuryImpact1;
    float num5 = RealismWrapper.IsAdrenaline ? 1f : injuryImpact2;
    float num6 = RealismWrapper.IsOverdose ? 1.2f : 1f;
    float num7 = num4 * num5 * injuryImpact4 * injuryImpact5 * injuryImpact10 * injuryImpact6 * num3 * injuryImpact11 * num6;
    float num8 = RealismWrapper.IsAdrenaline ? 0.5f : 1f;
    float num9 = AnimStateController.IsSideStep ? 1.3f : 1f;
    float num10 = StanceController.CurrentStance == EStance.HighReady ? 0.85f : 1f;
    float num11 = StanceController.CurrentStance == EStance.LowReady ? 1.1f : 1f;
    float num12 = StanceController.CurrentStance == EStance.ShortStock ? 0.7f : 1f;
    float num13 = StanceController.CurrentStance == EStance.ActiveAiming ? 1.1f : 1f;
    float num14 = StanceController.IsMounting ? 0.5f : 1f;
    float num15 = StanceController.IsLeftShoulder ? 1.1f : 1f;
    float num16 = player.IsSprintEnabled ? 1.5f : 1f;
    float num17 = (float) (0.5 + (double) player.Speed * 0.5);
    if (!PlayerMotionController.IsPlayerMovement)
      num17 = 0.5f;
    float num18 = (float) (0.5 + (double) player.PoseLevel * 0.5);
    float num19 = player.IsInPronePose ? 0.5f : 1f;
    float num20 = num17 * num18 * num19 * num16 * num10 * num11 * num14 * num12 * num15 * num13 * num9 * num8;
    EfficiencyController._efficiencyLerpTarget = num7 * num20 * injuryImpact3;
    EfficiencyController._efficiencyLerpTargetWithoutWeight = num7 * num20;
    TIRLUtils.Log($"overWeightMulti {injuryImpact3}, healthMulti {injuryImpact4}, stamMulti {injuryImpact5}, injuryMulti {num3} , negativeEffects total {num7}", false);
  }

  public static float EfficiencyModifier => EfficiencyController._efficiencyLerp;

  public static float EfficiencyModifierInverse => 1f / EfficiencyController._efficiencyLerp;

  private static float GetInjuryImpact(float value, float impactWeightPercent)
  {
    float num = impactWeightPercent * 0.01f * PrimeMover.EfficiencyInjuryImpact.Value;
    return (float) (1.0 + (1.0 - (double) value) * (double) num);
  }

  private static float GetInjuryImpact(float impactWeightPercent, int multipleInstances)
  {
    return multipleInstances == 0 ? 1f : (1f + impactWeightPercent * 0.01f * PrimeMover.EfficiencyInjuryImpact.Value) * (float) multipleInstances;
  }

  private static float GetInjuryImpact(float impactWeightPercent)
  {
    return 1f + impactWeightPercent * 0.01f * PrimeMover.EfficiencyInjuryImpact.Value;
  }

  private static bool IsEffectKnown(IEffect effect, int[] effectsArray)
  {
    foreach (int effects in effectsArray)
    {
      if (effect.Type.FullName.GetHashCode() == effects)
        return true;
    }
    return false;
  }
}
