using System;
using System.Collections;
using System.Reflection;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using EFT.UI;
using HarmonyLib;
using SPT.Reflection.Patching;
using UnityEngine;

namespace CameraRotationMod.Patches
{
    /// <summary>
    /// Atraso de exibição do HUD ao checar o carregador e a câmara (EftBattleUIScreen.ShowAmmoDetails).
    /// Tanto a checagem de carregador (vanilla ou via mod MagCheckInterrupt) quanto a checagem de câmara
    /// (ChamberCheckAmmoPatch) convergem para EftBattleUIScreen.ShowAmmoDetails para mostrar o AmmoCountPanel.
    /// Este patch intercepta ShowAmmoDetails com Prefix, suprime a chamada imediata no início da animação
    /// e aguarda o tempo configurado (padrão 2.0s) em uma Coroutine antes de exibir as informações na tela,
    /// sincronizando a revelação dos dados com a inspeção física do personagem.
    /// </summary>
    public class MagCheckDelayPatch : ModulePatch
    {
        private static Coroutine _activeRoutine;
        private static bool _isExecutingDelayedShow;

        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(EftBattleUIScreen), nameof(EftBattleUIScreen.ShowAmmoDetails), new[]
            {
                typeof(int),
                typeof(int),
                typeof(int),
                typeof(string),
                typeof(bool)
            });
        }

        [PatchPrefix]
        private static bool Prefix(
            EftBattleUIScreen __instance,
            int ammoCount,
            int maxAmmoCount,
            int mastering,
            string details,
            bool foldingMechanimWeapon)
        {
            try
            {
                // Se for a chamada atrasada executada pela própria Coroutine, deixa passar para a UI original
                if (_isExecutingDelayedShow)
                {
                    return true;
                }

                // Se o delay estiver desligado pelo usuário no menu do F12, exibe instantaneamente
                if (Plugin._EnableMagCheckDelay == null || !Plugin._EnableMagCheckDelay.Value)
                {
                    return true;
                }

                var player = Singleton<GameWorld>.Instantiated ? Singleton<GameWorld>.Instance.MainPlayer : null;
                if (player == null || !player.IsYourPlayer || !player.FirstPersonPointOfView)
                {
                    return true; // apenas para o jogador local em primeira pessoa
                }

                var fc = player.HandsController as Player.FirearmController;
                var weapon = fc?.Weapon;

                if (_activeRoutine != null && Plugin.Instance != null)
                {
                    Plugin.Instance.StopCoroutine(_activeRoutine);
                    _activeRoutine = null;
                }

                if (Plugin.Instance != null && __instance != null)
                {
                    _activeRoutine = Plugin.Instance.StartCoroutine(
                        DelayedShowRoutine(__instance, player, weapon, ammoCount, maxAmmoCount, mastering, details, foldingMechanimWeapon)
                    );
                }

                return false; // suprime a exibição instantânea no frame 0
            }
            catch (Exception ex)
            {
                // ref: CR-01-01 — {ex} (não só .Message) preserva o stack trace, igual ao resto do mod.
                Plugin.Logger.LogError($"[MagCheckDelay] Erro no Prefix: {ex}");
                return true;
            }
        }

        private static IEnumerator DelayedShowRoutine(
            EftBattleUIScreen screen,
            Player player,
            Weapon weapon,
            int ammoCount,
            int maxAmmoCount,
            int mastering,
            string details,
            bool foldingMechanimWeapon)
        {
            float delay = Mathf.Max(0.1f, Plugin._MagCheckDelaySeconds?.Value ?? 1.0f); // fallback casa com o default da ConfigEntry
            yield return new WaitForSeconds(delay);

            _activeRoutine = null;

            try
            {
                if (screen == null || !screen.gameObject.activeInHierarchy) yield break;
                if (player == null || !player.HealthController.IsAlive || !player.FirstPersonPointOfView) yield break;

                var fc = player.HandsController as Player.FirearmController;
                if (fc == null || fc.Weapon == null || fc.Weapon != weapon) yield break;

                _isExecutingDelayedShow = true;
                try
                {
                    screen.ShowAmmoDetails(ammoCount, maxAmmoCount, mastering, details, foldingMechanimWeapon);
                }
                finally
                {
                    _isExecutingDelayedShow = false;
                }
            }
            catch (Exception ex)
            {
                _isExecutingDelayedShow = false;
                // ref: CR-01-01
                Plugin.Logger.LogError($"[MagCheckDelay] Erro na exibição atrasada: {ex}");
            }
        }

        public static void OnRaidEnd()
        {
            if (_activeRoutine != null && Plugin.Instance != null)
            {
                Plugin.Instance.StopCoroutine(_activeRoutine);
            }
            _activeRoutine = null;
            _isExecutingDelayedShow = false;
        }
    }
}
