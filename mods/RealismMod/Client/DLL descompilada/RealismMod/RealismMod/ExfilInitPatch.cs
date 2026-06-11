// Decompiled with JetBrains decompiler
// Type: RealismMod.ExfilInitPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT.Interactive;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class ExfilInitPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (ExfiltrationControllerClass).GetMethod("InitAllExfiltrationPoints");
  }

  [SPT.Reflection.Patching.PatchPostfix]
  public static void PatchPostfix(ExfiltrationControllerClass __instance)
  {
    GameWorldController.ExfilsInLocation.Clear();
    foreach (ExfiltrationPoint exfiltrationPoint in __instance.ExfiltrationPoints)
    {
      if (PluginConfig.ZoneDebug.Value)
        ModulePatch.Logger.LogWarning((object) $"exfil {((Object) exfiltrationPoint).name}, id {exfiltrationPoint.Id}, go {((Component) exfiltrationPoint).gameObject.tag}, name {exfiltrationPoint.Settings.Name},  id {exfiltrationPoint.Settings.Id}");
      GameWorldController.ExfilsInLocation.Add(exfiltrationPoint);
    }
  }
}
