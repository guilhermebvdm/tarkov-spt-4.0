// Decompiled with JetBrains decompiler
// Type: RealismMod.SetSkinPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Diz.Skinning;
using EFT;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class SetSkinPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (PlayerBody).GetMethod("SetSkin", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPostfix]
  private static void Prefix(
    PlayerBody __instance,
    KeyValuePair<EBodyModelPart, ResourceKey> part,
    Skeleton skeleton)
  {
    __instance.BodySkins[part.Key].Unskin();
  }
}
