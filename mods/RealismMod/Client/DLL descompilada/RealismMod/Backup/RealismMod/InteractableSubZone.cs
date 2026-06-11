// Decompiled with JetBrains decompiler
// Type: RealismMod.InteractableSubZone
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

#nullable disable
namespace RealismMod;

public class InteractableSubZone
{
  public EIneractableType InteractionType { get; set; }

  public int CompletionStep { get; set; } = 0;

  public EInteractableState StartingState { get; set; } = EInteractableState.On;

  public EInteractableState DesiredEndState { get; set; } = EInteractableState.Off;

  public string TargeObject { get; set; }

  public EIneractableAction[] InteractionAction { get; set; }

  public float PartialCompletionModifier { get; set; }

  public float FullCompletionModifer { get; set; } = 0.0f;

  public string[] ZoneNames { get; set; }

  public bool Randomize { get; set; }
}
