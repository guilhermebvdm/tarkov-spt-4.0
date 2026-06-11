// Decompiled with JetBrains decompiler
// Type: RealismMod.GearController
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Comfort.Common;
using EFT;
using EFT.Communications;
using EFT.InventoryLogic;
using EFT.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
namespace RealismMod;

public static class GearController
{
  public const string SAFE_CONTAINER_ID = "66fd588d397ed74159826cf0";
  public static readonly string[] GasAnalysers;
  public static readonly string[] RadDevices;
  public static EquipmentSlot[] MainInventorySlots;
  private static float _currentGasProtection;
  private static float _currentRadProtection;
  private static float _gasMaskDurabilityFactor;
  private static bool _hadGasMask;
  private static ItemAddress _gasMaskStartingAddress;

  public static bool HasSafeContainer { get; set; }

  public static bool HasGasMask { get; private set; }

  public static bool HasGasFilter { get; private set; }

  public static bool HasRespirator { get; private set; }

  public static bool FSIsActive { get; set; }

  public static bool GearBlocksMouth { get; set; }

  public static bool NVGIsActive { get; set; }

  public static bool HasGasAnalyser { get; set; }

  public static bool HasGeiger { get; set; }

  public static bool GearAllowsADS { get; set; }

  public static float GearReloadMulti { get; set; }

  public static bool HasGasMaskWithFilter
  {
    get
    {
      return GearController.HasGasMask && (GearController.HasGasFilter || GearController.HasRespirator);
    }
  }

  public static float CurrentGasProtection
  {
    get
    {
      float f = GearController._currentGasProtection * GearController._gasMaskDurabilityFactor;
      return float.IsNaN(f) ? 0.0f : f;
    }
  }

  public static float CurrentRadProtection
  {
    get
    {
      float f = GearController._currentRadProtection * GearController._gasMaskDurabilityFactor;
      return float.IsNaN(f) ? 0.0f : f;
    }
  }

  public static float GasMaskDurabilityFactor
  {
    get
    {
      return float.IsNaN(GearController._gasMaskDurabilityFactor) ? 0.0f : GearController._gasMaskDurabilityFactor;
    }
  }

  private static Item FindGasMask(Inventory invClass)
  {
    // ISSUE: unable to decompile the method.
  }

  public static IEnumerable<StashGridClass> GetGrids(InventoryEquipment equipment)
  {
    Slot slot1 = equipment.GetSlot((EquipmentSlot) 6);
    Slot slot2 = equipment.GetSlot((EquipmentSlot) 8);
    Slot slot3 = equipment.GetSlot((EquipmentSlot) 14);
    VestItemClass containedItem1 = slot1.ContainedItem as VestItemClass;
    PocketsItemClass containedItem2 = slot2.ContainedItem as PocketsItemClass;
    VestItemClass containedItem3 = slot3.ContainedItem as VestItemClass;
    StashGridClass[] second1 = containedItem1 != null ? ((CompoundItem) containedItem1).Grids : new StashGridClass[0];
    StashGridClass[] first = containedItem2 != null ? ((CompoundItem) containedItem2).Grids : new StashGridClass[0];
    StashGridClass[] second2 = containedItem3 != null ? ((CompoundItem) containedItem3).Grids : new StashGridClass[0];
    return ((IEnumerable<StashGridClass>) first).Concat<StashGridClass>((IEnumerable<StashGridClass>) second1).Concat<StashGridClass>((IEnumerable<StashGridClass>) second2);
  }

  public static IEnumerable<Slot> GetSlots(InventoryEquipment equipment)
  {
    return equipment.GetSlot((EquipmentSlot) 8).ContainedItem is PocketsItemClass containedItem ? (IEnumerable<Slot>) ((CompoundItem) containedItem).Slots : (IEnumerable<Slot>) new Slot[0];
  }

