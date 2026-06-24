// Decompiled with JetBrains decompiler
// Type: TarkovIRL.TIRLUtils
// Assembly: TarkovIRL, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C42939BD-7BF0-4586-ABE5-9D2EFC361A0B
// Assembly location: D:\Drive\Google Drive\Users\Erick Saraiva\Downloads\TarkovIRL_WeaponsHandlingMod_1.0.0\BepInEx\plugins\TarkovIRL.dll

using BepInEx.Logging;
using UnityEngine;

#nullable disable
namespace TarkovIRL;

public static class TIRLUtils
{
  public static ManualLogSource Logger;
  private static float _dt = 0.0f;
  private static float _spamTimer = 0.0f;
  private static float _spamTimeResolution = 2f;

  public static void LogError(string toPrint) => TIRLUtils.Logger.LogError((object) toPrint);

  public static void Log(string toPrint, bool spam)
  {
    if (!PrimeMover.IsLogging.Value)
      return;
    if (spam)
    {
      TIRLUtils.Logger.LogInfo((object) toPrint);
    }
    else
    {
      TIRLUtils._spamTimer += TIRLUtils._dt;
      if ((double) TIRLUtils._spamTimer > (double) PrimeMover.LoggingUpdateFrequency.Value)
      {
        TIRLUtils._spamTimer = 0.0f;
        TIRLUtils.Logger.LogInfo((object) toPrint);
      }
    }
  }

  public static void LogPriority(TIRLUtils.E_DEBUG_PRIORITY priority, string toPrint)
  {
    switch (priority)
    {
      case TIRLUtils.E_DEBUG_PRIORITY.LOG:
        if (!PrimeMover.IsLogging.Value || PrimeMover.DebugSpam.Value)
          break;
        TIRLUtils.Logger.LogError((object) toPrint);
        break;
      case TIRLUtils.E_DEBUG_PRIORITY.SPAM_LOG:
        if (!PrimeMover.IsLogging.Value || !PrimeMover.DebugSpam.Value)
          break;
        TIRLUtils.Logger.LogError((object) toPrint);
        break;
      case TIRLUtils.E_DEBUG_PRIORITY.ALWAYS_LOG:
        TIRLUtils.Logger.LogError((object) toPrint);
        break;
    }
  }

  public static void Update(float dt) => TIRLUtils._dt = dt;

  public static float FulfilledLerp(float value, float target, float step)
  {
    value += step;
    if ((double) value > (double) target)
      value = target;
    return value;
  }

  public static Quaternion GetQuatFromV3(Vector3 v)
  {
    Quaternion identity = Quaternion.identity;
    identity.x = v.x;
    identity.y = v.y;
    identity.z = v.z;
    return identity;
  }

  public enum E_DEBUG_PRIORITY
  {
    LOG,
    SPAM_LOG,
    ALWAYS_LOG,
  }
}
