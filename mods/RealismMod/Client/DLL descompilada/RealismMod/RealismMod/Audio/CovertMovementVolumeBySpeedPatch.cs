// Decompiled with JetBrains decompiler
// Type: RealismMod.Audio.CovertMovementVolumeBySpeedPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace RealismMod.Audio;

public class CovertMovementVolumeBySpeedPatch : ModulePatch
{
  private static FieldInfo playerField;

  protected virtual MethodBase GetTargetMethod()
  {
    CovertMovementVolumeBySpeedPatch.playerField = AccessTools.Field(typeof (MovementContext), "_player");
    return (MethodBase) typeof (MovementContext).GetMethod("get_CovertMovementVolumeBySpeed", BindingFlags.Instance | BindingFlags.Public);
  }

  [PatchPrefix]
  private static bool Prefix(MovementContext __instance, ref float __result)
  {
    Player player = (Player) CovertMovementVolumeBySpeedPatch.playerField.GetValue((object) __instance);
    float num = player.IsYourPlayer ? PluginConfig.PlayerMovementVolume.Value : PluginConfig.NPCMovementVolume.Value;
    if (!__instance.SoftSurface)
      __result = (float) (1.0 - (double) SkillManager.SkillBuffClass.op_Implicit(player.Skills.CovertMovementLoud) * (double) __instance.CovertEfficiency) * num;
    __result = (float) (1.0 - (double) SkillManager.SkillBuffClass.op_Implicit(player.Skills.CovertMovementSoundVolume) * (double) __instance.CovertEfficiency) * num;
    return false;
  }
}
