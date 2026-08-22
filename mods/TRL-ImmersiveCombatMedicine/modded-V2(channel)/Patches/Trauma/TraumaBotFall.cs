using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using DrakiaXYZ.BigBrain.Brains; // ref: protótipo P6 compilado 0 erros (scratchpad/spike001/proto-traumadowned/)
using EFT;
using HarmonyLib;
using UnityEngine;

namespace TRLImmersiveCombatMedicine.Trauma
{
    /// <summary>Hold de bot do ciclo de queda (decisão 16 + P6): camada BigBrain prio 90 + BotLay + interop SAIN.
    /// Dono-only por construção (camada só existe onde BotOwner vive — host/headless; espelho sem brain).
    /// CICLO DE VIDA (PA-02-02): releases só MARCAM; quem REMOVE é o Stop() (ConsumeRelease, quando ForceGetUp)
    /// ou o sweep/ReleaseAll p/ entradas Released com camada já parada. Entrada Released com linha VIVA é
    /// retida de propósito — arma o RE-HOLD do Pump.
    /// Regra do arquivo: NENHUM método do TraumaBotFall fora de RegisterLayerCore referencia tipos BigBrain.</summary>
    internal static class TraumaBotFall
    {
        private sealed class Hold
        {
            internal Player Player;
            internal float ReleaseAt;
            internal bool Released;
            internal bool ForceGetUp;   // motivo do release (PA-01-08): cura/analgésico/toggle=true, X-expiry=false
            internal bool LayerStopped; // Stop() já consumiu esta entrada (ConsumeRelease force=false) — habilita
                                        //   o sweep "Released com linha morta" SEM corrida com um Stop() pendente
                                        //   (refinamento do "camada já parada" do PA-02-02/CR-01-02)
        }

        private static readonly Dictionary<string, Hold> _holds = new Dictionary<string, Hold>(); // por profileId

        /// <summary>ref: item 020 — holds de bot + camadas BigBrain pendentes.</summary>
        internal static int ResidualCount => _holds.Count + _brainPending.Count;
        private static readonly List<string> _scratch = new List<string>(); // reusada — sem alocação no caminho quente

        // CR-01-01: hold só nasce se a camada BigBrain EXISTE p/ o bot — lista única compartilhada com o
        // AddCustomLayer (RegisterLayerCore) + flag de registro; sem camada = sem hold (sem IsCycleEngaged
        // fantasma na arbitragem D2, sem churn RELEASE/RE-HOLD a cada X s).
        private static readonly List<string> LayerBrains = new List<string>
            { "PmcBear", "PmcUsec", "PMC", "Assault", "CursAssault", "ExUsec", "ArenaFighter", "Obdolbs" };
        private static readonly HashSet<string> LayerBrainSet = new HashSet<string>(LayerBrains);
        private static bool _layerRegistered; // setado SÓ no fim de RegisterLayerCore (BigBrain presente e registro ok)

        /// <summary>CR-02-01: transição chegou com Brain.BaseBrain ainda NÃO vinculado (bot adotado ferido na
        /// janela de spawn — o attach do BigBrain acontece no prefix de BaseBrain.Activate, no mesmo call stack
        /// da vinculação). Entrada PENDENTE re-checada no Pump com backoff curto até o BaseBrain vincular (aí
        /// decide hold/no-layer) ou o episódio sair da linha Cair.</summary>
        private sealed class BrainPending { internal Player Player; internal float NextCheckAt; }
        private static readonly Dictionary<string, BrainPending> _brainPending = new Dictionary<string, BrainPending>();
        private const float BrainPendingBackoff = 0.5f;

        /// <summary>Resultado da sondagem de camada: NoLayer = definitivo (sem BigBrain / brain fora da lista);
        /// Pending = BaseBrain ainda não vinculou (re-sondar); Ready = camada existe p/ o bot.</summary>
        private enum LayerProbe { NoLayer, Pending, Ready }

        // SAIN interop (ActiveLayer=None no Start do hold) — soft-dep por reflection (padrão TrySainSetTargetPose)
        private static bool _sainResolved;
        private static System.Type _sainBotComponentType;
        private static PropertyInfo _sainActiveLayerProp;

