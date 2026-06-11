// Decompiled with JetBrains decompiler
// Type: RealismMod.EnduranceMovementActionPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using SPT.Reflection.Patching;
using SPT.Reflection.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class EnduranceMovementActionPatch : ModulePatch
{
  private static Type _targetType;
  private static MethodInfo _method_1;

  public EnduranceMovementActionPatch()
  {
    // ISSUE: reference to a compiler-generated field
    // ISSUE: reference to a compiler-generated field
    EnduranceMovementActionPatch._targetType = ((IEnumerable<Type>) PatchConstants.EftTypes).Single<Type>(EnduranceMovementActionPatch.\u003C\u003EO.\u003C0\u003E__IsEnduraStrngthType ?? (EnduranceMovementActionPatch.\u003C\u003EO.\u003C0\u003E__IsEnduraStrngthType = new Func<Type, bool>(EndurancePatchHelper.IsEnduraStrngthType)));
    EnduranceMovementActionPatch._method_1 = EnduranceMovementActionPatch._targetType.GetMethod("method_1", BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public);
  }

  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) EnduranceMovementActionPatch._method_1;
  }

  [PatchPrefix]
  private static bool Prefix(
    ref float __result,
    SkillManager.GStruct242 movement,
    SkillManager __instance)
  {
    float num = __instance.Settings.Endurance.MovementAction * (float) (1.0 + (double) __instance.Settings.Endurance.GainPerFatigueStack * (double) movement.Fatigue);
    __result = (double) movement.Overweight > 0.0 ? num * 0.5f : num;
    return false;
  }
}
