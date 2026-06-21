using System;
using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace CameraRotationMod.Patches
{
    // Detecção do mount PASSIVO (item 011). Hook no cálculo de oclusão de arma do EFT — reaproveita o
    // origin/ln/weaponUp REAIS que o jogo passa (correção sobre o item 004, que inventava o ln) e dispara
    // raycasts próprios (Top/Left/Right), pois o EFT não expõe "superfície montável disponível" antes de
    // montar (GClass2667 é caixa-preta). Grava PassiveMountState. SOMENTE jogador local (AP-02).
    public class PassiveMountDetectPatch : ModulePatch
    {
        private static FieldInfo _playerField;
        private static float _lastDetect;
        private const float Cooldown = 0.1f; // ~10x/s — leve no hot path

        // Offsets em espaço local do WeaponRootAnim (validados no item 004 / RealismMod CollisionPatch).
        private static readonly Vector3 _down  = new Vector3(0f, 0f, -0.19f);
        private static readonly Vector3 _left  = new Vector3(0.143f, 0f, 0f);
        private static readonly Vector3 _right = new Vector3(-0.143f, 0f, 0f);

        protected override MethodBase GetTargetMethod()
        {
            _playerField = AccessTools.Field(typeof(Player.FirearmController), "_player");
            // AP-03: resolver por assinatura (origin, ln, ref overlapsWithPlayer, weaponUp), não por número.
            // ref: Assembly-CSharp/EFT/Player.cs:12814
            return AccessTools.Method(typeof(Player.FirearmController), "method_11",
                new[] { typeof(Vector3), typeof(float), typeof(bool).MakeByRefType(), typeof(Vector3?) });
        }

        [PatchPostfix]
        private static void Postfix(Player.FirearmController __instance, float ln, Vector3? weaponUp)
        {
            try
            {
                if (!Plugin._EnablePassiveMount.Value) return;

                var player = (Player)_playerField.GetValue(__instance);
                if (player == null || !player.IsYourPlayer) return; // AP-02: só o jogador local

                if (Time.time - _lastDetect <= Cooldown) return;
                _lastDetect = Time.time;
                PassiveMountState.LastDetectTick = Time.time;

                var pwa = player.ProceduralWeaponAnimation; // [validado-004]
                if (pwa == null || pwa.HandsContainer == null || pwa.HandsContainer.WeaponRootAnim == null)
                {
                    PassiveMountState.ClearBracing();
                    return;
                }

                // Cede ao vanilla: montado / bipé / deitado / correndo => sem passivo.
                if (pwa.IsMountedState || pwa.IsBipodUsed || player.IsInPronePose || player.IsSprintEnabled)
                {
                    PassiveMountState.ClearBracing();
                    return;
                }

                Transform w = pwa.HandsContainer.WeaponRootAnim;
                Vector3 up = weaponUp ?? w.TransformDirection(Vector3.up);
                float len = ln * 1.25f;
                int mask = EFTHardSettings.Instance.WEAPON_OCCLUSION_LAYERS; // ref: EFTHardSettings.cs:251
                int playerLayer = LayerMask.NameToLayer("Player");

                if (Cast(EBracingDir.Top,   w.position + w.TransformDirection(_down),  up, len, mask, playerLayer)) return;
                if (Cast(EBracingDir.Left,  w.position + w.TransformDirection(_left),  up, len, mask, playerLayer)) return;
                if (Cast(EBracingDir.Right, w.position + w.TransformDirection(_right), up, len, mask, playerLayer)) return;

                PassiveMountState.ClearBracing();
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"[PassiveMountDetect] {ex.Message}");
            }
        }

        private static bool Cast(EBracingDir dir, Vector3 start, Vector3 up, float len, int mask, int playerLayer)
        {
            if (Physics.Linecast(start, start - up * len, out RaycastHit hit, mask, QueryTriggerInteraction.Ignore)
                && hit.collider.gameObject.layer != playerLayer)
            {
                PassiveMountState.SetBracing(dir);
                return true;
            }
            return false;
        }
    }
}
