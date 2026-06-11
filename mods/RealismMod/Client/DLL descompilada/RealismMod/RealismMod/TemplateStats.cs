// Decompiled with JetBrains decompiler
// Type: RealismMod.TemplateStats
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using Newtonsoft.Json;
using SPT.Common.Http;
using System;
using System.Collections.Generic;

#nullable disable
namespace RealismMod;

public static class TemplateStats
{
  public static Dictionary<string, Gun> GunStats = new Dictionary<string, Gun>();
  public static Dictionary<string, Gear> GearStats = new Dictionary<string, Gear>();
  public static Dictionary<string, WeaponMod> WeaponModStats = new Dictionary<string, WeaponMod>();
  public static Dictionary<string, Consumable> ConsumableStats = new Dictionary<string, Consumable>();
  public static Dictionary<string, Ammo> RealismAmmoStats = new Dictionary<string, Ammo>();
  public static Dictionary<string, RealismItem> IncompatileItems = new Dictionary<string, RealismItem>();

  public static T GetDataObj<T>(Dictionary<string, T> stats, string itemId) where T : RealismItem, new()
  {
    T obj1;
    if (stats.TryGetValue(itemId, out obj1))
    {
      T obj2;
      return obj1.TemplateID != null && stats.TryGetValue(obj1.TemplateID, out obj2) ? obj2 : obj1;
    }
    RealismItem dataObj1;
    if (TemplateStats.IncompatileItems.TryGetValue(itemId, out dataObj1))
      return dataObj1 as T;
    T dataObj2 = new T();
    TemplateStats.IncompatileItems.Add(itemId, (RealismItem) dataObj2);
    return dataObj2;
  }

  public static void AddItemToDict<T>(Dictionary<string, T> dict, RealismItem item) where T : RealismItem
  {
    if (!dict.ContainsKey(MongoID.op_Implicit(item.ItemID)))
      dict.Add(MongoID.op_Implicit(item.ItemID), item as T);
    else
      Utils.Logger.LogWarning((object) $"Realism Mod: item with duplicate Id already added to dictionary {item.ItemID}");
  }

  public static void RequestAndProcessDataFromServer()
  {
    string str = "/RealismMod/GetTemplateData";
    new JsonSerializerSettings().MissingMemberHandling = (MissingMemberHandling) 0;
    try
    {
      TemplateStats.DeserializeTemplates(RequestHandler.GetJson(str));
    }
    catch (Exception ex)
    {
      Utils.Logger.LogError((object) $"REALISM MOD ERROR: FAILED TO FETCH DATA FROM SERVER USING ROUTE {str}: {ex.Message}");
    }
  }

  public static void DeserializeTemplates(string templateData)
  {
    JsonSerializerSettings serializerSettings = new JsonSerializerSettings()
    {
      TypeNameHandling = (TypeNameHandling) 4
    };
    foreach (KeyValuePair<string, RealismItem> keyValuePair in JsonConvert.DeserializeObject<Dictionary<string, RealismItem>>(templateData, serializerSettings))
    {
      RealismItem realismItem = keyValuePair.Value;
      switch (realismItem)
      {
        case Gun gun:
          TemplateStats.AddItemToDict<Gun>(TemplateStats.GunStats, (RealismItem) gun);
          break;
        case Gear gear:
          TemplateStats.AddItemToDict<Gear>(TemplateStats.GearStats, (RealismItem) gear);
          break;
        case WeaponMod weaponMod:
          TemplateStats.AddItemToDict<WeaponMod>(TemplateStats.WeaponModStats, (RealismItem) weaponMod);
          break;
        case Consumable consumable:
          TemplateStats.AddItemToDict<Consumable>(TemplateStats.ConsumableStats, (RealismItem) consumable);
          break;
        case Ammo ammo:
          TemplateStats.AddItemToDict<Ammo>(TemplateStats.RealismAmmoStats, (RealismItem) ammo);
          break;
        default:
          Utils.Logger.LogFatal((object) $"Realism Mod: Invalid Template found at {templateData}, id: {realismItem.ItemID}");
          break;
      }
    }
  }
}
