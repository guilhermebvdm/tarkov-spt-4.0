// Decompiled with JetBrains decompiler
// Type: RealismMod.ErgoWeightPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class ErgoWeightPatch : ModulePatch
{
  private static FieldInfo playerField;

  protected virtual MethodBase GetTargetMethod()
  {
    ErgoWeightPatch.playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    return (MethodBase) typeof (Player.FirearmController).GetMethod("get_ErgonomicWeight", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Prefix(Player.FirearmController __instance, ref float __result)
  {
    if (!((Player) ErgoWeightPatch.playerField.GetValue((object) __instance)).IsYourPlayer)
      return true;
    __result = (float) ((double) WeaponStats.ErgoFactor * (1.0 - (double) PlayerState.StrengthSkillAimBuff * 1.5) * (1.0 + (1.0 - (double) PlayerState.GearErgoPenalty)));
    if (PluginConfig.EnablePWALogging.Value)
    {
      ModulePatch.Logger.LogWarning((object) "===ErgonomicWeight===");
      ModulePatch.Logger.LogWarning((object) ("total ergo weight = " + __result.ToString()));
      ModulePatch.Logger.LogWarning((object) ("base ergo weight = " + WeaponStats.ErgoFactor.ToString()));
      ModulePatch.Logger.LogWarning((object) ("ergoweight ergo weight = " + WeaponStats.ErgonomicWeight.ToString()));
    }
    return false;
  }
}
