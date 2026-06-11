// Decompiled with JetBrains decompiler
// Type: RealismMod.ProceedMedsPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Comfort.Common;
using EFT;
using SPT.Reflection.Patching;
using System;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class ProceedMedsPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (Player).GetMethod("Proceed", BindingFlags.Instance | BindingFlags.Public, (Binder) null, new Type[5]
    {
      typeof (MedsItemClass),
      typeof (GStruct353<EBodyPart>),
      typeof (Callback<GInterface176>),
      typeof (int),
      typeof (bool)
    }, (ParameterModifier[]) null);
  }

  [PatchPrefix]
  private static bool Prefix(
    Player __instance,
    MedsItemClass meds,
    ref GStruct353<EBodyPart> bodyParts)
  {
    if (!__instance.IsYourPlayer || Plugin.FikaPresent || !bodyParts.nullable_0.HasValue || bodyParts.Length <= 0)
      return true;
    for (int index = 0; index < bodyParts.Length; ++index)
      ModulePatch.Logger.LogWarning((object) bodyParts[index].ToString());
    (EBodyPart NewBodyPart, bool ShouldAllowHeal) tuple = Plugin.RealHealthController.ProcessHealAttempt(meds, __instance, bodyParts[0]);
    bodyParts = new GStruct353<EBodyPart>(tuple.NewBodyPart);
    return tuple.ShouldAllowHeal;
  }
}
