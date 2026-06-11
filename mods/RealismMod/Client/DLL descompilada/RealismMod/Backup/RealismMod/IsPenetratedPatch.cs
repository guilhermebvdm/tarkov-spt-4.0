// Decompiled with JetBrains decompiler
// Type: RealismMod.IsPenetratedPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class IsPenetratedPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (BodyPartCollider).GetMethod("IsPenetrated", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Prefix(BodyPartCollider __instance, EftBulletClass shot, ref bool __result)
  {
    if (!Object.op_Inequality((Object) (shot.HittedBallisticCollider as BodyPartCollider), (Object) null) || !((BodyPartCollider) shot.HittedBallisticCollider).BodyPartColliderType.ToString().ToLower().Contains("arm") || (double) shot.PenetrationPower <= 10.0)
      return true;
    __result = true;
    return false;
  }
}
