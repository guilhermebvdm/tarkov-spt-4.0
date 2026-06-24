// Decompiled with JetBrains decompiler
// Type: TarkovIRL.ParallaxAdsController
// Assembly: TarkovIRL, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C42939BD-7BF0-4586-ABE5-9D2EFC361A0B
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\TarkovIRL_WeaponsHandlingMod_1.0.0\BepInEx\plugins\TarkovIRL.dll

using EFT.InventoryLogic;
using RealismMod;
using UnityEngine;

#nullable disable
namespace TarkovIRL;

public static class ParallaxAdsController
{
  private static float _parallaxWeight = 1f;
  private static bool _intoAds = false;
  private static float _adsSpeedMod = 1f;
  private static float _adsLerpWeight = 1f;
  private static bool _shotSwitch = false;
  private static bool _intoShot = false;
  private static float _shotLerp = 1f;
  private static float _shotWeight = 1f;
  private static readonly float _HeadTiltValY = 2f;
  private static readonly float _HeadTiltValZ = 6f;

  public static void UpdateLerps(float dt)
  {
    ParallaxAdsController.LerpAds(dt);
    ParallaxAdsController.LerpShot(dt);
    ParallaxAdsController.DefineParallaxWeight();
  }

  public static void StartNewAds(bool intoAds, float wpnMulti)
  {
    ParallaxAdsController._intoAds = intoAds;
    ParallaxAdsController._adsSpeedMod = wpnMulti;
  }

  private static void LerpAds(float dt)
  {
    float num = ParallaxAdsController._intoAds ? PrimeMover.ParallaxInAds.Value : 1f;
    ParallaxAdsController._adsLerpWeight = Mathf.Lerp(ParallaxAdsController._adsLerpWeight, num, dt * ParallaxAdsController._adsSpeedMod * PrimeMover.AdsParallaxTimeMulti.Value);
  }

  private static void LerpShot(float dt)
  {
    if (ParallaxAdsController._intoShot)
    {
      ParallaxAdsController._shotLerp = Mathf.Lerp(ParallaxAdsController._shotLerp, ParallaxAdsController._shotWeight * 1.05f, (float) ((double) dt * (double) ParallaxAdsController._shotWeight * (double) PrimeMover.ShotParallaxResetTimeMulti.Value * 3.0));
      if ((double) ParallaxAdsController._shotLerp < (double) ParallaxAdsController._shotWeight * 0.949999988079071)
        return;
      ParallaxAdsController._intoShot = false;
    }
    else
    {
      ParallaxAdsController._shotLerp = Mathf.Lerp(ParallaxAdsController._shotLerp, ParallaxAdsController._adsLerpWeight * 0.95f, dt * (1f / ParallaxAdsController._shotWeight) * PrimeMover.ShotParallaxResetTimeMulti.Value);
      if ((double) ParallaxAdsController._shotLerp <= (double) ParallaxAdsController._adsLerpWeight)
        ParallaxAdsController._shotSwitch = false;
    }
  }

  public static void StartNewShot(Weapon weapon)
  {
    float num = ((Item) weapon).Weight * PrimeMover.ShotParallaxWeaponWeightMulti.Value;
    ParallaxAdsController._shotWeight = weapon.CurrentAmmoTemplate.BulletMassGram / num;
    ParallaxAdsController._shotSwitch = true;
    ParallaxAdsController._intoShot = true;
  }

  private static void DefineParallaxWeight()
  {
    ParallaxAdsController._parallaxWeight = ParallaxAdsController._shotSwitch ? ParallaxAdsController._shotLerp : ParallaxAdsController._adsLerpWeight;
  }

  public static float ParallaxWeight => ParallaxAdsController._parallaxWeight;

  public static void GetParallaxADSHeadTilt(out float Y, out float Z)
  {
    if (PlayerMotionController.IsAiming && WeaponController.HasCheekWeld())
    {
      float num = StanceController.IsLeftShoulder ? -1f : 1f;
      Y = ParallaxAdsController._HeadTiltValY * ParallaxAdsController._adsLerpWeight * num;
      Z = ParallaxAdsController._HeadTiltValZ * ParallaxAdsController._adsLerpWeight * num;
    }
    else
    {
      Y = 0.0f;
      Z = 0.0f;
    }
  }
}
