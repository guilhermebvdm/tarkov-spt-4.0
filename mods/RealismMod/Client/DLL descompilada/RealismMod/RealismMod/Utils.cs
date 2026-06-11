// Decompiled with JetBrains decompiler
// Type: RealismMod.Utils
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using BepInEx.Logging;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

#nullable disable
namespace RealismMod;

public static class Utils
{
  private const float FPS_BASELINE = 144f;
  public const string GAMU_ID = "66fd571a05370c3ee1a1c613";
  public const string RAMU_ID = "66fd521442055447e2304fda";
  public const string GAMU_DATA_ID = "670120df4f0c4c37e6be90ae";
  public const string RAMU_DATA_ID = "670120ce354987453daf3d0c";
  public const string HALLOWEEN_TRANSMITTER_ID = "6703082a766cb6d11310094e";
  public static ManualLogSource Logger;
  public static Random SystemRandom = new Random();
  public static bool PlayerIsReady = false;
  public static bool IsInHideout = false;
  public static bool WeaponIsReady = false;
  public static bool HasRunErgoWeightCalc = false;
  public static string Silencer = "550aa4cd4bdc2dd8348b456c";
  public static string FlashHider = "550aa4bf4bdc2dd6348b456b";
  public static string MuzzleCombo = "550aa4dd4bdc2dc9348b4569";
  public static string Barrel = "555ef6e44bdc2de9068b457e";
  public static string Mount = "55818b224bdc2dde698b456f";
  public static string Receiver = "55818a304bdc2db5418b457d";
  public static string Stock = "55818a594bdc2db9688b456a";
  public static string Charge = "55818a6f4bdc2db9688b456b";
  public static string CompactCollimator = "55818acf4bdc2dde698b456b";
  public static string Collimator = "55818ad54bdc2ddc698b4569";
  public static string AssaultScope = "55818add4bdc2d5b648b456f";
  public static string Scope = "55818ae44bdc2dde698b456c";
  public static string IronSight = "55818ac54bdc2d5b648b456e";
  public static string SpecialScope = "55818aeb4bdc2ddc698b456a";
  public static string AuxiliaryMod = "5a74651486f7744e73386dd1";
  public static string Foregrip = "55818af64bdc2d5b648b4570";
  public static string PistolGrip = "55818a684bdc2ddd698b456d";
  public static string Gasblock = "56ea9461d2720b67698b456f";
  public static string Handguard = "55818a104bdc2db9688b4569";
  public static string Bipod = "55818afb4bdc2dde698b456d";
  public static string Flashlight = "55818b084bdc2d5b648b4571";
  public static string TacticalCombo = "55818b164bdc2ddc698b456c";
  public static string UBGL = "55818b014bdc2ddc698b456b";

  public static float GetFPSFactor()
  {
    return Mathf.Clamp((double) Plugin.FPS >= 1.0 ? 144f / Plugin.FPS : 1f, 0.05f, 5f);
  }

  public static bool IPlayerMatches(IPlayer x) => x.ProfileId == Utils.GetYourPlayer().ProfileId;

  public static async Task LoadLoot(Vector3 position, Quaternion rotation, string templateId)
  {
    Item item = Singleton<ItemFactoryClass>.Instance.CreateItem(Utils.GenId(), templateId, (GClass825) null);
    item.StackObjectsCount = 1;
    item.SpawnedInSession = true;
    ResourceKey[] resources = item.Template.AllResources.ToArray<ResourceKey>();
    await Utils.LoadBundle(resources);
    IPlayer player = Singleton<GameWorld>.Instance.RegisteredPlayers.FirstOrDefault<IPlayer>(new Func<IPlayer, bool>(Utils.IPlayerMatches));
    Singleton<GameWorld>.Instance.ThrowItem(item, player, position, rotation, Vector3.zero, Vector3.zero, true, true, EFTHardSettings.Instance.ThrowLootMakeVisibleDelay);
    item = (Item) null;
    resources = (ResourceKey[]) null;
    player = (IPlayer) null;
  }

