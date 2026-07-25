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
      return this.InjuryType == Injury.EInjury.LIGHT_BLEED ? 60f : (this.InjuryType == Injury.EInjury.HEAVY_BLEED ? 30f : 20f);
    }
  }

  public float InjuryWeight
  {
    get
    {
      return this.InjuryType == Injury.EInjury.LIGHT_BLEED ? 5f : (this.InjuryType == Injury.EInjury.HEAVY_BLEED ? 20f : 20f);
    }
  }

  public enum EInjury
  {
    LIGHT_BLEED,
    HEAVY_BLEED,
    BONE_BREAK,
  }
}

