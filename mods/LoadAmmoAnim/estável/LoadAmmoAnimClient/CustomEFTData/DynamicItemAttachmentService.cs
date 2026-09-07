using Comfort.Common;
using EFT;
using EFT.AssetsManager;
using EFT.CameraControl;
using EFT.InventoryLogic;
using System;
using System.Linq;
using UnityEngine;

namespace Manimal.LoadAmmoAnim.CustomEFTData
{
    /// <summary>
    /// Handles dynamic instantiation, bone socket attachment, layer propagation and pool recycling
    /// of native EFT magazine and ammo prefabs for LoadAmmoAnim.
    /// </summary>
    public static class DynamicItemAttachmentService
    {
        private static int _weaponLayer = -1;

        public static int WeaponLayer
        {
            get
            {
                if (_weaponLayer == -1)
                {
                    _weaponLayer = LayerMask.NameToLayer("Weapon");
                    if (_weaponLayer == -1) _weaponLayer = 8; // fallback standard EFT Weapon layer
                }
                return _weaponLayer;
            }
        }

        /// <summary>
        /// Resolves the socket transform in the bundle prefab hierarchy.
        /// Searches first for explicit socket name, then falls back to existing rigged bone or mesh transform.
        /// If the resolved transform has a SkinnedMeshRenderer, returns the actual animated bone.
        /// </summary>
        public static Transform ResolveSocket(GameObject bundleRoot, string socketName, string fallbackMeshName)
        {
            if (bundleRoot == null) return null;

            Transform[] allTransforms = bundleRoot.GetComponentsInChildren<Transform>(true);

            // 1. Search for explicit socket node
            Transform target = allTransforms.FirstOrDefault(t => t.gameObject.name.Equals(socketName, StringComparison.OrdinalIgnoreCase));
            if (target != null) return ResolveAnimatedBoneIfSkinned(target);

            // 2. Fallback to existing mesh/bone transform (e.g. stanag_MESH or patron_01)
            if (!string.IsNullOrEmpty(fallbackMeshName))
            {
                target = allTransforms.FirstOrDefault(t => t.gameObject.name.Equals(fallbackMeshName, StringComparison.OrdinalIgnoreCase));
                if (target != null) return ResolveAnimatedBoneIfSkinned(target);
            }

            // 3. Fallback to specific animated nodes or hand bones
            if (socketName.IndexOf("mag", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                target = allTransforms.FirstOrDefault(t => t.gameObject.name.Equals("Magazine", StringComparison.OrdinalIgnoreCase)
                                                        || t.gameObject.name.IndexOf("pmag", StringComparison.OrdinalIgnoreCase) >= 0
                                                        || t.gameObject.name.IndexOf("stanag", StringComparison.OrdinalIgnoreCase) >= 0
                                                        || t.gameObject.name.IndexOf("item_hand_l", StringComparison.OrdinalIgnoreCase) >= 0
                                                        || t.gameObject.name.IndexOf("Hand_L", StringComparison.OrdinalIgnoreCase) >= 0
                                                        || t.gameObject.name.IndexOf("LeftHand", StringComparison.OrdinalIgnoreCase) >= 0);
            }
            else if (socketName.IndexOf("bullet", StringComparison.OrdinalIgnoreCase) >= 0 || socketName.IndexOf("ammo", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                target = allTransforms.FirstOrDefault(t => t.gameObject.name.IndexOf("patron", StringComparison.OrdinalIgnoreCase) >= 0
                                                        || t.gameObject.name.IndexOf("bullet", StringComparison.OrdinalIgnoreCase) >= 0
                                                        || t.gameObject.name.IndexOf("item_hand_r", StringComparison.OrdinalIgnoreCase) >= 0
                                                        || t.gameObject.name.IndexOf("Hand_R", StringComparison.OrdinalIgnoreCase) >= 0
                                                        || t.gameObject.name.IndexOf("RightHand", StringComparison.OrdinalIgnoreCase) >= 0);
            }

            return ResolveAnimatedBoneIfSkinned(target) ?? bundleRoot.transform;
        }

        private static Transform ResolveAnimatedBoneIfSkinned(Transform target)
        {
            if (target == null) return null;
            var smr = target.GetComponent<SkinnedMeshRenderer>();
            if (smr != null)
            {
                if (smr.rootBone != null) return smr.rootBone;
                if (smr.bones != null && smr.bones.Length > 0 && smr.bones[0] != null) return smr.bones[0];
            }
            return target;
        }

        /// <summary>
        /// Instantiates native EFT item prefab (Magazine or Ammo) and attaches it to the specified socket.
        /// </summary>
        public static GameObject AttachItem(
            Player player,
            Item item,
            string templateId,
            Transform socket,
            Vector3 localOffset,
            Quaternion localRotation)
        {
            if (socket == null) return null;

            GameObject spawned = null;
            try
            {
                var poolManager = Singleton<PoolManagerClass>.Instance;
                if (poolManager == null) return null;

                Item targetItem = item;
                if (targetItem == null && !string.IsNullOrEmpty(templateId))
                {
                    var factory = Singleton<ItemFactoryClass>.Instance;
                    if (factory != null)
                    {
                        targetItem = factory.CreateItem(
                            MongoID.Generate(false).ToString(),
                            templateId,
                            null);
                    }
                }

                if (targetItem == null)
                {
                    Plugin.LogSource?.LogWarning($"[LoadAmmoAnim] AttachItem: targetItem could not be created for template '{templateId}'.");
                    return null;
                }

                spawned = poolManager.CreateItem(targetItem, ECameraType.Default, player, false);
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[LoadAmmoAnim] AttachItem instantiation failed: {ex.Message}");
            }

            if (spawned == null) return null;

            try
            {
                spawned.transform.SetParent(socket, false);
                spawned.transform.localPosition = localOffset;
                spawned.transform.localRotation = localRotation;

                Vector3 parentLossy = socket.lossyScale;
                if (parentLossy.x > 0.0001f && parentLossy.y > 0.0001f && parentLossy.z > 0.0001f)
                {
                    spawned.transform.localScale = new Vector3(
                        1f / parentLossy.x,
                        1f / parentLossy.y,
                        1f / parentLossy.z);
                }
                else
                {
                    spawned.transform.localScale = Vector3.one;
                }

                SetLayerRecursively(spawned, WeaponLayer);
                spawned.SetActive(true);
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogError($"[LoadAmmoAnim] AttachItem failed during parenting: {ex.Message}");
            }

            return spawned;
        }

        /// <summary>
        /// Recursively applies the WeaponLayer to all children to prevent camera near clipping.
        /// </summary>
        public static void SetLayerRecursively(GameObject obj, int layer)
        {
            if (obj == null) return;
            obj.layer = layer;
            foreach (Transform child in obj.transform)
            {
                if (child != null)
                    SetLayerRecursively(child.gameObject, layer);
            }
        }

        /// <summary>
        /// Disables all hardcoded embedded magazine/bullet meshes in the bundle prefab so they don't render.
        /// </summary>
        public static void DisableEmbeddedMeshes(GameObject bundleRoot)
        {
            if (bundleRoot == null) return;
            try
            {
                var meshRenderers = bundleRoot.GetComponentsInChildren<MeshRenderer>(true);
                foreach (var r in meshRenderers)
                {
                    if (IsArmOrCharacterMesh(r.gameObject.name)) continue;
                    r.enabled = false;
                }

                var skinnedRenderers = bundleRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true);
                foreach (var sr in skinnedRenderers)
                {
                    if (IsArmOrCharacterMesh(sr.gameObject.name)) continue;
                    sr.enabled = false;
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogWarning($"[LoadAmmoAnim] DisableEmbeddedMeshes error: {ex.Message}");
            }
        }

        private static bool IsArmOrCharacterMesh(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            return name.IndexOf("arm", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("hand", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("body", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("glove", StringComparison.OrdinalIgnoreCase) >= 0
                || name.IndexOf("sleeve", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static bool _dumpedOnce = false;

        /// <summary>
        /// Dumps the hierarchy tree of the bundle to the BepInEx log once for debugging.
        /// </summary>
        public static void DumpHierarchyOnce(GameObject root)
        {
            if (root == null || _dumpedOnce) return;
            _dumpedOnce = true;

            try
            {
                var sb = new System.Text.StringBuilder();
                sb.AppendLine($"\n========== [LoadAmmoAnim] BUNDLE HIERARCHY DUMP for '{root.name}' ==========");
                DumpTransformRecursive(root.transform, 0, sb);
                sb.AppendLine("===========================================================================\n");
                Plugin.LogSource?.LogInfo(sb.ToString());
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogWarning($"[LoadAmmoAnim] DumpHierarchy error: {ex.Message}");
            }
        }

        private static void DumpTransformRecursive(Transform t, int depth, System.Text.StringBuilder sb)
        {
            if (t == null) return;
            string indent = new string(' ', depth * 2);
            string compInfo = "";
            if (t.GetComponent<MeshRenderer>() != null) compInfo += " [MeshRenderer]";
            if (t.GetComponent<SkinnedMeshRenderer>() != null) compInfo += " [SkinnedMeshRenderer]";
            if (t.GetComponent<Animator>() != null) compInfo += " [Animator]";
            sb.AppendLine($"{indent}- {t.name}{compInfo} (pos: {t.localPosition.x:F3},{t.localPosition.y:F3},{t.localPosition.z:F3} | rot: {t.localRotation.eulerAngles.x:F1},{t.localRotation.eulerAngles.y:F1},{t.localRotation.eulerAngles.z:F1})");

            for (int i = 0; i < t.childCount; i++)
            {
                DumpTransformRecursive(t.GetChild(i), depth + 1, sb);
            }
        }

        /// <summary>
        /// Safely unparents and returns an instantiated EFT prefab to its asset pool.
        /// </summary>
        public static void SafeRelease(ref GameObject obj)
        {
            if (obj == null) return;
            try
            {
                obj.transform.SetParent(null, false);
                obj.SetActive(false);
                var poolObj = obj.GetComponent<AssetPoolObject>();
                if (poolObj != null)
                {
                    AssetPoolObject.ReturnToPool(obj);
                }
                else
                {
                    UnityEngine.Object.Destroy(obj);
                }
            }
            catch (Exception ex)
            {
                Plugin.LogSource?.LogWarning($"[LoadAmmoAnim] SafeRelease failed returning object to pool: {ex.Message}");
                try { UnityEngine.Object.Destroy(obj); } catch { }
            }
            finally
            {
                obj = null;
            }
        }
    }
}
