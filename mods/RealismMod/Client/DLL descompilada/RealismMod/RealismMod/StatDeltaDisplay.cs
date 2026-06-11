// Decompiled with JetBrains decompiler
// Type: RealismMod.StatDeltaDisplay
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.InventoryLogic;
using UnityEngine;

#nullable disable
namespace RealismMod;

public static class StatDeltaDisplay
{
  public static void DisplayDelta(Weapon __instance, Gun weapStats)
  {
    bool isChonker = __instance.IsBeltMachineGun || (double) ((Item) __instance).TotalWeight > 10.0;
    float centerOfImpactBase = __instance.CenterOfImpactBase;
    float currentCOI = centerOfImpactBase;
    float bFirerate = (float) __instance.Template.bFirerate;
    float currentSemiROF = (float) Mathf.Max(__instance.Template.SingleFireRate, 240 /*0xF0*/);
    float visualMulti = weapStats.VisualMulti;
    float recoilCamera = __instance.Template.RecoilCamera;
    float multiplierHandRotation = __instance.Template.RecoilCategoryMultiplierHandRotation;
    float currentConv = multiplierHandRotation;
    float recolDispersion = (float) __instance.Template.RecolDispersion;
    float recoilAngle1 = (float) __instance.Template.RecoilAngle;
    float recoilForceUp = __instance.Template.RecoilForceUp;
    float currentVRecoil = recoilForceUp;
    float recoilForceBack = __instance.Template.RecoilForceBack;
    float currentHRecoil = recoilForceBack;
    float ergonomics1 = __instance.Template.Ergonomics;
    float currentErgo = ergonomics1;
    float pureErgo = ergonomics1;
    float pureRecoil = 0.0f;
    float currentTorque = 0.0f;
    float currentShotDisp = 0.0f;
    float currentMalfChance = 0.0f;
    string operationType = weapStats.OperationType;
    string weapType = weapStats.WeapType;
    bool stockAllowsFSADS = false;
    bool folded = __instance.Folded;
    bool hasShoulderContact = false;
    if (weapStats.HasShoulderContact && !folded)
      hasShoulderContact = true;
    foreach (Mod mod in __instance.Mods)
    {
      WeaponMod dataObj = TemplateStats.GetDataObj<WeaponMod>(TemplateStats.WeaponModStats, MongoID.op_Implicit(((Item) mod).TemplateId));
      float modWeight = ((Item) mod).Weight;
      if (Utils.IsMagazine(mod))
        modWeight = ((Item) mod).TotalWeight;
      float modWeightFactored = StatCalc.FactoredWeight(modWeight);
      float ergonomics2 = mod.Ergonomics;
      float verticalRecoil = dataObj.VerticalRecoil;
      float horizontalRecoil = dataObj.HorizontalRecoil;
      float autoRof = dataObj.AutoROF;
      float semiRof = dataObj.SemiROF;
      float cameraRecoil = dataObj.CameraRecoil;
      float convergence = dataObj.Convergence;
      verticalRecoil += (double) convergence > 0.0 ? convergence * -1f : 0.0f;
      float dispersion = dataObj.Dispersion;
      float recoilAngle2 = dataObj.RecoilAngle;
      float accuracy = (float) mod.Accuracy;
      float modMalfChance = 0.0f;
      float modDuraBurn = 0.0f;
      float modChamber = 0.0f;
      float modFlash = 0.0f;
      float modStability = 0.0f;
      float modHandling = 0.0f;
      float modLoudness = 0.0f;
      float modAimSpeed = 0.0f;
      string modType = dataObj.ModType;
      string modPosition = StatCalc.GetModPosition(mod, weapType, operationType, modType);
      StatCalc.ModConditionalStatCalc(__instance, weapStats, mod, dataObj, folded, weapType, operationType, ref hasShoulderContact, ref autoRof, ref semiRof, ref stockAllowsFSADS, ref verticalRecoil, ref horizontalRecoil, ref cameraRecoil, ref recoilAngle2, ref dispersion, ref ergonomics2, ref accuracy, ref modType, ref modPosition, ref modChamber, ref modLoudness, ref modMalfChance, ref modDuraBurn, ref convergence, ref modFlash, ref modStability, ref modHandling, ref modAimSpeed);
      StatCalc.ModStatCalc(mod, true, isChonker, modWeight, ref currentTorque, modPosition, modWeightFactored, autoRof, ref bFirerate, semiRof, ref currentSemiROF, cameraRecoil, ref recoilCamera, dispersion, ref recolDispersion, recoilAngle2, ref recoilAngle1, accuracy, ref currentCOI, ergonomics2, ref currentErgo, verticalRecoil, ref currentVRecoil, horizontalRecoil, ref currentHRecoil, ref pureErgo, 0.0f, ref currentShotDisp, ref currentMalfChance, 0.0f, ref pureRecoil, ref currentConv, convergence, ref visualMulti);
    }
    float totalErgoLessMag = 0.0f;
    float totalTorque = 0.0f;
    float totalErgo = 0.0f;
    float totalVRecoil = 0.0f;
    float totalHRecoil = 0.0f;
    float totalDispersion = 0.0f;
    float totalCamRecoil = 0.0f;
    float totalRecoilAngle = 0.0f;
    float num1 = 0.0f;
    float num2 = 0.0f;
    float totalErgoDelta = 0.0f;
    float totalVRecoilDelta = 0.0f;
    float totalHRecoilDelta = 0.0f;
    float totalCOI = 0.0f;
    float totalCOIDelta = 0.0f;
    float totalPureErgoDelta = 0.0f;
    StatCalc.WeaponStatCalc(__instance, weapStats, currentTorque, ref totalTorque, currentErgo, currentVRecoil, currentHRecoil, recolDispersion, recoilCamera, recoilAngle1, ergonomics1, recoilForceUp, recoilForceBack, ref totalErgo, ref totalVRecoil, ref totalHRecoil, ref totalDispersion, ref totalCamRecoil, ref totalRecoilAngle, ref num1, ref num2, ref totalErgoDelta, ref totalVRecoilDelta, ref totalHRecoilDelta, ref num1, ref num2, currentCOI, hasShoulderContact, ref totalCOI, ref totalCOIDelta, centerOfImpactBase, pureErgo, ref totalPureErgoDelta, ref totalErgoLessMag, 0.0f, true);
    UIWeaponStats.TotalConvergence = currentConv;
    UIWeaponStats.ConvergenceDelta = (currentConv - multiplierHandRotation) / multiplierHandRotation;
    UIWeaponStats.HasShoulderContact = hasShoulderContact;
    UIWeaponStats.Dispersion = totalDispersion;
    UIWeaponStats.CamRecoil = totalCamRecoil;
    UIWeaponStats.RecoilAngle = totalRecoilAngle;
    UIWeaponStats.TotalVRecoil = totalVRecoil;
    UIWeaponStats.TotalHRecoil = totalHRecoil;
    UIWeaponStats.Balance = totalTorque;
    UIWeaponStats.TotalErgo = totalErgo;
    UIWeaponStats.ErgoDelta = totalErgoDelta;
    UIWeaponStats.VRecoilDelta = totalVRecoilDelta;
    UIWeaponStats.HRecoilDelta = totalHRecoilDelta;
    UIWeaponStats.AutoFireRate = Mathf.Max(300, (int) bFirerate);
    UIWeaponStats.SemiFireRate = Mathf.Max(200, (int) currentSemiROF);
    UIWeaponStats.COIDelta = totalCOIDelta;
    UIWeaponStats.CamReturnSpeed = visualMulti;
  }
}
