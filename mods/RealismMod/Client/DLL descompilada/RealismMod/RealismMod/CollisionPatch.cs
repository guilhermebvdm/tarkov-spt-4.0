// Decompiled with JetBrains decompiler
// Type: RealismMod.CollisionPatch
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using Comfort.Common;
using EFT;
using EFT.Ballistics;
using EFT.InventoryLogic;
using HarmonyLib;
using JsonType;
using SPT.Reflection.Patching;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

#nullable disable
namespace RealismMod;

public class CollisionPatch : ModulePatch
{
  private static FieldInfo _playerField;
  private static FieldInfo _hitIgnoreField;
  private static int _timer;
  private static MaterialType[] _allowedMats;
  private static Vector3 _startLeftDir;
  private static Vector3 _startRightDir;
  private static Vector3 _startDownDir;
  private static Vector3 _wiggleLeftDir;
  private static Vector3 _wiggleRightDir;
  private static Vector3 _wiggleDownDir;
  private static int PlayerMask;

  protected virtual MethodBase GetTargetMethod()
  {
    CollisionPatch._playerField = AccessTools.Field(typeof (Player.FirearmController), "_player");
    CollisionPatch._hitIgnoreField = AccessTools.Field(typeof (Player.FirearmController), "func_2");
    return (MethodBase) typeof (Player.FirearmController).GetMethod("method_11", BindingFlags.Instance | BindingFlags.Public);
  }

  private static void SetMountingStatus(EBracingDirection coverDir)
  {
    if (!StanceController.IsMounting)
      StanceController.BracingDirection = coverDir;
    StanceController.IsBracing = true;
  }

  private static Vector3 GetWiggleDir(EBracingDirection coverDir)
  {
    switch (coverDir)
    {
      case EBracingDirection.Top:
        return CollisionPatch._wiggleDownDir;
      case EBracingDirection.Left:
        return CollisionPatch._wiggleLeftDir;
      case EBracingDirection.Right:
        return CollisionPatch._wiggleRightDir;
      default:
        return Vector3.zero;
    }
  }

  private static bool IsBracingProne(Player player)
  {
    if (!player.IsInPronePose)
      return false;
    CollisionPatch.SetMountingStatus(EBracingDirection.Top);
    return true;
  }

  private static bool CheckForCoverCollision(
    EBracingDirection coverDir,
    Vector3 start,
    Vector3 direction,
    Vector3 spherePos,
    float radius)
  {
    RaycastHit raycastHit;
    if (Physics.Linecast(start, direction, ref raycastHit, LayerMask.op_Implicit(EFTHardSettings.Instance.WEAPON_OCCLUSION_LAYERS)) && ((Component) ((RaycastHit) ref raycastHit).collider).gameObject.layer != CollisionPatch.PlayerMask)
    {
      CollisionPatch.SetMountingStatus(coverDir);
      StanceController.CoverWiggleDirection = CollisionPatch.GetWiggleDir(coverDir);
      return true;
    }
    foreach (Component component in Physics.OverlapSphere(spherePos, radius, LayerMask.op_Implicit(EFTHardSettings.Instance.WEAPON_OCCLUSION_LAYERS)))
    {
      if (component.gameObject.layer != CollisionPatch.PlayerMask)
      {
        CollisionPatch.SetMountingStatus(coverDir);
        StanceController.CoverWiggleDirection = CollisionPatch.GetWiggleDir(coverDir);
        return true;
      }
    }
    return false;
  }