  private static ItemAddress TryFindAddressForMask(Player player, Item item)
  {
    return (ItemAddress) ((object) GearController.GetSlots(player.InventoryController.Inventory.Equipment).Select<Slot, ItemAddress>((Func<Slot, ItemAddress>) (slot =>
    {
      ItemAddress addressForMask;
      slot.TryFindLocationForItem(item, ref addressForMask);
      return addressForMask;
    })).FirstOrDefault<ItemAddress>((Func<ItemAddress, bool>) (address => address != null)) ?? (object) GearController.GetGrids(player.InventoryController.Inventory.Equipment).Select<StashGridClass, GClass3186>((Func<StashGridClass, GClass3186>) (grid => grid.FindLocationForItem(item))).FirstOrDefault<GClass3186>((Func<GClass3186, bool>) (address => address != null)));
  }

  private static void EquipGasMask(Item item, Player player)
  {
    ItemAddress slotToPickUp = GClass3169.FindSlotToPickUp(player.InventoryController, item);
    if (slotToPickUp == null)
      return;
    GStruct455<GClass3203> gstruct455 = InteractionsHandlerClass.Move(item, slotToPickUp, (TraderControllerClass) player.InventoryController, true);
    if (gstruct455.Succeeded)
    {
      ItemUiContext.smethod_0((TraderControllerClass) player.InventoryController, item, GStruct455<GClass3203>.op_Implicit(gstruct455), (Callback) null);
      if (string.IsNullOrWhiteSpace(TemplateStats.GetDataObj<Gear>(TemplateStats.GearStats, MongoID.op_Implicit(item.TemplateId)).MaskToUse))
        return;
      GearController.DoInteractionAnimation(player, (EInteraction) 13);
    }
    else
      NotificationManagerClass.DisplayWarningNotification("Face Cover/Glasses Slot Not Empty", (ENotificationDurationType) 0);
  }

  private static bool TryRemoveGasMask(Item item, Player player, ItemAddress address)
  {
    GStruct455<GClass3203> gstruct455 = InteractionsHandlerClass.Move(item, address, (TraderControllerClass) player.InventoryController, true);
    if (!gstruct455.Succeeded)
      return false;
    ItemUiContext.smethod_0((TraderControllerClass) player.InventoryController, item, GStruct455<GClass3203>.op_Implicit(gstruct455), (Callback) null);
    if (!string.IsNullOrWhiteSpace(TemplateStats.GetDataObj<Gear>(TemplateStats.GearStats, MongoID.op_Implicit(item.TemplateId)).MaskToUse))
      GearController.DoInteractionAnimation(player, (EInteraction) 14);
    return true;
  }

  private static void RemoveGasMask(Item item, Player player)
  {
    ItemAddress maskStartingAddress = GearController._gasMaskStartingAddress;
    bool flag = true;
    if (maskStartingAddress == null || !GearController.TryRemoveGasMask(item, player, maskStartingAddress))
    {
      ItemAddress addressForMask = GearController.TryFindAddressForMask(player, item);
      if (addressForMask == null)
        return;
      flag = GearController.TryRemoveGasMask(item, player, addressForMask);
    }
    if (flag)
      return;
    NotificationManagerClass.DisplayWarningNotification("Can't Find Space For Gas Mask", (ENotificationDurationType) 0);
  }

  public static void DoInteractionAnimation(Player player, EInteraction interaction)
  {
    player.MovementContext.PlayerAnimator.AnimatedInteractions.SetInteraction(interaction);
    player.OnAnimatedInteraction(interaction);
  }

  public static void ToggleGasMask(Player player)
  {
    bool flag = ((TraderControllerClass) player.InventoryController).IsChangingWeapon || Object.op_Inequality((Object) player.MovementContext.StationaryWeapon, (Object) null) || player.MovementContext.IsAnimatorInteractionOn;
    if (PlayerState.IsSprinting | flag || PlayerState.IsInReloadOpertation)
      return;
    Slot slot = player?.Inventory?.Equipment?.GetSlot((EquipmentSlot) 10);
    if (slot == null)
      return;
    Item containedItem = slot?.ContainedItem;
    if (containedItem == null)
    {
      Item gasMask = GearController.FindGasMask(player.Inventory);
      if (gasMask != null)
      {
        GearController._gasMaskStartingAddress = gasMask.CurrentAddress;
        GearController.EquipGasMask(gasMask, player);
      }
    }
    else
      GearController.RemoveGasMask(containedItem, player);
  }

