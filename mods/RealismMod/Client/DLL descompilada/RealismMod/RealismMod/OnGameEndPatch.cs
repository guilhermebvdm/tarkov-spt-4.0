// Decompiled with JetBrains decompiler
// Type: RealismMod.OnGameEndPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Comfort.Common;
using EFT;
using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class OnGameEndPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (GameWorld).GetMethod("OnDestroy", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPrefix]
  private static void PatchPrefix(GameWorld __instance)
  {
    if (Plugin.ServerConfig.enable_hazard_zones)
    {
      ISession clientBackEndSession = Singleton<ClientApplication<ISession>>.Instance?.GetClientBackEndSession();
      if (((IPlayerAndPetProfile) clientBackEndSession)?.Profile?.Info != null)
        ProfileData.PMCLevel = ((IPlayerAndPetProfile) clientBackEndSession).Profile.Info.Level;
      HazardTracker.ResetTracker();
      HazardTracker.UpdateHazardValues(ProfileData.CurrentProfileId);
      HazardTracker.SaveHazardValues();
      HazardTracker.GetHazardValues(ProfileData.PMCProfileId);
      HazardPlayerSpawnManager.RestOnRaidEnd();
    }
    GameWorldController.Reset();
    Plugin.RealismAudioController.ClipsAreReady = false;
  }
}
