// Decompiled with JetBrains decompiler
// Type: RealismMod.SetQuickSlotPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class SetQuickSlotPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (Player).GetMethod("SetQuickSlotItem", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Prefix(
    Player __instance,
    EBoundItem quickSlot,
    Callback<IHandsController> callback)
  {
    if (__instance.IsYourPlayer)
    {
      Item boundItem = __instance.InventoryController.Inventory.FastAccess.GetBoundItem(quickSlot);
      FoodDrinkItemClass foodDrinkItemClass = boundItem as FoodDrinkItemClass;
      if (boundItem != null && foodDrinkItemClass != null)
      {
        bool canUse = true;
        Plugin.RealHealthController.CanConsume(__instance, boundItem, ref canUse);
        if (PluginConfig.EnableMedicalLogging.Value)
          ModulePatch.Logger.LogWarning((object) ("quick slot, can use = " + canUse.ToString()));
        if (!canUse)
          callback.Invoke(Result<IHandsController>.op_Implicit((IHandsController) null));
        return canUse;
      }
      if (Plugin.FikaPresent)
      {
        MedsItemClass medsItemClass = boundItem as MedsItemClass;
        if (boundItem != null && medsItemClass != null)
        {
          // ISSUE: method pointer
          __instance.SetInHands(medsItemClass, (EBodyPart) 7, 1, new Callback<GInterface176>((object) GControl4.Class2153.class2153_0, __methodptr(method_1)));
          callback.Invoke(Result<IHandsController>.op_Implicit((IHandsController) null));
          return false;
        }
      }
    }
    return true;
  }
}