  public static async Task LoadBundle(ResourceKey[] resources)
  {
    await Singleton<PoolManagerClass>.Instance.LoadBundlesAndCreatePools((PoolManagerClass.PoolsCategory) 0, (PoolManagerClass.AssemblyType) 0, (ICollection<ResourceKey>) resources, JobPriorityClass.Immediate, (IProgress<LoadingProgressStruct>) null, PoolManagerClass.DefaultCancellationToken);
  }

  public static Vector3 ClampVector(Vector3 value, Vector3 min, Vector3 max)
  {
    return new Vector3(Mathf.Clamp(value.x, min.x, max.x), Mathf.Clamp(value.y, min.y, max.y), Mathf.Clamp(value.z, min.z, max.z));
  }

  public static bool AreVector2sEqual(Vector2 a, Vector2 b, float epsilon = 0.001f)
  {
    return (double) Vector2.Distance(a, b) < (double) epsilon;
  }

  public static bool IsGreaterThanOrEqualTo(float a, float b, float epsilon = 0.0001f)
  {
    return Utils.IsGreaterThan(a, b, epsilon) || Utils.AreFloatsEqual(a, b, epsilon);
  }

  public static bool IsLessThanOrEqualTo(float a, float b, float epsilon = 0.0001f)
  {
    return Utils.IsLessThan(a, b, epsilon) || Utils.AreFloatsEqual(a, b, epsilon);
  }

  public static bool IsLessThan(float a, float b, float epsilon = 0.0001f)
  {
    return (double) b - (double) a > (double) epsilon;
  }

  public static bool IsGreaterThan(float a, float b, float epsilon = 0.0001f)
  {
    return (double) a - (double) b > (double) epsilon;
  }

  public static bool AreFloatsEqual(float a, float b, float epsilon = 0.0001f)
  {
    return (double) Math.Abs(a - b) < (double) epsilon;
  }

  public static string GetRandomWeightedKey(Dictionary<string, int> items)
  {
    string randomWeightedKey = "";
    int maxValue = 0;
    foreach (KeyValuePair<string, int> keyValuePair in items)
      maxValue += keyValuePair.Value;
    int num1 = Utils.SystemRandom.Next(maxValue);
    foreach (KeyValuePair<string, int> keyValuePair in items)
    {
      int num2 = keyValuePair.Value;
      if (num1 >= num2)
      {
        num1 -= num2;
      }
      else
      {
        randomWeightedKey = keyValuePair.Key;
        break;
      }
    }
    return randomWeightedKey;
  }

  public static Player GetYourPlayer()
  {
    GameWorld instance = Singleton<GameWorld>.Instance;
    return Object.op_Equality((Object) instance, (Object) null) ? (Player) null : instance.MainPlayer;
  }

  public static Player GetPlayerByProfileId(string id)
  {
    return Singleton<GameWorld>.Instance.GetAlivePlayerByProfileID(id);
  }

  public static bool CheckIsReady()
  {
    GameWorld instance1 = Singleton<GameWorld>.Instance;
    SessionResultPanel instance2 = Singleton<SessionResultPanel>.Instance;
    Player mainPlayer = instance1?.MainPlayer;
    if (Object.op_Inequality((Object) mainPlayer, (Object) null))
    {
      Utils.WeaponIsReady = Object.op_Inequality((Object) mainPlayer?.HandsController, (Object) null) && mainPlayer?.HandsController?.Item != null && mainPlayer?.HandsController?.Item is Weapon;
      Utils.IsInHideout = mainPlayer is HideoutPlayer;
    }
    else
    {
      Utils.WeaponIsReady = false;
      Utils.IsInHideout = false;
    }
    if (Object.op_Equality((Object) instance1, (Object) null) || instance1.AllAlivePlayersList == null || Object.op_Equality((Object) instance1.MainPlayer, (Object) null) || Object.op_Inequality((Object) instance2, (Object) null))
    {
      Utils.PlayerIsReady = false;
      Utils.WeaponIsReady = false;
      Utils.IsInHideout = false;
      return false;
    }
    Utils.PlayerIsReady = true;
    return true;
  }

