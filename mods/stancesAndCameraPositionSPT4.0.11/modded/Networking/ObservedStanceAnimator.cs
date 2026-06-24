using EFT;
using Fika.Core.Main.Players;
using UnityEngine;
using CameraRotationMod.Patches;

namespace CameraRotationMod.Networking
{
    /// <summary>
    /// Item 014: state-holder por jogador OBSERVADO (Fika). Guarda o stance sincronizado + o spring state e
    /// aplica o offset de stance ADITIVO em PlayerBones.DeltaRotation/Offset (via ObservedStanceShiftPatch,
    /// Prefix de ShiftWeaponRoot). O ObservedPlayer reseta Offset/DeltaRotation com a pose nativa a cada frame
    /// ANTES do ShiftWeaponRoot (ObservedPlayer.cs:1852-1853), então aqui apenas SOMAMOS o stance — coexiste
    /// com lean/ombro/mira. O ShiftWeaponRoot usa Weapon_Root_Third.rotation * DeltaRotation → a arma de 3a
    /// pessoa reflete o stance; as mãos/braço seguem a arma por IK.
    /// </summary>
    public class ObservedStanceAnimator : MonoBehaviour
    {
        private ObservedPlayer _observedPlayer;
        private int _stance;
        private bool _isAiming;
        private Vector3 _euler, _pos, _rotVel, _posVel;
        // CR-02-01: guarda o último resultado escrito para detectar frames em que o vanilla NÃO re-setou
        // o Weapon_Root_Anim (early-return de ObservedVisualPass) e evitar acúmulo do offset aditivo.
        private Quaternion _lastWrittenRot;
        private Vector3 _lastWrittenPos;
        private bool _hasWritten;
        private static bool _loggedApply;

        public void Init(ObservedPlayer p) => _observedPlayer = p;

        public void SetStance(int stance, bool isAiming)
        {
            // CR-01-01: sanear o dado de rede — stance fora de 0..3 (cliente bugado/versão divergente) → 0.
            _stance = (stance < 0 || stance > 3) ? 0 : stance;
            _isAiming = isAiming;
        }

        public void ApplyToWeaponRoot(PlayerBones bones)
        {
            Transform wra = bones == null ? null : bones.Weapon_Root_Anim;
            if (wra == null) return;

            // CR-02-01: ObservedVisualPass tem early-return (ObservedPlayer.cs:1841: CustomAnimationsAreProcessing
            // || !IsVisible || !IsAlive) que pula ShiftWeaponRoot/Kinematics. Nesses frames o Weapon_Root_Anim NÃO
            // é re-setado para a pose base — e como somamos um offset ADITIVO, repeti-lo acumularia (arma girando).
            // Detecção robusta (sem depender de acessores privados do Fika): se o transform atual é idêntico ao que
            // NÓS escrevemos por último, o vanilla não o tocou neste frame → pulamos. Como offset != 0 garante
            // base != base+offset, isto só pula de fato em frame "sujo" (não-resetado).
            if (_hasWritten && wra.localRotation == _lastWrittenRot && wra.localPosition == _lastWrittenPos)
                return;

            bool inStance = _stance > 0 && !(_observedPlayer != null && _observedPlayer.IsInPronePose);
            Vector3 targetEuler = inStance ? StanceManager.GetTargetRotation((Stance)_stance, _isAiming) : Vector3.zero;
            Vector3 targetPos = inStance ? StanceManager.GetTargetPosition((Stance)_stance, _isAiming) : Vector3.zero;

            float dt = Time.deltaTime;
            float speedMult = Plugin._StanceTransitionSpeed?.Value ?? 1f;
            float stiffness = 150f * speedMult;
            float damping = Plugin._StanceOvershootDamping?.Value ?? 12f;
            _euler = ApplyComplexRotationPatch.SpringLerpAngle(_euler, targetEuler, ref _rotVel, stiffness, damping, dt);
            _pos = ApplyComplexRotationPatch.SpringLerp(_pos, targetPos, ref _posVel, stiffness, damping, dt);

            // Transform FINAL da arma de 3a pessoa, após ShiftWeaponRoot + Kinematics + IK (nada sobrescreve depois).
            wra.localRotation = wra.localRotation * Quaternion.Euler(_euler);
            wra.localPosition = wra.localPosition + _pos;

            // CR-02-01: registra o resultado para, no próximo frame, distinguir transform re-setado vs. congelado.
            _lastWrittenRot = wra.localRotation;
            _lastWrittenPos = wra.localPosition;
            _hasWritten = true;

            if (!_loggedApply && _stance > 0)
            {
                _loggedApply = true;
                Plugin.Logger.LogInfo($"[StanceSync-014] aplicando stance={_stance} euler={targetEuler} no Weapon_Root_Anim do observado.");
            }
        }
    }
}
