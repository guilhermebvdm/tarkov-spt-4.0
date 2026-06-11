// Decompiled with JetBrains decompiler
// Type: RealismMod.SyncWithCharacterSkillsPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SPT.Reflection.Patching;
using System.Reflection;

#nullable disable
namespace RealismMod;

public class SyncWithCharacterSkillsPatch : ModulePatch
{
  private static FieldInfo _playerField;

  protected virtual MethodBase GetTargetMethod()
  {
    SyncWithCharacterSkillsPatch._playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    return (MethodBase) typeof (Player.FirearmController).GetMethod("SyncWithCharacterSkills", BindingFlags.Instance | BindingFlags.Public);
  }

  [SPT.Reflection.Patching.PatchPostfix]
  private static void PatchPostfix(Player.FirearmController __instance)
  {
    Player player = (Player) SyncWithCharacterSkillsPatch._playerField.GetValue((object) __instance);
    if (!player.IsYourPlayer)
      return;
    SkillManager.GClass2017 weaponInfo = player.Skills.GetWeaponInfo((Item) __instance.Item);
    PlayerState.StrengthWeightBuff = player.Skills.StrengthBuffLiftWeightInc.Value;
    PlayerState.StrengthSkillAimBuff = player.Skills.StrengthBuffAimFatigue.Value;
    PlayerState.ReloadSkillMulti = weaponInfo.ReloadSpeed;
    PlayerState.FixSkillMulti = weaponInfo.FixSpeed;
    PlayerState.WeaponSkillErgo = weaponInfo.DeltaErgonomics;
    PlayerState.AimSkillADSBuff = weaponInfo.AimSpeed;
  }
}
