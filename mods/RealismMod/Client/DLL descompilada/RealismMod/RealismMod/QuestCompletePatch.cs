// Decompiled with JetBrains decompiler
// Type: RealismMod.QuestCompletePatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT.Communications;
using EFT.UI;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class QuestCompletePatch : ModulePatch
{
  private static string[] _hazardHealQuests = new string[3]
  {
    "667c643869df8111b81cb6dc",
    "667dbbc9c62a7c2ee8fe25b2",
    "6705425a0351f9f55b7d8c61"
  };

  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (QuestView).GetMethod("FinishQuest", BindingFlags.Instance | BindingFlags.Public, (Binder) null, new Type[0], (ParameterModifier[]) null);
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(QuestView __instance)
  {
    if (!((IEnumerable<string>) QuestCompletePatch._hazardHealQuests).Contains<string>(__instance.QuestId))
      return;
    HazardTracker.TotalRadiation = 0.0f;
    HazardTracker.TotalToxicity = 0.0f;
    HazardTracker.UpdateHazardValues(ProfileData.PMCProfileId);
    HazardTracker.UpdateHazardValues(ProfileData.ScavProfileId);
    HazardTracker.SaveHazardValues();
    if (PluginConfig.EnableMedNotes.Value)
      NotificationManagerClass.DisplayNotification((NotificationAbstractClass) new GClass2314(GClass2112.Localized("Blood Tests Came Back Clear, Your Radiation Poisoning Has Been Cured.", (string) null), (ENotificationDurationType) 1, (ENotificationIconType) 5, new Color?()));
  }
}
