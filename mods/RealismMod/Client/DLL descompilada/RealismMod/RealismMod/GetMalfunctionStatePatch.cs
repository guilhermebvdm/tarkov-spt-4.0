// Decompiled with JetBrains decompiler
// Type: RealismMod.GetMalfunctionStatePatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Comfort.Common;
using EFT;
using EFT.Communications;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class GetMalfunctionStatePatch : ModulePatch
{
  private static FieldInfo playerField;

  protected virtual MethodBase GetTargetMethod()
  {
    GetMalfunctionStatePatch.playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    return (MethodBase) typeof (Player.FirearmController).GetMethod("GetMalfunctionState", BindingFlags.Instance | BindingFlags.Public);
  }

  private static void ExplodeWeapon(Player.FirearmController fc, Player player)
  {
    Singleton<Systems.Effects.Effects>.Instance.EmitGrenade("Grenade_new2", fc.CurrentFireport.Original.position, Vector3.up, 1f);
    fc.Weapon.Repairable.Durability = 0.0f;
    fc.Weapon.Repairable.MaxDurability = 0.0f;
    if (player.ActiveHealthController == null)
      return;
    if (player.IsYourPlayer)
    {
      Plugin.RealHealthController.AddBasesEFTEffect(player, "Contusion", (EBodyPart) 0, new float?(0.0f), new float?(10f), new float?(5f), new float?(1f));
      Plugin.RealHealthController.AddBasesEFTEffect(player, "TunnelVision", (EBodyPart) 0, new float?(0.0f), new float?(10f), new float?(5f), new float?(1f));
      Plugin.RealHealthController.AddBasesEFTEffect(player, "Tremor", (EBodyPart) 0, new float?(0.0f), new float?(10f), new float?(5f), new float?(1f));
      Plugin.RealHealthController.AddBasesEFTEffect(player, "LightBleeding", (EBodyPart) 0, new float?(), new float?(), new float?(), new float?());
      Plugin.RealHealthController.AddBasesEFTEffect(player, "LightBleeding", (EBodyPart) 4, new float?(), new float?(), new float?(), new float?());
    }
    double num1 = (double) player.ActiveHealthController.ApplyDamage((EBodyPart) 0, (float) Random.Range(5, 21), GClass2855.Existence);
    double num2 = (double) player.ActiveHealthController.ApplyDamage((EBodyPart) 4, (float) Random.Range(20, 61), GClass2855.Existence);
    if (fc.Item != null && ((TraderControllerClass) player.InventoryController).CanThrow((Item) fc.Item))
      ((TraderControllerClass) player.InventoryController).TryThrowItem((Item) fc.Item, (Callback) null, false);
  }

  [PatchPostfix]
  private static void Postfix(
    Player.FirearmController __instance,
    ref Weapon.EMalfunctionState __result,
    AmmoItemClass ammoToFire)
  {
    Player player = (Player) GetMalfunctionStatePatch.playerField.GetValue((object) __instance);
    bool flag1 = false;
    bool flag2 = MongoID.op_Equality(((Item) ammoToFire).Template._id, MongoID.op_Implicit("57371aab2459775a77142f22"));
    float maxDurability = __instance.Weapon.Repairable.MaxDurability;
    float durability = __instance.Weapon.Repairable.Durability;
    if (__instance.Weapon.AmmoCaliber == "9x18PM" & flag2)
    {
      if (flag2)
        __instance.Weapon.Repairable.Durability = Mathf.Max(__instance.Weapon.Repairable.Durability - __instance.Weapon.DurabilityBurnRatio * ammoToFire.DurabilityBurnModificator, 0.0f);
      int num1 = Random.Range(1, 11);
      float num2 = (float) (2.0 - (double) __instance.Weapon.Repairable.Durability / (double) __instance.Weapon.Repairable.MaxDurability);
      flag1 = (((double) __instance.Weapon.Repairable.Durability > 80.0 ? 0 : ((double) num1 <= 4.0 * (double) num2 ? 1 : 0)) & (flag2 ? 1 : 0)) != 0;
    }
    if (!(__instance.Weapon.AmmoCaliber != ammoToFire.Caliber) && (double) __instance.Weapon.Repairable.MaxDurability > 0.0)
      return;
    bool flag3 = flag1 || __instance.Weapon.AmmoCaliber == "366TKM" && ammoToFire.Caliber == "762x39" || __instance.Weapon.AmmoCaliber == "556x45NATO" && ammoToFire.Caliber == "762x35" || __instance.Weapon.AmmoCaliber == "762x51" && ammoToFire.Caliber == "68x51";
    bool flag4 = __instance.Weapon.AmmoCaliber == "762x39" && ammoToFire.Caliber == "366TKM" || __instance.Weapon.AmmoCaliber == "762x35" && ammoToFire.Caliber == "556x45NATO" || __instance.Weapon.AmmoCaliber == "68x51" && ammoToFire.Caliber == "762x51";
    if (player.IsYourPlayer)
    {
      if ((double) durability <= 0.0 | flag4 || flag3 && !Plugin.ServerConfig.malf_changes)
      {
        if ((double) durability <= 0.0)
          NotificationManagerClass.DisplayWarningNotification("Weapon Is Broken Beyond Repair", (ENotificationDurationType) 1);
        else
          NotificationManagerClass.DisplayWarningNotification("Wrong Ammo/Weapon Caliber Combination Or Weapon Is Broken", (ENotificationDurationType) 1);
        // ISSUE: cast to a reference type
        // ISSUE: explicit reference operation
        ^(int&) ref __result = 1;
      }
      else if (flag3)
      {
        NotificationManagerClass.DisplayWarningNotification("Catastrophic Failure. Wrong Ammo/Weapon Caliber Combination", (ENotificationDurationType) 1);
        GetMalfunctionStatePatch.ExplodeWeapon(__instance, player);
      }
    }
    else if ((double) durability <= 0.0 | flag4 || flag3 && !Plugin.ServerConfig.malf_changes)
    {
      // ISSUE: cast to a reference type
      // ISSUE: explicit reference operation
      ^(int&) ref __result = 1;
    }
    else if (flag3)
      GetMalfunctionStatePatch.ExplodeWeapon(__instance, player);
  }
}