  private static void HandleGasMaskEffects(Player player, float gasProtection, float radProtection)
  {
    if (GearController.HasGasMask)
    {
      GearController._currentGasProtection = gasProtection;
      GearController._currentRadProtection = radProtection;
      GearController._hadGasMask = true;
      player.SpeechSource.SetLowPassFilterParameters(0.99f, (ESoundOcclusionType) 1, 1600f, 5000f, 1f, true);
      player.Muffled = true;
    }
    else
    {
      GearController._currentGasProtection = 0.0f;
      GearController._currentRadProtection = 0.0f;
      player.SpeechSource.ResetFilters();
    }
    player.UpdateBreathStatus();
    player.UpdateMuffledState();
    player.SendVoiceMuffledState(player.Muffled);
    if (GearController.HasGasMask || !GearController._hadGasMask || player.HealthStatus != 8192 /*0x2000*/)
      return;
    player.Say((EPhraseTrigger) 29, true, 0.0f, (ETagStatus) 0, 100, false);
    GearController._hadGasMask = false;
  }

  private static void DeviceCheckerHelper(IEnumerable<Item> items)
  {
    foreach (Item obj1 in items)
    {
      Item item = obj1;
      int num;
      if (item != null)
      {
        Item obj2 = item;
        if (obj2 == null)
        {
          num = 1;
        }
        else
        {
          MongoID templateId = obj2.TemplateId;
          num = 0;
        }
      }
      else
        num = 1;
      if (num == 0)
      {
        if (Array.Exists<string>(GearController.GasAnalysers, (Predicate<string>) (i => MongoID.op_Equality(MongoID.op_Implicit(i), item.TemplateId))))
          GearController.HasGasAnalyser = true;
        if (Array.Exists<string>(GearController.RadDevices, (Predicate<string>) (i => MongoID.op_Equality(MongoID.op_Implicit(i), item.TemplateId))))
          GearController.HasGeiger = true;
      }
    }
  }

  public static void CheckForDevices(Inventory invClass)
  {
    // ISSUE: unable to decompile the method.
  }

  public static float GetModifiedInventoryWeight(Inventory invClass)
  {
    float modifiedInventoryWeight = 0.0f;
    float num = 0.0f;
    foreach (int allSlotName in InventoryEquipment.AllSlotNames)
    {
      EquipmentSlot equipmentSlot = (EquipmentSlot) allSlotName;
      foreach (Item obj in invClass.Equipment.GetSlot(equipmentSlot).Items)
      {
        float singleItemTotalWeight = Utils.GetSingleItemTotalWeight(obj);
        num += singleItemTotalWeight;
        if (equipmentSlot == 4 || equipmentSlot == 6 || equipmentSlot == 7 || equipmentSlot == 11 || equipmentSlot == 14)
        {
          Gear dataObj = TemplateStats.GetDataObj<Gear>(TemplateStats.GearStats, MongoID.op_Implicit(obj.TemplateId));
          modifiedInventoryWeight += singleItemTotalWeight * dataObj.Comfort;
        }
        else
          modifiedInventoryWeight += singleItemTotalWeight;
      }
    }
    if (PluginConfig.EnableGeneralLogging.Value)
    {
      Utils.Logger.LogWarning((object) "==================");
      Utils.Logger.LogWarning((object) ("Total Modified Weight " + modifiedInventoryWeight.ToString()));
      Utils.Logger.LogWarning((object) ("Total Unmodified Weight " + num.ToString()));
      Utils.Logger.LogWarning((object) "==================");
    }
    return modifiedInventoryWeight;
  }

  private static EquipmentPenaltyComponent GetRigGearPenalty(Player player)
  {
    return GearController.GetSlotItem(player, (EquipmentSlot) 6)?.GetItemComponent<EquipmentPenaltyComponent>();
  }

