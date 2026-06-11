// Decompiled with JetBrains decompiler
// Type: RealismMod.RemoveEffectPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class RemoveEffectPatch : ModulePatch
{
  private static Type _targetType;
  private static MethodInfo _targetMethod;

  public RemoveEffectPatch()
  {
    RemoveEffectPatch._targetType = AccessTools.TypeByName("MedsController");
    RemoveEffectPatch._targetMethod = AccessTools.Method(RemoveEffectPatch._targetType, "Remove");
  }

  protected virtual MethodBase GetTargetMethod() => (MethodBase) RemoveEffectPatch._targetMethod;

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix()
  {
    ModulePatch.Logger.LogWarning((object) "Remove");
    if (PluginConfig.EnableMedicalLogging.Value)
      ModulePatch.Logger.LogWarning((object) "Cancelling Meds");
    Plugin.RealHealthController.CancelPendingEffects();
  }
}
