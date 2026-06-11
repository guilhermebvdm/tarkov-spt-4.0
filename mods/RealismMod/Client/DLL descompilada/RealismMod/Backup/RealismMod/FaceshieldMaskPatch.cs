// Decompiled with JetBrains decompiler
// Type: RealismMod.FaceshieldMaskPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.CameraControl;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class FaceshieldMaskPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (PlayerCameraController).GetMethod("method_3", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(
    PlayerCameraController __instance,
    FaceShieldComponent faceShield)
  {
    if (faceShield == null || ((GClass3175) faceShield)?.Item == null || Plugin.LoadedTextures.Count <= 0)
      return;
    Material material = ((VisorEffect) AccessTools.Field(typeof (PlayerCameraController), "visorEffect_0").GetValue((object) __instance)).method_4();
    string maskToUse = TemplateStats.GetDataObj<Gear>(TemplateStats.GearStats, MongoID.op_Implicit(((GClass3175) faceShield).Item.TemplateId)).MaskToUse;
    if (maskToUse == null || maskToUse == string.Empty || maskToUse == "")
      return;
    Texture loadedTexture = Plugin.LoadedTextures[maskToUse + ".png"];
    material.SetTexture("_Mask", loadedTexture);
  }
}
