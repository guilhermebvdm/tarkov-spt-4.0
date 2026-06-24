using EFT;
using Comfort.Common;
using System.Reflection;

namespace TarkovIRL
{
    public enum EStance
    {
        None,
        HighReady,
        LowReady,
        ShortStock,
        ActiveAiming,
        Patrol
    }

    public static class StanceController
    {
        public static EStance CurrentStance
        {
            get
            {
                var valString = CameraRotationMod.StanceManager.CurrentStance.ToString();
                if (valString == "Stance1") return EStance.HighReady;
                if (valString == "Stance2") return EStance.LowReady;
                if (valString == "Stance3") return EStance.ShortStock;
                return EStance.None;
            }
        }

        public static bool IsLeftShoulder
        {
            get
            {
                if (Singleton<GameWorld>.Instantiated && Singleton<GameWorld>.Instance.MainPlayer != null)
                {
                    // Update to EFT 0.16.9 LeftStance property
                    return Singleton<GameWorld>.Instance.MainPlayer.MovementContext.LeftStanceEnabled;
                }
                return false;
            }
        }

        public static bool IsMounting
        {
            get
            {
                // In EFT 0.14+ we can check mounting state, but for now we default to false to avoid crash.
                return false; 
            }
        }
    }
}
