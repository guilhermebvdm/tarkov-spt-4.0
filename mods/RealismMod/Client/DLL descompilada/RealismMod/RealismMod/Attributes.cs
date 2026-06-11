// Decompiled with JetBrains decompiler
// Type: RealismMod.Attributes
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

#nullable disable
namespace RealismMod;

public static class Attributes
{
  public static string GetName(this Attributes.ENewItemAttributeId id)
  {
    switch (id)
    {
      case Attributes.ENewItemAttributeId.HorizontalRecoil:
        return "HORIZONTAL RECOIL";
      case Attributes.ENewItemAttributeId.VerticalRecoil:
        return "VERTICAL RECOIL";
      case Attributes.ENewItemAttributeId.Balance:
        return "BALANCE";
      case Attributes.ENewItemAttributeId.CameraRecoil:
        return "CAMERA RECOIL";
      case Attributes.ENewItemAttributeId.Dispersion:
        return "DISPERSION";
      case Attributes.ENewItemAttributeId.MalfunctionChance:
        return "MALFUNCTION CHANCE";
      case Attributes.ENewItemAttributeId.AutoROF:
        return "AUTO FIRE RATE";
      case Attributes.ENewItemAttributeId.SemiROF:
        return "SEMI FIRE RATE";
      case Attributes.ENewItemAttributeId.RecoilAngle:
        return "RECOIL ANGLE";
      case Attributes.ENewItemAttributeId.ReloadSpeed:
        return "RELOAD SPEED";
      case Attributes.ENewItemAttributeId.FixSpeed:
        return "FIX SPEED";
      case Attributes.ENewItemAttributeId.AimSpeed:
        return "AIM SPEED";
      case Attributes.ENewItemAttributeId.ChamberSpeed:
        return "CHAMBER SPEED";
      case Attributes.ENewItemAttributeId.Firerate:
        return "FIRE RATE";
      case Attributes.ENewItemAttributeId.Damage:
        return "DAMAGE";
      case Attributes.ENewItemAttributeId.Penetration:
        return "PENETRATION";
      case Attributes.ENewItemAttributeId.ArmorDamage:
        return "ARMOR DAMAGE";
      case Attributes.ENewItemAttributeId.FragmentationChance:
        return "FRAGMENTATION CHANCE";
      case Attributes.ENewItemAttributeId.BluntThroughput:
        return "AVG. BLUNT DAMAGE REDUCTION";
      case Attributes.ENewItemAttributeId.ShotDispersion:
        return "BUCKSHOT SPREAD";
      case Attributes.ENewItemAttributeId.GearReloadSpeed:
        return "RELOAD SPEED";
      case Attributes.ENewItemAttributeId.CanSpall:
        return "CAN SPALL";
      case Attributes.ENewItemAttributeId.SpallReduction:
        return "SPALLING REDUCTION";
      case Attributes.ENewItemAttributeId.CantADS:
        return "BLOCKS AIMING DOWN SIGHTS";
      case Attributes.ENewItemAttributeId.CanADS:
        return "ALLOWS AIMING WITH FACESHIELD";
      case Attributes.ENewItemAttributeId.NoiseReduction:
        return "NOISE REDUCTION RATING";
      case Attributes.ENewItemAttributeId.ProjectileCount:
        return "PROJECTILES";
      case Attributes.ENewItemAttributeId.Convergence:
        return "FLATNESS";
      case Attributes.ENewItemAttributeId.HBleedType:
        return "HEAVY BLEED HEAL TYPE";
      case Attributes.ENewItemAttributeId.LimbHpPerTick:
        return "TOURNIQUET HP LOSS PER TICK";
      case Attributes.ENewItemAttributeId.HpPerTick:
        return "HP CHANGE PER TICK";
      case Attributes.ENewItemAttributeId.RemoveTrnqt:
        return "REMOVES TOURNIQUET EFFECT";
      case Attributes.ENewItemAttributeId.Comfort:
        return "COMFORT MODIFIER";
      case Attributes.ENewItemAttributeId.GasProtection:
        return "GAS PROTECTION";
      case Attributes.ENewItemAttributeId.RadProtection:
        return "RADIATION PROTECTION";
      case Attributes.ENewItemAttributeId.PainKillerStrength:
        return "PAIN RELIEF STRENGTH";
      case Attributes.ENewItemAttributeId.MeleeDamage:
        return "MELEE DAMAGE";
      case Attributes.ENewItemAttributeId.MeleePen:
        return "MELEE PENETRATION";
      case Attributes.ENewItemAttributeId.BallisticCoefficient:
        return "BALLISTIC COEFFICIENT";
      case Attributes.ENewItemAttributeId.OutOfRaidHP:
        return "OUT-OF-RAID HP RESTORATION";
      case Attributes.ENewItemAttributeId.StimType:
        return "STIM TYPE";
      case Attributes.ENewItemAttributeId.DurabilityBurn:
        return "DURABILITY BURN";
      case Attributes.ENewItemAttributeId.Heat:
        return "HEAT";
      case Attributes.ENewItemAttributeId.MuzzleFlash:
        return "MUZZLE FLASH REDUCTION";
      case Attributes.ENewItemAttributeId.Gas:
        return "VISUAL GAS";
      case Attributes.ENewItemAttributeId.AimStability:
        return "STABILITY";
      case Attributes.ENewItemAttributeId.Handling:
        return "HANDLING";
      default:
        return id.ToString();
    }
  }

