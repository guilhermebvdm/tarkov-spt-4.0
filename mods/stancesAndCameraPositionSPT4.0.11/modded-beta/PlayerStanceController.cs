using UnityEngine;
using EFT;
using EFT.Animations;
using CameraRotationMod.Patches;

namespace CameraRotationMod
{
    public class PlayerStanceController : MonoBehaviour
    {
        private Player _player;
        public Stance CurrentStance { get; private set; } = Stance.Default;
        public bool IsMounting { get; private set; } = false;

        private Spring _handsRotationSpring;
        private Spring _handsPositionSpring;

        private Vector3 _currentPosOffset;
        private Vector3 _currentRotOffset;

        private float _smoothTimeRot = 0.2f;
        private float _smoothTimePos = 0.2f;

        private Vector3 _rotVelocity;
        private Vector3 _posVelocity;

        public void Init(Player player)
        {
            _player = player;
        }

        public bool IsRotationSpring(Spring spring)
        {
            return spring == _handsRotationSpring;
        }

        public void UpdateStanceFromNetwork(Stance stance, bool isMounting)
        {
            CurrentStance = stance;
            IsMounting = isMounting;
        }

        private void Update()
        {
            if (_player == null || _player.ProceduralWeaponAnimation == null || _player.ProceduralWeaponAnimation.HandsContainer == null)
                return;

            var pwa = _player.ProceduralWeaponAnimation;
            
            // Removed SpringGetPatch references
            // Calcula os offsets alvo baseados na stance atual
            Vector3 targetRot = Vector3.zero;
            Vector3 targetPos = Vector3.zero;
            float transitionSpeed = 4f;

            if (IsMounting)
            {
                // Simple mounting visuals
                targetPos = new Vector3(0, -0.05f, 0.05f); 
                transitionSpeed = 5f;
            }
            else
            {
                switch (CurrentStance)
                {
                    case Stance.Stance1:
                        targetRot = new Vector3(Plugin._Stance1HandsPitchRotation.Value, 0, Plugin._Stance1HandsRollRotation.Value);
                        targetPos = new Vector3(Plugin._Stance1HandsSidewaysOffset.Value, Plugin._Stance1HandsUpDownOffset.Value, Plugin._Stance1HandsForwardBackwardOffset.Value);
                        transitionSpeed = Plugin._StanceTransitionSpeed.Value;
                        break;
                    case Stance.Stance2:
                        targetRot = new Vector3(Plugin._Stance2HandsPitchRotation.Value, 0, Plugin._Stance2HandsRollRotation.Value);
                        targetPos = new Vector3(Plugin._Stance2HandsSidewaysOffset.Value, Plugin._Stance2HandsUpDownOffset.Value, Plugin._Stance2HandsForwardBackwardOffset.Value);
                        transitionSpeed = Plugin._StanceTransitionSpeed.Value;
                        break;
                    case Stance.Stance3:
                        targetRot = new Vector3(Plugin._Stance3HandsPitchRotation.Value, 0, Plugin._Stance3HandsRollRotation.Value);
                        targetPos = new Vector3(Plugin._Stance3HandsSidewaysOffset.Value, Plugin._Stance3HandsUpDownOffset.Value, Plugin._Stance3HandsForwardBackwardOffset.Value);
                        transitionSpeed = Plugin._StanceTransitionSpeed.Value;
                        break;
                }
            }

            _smoothTimeRot = SpeedToSmoothTime(transitionSpeed);
            _smoothTimePos = SpeedToSmoothTime(transitionSpeed);

            _currentRotOffset = Vector3.SmoothDamp(_currentRotOffset, targetRot, ref _rotVelocity, _smoothTimeRot);
            _currentPosOffset = Vector3.SmoothDamp(_currentPosOffset, targetPos, ref _posVelocity, _smoothTimePos);
        }

        private void OnDestroy()
        {
        }

        public Vector3 GetRotationOffset() => _currentRotOffset;
        public Vector3 GetPositionOffset() => _currentPosOffset;

        private static float SpeedToSmoothTime(float speed)
        {
            speed = Mathf.Clamp(speed, 0.5f, 20f);
            return 0.25f / speed;
        }
    }
}
