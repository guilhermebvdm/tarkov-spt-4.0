using System;
using System.Collections.Generic;
using System.Reflection;
using EFT;
using HarmonyLib;
using UnityEngine;

namespace TRLImmersiveCombatMedicine.Trauma
{
    /// <summary>Primitiva COMPARTILHADA de agachar involuntário (003; 006 reusa). One-shot SÓ-PARA-BAIXO,
    /// sem lock (decisão 5): agacha via SetPoseLevel(0f) SEM force (animação vanilla) e o jogador levanta
    /// livre em seguida. Guards D7 3 eixos com adiamento + re-validação + cancelamento (spec 003 §5).</summary>
    internal static class TraumaPose
    {
        /// <summary>Fila de adiados com DEDUP por (player, kind). No HIT de dedup a entrada existente é ATUALIZADA
        /// (PublishDeadline + RequiredLine) — não é no-op: senão o cancel não devolve o cooldown do RE-publish
        /// (review 2 do 003, achado 2). O cancel só devolve cooldown se o stamp corrente ainda for o
        /// PublishDeadline guardado (re-ancorado não é apagado — review 1, achado 6).</summary>
        private static readonly List<DeferredCrouch> _deferred = new List<DeferredCrouch>();
        private static readonly List<BotRestore> _botRestores = new List<BotRestore>();

        private struct DeferredCrouch
        {
            internal Player Player;
            internal TraumaOneShotKind Kind;   // dedup por (player, kind) — primitiva compartilhada (006 reusa)
            internal TraumaRegion Region;
            internal TraumaLine RequiredLine;  // re-validação na execução: mudou → cancela
            internal float PublishDeadline;    // p/ ReportOneShotCanceled(player, kind, publishDeadline)
        }

        private struct BotRestore
        {
            internal Player Player;
            internal float RestoreAt;
        }

        // tarkin-ladders soft-dependency (guard de escada — P4 rec. (3c); string de tipo de terceiro)
        private static bool _ladderResolved;
        private static Type _ladderType;

        // SAIN soft-dependency (dip em combate — P6 rec. (7); null-check + no-op, padrão AggroHelper)
        private static bool _sainResolved;
        private static Type _sainBotComponentType;
        private static PropertyInfo _sainMoverProp;
        private static PropertyInfo _sainPoseProp;
        private static MethodInfo _sainSetTargetPose;

        /// <summary>Guards D7 (3 eixos, TODOS adiam — nunca executam em contexto inválido).</summary>
        internal static bool CanForcePose(Player p, out string blockedBy)
        {
            var mc = p.MovementContext;
            // (a) vault/ar — ref: P4 rec. (3a); MovementContext.IsGrounded (:1089) + CurrentState.Name (:732)
            if (!mc.IsGrounded) { blockedBy = "airborne"; return false; }
            var cs = mc.CurrentState;
            if (cs != null)
            {
                EPlayerState st = cs.Name;
                if (st == EPlayerState.ClimbOver || st == EPlayerState.ClimbUp
                    || st == EPlayerState.VaultingFallDown || st == EPlayerState.VaultingLanding
                    || st == EPlayerState.Jump || st == EPlayerState.FallDown)
                {
                    blockedBy = st.ToString();
                    return false;
                }
            }
            // (b) BTR — ref: Player.cs:25413
            if (p.BtrState != EPlayerBtrState.Outside) { blockedBy = "btr"; return false; }
            // (c) escada — tarkin-ladders por reflection (vanilla não tem escada interativa; P4 correção D7)
            ResolveLadderType();
            if (_ladderType != null && p.GetComponent(_ladderType) != null) { blockedBy = "ladder"; return false; }
            blockedBy = null;
            return true;
        }

