// Decompiled with JetBrains decompiler
// Type: RealismMod.AimPunchPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Comfort.Common;
using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace RealismMod;

internal class AimPunchPatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (ForceEffector).GetMethod("Initialize", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPostfix]
  private static void Postfix(ForceEffector __instance)
  {
    __instance.WiggleMagnitude = Singleton<BackendConfigSettingsClass>.Instance.AimPunchMagnitude * PluginConfig.AimPunchMulti.Value;
  }
}
