// Decompiled with JetBrains decompiler
// Type: RealismMod.AfterPenPlatePatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.Ballistics;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class AfterPenPlatePatch : ModulePatch
{
  private static FieldInfo armorCompsField;

  protected virtual MethodBase GetTargetMethod()
  {
    AfterPenPlatePatch.armorCompsField = AccessTools.Field(typeof (Player), "_preAllocatedArmorComponents");
    return (MethodBase) typeof (EftBulletClass).GetMethod("smethod_3", BindingFlags.Public | BindingFlags.Static);
  }

  [PatchPrefix]
  private static bool Prefix(
    BallisticCollider parentBallisticCollider,
    bool isForwardHit,
    EftBulletClass shot)
  {
    BodyPartCollider bodyPartCollider;
    if (!isForwardHit || Object.op_Equality((Object) (bodyPartCollider = parentBallisticCollider as BodyPartCollider), (Object) null) || bodyPartCollider.playerBridge == null)
      return false;
    Player playerByProfileId = Utils.GetPlayerByProfileId(bodyPartCollider.playerBridge.iPlayer.ProfileId);
    if (Object.op_Equality((Object) playerByProfileId, (Object) null) || Object.op_Inequality((Object) playerByProfileId, (Object) null) && playerByProfileId.UsedSimplifiedSkeleton)
      return true;
    List<ArmorComponent> armorComponentList = (List<ArmorComponent>) AfterPenPlatePatch.armorCompsField.GetValue((object) playerByProfileId);
    armorComponentList.Clear();
    playerByProfileId.Inventory.GetPutOnArmorsNonAlloc(armorComponentList);
    ArmorPlateCollider armorPlateCollider = bodyPartCollider as ArmorPlateCollider;
    EArmorPlateCollider plateColliderType = Object.op_Equality((Object) armorPlateCollider, (Object) null) ? (EArmorPlateCollider) (object) 0 : armorPlateCollider.ArmorPlateColliderType;
    for (int index = 0; index < armorComponentList.Count; ++index)
    {
      ArmorComponent armorComponent = armorComponentList[index];
      if (armorComponent.ShotMatches(bodyPartCollider.BodyPartColliderType, plateColliderType))
      {
        if (PluginConfig.EnableBallisticsLogging.Value)
        {
          ModulePatch.Logger.LogWarning((object) "///AFTER PLATE PEN///");
          ModulePatch.Logger.LogWarning((object) ("damage before = " + shot.Damage.ToString()));
          ModulePatch.Logger.LogWarning((object) ("pen before = " + shot.PenetrationPower.ToString()));
        }
        float factor = 1f;
        if (((GClass3175) armorComponent).Item.CurrentAddress is GClass3184 currentAddress && currentAddress.Slot is GClass2929 slot && ((Slot) slot).BluntDamageReduceFromSoftArmor)
          factor = 0.7f;
        BallisticsController.CalcAfterPenStats(armorComponent.Repairable.Durability, (float) armorComponent.ArmorClass, (float) armorComponent.Repairable.TemplateDurability, ref shot.Damage, ref shot.PenetrationPower, factor);
        if (PluginConfig.EnableBallisticsLogging.Value)
        {
          ModulePatch.Logger.LogWarning((object) ("damage after = " + shot.Damage.ToString()));
          ModulePatch.Logger.LogWarning((object) ("pen after = " + shot.PenetrationPower.ToString()));
          ModulePatch.Logger.LogWarning((object) "////");
          break;
        }
        break;
      }
    }
    return false;
  }
}
