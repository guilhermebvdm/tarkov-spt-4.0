using System.Reflection;
using Comfort.Common;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;
using TRLCoreSight.Configuration;
using UnityEngine;

namespace TRLCoreSight.Patches
{
    /// <summary>
    /// Intercepta o spawn de cápsulas de bala ejetadas por armas de bots distantes,
    /// suprimindo a criação de corrotinas na Unity e simulação de corpos rígidos na PhysX.
    /// ref: Assembly-CSharp/WeaponManagerClass.cs:595
    /// ref: Assembly-CSharp/WeaponManagerClass.cs:603
    /// ref: Assembly-CSharp/WeaponManagerClass.cs:631
    /// </summary>
    public class ShellSpawnCullingPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // ref: Assembly-CSharp/WeaponManagerClass.cs:595
            return AccessTools.Method(typeof(WeaponManagerClass), nameof(WeaponManagerClass.StartSpawnShell));
        }

        [PatchPrefix]
        private static bool Prefix(WeaponManagerClass __instance)
        {
            return ShouldAllowShellSpawn(__instance);
        }

        internal static bool ShouldAllowShellSpawn(WeaponManagerClass weaponManager)
        {
            if (weaponManager == null)
            {
                return true;
            }

            if (!ModConfig.ModEnabled.Value || !ModConfig.EnableShellCulling.Value)
            {
                return true;
            }

            // A arma do jogador local sempre mantém ejeção e física completas
            if (weaponManager.Player != null && weaponManager.Player.IsYourPlayer)
            {
                return true;
            }

            Camera activeCamera = (CameraClass.Exist && CameraClass.Instance.Camera != null)
                ? CameraClass.Instance.Camera
                : Camera.main;

            if (activeCamera == null)
            {
                return true;
            }

            Vector3 weaponPos;
            if (weaponManager.WeaponPrefab_0 != null)
            {
                weaponPos = weaponManager.WeaponPrefab_0.transform.position;
            }
            else if (weaponManager.Player != null)
            {
                weaponPos = weaponManager.Player.Position;
            }
            else
            {
                return true;
            }

            float sqrDist = (weaponPos - activeCamera.transform.position).sqrMagnitude;
            float maxDist = ModConfig.ShellCullingDistance.Value;

            // Se o disparo ocorreu além da distância limite, cancela a corrotina de spawn
            if (sqrDist > (maxDist * maxDist))
            {
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Intercepta a ejeção múltipla de cartuchos (espingardas de cano duplo, revólveres) a longa distância.
    /// ref: Assembly-CSharp/WeaponManagerClass.cs:603
    /// </summary>
    public class ShellSpawnAllCullingPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // ref: Assembly-CSharp/WeaponManagerClass.cs:603
            return AccessTools.Method(typeof(WeaponManagerClass), nameof(WeaponManagerClass.StartSpawnAllShells));
        }

        [PatchPrefix]
        private static bool Prefix(WeaponManagerClass __instance)
        {
            return ShellSpawnCullingPatch.ShouldAllowShellSpawn(__instance);
        }
    }

    /// <summary>
    /// Intercepta ejeção de cartuchos ao sanar panes mecânicas de armas de bots distantes.
    /// ref: Assembly-CSharp/WeaponManagerClass.cs:631
    /// </summary>
    public class ShellSpawnJamCullingPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            // ref: Assembly-CSharp/WeaponManagerClass.cs:631
            return AccessTools.Method(typeof(WeaponManagerClass), nameof(WeaponManagerClass.SpawnShellAfterJam));
        }

        [PatchPrefix]
        private static bool Prefix(WeaponManagerClass __instance)
        {
            return ShellSpawnCullingPatch.ShouldAllowShellSpawn(__instance);
        }
    }
}
