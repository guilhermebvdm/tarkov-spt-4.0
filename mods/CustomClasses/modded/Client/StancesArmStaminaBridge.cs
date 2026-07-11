using System;
using System.Reflection;
using Comfort.Common;
using EFT;
using HarmonyLib;

namespace CustomClasses.Client;

/// <summary>
///     Item 051 — ponte com o stances mod (opção (a), decisão do usuário): o stances expõe
///     <c>CameraRotationMod.StaminaController.ExternalHandsDrainMult</c> (contrato externo) e este bridge o
///     preenche por soft-detect (reflection, zero tipos do stances no IL — padrão FIKA/SAIN). O fator retorna:
///     Caçador MIRANDO → Steady Arms (default ×0.65) · Tanque com ARMA PESADA em mãos → Tireless Arms (default
///     ×0.20 desde o B16; era ×0 = imunidade absoluta) · senão ×1. O stances aplica só no DRENO (delta&lt;0);
///     recuperação nunca é tocada.
///     PA-01-01: attach no Awake + re-try no raid-start (todos os plugins já carregados). PA-01-02: Player
///     resolvido aqui com null-guards; warn-once. PA-01-05: guard de NaN.
/// </summary>
internal static class StancesArmStaminaBridge
{
    private static bool _attached;
    private static bool _warnedMissing;
    private static bool _warnedFactorError;

    /// <summary>
    ///     Acopla o Factor ao hook do stances. Idempotente; chamado no Awake e re-tentado no raid-start.
    ///     (review CR-051-04) <paramref name="finalAttempt"/>: no Awake o miss é ESPERADO (ordem alfabética do
    ///     chainloader: "CustomClasses" &lt; "RealisticMobility") → só Info, sem consumir o warn-once; o WARNING
    ///     conclusivo ("instale/atualize") fica pro re-try do raid-start, quando todos os plugins já carregaram.
    /// </summary>
    public static void TryAttach(bool finalAttempt = false)
    {
        if (_attached)
        {
            return;
        }

        try
        {
            var t = AccessTools.TypeByName("CameraRotationMod.StaminaController");   // ref: stances Plugin.cs:14 (namespace real)
            var field = t != null ? AccessTools.Field(t, "ExternalHandsDrainMult") : null;
            if (field == null)
            {
                WarnUnavailable(finalAttempt, "hook do stances indisponível — Steady Arms/Tireless Arms inativos (instale/atualize o stances mod).");
                return;
            }

            field.SetValue(null, new Func<float>(Factor));   // Func<float> é tipo do mscorlib — estável cross-assembly
            _attached = true;
            Plugin.Log?.LogInfo("[CustomClasses] (051) fator de stamina de braço acoplado ao stances (Steady Arms/Tireless Arms ativos).");
        }
        catch (Exception ex)
        {
            WarnUnavailable(finalAttempt, $"attach no stances falhou: {ex.Message}");
        }
    }

    /// <summary>(review CR-051-04) Info no boot (miss esperado); Warning warn-once só no attempt conclusivo.</summary>
    private static void WarnUnavailable(bool finalAttempt, string message)
    {
        if (!finalAttempt)
        {
            Plugin.Log?.LogInfo("[CustomClasses] (051) stances ainda não carregado no boot — re-try no raid-start.");
            return;
        }

        if (!_warnedMissing)
        {
            _warnedMissing = true;
            Plugin.Log?.LogWarning($"[CustomClasses] (051) {message}");
        }
    }

    /// <summary>
    ///     Multiplicador de dreno de braço do frame (chamado 1×/frame de dreno pelo Tick do stances).
    ///     Função pura do estado atual (classe local + mira/arma) — sem cache (AP-08); exceção nunca
    ///     escapa pro Tick (warn-once + 1f).
    /// </summary>
    private static float Factor()
    {
        try
        {
            var p = Singleton<GameWorld>.Instance?.MainPlayer;
            if (p == null)
            {
                return 1f;
            }

            // 🔧 Steady Arms (Caçador): braço cansa menos AO MIRAR. IsAiming é o MESMO sinal que o Resolve()
            // do stances usa pros cenários *Ads — e HoldBreath implica IsAiming (evidência na review 01).
            if (PerksConfig.SteadyArmsEnabled?.Value == true && SkillMultipliers.IsLocalClass("Hunter")
                && p.ProceduralWeaponAnimation != null && p.ProceduralWeaponAnimation.IsAiming)
            {
                return Sane(PerksConfig.SteadyArmsDrain?.Value ?? 1f);
            }

            // 🔧 Tireless Arms (Tanque): braço cansa MUITO devagar com arma pesada EM MÃOS (B16: ×0.20; era ×0 = imune). Mesmo gate do Bunker.
            if (PerksConfig.TirelessArmsEnabled?.Value == true && SkillMultipliers.IsLocalClass("Tank")
                && HeavyWeapon.InHand(p))
            {
                return Sane(PerksConfig.TirelessArmsDrain?.Value ?? 1f);
            }

            return 1f;
        }
        catch (Exception ex)
        {
            if (!_warnedFactorError)
            {
                _warnedFactorError = true;
                Plugin.Log?.LogError($"[CustomClasses] (051) Factor falhou (desativando log repetido): {ex.Message}");
            }

            return 1f;
        }
    }

    /// <summary>PA-01-05: NaN de config manual não pode propagar pro hands.Current do stances.</summary>
    private static float Sane(float v) => float.IsNaN(v) ? 1f : v;
}
