using EFT;
using EFT.Animations;
using Fika.Core.Main.Players;
using UnityEngine;
using CameraRotationMod.Patches;

namespace CameraRotationMod.Networking
{
    /// <summary>
    /// Item 014: state-holder por jogador OBSERVADO (Fika). Guarda o stance sincronizado + o spring state e
    /// aplica o offset no WeaponRootAnim do observado, de forma ADITIVA, a partir do Postfix de
    /// ProcessEffectors (ObservedStanceProcessPatch) — que roda DEPOIS de todo o processamento da PWA e
    /// ANTES de o ObservedPlayer copiar WeaponRootAnim.localPosition/localRotation -> PlayerBones.Offset/
    /// DeltaRotation (ObservedPlayer.cs:1852-1853) -> ShiftWeaponRoot (1876), que renderiza a arma em 3a pessoa.
    /// (fix-01: antes aplicava no Spine3 (só torso) e depois via Postfix de ApplyComplexRotation, que era
    /// sobrescrito por etapas posteriores do ProcessEffectors — por isso a arma não acompanhava.)
    /// </summary>
    public class ObservedStanceAnimator : MonoBehaviour
    {
        private ObservedPlayer _observedPlayer;
        private int _stance;
        private bool _isAiming;
        private Vector3 _euler, _pos, _rotVel, _posVel;
        private static bool _loggedOnce;

        public void Init(ObservedPlayer p) => _observedPlayer = p;

        public void SetStance(int stance, bool isAiming)
        {
            // CR-01-01: sanear o dado de rede — stance fora de 0..3 (cliente bugado/versão divergente) → 0.
            _stance = (stance < 0 || stance > 3) ? 0 : stance;
            _isAiming = isAiming;
        }

        /// <summary>
        /// Aplica o offset de stance ADITIVO sobre a pose já calculada no WeaponRootAnim do observado.
        /// Modifica localPosition/localRotation — que é o que o ObservedPlayer copia para PlayerBones.
        /// </summary>
        public void ApplyToObserved(ProceduralWeaponAnimation pwa)
        {
            Transform wra = pwa == null || pwa.HandsContainer == null ? null : pwa.HandsContainer.WeaponRootAnim;
            if (wra == null) return;

            bool inStance = _stance > 0 && !(_observedPlayer != null && _observedPlayer.IsInPronePose);
            Vector3 targetEuler = inStance ? StanceManager.GetTargetRotation((Stance)_stance, _isAiming) : Vector3.zero;
            Vector3 targetPos = inStance ? StanceManager.GetTargetPosition((Stance)_stance, _isAiming) : Vector3.zero;

            float dt = Time.deltaTime;
            float speedMult = Plugin._StanceTransitionSpeed?.Value ?? 1f;
            float stiffness = 150f * speedMult;
            float damping = Plugin._StanceOvershootDamping?.Value ?? 12f;
            _euler = ApplyComplexRotationPatch.SpringLerpAngle(_euler, targetEuler, ref _rotVel, stiffness, damping, dt);
            _pos = ApplyComplexRotationPatch.SpringLerp(_pos, targetPos, ref _posVel, stiffness, damping, dt);

            // Aditivo sobre a pose nativa (que já inclui lean/ombro/mira). Posição orientada pela rotação atual.
            wra.localPosition = wra.localPosition + wra.localRotation * _pos;
            wra.localRotation = wra.localRotation * Quaternion.Euler(_euler);

            if (!_loggedOnce)
            {
                _loggedOnce = true;
                Plugin.Logger.LogInfo($"[ObservedStance] fix-01 ATIVO — aplicando stance {_stance} no WeaponRootAnim do observado.");
            }
        }
    }
}
