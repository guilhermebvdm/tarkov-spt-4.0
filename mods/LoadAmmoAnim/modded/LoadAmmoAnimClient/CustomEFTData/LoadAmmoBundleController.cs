using System;
using System.Linq;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

namespace Manimal.LoadAmmoAnim.CustomEFTData
{
    // hands controller for the mag-loading bundle. binds the animator inside
    // vmethod_0 when the engine attaches us to the spawned WeaponPrefab.
    public class LoadAmmoBundleController : Player.UsableItemController
    {
        // layer 1 is the firearms-anim convention used everywhere FirearmsAnimator's
        // Play call sites live. our bundle borrows the IFAK animator graph.
        private const int HandsLayer = 1;

        // state names in the bundle's animator graph.
        private const string PutAwayStateName = "USE TO OUT S";
        private const string DrawStateName    = "OUT TO USE S";

        private const string SocketMagazineName = "Magazine";
        private const string FallbackMagazineMesh = "pmag_MESH";
        private const string SocketBulletName   = "socket_bullet";
        private const string FallbackBulletMesh   = "patron_01";

        public Animator BundleAnimator { get; private set; }

        private GameObject _attachedMagGameObject;
        private GameObject _attachedBulletGameObject;
        private Renderer[] _bulletRenderers;
        private OffsetData _baseOffsets;

        // Populated in AttachDynamicItems so the LateUpdate save-hotkey handler can
        // read the current session without going through a static lookup every frame.
        private Player _ownerPlayer;
        private string _currentMagTemplateId;

        public override void vmethod_0(Player player, WeaponPrefab weaponPrefab)
        {
            try
            {
                base.vmethod_0(player, weaponPrefab);

                if (weaponPrefab == null) return;
                BundleAnimator = weaponPrefab.GetComponentInChildren<Animator>();
                if (BundleAnimator == null) return;

                // pool-reuse reset. between sessions the bundle gameobject is just
                // deactivated, not destroyed, so the animator wakes up in whatever
                // state it was in last session. force-jump to the draw state and
                // SetActive(true) so the engine's use-loop machinery takes over.
                BundleAnimator.Play(DrawStateName, HandsLayer, 0f);
                BundleAnimator.Update(0f);
                BundleAnimator.SetBool("Active", true);

                DynamicItemAttachmentService.DumpHierarchyOnce(weaponPrefab.gameObject);
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError(
                    $"[LoadAmmoAnim] LoadAmmoBundleController.vmethod_0 threw: {ex.GetType().Name}: {ex.Message}");
            }
        }

        /// <summary>
        /// Dynamically attaches native EFT magazine and ammo prefabs to the bundle's hand bones/sockets.
        /// </summary>
        public void AttachDynamicItems(
            Player player,
            MagazineItemClass mag,
            string magTemplateId,
            AmmoItemClass ammo,
            string ammoTemplateId)
        {
            ReleaseDynamicItems();

            GameObject root = ControllerGameObject;
            if (root == null) return;

            // 1. Dump hierarchy for troubleshooting
            DynamicItemAttachmentService.DumpHierarchyOnce(root);

            // 2. Disable any old static embedded meshes
            DynamicItemAttachmentService.DisableEmbeddedMeshes(root);

            // 3. Resolve sockets
            Transform magSocket = DynamicItemAttachmentService.ResolveSocket(root, SocketMagazineName, FallbackMagazineMesh);
            Transform bulletSocket = DynamicItemAttachmentService.ResolveSocket(root, SocketBulletName, FallbackBulletMesh);

            // 4. Look up caliber offset — priority: offsets.json (by templateId) > caliber family > default.
            string caliber = ammo?.Caliber ?? (mag?.Cartridges?.Items_1?.FirstOrDefault() as AmmoItemClass)?.Caliber;
            _baseOffsets = MagOffsetRegistry.GetOffset(caliber, magTemplateId, mag);

            // Cache for save-hotkey use in LateUpdate.
            _ownerPlayer = player;
            _currentMagTemplateId = magTemplateId;

            // 5. Attach native magazine prefab
            _attachedMagGameObject = DynamicItemAttachmentService.AttachItem(
                player,
                mag,
                magTemplateId,
                magSocket,
                _baseOffsets.MagPosition,
                _baseOffsets.MagRotation);

            // 6. Attach native bullet prefab
            _attachedBulletGameObject = DynamicItemAttachmentService.AttachItem(
                player,
                ammo,
                ammoTemplateId,
                bulletSocket,
                _baseOffsets.BulletPosition,
                _baseOffsets.BulletRotation);

            _bulletRenderers = _attachedBulletGameObject != null
                ? _attachedBulletGameObject.GetComponentsInChildren<Renderer>(true)
                : null;
        }

