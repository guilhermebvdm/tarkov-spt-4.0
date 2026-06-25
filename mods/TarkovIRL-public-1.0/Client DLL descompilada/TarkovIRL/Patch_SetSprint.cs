// Decompiled with JetBrains decompiler
// Type: TarkovIRL.Patch_SetSprint
// Assembly: TarkovIRL, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C42939BD-7BF0-4586-ABE5-9D2EFC361A0B
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\TarkovIRL_WeaponsHandlingMod_1.0.0\BepInEx\plugins\TarkovIRL.dll

using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace TarkovIRL;

internal class Patch_SetSprint : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (FirearmsAnimator).GetMethod("SetSprint", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Prefix(FirearmsAnimator __instance, bool sprint)
  {
    TIRLUtils.LogError("set to sprint interrupted");
    return false;
  }
}