  public static Item GetSlotItem(Player player, EquipmentSlot slot)
  {
    return player?.Inventory?.Equipment?.GetSlot(slot)?.ContainedItem;
  }

  public static void UpdateFilterResource(Player player, PlayerZoneBridge phb)
  {
    GearController.HasGasFilter = false;
    Item slotItem = GearController.GetSlotItem(player, (EquipmentSlot) 10);
    if (slotItem == null)
      return;
    ResourceComponent resourceComponent = slotItem != null ? GClass3176.GetItemComponentsInChildren<ResourceComponent>(slotItem, false).FirstOrDefault<ResourceComponent>() : (ResourceComponent) null;
    if (resourceComponent == null)
      return;
    float num1 = (float) ((double) Plugin.RealHealthController.ToxicItemCount * 0.05000000074505806 + (double) Plugin.RealHealthController.RadItemCount * 0.15000000596046448);
    float num2 = (float) (((double) phb.TotalGasRate + (double) phb.TotalRadRate + (double) GameWorldController.CurrentGasEventStrength + (double) GameWorldController.CurrentMapRadStrength + (double) num1) / 3.0);
    resourceComponent.Value = Mathf.Clamp(resourceComponent.Value - num2, 0.0f, 100f);
    if ((double) resourceComponent.Value <= 0.0)
      return;
    GearController.HasGasFilter = true;
  }

  public static void CalcGasMaskDuraFactor(Player player)
  {
    GearController.HasRespirator = false;
    GearController.HasGasFilter = false;
    Item slotItem = GearController.GetSlotItem(player, (EquipmentSlot) 10);
    if (slotItem == null)
      return;
    ResourceComponent resourceComponent = slotItem != null ? GClass3176.GetItemComponentsInChildren<ResourceComponent>(slotItem, false).FirstOrDefault<ResourceComponent>() : (ResourceComponent) null;
    float num1 = 0.0f;
    if (resourceComponent != null)
    {
      if ((double) resourceComponent.Value > 0.0)
        GearController.HasGasFilter = true;
      float num2 = Mathf.Pow(resourceComponent.Value / resourceComponent.MaxResource, 0.15f);
      num1 = (double) num2 > 0.85000002384185791 ? 1f : num2;
    }
    ArmorComponent itemComponent = slotItem.GetItemComponent<ArmorComponent>();
    if (itemComponent == null)
    {
      GearController.HasRespirator = true;
      GearController._gasMaskDurabilityFactor = 1f;
    }
    else
    {
      float num3 = itemComponent.Repairable.Durability / itemComponent.Repairable.MaxDurability;
      GearController._gasMaskDurabilityFactor = (double) num3 <= 0.5 || resourceComponent == null ? 0.0f : num3 * num1;
    }
  }

  public static EquipmentPenaltyComponent CheckFaceCoverGear(
    Player player,
    ref float gasProtection,
    ref float radProtection)
  {
    Item slotItem = GearController.GetSlotItem(player, (EquipmentSlot) 10);
    if (slotItem == null)
      return (EquipmentPenaltyComponent) null;
    Gear dataObj = TemplateStats.GetDataObj<Gear>(TemplateStats.GearStats, MongoID.op_Implicit(slotItem.TemplateId));
    GearController.HasGasMask = dataObj.IsGasMask;
    gasProtection = dataObj.GasProtection;
    radProtection = dataObj.RadProtection;
    if (GearController.HasGasMask)
      GearController.CalcGasMaskDuraFactor(player);
    return slotItem.GetItemComponent<EquipmentPenaltyComponent>();
  }

