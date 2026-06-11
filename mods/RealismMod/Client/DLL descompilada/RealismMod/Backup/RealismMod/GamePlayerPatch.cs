// Decompiled with JetBrains decompiler
// Type: RealismMod.GamePlayerPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class GamePlayerPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (GamePlayerOwner).GetMethod("LateUpdate");
  }

  [SPT.Reflection.Patching.PatchPostfix]
  public static void PatchPostfix(GamePlayerOwner __instance)
  {
    if (!Object.op_Equality((Object) GameWorldController.GamePlayerOwner, (Object) null))
      return;
    GameWorldController.GamePlayerOwner = __instance;
  }
}
