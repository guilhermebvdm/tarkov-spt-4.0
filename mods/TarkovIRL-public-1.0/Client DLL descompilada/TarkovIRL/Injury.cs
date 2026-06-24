// Decompiled with JetBrains decompiler
// Type: TarkovIRL.Injury
// Assembly: TarkovIRL, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C42939BD-7BF0-4586-ABE5-9D2EFC361A0B
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\TarkovIRL_WeaponsHandlingMod_1.0.0\BepInEx\plugins\TarkovIRL.dll

#nullable disable
namespace TarkovIRL;

internal class Injury
{
  public Injury(Injury.EInjury type, float time)
  {
    this.InjuryType = type;
    this.TimeInflicted = time;
  }

  public Injury.EInjury InjuryType { get; set; }

  public float TimeInflicted { get; set; }

  public float TimeUntilEffect
  {
    get
    {
      if (this.InjuryType == Injury.EInjury.LIGHT_BLEED)
        return 60f;
      return this.InjuryType == Injury.EInjury.HEAVY_BLEED ? 30f : 20f;
    }
  }

  public float InjuryWeight
  {
    get
    {
      if (this.InjuryType == Injury.EInjury.LIGHT_BLEED)
        return 5f;
      return this.InjuryType == Injury.EInjury.HEAVY_BLEED ? 20f : 20f;
    }
  }

  public enum EInjury
  {
    LIGHT_BLEED,
    HEAVY_BLEED,
    BONE_BREAK,
  }
}
