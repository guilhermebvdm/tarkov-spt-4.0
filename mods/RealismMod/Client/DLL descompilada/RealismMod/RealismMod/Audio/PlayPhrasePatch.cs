// Decompiled with JetBrains decompiler
// Type: RealismMod.Audio.PlayPhrasePatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using SPT.Reflection.Patching;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod.Audio;

public class PlayPhrasePatch : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (PhraseSpeakerClass).GetMethod("Play", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPrefix]
  private static bool PatchPrefix(
    PhraseSpeakerClass __instance,
    EPhraseTrigger trigger,
    ETagStatus tags)
  {
    Player yourPlayer = Utils.GetYourPlayer();
    if (Object.op_Equality((Object) yourPlayer, (Object) null))
      return true;
    PhraseSpeakerClass speaker = yourPlayer.Speaker;
    return speaker == null || speaker != __instance || !GearController.HasGasMask || trigger != 29 && (tags & 8192 /*0x2000*/) != 8192 /*0x2000*/;
  }
}
