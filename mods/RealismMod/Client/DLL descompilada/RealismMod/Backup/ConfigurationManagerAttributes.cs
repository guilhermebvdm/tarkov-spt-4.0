// Decompiled with JetBrains decompiler
// Type: ConfigurationManagerAttributes
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using BepInEx.Configuration;
using System;

#nullable disable
internal sealed class ConfigurationManagerAttributes
{
  public bool? ShowRangeAsPercent;
  public Action<ConfigEntryBase> CustomDrawer;
  public bool? Browsable;
  public string Category;
  public object DefaultValue;
  public bool? HideDefaultButton;
  public bool? HideSettingName;
  public string Description;
  public string DispName;
  public int? Order;
  public bool? ReadOnly;
  public bool? IsAdvanced;
  public Func<object, string> ObjToStr;
  public Func<string, object> StrToObj;
}
