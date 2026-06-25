using Comfort.Common;
using EFT;
using UnityEngine;

namespace hazelify.StanceSync
{
    public class Camera : MonoBehaviour
    {
        public Player localPlayer;
        private SharedGameSettingsClass _gameSettings;
        private Player.ItemHandsController _handsController;
        private Player.FirearmController _firearmController;

        public void Awake()
        {
            _gameSettings = Singleton<SharedGameSettingsClass>.Instance;
        }

        public void UpdateCamera()
        {
            if (localPlayer.CameraPosition == null || !localPlayer.HealthController.IsAlive)
                return;

            _handsController = localPlayer.HandsController as Player.ItemHandsController;
            _firearmController = localPlayer.HandsController as Player.FirearmController;

            // var isAiming = _handsController != null && _handsController.IsAiming;
            // possible we do not need this

            if (Plugin.enableSync.Value)
            {
                if (_firearmController != null)
                {
                    if (localPlayer.MovementContext.LeftStanceEnabled && localPlayer.MovementContext.Tilt == 0f)
                        _firearmController.ChangeLeftStance();
                }
            }
        }
    }
}
