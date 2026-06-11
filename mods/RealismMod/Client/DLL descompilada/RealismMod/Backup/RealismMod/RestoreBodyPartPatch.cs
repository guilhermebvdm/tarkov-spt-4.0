// Decompiled with JetBrains decompiler
// Type: RealismMod.RestoreBodyPartPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.HealthSystem;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class RestoreBodyPartPatch : ModulePatch
{
  private static FieldInfo _bodyPartRestoredField;

  protected virtual MethodBase GetTargetMethod()
  {
    RestoreBodyPartPatch._bodyPartRestoredField = AccessTools.Field(typeof (ActiveHealthController), "BodyPartRestoredEvent");
    return (MethodBase) typeof (ActiveHealthController).GetMethod("RestoreBodyPart", BindingFlags.Instance | BindingFlags.Public);
  }

  private static BodyPartStateWrapper GetBodyPartStateWrapper(
    ActiveHealthController instance,
    EBodyPart bodyPart)
  {
    IDictionary dictionary = (IDictionary) typeof (ActiveHealthController).GetProperty("Dictionary_0", BindingFlags.Instance | BindingFlags.Public).GetMethod.Invoke((object) instance, (object[]) null);
    if (dictionary.Contains((object) bodyPart))
      return new BodyPartStateWrapper(dictionary[(object) bodyPart]);
    ModulePatch.Logger.LogError((object) "=======Realism Mod: FAILED TO GET BODYPARTSTATE INSTANCE=========");
    return (BodyPartStateWrapper) null;
  }

  [PatchPrefix]
  private static bool Prefix(
    ActiveHealthController __instance,
    EBodyPart bodyPart,
    float healthPenalty,
    ref bool __result)
  {
    if (!__instance.Player.IsYourPlayer)
      return true;
    GClass2814<ActiveHealthController.GClass2813>.BodyPartState bodyPartState = ((GClass2814<ActiveHealthController.GClass2813>) __instance).Dictionary_0[bodyPart];
    Action<EBodyPart, ValueStruct> action1 = (Action<EBodyPart, ValueStruct>) RestoreBodyPartPatch._bodyPartRestoredField.GetValue((object) __instance);
    if (!bodyPartState.IsDestroyed)
    {
      __result = false;
      return false;
    }
    HealthValue health = bodyPartState.Health;
    bodyPartState.IsDestroyed = false;
    healthPenalty += (1f - healthPenalty) * SkillManager.SkillBuffClass.op_Implicit(((GClass2814<ActiveHealthController.GClass2813>) __instance).skillManager_0.SurgeryReducePenalty);
    bodyPartState.Health = new HealthValue(1f, Mathf.Max(1f, Mathf.Ceil(bodyPartState.Health.Maximum * healthPenalty)), 0.0f);
    __instance.method_43(bodyPart, (EDamageType) 256 /*0x0100*/);
    __instance.method_35(bodyPart);
    Action<EBodyPart, ValueStruct> action2 = action1;
    if (action2 != null)
      action2(bodyPart, health.CurrentAndMaximum);
    __result = true;
    return false;
  }
}