        private void LateUpdate()
        {
            // Real-time F12 slider offset updates
            if (_attachedMagGameObject != null)
            {
                Vector3 magPos = _baseOffsets.MagPosition + new Vector3(
                    Plugin.MagOffsetX?.Value ?? 0f,
                    Plugin.MagOffsetY?.Value ?? 0f,
                    Plugin.MagOffsetZ?.Value ?? 0f);

                Quaternion magRot = _baseOffsets.MagRotation * Quaternion.Euler(
                    Plugin.MagRotX?.Value ?? 0f,
                    Plugin.MagRotY?.Value ?? 0f,
                    Plugin.MagRotZ?.Value ?? 0f);

                _attachedMagGameObject.transform.localPosition = magPos;
                _attachedMagGameObject.transform.localRotation = magRot;
            }

            if (_attachedBulletGameObject != null)
            {
                Vector3 bulletPos = _baseOffsets.BulletPosition + new Vector3(
                    Plugin.BulletOffsetX?.Value ?? 0f,
                    Plugin.BulletOffsetY?.Value ?? 0f,
                    Plugin.BulletOffsetZ?.Value ?? 0f);

                Quaternion bulletRot = _baseOffsets.BulletRotation * Quaternion.Euler(
                    Plugin.BulletRotX?.Value ?? 0f,
                    Plugin.BulletRotY?.Value ?? 0f,
                    Plugin.BulletRotZ?.Value ?? 0f);

                _attachedBulletGameObject.transform.localPosition = bulletPos;
                _attachedBulletGameObject.transform.localRotation = bulletRot;

                // Sync bullet visibility with the loading animation cycle.
                // Adjustable via F12: BulletHideStart (when it enters the mag) and BulletHideEnd (when next round appears).
                if (BundleAnimator != null && _bulletRenderers != null && _bulletRenderers.Length > 0)
                {
                    bool shouldBeVisible = true;
                    var stateInfo = BundleAnimator.GetCurrentAnimatorStateInfo(HandsLayer);
                    if (!stateInfo.IsName(DrawStateName) && !stateInfo.IsName(PutAwayStateName))
                    {
                        float normTime = stateInfo.normalizedTime % 1.0f;
                        float hideStart = Plugin.BulletHideStart?.Value ?? 0.70f;
                        float hideEnd = Plugin.BulletHideEnd?.Value ?? 0.95f;

                        if (hideStart <= hideEnd)
                        {
                            if (normTime >= hideStart && normTime <= hideEnd)
                                shouldBeVisible = false;
                        }
                        else
                        {
                            // In case the range wraps around the loop boundary (e.g. 0.85 to 0.10)
                            if (normTime >= hideStart || normTime <= hideEnd)
                                shouldBeVisible = false;
                        }
                    }

                    for (int i = 0; i < _bulletRenderers.Length; i++)
                    {
                        if (_bulletRenderers[i] != null && _bulletRenderers[i].enabled != shouldBeVisible)
                        {
                            _bulletRenderers[i].enabled = shouldBeVisible;
                        }
                    }
                }
            }

            // Save-offset hotkey (Ctrl+S by default). Only fires when animation is active.
            if (Plugin.SaveOffsetHotkey != null
                && (Plugin.SaveOffsetHotkey.Value.IsDown() || ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.S)))
                && _attachedMagGameObject != null)
            {
                Plugin.SaveCurrentActiveOffset();
            }
        }

        /// <summary>
        /// Releases attached GameObjects back to EFT asset pools safely.
        /// </summary>
        public void ReleaseDynamicItems()
        {
            _bulletRenderers = null;
            DynamicItemAttachmentService.SafeRelease(ref _attachedMagGameObject);
            DynamicItemAttachmentService.SafeRelease(ref _attachedBulletGameObject);
        }

        private void OnDestroy()
        {
            try
            {
                ReleaseDynamicItems();
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogWarning($"[LoadAmmoAnim] LoadAmmoBundleController.OnDestroy exception: {ex.Message}");
            }
        }

        // play the put-away clip. SetActive(false) handles the engine side; the
        // explicit Play covers the case where the animator graph has no transition
        // out of the put-away state once entered.
        public void PlayPutAway()
        {
            if (BundleAnimator == null) return;
            try
            {
                BundleAnimator.SetBool("Active", false);
                BundleAnimator.Play(PutAwayStateName, HandsLayer, 0f);
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError(
                    $"[LoadAmmoAnim] PlayPutAway: {ex.GetType().Name}: {ex.Message}");
            }
        }

        // play the draw clip. used by the chained-mag transition after the mesh
        // has been swapped off-screen.
        public void PlayDraw()
        {
            if (BundleAnimator == null) return;
            try
            {
                BundleAnimator.Play(DrawStateName, HandsLayer, 0f);
                BundleAnimator.SetBool("Active", true);
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError(
                    $"[LoadAmmoAnim] PlayDraw: {ex.GetType().Name}: {ex.Message}");
            }
        }

        // true once the put-away clip has played past 95% normalized time. AnimLoop
        // polls this so it knows when the mag is off-screen and its safe to swap.
        public bool IsPutAwayNearlyDone()
        {
            if (BundleAnimator == null) return true;
            try
            {
                var info = BundleAnimator.GetCurrentAnimatorStateInfo(HandsLayer);
                return info.IsName(PutAwayStateName) && info.normalizedTime >= 0.95f;
            }
            catch { return true; }
        }
    }
}