  public static void CheckGear(Player player)
  {
    FaceShieldComponent component1 = player.FaceShieldObserver.Component;
    NightVisionComponent component2 = player.NightVisionObserver.Component;
    ThermalVisionComponent component3 = player.ThermalVisionObserver.Component;
    GearController.HasGasMask = false;
    bool flag1 = component1 != null && (component1.Togglable == null || component1.Togglable.On);
    bool flag2 = component2 != null && (component2.Togglable == null || component2.Togglable.On);
    bool flag3 = component3 != null && (component3.Togglable == null || component3.Togglable.On);
    float gasProtection = 0.0f;
    float radProtection = 0.0f;
    List<ArmorComponent> armorComponentList = new List<ArmorComponent>(20);
    player.Inventory.GetPutOnArmorsNonAlloc(armorComponentList);
    float num1 = 0.0f;
    float num2 = 0.0f;
    for (int index = 0; index < armorComponentList.Count; ++index)
    {
      ArmorComponent armorComponent = armorComponentList[index];
      MongoID? nullable1 = ((GClass3175) armorComponent).Item.Template.ParentId;
      MongoID? nullable2 = MongoID.op_Implicit("5448e5284bdc2dcb718b4567");
      int num3;
      if ((nullable1.HasValue == nullable2.HasValue ? (nullable1.HasValue ? (MongoID.op_Equality(nullable1.GetValueOrDefault(), nullable2.GetValueOrDefault()) ? 1 : 0) : 1) : 0) == 0)
      {
        nullable2 = ((GClass3175) armorComponent).Item.Template.ParentId;
        nullable1 = MongoID.op_Implicit("5a341c4686f77469e155819e");
        num3 = nullable2.HasValue == nullable1.HasValue ? (nullable2.HasValue ? (MongoID.op_Equality(nullable2.GetValueOrDefault(), nullable1.GetValueOrDefault()) ? 1 : 0) : 1) : 0;
      }
      else
        num3 = 1;
      if (num3 == 0)
      {
        if (player.FaceShieldObserver.Component != null && MongoID.op_Equality(((GClass3175) player.FaceShieldObserver.Component).Item.TemplateId, ((GClass3175) armorComponent).Item.TemplateId))
        {
          Gear dataObj = TemplateStats.GetDataObj<Gear>(TemplateStats.GearStats, MongoID.op_Implicit(((GClass3175) armorComponent).Item.TemplateId));
          if (flag1 && dataObj.BlocksMouth)
          {
            num1 += armorComponent.WeaponErgonomicPenalty;
            num2 += armorComponent.SpeedPenalty - 15f;
          }
        }
        else
        {
          num1 += armorComponent.WeaponErgonomicPenalty;
          num2 += armorComponent.SpeedPenalty;
        }
      }
    }
    EquipmentPenaltyComponent putOnBackpack = player.Inventory.GetPutOnBackpack();
    if (putOnBackpack != null)
    {
      num1 += putOnBackpack.Template.WeaponErgonomicPenalty;
      num2 += putOnBackpack.Template.SpeedPenaltyPercent;
    }
    EquipmentPenaltyComponent rigGearPenalty = GearController.GetRigGearPenalty(player);
    if (rigGearPenalty != null)
    {
      num1 += rigGearPenalty.Template.WeaponErgonomicPenalty;
      num2 += rigGearPenalty.Template.SpeedPenaltyPercent;
    }
    EquipmentPenaltyComponent penaltyComponent = GearController.CheckFaceCoverGear(player, ref gasProtection, ref radProtection);
    if (penaltyComponent != null)
    {
      num1 += penaltyComponent.Template.WeaponErgonomicPenalty;
      num2 += penaltyComponent.Template.SpeedPenaltyPercent;
    }
    if (flag2 | flag3)
    {
      num1 += -30f;
      num2 += -12.5f;
    }
    float num4 = num1 / 100f;
    float num5 = num2 / 100f;
    PlayerState.GearErgoPenalty = Mathf.Clamp(1f + num4, 0.1f, 2f);
    PlayerState.GearSpeedPenalty = Mathf.Clamp(1f + num5, 0.1f, 2f);
    GearController.HandleGasMaskEffects(player, gasProtection, radProtection);
    Player.FirearmController handsController = player.HandsController as Player.FirearmController;
    if (Object.op_Inequality((Object) handsController, (Object) null) && Plugin.ServerConfig.recoil_attachment_overhaul)
      StatCalc.UpdateAimParameters(handsController, player.ProceduralWeaponAnimation);
    if (!PluginConfig.EnableGeneralLogging.Value)
      return;
    Utils.Logger.LogWarning((object) ("gear speed " + PlayerState.GearSpeedPenalty.ToString()));
    Utils.Logger.LogWarning((object) ("gear ergo " + PlayerState.GearErgoPenalty.ToString()));
  }