        /// <summary>Registro no Awake do plugin. Guard AQUI, tipos BigBrain SÓ no Core (PA-02-05): no Mono o JIT
        /// resolve os tokens do corpo INTEIRO na 1ª chamada — typeof(TraumaDownedLayer) neste método forçaria
        /// carregar CustomLayer (DLL do BigBrain) ANTES do guard → TypeLoadException derrubando o Awake.
        /// Ordem de load garantida pelo [BepInDependency("xyz.drakia.bigbrain", SoftDependency)] do plugin.
        /// BigBrain ausente → warn 1×, humano intacto (degradação graciosa).</summary>
        internal static void RegisterLayer()
        {
            if (!BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("xyz.drakia.bigbrain"))
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning(
                    "[Trauma2] BigBrain ausente — ciclo de queda de BOTS desativado (humano intacto)");
                return;
            }
            try { RegisterLayerCore(); }
            catch (System.TypeLoadException ex)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning($"[Trauma2] BigBrain incompatível (TypeLoad) — bots sem ciclo: {ex.Message}");
            }
            catch (System.IO.FileNotFoundException ex)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning($"[Trauma2] BigBrain não carregou (FileNotFound) — bots sem ciclo: {ex.Message}");
            }
        }

        /// <summary>ÚNICO método fora das classes de camada que toca tipos BigBrain; NoInlining impede o JIT de
        /// puxar os tokens p/ dentro do RegisterLayer (PA-02-05).</summary>
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void RegisterLayerCore()
        {
            BrainManager.AddCustomLayer(typeof(TraumaDownedLayer), LayerBrains,
                90); // ref: scratchpad bigbrain_BrainManager.cs:147-165; prioridades P6 (SAIN≤80, ORBIT 19, UNTAR 4/5, Exfil 79);
                     //   bosses/followers FORA no 004 (animações especiais — premissa p/ item 011, abertura 4)
            _layerRegistered = true; // CR-01-01: gate de nascimento de hold
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo("[Trauma2] bot fall layer registered (BigBrain prio 90)");
        }

        /// <summary>CR-01-01 + CR-02-01: a camada existe DE FATO p/ este bot? Mesmo critério que o BigBrain usa
        /// para anexar (brainNames.Contains(Brain.BaseBrain.ShortName()) — bigbrain ExcludeLayerHelpers.
        /// IsAffectedBySettings; roles = AllWildSpawnTypes no nosso registro, só o brain decide). BaseBrain ainda
        /// null (brain não ativou — janela de spawn/adoção) é caso DISTINTO de "fora da lista": Pending, não
        /// NoLayer (CR-02-01 — o attach do BigBrain vem no prefix de BaseBrain.Activate, ms depois).</summary>
        private static LayerProbe ProbeLayer(Player bot)
        {
            if (!_layerRegistered) return LayerProbe.NoLayer;
            BotOwner bo = bot.AIData?.BotOwner;
            if (bo == null) return LayerProbe.NoLayer;
            BaseBrain brain = bo.Brain?.BaseBrain; // ref: StandartBotBrain.cs:11; BaseBrain.cs:78
            if (brain == null) return LayerProbe.Pending;
            return LayerBrainSet.Contains(brain.ShortName()) ? LayerProbe.Ready : LayerProbe.NoLayer;
        }

        private static void CreateHold(Player bot, string id)
        {
            float x = X();
            _holds[id] = new Hold { Player = bot, ReleaseAt = Time.time + x }; // camada IsActive no próximo tick de IA
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] bot fall HOLD {id} x={x:0.#}");
        }

        internal static bool IsHeld(string profileId) =>
            profileId != null && _holds.TryGetValue(profileId, out Hold h) && !h.Released;

        /// <summary>Ponto ÚNICO de consumo do release — chamado SÓ pelo Stop() da camada (PA-02-02): lê ForceGetUp
        /// e, quando true (cura/analgésico/toggle-off), REMOVE a entrada (release final). ForceGetUp=false
        /// (X-expiry): entrada Released fica retida (LayerStopped=true) — arma o RE-HOLD do Pump.</summary>
        internal static bool ConsumeRelease(string profileId)
        {
            if (profileId == null || !_holds.TryGetValue(profileId, out Hold h)) return false;
            if (h.ForceGetUp)
            {
                _holds.Remove(profileId);
                return true;
            }
            h.LayerStopped = true;
            return false;
        }

        /// <summary>Caminho ÚNICO e idempotente de ENTRADA por transição (PA-02-06) — SEM depender de establishing.
        /// Cobre: estabelecedora (adoção/spawn ferido — PA-01-11, o motor não publica one-shot em establishing,
        /// TraumaEngine.cs:567), publish SUPRIMIDO por cooldown (re-quebra ≤3-5s pós-cura) e a publicada (o
        /// OnFallOneShot do mesmo frame só re-ancora o cooldown).</summary>
        internal static void OnLine(Player bot, TraumaLine to)
        {
            if (bot is null) return;
            string id = bot.ProfileId;
            if (id == null) return;
            if (to == TraumaLine.LegsFallCycle)
            {
                if (IsHeld(id)) return; // no-op (idempotente)
                // Guards: BotOwner/MovementContext nulos → no-op SEM refund (não há publish atrelado à transição)
                if (bot.MovementContext == null || bot.AIData?.BotOwner == null) return;
                switch (ProbeLayer(bot))
                {
                    case LayerProbe.Pending:
                        // CR-02-01: BaseBrain ainda não vinculou — entrada PENDENTE re-checada no Pump com
                        // backoff curto até vincular (decide hold/no-layer) ou o episódio sair da linha.
                        if (!_brainPending.ContainsKey(id))
                        {
                            _brainPending[id] = new BrainPending { Player = bot, NextCheckAt = Time.time + BrainPendingBackoff };
                            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] bot fall brain-pending {id}");
                        }
                        return;
                    case LayerProbe.NoLayer:
                        // CR-01-01: sem camada p/ este bot (BigBrain ausente / brain fora da lista) → SEM hold —
                        // ninguém deitaria o bot; hold fantasma distorceria IsCycleEngaged (D2) e geraria churn
                        // RELEASE/RE-HOLD. O publish do one-shot é refundado pelo OnFallOneShot não-held.
                        TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] bot fall no-layer {id}");
                        return;
                }
                _brainPending.Remove(id); // pendência antiga (se houver) resolvida por esta transição
                CreateHold(bot, id);
            }
            else
            {
                _brainPending.Remove(id); // CR-02-01: episódio saiu da linha — pendência morre
                // Cura/analgésico/None → MARCA release SEM re-hold ("a IA levanta sem re-derrubada" — PA-01-08);
                // a REMOÇÃO é do Stop()/sweep (PA-02-02).
                if (_holds.TryGetValue(id, out Hold h))
                {
                    h.Released = true;
                    h.ForceGetUp = true;
                }
            }
        }

        internal static void OnFallOneShot(Player bot)
        {
            if (bot is null) return;
            // Entrada NORMAL já foi tratada pelo OnLine do MESMO frame (StateChanged antes do publish — PA-02-06):
            if (IsHeld(bot.ProfileId))
            {
                TraumaEngine.ReportOneShotExecuted(bot, TraumaOneShotKind.InvoluntaryFall); // SÓ re-ancora o cooldown
                return;
            }
            // Não-held (guards do OnLine falharam — MovementContext/BotOwner nulos) → refund do publish
            // (padrão BotCrouchDip) — coerente: nenhum hold existe p/ contradizer o refund.
            if (TraumaEngine.TryGetOneShotDeadline(bot, TraumaOneShotKind.InvoluntaryFall, out float d))
                TraumaEngine.ReportOneShotCanceled(bot, TraumaOneShotKind.InvoluntaryFall, d);
        }

        /// <summary>Religar do toggle: bots com GetLine==FallCycle → hold estabelecedor sem one-shot (mesmo
        /// caminho idempotente do OnLine — PA-02-06).</summary>
        internal static void EstablishFromSnapshot(GameWorld gw)
        {
            if (gw == null || !_layerRegistered) return; // CR-01-01: sem camada registrada, nenhum hold nasce
            var players = gw.RegisteredPlayers;
            for (int i = 0; i < players.Count; i++)
            {
                var p = players[i] as Player;
                if (p == null || !p.IsAI || !TraumaEngine.IsOwnedHere(p)) continue;
                if (TraumaEngine.GetLine(p, TraumaRegion.Legs) == TraumaLine.LegsFallCycle)
                    OnLine(p, TraumaLine.LegsFallCycle);
            }
        }

        internal static void Pump()
        {
            PumpBrainPending(); // CR-02-01: pendências resolvem mesmo sem hold algum vivo
            if (_holds.Count == 0) return;
            _scratch.Clear();
            foreach (KeyValuePair<string, Hold> kv in _holds) _scratch.Add(kv.Key);
            for (int i = 0; i < _scratch.Count; i++)
            {
                string id = _scratch[i];
                if (!_holds.TryGetValue(id, out Hold h)) continue;
                Player p = h.Player;
                TraumaLine line = p is null ? TraumaLine.None : TraumaEngine.GetLine(p, TraumaRegion.Legs);
                // 3. Sweep: morto/despawn/GetLine==None (untrack do motor) → remove (CR-01-02: sem entrada órfã);
                //    TAMBÉM Released com linha != FallCycle E camada JÁ parada → remove (bot CURADO — nenhum
                //    Stop() virá; PA-02-02). Released com Stop() PENDENTE (LayerStopped=false) fica p/ o Stop()
                //    consumir (senão o GetUp(false) da cura nunca rodaria). NUNCA remove Released com linha VIVA
                //    (é ela que arma o re-hold do passo 2).
                if (p is null || p == null || line == TraumaLine.None)
                {
                    _holds.Remove(id);
                    continue;
                }
                if (h.Released && line != TraumaLine.LegsFallCycle && h.LayerStopped)
                {
                    _holds.Remove(id);
                    continue;
                }
                // 1. X-expiry → Released, ForceGetUp=false (devolve a DECISÃO à IA — PA-01-08; camada
                //    IsActive=false → árbitro devolve, D14); entrada RETIDA até o Stop()/sweep (PA-02-02)
                if (!h.Released && Time.time >= h.ReleaseAt)
                {
                    h.Released = true;
                    h.ForceGetUp = false;
                    TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] bot fall RELEASE (x-expiry) {id}");
                    continue;
                }
                // 2. Released && bot levantou && linha ainda FallCycle → RE-HOLD com X novo (interno, isento do
                //    cooldown do motor — sem publish)
                if (h.Released && !h.ForceGetUp && line == TraumaLine.LegsFallCycle)
                {
                    BotOwner bo = p.AIData?.BotOwner;
                    var mc = p.MovementContext;
                    if (bo?.BotLay == null || mc == null) continue; // despawn parcial — sweep pega no próximo tick
                    if (!bo.BotLay.IsLay && !mc.IsInPronePose)      // ref: scratchpad BotLay.cs:34-72
                    {
                        float x = X();
                        h.Released = false;
                        h.ForceGetUp = false;
                        h.LayerStopped = false;
                        h.ReleaseAt = Time.time + x;
                        TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] bot fall RE-HOLD {id} x={x:0.#}");
                    }
                }
            }
            _scratch.Clear();
        }

        /// <summary>CR-02-01: re-checa entradas brain-pending com backoff — BaseBrain vinculou → decide
        /// hold/no-layer (mesmo log/refund do caminho degradado definitivo); episódio saiu da linha ou bot
        /// morreu/despawnou → pendência morre.</summary>
        private static void PumpBrainPending()
        {
            if (_brainPending.Count == 0) return;
            _scratch.Clear();
            foreach (KeyValuePair<string, BrainPending> kv in _brainPending) _scratch.Add(kv.Key);
            for (int i = 0; i < _scratch.Count; i++)
            {
                string id = _scratch[i];
                if (!_brainPending.TryGetValue(id, out BrainPending pend)) continue;
                Player p = pend.Player;
                if (p is null || p == null || TraumaEngine.GetLine(p, TraumaRegion.Legs) != TraumaLine.LegsFallCycle)
                {
                    _brainPending.Remove(id); // morto/despawn/linha saiu — episódio encerrado
                    continue;
                }
                if (Time.time < pend.NextCheckAt) continue;
                switch (ProbeLayer(p))
                {
                    case LayerProbe.Pending:
                        pend.NextCheckAt = Time.time + BrainPendingBackoff; // segue aguardando o Activate
                        break;
                    case LayerProbe.NoLayer:
                        _brainPending.Remove(id); // definitivo — mesmo caminho degradado logado do OnLine
                        TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] bot fall no-layer {id}");
                        break;
                    case LayerProbe.Ready:
                        _brainPending.Remove(id);
                        if (!IsHeld(id)) CreateHold(p, id); // hold atrasado (shape estabelecedor — sem one-shot, PA-02-06)
                        break;
                }
            }
            _scratch.Clear();
        }

        /// <summary>Toggle-off (PA-02-02): entradas com camada possivelmente ATIVA → MARCA Released+ForceGetUp
        /// (o Stop() assíncrono do árbitro consome via ConsumeRelease no tick de IA seguinte e ELE remove —
        /// remover aqui faria o GetUp(false) nunca rodar, corner da funcional). Entradas JÁ Released e JÁ paradas
        /// (Stop() não virá de novo — NextPosibleGetUp já zerado no Stop() anterior) → remove DIRETO.</summary>
        internal static void ReleaseAll(string reason)
        {
            if (_holds.Count == 0) return;
            _scratch.Clear();
            foreach (KeyValuePair<string, Hold> kv in _holds) _scratch.Add(kv.Key);
            for (int i = 0; i < _scratch.Count; i++)
            {
                string id = _scratch[i];
                if (!_holds.TryGetValue(id, out Hold h)) continue;
                if (h.Released && h.LayerStopped)
                {
                    _holds.Remove(id);
                    continue;
                }
                h.Released = true;
                h.ForceGetUp = true;
            }
            _scratch.Clear();
            _brainPending.Clear(); // CR-02-01: toggle-off mata pendências (religar re-estabelece do snapshot)
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] bot fall RELEASE-ALL ({reason})");
        }

        internal static void ClearAll() // mundo morto — objetos destruídos, só bookkeeping (sem Stop())
        {
            _holds.Clear();
            _brainPending.Clear(); // CR-02-01
        }

        internal static float X() => Mathf.Clamp(TRLImmersiveCombatMedicinePlugin.ConfigBotFallHoldSeconds.Value, 5f, 120f); // internal: Start() da camada usa (PA-02-07)

        /// <summary>Interop SAIN: BotComponent.ActiveLayer = ESAINLayer.None via reflection — sem isso o SAIN
        /// atira/gira caído (estado stale; ref: SAIN BotComponent.cs:183-187, SAINActivationClass.cs:83-98).
        /// Soft-dep: shape mudou → no-op silencioso (padrão TrySainSetTargetPose).</summary>
        internal static void TrySainClearActiveLayer(BotOwner bo)
        {
            try
            {
                if (!_sainResolved)
                {
                    _sainResolved = true;
                    _sainBotComponentType = AccessTools.TypeByName("SAIN.Components.BotComponent");
                    if (_sainBotComponentType != null)
                        _sainActiveLayerProp = AccessTools.Property(_sainBotComponentType, "ActiveLayer");
                }
                if (_sainBotComponentType == null || _sainActiveLayerProp == null) return;
                Player p = bo?.GetPlayer;
                if (p == null) return;
                Component comp = p.gameObject.GetComponent(_sainBotComponentType);
                if (comp == null) return;
                _sainActiveLayerProp.SetValue(comp, System.Enum.ToObject(_sainActiveLayerProp.PropertyType, 0)); // ESAINLayer.None == 0
            }
            catch
            {
                // soft-dep — qualquer quebra de shape vira no-op silencioso
            }
        }
    }

    /// <summary>Camada BigBrain do hold (prio 90 preempta SAIN 20/22/24/69/70/80, ORBIT 19, UNTAR 4/5, Exfil 79
    /// — P6 evid.). Árbitro re-decide por tick (D14): IsActive() consulta o manager.</summary>
    internal class TraumaDownedLayer : CustomLayer
    {
        public TraumaDownedLayer(BotOwner botOwner, int priority) : base(botOwner, priority) { }

        public override string GetName() => "TraumaDowned";

        public override bool IsActive() => TraumaBotFall.IsHeld(BotOwner != null ? BotOwner.ProfileId : null);

        public override Action GetNextAction() => new Action(typeof(DownedIdleLogic), "TraumaDowned");

        public override bool IsCurrentActionEnding() => false;

        public override void Start()
        {
            try // corpo no tick de IA: exceção quebraria o brain inteiro (PA-01-15)
            {
                if (BotOwner?.BotLay == null) return;                      // null-guard: despawn durante hold (janela de 1 tick; sweep CR-01-02 limpa)
                BotOwner.BotLay.IsLay = true;                              // ref: scratchpad BotLay.cs:34-72 (pose 0 + DoProne + corta tiro/corrida)
                BotOwner.BotLay.NextPosibleGetUp = Time.time + TraumaBotFall.X(); // PA-02-07: unificado com o hold — expira JUNTO
                //   (auto-consistente: não depende de Stop() p/ destravar a IA no X-expiry); write DEPOIS do IsLay=true —
                //   o setter vanilla já stampa Time.time + DELTA_GETUP (BotLay.cs:46-54) e o nosso valor prevalece;
                //   neutraliza os call sites de BotLay.GetUp (:182-188)
                BotOwner.ShootData?.EndShoot();
                BotOwner.AimingManager?.CurrentAiming?.LoseTarget();       // ref: protótipo P6
                TraumaBotFall.TrySainClearActiveLayer(BotOwner);           // interop SAIN — estado stale (P6 evid.)
            }
            catch (System.Exception ex)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogError($"[Trauma2] TraumaDownedLayer.Start: {ex.Message}");
            }
        }

        public override void Stop()
        {
            try // PA-01-15: mesmos guards do Start
            {
                // Ponto ÚNICO de consumo do release (PA-02-02): lê ForceGetUp e REMOVE a entrada quando true;
                // X-expiry (false) mantém a entrada Released retida — arma o RE-HOLD do Pump. Consome ANTES dos
                // null-guards: o bookkeeping precisa fechar mesmo com BotLay já destruído (despawn).
                bool force = BotOwner != null && TraumaBotFall.ConsumeRelease(BotOwner.ProfileId);
                if (BotOwner?.BotLay == null) return;
                BotOwner.BotLay.NextPosibleGetUp = 0f;                     // destrava os BotLay.GetUp da IA
                // Release diferenciado pelo MOTIVO (PA-01-08): X-expiry (ForceGetUp=false) → SEM GetUp forçado —
                // o bot levanta quando alguma camada DECIDIR (SAIN em cover/ORBIT podem mantê-lo deitado; o Pump
                // re-holda ao detectar IsLay==false com linha viva). Cura/analgésico/toggle-off (ForceGetUp=true):
                if (force)
                    BotOwner.BotLay.GetUp(false);                          // devolução imediata — próxima camada assume no tick (D14)
            }
            catch (System.Exception ex)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogError($"[Trauma2] TraumaDownedLayer.Stop: {ex.Message}");
            }
        }
    }

    /// <summary>Lógica do hold: re-assert por frame (cinto contra IsLay=false direto do SAIN — Rodada 2 do P6);
    /// SEM path/steering (padrão IdleAction do ORBIT) = bot NÃO combate deitado.</summary>
    internal class DownedIdleLogic : CustomLogic
    {
        private static float _nextErrorLog; // throttle do LogError (roda no tick de IA — PA-01-15)

        public DownedIdleLogic(BotOwner botOwner) : base(botOwner) { }

        public override void Update(CustomLayer.ActionData data)
        {
            try
            {
                if (BotOwner?.BotLay == null) return; // null-guards: BotLay/ShootData nulos em despawn
                if (!BotOwner.BotLay.IsLay) BotOwner.BotLay.IsLay = true;
                BotOwner.ShootData?.EndShoot();
            }
            catch (System.Exception ex)
            {
                if (Time.time >= _nextErrorLog)
                {
                    _nextErrorLog = Time.time + 5f;
                    TRLImmersiveCombatMedicinePlugin.ModLogger.LogError($"[Trauma2] DownedIdleLogic.Update: {ex.Message}");
                }
            }
        }
    }
}
