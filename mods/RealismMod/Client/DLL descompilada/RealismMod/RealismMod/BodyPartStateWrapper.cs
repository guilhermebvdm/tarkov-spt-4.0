// Decompiled with JetBrains decompiler
// Type: RealismMod.BodyPartStateWrapper
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT.HealthSystem;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class BodyPartStateWrapper
{
  private readonly object bodyPartStateInstance;
  private FieldInfo isDestroyedField;
  private FieldInfo healthField;

  public BodyPartStateWrapper(object bodyPartStateInstance)
  {
    this.bodyPartStateInstance = bodyPartStateInstance;
    this.isDestroyedField = bodyPartStateInstance.GetType().GetField(nameof (IsDestroyed));
    this.healthField = bodyPartStateInstance.GetType().GetField(nameof (Health));
  }

  public bool IsDestroyed
  {
    get => (bool) this.isDestroyedField.GetValue(this.bodyPartStateInstance);
    set => this.isDestroyedField.SetValue(this.bodyPartStateInstance, (object) value);
  }

  public HealthValue Health
  {
    get => (HealthValue) this.healthField.GetValue(this.bodyPartStateInstance);
    set => this.healthField.SetValue(this.bodyPartStateInstance, (object) value);
  }
}
