// Decompiled with JetBrains decompiler
// Type: RealismMod.DamageTracker
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace RealismMod;

public class DamageTracker
{
  public Dictionary<EDamageType, Dictionary<EBodyPart, float>> DamageRecord = new Dictionary<EDamageType, Dictionary<EBodyPart, float>>();
  public float TotalHeavyBleedDamage = 0.0f;
  public float TotalLightBleedDamage = 0.0f;
  public float TotalDehydrationDamage = 0.0f;
  public float TotalExhaustionDamage = 0.0f;

  public void UpdateDamage(EDamageType damageType, EBodyPart bodyPart, float damage)
  {
    EDamageType edamageType = damageType;
    if (edamageType != 16384 /*0x4000*/)
    {
      if (edamageType == 32768 /*0x8000*/)
      {
        this.TotalHeavyBleedDamage += damage;
      }
      else
      {
        if (!this.DamageRecord.ContainsKey(damageType))
          this.DamageRecord[damageType] = new Dictionary<EBodyPart, float>();
        Dictionary<EBodyPart, float> dictionary = this.DamageRecord[damageType];
        if (!dictionary.ContainsKey(bodyPart))
          dictionary[bodyPart] = damage;
        else
          dictionary[bodyPart] += damage;
      }
    }
    else
      this.TotalLightBleedDamage += damage;
  }

  public void ResetTracker()
  {
    if (this.DamageRecord.Any<KeyValuePair<EDamageType, Dictionary<EBodyPart, float>>>())
      this.DamageRecord.Clear();
    this.TotalHeavyBleedDamage = 0.0f;
    this.TotalLightBleedDamage = 0.0f;
    this.TotalDehydrationDamage = 0.0f;
    this.TotalExhaustionDamage = 0.0f;
  }
}