  private static void DetectBracing(Player.FirearmController fc, Player player, float ln)
  {
    ++CollisionPatch._timer;
    if (CollisionPatch._timer >= 60)
    {
      CollisionPatch._timer = 0;
      Transform weaponRootAnim = player.ProceduralWeaponAnimation.HandsContainer.WeaponRootAnim;
      Vector3 vector3_1 = weaponRootAnim.TransformDirection(Vector3.up);
      Vector3 vector3_2 = WeaponStats.BipodIsDeployed ? new Vector3(CollisionPatch._startDownDir.x, CollisionPatch._startDownDir.y, CollisionPatch._startDownDir.z - 0.21f) : CollisionPatch._startDownDir;
      Vector3 vector3_3 = Vector3.op_Addition(weaponRootAnim.position, weaponRootAnim.TransformDirection(vector3_2));
      Vector3 vector3_4 = Vector3.op_Addition(weaponRootAnim.position, weaponRootAnim.TransformDirection(CollisionPatch._startLeftDir));
      Vector3 vector3_5 = Vector3.op_Addition(weaponRootAnim.position, weaponRootAnim.TransformDirection(CollisionPatch._startRightDir));
      Vector3 vector3_6 = Vector3.op_Addition(weaponRootAnim.position, weaponRootAnim.TransformDirection(new Vector3(0.0f, -0.45f, -0.1f)));
      Vector3 vector3_7 = Vector3.op_Addition(weaponRootAnim.position, weaponRootAnim.TransformDirection(new Vector3(0.05f, -0.5f, -0.065f)));
      Vector3 vector3_8 = Vector3.op_Addition(weaponRootAnim.position, weaponRootAnim.TransformDirection(new Vector3(-0.05f, -0.5f, -0.065f)));
      Vector3 vector3_9 = Vector3.op_Subtraction(vector3_3, Vector3.op_Multiply(vector3_1, ln));
      Vector3 vector3_10 = Vector3.op_Subtraction(vector3_4, Vector3.op_Multiply(vector3_1, ln));
      Vector3 vector3_11 = Vector3.op_Subtraction(vector3_5, Vector3.op_Multiply(vector3_1, ln));
      if (PluginConfig.EnableGeneralLogging.Value)
      {
        DebugGizmos.SingleObjects.Line(vector3_3, vector3_9, Color.red, 0.02f, true, 0.3f, true);
        DebugGizmos.SingleObjects.Sphere(vector3_6, 0.1f, Color.red, true, 0.3f);
        DebugGizmos.SingleObjects.Line(vector3_4, vector3_10, Color.green, 0.02f, true, 0.3f, true);
        DebugGizmos.SingleObjects.Sphere(vector3_7, 0.1f, Color.green, true, 0.3f);
        DebugGizmos.SingleObjects.Line(vector3_5, vector3_11, Color.yellow, 0.02f, true, 0.3f, true);
        DebugGizmos.SingleObjects.Sphere(vector3_8, 0.1f, Color.yellow, true, 0.3f);
      }
      if (PluginConfig.OverrideMounting.Value && Plugin.ServerConfig.enable_stances && (CollisionPatch.IsBracingProne(player) || CollisionPatch.CheckForCoverCollision(EBracingDirection.Top, vector3_3, vector3_9, vector3_6, 0.045f) || CollisionPatch.CheckForCoverCollision(EBracingDirection.Left, vector3_4, vector3_10, vector3_7, 0.09f) || CollisionPatch.CheckForCoverCollision(EBracingDirection.Right, vector3_5, vector3_11, vector3_8, 0.09f)))
        return;
      StanceController.IsBracing = false;
    }
    if (StanceController.IsBracing || StanceController.IsMounting)
    {
      float num1 = StanceController.BracingDirection == EBracingDirection.Top ? 0.75f : 1f;
      float num2 = StanceController.TreatWeaponAsPistolStance ? 0.25f : 0.75f;
      float num3 = !StanceController.IsMounting || !fc.Weapon.IsBeltMachineGun || !WeaponStats.BipodIsDeployed ? (!StanceController.IsMounting || !fc.Weapon.IsBeltMachineGun ? (!StanceController.IsMounting || !WeaponStats.BipodIsDeployed ? (StanceController.IsMounting ? 0.85f : 0.95f) : 0.45f) : 0.75f) : 0.4f;
      float num4 = !StanceController.IsMounting || !WeaponStats.BipodIsDeployed ? (StanceController.IsMounting ? 0.35f : 0.65f) : 0.05f;
      StanceController.BracingRecoilBonus = Mathf.Lerp(StanceController.BracingRecoilBonus, num3 * num1, 0.04f);
      StanceController.BracingSwayBonus = Mathf.Lerp(StanceController.BracingSwayBonus, num4 * num1, 0.04f);
    }
    else
    {
      StanceController.BracingSwayBonus = Mathf.Lerp(StanceController.BracingSwayBonus, 1f, 0.05f);
      StanceController.BracingRecoilBonus = Mathf.Lerp(StanceController.BracingRecoilBonus, 1f, 0.05f);
    }
  }

