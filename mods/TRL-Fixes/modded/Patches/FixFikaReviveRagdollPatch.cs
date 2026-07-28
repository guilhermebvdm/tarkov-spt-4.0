using HarmonyLib;
using EFT;
using EFT.AssetsManager;
using UnityEngine;
using System;
using System.Reflection;

namespace TRLFixes.Patches
{
    public class FixFikaReviveRagdollPatch
    {
        private static FieldInfo _observedPlayerField;
        private static FieldInfo _ragdollField;

        public void Enable()
        {
            try
            {
                var harmony = new Harmony("com.trl.fixes.fikareviveragdoll");
                var reviveType = AccessTools.TypeByName("Fika.Core.Main.Components.ReviveInteractable");

                if (reviveType != null)
                {
                    // Cache estatico dos campos de reflexao para otimizar performance no runtime (CR-01-01)
                    _observedPlayerField = AccessTools.Field(reviveType, "_observedPlayer");
                    _ragdollField = AccessTools.Field(reviveType, "_ragdoll");

                    var removeRagdollMethod = AccessTools.Method(reviveType, "RemoveRagdoll");
                    if (removeRagdollMethod != null)
                    {
                        var postfixMethod = AccessTools.Method(typeof(FixFikaReviveRagdollPatch), nameof(Postfix));
                        harmony.Patch(removeRagdollMethod, postfix: new HarmonyMethod(postfixMethod));
                        Debug.Log("TRL-Fixes: Hook no ReviveInteractable.RemoveRagdoll aplicado com sucesso!");
                    }
                    else
                    {
                        Debug.LogError("TRL-Fixes: Metodo ReviveInteractable.RemoveRagdoll nao encontrado!");
                    }
                }
                else
                {
                    Debug.Log("TRL-Fixes: FIKA nao detectado (ReviveInteractable nulo). O patch de colisao pos-revive nao sera ativado.");
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"TRL-Fixes: Erro ao aplicar patch FixFikaReviveRagdollPatch: {ex}");
            }
        }

        public static void Postfix(object __instance)
        {
            try
            {
                if (__instance == null || _observedPlayerField == null) return;

                var observedPlayer = _observedPlayerField.GetValue(__instance) as Player;
                if (observedPlayer == null || observedPlayer.gameObject == null) return;

                int hitColliderLayer = LayerMask.NameToLayer("HitCollider");
                int playerLayer = LayerMask.NameToLayer("Player");

                if (hitColliderLayer == -1) hitColliderLayer = 12;
                if (playerLayer == -1) playerLayer = 8;

                // 1. Garante que a raiz do player e visuais permaneçam no layer Player
                TransformHelperClass.SetLayersRecursively(observedPlayer.gameObject, playerLayer);

                // 2. Restaura explicitamente TODOS os BodyPartColliders para o layer HitCollider (Layer 12)
                if (observedPlayer.PlayerBones != null && observedPlayer.PlayerBones.BodyPartColliders != null)
                {
                    foreach (var bpc in observedPlayer.PlayerBones.BodyPartColliders)
                    {
                        if (bpc != null && bpc.gameObject != null)
                        {
                            bpc.gameObject.layer = hitColliderLayer;
                        }
                    }
                }

                // Fallback defensivo (CR-01-03): varre qualquer BodyPartCollider extra na hierarquia do ObservedPlayer
                foreach (var bpc in observedPlayer.gameObject.GetComponentsInChildren<BodyPartCollider>(true))
                {
                    if (bpc != null && bpc.gameObject != null)
                    {
                        bpc.gameObject.layer = hitColliderLayer;
                    }
                }

                // 3. Reativa e restaura o layer HitCollider em todos os ArmorPlateColliders
                if (observedPlayer.PlayerBones != null && observedPlayer.PlayerBones.ArmorPlateColliders != null)
                {
                    foreach (var apc in observedPlayer.PlayerBones.ArmorPlateColliders)
                    {
                        if (apc != null && apc.gameObject != null)
                        {
                            apc.gameObject.SetActive(true);
                            apc.gameObject.layer = hitColliderLayer;
                        }
                    }
                }

                // Fallback defensivo (CR-01-03): garante a reativacao de qualquer ArmorPlateCollider extra
                foreach (var apc in observedPlayer.gameObject.GetComponentsInChildren<ArmorPlateCollider>(true))
                {
                    if (apc != null && apc.gameObject != null)
                    {
                        apc.gameObject.SetActive(true);
                        apc.gameObject.layer = hitColliderLayer;
                    }
                }

                // 4. Congela Rigidbodies para ficarem isKinematic e evita conflito de fisica vs Animator
                foreach (var rb in observedPlayer.gameObject.GetComponentsInChildren<Rigidbody>(true))
                {
                    if (rb != null)
                    {
                        rb.isKinematic = true;
                        rb.collisionDetectionMode = CollisionDetectionMode.Discrete;
                        
                        // [Conceito 4.1 / CR-01-02] GClass745: Helper de gerenciamento/suporte a Rigidbodies sob EFTPhysicsClass
                        EFTPhysicsClass.GClass745.UnsupportRigidbody(rb);
                    }
                }

                // 5. Encerra qualquer estado/corrotina pendente do Ragdoll usando a FieldInfo em cache (CR-01-01)
                if (_ragdollField != null)
                {
                    var ragdollObj = _ragdollField.GetValue(__instance);
                    if (ragdollObj is RagdollClass ragdoll)
                    {
                        ragdoll.ForceStopRigidBody();
                    }
                }

                Debug.Log($"[TRL-Fixes] Colisao e armaduras do ObservedPlayer ({observedPlayer.ProfileId}) restauradas com sucesso pos-revive!");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[TRL-Fixes] Excecao ao restaurar colisao pos-revive: {ex}");
            }
        }
    }
}
