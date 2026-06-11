// Decompiled with JetBrains decompiler
// Type: RealismMod.MedProperties
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using System;
using System.Collections.Generic;

#nullable disable
namespace RealismMod;

public static class MedProperties
{
  public static readonly Dictionary<string, Type> EffectTypes = new Dictionary<string, Type>()
  {
    {
      "PainKiller",
      typeof (GInterface332)
    },
    {
      "Tremor",
      typeof (GInterface335)
    },
    {
      "BrokenBone",
      typeof (GInterface316)
    },
    {
      "TunnelVision",
      typeof (GInterface337)
    },
    {
      "Contusion",
      typeof (GInterface326)
    },
    {
      "HeavyBleeding",
      typeof (GInterface314)
    },
    {
      "LightBleeding",
      typeof (GInterface313)
    },
    {
      "Dehydration",
      typeof (GInterface317)
    },
    {
      "Exhaustion",
      typeof (GInterface318)
    },
    {
      "LethalToxin",
      typeof (GInterface320)
    },
    {
      "Intoxication",
      typeof (GInterface321)
    }
  };
}