  public enum ENewItemAttributeId
  {
    HorizontalRecoil = 0,
    VerticalRecoil = 1,
    Balance = 11, // 0x0000000B
    CameraRecoil = 12, // 0x0000000C
    Dispersion = 13, // 0x0000000D
    MalfunctionChance = 14, // 0x0000000E
    AutoROF = 15, // 0x0000000F
    SemiROF = 16, // 0x00000010
    RecoilAngle = 17, // 0x00000011
    ReloadSpeed = 18, // 0x00000012
    FixSpeed = 19, // 0x00000013
    AimSpeed = 20, // 0x00000014
    ChamberSpeed = 21, // 0x00000015
    Firerate = 22, // 0x00000016
    Damage = 23, // 0x00000017
    Penetration = 24, // 0x00000018
    ArmorDamage = 25, // 0x00000019
    FragmentationChance = 26, // 0x0000001A
    BluntThroughput = 27, // 0x0000001B
    ShotDispersion = 28, // 0x0000001C
    GearReloadSpeed = 29, // 0x0000001D
    CanSpall = 30, // 0x0000001E
    SpallReduction = 31, // 0x0000001F
    CantADS = 32, // 0x00000020
    CanADS = 33, // 0x00000021
    NoiseReduction = 34, // 0x00000022
    ProjectileCount = 35, // 0x00000023
    Convergence = 36, // 0x00000024
    HBleedType = 37, // 0x00000025
    LimbHpPerTick = 38, // 0x00000026
    HpPerTick = 39, // 0x00000027
    RemoveTrnqt = 40, // 0x00000028
    Comfort = 41, // 0x00000029
    GasProtection = 42, // 0x0000002A
    RadProtection = 43, // 0x0000002B
    PainKillerStrength = 44, // 0x0000002C
    MeleeDamage = 45, // 0x0000002D
    MeleePen = 46, // 0x0000002E
    BallisticCoefficient = 47, // 0x0000002F
    OutOfRaidHP = 48, // 0x00000030
    StimType = 49, // 0x00000031
    DurabilityBurn = 50, // 0x00000032
    Heat = 51, // 0x00000033
    MuzzleFlash = 52, // 0x00000034
    Gas = 53, // 0x00000035
    AimStability = 54, // 0x00000036
    Handling = 55, // 0x00000037
  }
}
