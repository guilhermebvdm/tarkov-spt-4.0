// Decompiled with JetBrains decompiler
// Type: RealismMod.MedEffectStartedPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class MedEffectStartedPatch : ModulePatch
{
  private static readonly Type MedEffectType = typeof (HealthControllerClass).GetNestedType("MedEffect", BindingFlags.NonPublic);
  private static readonly PropertyInfo HealthControllerProperty = AccessTools.Property(MedEffectStartedPatch.MedEffectType, "HealthControllerClass");
  private static readonly PropertyInfo BodyPartProperty = AccessTools.Property(MedEffectStartedPatch.MedEffectType, "BodyPart");
  private static readonly PropertyInfo MedItemProperty = AccessTools.Property(MedEffectStartedPatch.MedEffectType, "MedItem");
  private static readonly FieldInfo MedKitComponentField = AccessTools.Field(MedEffectStartedPatch.MedEffectType, "medKitComponent_0");

  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) MedEffectStartedPatch.MedEffectType.GetMethod("Started", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
  }

  private static void ApplyStim(HealthControllerClass healthController, Item medItem)
  {
    if (((GClass2814<HealthControllerClass.GClass2819>) healthController).inventoryController_0 is GControl6 inventoryController0)
      inventoryController0.Heal(medItem, (EBodyPart) 0, Mathf.RoundToInt(1f));
    GClass2821.RemoveItem(medItem);
    AccessTools.Method(medItem.GetType(), "RaiseRefreshEvent")?.Invoke((object) medItem, new object[2]
    {
      (object) false,
      (object) true
    });
    Singleton<GUISounds>.Instance.PlayItemSound(medItem.ItemSound, (EInventorySoundType) 5, false);
  }

  private static void TryApplyDebuffs(
    HealthControllerClass healthController,
    GClass2823.GClass2848.GClass2849[] buffs,
    bool getsAdditionalDebuff)
  {
    float num = getsAdditionalDebuff ? 10f : 1f;
    List<GClass2823.GClass2848.GClass2849> list = ((IEnumerable<GClass2823.GClass2848.GClass2849>) buffs).ToList<GClass2823.GClass2848.GClass2849>();
    GClass2823.GClass2848.GClass2849 gclass2849_1 = list.FirstOrDefault<GClass2823.GClass2848.GClass2849>((Func<GClass2823.GClass2848.GClass2849, bool>) (b => b.BuffType == 2));
    GClass2823.GClass2848.GClass2849 gclass2849_2 = list.FirstOrDefault<GClass2823.GClass2848.GClass2849>((Func<GClass2823.GClass2848.GClass2849, bool>) (b => b.BuffType == 1));
    if (gclass2849_1 != null)
      healthController.ChangeHydration(gclass2849_1.Value * gclass2849_1.Duration * num);
    if (gclass2849_2 == null)
      return;
    healthController.ChangeEnergy(gclass2849_2.Value * gclass2849_2.Duration * num);
  }

  private static bool HandleBuffs(
    HealthControllerClass healthController,
    Item medItem,
    MedsItemClass medClass,
    GClass2823.GClass2848.GClass2849[] buffs,
    bool getsAdditionalDebuff)
  {
    bool flag = false;
    foreach (GClass2823.GClass2848.GClass2849 buff in buffs)
    {
      if (buff.BuffType == 0)
        flag = (double) MedEffectStartedPatch.RestoreHP(healthController, (EBodyPart) 7, buff.Value * buff.Duration, true) > 0.0;
    }
    return flag;
  }

  private static bool HandleStim(
    HealthControllerClass healthController,
    MedsItemClass medClass,
    Item medItem)
  {
    bool flag1 = Plugin.RealHealthController.TryHazardTreatmentOutOfRaid(medClass);
    GClass2823.GClass2848.GClass2849[] buffSettings = ((StimulatorBuffsComponent) medClass.HealthEffectsComponent).BuffSettings;
    bool flag2 = MedEffectStartedPatch.HandleBuffs(healthController, medItem, medClass, buffSettings, false);
    if (!(flag1 | flag2))
      return true;
    MedEffectStartedPatch.TryApplyDebuffs(healthController, buffSettings, false);
    MedEffectStartedPatch.ApplyStim(healthController, medItem);
    ModulePatch.Logger.LogWarning((object) $"treatedHazard {flag1}, appliedBuffs{flag2}");
    return false;
  }

  private static bool HandleDrug(
    HealthControllerClass healthController,
    MedsItemClass medClass,
    Item medItem,
    MedKitComponent medKitComponent,
    bool getsAdditionalDebuff)
  {
    bool flag1 = Plugin.RealHealthController.TryHazardTreatmentOutOfRaid(medClass);
    GClass2823.GClass2848.GClass2849[] buffSettings = ((StimulatorBuffsComponent) medClass.HealthEffectsComponent).BuffSettings;
    bool flag2 = MedEffectStartedPatch.HandleBuffs(healthController, medItem, medClass, buffSettings, getsAdditionalDebuff);
    if (!(flag1 | flag2))
      return true;
    MedEffectStartedPatch.TryApplyDebuffs(healthController, buffSettings, getsAdditionalDebuff);
    return MedEffectStartedPatch.ApplyMedItem(healthController, medKitComponent, medItem, (EBodyPart) 0, 1f);
  }

  private static float HealBodyPart(
    HealthControllerClass hc,
    EBodyPart bodyPart,
    ref float hpToRestore)
  {
    float val1 = ((GClass2814<HealthControllerClass.GClass2819>) hc).GetBodyPartHealth(bodyPart, false).Maximum - ((GClass2814<HealthControllerClass.GClass2819>) hc).GetBodyPartHealth(bodyPart, false).Current;
    if ((double) val1 <= 0.0)
      return 0.0f;
    float num = Math.Min(val1, hpToRestore);
    hc.ChangeHealth(bodyPart, num, GClass2855.MedKitUse);
    hpToRestore -= num;
    return num;
  }

  private static float RestoreHP(
    HealthControllerClass hc,
    EBodyPart initialTarget,
    float hpToRestore,
    bool restoreAll)
  {
    float num = 0.0f;
    if (initialTarget != 7 && !restoreAll)
      return MedEffectStartedPatch.HealBodyPart(hc, initialTarget, ref hpToRestore);
    foreach (EBodyPart possibleBodyPart in Plugin.RealHealthController.PossibleBodyParts)
    {
      if ((double) hpToRestore > 0.0)
        num += MedEffectStartedPatch.HealBodyPart(hc, possibleBodyPart, ref hpToRestore);
      else
        break;
    }
    return num;
  }

  private static bool ApplyMedItem(
    HealthControllerClass healthController,
    MedKitComponent medKitComponent,
    Item medItem,
    EBodyPart bodyPart,
    float cost)
  {
    bodyPart = bodyPart == 7 ? (EBodyPart) (object) 0 : bodyPart;
    FieldInfo fieldInfo = AccessTools.Field(medKitComponent.GetType(), "HpResource");
    float num1 = (float) fieldInfo.GetValue((object) medKitComponent);
    if (((GClass2814<HealthControllerClass.GClass2819>) healthController).inventoryController_0 is GControl6 inventoryController0)
      inventoryController0.Heal(medItem, bodyPart, Mathf.RoundToInt(cost));
    float num2 = num1 - cost;
    fieldInfo.SetValue((object) medKitComponent, (object) num2);
    if ((double) num2 <= 0.0)
    {
      GClass2821.RemoveItem(medItem);
      Singleton<GUISounds>.Instance.PlayItemSound(medItem.ItemSound, (EInventorySoundType) 5, false);
      return false;
    }
    AccessTools.Method(medItem.GetType(), "RaiseRefreshEvent")?.Invoke((object) medItem, new object[2]
    {
      (object) false,
      (object) true
    });
    Singleton<GUISounds>.Instance.PlayItemSound(medItem.ItemSound, (EInventorySoundType) 5, false);
    return true;
  }

  private static bool HandleHealing(
    HealthControllerClass healthController,
    Consumable itemStats,
    Item medItem,
    EBodyPart bodyPart,
    MedKitComponent medKitComponent)
  {
    if ((double) itemStats.HPRestoreAmount <= 0.0)
      return false;
    bool restoreAll = (double) ((GClass2814<HealthControllerClass.GClass2819>) healthController).GetBodyPartHealth(bodyPart, false).Maximum - (double) ((GClass2814<HealthControllerClass.GClass2819>) healthController).GetBodyPartHealth(bodyPart, false).Current <= 0.0;
    return (double) MedEffectStartedPatch.RestoreHP(healthController, bodyPart, itemStats.HPRestoreAmount, restoreAll) <= 0.0 || MedEffectStartedPatch.ApplyMedItem(healthController, medKitComponent, medItem, bodyPart, 1f);
  }

  [PatchPrefix]
  private static bool Prefix(object __instance)
  {
    if (GameWorldController.IsInRaid())
      return true;
    HealthControllerClass healthController = (HealthControllerClass) MedEffectStartedPatch.HealthControllerProperty.GetValue(__instance);
    EBodyPart bodyPart = (EBodyPart) MedEffectStartedPatch.BodyPartProperty.GetValue(__instance);
    Item medItem = (Item) MedEffectStartedPatch.MedItemProperty.GetValue(__instance);
    MedKitComponent medKitComponent = (MedKitComponent) MedEffectStartedPatch.MedKitComponentField.GetValue(__instance);
    MedsItemClass medClass = medItem as MedsItemClass;
    Consumable dataObj = TemplateStats.GetDataObj<Consumable>(TemplateStats.ConsumableStats, MongoID.op_Implicit(medItem.TemplateId));
    if (!Plugin.RealHealthController.ShouldAlwaysAllowOutOfRaid(medItem, dataObj))
      return true;
    if (medClass != null && medItem is StimulatorItemClass)
    {
      MedEffectStartedPatch.HandleStim(healthController, medClass, medItem);
      return false;
    }
    if (dataObj.ConsumableType == EConsumableType.Medkit || dataObj.ConsumableType == EConsumableType.Surgical)
      return MedEffectStartedPatch.HandleHealing(healthController, dataObj, medItem, bodyPart, medKitComponent);
    bool flag = dataObj.ConsumableType == EConsumableType.Drug || dataObj.ConsumableType == EConsumableType.PainPills || dataObj.ConsumableType == EConsumableType.Pills || dataObj.ConsumableType == EConsumableType.PainDrug;
    return !(medClass != null & flag) || MedEffectStartedPatch.HandleDrug(healthController, medClass, medItem, medKitComponent, dataObj.DoesExtraResourceDebuff);
  }
}
