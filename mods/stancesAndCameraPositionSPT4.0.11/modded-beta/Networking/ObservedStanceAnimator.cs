using EFT;
using EFT.Animations;
using Fika.Core.Main.Players;
using UnityEngine;
using CameraRotationMod.Patches;

namespace CameraRotationMod.Networking
{
    /// <summary>
    /// Item 014: state-holder por jogador OBSERVADO (Fika). Guarda o stance sincronizado + o spring state e
    /// aplica o offset no MESMO transform que o jogador local (HandsContainer.WeaponRootAnim), via ApplyTo,
    /// chamado pelo Postfix de ApplyComplexRotation. Antes aplicava errado no PlayerBones.Spine3 num
    /// LateUpdate — só girava o tronco; a arma ficava imóvel para os outros players.
    /// </summary>
    public class ObservedStanceAnimator : MonoBehaviour
    {
        private ObservedPlayer _observedPlayer;
        private int _stance;
        private bool _isAiming;
        private Vector3 _euler, _pos, _rotVel, _posVel;

        public void Init(ObservedPlayer p) => _observedPlayer = p;

        public void SetStance(int stance, bool isAiming)
        {
            // CR-01-01: sanear o dado de rede — stance fora de 0..3 (cliente bugado/versão divergente) → 0.
            _stance = (stance < 0 || stance > 3) ? 0 : stance;
            _isAiming = isAiming;
        }

        /// <summary>
        /// Aplica o offset de stance no WeaponRootAnim do observado — mesma fórmula do local
        /// (ApplyComplexRotationPatch.cs:280), aditivo sobre a pose nativa (lean/ombro/mira já em weapRotation).
        /// </summary>
        public void ApplyTo(ProceduralWeaponAnimation pwa, Vector3 weaponPosition, Quaternion weapRotation, float dt)
        {
            if (pwa == null || pwa.HandsContainer == null || pwa.HandsContainer.WeaponRootAnim == null) return;

            bool inStance = _stance > 0 && !(_observedPlayer != null && _observedPlayer.IsInPronePose);
            Vector3 targetEuler = inStance ? StanceManager.GetTargetRotation((Stance)_stance, _isAiming) : Vector3.zero;
            Vector3 targetPos = inStance ? StanceManager.GetTargetPosition((Stance)_stance, _isAiming) : Vector3.zero;

            float speedMult = Plugin._StanceTransitionSpeed?.Value ?? 1f;
            float stiffness = 150f * speedMult;
            float damping = Plugin._StanceOvershootDamping?.Value ?? 12f;
            _euler = ApplyComplexRotationPatch.SpringLerpAngle(_euler, targetEuler, ref _rotVel, stiffness, damping, dt);
            _pos = ApplyComplexRotationPatch.SpringLerp(_pos, targetPos, ref _posVel, stiffness, damping, dt);

            Vector3 oriented = weapRotation * _pos;
            pwa.HandsContainer.WeaponRootAnim.SetPositionAndRotation(
                weaponPosition + oriented, weapRotation * Quaternion.Euler(_euler));
        }
    }
}
