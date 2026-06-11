using System.Linq;
using BepInEx.Configuration;
using EFT;
using EFT.Animations;
using EFT.InventoryLogic;
using EFT.WeaponMounting;
using UnityEngine;

namespace CameraRotationMod
{
    public enum EBracingDirection 
    {
        Top,
        Left, 
        Right,
        None
    }

    public class MountingManager : MonoBehaviour
    {
        public static MountingManager Instance { get; private set; }

        private static bool _isMounting;
        public static bool IsMounting 
        { 
            get => _isMounting; 
            set
            {
                if (_isMounting != value)
                {
                    _isMounting = value;
                    var gw = StanceManager.GetCachedGameWorld();
                    if (gw?.MainPlayer != null)
                    {
                        var fc = gw.MainPlayer.HandsController as EFT.Player.FirearmController;
                        if (fc != null)
                        {
                            fc.FirearmsAnimator.SetMounted(value);
                        }
                        FikaSync.FikaNetworkSync.SendStanceUpdate(gw.MainPlayer.ProfileId, StanceManager.CurrentStance, _isMounting);
                    }
                }
            }
        }
        public static bool CanMount { get; private set; }
        public static EBracingDirection BracingDirection { get; private set; } = EBracingDirection.None;
        public static bool IsBracing { get; private set; }

        private Player _player;
        private ProceduralWeaponAnimation _pwa;

        private Texture2D _mountingTex;
        
        // Controle de performance (Gaps de FPS)
        private float _lastRaycastTime = 0f;
        private const float RaycastCooldown = 0.2f; // 5 ticks por segundo

        private static Vector3 _startLeftDir = new Vector3(0.143f, 0f, 0f);
        private static Vector3 _startRightDir = new Vector3(-0.143f, 0f, 0f);
        private static Vector3 _startDownDir = new Vector3(0f, 0f, -0.19f);

        private void Awake()
        {
            Instance = this;
            _mountingTex = new Texture2D(2, 2);
            _mountingTex.SetPixels(new Color[] { Color.white, Color.white, Color.white, Color.white });
            _mountingTex.Apply();
        }

        private void Update()
        {
            var gameWorld = StanceManager.GetCachedGameWorld();
            _player = gameWorld?.MainPlayer;

            if (_player == null || !_player.IsYourPlayer || !_player.HealthController.IsAlive)
            {
                IsMounting = false;
                CanMount = false;
                IsBracing = false;
                return;
            }

            _pwa = _player.ProceduralWeaponAnimation;

            if (!Plugin._EnableWeaponMounting.Value)
            {
                CanMount = false;
                IsMounting = false;
                IsBracing = false;
                return;
            }

            // Sincroniza o IsMounting nativo do Tarkov
            bool isMountedNatively = _player.MovementContext.IsInMountedState;
            IsMounting = isMountedNatively;

            if (isMountedNatively)
            {
                IsBracing = true;
                CanMount = true;
                if (_player.MovementContext.PlayerMountingPointData?.MountPointData != null)
                {
                    var nativeDir = _player.MovementContext.PlayerMountingPointData.MountPointData.MountSideDirection;
                    if (nativeDir == EMountSideDirection.Left)
                    {
                        BracingDirection = EBracingDirection.Left;
                    }
                    else if (nativeDir == EMountSideDirection.Right)
                    {
                        BracingDirection = EBracingDirection.Right;
                    }
                    else
                    {
                        BracingDirection = EBracingDirection.Top;
                    }
                }
            }
            else
            {
                if (!IsBracing)
                {
                    BracingDirection = EBracingDirection.None;
                }
            }

            // Cancelar ao correr
            if (IsMounting && _player.IsSprintEnabled)
            {
                IsMounting = false;
            }

            DetectBracing();
        }

        private void SetMountingStatus(EBracingDirection coverDir)
        {
            if (!IsMounting)
            {
                BracingDirection = coverDir;
            }
            if (!IsBracing)
            {
                Plugin.Logger.LogInfo($"[MountingManager] Bracing ATIVADO! Direção: {coverDir}");
            }
            IsBracing = true;
            CanMount = true;
        }

