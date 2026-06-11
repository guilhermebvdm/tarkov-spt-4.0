// Decompiled with JetBrains decompiler
// Type: RealismMod.Audio.GunshotVolumePatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace RealismMod.Audio;

public class GunshotVolumePatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (GClass869).GetMethod("Enqueue");
  }

  [SPT.Reflection.Patching.PatchPrefix]
  private static void PatchPrefix(GClass869 __instance, ref float volume)
  {
    volume *= PluginConfig.GunshotVolume.Value;
  }
}
