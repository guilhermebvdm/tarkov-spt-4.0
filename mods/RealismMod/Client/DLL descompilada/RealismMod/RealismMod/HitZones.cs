// Decompiled with JetBrains decompiler
// Type: RealismMod.HitZones
// Assembly: RealismMod, Version=0.14.8.0, Culture=neutral, PublicKeyToken=null
// MVID: 543CE9EB-B42D-4C5F-BBD9-23AF6383D504
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\Realism-Mod-1.6.4-SPT-3.11.0 (1)\BepInEx\plugins\RealismMod.dll

using UnityEngine;

#nullable disable
namespace RealismMod;

public static class HitZones
{
  public static EHitOrientation GetHitOrientation(Vector3 hitNormal, Transform colliderTransform)
  {
    Vector3 vector3 = colliderTransform.InverseTransformDirection(hitNormal);
    if ((double) Mathf.Abs(vector3.y) > (double) Mathf.Abs(vector3.x) && (double) Mathf.Abs(vector3.y) > (double) Mathf.Abs(vector3.z))
    {
      if ((double) vector3.y > 0.0)
      {
        if (PluginConfig.EnableBallisticsLogging.Value)
          Utils.Logger.LogWarning((object) "hit front side of the box collider");
        return EHitOrientation.FrontHit;
      }
      if (PluginConfig.EnableBallisticsLogging.Value)
        Utils.Logger.LogWarning((object) "hit back side of the box collider");
      return EHitOrientation.BackHit;
    }
    if ((double) Mathf.Abs(vector3.x) > (double) Mathf.Abs(vector3.y) && (double) Mathf.Abs(vector3.x) > (double) Mathf.Abs(vector3.z))
    {
      if ((double) vector3.x > 0.0)
      {
        if (PluginConfig.EnableBallisticsLogging.Value)
          Utils.Logger.LogWarning((object) "hit bottom side of the box collider");
        return EHitOrientation.BottomHit;
      }
      if (PluginConfig.EnableBallisticsLogging.Value)
        Utils.Logger.LogWarning((object) "hit top side of the box collider");
      return EHitOrientation.TopHit;
    }
    if ((double) vector3.z > 0.0)
    {
      if (PluginConfig.EnableBallisticsLogging.Value)
        Utils.Logger.LogWarning((object) "hit left side of the box collider");
      return EHitOrientation.LeftSideHit;
    }
    if (PluginConfig.EnableBallisticsLogging.Value)
      Utils.Logger.LogWarning((object) "hit right side of the box collider");
    return EHitOrientation.RightSideHit;
  }

  private static bool hitSpine(Vector3 localPoint, bool isSideHit, float spineZ)
  {
    if ((double) localPoint.z < -(double) spineZ || (double) localPoint.z > (double) spineZ || isSideHit)
      return false;
    if (PluginConfig.EnableBallisticsLogging.Value)
      Utils.Logger.LogWarning((object) "SPINE HIT");
    return true;
  }

