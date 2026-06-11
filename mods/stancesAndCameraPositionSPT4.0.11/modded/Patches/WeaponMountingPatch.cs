using System.Reflection;
using EFT;
using EFT.Animations;
using EFT.Animations.NewRecoil;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace CameraRotationMod.Patches
{
    // Detecção de superfície unificada (passivo E ativo): Prefix de LEITURA em
    // Player.FirearmController.method_11 — mesmo hook do RealismMod CollisionPatch.cs:209.
    // Não altera o resultado; apenas alimenta MountingManager.DetectBracing com o `ln` (comprimento
    // real da arma) que o EFT já calcula. Substitui o antigo DetectBracing no Update do MonoBehaviour.
    public class FirearmCollisionDetectPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player.FirearmController), "method_11");
        }

        [PatchPrefix]
        private static void Prefix(Player.FirearmController __instance, float ln)
        {
            if (!Plugin._EnableWeaponMounting.Value) return;
            if (MountingManager.Instance == null) return;

            var player = Traverse.Create(__instance).Field<Player>("_player").Value;
            if (player == null || !player.IsYourPlayer) return;

            MountingManager.DetectBracing(__instance, player, ln);
        }
    }

    // Sway (respiração): reduzido no passivo (parcial) e no ativo (full).
    public class WeaponMountingPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ProceduralWeaponAnimation), nameof(ProceduralWeaponAnimation.ProcessEffectors));
        }

        [PatchPostfix]
        private static void PatchPostfix(ProceduralWeaponAnimation __instance)
        {
            if (!Plugin._EnableWeaponMounting.Value) return;
            if (MountingManager.Instance == null) return;

            float swayMult = 1f;

            if (MountingManager.IsMounting) // Active
            {
                swayMult = Plugin._MountingSwayMultiplier.Value;
                if (MountingManager.BracingDirection == EBracingDirection.Top)
                    swayMult *= 0.75f;
            }
            else if (MountingManager.IsBracing) // Passive
            {
                swayMult = Mathf.Lerp(1f, Plugin._MountingSwayMultiplier.Value, 0.5f);
                if (MountingManager.BracingDirection == EBracingDirection.Top)
                    swayMult *= 0.85f;
            }
            else
            {
                return;
            }

            if (__instance.Breath != null)
                __instance.Breath.Intensity *= swayMult;
        }
    }

    // Recoil: reduzido no passivo (parcial) e no ativo (full).
    public class AddRecoilForceMountPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(NewRecoilShotEffect), "AddRecoilForce");
        }

        [PatchPrefix]
        private static void Prefix(NewRecoilShotEffect __instance, ref float incomingForce)
        {
            if (!Plugin._EnableWeaponMounting.Value) return;
            if (MountingManager.Instance == null) return;

            float recoilMult = 1f;

            if (MountingManager.IsMounting) // Active
            {
                recoilMult = Plugin._MountingRecoilMultiplier.Value;
                if (MountingManager.BracingDirection == EBracingDirection.Top)
                    recoilMult *= 0.75f;
            }
            else if (MountingManager.IsBracing) // Passive
            {
                recoilMult = Mathf.Lerp(1f, Plugin._MountingRecoilMultiplier.Value, 0.5f);
                if (MountingManager.BracingDirection == EBracingDirection.Top)
                    recoilMult *= 0.85f;
            }

            incomingForce *= recoilMult;
        }
    }

    // "Grude": desloca o WeaponRoot para encostar na superfície. SÓ no mount ATIVO (06-fix-01 — antes
    // rodava no passivo, causando o desalinhamento da Stance 0 ao apenas encostar a arma). Ao sair do
    // ativo, ResetCollisionOffsets() zera o deslocamento e o TurnAway vanilla é restaurado.
    public class MountingCollisionPatch : ModulePatch
    {
        private static FieldInfo _fcField;
        private static FieldInfo _blendField;
        private static FieldInfo _smoothInField;
        private static FieldInfo _smoothOutField;

        private static Vector3 _collisionPos = Vector3.zero;
        private static Vector3 _collisionRot = Vector3.zero;

        // Cache dos valores originais do TurnAwayEffector (anti-colisão vanilla), para restaurar fora
        // do mount ativo — antes ficavam permanentemente zerados, causando clipping de arma em parede.
        private static bool _turnAwayCached = false;
        private static float _origBlend, _origInSmooth, _origOutSmooth;

        public static void ResetCollisionOffsets()
        {
            _collisionPos = Vector3.zero;
            _collisionRot = Vector3.zero;
        }

        protected override MethodBase GetTargetMethod()
        {
            _fcField = AccessTools.Field(typeof(ProceduralWeaponAnimation), "_firearmController");
            _blendField = AccessTools.Field(typeof(TurnAwayEffector), "_blendSpeed");
            _smoothInField = AccessTools.Field(typeof(TurnAwayEffector), "_inSmoothTime");
            _smoothOutField = AccessTools.Field(typeof(TurnAwayEffector), "_outSmoothTime");
            return AccessTools.Method(typeof(ProceduralWeaponAnimation), nameof(ProceduralWeaponAnimation.AvoidObstacles));
        }

        [PatchPostfix]
        private static void Postfix(ProceduralWeaponAnimation __instance)
        {
            if (!Plugin._EnableWeaponMounting.Value) { RestoreTurnAway(__instance); return; }

            Player.FirearmController fc = (Player.FirearmController)_fcField.GetValue(__instance);
            if (fc == null || fc.Weapon == null) return;
            var player = Traverse.Create(fc).Field<Player>("_player").Value;
            if (player == null || !player.IsYourPlayer || player.MovementContext == null
                || player.MovementContext.CurrentState == null
                || player.MovementContext.CurrentState.Name == EPlayerState.Stationary) return;

            bool active = MountingManager.IsMounting; // Active

            if (!active)
            {
                // Passivo/None: SEM grude. Restaura o anti-colisão vanilla e relaxa os offsets a zero.
                RestoreTurnAway(__instance);
                float relax = 10f * Time.deltaTime;
                _collisionPos = Vector3.Lerp(_collisionPos, Vector3.zero, relax);
                _collisionRot = Vector3.Lerp(_collisionRot, Vector3.zero, relax);
                ApplyOffsets(__instance);
                return;
            }

            // ATIVO: aplica o "grude". Desativa o TurnAway vanilla enquanto montado.
            DisableTurnAway(__instance);

            if (__instance.HandsContainer == null || __instance.HandsContainer.WeaponRoot == null) return;

            Vector3 direction = -__instance.HandsContainer.WeaponRoot.up;
            Vector3 position = __instance.HandsContainer.WeaponRoot.position + (direction * 0.15f);
            float weaponLength = fc.Weapon.CalculateCellSize().X * 0.1f;
            float detectLength = weaponLength * 1.25f;

            bool isColliding = false;
            float lastDistance = 0f;
            if (Physics.Raycast(position, direction, out RaycastHit hit, detectLength,
                    LayerMaskClass.HighPolyWithTerrainMask, QueryTriggerInteraction.Ignore))
            {
                lastDistance = hit.distance + 0.15f;
                isColliding = true;
            }

            // No ativo a stance é forçada para Default; o offset-alvo é o de Stance 0.
            Vector3 finalPos = new Vector3(0.0f, 0.05f, -0.15f);
            Vector3 finalRot = new Vector3(0.2f, -0.1f, -0.1f);

            float ratio = isColliding ? Mathf.Pow(Mathf.InverseLerp(weaponLength, 0f, lastDistance), 0.45f) : 0f;
            float yPush = (float)(weaponLength * -0.4 * (1.0 - lastDistance / detectLength));
            float zPush = (float)(weaponLength * -0.65 * (1.0 - lastDistance / detectLength));
            Vector3 linearPos = new Vector3(0.025f * ratio, yPush, zPush);

            Vector3 targetPos = isColliding ? Vector3.Lerp(linearPos, finalPos * ratio, 0.5f) : Vector3.zero;
            Vector3 targetRot = isColliding ? finalRot * ratio : Vector3.zero;

            float speed = 10f * Time.deltaTime;
            _collisionPos = Vector3.Lerp(_collisionPos, targetPos, speed);
            _collisionRot = Vector3.Lerp(_collisionRot, targetRot, speed);

            ApplyOffsets(__instance);
        }

        private static void ApplyOffsets(ProceduralWeaponAnimation pwa)
        {
            if (pwa.HandsContainer == null || pwa.HandsContainer.WeaponRoot == null) return;

            pwa.HandsContainer.WeaponRoot.localPosition += _collisionPos;

            Quaternion rotDelta = Quaternion.identity;
            rotDelta.x = _collisionRot.x;
            rotDelta.y = _collisionRot.y;
            rotDelta.z = _collisionRot.z;
            float norm = Mathf.Sqrt(rotDelta.x * rotDelta.x + rotDelta.y * rotDelta.y + rotDelta.z * rotDelta.z + rotDelta.w * rotDelta.w);
            if (norm > 0.0001f)
            {
                rotDelta.x /= norm; rotDelta.y /= norm; rotDelta.z /= norm; rotDelta.w /= norm;
                pwa.HandsContainer.WeaponRoot.localRotation *= rotDelta;
            }
        }

        private static void DisableTurnAway(ProceduralWeaponAnimation pwa)
        {
            if (pwa.TurnAway == null) return;
            if (!_turnAwayCached)
            {
                _origBlend = (float)_blendField.GetValue(pwa.TurnAway);
                _origInSmooth = (float)_smoothInField.GetValue(pwa.TurnAway);
                _origOutSmooth = (float)_smoothOutField.GetValue(pwa.TurnAway);
                _turnAwayCached = true;
            }
            _blendField.SetValue(pwa.TurnAway, 0.0f);
            _smoothInField.SetValue(pwa.TurnAway, 0.0f);
            _smoothOutField.SetValue(pwa.TurnAway, 0.0f);
        }

        private static void RestoreTurnAway(ProceduralWeaponAnimation pwa)
        {
            if (pwa.TurnAway == null || !_turnAwayCached) return;
            _blendField.SetValue(pwa.TurnAway, _origBlend);
            _smoothInField.SetValue(pwa.TurnAway, _origInSmooth);
            _smoothOutField.SetValue(pwa.TurnAway, _origOutSmooth);
        }
    }
}
