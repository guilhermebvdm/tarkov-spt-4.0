// Decompiled with JetBrains decompiler
// Type: RealismMod.GetAvailableActionsPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT.Interactive;
using HarmonyLib;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class GetAvailableActionsPatch : ModulePatch
{
  public static void DummyAction()
  {
  }

  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) AccessTools.FirstMethod(typeof (GetActionsClass), (Func<MethodInfo, bool>) (x => x.Name == "GetAvailableActions" && x.GetParameters()[0].Name == "owner"));
  }

  [SPT.Reflection.Patching.PatchPrefix]
  public static bool PatchPrefix(object[] __args, ref ActionsReturnClass __result)
  {
    if (__args[1] is DebugComponent)
    {
      DebugComponent debugComponent = __args[1] as DebugComponent;
      __result = new ActionsReturnClass()
      {
        Actions = debugComponent.DebugActions
      };
      return false;
    }
    if (!(__args[1] is InteractionZone))
      return true;
    InteractionZone interactionZone = __args[1] as InteractionZone;
    __result = new ActionsReturnClass()
    {
      Actions = interactionZone.InteractableActions
    };
    return false;
  }

  [SPT.Reflection.Patching.PatchPostfix]
  public static void PatchPostfix(object[] __args, ActionsReturnClass __result)
  {
    LootItem lootItem;
    if (__result == null || __result.Actions == null || __args == null || ((IEnumerable<object>) __args).Count<object>() <= 0 || !Object.op_Inequality((Object) (lootItem = __args[1] as LootItem), (Object) null))
      return;
    HazardAnalyser hazardAnalyser;
    if ((lootItem.TemplateId == "66fd571a05370c3ee1a1c613" || lootItem.TemplateId == "66fd521442055447e2304fda") && ((Component) lootItem).gameObject.TryGetComponent<HazardAnalyser>(ref hazardAnalyser))
    {
      bool flag1 = hazardAnalyser.TargetZone != null && hazardAnalyser.TargetZone.HasBeenAnalysed;
      bool flag2 = hazardAnalyser.ZoneAlreadyHasDevice();
      if (hazardAnalyser.CanTurnOn && !flag1 && !flag2)
        __result.Actions.AddRange((IEnumerable<ActionsTypesClass>) hazardAnalyser.Actions);
    }
    TransmitterHalloweenEvent transmitterHalloweenEvent;
    if (lootItem.TemplateId == "6703082a766cb6d11310094e" && ((Component) lootItem).gameObject.TryGetComponent<TransmitterHalloweenEvent>(ref transmitterHalloweenEvent))
    {
      bool flag3 = transmitterHalloweenEvent.ZoneAlreadyHasDevice();
      bool flag4 = transmitterHalloweenEvent.TargetZone != null && transmitterHalloweenEvent.TargetZone.HasBeenAnalysed;
      if (transmitterHalloweenEvent.TriggeredExplosion | flag4)
      {
        // ISSUE: reference to a compiler-generated field
        // ISSUE: reference to a compiler-generated field
        __result.Actions = new List<ActionsTypesClass>()
        {
          new ActionsTypesClass()
          {
            Name = "",
            Action = GetAvailableActionsPatch.\u003C\u003EO.\u003C0\u003E__DummyAction ?? (GetAvailableActionsPatch.\u003C\u003EO.\u003C0\u003E__DummyAction = new Action(GetAvailableActionsPatch.DummyAction))
          }
        };
      }
      else if (transmitterHalloweenEvent.CanTurnOn && !flag3)
        __result.Actions.AddRange((IEnumerable<ActionsTypesClass>) transmitterHalloweenEvent.Actions);
    }
  }
}
