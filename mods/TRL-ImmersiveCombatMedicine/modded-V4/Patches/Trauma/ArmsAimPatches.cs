using System.Reflection;
using EFT;
using EFT.Animations;
using HarmonyLib;

namespace TRLImmersiveCombatMedicine.Trauma
{
    /// <summary>Lockout de re-ADS (decisão 17 — P9): prefix no funil ÚNICO de mira. Todas as rotas de
    /// re-entrada convergem aqui (input/ToggleAim :13701, scope :13705-13709, fast-slot :10670-10673,
    /// pós-overlap :13067-13070). Skip não emite pacote → invisível ao peer (P9 corrigido).</summary>
    [HarmonyPatch]
    internal static class SetAimLockoutPatch
    {
        // Alvo por assinatura (não GClassNNNN — AP-03): mesmo lookup que RecoilRework/FOVFix resolvem em produção (IL — P9)
        private static readonly MethodBase Target =
            AccessTools.Method(typeof(Player.FirearmController), "SetAim", new[] { typeof(bool) });
        private static readonly FieldInfo PlayerField =
            AccessTools.Field(typeof(Player.FirearmController), "_player"); // cacheado (mesmo acesso do FOVFix — P9; declarado em ItemHandsController, AccessTools sobe a hierarquia)

        static MethodBase TargetMethod() { return Target; }

        static bool Prefix(Player.FirearmController __instance, bool value)
        {
            try
            {
                if (!value) return true; // saída de mira NUNCA bloqueada (nosso cancel e o desmaio passam livres — AP-07)
                var p = PlayerField.GetValue(__instance) as Player;
                // AP-03 auditado: FikaClientFirearmController chama base (humano local COBERTO — fika :216-227);
                // BotFirearmController/AIFirearmController descem à base SEM override → guard obrigatório
                // (bots excluídos — D9/funcional 5); ObservedFirearmController seta IsAiming sobrescrito sem
                // base.SetAim (espelhos nunca chegam).
                if (p == null || !p.IsYourPlayer || p.IsAI) return true;
                return !TraumaArmsConsumer.TryBlockReAds(p); // true=bloqueado → skip (postfixes RecoilRework/FOVFix ainda rodam — idempotentes, D13)
            }
            catch (System.Exception ex)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogError($"[Trauma2] SetAimLockoutPatch: {ex}");
                return true; // nunca engolir o original por exceção nossa
            }
        }
    }

    /// <summary>Contorno da supressão VISUAL do analgésico (P2 rec. (e)): o corpo vanilla força
    /// Breath.TremorOn=false sob OnPainkillers (PWA:1182-1186) atingindo QUALQUER tremor, inclusive o nosso.
    /// Re-assert SÓ da instância gerenciada — nunca pelo flag EPhysicalCondition.Tremor (zumbi/stim têm gate
    /// intencional). Corpo = 1 write de bool; roda só em MUDANÇA de condição (fora do hot path por frame).</summary>
    [HarmonyPatch(typeof(ProceduralWeaponAnimation), nameof(ProceduralWeaponAnimation.PhysicalConditionUpdated))] // ref: PWA:1175 (público, instância, não-virtual — sem superfície AP-03)
    internal static class TremorVisualReassertPatch
    {
        static void Postfix(ProceduralWeaponAnimation __instance)
        {
            try
            {
                if (TraumaTremor.Owned == null || !TraumaTremor.Owned.Existing) return; // Existing = Added|Started (AHC:219-233)
                //   PA-01-03: gate por Active (só Started) deixava descoberto o gap Added→Started de até 1 tick —
                //   um PhysicalConditionUpdated nesse frame com OnPainkillers forçaria TremorOn=false sem re-assert,
                //   e o Started seguinte NÃO re-dispara o evento se o flag Tremor já era true (tremor-por-dor
                //   coexistindo — AC-2). Residued segue fora (fade não re-asserta); delay=0 → Added dura <=1 tick.
                Player p = TraumaTremor.OwnedPlayer;
                if (p == null || !ReferenceEquals(__instance, p.ProceduralWeaponAnimation)) return; // PWA de bot/espelho nunca casa
                __instance.Breath.TremorOn = true; // ref: BreathEffector.cs:74 (campo público); shake = TremorOn || Fracture (OR — sem dupla intensidade, :182)
            }
            catch (System.Exception ex)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogError($"[Trauma2] TremorVisualReassertPatch: {ex}");
            }
        }
    }
}
