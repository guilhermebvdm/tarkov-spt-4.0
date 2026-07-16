using CameraRotationMod;
using Comfort.Common;
using EFT;
using UnityEngine;

#nullable disable
namespace TarkovIRL;

public static class StanceController
{
  public static EStance CurrentStance
  {
    get
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