  public static EBodyHitZone GetHitBodyZone(
    Vector3 localPoint,
    EHitOrientation hitOrientation,
    EBodyPartColliderType hitPart)
  {
    bool isSideHit = hitOrientation == EHitOrientation.LeftSideHit || hitOrientation == EHitOrientation.RightSideHit;
    if (hitPart == 1 || hitPart == 26 || hitPart == 22 || hitPart == 21)
    {
      float spineZ = 0.0125f;
      float num1 = 0.0374f;
      float num2 = -0.02f;
      float num3 = hitOrientation == EHitOrientation.BackHit ? -0.0435f : -0.063f;
      float num4 = hitOrientation == EHitOrientation.BackHit ? -0.028f : -0.05f;
      float num5 = hitOrientation == EHitOrientation.BackHit ? 0.11f : 0.115f;
      float num6 = hitOrientation == EHitOrientation.BackHit ? 0.11f : 0.115f;
      float num7 = 0.0465f;
      float num8 = 0.0465f;
      float num9 = -0.07f;
      float num10 = 0.04f;
      float num11 = 0.05f;
      float num12 = -0.2f;
      int num13;
      switch (hitOrientation)
      {
        case EHitOrientation.TopHit:
          num13 = 0;
          break;
        case EHitOrientation.BottomHit:
          return EBodyHitZone.CZone;
        default:
          num13 = hitOrientation != EHitOrientation.BottomHit ? 1 : 0;
          break;
      }
      if (num13 != 0)
      {
        if (isSideHit)
        {
          if (PluginConfig.EnableBallisticsLogging.Value)
            Utils.Logger.LogWarning((object) "SIDE HIT");
          return EBodyHitZone.DZone;
        }
        if (hitPart == 1 || hitPart == 21)
        {
          if (hitOrientation == EHitOrientation.BackHit && (double) localPoint.z > -(double) num11 && (double) localPoint.z < (double) num11 && (double) localPoint.x < (double) num12)
          {
            if (PluginConfig.EnableBallisticsLogging.Value)
              Utils.Logger.LogWarning((object) "NECK: BACK HIT (Counting As A-Zone)");
            return EBodyHitZone.AZone;
          }
          if ((double) localPoint.z <= (double) num1 && (double) localPoint.z >= (double) num2 && (double) localPoint.x >= (double) num3 && (double) localPoint.x <= (double) num4 && !isSideHit)
          {
            if (PluginConfig.EnableBallisticsLogging.Value)
              Utils.Logger.LogWarning((object) "HEART HIT");
            return EBodyHitZone.Heart;
          }
          if (HitZones.hitSpine(localPoint, isSideHit, spineZ))
            return EBodyHitZone.Spine;
          if ((double) localPoint.z < -(double) num5 || (double) localPoint.z > (double) num5)
          {
            if (PluginConfig.EnableBallisticsLogging.Value)
              Utils.Logger.LogWarning((object) "D-ZONE HIT: UPPER TORSO");
            return EBodyHitZone.DZone;
          }
          if ((double) localPoint.z > -(double) num7 && (double) localPoint.z < (double) num7)
          {
            if (PluginConfig.EnableBallisticsLogging.Value)
              Utils.Logger.LogWarning((object) "A-ZONE HIT: UPPER TORSO");
            return EBodyHitZone.AZone;
          }
          if (PluginConfig.EnableBallisticsLogging.Value)
            Utils.Logger.LogWarning((object) "C-ZONE HIT: UPPER TORSO");
          return EBodyHitZone.CZone;
        }
        if (HitZones.hitSpine(localPoint, isSideHit, spineZ))
          return EBodyHitZone.Spine;
        if (hitPart == 26 || hitPart == 22)
        {
          if ((double) localPoint.z < -(double) num6 || (double) localPoint.z > (double) num6)
          {
            if (PluginConfig.EnableBallisticsLogging.Value)
              Utils.Logger.LogWarning((object) "D-ZONE HIT: MID TORSO");
            return EBodyHitZone.DZone;
          }
          if ((double) localPoint.z > -(double) num8 && (double) localPoint.z < (double) num8 && (double) localPoint.x < (double) num9)
          {
            if (PluginConfig.EnableBallisticsLogging.Value)
              Utils.Logger.LogWarning((object) "A-ZONE HIT: MID TORSO");
            return EBodyHitZone.AZone;
          }
          if (PluginConfig.EnableBallisticsLogging.Value)
            Utils.Logger.LogWarning((object) "C-ZONE HIT: MID TORSO");
          return EBodyHitZone.CZone;
        }
        if (PluginConfig.EnableBallisticsLogging.Value)
          Utils.Logger.LogWarning((object) "COULDN'T FIND HIT ZONE");
        return EBodyHitZone.Unknown;
      }
      if (hitOrientation == EHitOrientation.TopHit && (hitPart == 1 || hitPart == 21))
      {
        if ((double) localPoint.z > -(double) spineZ && (double) localPoint.z < (double) spineZ)
        {
          if (PluginConfig.EnableBallisticsLogging.Value)
            Utils.Logger.LogWarning((object) "SPINE: TOP HIT");
          return EBodyHitZone.Spine;
        }
        if ((double) localPoint.z > -(double) num10 && (double) localPoint.z < (double) num10)
        {
          if (PluginConfig.EnableBallisticsLogging.Value)
            Utils.Logger.LogWarning((object) "NECK: TOP HIT (Counting As A-Zone)");
          return EBodyHitZone.AZone;
        }
        if (PluginConfig.EnableBallisticsLogging.Value)
          Utils.Logger.LogWarning((object) "D-ZONE: TOP SHOULDERS HIT");
        return EBodyHitZone.DZone;
      }
    }
    if (PluginConfig.EnableBallisticsLogging.Value)
      Utils.Logger.LogWarning((object) "COULDN'T FIND HIT ZONE");
    return EBodyHitZone.Unknown;
  }
}
