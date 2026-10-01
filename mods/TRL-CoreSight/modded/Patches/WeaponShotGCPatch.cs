using System.Reflection;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using TRLCoreSight.Core;
using UnityEngine;

namespace TRLCoreSight.Patches
{
    /// <summary>
    /// Registra disparos de arma efetuados pelo jogador local e por bots próximos para estender a janela de combate do GCOptimizer.
    /// ref: Assembly-CSharp/WeaponManagerClass.cs:595
    /// </summary>
    public class WeaponShotGCPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(WeaponManagerClass), nameof(WeaponManagerClass.StartSpawnShell));
        }

        [PatchPostfix]
        private static void Postfix(WeaponManagerClass __instance)
        {
            if (GCOptimizerManager.Instance == null || __instance?.Player == null)
            {
                return;
            }

            // Se o disparo for do jogador local, inicia imediatamente a janela de proteção contra GC
            if (__instance.Player.IsYourPlayer)
            {
                GCOptimizerManager.Instance.NotifyShotFired();
                return;
            }

            // Se for bot em combate próximo (< 50m da câmera), também suprime o GC
            Camera activeCam = (CameraClass.Exist && CameraClass.Instance.Camera != null)
                ? CameraClass.Instance.Camera
                : Camera.main;

            if (activeCam != null)
            {
                float sqrDist = (__instance.Player.Position - activeCam.transform.position).sqrMagnitude;
                if (sqrDist <= 2500f) // 50m * 50m
                {
                    GCOptimizerManager.Instance.NotifyShotFired();
                }
            }
        }
    }
}