        private bool IsBracingProne() 
        {
            if (_player.IsInPronePose) 
            {
                SetMountingStatus(EBracingDirection.Top);
                return true;
            }
            return false;
        }

        private bool CheckForCoverCollision(EBracingDirection coverDir, Vector3 start, Vector3 direction, Vector3 spherePos, float radius)
        {
            int playerMask = LayerMask.NameToLayer("Player");
            int layerMask = LayerMaskClass.HighPolyWithTerrainMask;

            if (Physics.Linecast(start, direction, out RaycastHit raycastHit, layerMask))
            {
                if (raycastHit.collider.gameObject.layer != playerMask)
                {
                    SetMountingStatus(coverDir);
                    if (!IsBracing)
                    {
                        Plugin.Logger.LogInfo($"[MountingManager] Linecast detectou cobertura {coverDir} no collider: {raycastHit.collider.gameObject.name} (Layer: {LayerMask.LayerToName(raycastHit.collider.gameObject.layer)})");
                    }
                    return true;
                }
            }

            Collider[] hitColliders = Physics.OverlapSphere(spherePos, radius, layerMask);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.gameObject.layer != playerMask)
                {
                    SetMountingStatus(coverDir);
                    if (!IsBracing)
                    {
                        Plugin.Logger.LogInfo($"[MountingManager] OverlapSphere detectou cobertura {coverDir} no collider: {hitCollider.gameObject.name} (Layer: {LayerMask.LayerToName(hitCollider.gameObject.layer)})");
                    }
                    return true;
                }
            }

            return false;
        }

        public void DetectBracing()
        {
            if (Time.time - _lastRaycastTime <= RaycastCooldown) return;
            _lastRaycastTime = Time.time;

            if (_pwa == null || _player == null) return;

            // Se já estiver montado nativamente, as flags e direções já são tratadas no Update
            if (_player.MovementContext.IsInMountedState)
            {
                return;
            }

            var fc = _player.HandsController as EFT.Player.FirearmController;
            if (fc == null || fc.Weapon == null) return;
            
            float ln = fc.Weapon.CalculateCellSize().X * 0.1f + 0.15f;


            Transform weapTransform = _pwa.HandsContainer.WeaponRootAnim;
            if (weapTransform == null) return;

            Vector3 linecastDirection = weapTransform.TransformDirection(Vector3.up);

            Vector3 downDir = _startDownDir;
            
            Vector3 startDown = weapTransform.position + weapTransform.TransformDirection(downDir);
            Vector3 startLeft = weapTransform.position + weapTransform.TransformDirection(_startLeftDir);
            Vector3 startRight = weapTransform.position + weapTransform.TransformDirection(_startRightDir);

            Vector3 sphereDown = weapTransform.position + weapTransform.TransformDirection(new Vector3(0f, -0.45f, -0.1f));
            Vector3 sphereLeft = weapTransform.position + weapTransform.TransformDirection(new Vector3(0.05f, -0.5f, -0.065f));
            Vector3 sphereRight = weapTransform.position + weapTransform.TransformDirection(new Vector3(-0.05f, -0.5f, -0.065f));

            // Multiplicamos o ln por 1.25f para ter um alcance melhor de detecção passiva de bracing
            float detectLength = ln * 1.25f;

            Vector3 forwardDirection = startDown - linecastDirection * detectLength;
            Vector3 leftDirection = startLeft - linecastDirection * detectLength;
            Vector3 rightDirection = startRight - linecastDirection * detectLength;

            if (IsBracingProne() ||
                CheckForCoverCollision(EBracingDirection.Top, startDown, forwardDirection, sphereDown, 0.045f) ||
                CheckForCoverCollision(EBracingDirection.Left, startLeft, leftDirection, sphereLeft, 0.09f) ||
                CheckForCoverCollision(EBracingDirection.Right, startRight, rightDirection, sphereRight, 0.09f))
            {
                return;
            }
            
            if (IsBracing)
            {
                Plugin.Logger.LogInfo("[MountingManager] Perdeu contato com cobertura. Bracing desativado.");
                BracingDirection = EBracingDirection.None;
            }
            IsBracing = false;
            CanMount = false;
        }
    }
}