  public static void AddAttribute(
    Item item,
    Attributes.ENewItemAttributeId att,
    float baseValue,
    string displayValue,
    bool? lessIsGood = null,
    string name = null,
    bool colored = true)
  {
    string str = name == null ? att.GetName() : name;
    ItemAttributeClass itemAttribute = new ItemAttributeClass((Enum) att);
    itemAttribute.Name = str;
    itemAttribute.Base = (Func<float>) (() => baseValue);
    itemAttribute.StringValue = (Func<string>) (() => displayValue);
    if (lessIsGood.HasValue)
      itemAttribute.LessIsGood = lessIsGood.Value;
    itemAttribute.DisplayType = (Func<EItemAttributeDisplayType>) (() => (EItemAttributeDisplayType) 1);
    itemAttribute.LabelVariations = colored ? (EItemAttributeLabelVariations) 1 : (EItemAttributeLabelVariations) 0;
    Utils.SafelyAddAttributeToList(itemAttribute, item);
  }

  public static void SafelyAddAttributeToList(ItemAttributeClass itemAttribute, Item item)
  {
    if ((double) itemAttribute.Base() == 0.0)
      return;
    item.Attributes.Add(itemAttribute);
  }

  public static float GetSingleItemTotalWeight(Item item)
  {
    return !(item is GClass3050 gclass3050) ? item.TotalWeight : ((Item) gclass3050).TotalWeight;
  }

  public static float CalcultateModifierFromRange(
    float value,
    float minValue,
    float maxValue,
    float minModifier,
    float maxModifier)
  {
    float num1 = (float) (((double) maxModifier - (double) minModifier) / ((double) minValue - (double) maxValue));
    float num2 = minModifier - num1 * maxValue;
    return Mathf.Clamp(num1 * value + num2, minModifier, maxModifier);
  }

  public static T GetItemTemplate<T>(MongoID id) where T : ItemTemplate
  {
    return ((Dictionary<MongoID, ItemTemplate>) Singleton<ItemFactoryClass>.Instance.ItemTemplates).ContainsKey(id) ? ((Dictionary<MongoID, ItemTemplate>) Singleton<ItemFactoryClass>.Instance.ItemTemplates)[id] as T : default (T);
  }

  public static bool IsZombie(Player player)
  {
    return Object.op_Inequality((Object) player, (Object) null) && player.UsedSimplifiedSkeleton;
  }

  public static string GenId() => MongoID.op_Implicit(MongoID.Generate(true));

  public static bool IsSight(Mod mod)
  {
    return mod.GetType() == TemplateIdToObjectMappingsClass.TypeTable[Utils.Scope] || mod.GetType() == TemplateIdToObjectMappingsClass.TypeTable[Utils.AssaultScope] || mod.GetType() == TemplateIdToObjectMappingsClass.TypeTable[Utils.SpecialScope] || mod.GetType() == TemplateIdToObjectMappingsClass.TypeTable[Utils.CompactCollimator] || mod.GetType() == TemplateIdToObjectMappingsClass.TypeTable[Utils.Collimator] || mod.GetType() == TemplateIdToObjectMappingsClass.TypeTable[Utils.IronSight];
  }

  public static bool IsMuzzleDevice(Mod mod)
  {
    return mod.GetType() == TemplateIdToObjectMappingsClass.TypeTable[Utils.MuzzleCombo] || mod.GetType() == TemplateIdToObjectMappingsClass.TypeTable[Utils.Silencer] || mod.GetType() == TemplateIdToObjectMappingsClass.TypeTable[Utils.FlashHider];
  }

  public static bool IsStock(Mod mod)
  {
    return mod.GetType() == TemplateIdToObjectMappingsClass.TypeTable[Utils.Stock];
  }