  private static void DoMelee(Player.FirearmController fc, Player player, float ln)
  {
    if (StanceController.CurrentStance != EStance.Melee || !StanceController.CanDoMeleeDetection || StanceController.MeleeHitSomething)
      return;
    Transform weaponRootAnim = player.ProceduralWeaponAnimation.HandsContainer.WeaponRootAnim;
    Vector3 vector3_1 = weaponRootAnim.TransformDirection(Vector3.up);
    Vector3 vector3_2;
    // ISSUE: explicit constructor call
    ((Vector3) ref vector3_2).\u002Ector(0.0f, -0.5f, -0.025f);
    Vector3 startPoint = Vector3.op_Addition(weaponRootAnim.position, weaponRootAnim.TransformDirection(vector3_2));
    Vector3 endPoint = Vector3.op_Subtraction(startPoint, Vector3.op_Multiply(vector3_1, ln - (WeaponStats.HasBayonet ? 0.1f : 0.25f)));
    if (PluginConfig.EnablePWALogging.Value)
      DebugGizmos.SingleObjects.Line(startPoint, endPoint, Color.red, 0.02f, true, 0.3f, true);
    BallisticCollider ballisticCollider = (BallisticCollider) null;
    RaycastHit raycastHit;
    if (!Physics.Linecast(startPoint, endPoint, ref raycastHit, LayerMask.op_Implicit(GClass3449.HitMask)))
      return;
    Collider collider = ((RaycastHit) ref raycastHit).collider;
    BaseBallistic component = ((Component) collider).GetComponent<BaseBallistic>();
    if (Object.op_Inequality((Object) component, (Object) null))
      ballisticCollider = component.Get(((RaycastHit) ref raycastHit).point);
    float totalWeight = ((Item) fc.Weapon).TotalWeight;
    float num1 = (float) ((19.0 + (double) WeaponStats.BaseMeleeDamage * (1.0 + (double) SkillManager.SkillBuffClass.op_Implicit(player.Skills.StrengthBuffMeleePowerInc)) * (1.0 + (double) totalWeight / 10.0)) * (player.Physical.HandsStamina.Exhausted ? (double) Singleton<BackendConfigSettingsClass>.Instance.Stamina.ExhaustedMeleeDamageMultiplier : 1.0)) * player.Speed;
    float num2 = (float) (15.0 + (double) WeaponStats.BaseMeleePen * (1.0 + (double) totalWeight / 10.0));
    bool flag1 = false;
    if (Object.op_Inequality((Object) (ballisticCollider as BodyPartCollider), (Object) null))
      player.ExecuteSkill((Action) (() => player.Skills.FistfightAction.Complete(1f)));
    if ((ballisticCollider.TypeOfMaterial == 10 || ballisticCollider.TypeOfMaterial == 11) && (double) Random.Range(1, 11) > 4.0 + (double) WeaponStats.BaseMeleeDamage)
      flag1 = true;
    bool flag2 = ((IEnumerable<MaterialType>) CollisionPatch._allowedMats).Contains<MaterialType>(ballisticCollider.TypeOfMaterial) && !flag1;
    if (WeaponStats.HasBayonet | flag2 && ((Component) ((RaycastHit) ref raycastHit).collider).gameObject.layer != CollisionPatch.PlayerMask)
    {
      Vector3 position = fc.CurrentFireport.position;
      Vector3 weaponDirection = fc.WeaponDirection;
      Vector3 vector3_3 = position;
      fc.AdjustShotVectors(ref vector3_3, ref weaponDirection);
      Vector3 vector3_4 = weaponDirection;
      Singleton<GameWorld>.Instance.HackShot(new DamageInfoStruct()
      {
        SourceId = ((Item) fc.Weapon).Id,
        DamageType = (EDamageType) 1024 /*0x0400*/,
        Damage = num1,
        PenetrationPower = num2,
        ArmorDamage = (float) (10.0 + (double) num1 / 10.0),
        Direction = ((Vector3) ref vector3_4).normalized,
        HitCollider = collider,
        HitPoint = ((RaycastHit) ref raycastHit).point,
        Player = Singleton<GameWorld>.Instance.GetAlivePlayerBridgeByProfileID(player.ProfileId),
        HittedBallisticCollider = ballisticCollider,
        HitNormal = ((RaycastHit) ref raycastHit).normal,
        Weapon = (Item) fc.Item,
        IsForwardHit = true,
        StaminaBurnRate = 5f
      });
    }
    float num3 = WeaponStats.HasBayonet ? 10f : 12f;
    Singleton<BetterAudio>.Instance.PlayDropItem(component.SurfaceSound, (EItemDropSoundType) 3, ((RaycastHit) ref raycastHit).point, num3);
    player.Physical.ConsumeAsMelee((float) (0.20000000298023224 * (1.0 + (double) totalWeight * 0.10000000149011612)));
    StanceController.CanDoMeleeDetection = false;
    StanceController.MeleeHitSomething = true;
  }

  [SPT.Reflection.Patching.PatchPrefix]
  private static void PatchPrefix(
    Player.FirearmController __instance,
    Vector3 origin,
    float ln,
    Vector3? weaponUp = null)
  {
    Player player = (Player) CollisionPatch._playerField.GetValue((object) __instance);
    if (!player.IsYourPlayer)
      return;
    CollisionPatch.DoMelee(__instance, player, ln);
    CollisionPatch.DetectBracing(__instance, player, ln);
  }

  static CollisionPatch()
  {
    // ISSUE: unable to decompile the method.
  }
}
