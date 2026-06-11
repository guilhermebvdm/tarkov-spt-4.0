// Decompiled with JetBrains decompiler
// Type: RealismMod.StaminaRegenRatePatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Comfort.Common;
using EFT;
using EFT.HealthSystem;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class StaminaRegenRatePatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (PlayerPhysicalClass).GetMethod("method_21", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Prefix(PlayerPhysicalClass __instance, float baseValue, ref float __result)
  {
    float[] numArray = (float[]) AccessTools.Field(typeof (PlayerPhysicalClass), "float_7").GetValue((object) __instance);
    PlayerPhysicalClass.EPose index = (PlayerPhysicalClass.EPose) AccessTools.Field(typeof (PlayerPhysicalClass), "epose_0").GetValue((object) __instance);
    Player player = (Player) AccessTools.Field(typeof (PlayerPhysicalClass), "player_0").GetValue((object) __instance);
    float num1 = (float) AccessTools.Property(typeof (PlayerPhysicalClass), "Single_0").GetValue((object) __instance);
    float num2 = GearController.HasRespirator ? 0.75f : (GearController.HasGasMask ? 0.5f : (GearController.FSIsActive ? 0.75f : 1f));
    double num3;
    if (!StanceController.IsAiming)
    {
      switch (StanceController.CurrentStance)
      {
        case EStance.LowReady:
          num3 = 1.1499999761581421;
          break;
        case EStance.HighReady:
          num3 = 1.0499999523162842;
          break;
        case EStance.ShortStock:
          num3 = 1.1000000238418579;
          break;
        case EStance.ActiveAiming:
          num3 = 0.949999988079071;
          break;
        case EStance.PatrolStance:
          num3 = 1.25;
          break;
        case EStance.PistolCompressed:
          num3 = 1.2000000476837158;
          break;
        default:
          num3 = 1.0;
          break;
      }
    }
    else
      num3 = 0.89999997615814209;
    float num4 = (float) num3;
    float num5 = (float) (1.0 - (double) PlayerState.TotalModifiedWeightMinusWeapon / 100.0 * (1.0 - (double) PlayerState.StrengthWeightBuff));
    ref float local = ref __result;
    double num6 = (double) baseValue * (double) numArray[index];
    BackendConfigSettingsClass.GClass1498 staminaRestoration = Singleton<BackendConfigSettingsClass>.Instance.StaminaRestoration;
    ValueStruct energy = ((GInterface355) player.HealthController).Energy;
    double normalized = (double) ((ValueStruct) ref energy).Normalized;
    double at = (double) staminaRestoration.GetAt((float) normalized);
    double num7 = num6 * at * ((double) SkillManager.SkillBuffClass.op_Implicit(player.Skills.EnduranceBuffRestoration) + 1.0) * (double) PlayerState.HealthStamRegenFactor * (double) num2 * (double) num4 * (double) num5 / (double) num1;
    local = (float) num7;
    return false;
  }
}
