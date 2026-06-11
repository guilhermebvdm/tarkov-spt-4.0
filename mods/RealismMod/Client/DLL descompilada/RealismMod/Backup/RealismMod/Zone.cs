// Decompiled with JetBrains decompiler
// Type: RealismMod.Zone
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

#nullable disable
namespace RealismMod;

public class Zone
{
  public string Name { get; set; }

  public float Strength { get; set; }

  public bool UsesDistanceFalloff { get; set; }

  public bool BlockNav { get; set; } = true;

  public Analysable Analysable { get; set; }

  public InteractableSubZone Interactable { get; set; }

  public string AudioFile { get; set; }

  public Position Position { get; set; }

  public Rotation Rotation { get; set; }

  public Size Size { get; set; }

  public bool UseVisual { get; set; } = true;

  public float VisParticleRate { get; set; } = 40f;

  public float VisOpacityModi { get; set; } = 1f;

  public float VisSpeedModi { get; set; } = 1f;

  public float VisZoneSizeMulti { get; set; } = 0.85f;

  public bool VisUsePhysics { get; set; } = false;
}
