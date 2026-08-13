using HarmonyLib;
using EFT;
using EFT.InputSystem;
using UnityEngine;
using TRLImmersiveCombatMedicine;
using TRLImmersiveCombatMedicine.Trauma;

namespace TrueTrauma
{
    [HarmonyPatch(typeof(EFT.GamePlayerOwner), "TranslateAxes")]
    class FreezeAxesPatch
    {
        static void Prefix(EFT.GamePlayerOwner __instance, ref float[] axes)
        {
            if (!TRLImmersiveCombatMedicinePlugin.ConfigMasterEnabled.Value || !TRLImmersiveCombatMedicinePlugin.ConfigBlackoutEnabled.Value) return;

            if (__instance.Player != null && TraumaState.BlackoutTimers.ContainsKey(__instance.Player.ProfileId))
                if (axes != null) for (int i = 0; i < axes.Length; i++) axes[i] = 0f;
        }
    }

    [HarmonyPatch(typeof(EFT.GamePlayerOwner), "TranslateCommand")]
    class FreezeCommandPatch
    {
        static bool Prefix(EFT.GamePlayerOwner __instance, ECommand command, ref InputNode.ETranslateResult __result)
        {
            if (!TRLImmersiveCombatMedicinePlugin.ConfigMasterEnabled.Value || !TRLImmersiveCombatMedicinePlugin.ConfigBlackoutEnabled.Value) return true;

            if (__instance.Player != null && TraumaState.BlackoutTimers.ContainsKey(__instance.Player.ProfileId))
            {
                if (command == ECommand.Escape) return true;
                __result = InputNode.ETranslateResult.BlockAll;
                return false;
            }
            return true;
        }
    }

    // ref: MovementContext.cs:3304 (virtual). Overrides auditados (AP-03): ObservedMovementContext.CanStandAt
    // => true sem base-call (fika ObservedMovementContext.cs:109-112) — espelhos imunes por construção;
    // ClientMovementContext/NoInertiaMovementContext NÃO sobrescrevem (dispatch cai na base patchada).
    [HarmonyPatch(typeof(MovementContext), "CanStandAt")]
    class CantStandUpPatch
    {
        static bool Prefix(MovementContext __instance, float h, ref bool __result)
        {
            if (!TRLImmersiveCombatMedicinePlugin.ConfigMasterEnabled.Value) return true;
            if (TraumaState.PlayerField == null) return true;

            try
            {
                Player player = TraumaState.PlayerField.GetValue(__instance) as Player;

                if (player != null && player.HealthController.IsAlive)
                {
                    string id = player.ProfileId;

                    if (TRLImmersiveCombatMedicinePlugin.ConfigBlackoutEnabled.Value && TraumaState.BlackoutTimers.ContainsKey(id))
                    {
                        __result = false;
                        return false;
                    }

                    // Trava de agachamento involuntário (1 perna quebrada): bloqueia subir de postura por 1s (1f)
                    if (TraumaPose.IsCrouchLocked(player) && h > __instance.PoseLevel + 0.05f)
                    {
                        __result = false;
                        return false;
                    }

                    // ref: spec 003 §4 (D10) — branches legados de pernas removidos (ImpactTimers 1 s;
                    // bloqueio de levantar 10 s humano / 90 s bot com 2 pernas zeradas): levantar não é mais
                    // travado pelo sistema legado — o ciclo de queda real é o branch abaixo (item 004).

                    // Branch do ciclo de queda (spec 004 §1.5) — NUNCA blanket-false (P5: quebraria CanInteract
                    // :1416, CanSit :1328 e o SetPoseLevel(force) do próprio mod :2149). Negação SÓ p/ o dono
                    // humano local em BLOQUEIO: prone nega qualquer h; fallback agachado nega SÓ SUBIR
                    // (h > PoseLevel — deixa passar os SetPoseLevel(0f) do mod e o mover agachado). Gate
                    // IsYourPlayer OBRIGATÓRIO (FikaBot passa pela base via SetPoseLevel :2149 — AP-03).
                    if (!TraumaFallCycleConsumer.StandReentryFlag
                        && player.IsYourPlayer
                        && TraumaFallCycleConsumer.IsBlockedPhase(player)
                        && (__instance.IsInPronePose || h > __instance.PoseLevel + 0.05f))
                    {
                        __result = false; // negação silenciosa — pose nunca muda (peers não veem nada)
                        return false;
                    }
                }
            }
            catch { }
            return true;
        }
    }

    /// <summary>Detecção de TENTATIVA de levantar no BLOQUEIO do ciclo (spec 004 §1.5 — som + anti-spam).
    /// NUNCA bloqueia nem muda __result — a imposição é o CanStandAt; comando não mapeado = no máximo um som
    /// perdido, nunca um levantar vazado. Coexiste com o FreezeCommandPatch (mesmo alvo; o dele retorna false
    /// no blackout — e o ciclo está PAUSED sob blackout). GamePlayerOwner só existe p/ o humano local (dono por
    /// construção); HideoutPlayerOwner tem TranslateCommand próprio — hideout fora do ciclo (PA-01-13).</summary>
    [HarmonyPatch(typeof(EFT.GamePlayerOwner), "TranslateCommand")] // ref: GamePlayerOwner.cs:801
    class FallAttemptCommandPatch
    {
        static void Prefix(EFT.GamePlayerOwner __instance, ECommand command)
        {
            try // PA-01-15: detecção-only nunca pode quebrar input
            {
                if (command != ECommand.ToggleProne && command != ECommand.ToggleDuck
                    && command != ECommand.Jump && command != ECommand.NextWalkPose) return; // ref: ECommand.cs:32/27/44/34
                Player p = __instance != null ? __instance.Player : null;
                if (p == null || !TraumaFallCycleConsumer.IsBlockedPhase(p)) return;
                TraumaVoice.PlayStrong(p); // anti-spam interno ≥2s — spam de input NÃO repete o som
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] fall attempt BLOCKED {p.ProfileId}");
            }
            catch (System.Exception ex)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogError($"[Trauma2] FallAttemptCommandPatch: {ex.Message}");
            }
        }
    }
}
