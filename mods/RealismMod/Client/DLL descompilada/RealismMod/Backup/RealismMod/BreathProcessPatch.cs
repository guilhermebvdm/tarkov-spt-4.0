// Decompiled with JetBrains decompiler
// Type: RealismMod.BreathProcessPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.Animations;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class BreathProcessPatch : ModulePatch
{
  private static FieldInfo breathIntensityField;
  private static FieldInfo shakeIntensityField;
  private static FieldInfo breathFrequencyField;
  private static FieldInfo cameraSensitivityField;
  private static FieldInfo baseHipRandomAmplitudesField;
  private static FieldInfo shotEffectorField;
  private static FieldInfo handsRotationSpringField;
  private static FieldInfo processorsField;
  private static FieldInfo lackOfOxygenStrengthField;

  protected virtual MethodBase GetTargetMethod()
  {
    BreathProcessPatch.breathIntensityField = AccessTools.Field(typeof (BreathEffector), "_breathIntensity");
    BreathProcessPatch.shakeIntensityField = AccessTools.Field(typeof (BreathEffector), "_shakeIntensity");
    BreathProcessPatch.breathFrequencyField = AccessTools.Field(typeof (BreathEffector), "_breathFrequency");
    BreathProcessPatch.cameraSensitivityField = AccessTools.Field(typeof (BreathEffector), "_cameraSensetivity");
    BreathProcessPatch.baseHipRandomAmplitudesField = AccessTools.Field(typeof (BreathEffector), "_baseHipRandomAmplitudes");
    BreathProcessPatch.shotEffectorField = AccessTools.Field(typeof (BreathEffector), "_shotEffector");
    BreathProcessPatch.handsRotationSpringField = AccessTools.Field(typeof (BreathEffector), "_handsRotationSpring");
    BreathProcessPatch.lackOfOxygenStrengthField = AccessTools.Field(typeof (BreathEffector), "_lackOfOxygenStrength");
    BreathProcessPatch.processorsField = AccessTools.Field(typeof (BreathEffector), "_processors");
    return (MethodBase) typeof (BreathEffector).GetMethod("Process", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPrefix]
  private static bool PatchPrefix(BreathEffector __instance, float deltaTime)
  {
    float num1 = (float) AccessTools.Field(typeof (BreathEffector), "_breathFrequency").GetValue((object) __instance);
    float num2 = (float) AccessTools.Field(typeof (BreathEffector), "_cameraSensetivity").GetValue((object) __instance);
    Vector2 vector2 = (Vector2) AccessTools.Field(typeof (BreathEffector), "_baseHipRandomAmplitudes").GetValue((object) __instance);
    ShotEffector shotEffector = (ShotEffector) AccessTools.Field(typeof (BreathEffector), "_shotEffector").GetValue((object) __instance);
    Spring spring = (Spring) AccessTools.Field(typeof (BreathEffector), "_handsRotationSpring").GetValue((object) __instance);
    AnimationCurve animationCurve = (AnimationCurve) AccessTools.Field(typeof (BreathEffector), "_lackOfOxygenStrength").GetValue((object) __instance);
    GClass2599[] gclass2599Array = (GClass2599[]) AccessTools.Field(typeof (BreathEffector), "_processors").GetValue((object) __instance);
    float num3 = Mathf.Sqrt(((Player.ValueBlender) __instance.AmplitudeGain).Value);
    __instance.HipXRandom.Amplitude = Mathf.Clamp(vector2.x + num3, 0.0f, 3f);
    __instance.HipZRandom.Amplitude = Mathf.Clamp(vector2.y + num3, 0.0f, 3f);
    __instance.HipXRandom.Hardness = __instance.HipZRandom.Hardness = ((Player.ValueBlender) __instance.Hardness).Value;
    BreathProcessPatch.shakeIntensityField.SetValue((object) __instance, (object) 1f);
    bool flag = __instance.TremorOn || __instance.Fracture || Plugin.RealHealthController.ArmsAreIncapacitated || Plugin.RealHealthController.HasOverdosed;
    float num4 = 1f;
    if ((double) Time.time < (double) __instance.StiffUntill)
    {
      float num5 = Mathf.Clamp((float) (-(double) __instance.StiffUntill + (double) Time.time + 1.0), flag ? 0.75f : 0.3f, 1f);
      BreathProcessPatch.breathIntensityField.SetValue((object) __instance, (object) (float) ((double) num5 * (double) __instance.Intensity));
      BreathProcessPatch.shakeIntensityField.SetValue((object) __instance, (object) num5);
      num4 = num5;
    }
    else
    {
      float num6 = Mathf.Pow(WeaponStats.TotalAimStabilityModi, 1.2f);
      float num7 = (__instance.Physical.HoldingBreath ? 0.495f : 1f) * num6;
      float num8 = (__instance.Physical.HoldingBreath ? 0.375f : 1f) * num6;
      float num9 = WeaponStats.IsOptic ? PluginConfig.SwayIntensity.Value * 1.1f : PluginConfig.SwayIntensity.Value;
      float num10 = animationCurve.Evaluate(GClass827<float>.op_Implicit(__instance.OxygenLevel));
      float num11 = __instance.IsAiming ? 0.8f : 1.05f;
      BreathProcessPatch.breathIntensityField.SetValue((object) __instance, (object) (float) ((double) Mathf.Clamp(Mathf.Lerp(4f, num11, num10), 1f, 1.5f) * (double) __instance.Intensity * (double) num8 * (double) num9));
      BreathProcessPatch.breathFrequencyField.SetValue((object) __instance, (object) (float) ((double) Mathf.Clamp(Mathf.Lerp(4f, 1f, num10), 1f, 2.5f) * (double) deltaTime * (double) num7 * (double) num9));
      BreathProcessPatch.shakeIntensityField.SetValue((object) __instance, (object) (float) ((double) num7 * (double) num9));
      BreathProcessPatch.cameraSensitivityField.SetValue((object) __instance, (object) (float) ((double) Mathf.Lerp(2f, 0.0f, num10) * (double) __instance.Intensity));
      num1 = (float) AccessTools.Field(typeof (BreathEffector), "_breathFrequency").GetValue((object) __instance);
      num2 = (float) AccessTools.Field(typeof (BreathEffector), "_cameraSensetivity").GetValue((object) __instance);
    }
    GClass827<float> staminaLevel = __instance.StaminaLevel;
    __instance.YRandom.Amplitude = __instance.BreathParams.AmplitudeCurve.Evaluate(GClass827<float>.op_Implicit(staminaLevel));
    float num12 = __instance.BreathParams.Delay.Evaluate(GClass827<float>.op_Implicit(staminaLevel));
    __instance.XRandom.MinMaxDelay = __instance.YRandom.MinMaxDelay = new Vector2(num12 / 2f, num12);
    __instance.YRandom.Hardness = __instance.BreathParams.Hardness.Evaluate(GClass827<float>.op_Implicit(staminaLevel));
    float num13 = __instance.YRandom.GetValue(deltaTime);
    float num14 = __instance.XRandom.GetValue(deltaTime);
    spring.AddAcceleration(Vector3.op_Multiply(new Vector3((float) ((double) Mathf.Max(0.0f, -num13) * (1.0 - (double) GClass827<float>.op_Implicit(staminaLevel)) * 2.0), num13, num14), (float) BreathProcessPatch.shakeIntensityField.GetValue((object) __instance) * __instance.Intensity));
    Vector3 vector3 = Vector3.zero;
    if (flag)
    {
      float num15 = (__instance.TremorOn ? deltaTime : deltaTime / 2f) * num4;
      float num16 = __instance.TremorXRandom.GetValue(num15);
      float num17 = __instance.TremorYRandom.GetValue(num15);
      float num18 = __instance.TremorZRandom.GetValue(num15);
      if ((__instance.Fracture || Plugin.RealHealthController.ArmsAreIncapacitated || Plugin.RealHealthController.HasOverdosed) && !__instance.IsAiming)
        num16 += Mathf.Max(0.0f, num13) * Mathf.Lerp(1f, 100f / __instance.EnergyFractureLimit, GClass827<float>.op_Implicit(staminaLevel));
      vector3 = Vector3.op_Multiply(new Vector3(num16, num17, num18), __instance.Intensity);
    }
    else if (!__instance.IsAiming && ShootController.IsFiring)
      vector3 = Vector3.op_Multiply(new Vector3(__instance.HipXRandom.GetValue(deltaTime), 0.0f, __instance.HipZRandom.GetValue(deltaTime)), __instance.Intensity * __instance.HipPenalty);
    if ((double) Vector3.SqrMagnitude(Vector3.op_Subtraction(vector3, ((RecoilProcessBase) shotEffector.CurrentRecoilEffect.HandRotationRecoilEffect).Offset)) > 0.0099999997764825821)
      ((RecoilProcessBase) shotEffector.CurrentRecoilEffect.HandRotationRecoilEffect).Offset = Vector3.Lerp(((RecoilProcessBase) shotEffector.CurrentRecoilEffect.HandRotationRecoilEffect).Offset, vector3, 0.1f);
    else
      ((RecoilProcessBase) shotEffector.CurrentRecoilEffect.HandRotationRecoilEffect).Offset = vector3;
    gclass2599Array[0].ProcessRaw(num1, (float) BreathProcessPatch.breathIntensityField.GetValue((object) __instance) * 0.75f);
    gclass2599Array[1].ProcessRaw(num1, (float) ((double) (float) BreathProcessPatch.breathIntensityField.GetValue((object) __instance) * (double) num2 * 0.75));
    return false;
  }
}
