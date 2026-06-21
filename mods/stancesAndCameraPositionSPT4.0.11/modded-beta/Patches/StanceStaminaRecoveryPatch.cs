using System;
using System.Reflection;
using Comfort.Common;
using EFT;
using HarmonyLib;
using SPT.Reflection.Patching;

namespace CameraRotationMod.Patches
{
    /// <summary>
    /// Postfix em PlayerPhysicalClass.GetHandsRestorationFunc — quando StaminaMultiplier ≠ 1.0
    /// (mod ativo), zera o resultado vanilla para o tick do StanceManager controlar o delta.
    /// Preserva comportamento vanilla em ADS, prone-suspenso e bots (Player_0 check).
    /// ref: Assembly-CSharp/PlayerPhysicalClass.cs:1022
    /// ref: fix-01 — substitui Mode/Intensity pelo StaminaMultiplier unificado.
    /// </summary>
    public class StanceStaminaRecoveryPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(
                typeof(PlayerPhysicalClass),
                nameof(PlayerPhysicalClass.GetHandsRestorationFunc));
        }

        [PatchPostfix]
        [HarmonyPriority(Priority.Low)] // rodar depois de outros mods de stamina
        private static void Postfix(PlayerPhysicalClass __instance, ref float __result)
        {
            try
            {
                var gw = Singleton<GameWorld>.Instance;
                if (gw?.MainPlayer == null) return;
                if (gw.MainPlayer is HideoutPlayer && Plugin._DebugApplyInHideout?.Value != true) return;
                if (__instance.Player_0 != gw.MainPlayer) return;    // só MainPlayer (ignorar bots)

                // Quando Multiplier ≠ 1.0 o tick controla o delta de stamina diretamente.
                // Zeramos o vanilla aqui para evitar que ele interfira (soma indesejada).
                // Em ADS e prone-suspenso preservamos o comportamento vanilla do EFT.
                // ref: fix-01 — StaminaMode+Intensity unificados em StaminaMultiplier.
                // Mount ativo (vanilla) ou passivo (item 011): poupar stamina de braço — INDEPENDENTE do
                // stance multiplier. Ativo poupa mais; passivo, ~metade (PA-01-04). Passivo cede ao ativo
                // (IsBracing é limpo quando IsMountedState).
                var player = gw.MainPlayer;
                bool ads = player.ProceduralWeaponAnimation?.IsAiming == true;

                // Coordenador (fix-06-01): UMA fonte controla a stamina de braço por frame — Perfil B.
                // O consumo de aim-drain é zerado nos modos de mount (HandsStaminaConsumePatch), então o
                // resultado é determinístico: só a restauração abaixo decide drena/parado/recupera.
                switch (ArmStamina.Resolve(player))
                {
                    case ArmStaminaMode.MountActive:
                        // hipfire = recupera forte; ADS = recupera leve
                        __result = ads ? Plugin._PassiveMountStaminaRegen.Value
                                       : Plugin._ActiveMountStaminaRegen.Value;
                        return;
                    case ArmStaminaMode.MountPassive:
                        // hipfire = recupera leve; ADS = parado (segura, sem regen)
                        __result = ads ? 0f : Plugin._PassiveMountStaminaRegen.Value;
                        return;
                    case ArmStaminaMode.HoldBreath:
                        __result = 0f; // drain via ApplyComplexRotationPatch; sem regen contrariando
                        return;
                    case ArmStaminaMode.StanceDrain:
                        __result = 0f; // o tick do StanceManager aplica o delta
                        return;
                    // Prone, Vanilla → não interferir (EFT controla)
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"[StanceStaminaRecoveryPatch] {ex}");
            }
        }
    }

    public class HandsStaminaConsumePatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass774), nameof(GClass774.Consume));
        }

        [PatchPrefix]
        private static bool Prefix(GClass774 __instance, ref float __result)
        {
            try
            {
                var gw = Singleton<GameWorld>.Instance;
                var mp = gw?.MainPlayer;
                if (mp?.Physical?.HandsStamina == __instance)
                {
                    // Prone (vanilla congela) e os modos de mount (controle via restauração) zeram o
                    // consumo de braço — torna o Perfil B determinístico (sem depender do regen vencer o
                    // aim-drain). fix-06-01.
                    var mode = ArmStamina.Resolve(mp);
                    if (mp.IsInPronePose
                        || mode == ArmStaminaMode.MountActive
                        || mode == ArmStaminaMode.MountPassive)
                    {
                        __result = 0f;
                        return false; // Ignora o método original
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"[HandsStaminaConsumePatch] {ex}");
            }
            return true;
        }
    }

    public class HandsStaminaProcessPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return AccessTools.Method(typeof(GClass774), nameof(GClass774.Process));
        }

        [PatchPrefix]
        private static void Prefix(GClass774 __instance, ref bool __state)
        {
            try
            {
                var gw = Singleton<GameWorld>.Instance;
                if (gw?.MainPlayer?.Physical?.HandsStamina == __instance && gw.MainPlayer.IsInPronePose)
                {
                    __state = __instance.ForceMode;
                    __instance.ForceMode = true;
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"[HandsStaminaProcessPatch Prefix] {ex}");
            }
        }

        [PatchPostfix]
        private static void Postfix(GClass774 __instance, bool __state)
        {
            try
            {
                var gw = Singleton<GameWorld>.Instance;
                if (gw?.MainPlayer?.Physical?.HandsStamina == __instance && gw.MainPlayer.IsInPronePose)
                {
                    __instance.ForceMode = __state;
                }
            }
            catch (Exception ex)
            {
                Plugin.Logger.LogError($"[HandsStaminaProcessPatch Postfix] {ex}");
            }
        }
    }
}