  public static bool IsSilencer(Mod mod)
  {
    return mod.GetType() == TemplateIdToObjectMappingsClass.TypeTable[Utils.Silencer];
  }

  public static bool IsMagazine(Mod mod) => mod is MagazineItemClass;

  public static bool IsFlashHider(Mod mod)
  {
    return mod.GetType() == TemplateIdToObjectMappingsClass.TypeTable[Utils.FlashHider];
  }

  public static bool IsMuzzleCombo(Mod mod)
  {
    return mod.GetType() == TemplateIdToObjectMappingsClass.TypeTable[Utils.MuzzleCombo];
  }

  public static bool IsBarrel(Mod mod)
  {
    return mod.GetType() == TemplateIdToObjectMappingsClass.TypeTable[Utils.Barrel];
  }

  public static bool IsMount(Mod mod)
  {
    return mod.GetType() == TemplateIdToObjectMappingsClass.TypeTable[Utils.Mount];
  }

  public static bool IsReceiver(Mod mod)
  {
    return mod.GetType() == TemplateIdToObjectMappingsClass.TypeTable[Utils.Receiver];
  }

  public static bool IsCharge(Mod mod)
  {
    return mod.GetType() == TemplateIdToObjectMappingsClass.TypeTable[Utils.Charge];
  }

  public static bool IsCompactCollimator(Mod mod)
  {
    return mod.GetType() == TemplateIdToObjectMappingsClass.TypeTable[Utils.CompactCollimator];
  }

  public static bool IsCollimator(Mod mod)
  {
    return mod.GetType() == TemplateIdToObjectMappingsClass.TypeTable[Utils.Collimator];
  }

  public static bool IsAssaultScope(Mod mod)
  {
    return mod.GetType() == TemplateIdToObjectMappingsClass.TypeTable[Utils.AssaultScope];
  }

  public static bool IsScope(Mod mod)
  {
    return mod.GetType() == TemplateIdToObjectMappingsClass.TypeTable[Utils.Scope];
  }

  public static bool IsIronSight(Mod mod)
  {
    return mod.GetType() == TemplateIdToObjectMappingsClass.TypeTable[Utils.IronSight];
  }

  public static bool IsSpecialScope(Mod mod)
  {
    return mod.GetType() == TemplateIdToObjectMappingsClass.TypeTable[Utils.SpecialScope];
  }

  public static bool IsAuxiliaryMod(Mod mod)
  {
    return mod.GetType() == TemplateIdToObjectMappingsClass.TypeTable[Utils.AuxiliaryMod];
  }

  public static bool IsForegrip(Mod mod)
  {
    return mod.GetType() == TemplateIdToObjectMappingsClass.TypeTable[Utils.Foregrip];
  }

  public static bool IsPistolGrip(Mod mod)
  {
    return mod.GetType() == TemplateIdToObjectMappingsClass.TypeTable[Utils.PistolGrip];
  }

  public static bool IsGasblock(Mod mod)
  {
    return mod.GetType() == TemplateIdToObjectMappingsClass.TypeTable[Utils.Gasblock];
  }

  public static bool IsHandguard(Mod mod)
  {
    return mod.GetType() == TemplateIdToObjectMappingsClass.TypeTable[Utils.Handguard];
  }

  public static bool IsBipod(Mod mod)
  {
    return mod.GetType() == TemplateIdToObjectMappingsClass.TypeTable[Utils.Bipod];
  }

  public static bool IsFlashlight(Mod mod)
  {
    return mod.GetType() == TemplateIdToObjectMappingsClass.TypeTable[Utils.Flashlight];
  }

  public static bool IsTacticalCombo(Mod mod)
  {
    return mod.GetType() == TemplateIdToObjectMappingsClass.TypeTable[Utils.TacticalCombo];
  }

  public static bool IsUBGL(Mod mod)
  {
    return mod.GetType() == TemplateIdToObjectMappingsClass.TypeTable[Utils.UBGL];
  }
}
