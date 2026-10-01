using System;
using System.Reflection;
using EFT;
using EFT.HealthSystem;

namespace TRLImmersiveCombatMedicine.Trauma
{
    /// <summary>Primitiva do TREMOR GERENCIADO (item 005 — P2): no máximo 1 instância por processo
    /// (efeito só no humano local — bots excluídos, funcional 5). Remoção SEMPRE pela própria instância
    /// (NUNCA method_15/16 — FirstOrDefault come o tremor do Pain/stim, AHC:3615-3634).</summary>
    internal static class TraumaTremor
    {
        // Reflection cacheada com FAIL-FAST (csharp-best-practices §3): resolver 1x; falha → tremor degrada
        // p/ no-op logado como ERROR e o resto do 005 (ADS-cancel/lockout) segue — premissa P-005-B.
        private static MethodInfo _addTremor;   // AddEffect<Tremor> fechado — ref: P2 prova runtime (GetMethod("AddEffect") não-ambíguo: único público, AHC:3514)
        private static bool _resolveTried, _resolveOk;

        /// <summary>Instância gerenciada + dono + âncora — consumidos pelo postfix do PWA (ArmsAimPatches)
        /// e pela re-âncora pós-cura (PA-01-06).</summary>
        internal static ActiveHealthController.GClass3008 Owned;  // nested PÚBLICO — ref: AHC:29,219-233 (P2 evid.)
        internal static Player OwnedPlayer;
        internal static EBodyPart OwnedAnchor;                    // parte da instância viva (re-âncora — PA-01-06)

        internal static bool EnsureResolved()
        {
            if (_resolveTried) return _resolveOk;
            _resolveTried = true;
            try
            {
                // ref: AHC:3117-3122 — nested "Tremor" protected (nome estável, nunca GClassNNNN hardcoded — AP-03)
                Type tremor = typeof(ActiveHealthController).GetNestedType("Tremor", BindingFlags.NonPublic);
                // ref: AHC:3514 — ÚNICO AddEffect público do tipo (o AddEffect(T) de GClass3009:107 é de nested
                // Class2234, fora do lookup); GetMethod sem overload-ambiguidade verificado no decompile real
                MethodInfo open = typeof(ActiveHealthController).GetMethod("AddEffect");
                if (tremor != null && open != null && open.IsGenericMethodDefinition)
                    _addTremor = open.MakeGenericMethod(tremor);
                _resolveOk = _addTremor != null;
            }
            catch (Exception ex)
            {
                _resolveOk = false;
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogError($"[Trauma2] tremor reflection FAILED — tremor disabled: {ex.Message}");
                return false;
            }
            if (!_resolveOk)
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogError("[Trauma2] tremor reflection FAILED — tremor disabled (nested Tremor/AddEffect não resolvidos)");
            return _resolveOk;
        }

