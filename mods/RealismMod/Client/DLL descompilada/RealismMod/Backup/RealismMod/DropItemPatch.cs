// Decompiled with JetBrains decompiler
// Type: RealismMod.DropItemPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.Interactive;
using EFT.InventoryLogic;
using SPT.Reflection.Patching;
using System;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

internal class DropItemPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (GameWorld).GetMethod("ThrowItem", new Type[9]
    {
      typeof (Item),
      typeof (IPlayer),
      typeof (Vector3),
      typeof (Quaternion),
      typeof (Vector3),
      typeof (Vector3),
      typeof (bool),
      typeof (bool),
      typeof (float)
    });
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(ref LootItem __result, IPlayer player)
  {
    bool flag1 = MongoID.op_Equality(__result.Item.TemplateId, MongoID.op_Implicit("66fd571a05370c3ee1a1c613"));
    bool flag2 = MongoID.op_Equality(__result.Item.TemplateId, MongoID.op_Implicit("66fd521442055447e2304fda"));
    bool flag3 = MongoID.op_Equality(__result.Item.TemplateId, MongoID.op_Implicit("6703082a766cb6d11310094e"));
    if (flag1 | flag2)
    {
      HazardAnalyser hazardAnalyser1;
      if (((Component) __result).gameObject.TryGetComponent<HazardAnalyser>(ref hazardAnalyser1))
        Object.Destroy((Object) hazardAnalyser1);
      HazardAnalyser hazardAnalyser2 = ((Component) __result).gameObject.AddComponent<HazardAnalyser>();
      hazardAnalyser2._IPlayer = player;
      hazardAnalyser2._Player = Utils.GetPlayerByProfileId(player.ProfileId);
      hazardAnalyser2._LootItem = __result;
      hazardAnalyser2.TargetZoneType = flag1 ? EZoneType.Gas : EZoneType.Radiation;
      BoxCollider boxCollider = ((Component) hazardAnalyser2).gameObject.AddComponent<BoxCollider>();
      ((Collider) boxCollider).isTrigger = true;
      boxCollider.size = new Vector3(0.1f, 0.1f, 0.1f);
    }
    if (!flag3)
      return;
    TransmitterHalloweenEvent transmitterHalloweenEvent1;
    if (((Component) __result).gameObject.TryGetComponent<TransmitterHalloweenEvent>(ref transmitterHalloweenEvent1))
      Object.Destroy((Object) transmitterHalloweenEvent1);
    TransmitterHalloweenEvent transmitterHalloweenEvent2 = ((Component) __result).gameObject.AddComponent<TransmitterHalloweenEvent>();
    transmitterHalloweenEvent2._IPlayer = player;
    transmitterHalloweenEvent2._Player = Utils.GetPlayerByProfileId(player.ProfileId);
    transmitterHalloweenEvent2._LootItem = __result;
    transmitterHalloweenEvent2.TargetQuestZones = new string[1]
    {
      "SateliteCommLink"
    };
    transmitterHalloweenEvent2.QuestTrigger = "SateliteCommLinkEstablished";
    BoxCollider boxCollider1 = ((Component) transmitterHalloweenEvent2).gameObject.AddComponent<BoxCollider>();
    ((Collider) boxCollider1).isTrigger = true;
    boxCollider1.size = new Vector3(0.1f, 0.1f, 0.1f);
  }
}
