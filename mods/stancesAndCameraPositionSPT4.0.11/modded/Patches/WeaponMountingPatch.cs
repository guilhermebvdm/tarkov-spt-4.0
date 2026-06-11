using System;
using System.Reflection;
using EFT;
using EFT.Animations;
using EFT.Animations.NewRecoil;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace CameraRotationMod.Patches
{
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

            if (MountingManager.Instance == null)
                return;

            float swayMult = 1f;

            if (MountingManager.IsMounting)
            {
                swayMult = Plugin._MountingSwayMultiplier.Value;
                
                if (MountingManager.BracingDirection == EBracingDirection.Top)
                {
                    swayMult *= 0.75f;
                }
            }
            else if (MountingManager.IsBracing)
            {
                swayMult = Mathf.Lerp(1f, Plugin._MountingSwayMultiplier.Value, 0.5f);
                
                if (MountingManager.BracingDirection == EBracingDirection.Top)
                {
                    swayMult *= 0.85f;
                }
            }
            else
            {
                return;
            }

            if (__instance.Breath != null)
            {
                __instance.Breath.Intensity *= swayMult;
            }
        }
    }

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

            if (MountingManager.IsMounting)
            {
                recoilMult = Plugin._MountingRecoilMultiplier.Value;
                if (MountingManager.BracingDirection == EBracingDirection.Top)
                {
                    recoilMult *= 0.75f;
                }
            }
            else if (MountingManager.IsBracing)
            {
                recoilMult = Mathf.Lerp(1f, Plugin._MountingRecoilMultiplier.Value, 0.5f);
                if (MountingManager.BracingDirection == EBracingDirection.Top)
                {
                    recoilMult *= 0.85f;
                }
            }

            incomingForce *= recoilMult;
        }
    }

    public class MountingCollisionPatch : ModulePatch
    {
        private static FieldInfo _fcField;
        private static FieldInfo _blendField;
        private static FieldInfo _smoothInField;
        private static FieldInfo _smoothOutField;

        private static Vector3 _collisionPos = Vector3.zero;
        private static Vector3 _collisionRot = Vector3.zero;

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
            if (!Plugin._EnableWeaponMounting.Value) return;

            Player.FirearmController fc = (Player.FirearmController)_fcField.GetValue(__instance);
            if (fc == null || fc.Weapon == null) return;
            var player = Traverse.Create(fc).Field<Player>("_player").Value;
            if (player == null || !player.IsYourPlayer || player.MovementContext == null || player.MovementContext.CurrentState == null || player.MovementContext.CurrentState.Name == EPlayerState.Stationary) return;

            // Zera a colisão padrão (como feito no Realism)
            if (__instance.TurnAway != null)
            {
                _blendField.SetValue(__instance.TurnAway, 0.0f);
                _smoothInField.SetValue(__instance.TurnAway, 0.0f);
                _smoothOutField.SetValue(__instance.TurnAway, 0.0f);
            }

            if (__instance.HandsContainer == null || __instance.HandsContainer.WeaponRoot == null) return;

            // Calcula nova colisão baseada em Raycast local (direção frente da arma = Y negativo local)
            Vector3 direction = -__instance.HandsContainer.WeaponRoot.up; 
            // Movemos a origem 15cm para frente para evitar colidir com o próprio peito/câmera do jogador
            Vector3 position = __instance.HandsContainer.WeaponRoot.position + (direction * 0.15f);
            
            // Calculo simples de comprimento da arma baseado na célula do inventário
            float weaponLength = fc.Weapon.CalculateCellSize().X * 0.1f; 
            float detectLength = weaponLength * 1.25f;

            RaycastHit hit;
            bool isColliding = false;
            float lastDistance = 0f;

            // Ignorar triggers (zonas de extração invisíveis) que prendiam a arma!
            if (!MountingManager.IsMounting && Physics.Raycast(position, direction, out hit, detectLength, LayerMaskClass.HighPolyWithTerrainMask, QueryTriggerInteraction.Ignore))
            {
                // Compensamos os 15cm que avançamos
                lastDistance = hit.distance + 0.15f;
                isColliding = true;
                if (Time.frameCount % 60 == 0) 
                    Console.WriteLine($"[MountingCollision] Colliding with: {hit.collider.name} at distance: {lastDistance}");
            }

            Vector3 finalPos = new Vector3(0.0f, 0.05f, -0.15f);
            Vector3 finalRot = new Vector3(0.2f, -0.1f, -0.1f);
            
            // Adaptar para as stances do mod atual
            if (StanceManager.CurrentStance == Stance.Stance3) // ShortStock
            {
                finalPos = new Vector3(0f, 0f, -0.5f);
                finalRot = new Vector3(0.01f, 0.1f, -0.05f);
            }
            else if (StanceManager.CurrentStance == Stance.Stance1) // HighReady
            {
                finalPos = new Vector3(0.08f, -0.34f, -0.4f);
                finalRot = new Vector3(-0.25f, -0.05f, -0.025f);
            }
            else if (StanceManager.CurrentStance == Stance.Stance2) // LowReady
            {
                finalPos = new Vector3(0f, 0f, -0.15f);
                finalRot = new Vector3(0.15f, -0.4f, 0f);
            }

            float ratio = isColliding ? Mathf.Pow(Mathf.InverseLerp(weaponLength, 0f, lastDistance), 0.45f) : 0f;

            float yPush = (float)(weaponLength * -0.4 * (1.0 - lastDistance / detectLength));
            float zPush = (float)(weaponLength * -0.65 * (1.0 - lastDistance / detectLength));
            Vector3 linearPos = new Vector3(0.025f * ratio, yPush, zPush);

            Vector3 targetPos = isColliding ? Vector3.Lerp(linearPos, finalPos * ratio, 0.5f) : Vector3.zero;
            Vector3 targetRot = isColliding ? finalRot * ratio : Vector3.zero;

            float speed = 10f * Time.deltaTime;
            _collisionPos = Vector3.Lerp(_collisionPos, targetPos, speed);
            _collisionRot = Vector3.Lerp(_collisionRot, targetRot, speed);

            if (Time.frameCount % 60 == 0 && isColliding)
            {
                Console.WriteLine($"[MountingCollision] Before +=: {__instance.HandsContainer.WeaponRoot.localPosition}, _collisionPos: {_collisionPos}");
            }

            __instance.HandsContainer.WeaponRoot.localPosition += _collisionPos;
            
            if (Time.frameCount % 60 == 0 && isColliding)
            {
                Console.WriteLine($"[MountingCollision] After +=: {__instance.HandsContainer.WeaponRoot.localPosition}");
            }
            
            Quaternion rotDelta = Quaternion.identity;
            rotDelta.x = _collisionRot.x;
            rotDelta.y = _collisionRot.y;
            rotDelta.z = _collisionRot.z;
            
            // Normalizar quaternion após edição direta
            float norm = Mathf.Sqrt(rotDelta.x * rotDelta.x + rotDelta.y * rotDelta.y + rotDelta.z * rotDelta.z + rotDelta.w * rotDelta.w);
            rotDelta.x /= norm; 
            rotDelta.y /= norm; 
            rotDelta.z /= norm; 
            rotDelta.w /= norm;

            __instance.HandsContainer.WeaponRoot.localRotation *= rotDelta;
        }
    }
}
