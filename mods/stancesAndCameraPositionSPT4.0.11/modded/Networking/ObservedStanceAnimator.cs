using EFT;
using Fika.Core.Main.Players;
using UnityEngine;
using CameraRotationMod.Patches;

namespace CameraRotationMod.Networking
{
    /// <summary>
    /// Item 014: state-holder por jogador OBSERVADO (Fika). Guarda o stance sincronizado + o spring state e
    /// aplica o offset de stance ADITIVO no <c>PlayerBones.Weapon_Root_Anim</c>, chamado por
    /// <c>ObservedStanceShiftPatch</c> (Postfix de ShiftWeaponRoot, janela PRÉ-IK). Como os markers de IK da
    /// arma são filhos do Weapon_Root_Anim, mover o root aqui faz a IK das mãos levar o BRAÇO à stance e o
    /// Kinematics colar a ARMA na mão — braço e arma acompanham juntos. Coexiste com lean/ombro/mira (o
    /// ShiftWeaponRoot já pôs a pose nativa no transform antes; aqui só somamos o stance).
    /// </summary>
    public class ObservedStanceAnimator : MonoBehaviour
    {
        private ObservedPlayer _observedPlayer;
        private int _stance;
        private bool _isAiming;
        private Vector3 _euler, _pos, _rotVel, _posVel;
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

            bool inStance = _stance > 0 && !(_observedPlayer != null && _observedPlayer.IsInPronePose);
            Vector3 targetEuler = inStance ? StanceManager.GetTargetRotation((Stance)_stance, _isAiming) : Vector3.zero;
            Vector3 targetPos = inStance ? StanceManager.GetTargetPosition((Stance)_stance, _isAiming) : Vector3.zero;

            float dt = Time.deltaTime;
            float speedMult = Plugin._StanceTransitionSpeed?.Value ?? 1f;
            float stiffness = 150f * speedMult;
            float damping = Plugin._StanceOvershootDamping?.Value ?? 12f;
            _euler = ApplyComplexRotationPatch.SpringLerpAngle(_euler, targetEuler, ref _rotVel, stiffness, damping, dt);
            _pos = ApplyComplexRotationPatch.SpringLerp(_pos, targetPos, ref _posVel, stiffness, damping, dt);

            // Janela PRÉ-IK (Postfix de ShiftWeaponRoot): mover o Weapon_Root_Anim desloca os markers de IK da
            // arma (filhos dele) → a LimbIK leva o braço até a stance e o Kinematics cola a arma na mão. O
            // ShiftWeaponRoot re-seta o transform (valor absoluto) todo frame antes daqui, então SOMAR não acumula.
            wra.localRotation = wra.localRotation * Quaternion.Euler(_euler);
            wra.localPosition = wra.localPosition + _pos;

            if (!_loggedApply && _stance > 0)
            {
                _loggedApply = true;
                Plugin.Logger.LogInfo($"[StanceSync-014] aplicando stance={_stance} euler={targetEuler} no Weapon_Root_Anim (pré-IK).");
            }
        }
    }
}