        /// <summary>Agachar one-shot de HUMANO: pose já ≤ agachado/prone → NO-OP devolvendo o cooldown do publish
        /// (funcional §3 — "sem consumir cooldown"); guard falhou → ADIA; ok → SetPoseLevel(0f) sem force.</summary>
        internal static void TryInvoluntaryCrouch(Player p, TraumaRegion region, TraumaOneShotKind kind)
        {
            if (p == null || p.MovementContext == null)
            {
                // code-review 1 do 003, achado 4: contexto morto = one-shot nunca executará — refundar o
                // cooldown do publish (mesmo padrão do NOOP) antes de sair
                if (!(p is null) && TraumaEngine.TryGetOneShotDeadline(p, kind, out float dNull))
                    TraumaEngine.ReportOneShotCanceled(p, kind, dNull);
                return;
            }
            var mc = p.MovementContext;
            if (mc.IsInPronePose || mc.PoseLevel <= 0.05f) // ref: MovementContext.cs:1016 — 0=agachado, 1=de pé
            {
                if (TraumaEngine.TryGetOneShotDeadline(p, kind, out float d0))
                    TraumaEngine.ReportOneShotCanceled(p, kind, d0);
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] crouch NOOP (pose already low) {p.ProfileId}");
                return;
            }
            if (!CanForcePose(p, out string blockedBy))
            {
                Defer(p, region, kind, blockedBy);
                return;
            }
            if (!mc.SetPoseLevel(0f)) // ref: MovementContext.cs:2139 — sem force: animação vanilla; guard interno pode recusar
            {
                Defer(p, region, kind, "internal-guard");
                return;
            }
            TraumaEngine.ReportOneShotExecuted(p, kind); // D7 — cooldown conta da EXECUÇÃO
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] crouch EXECUTED {p.ProfileId}");
        }

        private static void Defer(Player p, TraumaRegion region, TraumaOneShotKind kind, string reason)
        {
            TraumaEngine.TryGetOneShotDeadline(p, kind, out float deadline);
            TraumaLine required = TraumaEngine.GetLine(p, region);
            for (int i = 0; i < _deferred.Count; i++)
            {
                DeferredCrouch e = _deferred[i];
                if (ReferenceEquals(e.Player, p) && e.Kind == kind)
                {
                    // review 2, achado 2: hit de dedup ATUALIZA a entrada (re-publish traz stamp/linha novos)
                    e.PublishDeadline = deadline;
                    e.RequiredLine = required;
                    _deferred[i] = e;
                    TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] crouch DEFERRED ({reason}) {p.ProfileId}");
                    return;
                }
            }
            _deferred.Add(new DeferredCrouch { Player = p, Kind = kind, Region = region, RequiredLine = required, PublishDeadline = deadline });
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] crouch DEFERRED ({reason}) {p.ProfileId}");
        }

        /// <summary>Pump 1×/frame (chamado pelo consumidor): re-checa guards; contexto válido → RE-VALIDA o snapshot
        /// (GetLine == RequiredLine) — mudou (curado/analgésico) → CANCELA devolvendo cooldown do publish.</summary>
        internal static void PumpDeferred()
        {
            for (int i = _deferred.Count - 1; i >= 0; i--)
            {
                DeferredCrouch e = _deferred[i];
                Player p = e.Player;
                if (p is null || p.MovementContext == null || TraumaEngine.GetLine(p, e.Region) != e.RequiredLine)
                {
                    _deferred.RemoveAt(i);
                    if (!(p is null)) TraumaEngine.ReportOneShotCanceled(p, e.Kind, e.PublishDeadline);
                    TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo(
                        $"[Trauma2] crouch CANCELED (state-changed) {(p is null ? "?" : p.ProfileId)}");
                    continue;
                }
                var mc = p.MovementContext;
                if (mc.IsInPronePose || mc.PoseLevel <= 0.05f)
                {
                    // agachou/deitou por conta própria enquanto adiado — intent satisfeita sem forçar (só-para-baixo)
                    _deferred.RemoveAt(i);
                    TraumaEngine.ReportOneShotCanceled(p, e.Kind, e.PublishDeadline);
                    TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] crouch NOOP (pose already low) {p.ProfileId}");
                    continue;
                }
                if (!CanForcePose(p, out _)) continue;   // segue adiado
                if (!mc.SetPoseLevel(0f)) continue;      // guard interno ainda recusa — segue adiado
                _deferred.RemoveAt(i);
                TraumaEngine.ReportOneShotExecuted(p, e.Kind); // cooldown conta da execução (D7)
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] crouch EXECUTED {p.ProfileId}");
            }
        }

        /// <summary>Cancela todos os adiados (toggle-off / fim de raid) devolvendo o cooldown do publish.</summary>
        internal static void CancelAll(string reason)
        {
            if (_deferred.Count == 0) return;
            for (int i = 0; i < _deferred.Count; i++)
            {
                DeferredCrouch e = _deferred[i];
                if (!(e.Player is null)) TraumaEngine.ReportOneShotCanceled(e.Player, e.Kind, e.PublishDeadline);
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo(
                    $"[Trauma2] crouch CANCELED ({reason}) {(e.Player is null ? "?" : e.Player.ProfileId)}");
            }
            _deferred.Clear();
        }

        /// <summary>Dip de BOT (P6 rec. (7)) — FIRE-AND-FORGET, nunca entra na fila de adiados (review 1, achado 4):
        /// devolução imediata de controle (decisão 16) com restauração própria fora de combate.</summary>
        internal static void BotCrouchDip(Player botPlayer)
        {
            if (botPlayer == null || botPlayer.MovementContext == null) return;
            BotOwner bo = botPlayer.AIData?.BotOwner;
            if (bo == null) return;
            var mcBot = botPlayer.MovementContext;
            if (mcBot.IsInPronePose || mcBot.PoseLevel <= 0.05f)
            {
                // code-review 1 do 003, achado 3: só-para-baixo vale p/ bot também — pose já baixa é NOOP,
                // devolve o cooldown do publish e NÃO agenda restore (não há dip a desfazer)
                if (TraumaEngine.TryGetOneShotDeadline(botPlayer, TraumaOneShotKind.InvoluntaryCrouch, out float d0))
                    TraumaEngine.ReportOneShotCanceled(botPlayer, TraumaOneShotKind.InvoluntaryCrouch, d0);
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] bot dip NOOP (pose already low) {botPlayer.ProfileId}");
                return;
            }
            bool sain = TrySainSetTargetPose(botPlayer, 0f);      // em combate o SAIN dirige a pose — target via reflection
            bo.SetPose(0f);                                        // ref: BotOwner.cs:1120 — target de pose da IA vanilla
            botPlayer.MovementContext.SetPoseLevel(0f, true);      // dip imediato no dono (host/headless)
            float dip = Mathf.Clamp(TRLImmersiveCombatMedicinePlugin.ConfigBotCrouchDipSeconds.Value, 0.3f, 1.5f);
            _botRestores.Add(new BotRestore { Player = botPlayer, RestoreAt = Time.time + dip });
            TraumaEngine.ReportOneShotExecuted(botPlayer, TraumaOneShotKind.InvoluntaryCrouch); // cooldown vale p/ bot
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo(
                $"[Trauma2] bot dip {botPlayer.ProfileId} mode={(sain ? "sain" : "vanilla")}");
        }

        /// <summary>Restauração do dip (fora de combate; em combate SAIN reescreve antes). Pump pelo consumidor.</summary>
        internal static void PumpBotRestores()
        {
            for (int i = _botRestores.Count - 1; i >= 0; i--)
            {
                BotRestore r = _botRestores[i];
                if (Time.time < r.RestoreAt) continue;
                _botRestores.RemoveAt(i);
                RestoreBot(r.Player);
            }
        }

        /// <summary>Restaura imediatamente todos os dips pendentes (toggle-off).</summary>
        internal static void FlushBotRestores()
        {
            for (int i = _botRestores.Count - 1; i >= 0; i--) RestoreBot(_botRestores[i].Player);
            _botRestores.Clear();
        }

        internal static void ClearBotRestores()
        {
            _botRestores.Clear(); // mundo morto — sem restore (objetos destruídos)
        }

        private static void RestoreBot(Player p)
        {
            if (p is null || p == null || p.MovementContext == null) return; // gerenciado E fake-null
            if (p.HealthController == null || !p.HealthController.IsAlive) return;
            p.AIData?.BotOwner?.SetPose(1f); // devolução — SAIN/BotMover re-decidem em seguida (decisão 16)
            TrySainSetTargetPose(p, 1f);
        }

        private static void ResolveLadderType()
        {
            if (_ladderResolved) return;
            _ladderResolved = true;
            _ladderType = Type.GetType("tarkin.ladders.bep.PlayerLadderController, tarkin.ladders.bep"); // ref: P4 rec. (3c)
            if (_ladderType != null) return;
            // P4 risco 3: warn quando o mod está na load order e a string de tipo quebrou (update do tarkin-ladders).
            // code-review 1 do 003, achado 5: match restrito a "ladders" no GUID — o OR "tarkin" dava
            // falso-positivo com outros mods do mesmo autor.
            foreach (var kv in BepInEx.Bootstrap.Chainloader.PluginInfos)
            {
                string key = kv.Key ?? string.Empty;
                if (key.IndexOf("ladders", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning(
                        "[Trauma2] tarkin-ladders presente mas PlayerLadderController não resolveu — guard de escada INATIVO");
                    break;
                }
            }
        }

        private static bool TrySainSetTargetPose(Player p, float pose)
        {
            try
            {
                ResolveSain();
                if (_sainBotComponentType == null || _sainSetTargetPose == null) return false;
                Component comp = p.gameObject.GetComponent(_sainBotComponentType);
                if (comp == null) return false;
                object mover = _sainMoverProp?.GetValue(comp);
                object poseObj = mover != null ? _sainPoseProp?.GetValue(mover) : null;
                if (poseObj == null) return false;
                _sainSetTargetPose.Invoke(poseObj, new object[] { pose });
                return true;
            }
            catch
            {
                return false; // soft-dep: qualquer quebra de shape do SAIN vira no-op silencioso (padrão AggroHelper)
            }
        }

        private static void ResolveSain()
        {
            if (_sainResolved) return;
            _sainResolved = true;
            // ref: P6 rec. (7) — nomes estáveis no SAIN 4.4.3 instalado; major update → no-op (risco 7 do P6)
            _sainBotComponentType = AccessTools.TypeByName("SAIN.Components.BotComponent");
            if (_sainBotComponentType == null) return;
            _sainMoverProp = AccessTools.Property(_sainBotComponentType, "Mover");
            Type moverType = _sainMoverProp?.PropertyType;
            _sainPoseProp = moverType != null ? AccessTools.Property(moverType, "Pose") : null;
            Type poseType = _sainPoseProp?.PropertyType;
            _sainSetTargetPose = poseType != null ? AccessTools.Method(poseType, "SetTargetPose") : null;
        }
    }
}