  public static float GetGearReloadSpeed(Player player, EquipmentSlot[] slots)
  {
    float gearReloadSpeed = 1f;
    foreach (int slot in slots)
    {
      EquipmentSlot equipmentSlot = (EquipmentSlot) slot;
      Item containedItem = player.Equipment.GetSlot(equipmentSlot).ContainedItem;
      if (containedItem != null)
      {
        Gear dataObj = TemplateStats.GetDataObj<Gear>(TemplateStats.GearStats, MongoID.op_Implicit(containedItem.TemplateId));
        gearReloadSpeed *= dataObj.ReloadSpeedMulti;
      }
      else
        gearReloadSpeed *= 1f;
    }
    return gearReloadSpeed;
  }

  public static void GetFacecoverADS(Player player)
  {
    Item containedItem = player.Equipment.GetSlot((EquipmentSlot) 10).ContainedItem;
    GearController.GearAllowsADS = true;
    GearController.GearBlocksMouth = false;
    if (containedItem == null)
      return;
    GearController.GearAllowsADS = TemplateStats.GetDataObj<Gear>(TemplateStats.GearStats, MongoID.op_Implicit(containedItem.TemplateId)).AllowADS;
  }

  public static void SetGearParamaters(Player player)
  {
    float num1 = 1f;
    List<ArmorComponent> armorComponentList = new List<ArmorComponent>(20);
    player.Inventory.GetPutOnArmorsNonAlloc(armorComponentList);
    float num2 = (float) ((double) num1 * (double) GearController.GetGearReloadSpeed(player, new EquipmentSlot[2]
    {
      (EquipmentSlot) 14,
      (EquipmentSlot) 6
    }));
    GearController.GetFacecoverADS(player);
    foreach (ArmorComponent armorComponent in armorComponentList)
    {
      MongoID? nullable1 = ((GClass3175) armorComponent).Item.Template.ParentId;
      MongoID? nullable2 = MongoID.op_Implicit("5448e5284bdc2dcb718b4567");
      int num3;
      if ((nullable1.HasValue == nullable2.HasValue ? (nullable1.HasValue ? (MongoID.op_Equality(nullable1.GetValueOrDefault(), nullable2.GetValueOrDefault()) ? 1 : 0) : 1) : 0) == 0)
      {
        nullable2 = ((GClass3175) armorComponent).Item.Template.ParentId;
        nullable1 = MongoID.op_Implicit("5a341c4686f77469e155819e");
        num3 = nullable2.HasValue == nullable1.HasValue ? (nullable2.HasValue ? (MongoID.op_Equality(nullable2.GetValueOrDefault(), nullable1.GetValueOrDefault()) ? 1 : 0) : 1) : 0;
      }
      else
        num3 = 1;
      if (num3 == 0)
      {
        Gear dataObj = TemplateStats.GetDataObj<Gear>(TemplateStats.GearStats, MongoID.op_Implicit(((GClass3175) armorComponent).Item.TemplateId));
        num2 *= dataObj.ReloadSpeedMulti;
        ArmoredEquipmentTemplateClass template = armorComponent.Template as ArmoredEquipmentTemplateClass;
        if (!dataObj.AllowADS)
          GearController.GearAllowsADS = false;
        if (dataObj.BlocksMouth)
          GearController.GearBlocksMouth = true;
      }
    }
    GearController.GearReloadMulti = num2;
  }

  static GearController()
  {
    // ISSUE: unable to decompile the method.
  }
}
