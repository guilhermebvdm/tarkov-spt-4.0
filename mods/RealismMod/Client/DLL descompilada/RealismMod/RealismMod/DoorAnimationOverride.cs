// Decompiled with JetBrains decompiler
// Type: RealismMod.DoorAnimationOverride
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class DoorAnimationOverride : ModulePatch
{
  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (MovementContext).GetMethod("SetInteractInHands", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPrefix]
  private static void PatchPrefix(MovementContext __instance, ref EInteraction interaction)
  {
    if (!PluginConfig.EnableAnimationFixes.Value || !((Player) AccessTools.Field(typeof (MovementContext), "_player").GetValue((object) __instance)).IsYourPlayer)
      return;
    // ISSUE: cast to a reference type
    // ISSUE: explicit reference operation
    switch ((EInteraction) (int) ^(byte&) ref interaction - 23)
    {
      case 0:
      case 2:
      case 4:
        // ISSUE: cast to a reference type
        // ISSUE: explicit reference operation
        ^(sbyte&) ref interaction = (sbyte) 33;
        break;
      case 1:
      case 3:
      case 5:
        // ISSUE: cast to a reference type
        // ISSUE: explicit reference operation
        ^(sbyte&) ref interaction = (sbyte) 31 /*0x1F*/;
        break;
    }
  }
}
