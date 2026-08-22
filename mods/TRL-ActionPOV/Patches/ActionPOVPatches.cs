using System.Reflection;
using ActionPOV.Core;
using EFT;
using EFT.Animations;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

#nullable disable
namespace ActionPOV.Patches
{
    // 1. Interceptação e Divisão Proporcional de Input
    public class Patch_PlayerRotate : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), nameof(Player.Rotate));
        }

        [PatchPrefix]
        private static bool Prefix(Player __instance, ref Vector2 deltaRotation, bool ignoreClamp)
        {
            if (__instance == null || !__instance.IsYourPlayer || !Plugin.EnableMod.Value)
                return true;

            // Guards de Estado (Sprint, Inventário, Cura/Itens)
            // Em vez de um Reset() abrupto que teletransporta a arma, direciona suavemente o alvo para zero
            if (__instance.MovementContext.CurrentState.Name == EPlayerState.Stationary ||
                __instance.MovementContext.IsSprintEnabled ||
                __instance.HandsController is Player.UsableItemController)
            {
                KineticSpringEngine.TargetWeaponAngle = Vector3.zero;
                return true;
            }

            bool isAiming = __instance.HandsController != null && __instance.HandsController.IsAiming;
            KineticSpringEngine.ProcessMouseInput(ref deltaRotation, isAiming, __instance);
            return true;
        }
    }

    // 2. Cinética da Visão / Roll Orgânico da Cabeça e Tranco de Disparo Bodycam
    public class Patch_SetHeadRotation : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ProceduralWeaponAnimation), nameof(ProceduralWeaponAnimation.SetHeadRotation));
        }

        [PatchPrefix]
        private static bool Prefix(ProceduralWeaponAnimation __instance, Vector3 headRot)
        {
            if (!Plugin.EnableMod.Value) return true;

            var player = EFTBindings.GetPlayer(__instance);
            if (player == null || !player.IsYourPlayer) return true;

            if (player.MovementContext.CurrentState.Name == EPlayerState.Stationary) return true;

            bool hasMovementMotion = Plugin.EnableHeadMovementMotion.Value;
            bool hasShotPunch = Plugin.EnableShotCameraHeadPunch.Value && (Mathf.Abs(KineticSpringEngine.CurrentShotHeadRoll) > 0.01f || Mathf.Abs(KineticSpringEngine.CurrentShotHeadPitch) > 0.01f || Mathf.Abs(KineticSpringEngine.CurrentShotHeadYaw) > 0.01f);
            bool hasOverrides = Plugin.EnableDiagnosticOverrides.Value;

            // Se nenhum efeito estiver ativo ou produzindo deslocamento, executa bypass nativo total
            if (!hasMovementMotion && !hasShotPunch && !hasOverrides)
                return true;

            Vector3 finalRot = headRot;

            // 1. Movimentação contínua da visão ao virar o mouse ou andar
            if (hasMovementMotion)
            {
                finalRot.x += KineticSpringEngine.CurrentHeadPitch;
                finalRot.y += KineticSpringEngine.CurrentHeadYaw;
                finalRot.z += KineticSpringEngine.CurrentHeadRoll;
            }

            // 2. Tranco e impacto físico violento de disparo (Bodycam Punch)
            if (Plugin.EnableShotCameraHeadPunch.Value)
            {
                finalRot.x += KineticSpringEngine.CurrentShotHeadPitch;
                finalRot.y += KineticSpringEngine.CurrentShotHeadYaw;
                finalRot.z += KineticSpringEngine.CurrentShotHeadRoll;
            }

            // 3. Injeção de Offsets de Diagnóstico Manual (F12)
            if (hasOverrides)
            {
                finalRot.x += Plugin.DebugHeadRotX.Value;
                finalRot.y += Plugin.DebugHeadRotY.Value;
                finalRot.z += Plugin.DebugHeadRotZ.Value;
            }

            player.HeadRotation = finalRot;
            EFTBindings.SetHeadRotationVec(__instance, finalRot);

            return false; // Assume o controle da cabeça durante o efeito
        }
    }

    // 3. Aplicação Pura e Direta no Osso Real da Arma (HandsContainer.WeaponRootAnim)
    public class Patch_WeaponRootAnimTransform : ModulePatch
    {
        private static bool _wasAimingLastFrame = false;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ProceduralWeaponAnimation), nameof(ProceduralWeaponAnimation.CalculateCameraPosition));
        }

        [PatchPostfix]
        private static void Postfix(ProceduralWeaponAnimation __instance)
        {
            if (!Plugin.EnableMod.Value || __instance == null || __instance.HandsContainer == null) return;

            var player = EFTBindings.GetPlayer(__instance);
            if (player == null || !player.IsYourPlayer) return;

            Transform weaponRootAnim = __instance.HandsContainer.WeaponRootAnim;
            if (weaponRootAnim == null) return;

            // Atualiza estado de mira sem resets destrutivos
            bool isAiming = __instance.IsAiming;
            _wasAimingLastFrame = isAiming;

            // Executa a física de amortecimento, roll e sway orgânico
            KineticSpringEngine.UpdatePhysics(player, Time.deltaTime);

            // Obtenção da orientação e posição da Câmera de Visão do Jogador (Camera View Space)
            Transform cameraTransform = __instance.HandsContainer.CameraTransform;
            Quaternion camRot = (cameraTransform != null) ? cameraTransform.rotation : (weaponRootAnim.parent != null ? weaponRootAnim.parent.rotation : Quaternion.identity);
            Vector3 camPos = (cameraTransform != null) ? cameraTransform.position : weaponRootAnim.position;

            // Rotação da física expressa no espaço de visão da câmera
            Quaternion camLocalRot = Quaternion.Euler(
                KineticSpringEngine.CurrentWeaponAngle.x, // Pitch
                KineticSpringEngine.CurrentWeaponAngle.y, // Yaw
                KineticSpringEngine.CurrentWeaponAngle.z  // Roll
            );

            // Matriz delta de rotação e translação no espaço de mundo
            Quaternion worldDeltaRot = camRot * camLocalRot * Quaternion.Inverse(camRot);
            Vector3 worldDeltaPos = camRot * KineticSpringEngine.CurrentWeaponPos;

            float blend = KineticSpringEngine.ADSTransitionBlend;

            // 1. NO HIPFIRE (blend = 0): Rotação pelo centro do WeaponRoot + Sway lateral/vertical de CQB
            Vector3 hipPos = weaponRootAnim.position + worldDeltaPos;
            Quaternion hipRot = worldDeltaRot * weaponRootAnim.rotation;

            // 2. NO ADS (blend = 1): Co-Axial Rígido em Repouso + Mola Dinâmica Inercial de Arrasto
            // Em repouso: ADSDynamicInertialAngle é zero, mantendo retículo e massa 100% cravados e co-axiais.
            // Em movimento de arraste: A mola inercial inclina suavemente a arma, gerando o micro-desalinhamento elástico de 4kg de peso.
            Quaternion adsInertialCamRot = Quaternion.Euler(
                KineticSpringEngine.ADSDynamicInertialAngle.x,
                KineticSpringEngine.ADSDynamicInertialAngle.y,
                KineticSpringEngine.ADSDynamicInertialAngle.z
            );
            Quaternion adsWorldInertialRot = camRot * adsInertialCamRot * Quaternion.Inverse(camRot);

            Vector3 toWeapon = weaponRootAnim.position - camPos;
            Vector3 adsPos = camPos + (adsWorldInertialRot * toWeapon);
            Quaternion adsRot = adsWorldInertialRot * weaponRootAnim.rotation;

            // 3. Interpolação Orgânica Contínua Hipfire <-> ADS
            weaponRootAnim.position = Vector3.Lerp(hipPos, adsPos, blend);
            weaponRootAnim.rotation = Quaternion.Slerp(hipRot, adsRot, blend);

            // Injeção de Offsets de Diagnóstico Manual (F12)
            if (Plugin.EnableDiagnosticOverrides.Value)
            {
                Vector3 manualPos = new Vector3(Plugin.DebugWeaponPosX.Value, Plugin.DebugWeaponPosY.Value, Plugin.DebugWeaponPosZ.Value);
                Quaternion manualRot = Quaternion.Euler(Plugin.DebugWeaponRotX.Value, Plugin.DebugWeaponRotY.Value, Plugin.DebugWeaponRotZ.Value);

                weaponRootAnim.position += camRot * manualPos;
                weaponRootAnim.rotation = (camRot * manualRot * Quaternion.Inverse(camRot)) * weaponRootAnim.rotation;
            }
        }
    }

    // 3.1 Espelhamento de Cinemática no Modelo de Terceira Pessoa (PlayerBones.ShiftWeaponRoot)
    public class Patch_ThirdPersonWeaponRoot : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(PlayerBones), nameof(PlayerBones.ShiftWeaponRoot));
        }

        [PatchPostfix]
        private static void Postfix(PlayerBones __instance)
        {
            if (!Plugin.EnableMod.Value || !Plugin.EnableThirdPersonSync.Value) return;
            if (__instance == null || __instance.Player == null || !__instance.Player.IsYourPlayer) return;

            Transform weaponRootThird = __instance.Weapon_Root_Third;
            if (weaponRootThird == null) return;

            // Orientação e posição da Câmera de Visão do Jogador
            Transform cameraTransform = __instance.Player.ProceduralWeaponAnimation?.HandsContainer?.CameraTransform;
            Quaternion camRot = (cameraTransform != null) ? cameraTransform.rotation : __instance.Player.Transform.rotation;
            Vector3 camPos = (cameraTransform != null) ? cameraTransform.position : weaponRootThird.position;

            Quaternion camLocalRot = Quaternion.Euler(
                KineticSpringEngine.CurrentWeaponAngle.x,
                KineticSpringEngine.CurrentWeaponAngle.y,
                KineticSpringEngine.CurrentWeaponAngle.z
            );

            Quaternion worldDeltaRot = camRot * camLocalRot * Quaternion.Inverse(camRot);
            Vector3 worldDeltaPos = camRot * KineticSpringEngine.CurrentWeaponPos;

            float blend = KineticSpringEngine.ADSTransitionBlend;

            Vector3 hipPos = weaponRootThird.position + worldDeltaPos;
            Quaternion hipRot = worldDeltaRot * weaponRootThird.rotation;

            Quaternion adsInertialCamRot = Quaternion.Euler(
                KineticSpringEngine.ADSDynamicInertialAngle.x,
                KineticSpringEngine.ADSDynamicInertialAngle.y,
                KineticSpringEngine.ADSDynamicInertialAngle.z
            );
            Quaternion adsWorldInertialRot = camRot * adsInertialCamRot * Quaternion.Inverse(camRot);

            Vector3 toWeapon = weaponRootThird.position - camPos;
            Vector3 adsPos = camPos + (adsWorldInertialRot * toWeapon);
            Quaternion adsRot = adsWorldInertialRot * weaponRootThird.rotation;

            weaponRootThird.position = Vector3.Lerp(hipPos, adsPos, blend);
            weaponRootThird.rotation = Quaternion.Slerp(hipRot, adsRot, blend);
        }
    }

    // 4. Atenuação do Sway Vanilla do Tarkov (Permite que a nossa mola física atue limpa)
    public class Patch_UpdateSwayFactors : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(ProceduralWeaponAnimation), "UpdateSwayFactors");
        }

        [PatchPostfix]
        private static void Postfix(ProceduralWeaponAnimation __instance)
        {
            if (!Plugin.EnableMod.Value || __instance == null || __instance.MotionReact == null)
                return;

            var player = EFTBindings.GetPlayer(__instance);
            if (player == null || !player.IsYourPlayer) return;

            // Limpa interferência dos eixos X e Z nativos do jogo e reduz o Y
            Vector3 vanillaSway = __instance.MotionReact.SwayFactors;
            __instance.MotionReact.SwayFactors = new Vector3(0f, vanillaSway.y * 0.2f, 0f);
        }
    }

    // 5. Impacto e Recuo Visual de Disparo (Player.OnMakingShot)
    public class Patch_OnMakingShot : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(Player), "OnMakingShot");
        }

        [PatchPostfix]
        private static void Postfix(Player __instance)
        {
            if (!Plugin.EnableMod.Value || __instance == null || !__instance.IsYourPlayer)
                return;

            bool isAiming = __instance.HandsController != null && __instance.HandsController.IsAiming;
            KineticSpringEngine.ApplyRecoilKick(isAiming);
        }
    }

    // 6. Sistema de Spy e Telemetria em Tempo Real do Laser
    public static class LaserSpy
    {
        public static bool Enabled = true;
        public static int CallsPerSecond = 0;
        public static int DistinctInstancesCount = 0;
        private static int _frameCounter = 0;
        private static float _lastTime = 0f;
        private static readonly System.Collections.Generic.HashSet<int> _instanceIds = new System.Collections.Generic.HashSet<int>();

        public static bool IsYourPlayer = false;
        public static string LastHierarchyPath = "";
        public static Vector3 WeaponDirection;
        public static Vector3 FireportForward;
        public static Vector3 TransformForward;
        public static float DeltaAngle;
        public static float HitDistance;
        public static Vector3 HitPoint;

        public static void RecordCall(int instanceId, string hierarchyPath, bool isOwner, Vector3 weaponDir, Vector3 fireportFwd, Vector3 transFwd, float deltaAng, float dist, Vector3 hitPt)
        {
            _instanceIds.Add(instanceId);
            LastHierarchyPath = hierarchyPath;
            IsYourPlayer = isOwner;
            WeaponDirection = weaponDir;
            FireportForward = fireportFwd;
            TransformForward = transFwd;
            DeltaAngle = deltaAng;
            HitDistance = dist;
            HitPoint = hitPt;

            _frameCounter++;
            if (Time.time - _lastTime >= 1.0f)
            {
                CallsPerSecond = _frameCounter;
                DistinctInstancesCount = _instanceIds.Count;
                _instanceIds.Clear();
                _frameCounter = 0;
                _lastTime = Time.time;
            }
        }
    }

    // 7. Sincronização Cirúrgica do Laser com a Balística Real do Cano (WeaponDirection / Fireport)
    public class Patch_LaserBeam_FireportSync : ModulePatch
    {
        private static readonly System.Reflection.FieldInfo _isOwnerField = AccessTools.Field(typeof(LaserBeam), "bool_0");
        private static readonly System.Reflection.FieldInfo _beamMeshField = AccessTools.Field(typeof(LaserBeam), "mesh_1");
        private static readonly System.Reflection.FieldInfo _pointMeshField = AccessTools.Field(typeof(LaserBeam), "mesh_0");
        private static readonly System.Reflection.FieldInfo _lightField = AccessTools.Field(typeof(LaserBeam), "light_0");
        private static readonly System.Reflection.FieldInfo _matBlock0Field = AccessTools.Field(typeof(LaserBeam), "materialPropertyBlock_0");
        private static readonly System.Reflection.FieldInfo _matBlock1Field = AccessTools.Field(typeof(LaserBeam), "materialPropertyBlock_1");

        private static readonly int _distanceId = Shader.PropertyToID("_Distance");
        private static readonly int _intensityId = Shader.PropertyToID("_Intensity");
        private static readonly int _sizeId = Shader.PropertyToID("_Size");
        private static readonly int _maxDistId = Shader.PropertyToID("_MaxDist");

        protected override System.Reflection.MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(LaserBeam), nameof(LaserBeam.LateUpdate));
        }

        [PatchPrefix]
        private static bool Prefix(LaserBeam __instance)
        {
            if (__instance == null || !Plugin.EnableMod.Value) return true;

            // Identificação direta e precisa do dono do Laser (bool_0 setado por SetOwner(isYourPlayer))
            bool isYourPlayer = false;
            if (_isOwnerField != null)
            {
                isYourPlayer = (bool)_isOwnerField.GetValue(__instance);
            }

            var myPlayer = GamePlayerOwner.MyPlayer;
            if (myPlayer == null) return true;

            var pwa = myPlayer.ProceduralWeaponAnimation;
            var firearmController = myPlayer.HandsController as Player.FirearmController;
            if (firearmController == null) return true;

            // Verificação precisa de Primeira Pessoa (FPS Rig) vs Terceira Pessoa (Body Rig)
            bool isFirstPerson = false;
            if (pwa != null && pwa.HandsContainer != null)
            {
                if ((pwa.HandsContainer.Weapon != null && __instance.transform.IsChildOf(pwa.HandsContainer.Weapon)) ||
                    (pwa.HandsContainer.WeaponRootAnim != null && __instance.transform.IsChildOf(pwa.HandsContainer.WeaponRootAnim)))
                {
                    isFirstPerson = true;
                    isYourPlayer = true;
                }
            }

            // Se for laser da 3ª pessoa do jogador local enquanto ele joga em 1ª pessoa:
            if (!isFirstPerson && isYourPlayer && myPlayer.PointOfView == EPointOfView.FirstPerson)
            {
                // Suprime a renderização duplicada/fantasma da 3ª pessoa na visão de 1ª pessoa
                return false;
            }

            if (!isYourPlayer)
                return true; // Deixa o laser de bots e outros players no fluxo vanilla

            // 1. Origem real do laser: sai da lente física do acessório montado no guarda-mão
            Vector3 startPos = __instance.transform.position;

            // 2. Direção balística consolidada do tiro (mesma trajetória usada pelo CreateShot do jogo)
            Vector3 forward = firearmController.WeaponDirection;
            Vector3 fireportForward = forward;
            Vector3 shotOrigin = firearmController.FireportPosition;

            // Aplica os ajustes de FOV e compensação de tórax (Ribcage/HandsHierarchy) do EFT
            firearmController.AdjustShotVectors(ref shotOrigin, ref forward);

            if (forward.sqrMagnitude < 0.0001f)
            {
                if (pwa != null && pwa.HandsContainer != null && pwa.HandsContainer.Fireport != null)
                    forward = -pwa.HandsContainer.Fireport.up;
                else
                    forward = __instance.transform.forward;
                fireportForward = forward;
            }

            Mesh pointMesh = (Mesh)_pointMeshField?.GetValue(__instance);
            Mesh beamMesh = (Mesh)_beamMeshField?.GetValue(__instance);
            Light spotLight = (Light)_lightField?.GetValue(__instance);
            MaterialPropertyBlock matBlock0 = (MaterialPropertyBlock)_matBlock0Field?.GetValue(__instance);
            MaterialPropertyBlock matBlock1 = (MaterialPropertyBlock)_matBlock1Field?.GetValue(__instance);

            if (pointMesh == null || beamMesh == null || matBlock0 == null || matBlock1 == null)
                return true;

            float hitDist = __instance.MaxDistance;
            Vector3 hitPoint = startPos + forward * __instance.MaxDistance;

            if (Physics.Raycast(startPos + forward * __instance.RayStart, forward, out var hitInfo, __instance.MaxDistance, __instance.Mask))
            {
                hitDist = hitInfo.distance;
                hitPoint = hitInfo.point;

                float sizeVal = Mathf.Lerp(__instance.PointSizeClose, __instance.PointSizeFar, hitInfo.distance / __instance.MaxDistance);
                float intensityVal = (1f - hitInfo.distance / __instance.MaxDistance);

                matBlock0.SetFloat(_distanceId, hitInfo.distance + __instance.RayStart);
                matBlock0.SetFloat(_intensityId, intensityVal);
                matBlock1.SetFloat(_intensityId, intensityVal);
                matBlock1.SetFloat(_sizeId, sizeVal);
                matBlock1.SetFloat(_maxDistId, __instance.MaxDistance);

                if (spotLight != null)
                {
                    Vector3 lightPos = hitInfo.point + (hitInfo.normal - forward).normalized * __instance.SurfaceOffsetForLight;
                    spotLight.transform.SetPositionAndRotation(lightPos, Quaternion.Lerp(Quaternion.LookRotation(hitInfo.point - lightPos, Vector3.up), Quaternion.LookRotation(forward), 0.25f));
                    spotLight.intensity = intensityVal * __instance.LightIntensity;
                    spotLight.spotAngle = Mathf.Lerp(__instance.AngleCloseFar.x, __instance.AngleCloseFar.y, hitInfo.distance / __instance.MaxDistance);
                }

                Vector3 normal = hitInfo.normal;
                Graphics.DrawMesh(pointMesh, hitInfo.point, Quaternion.LookRotation(normal), __instance.PointMaterial, LayerMask.NameToLayer("Default"), null, 0, matBlock1);
            }
            else
            {
                if (spotLight != null) spotLight.intensity = 0f;
                matBlock0.SetFloat(_distanceId, __instance.MaxDistance);
                matBlock0.SetFloat(_intensityId, 1f);
            }

            // Desenha o feixe volumétrico a partir da origem corrigida, na direção do cano de 1ª pessoa
            Graphics.DrawMesh(beamMesh, startPos, Quaternion.LookRotation(forward), __instance.BeamMaterial, LayerMask.NameToLayer("Default"), null, 0, matBlock0);

            // Spy: reporta a posição de origem corrigida para diagnóstico
            float deltaAngle = Vector3.Angle(forward, __instance.transform.forward);
            string povLabel = isFirstPerson ? "[1ª PESSOA]" : "[3ª PESSOA]";
            string hierarchyPath = (__instance.transform.parent != null) ? $"{povLabel} {__instance.transform.parent.name}/{__instance.name}" : $"{povLabel} {__instance.name}";
            LaserSpy.RecordCall(__instance.GetInstanceID(), hierarchyPath, isYourPlayer, forward, fireportForward, __instance.transform.forward, deltaAngle, hitDist, hitPoint);

            return false; // Suprime o LateUpdate original descompassado
        }
    }
}
