using System;
using EFT;
using Fika.Core.Main.Players;
using UnityEngine;

namespace CameraRotationMod.Networking
{
    public class ObservedStanceAnimator : MonoBehaviour
    {
        private ObservedPlayer _observedPlayer;
        private int _currentStance = 0;
        private bool _isAiming = false;

        private Vector3 _currentEuler = Vector3.zero;
        private Vector3 _rotVelocity = Vector3.zero;

        public void Init(ObservedPlayer player)
        {
            _observedPlayer = player;
        }

        public void SetStance(int stance, bool isAiming)
        {
            _currentStance = stance;
            _isAiming = isAiming;
        }

        private void LateUpdate()
        {
            if (_observedPlayer == null || _observedPlayer.PlayerBones == null)
                return;

            if (_observedPlayer.PlayerBones.Spine3 == null || _observedPlayer.PlayerBones.Spine3.Original == null)
                return;

            // Não aplica stance se o jogador observado estiver deitado
            if (_observedPlayer.IsInPronePose)
                return;

            // Encontra a rotação alvo (mesmos graus do StanceManager)
            Vector3 targetEuler = Vector3.zero;
            bool isInStance = _currentStance > 0;
            
            if (isInStance)
            {
                targetEuler = GetTargetRotationForStance(_currentStance, _isAiming);
            }

            // Suaviza a rotação (Mathf.SmoothDampAngle)
            float dt = Time.deltaTime;
            _currentEuler.x = Mathf.SmoothDampAngle(_currentEuler.x, targetEuler.x, ref _rotVelocity.x, 0.1f, float.PositiveInfinity, dt);
            _currentEuler.y = Mathf.SmoothDampAngle(_currentEuler.y, targetEuler.y, ref _rotVelocity.y, 0.1f, float.PositiveInfinity, dt);
            _currentEuler.z = Mathf.SmoothDampAngle(_currentEuler.z, targetEuler.z, ref _rotVelocity.z, 0.1f, float.PositiveInfinity, dt);

            // Aplica a rotação adicional no Spine3 (Torso superior)
            // Isso vai inclinar o braço inteiro dele e a arma
            _observedPlayer.PlayerBones.Spine3.Original.localRotation *= Quaternion.Euler(_currentEuler);
        }

        private Vector3 GetTargetRotationForStance(int stance, bool isAiming)
        {
            if (stance == 1) // LowReady (Stance1)
            {
                return new Vector3(
                    Plugin._Stance1HandsPitchRotation?.Value ?? 0f,
                    Plugin._Stance1HandsYawRotation?.Value ?? 0f,
                    Plugin._Stance1HandsRollRotation?.Value ?? 0f
                );
            }
            if (stance == 2) // HighReady (Stance2)
            {
                return new Vector3(
                    Plugin._Stance2HandsPitchRotation?.Value ?? 0f,
                    Plugin._Stance2HandsYawRotation?.Value ?? 0f,
                    Plugin._Stance2HandsRollRotation?.Value ?? 0f
                );
            }
            if (stance == 3) // Patrol (Stance3)
            {
                return new Vector3(
                    Plugin._Stance3HandsPitchRotation?.Value ?? 0f,
                    Plugin._Stance3HandsYawRotation?.Value ?? 0f,
                    Plugin._Stance3HandsRollRotation?.Value ?? 0f
                );
            }
            
            return Vector3.zero;
        }
    }
}