        /// <summary>Aplica se não há instância viva (Owned==null || !Owned.Existing — GInterface331 empilha, AHC:3514-3538).
        /// delayTime=0f OBRIGATÓRIO (DefaultDelay=15s do globals — AHC:3119); workTime=null=∞ (AHC:383,462-466).
        /// armAnchor = braço COMPROMETIDO (escapa do lookup method_16&lt;Tremor&gt;(Head) de stim — AHC:2789-2806).
        /// Instância viva com âncora DIVERGENTE e parte antiga CURADA → re-âncora (PA-01-06).</summary>
        internal static void Apply(Player p, EBodyPart armAnchor)
        {
            if (p is null || !EnsureResolved()) return;
            if (Owned != null && Owned.Existing && ReferenceEquals(OwnedPlayer, p))
            {
                // Idempotência + re-âncora (PA-01-06): cura parcial moveu o braço comprometido — RestoreBodyPart
                // NÃO remove o efeito (só sync — AHC:3891-3907/4573) e a idempotência sozinha deixaria a âncora
                // num braço SAUDÁVEL (ícone da UI de saúde errado + alcance de method_16<Tremor>(parte) de
                // terceiros). Com a parte antiga AINDA comprometida (ambos os braços), mantém — sem churn.
                IHealthController hc = p.HealthController;
                bool oldStillCompromised = hc != null
                    && (hc.IsBodyPartDestroyed(OwnedAnchor) || hc.IsBodyPartBroken(OwnedAnchor));
                if (armAnchor == OwnedAnchor || oldStillCompromised) return;
                Remove("re-anchor"); // AddEffect força-remove o residued da mesma parte antes do Create (AHC:3533-3537)
            }
            ActiveHealthController ahc = p.ActiveHealthController; // ref: Player.cs:25291 (propriedade pública)
            if (ahc == null) return; // edge transiente (nunca no dono — IsOwnedHere) NÃO degrada a sessão
            try
            {
                // PA-01-04: Apply roda dentro do dispatch DESPROTEGIDO do motor (StateChanged?.Invoke —
                // TraumaEngine.cs:565); exceção escapando abortaria a consolidação do frame p/ TODOS os
                // consumidores — degrada SÓ o tremor (P-005-B).
                Owned = (ActiveHealthController.GClass3008)_addTremor.Invoke(
                    ahc, new object[] { armAnchor, 0f, null, null, null, null });
                OwnedPlayer = p;
                OwnedAnchor = armAnchor;
            }
            catch (Exception ex)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogError($"[Trauma2] tremor APPLY FAILED — tremor disabled: {ex}");
                _resolveOk = false;
                Owned = null;
                OwnedPlayer = null;
                return;
            }
            // Edge de flag sem update — ref: P2 rec. (b); o postfix do PWA só roda em MUDANÇA de condição e o
            // campo é consumido por frame (BreathEffector.cs:74,182)
            if (p.ProceduralWeaponAnimation != null)
                p.ProceduralWeaponAnimation.Breath.TremorOn = true;
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo(
                $"[Trauma2] arms tremor ON {p.ProfileId} anchor={armAnchor} pk={TraumaEngine.IsUnderPainkiller(p)}");
        }

        /// <summary>Remove a PRÓPRIA instância: ForceResidue() (fade 0.2s — AHC:550-570), NUNCA method_15/16
        /// (FirstOrDefault come tremor do Pain/stim — AHC:3615-3634). Roda inclusive com IsAlive=false
        /// (downed Fika revive com o MESMO AHC — lição CR-02 do 003). Usar em state-exit/rebaixamento p/ None/
        /// toggle-off/re-âncora — mundo morto usa Discard (PA-01-01). Membros gerenciados: seguro pós-destroy.</summary>
        internal static void Remove(string reason)
        {
            ActiveHealthController.GClass3008 owned = Owned;
            Player p = OwnedPlayer;
            Owned = null;       // anular ANTES do ForceResidue (PA-01-05): o EffectResidualEvent dispara SÍNCRONO
            OwnedPlayer = null; // dentro dele — com Owned==null o watchdog falha o ReferenceEquals e a remoção
                                // PRÓPRIA não latcha pending espúrio
            if (owned == null) return;
            try
            {
                if (owned.Existing) owned.ForceResidue();
            }
            catch (Exception ex)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogError($"[Trauma2] tremor REMOVE FAILED — tremor disabled: {ex}");
                _resolveOk = false; // PA-01-04 (degradação P-005-B)
                return;
            }
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo(
                $"[Trauma2] arms tremor OFF {(p is null ? "?" : p.ProfileId)} ({reason})");
        }

        /// <summary>Descarte bookkeeping-only (raid-end/world-swap — PA-01-01): efeito/AHC morreram com o
        /// Player — ForceResidue aqui dispararia method_0 → eventos do AHC destruído + sync de rede
        /// (AHC:462-476; method_36 = SendNetworkSyncPacket, AHC:4573) no meio do teardown da sessão Fika.
        /// Padrão do 003 (TraumaLegsConsumer.cs:180-199: "só limpar bookkeeping").</summary>
        internal static void Discard(string reason)
        {
            if (Owned == null && OwnedPlayer == null) return;
            Owned = null;
            OwnedPlayer = null;
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] arms tremor DISCARD ({reason})");
        }

        /// <summary>Consulta do watchdog: o efeito que saiu é o NOSSO?</summary>
        internal static bool IsOurs(IEffect e)
        {
            return Owned != null && ReferenceEquals(e, Owned);
        }
    }
}
