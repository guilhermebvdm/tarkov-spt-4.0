// Decompiled with JetBrains decompiler
// Type: RealismMod.ApplyItemStashPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class ApplyItemStashPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (HealthControllerClass).GetMethod("ApplyItem", new Type[3]
    {
      typeof (Item),
      typeof (GStruct353<EBodyPart>),
      typeof (float)
    });
  }

  private static void CallMedEffect(
    HealthControllerClass hc,
    Item item,
    EBodyPart? eBodyPart,
    float amountUsed)
  {
    Type nestedType = typeof (HealthControllerClass).GetNestedType("MedEffect", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);
    if (nestedType == (Type) null)
      throw new Exception("Could not find MedEffect type");
    MethodInfo method = typeof (HealthControllerClass.GClass2820<>).MakeGenericType(typeof (GStruct364)).GetMethod("Create", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
    if (method == (MethodInfo) null)
      throw new Exception("Could not find Create<T>()");
    method.MakeGenericMethod(nestedType).Invoke((object) null, new object[5]
    {
      (object) hc,
      (object) eBodyPart.Value,
      (object) new Profile.ProfileHealthClass.GClass1974(),
      (object) ((GClass2814<HealthControllerClass.GClass2819>) hc).UpdateTime,
      (object) new GStruct364()
      {
        ItemId = item.Id,
        Amount = amountUsed
      }
    });
  }

  private static void HandleBuffs(HealthControllerClass hc, FoodDrinkItemClass foodClass)
  {
    foreach (GClass2823.GClass2848.GClass2849 buffSetting in ((StimulatorBuffsComponent) foodClass.HealthEffectsComponent).BuffSettings)
    {
      if (PluginConfig.EnableMedicalLogging.Value)
        ModulePatch.Logger.LogWarning((object) $"buff {buffSetting.BuffName}{buffSetting.BuffType.ToString()}");
      if (buffSetting.BuffType == 1)
      {
        if (PluginConfig.EnableMedicalLogging.Value)
          ModulePatch.Logger.LogWarning((object) ("has energy buff " + buffSetting.Value.ToString()));
        if ((double) buffSetting.Value > 0.0)
          hc.ChangeEnergy(buffSetting.Value * buffSetting.Duration);
      }
      if (buffSetting.BuffType == 2)
      {
        if (PluginConfig.EnableMedicalLogging.Value)
          ModulePatch.Logger.LogWarning((object) ("has hydration buff " + buffSetting.Value.ToString()));
        if ((double) buffSetting.Value > 0.0)
          hc.ChangeHydration(buffSetting.Value * buffSetting.Duration);
      }
    }
  }

  private static bool TryDoPoisoning(HealthControllerClass hc, FoodDrinkItemClass foodClass)
  {
    IEnumerable<GClass2823.GClass2848.GClass2849> source = ((IEnumerable<GClass2823.GClass2848.GClass2849>) ((StimulatorBuffsComponent) foodClass.HealthEffectsComponent).BuffSettings).Where<GClass2823.GClass2848.GClass2849>((Func<GClass2823.GClass2848.GClass2849, bool>) (b => b.BuffType == 17));
    if (source.Count<GClass2823.GClass2848.GClass2849>() > 0)
    {
      GClass2823.GClass2848.GClass2849 gclass2849 = source.First<GClass2823.GClass2848.GClass2849>();
      if (PluginConfig.EnableMedicalLogging.Value)
        ModulePatch.Logger.LogWarning((object) ("has toxin debuff " + gclass2849.Chance.ToString()));
      if ((double) gclass2849.Chance > 0.0 && (double) Random.Range(0, 100) < (double) gclass2849.Chance * 100.0)
      {
        if (PluginConfig.EnableMedicalLogging.Value)
          ModulePatch.Logger.LogWarning((object) "applying toxin debuff");
        float num1 = Mathf.Clamp(Random.Range(gclass2849.Chance * 250f, gclass2849.Chance * 500f), 2.5f, 90f);
        float num2 = Mathf.Clamp(Random.Range(gclass2849.Chance * 250f, gclass2849.Chance * 500f), 2.5f, 90f);
        hc.ChangeEnergy(-num1);
        hc.ChangeHydration(-num2);
        Singleton<GUISounds>.Instance.PlaySound(GClass835.RandomElement<KeyValuePair<string, AudioClip>>((IEnumerable<KeyValuePair<string, AudioClip>>) Plugin.RealismAudioController.FoodPoisoningSfx).Value, false, false, 0.5f);
        return true;
      }
    }
    return false;
  }

  private static void DoFoodItem(HealthControllerClass hc, FoodDrinkItemClass foodClass)
  {
    if (ApplyItemStashPatch.TryDoPoisoning(hc, foodClass))
      return;
    ApplyItemStashPatch.HandleBuffs(hc, foodClass);
    Plugin.RealHealthController.TryReduceToxinInStashFood((Item) foodClass, false, hc);
  }

  [SPT.Reflection.Patching.PatchPrefix]
  private static bool PatchPrefix(
    HealthControllerClass __instance,
    Item item,
    GStruct353<EBodyPart> bodyParts,
    float? amount,
    ref bool __result)
  {
    if (GameWorldController.IsInRaid() || !Plugin.ServerConfig.med_changes)
      return true;
    Plugin.BSGHealthController = __instance;
    TemplateStats.GetDataObj<Consumable>(TemplateStats.ConsumableStats, MongoID.op_Implicit(item.TemplateId));
    if (bodyParts.Length == 0)
    {
      __result = false;
      return false;
    }
    EBodyPart? eBodyPart = new EBodyPart?(bodyParts[0]);
    eBodyPart = eBodyPart.GetValueOrDefault() == 7 ? new EBodyPart?((EBodyPart) 0) : eBodyPart;
    FoodDrinkItemClass foodDrinkItemClass = item as FoodDrinkItemClass;
    float amountUsed = 1f;
    if (foodDrinkItemClass != null)
      amountUsed = (float) ((double) amount ?? (double) foodDrinkItemClass.FoodDrinkComponent.HpPercent / (double) foodDrinkItemClass.FoodDrinkComponent.MaxResource);
    ApplyItemStashPatch.CallMedEffect(__instance, item, eBodyPart, amountUsed);
    if (((GClass2814<HealthControllerClass.GClass2819>) __instance).inventoryController_0.Profile is Profile profile)
      profile.Health = ((GClass2814<HealthControllerClass.GClass2819>) __instance).Store((Profile.ProfileHealthClass) null);
    __result = true;
    return false;
  }

  [PatchPostfix]
  private static void Postfix(
    HealthControllerClass __instance,
    Item item,
    GStruct353<EBodyPart> bodyParts,
    float? amount)
  {
    if (!Plugin.ServerConfig.food_changes || !(item is FoodDrinkItemClass foodClass))
      return;
    if (PluginConfig.EnableMedicalLogging.Value)
      ModulePatch.Logger.LogWarning((object) "is food");
    ApplyItemStashPatch.DoFoodItem(__instance, foodClass);
    Singleton<GUISounds>.Instance.PlayItemSound(item.ItemSound, (EInventorySoundType) 5, false);
  }
}
