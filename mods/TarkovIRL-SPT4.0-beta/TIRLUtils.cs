using BepInEx.Logging;
using UnityEngine;

#nullable disable
namespace TarkovIRL;

public static class TIRLUtils
{
  public static ManualLogSource Logger;
  private static float _dt;
  private static float _spamTimer;

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

  public static Quaternion GetQuatFromV3(Vector3 v) => Quaternion.Euler(v.x, v.y, v.z);

  public enum E_DEBUG_PRIORITY
  {
    LOG,
    SPAM_LOG,
    ALWAYS_LOG,
  }
}

