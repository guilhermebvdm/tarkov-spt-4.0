using CameraRotationMod;
using Comfort.Common;
using EFT;
using UnityEngine;

#nullable disable
namespace TarkovIRL;

public static class StanceController
{
  private static bool? _isStanceModLoaded;

  public static bool IsStanceModLoaded
  {
    get
    {
      if (!_isStanceModLoaded.HasValue)
      {
        try
        {
          System.Type type = System.Type.GetType("CameraRotationMod.StanceManager, shwngFpsCameraStances4");
          _isStanceModLoaded = type != null;
        }
        catch
        {
          _isStanceModLoaded = false;
        }
      }
      return _isStanceModLoaded.Value;
    }
  }

  public static EStance CurrentStance
  {
    get
    {
      if (!IsStanceModLoaded)
        return EStance.None;
      return GetCurrentStanceInternal();
    }
  }

  [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
  private static EStance GetCurrentStanceInternal()
  {
    switch (StanceManager.CurrentStance.ToString())
    {
      case "Stance1":
        return EStance.HighReady;
      case "Stance2":
        return EStance.LowReady;
      case "Stance3":
        return EStance.ShortStock;
      default:
        return EStance.None;
    }
  }

  public static bool IsLeftShoulder
  {
    get
    {
      return Singleton<GameWorld>.Instantiated && (Singleton<GameWorld>.Instance.MainPlayer != null) && Singleton<GameWorld>.Instance.MainPlayer.MovementContext.LeftStanceEnabled;
    }
  }

  public static bool IsMounting => false;
}

