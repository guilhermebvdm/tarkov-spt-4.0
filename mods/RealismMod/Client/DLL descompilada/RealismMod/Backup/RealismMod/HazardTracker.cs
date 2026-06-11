// Decompiled with JetBrains decompiler
// Type: RealismMod.HazardTracker
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Comfort.Common;
using EFT;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

#nullable disable
namespace RealismMod;

public static class HazardTracker
{
  private static Dictionary<string, HazardRecord> _hazardRecords = new Dictionary<string, HazardRecord>();
  private static float _totalToxicity = 0.0f;
  private static float _totalRadiation = 0.0f;
  private static float _toxicityRateMeds = 0.0f;
  private static float _radiationRateMeds = 0.0f;
  private static float _upateTimer = 0.0f;
  private static float _hideoutRegenTick = 0.0f;
  public static bool _loadedData = false;
  public static int _medStationLevel = 0;
  public static int _ventsLevel = 0;

  public static float TotalToxicityRate { get; set; } = 0.0f;

  public static float TotalRadiationRate { get; set; } = 0.0f;

  public static float BaseTotalToxicityRate { get; set; } = 0.0f;

  public static float BaseTotalRadiationRate { get; set; } = 0.0f;

  public static float RadTreatmentRate
  {
    get => HazardTracker._radiationRateMeds;
    set => HazardTracker._radiationRateMeds = Mathf.Clamp(value, -0.2f, 0.0f);
  }

  public static float DetoxicationRate
  {
    get => HazardTracker._toxicityRateMeds;
    set => HazardTracker._toxicityRateMeds = Mathf.Clamp(value, -0.15f, 0.0f);
  }

  public static float TotalToxicity
  {
    get => HazardTracker._totalToxicity;
    set => HazardTracker._totalToxicity = Mathf.Clamp(value, 0.0f, 100f);
  }

  public static float TotalRadiation
  {
    get => HazardTracker._totalRadiation;
    set => HazardTracker._totalRadiation = Mathf.Clamp(value, 0.0f, 100f);
  }

  public static int MedStationLevel => HazardTracker._medStationLevel;

  public static void OutOfRaidUpdate()
  {
    if (GameWorldController.IsInRaid())
      return;
    HazardTracker._upateTimer += Time.deltaTime;
    HazardTracker._hideoutRegenTick += Time.deltaTime;
    if ((double) HazardTracker._hideoutRegenTick > 1.0)
    {
      Profile profile = ((IPlayerAndPetProfile) Singleton<ClientApplication<ISession>>.Instance.GetClientBackEndSession())?.Profile;
      EnergyControllerClass energyController = Singleton<HideoutClass>.Instance?.EnergyController;
      if (profile != null && HazardTracker._loadedData && energyController != null && energyController.IsEnergyGenerationOn)
      {
        HazardTracker._medStationLevel = profile.Hideout.Areas[7].Level;
        HazardTracker._ventsLevel = profile.Hideout.Areas[0].Level;
        HazardTracker.TotalToxicityRate = (float) -((double) HazardTracker._ventsLevel * 0.0099999997764825821) + (float) -((double) HazardTracker._medStationLevel * 0.02500000037252903);
        HazardTracker.TotalToxicity += HazardTracker.TotalToxicityRate;
      }
      else
        HazardTracker.TotalToxicityRate = 0.0f;
      HazardTracker._hideoutRegenTick = 0.0f;
    }
    if ((double) HazardTracker._upateTimer <= 10.0 || !HazardTracker._loadedData)
      return;
    HazardTracker.UpdateHazardValues(ProfileData.PMCProfileId);
    HazardTracker.SaveHazardValues();
    HazardTracker._upateTimer = 0.0f;
  }

  private static string GetFilePath()
  {
    return AppDomain.CurrentDomain.BaseDirectory + "\\BepInEx\\plugins\\Realism\\data\\hazard_tracker.json";
  }

  public static int GetNextLowestHazardLevel(int value) => value / 10 * 10;

  public static void ResetTracker()
  {
    HazardTracker._totalToxicity = (float) HazardTracker.GetNextLowestHazardLevel((int) HazardTracker._totalToxicity);
    HazardTracker.DetoxicationRate = 0.0f;
    HazardTracker.RadTreatmentRate = 0.0f;
    HazardTracker.TotalToxicityRate = 0.0f;
    HazardTracker.TotalRadiationRate = 0.0f;
  }

  public static void WipeTracker()
  {
    HazardTracker._totalToxicity = 0.0f;
    HazardTracker._totalRadiation = 0.0f;
  }

  public static void UpdateHazardValues(string profileId)
  {
    HazardRecord hazardRecord = (HazardRecord) null;
    if (!HazardTracker._hazardRecords.TryGetValue(profileId, out hazardRecord))
      return;
    bool flag = HazardTracker.IsWipedProfile();
    hazardRecord.RecordedTotalToxicity = flag ? 0.0f : HazardTracker.TotalToxicity;
    hazardRecord.RecordedTotalRadiation = flag ? 0.0f : HazardTracker.TotalRadiation;
  }

  public static void SaveHazardValues()
  {
    string contents = JsonConvert.SerializeObject((object) HazardTracker._hazardRecords, (Formatting) 1);
    File.WriteAllText(HazardTracker.GetFilePath(), contents);
  }

  private static bool IsWipedProfile() => ProfileData.PMCLevel <= 1;

  public static void GetHazardValues(string profileId)
  {
    string str = File.ReadAllText(HazardTracker.GetFilePath());
    if (string.IsNullOrWhiteSpace(str) || str.Trim() == "{}")
      Utils.Logger.LogWarning((object) "Realism Mod: Hazard Data JSON File is empty");
    else
      HazardTracker._hazardRecords = JsonConvert.DeserializeObject<Dictionary<string, HazardRecord>>(str);
    if (HazardTracker._hazardRecords == null)
      HazardTracker._hazardRecords = new Dictionary<string, HazardRecord>();
    HazardRecord hazardRecord = (HazardRecord) null;
    if (HazardTracker._hazardRecords.TryGetValue(profileId, out hazardRecord))
    {
      bool flag = HazardTracker.IsWipedProfile();
      HazardTracker.TotalToxicity = flag ? 0.0f : hazardRecord.RecordedTotalToxicity;
      HazardTracker.TotalRadiation = flag ? 0.0f : hazardRecord.RecordedTotalRadiation;
    }
    else
    {
      HazardTracker._hazardRecords.Add(profileId, new HazardRecord()
      {
        RecordedTotalRadiation = 0.0f,
        RecordedTotalToxicity = 0.0f
      });
      HazardTracker.TotalToxicity = 0.0f;
      HazardTracker.TotalRadiation = 0.0f;
      HazardTracker.SaveHazardValues();
    }
    HazardTracker._loadedData = true;
  }
}
