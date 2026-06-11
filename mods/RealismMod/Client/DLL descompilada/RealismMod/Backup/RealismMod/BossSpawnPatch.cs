// Decompiled with JetBrains decompiler
// Type: RealismMod.BossSpawnPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using BepInEx.Logging;
using EFT;
using SPT.Reflection.Patching;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class BossSpawnPatch : ModulePatch
{
  private static string[] _forbiddenZones = new string[4]
  {
    "BotZone",
    "BotZoneFloor1",
    "BotZoneFloor2",
    "BotZoneBasement"
  };

  protected virtual MethodBase GetTargetMethod()
  {
    return (MethodBase) typeof (BossLocationSpawn).GetMethod("ParseMainTypesTypes");
  }

  private static void HandleZombies(
    BossLocationSpawn __instance,
    ref bool disabledZombieSpawn,
    bool isZombie)
  {
    bool flag = Plugin.ServerConfig.realistic_zombies && Plugin.ModInfo.DoGasEvent;
    if (!isZombie || flag)
      return;
    __instance.BossChance = 0.0f;
    __instance.ShallSpawn = false;
    disabledZombieSpawn = true;
  }

  private static bool IsForbiddenSpawnZone(string[] zones)
  {
    return ((IEnumerable<string>) BossSpawnPatch._forbiddenZones).Intersect<string>((IEnumerable<string>) zones).Any<string>();
  }

  [SPT.Reflection.Patching.PatchPostfix]
  public static void PatchPostfix(BossLocationSpawn __instance)
  {
    GameWorldController.RunEarlyGameCheck();
    bool isZombie = __instance.BossType.ToString().ToLower().Contains("infected");
    bool disabledZombieSpawn = false;
    BossSpawnPatch.HandleZombies(__instance, ref disabledZombieSpawn, isZombie);
    string[] zones = __instance.BossZone.Split(new char[1]
    {
      ','
    });
    if (disabledZombieSpawn || BossSpawnPatch.IsForbiddenSpawnZone(zones) && !isZombie)
      return;
    bool flag1 = __instance.BossType == 21 && Plugin.ModInfo.DoGasEvent && !Plugin.ModInfo.DoExtraRaiders;
    bool flag2 = __instance.BossType == 9 && Plugin.ModInfo.DoExtraRaiders;
    bool flag3 = __instance.BossType == 51 || __instance.BossType == 52;
    bool flag4 = !flag3 && Plugin.ModInfo.IsHalloween && (Plugin.ModInfo.HasExploded || GameWorldController.DidExplosionClientSide);
    bool flag5 = Plugin.ModInfo.IsPreExplosion && GameWorldController.IsRightDateForExp;
    bool flag6 = ((flag4 ? 1 : (Plugin.ModInfo.DoGasEvent ? 1 : 0)) | (flag5 ? 1 : 0)) != 0;
    bool flag7 = __instance.BossType == 9;
    bool flag8 = __instance.BossType != 21;
    if (flag1)
    {
      bool doExtraCultists = Plugin.ModInfo.DoExtraCultists;
      __instance.BossChance = (double) __instance.BossChance != 0.0 || doExtraCultists ? 100f : 25f;
      __instance.ShallSpawn = GClass835.IsTrue100(__instance.BossChance);
    }
    else if (flag2)
      __instance.BossChance = 100f;
    else if ((flag6 || Plugin.ModInfo.DoExtraRaiders) && !flag7 && !flag8 && !flag3 && !isZombie)
    {
      __instance.BossChance = 0.0f;
      __instance.ShallSpawn = false;
    }
    if (!PluginConfig.ZoneDebug.Value)
      return;
    ModulePatch.Logger.LogWarning((object) "=============");
    ModulePatch.Logger.LogWarning((object) $"Do Gas Event ? {Plugin.ModInfo.DoGasEvent}");
    ModulePatch.Logger.LogWarning((object) $"Do raider Event ? {Plugin.ModInfo.DoExtraRaiders}");
    ModulePatch.Logger.LogWarning((object) $"Do extra cultists ? {Plugin.ModInfo.DoExtraCultists}");
    ManualLogSource logger1 = ModulePatch.Logger;
    WildSpawnType bossType = __instance.BossType;
    string str1 = "Boss type " + bossType.ToString();
    logger1.LogWarning((object) str1);
    ManualLogSource logger2 = ModulePatch.Logger;
    bossType = __instance.BossType;
    string str2 = "Boss type " + bossType.ToString();
    logger2.LogWarning((object) str2);
    ModulePatch.Logger.LogWarning((object) ("Spawn Chance " + __instance.BossChance.ToString()));
    ModulePatch.Logger.LogWarning((object) ("Shall Spawn " + __instance.ShallSpawn.ToString()));
    ModulePatch.Logger.LogWarning((object) "=============");
  }
}
