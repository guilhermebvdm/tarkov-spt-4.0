# 004 — Pernas: Cair + ciclo levantar 3s/15s · Spec Técnica

**Mod:** TRL-ImmersiveCombatMedicine
**Status:** Em progresso (review 01 aplicada)
**Spec funcional:** [004-pernas-cair-ciclo-01-spec.md](004-pernas-cair-ciclo-01-spec.md)
**Criado:** 2026-07-19

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT deve citar `arquivo.cs:linha`. Segunda fonte canônica: [docs/trauma-primitives.md](../../docs/trauma-primitives.md) — P4 (pose/prone/guards/fall damage), P5 (levantar controlado + vozes, com as correções da Rodada 2), P6 (bots: BigBrain/BotLay/SAIN) — âncoras verificadas por ilspycmd no assembly real (decompilações auditáveis em `scratchpad/spike001/`: BotLay.cs, bigbrain_*.cs, SpeakerManager.cs, ActiveHealthController.cs). Terceira fonte: o código ENTREGUE do Trauma 2.0 ([TraumaEngine.cs](../../modded/Patches/Trauma/TraumaEngine.cs), [TraumaPose.cs](../../modded/Patches/Trauma/TraumaPose.cs), [TraumaLegsConsumer.cs](../../modded/Patches/Trauma/TraumaLegsConsumer.cs)) — assinaturas citadas por `arquivo:linha` do próprio mod.
>
> `Memória consultada: snapshot de 2026-07-19 (Sessão 3) · pendências que afetam: [P-3.5 003 entregue v1.4.1, VALIDAÇÃO IN-GAME PENDENTE — o 004 estende exatamente esse código], [P-3.4 diretiva do overhaul 003→008 + rastro de premissas p/ item 011] / nenhuma 🔴`

## 1. Estratégia

**Segundo consumidor do motor 002 — máquina de estados de 3 fases no dono, 2 extensões + 1 patch Harmony novo, camada BigBrain para bots, zero mudança no motor.**

1. **Motor: NADA a criar.** O kind `InvoluntaryFall` JÁ existe no enum ([TraumaEngineState.cs:59](../../modded/Patches/Trauma/TraumaEngineState.cs)) e JÁ é publicado pela linha Cair com o MESMO padrão do Zerar-2: `EvaluatePlayer` publica `TryPublishOneShot(p, InvoluntaryFall, to)` quando `to == LegsFallCycle` ([TraumaEngine.cs:572](../../modded/Patches/Trauma/TraumaEngine.cs); o do agachar é :571). O cooldown anti-thrash já é **por (profileId, kind)** (:27-28, :595) — cooldown do agachar não bloqueia o derrubar nem vice-versa, por construção (premissa da funcional atendida sem código novo). `TraumaConsumerId.FallCycle` também já existe (:61). Re-falls internos do ciclo NÃO passam pelo motor (isenção documentada em :585-586) — o cooldown do motor governa só a ENTRADA na linha.
2. **Consumidor `TraumaFallCycleConsumer`** (MonoBehaviour no GO do plugin, padrão 003): assina `SubscribeWithSnapshot` + `OneShotPublished` ([TraumaEngine.cs:72,22](../../modded/Patches/Trauma/TraumaEngine.cs)), registra `FallCycle` p/ `Legs` no registry (destrava o toast — a chave de texto `LegsFall` já existe, [TraumaLocale.cs:18,29,66](../../modded/Patches/Trauma/TraumaLocale.cs) — EN em :18 (:20 é ArmsAdsCancel; drift corrigido PA-01-13)). Roteia: `p.IsYourPlayer` → FSM humana; `p.IsAI` → `TraumaBotFall`. Dono-only herdado do motor (D16 — humanos peers rodam o próprio ciclo no cliente deles).
3. **FSM humana (fases BLOQUEIO → LIBERAÇÃO → [levantar lento] → JANELA → re-queda):** deadlines ABSOLUTOS lidos de `.Value` no início de cada fase (timers F12 valem na próxima fase iniciada — AC1). **Ordem real dos eventos: `StateChanged` dispara ANTES de `OneShotPublished`** ([TraumaEngine.cs:565 vs :571-572](../../modded/Patches/Trauma/TraumaEngine.cs)) — o `OnTransition` de entrada NÃO engaja às cegas (PA-01-03): sem establishing, consulta `TryGetOneShotDeadline(p, InvoluntaryFall, out d)`: `d` futuro (cooldown ativo — thrash de analgésico) ⇒ o publish será SUPRIMIDO ⇒ engaja direto em **JANELA** (estados contínuos seguem o snapshot — corner da funcional; já prone → BLOQUEIO); caso contrário NÃO engaja — o `OnOneShot(InvoluntaryFall)` do MESMO frame conduz (executa → derrubar, item 4 → BLOQUEIO; guard D7 adia → fase **FallPending**: sem timers, sem cap, negação OFF — encerrada pelo callback do pump, pelo cancelamento ou pela saída da linha). Estabelecimento (establishing/religar/adoção): **JANELA sem one-shot/toast**; já prone nesse instante → **BLOQUEIO** (item 5 da funcional). Troca de linha DENTRO do ciclo (Q2→Z2+Q2 ou Z2+Q2→Q2) é invisível por construção: a linha publicada segue `LegsFallCycle` e o motor não publica transição com `From==To` ([TraumaEngine.cs:548](../../modded/Patches/Trauma/TraumaEngine.cs)) — sem re-derrubar, sem reset de timers.
4. **Derrubar forçado (P4 rec. (2), par provado em produção):** `TraumaPose.TryInvoluntaryFall` — guards D7 reusando `CanForcePose` ([TraumaPose.cs:49-73](../../modded/Patches/Trauma/TraumaPose.cs)) → `SetPoseLevel(0f, force: true)` + `IsInPronePose = true` + **READBACK obrigatório** (o setter recusa SILENCIOSO quando `CanProne` falha — [MovementContext.cs:676-727](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L676), recusa :714-717; `CanProne` :1181-1230: UsingMeds/HealingLegs/ladeira>30°/sem espaço/zona ProneDisabled). Readback falhou FORA de D7 → **fallback agachado** `SetPoseLevel(0f)` + flag `PronePending` — re-tentada SÓ com a FSM em BLOQUEIO e com cadência ≥0.5s (timestamp na entrada; `CanProne` é SphereCast físico — fora do caminho quente), limpa na transição p/ LIBERAÇÃO e no Disengage (PA-01-06; contrato unificado da Rodada 2 do doc de primitivas). Guard D7 falhou → **adia** na fila do `TraumaPose` (generalizada por kind — dedup `(player, kind)` já existente, :113-131) com re-validação de snapshot e cancelamento com refund (`ReportOneShotCanceled`, padrão 003). Sem dano de queda: estruturalmente impossível (P4 (4) — `CheckFlying` re-seta `StartFallingHeight` por frame em solo, [MovementContext.cs:2566-2594](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L2566)); nenhuma mitigação, nenhuma injeção legada (decisão 21). Re-queda da JANELA e re-derrubada de bot são **internas** (entram na fila com `Internal=true`, sem refund de cooldown — não há publish a devolver); enqueue interno é PROIBIDO enquanto houver entrada NÃO-interna pendente do mesmo `(player, kind)` — a pendente já entrega a queda, sem colisão de dedup sobrescrevendo `Internal`/refund (PA-01-03).
5. **Negação de levantar NA ORIGEM (P5):** prefix condicional em `MovementContext.CanStandAt` (virtual, [MovementContext.cs:3304](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L3304)) — o gate vanilla do levantar-de-prone humano é exatamente esse (`IsInPronePose` setter: `if (!IsAI && !CanStandAt(PoseLevel))` recusa silencioso, :690) e há precedente vanilla do mesmo shape (`IsInPronePose && UsingMeds` → false, :3304-3309). **NUNCA blanket-false** (correção P5: quebraria `CanInteract` :1416, `CanSit` :1328 e o `SetPoseLevel(force)` do próprio mod :2149): `__result=false` SOMENTE quando `IsYourPlayer` + fase BLOQUEIO + (`IsInPronePose` OU, no fallback agachado, `h > PoseLevel + 0.05f`) + `!reentryFlag`. A condição `h > PoseLevel` deixa passar os `SetPoseLevel(0f)` do próprio mod e o rastejar/mover agachado (movimento horizontal não consulta `CanStandAt`). Implementação por EXTENSÃO do `CantStandUpPatch` existente ([InputPatches.cs:38-68](../../modded/Patches/Trauma/InputPatches.cs)) — choke point único, sem corrida entre dois prefixes no mesmo alvo. Colateral aceito da funcional: porta/loot negados no bloqueio (`CanInteract` consulta `CanSit`→`CanStandAt(0f)` — parte da incapacitação). **Detecção de TENTATIVA (som) separada da imposição:** prefix NOVO em `GamePlayerOwner.TranslateCommand` ([GamePlayerOwner.cs:801](../../../../references/eft-decompiled/Assembly-CSharp/EFT/GamePlayerOwner.cs#L801) — ponto já patchado em produção pelo `FreezeCommandPatch`) observando `ToggleProne`/`ToggleDuck`/`Jump`/`NextWalkPose` ([ECommand.cs:32,27,44,34](../../../../references/eft-decompiled/Assembly-CSharp/EFT.InputSystem/ECommand.cs#L32)) com fase BLOQUEIO → voz forte com anti-spam; o comando SEGUE normal (quem nega é o `CanStandAt`) — peers não veem movimento algum, só ouvem (a pose nunca muda no dono, logo nunca replica).
6. **Levantar lento (LIBERAÇÃO — P5):** ao expirar o bloqueio, fase LIBERAÇÃO estável (sem timers). No primeiro get-up: `reentryFlag` + `SetPoseLevel(0f, force: true)` p/ a saída de prone terminar AGACHADA; detecção da DECISÃO de levantar por POSE, não por prone (PA-01-05 — cobre o fallback agachado): bloqueio em prone → polling `IsInPronePose == false`; bloqueio em fallback → `PoseLevel > 0.05f` (subida só possível com a negação desligada — fim do BLOQUEIO; a rampa parte da pose corrente, sem `SetPoseLevel(0f, force)`); sem decisão = LIBERAÇÃO estável (funcional §2) → voz leve + rampa por frame SOBRE A POSE REAL `SetPoseLevel(Mathf.MoveTowards(mc.PoseLevel, PoseMemo, dt/SlowRiseSeconds))` com readback (PA-01-07: `SetPoseLevel` recusa sob teto baixo via `CanStandAt` :2149 — a rampa ESTACIONA e retoma quando houver espaço) até `PoseMemo` ([Player.cs:23912](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L23912); a suavização vanilla converge rápido demais p/ servir de knob — [MovementContext.cs:2208-2245](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L2208)). `SlowRiseSeconds` = const interna `1.5f` (decisão 13 cobre só os 3 timers — premissa nova, item 011). **"De pé efetivo" (início da JANELA) = fim da rampa** (pose alvo REAL atingida — `mc.PoseLevel >= PoseMemo - 0.01f`, nunca variável local da rampa); sem rampa (estabelecimento, analgésico destravando levantar em curso) = jogador já de pé (premissa técnica — item 011). Meio-levantar visual da tentativa frustrada (decisão 6): **fora do default** — a Rodada 2 do P5 rebaixou o veredito a "indeterminado sem protótipo runtime"; o 004 entrega só voz (fallback aceito da funcional), experimento fica registrado p/ item 011.
7. **Cap N2 da JANELA — causa PRÓPRIA do 004, sem compartilhamento (PA-01-01):** helper novo `TraumaSpeedCap` com causa exclusiva `(ESpeedLimit)1001` (mesmo shape Remove+Add do 003 — `AddStateSpeedLimit` é no-op com causa existente, [MovementContext.cs:1672-1679](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L1672); recompute único por frame via dirty-flag: `ProcessSpeedLimits` :2553-2558 → `method_4()` :1798, que toma o MIN de todas as causas ativas — âncora corrigida PA-01-13). O 003 mantém `ApplyCap`/`RemoveCapGuarded` e a causa `(ESpeedLimit)1000` INTOCADOS ([TraumaLegsConsumer.cs:116-152](../../modded/Patches/Trauma/TraumaLegsConsumer.cs)): 003 e 004 NUNCA escrevem a mesma causa — cada um remove só a própria, e a coexistência no frame de handoff é arbitrada pela **min-composição nativa** do dict `SpeedLimits`, em qualquer ordem de handlers (undo cruzado impossível por construção; premissa p/ item 011: consumidores futuros ganham causas próprias 1002+). Handoff explícito na ENTRADA da linha Cair (PA-01-02): o `OnTransition` do 003 trata `To == LegsFallCycle` como SAÍDA para efeitos do 003 — `_applied.Remove(p)` + `RemoveCapGuarded(p)` — e a poda oportunista também poda entradas cujo `GetLine == LegsFallCycle`. O cap da janela **independe do toggle 003** (helper não consulta `TraumaLegsConsumer.IsActive`). Interim removido: `IsN2Tier` do 003 deixa de incluir `LegsFallCycle` ([TraumaLegsConsumer.cs:59-64](../../modded/Patches/Trauma/TraumaLegsConsumer.cs)). Gate de sprint estendido: `CanSprintPatch` também força `false` quando `GetLine == LegsFallCycle` + consumidor 004 ativo ([SpeedLimitPatches.cs:15-31](../../modded/Patches/Trauma/SpeedLimitPatches.cs)) — cumpre o contrato "cap N2 bloqueia sprint" na JANELA (prone/bloqueio não sprintam por natureza).
8. **Arbitragem D2/D3:** (a) **absorção de agachar** no ponto compartilhado — `TraumaPose.TryInvoluntaryCrouch` e `BotCrouchDip` consultam `TraumaFallCycleConsumer.IsCycleEngaged(p)` no topo: ciclo ativo → refund via `TryGetOneShotDeadline`+`ReportOneShotCanceled` (:90-91 padrão já entregue) + log ABSORB — nunca descarte silencioso; cobre humano em QUALQUER fase (inclusive JANELA de pé) e bot no hold, e já serve o 006 de graça. (b) **desmaio legado pausa** (D3): FSM consulta `TraumaState.BlackoutTimers`/`IsFainted` ([TraumaState.cs:19,31](../../modded/Patches/Trauma/TraumaState.cs)) por frame — blackout ativo → fase PAUSED (nada escreve pose: o blackout já força prone por frame no `MainLoopPatch` :35-39, sem double-writer); wake → re-avalia `GetLine`: Cair persiste → **BLOQUEIO reiniciado** (deadline novo); curado/rebaixado → ciclo encerra. (c) **Cair+desmaio no MESMO evento**: `OnOneShot(InvoluntaryFall)` com blackout já ativo → absorve com refund; wake re-avalia → BLOQUEIO direto (já prone). (d) **DOWNED do Fika pausa como desmaio**: heurística `!hc.IsAlive` com record do motor ainda vivo (o mesmo contrato downed-safe do 003 — `FikaPlayer` re-seta `IsAlive` no revive); revive (`IsAlive` volta) → re-avalia como no wake; morte real → `OnPlayerDeadOrUnspawn` untracka no motor → `GetLine==None` → sweep limpa (remoção de efeitos NUNCA gateada em `IsAlive` — lição CR do 003). (e) **estômago LEGADO fora do motor** (até o 006 — PA-01-09): o agachar legado de estômago ([HealthPatches.cs:97-108](../../modded/Patches/Trauma/HealthPatches.cs) — `SetPoseLevel(0f, true)` direto com dano ≥35 e `!IsInPronePose`) NÃO passa pela absorção (a) — nenhum one-shot de estômago existe antes do 006; ganha guard próprio `IsCycleEngaged(p)` → suprime + log `stomach legacy suppressed (fall-cycle)` (sem o guard, agacharia o jogador na JANELA/Rising por fora do TraumaPose); o `crouch ABSORB` por estômago só é exercitável no 006 — premissa p/ item 011.
9. **Bots (decisão 16 + P6, mecanismo ÚNICO = camada BigBrain + BotLay):** registro no Awake `BrainManager.AddCustomLayer(typeof(TraumaDownedLayer), brains, 90)` (prio 90 preempta SAIN 20/22/24/69/70/80, ORBIT 19, UNTAR 4/5, BSG Exfil 79 — P6 evid.); brains = união recomendada `{PmcBear, PmcUsec, PMC, Assault, CursAssault, ExUsec, ArenaFighter, Obdolbs}` (cobre UNTAR 1170-1173 via PMC/ExUsec — D15); **bosses/followers FORA no 004** (animações especiais; premissa nova — item 011). Derrubar (`Start()`): `BotLay.IsLay = true` (caminho vanilla: pose 0 + DoProne + corta tiro/corrida — scratchpad BotLay.cs:34-72) + `NextPosibleGetUp = Time.time + X` (campo público :22-23; neutraliza TODOS os call sites de `BotLay.GetUp` :182-188) + `ShootData.EndShoot()` + `AimingManager?.CurrentAiming?.LoseTarget()` + interop SAIN `BotComponent.ActiveLayer = None` por reflection (sem isso o SAIN atira/gira caído — estado stale, P6 evid.). Hold (`DownedIdleLogic.Update`): re-assert `IsLay` + `EndShoot` por frame (cinto contra `IsLay=false` direto do SAIN — Rodada 2 P6); **sem path/steering** (padrão IdleAction do ORBIT) = bot NÃO combate deitado. Fim de X → `IsActive()` false → `Stop()` diferenciado pelo MOTIVO do release (flag `ForceGetUp` no hold — PA-01-08): X-expiry → SÓ `NextPosibleGetUp = 0` (destrava os `BotLay.GetUp` da IA — o bot levanta quando alguma camada DECIDIR, sem ioiô mecânico; "quando a IA decidir levantar", funcional 6/D14); cura/analgésico/toggle-off → `NextPosibleGetUp = 0` + `GetUp(false)` forçado (a IA levanta sem re-derrubada). Bot levantou (`IsLay==false && !IsInPronePose`) com linha ainda Cair → **re-hold imediato com X novo** (interno, isento do cooldown do motor). Entrada de bot por transição ESTABELECEDORA (adoção mid-raid/spawn ferido — establishing não publica one-shot, TraumaEngine.cs:567) → `OnLine(p, LegsFallCycle)` gera hold estabelecedor idempotente via `IsHeld`, sem refund/one-shot (PA-01-11). Cura/analgésico → release imediato sem re-hold. Headless idêntico (BotsController vive lá — P6). Exige referência compile-time a `DrakiaXYZ-BigBrain.dll` (`CustomLayer` é herança, não reflection); registro gateado por `Chainloader.PluginInfos` (ausente → bots sem ciclo + warn, humano intacto).
10. **Sons (P5, decisão 20):** helper novo `TraumaVoice` com chamadas TIPADAS (substitui o padrão reflection do `VoiceHelper` legado — rec. P5). **Forte** (queda executada + tentativa negada): `player.Speaker.Play(EPhraseTrigger.OnAgony, ETagStatus.Combat | ETagStatus.Dying, demand: true, importance: 100)` — importance explícita fura o Busy em tiroteio ([PhraseSpeakerClass.cs:175,206-227](../../../../references/eft-decompiled/Assembly-CSharp/PhraseSpeakerClass.cs#L175)); **leve** (liberação): `player.Say(EPhraseTrigger.OnBeingHurt, demand: true)` ([Player.cs:28799-28829](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L28799) — demand obrigatório: humano local tem `OnDemandOnly=true` no inicializador do `new PhraseSpeakerClass` :28670; `Init(...)` é chamada separada :28672 — âncora corrigida PA-01-13). Anti-spam próprio ≥2 s por (player, tipo) — spam de input no bloqueio não repete o som. Peers ouvem o MESMO clipe via `PhrasePacket` do Fika (FikaPlayer.cs:1093-1103; sem o filtro LocalPhrases do netcode BSG). Bots: **sem sons próprios do ciclo no 004** (funcional: sem sons de tentativa p/ bot; decisão registrada). Fallback OGG local (pipeline do repo) SÓ se a validação in-game reprovar a distinção forte×leve — aciona a limitação aceita da funcional (peers deixam de ouvir).
11. **Substituição do interim + rename-at-delivery:** `Fall Cycle (item 004)` → `Fall Cycle` (nasce ON; órfã DELETADA sem copiar valor em `MigrateOrphanedConfigKeys` — padrão registrado no [PROPRIEDADES.md](../../PROPRIEDADES.md) tabela Renomeadas + lição CR-03-01).

**Alternativas descartadas:** (a) clamp de pose por frame p/ humano ou bot — briga com SAIN/orçamento (P6; corner da funcional); (b) blanket-false em `CanStandAt` — quebra `CanInteract`/`SetPoseLevel(force)` do próprio mod (correção P5; o `CantStandUpPatch` do blackout continua blanket mas o ciclo NUNCA coexiste com blackout ativo — pausa D3); (c) imposição via `BlockAll` no `TranslateCommand` — não cobre todas as origens de saída de prone (o funil real é o setter de `IsInPronePose` → `CanStandAt`) e conflitaria com o `FreezeCommandPatch`; comando é só detecção de tentativa; (d) prone via `Player.ToggleProne()` — soma gates extras (IsAnimatorInteractionOn/StationaryWeapon, [Player.cs:26054](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L26054)) sem controle de recusa; o par `SetPoseLevel(force)+IsInPronePose` é o baseline provado em produção (P4); (e) hold de bot só por `NextPosibleGetUp` — não cobre `StartInteraction`/`IsLay=false` direto do SAIN (Rodada 2 P6; a camada + re-assert cobrem); (f) protocolo Fika custom — pose e voz replicam nativo (P4/P5 dono/peers); (g) novo kind/publicação no motor — já entregues no 002.

## 2. Pontos de patch

**1 patch Harmony novo + 2 extensões de patches existentes** + hooks C# (motor 002, BigBrain, BotLay, voz):

| Alvo (Assembly) | Tipo | Motivo |
|---|---|---|
| [`EFT/MovementContext.cs:3304` — `CanStandAt` (virtual)](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L3304) | Prefix (EXTENSÃO do `CantStandUpPatch`, [InputPatches.cs:38](../../modded/Patches/Trauma/InputPatches.cs)) | Negação do levantar na origem (fase BLOQUEIO): condicional, nunca blanket (P5). Overrides auditados (AP-03): `ObservedMovementContext.CanStandAt => true` sem base-call (fika ObservedMovementContext.cs:109-112) → espelhos imunes por construção; `ClientMovementContext`/`NoInertiaMovementContext` NÃO sobrescrevem (dispatch cai na base patchada — correção P5); gate `IsYourPlayer` OBRIGATÓRIO no branch novo (FikaBot passa pela base via `SetPoseLevel` :2149 — sem o gate, clamparíamos bots). |
| [`EFT/GamePlayerOwner.cs:801` — `TranslateCommand`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/GamePlayerOwner.cs#L801) | Prefix (NOVO, detecção-only) | Tentativa de levantar no BLOQUEIO → voz forte + log (anti-spam); NUNCA bloqueia nem muda `__result` (imposição é o `CanStandAt`). Coexiste com `FreezeCommandPatch` (blackout retorna `false` antes — e o ciclo está PAUSED sob blackout). `GamePlayerOwner` só existe p/ o humano local (dono por construção); `HideoutPlayerOwner` tem `TranslateCommand` próprio (override declarado em HideoutPlayerOwner.cs:558; :564 é o `ECommand.ToggleProne` dentro dele — âncora corrigida PA-01-13) mas o ciclo exige raid — irrelevante. |
| [`EFT/MovementContext.cs:1240` — `CanSprint` (getter virtual)](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L1240) | Postfix (EXTENSÃO do `CanSprintPatch`, [SpeedLimitPatches.cs:15](../../modded/Patches/Trauma/SpeedLimitPatches.cs)) | Sprint bloqueado na JANELA: `__result=false` também quando `GetLine == LegsFallCycle` + consumidor 004 ativo (contrato do cap N2). Espelhos já imunes (ObservedMovementContext.cs:34 — AP-03 auditado no 003). |

| Hook C# (sem patch) | Assinatura / âncora | Uso |
|---|---|---|
| `TraumaEngine.SubscribeWithSnapshot` / `OneShotPublished` / `GetLine` | [TraumaEngine.cs:72 / :22 / :48](../../modded/Patches/Trauma/TraumaEngine.cs) | Entrada/saída da linha Cair (replay establishing) · one-shot `InvoluntaryFall` (:572) · re-validação por fase/pump |
| `TraumaEngine.ReportOneShotExecuted` / `ReportOneShotCanceled` / `TryGetOneShotDeadline` | [TraumaEngine.cs:117 / :127 / :137](../../modded/Patches/Trauma/TraumaEngine.cs) | Cooldown da execução (D7) · refund em cancelamento/absorção D2 (nunca descarte silencioso) |
| `TraumaConsumerRegistry.Register` | [TraumaEngineState.cs:132](../../modded/Patches/Trauma/TraumaEngineState.cs) | `FallCycle` cobre `Legs` — destrava toast (decisão 20; texto `LegsFall` já existe) |
| Prone forçado / fallback / rastejar | `SetPoseLevel(0f, force:true)` + `IsInPronePose=true` + readback ([MovementContext.cs:2139/:676-727](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L2139)); `CanProne` :1181-1230 | Derrubar + fallback agachado (P4 rec. (2)); rastejar = locomoção prone vanilla, intocada |
| Guards D7 | `TraumaPose.CanForcePose` ([TraumaPose.cs:49-73](../../modded/Patches/Trauma/TraumaPose.cs)) — IsGrounded :1089, estados vault, `BtrState` [Player.cs:25413](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L25413), tarkin-ladders reflection | Todo derrubar (entrada, re-queda, expiração de analgésico) adia em contexto inválido |
| Desmaio legado (D3) | `TraumaState.BlackoutTimers` / `IsFainted` ([TraumaState.cs:19,31](../../modded/Patches/Trauma/TraumaState.cs)); wake em [MovementPatches.cs:57-93](../../modded/Patches/Trauma/MovementPatches.cs) + `WakeLocalPlayer` ([Plugin :402-419](../../modded/TRLImmersiveCombatMedicinePlugin.cs)) | Pausa/retomada do ciclo por polling do estado (sem hook novo no legado) |
| BigBrain | `BrainManager.AddCustomLayer(Type, List<string>, 90)`; `CustomLayer`/`CustomLogic` (scratchpad bigbrain_BrainManager.cs:147-165, bigbrain_CustomLayer.cs:35-59; protótipo P6 compilado 0 erros) | Camada `TraumaDownedLayer` — hold do bot |
| BotLay | `IsLay` setter (scratchpad BotLay.cs:34-72), `NextPosibleGetUp` (:22-23), `GetUp` (:182-188); `ShootData.EndShoot()`, `AimingManager.CurrentAiming.LoseTarget()` (protótipo P6) | Derrubar/segurar/devolver bot pelo caminho vanilla |
| Interop SAIN | `BotComponent.ActiveLayer` setter via reflection (SAIN BotComponent.cs:183-187; padrão AggroHelper/`TrySainSetTargetPose` [TraumaPose.cs:272-304](../../modded/Patches/Trauma/TraumaPose.cs)) | Zera camada stale do SAIN no Start() do hold (senão atira/gira caído) |
| Voz nativa | `Speaker.Play(OnAgony, tags, true, importance:100)` ([PhraseSpeakerClass.cs:175](../../../../references/eft-decompiled/Assembly-CSharp/PhraseSpeakerClass.cs#L175)); `Say(OnBeingHurt, demand:true)` ([Player.cs:28799](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L28799)); triggers [EPhraseTrigger.cs:6,12](../../../../references/eft-decompiled/Assembly-CSharp/EPhraseTrigger.cs#L6) | Forte (queda/tentativa) e leve (liberação); peers via PhrasePacket (fika FikaPlayer.cs:1093-1103) |
| Cap N2 | `TraumaSpeedCap` (helper NOVO do 004, causa própria `(ESpeedLimit)1001` — `AddStateSpeedLimit`/`RemoveStateSpeedLimit` [MovementContext.cs:1672/:1790](../../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs#L1672), min-composição `method_4()` :1798, `MaxSpeed` :910, `UpdateSpeedLimitByHealth` [Player.cs:29068](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L29068)) | Mancar N2 na JANELA, independente do toggle 003; coexiste com a causa 1000 do 003 por min nativo (PA-01-01) |

## 3. Novas propriedades F12 (BepInEx)

Seção nova `8. Trauma 2.0 (Queda)` + rename-at-delivery na seção 6. `PROPRIEDADES.md` atualizado na entrega (gate).

| Seção | Nome (EN) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|
| `8. Trauma 2.0 (Queda)` | `Fall Window Seconds` | float | `3` | 1 a 10 | — | JANELA: tempo DE PÉ antes de cair de novo com as duas pernas quebradas (linha Cair). Conta do fim do levantar; mudanças valem a partir da PRÓXIMA janela iniciada. Piso 1s intencional (0 degeneraria em prone permanente). |
| `8. Trauma 2.0 (Queda)` | `Fall Block Seconds` | float | `15` | 5 a 60 | — | BLOQUEIO: tempo no chão sem poder levantar após cada queda (tentar dá som de dor e nada acontece; rastejar é livre). Mudanças valem a partir do PRÓXIMO bloqueio iniciado. Piso 5s intencional (0 anularia o ciclo, conflitando com o anti-thrash do motor). |
| `8. Trauma 2.0 (Queda)` | `Bot Fall Hold Seconds` | float | `15` | 5 a 120 | — | Tempo MÍNIMO que um bot com linha Cair fica no chão SEM combater antes de a IA poder levantar (ao levantar, é re-derrubado enquanto a condição durar). Separado dos timers humanos. |
| `6. Trauma 2.0 (Consumidores)` | `Fall Cycle` | bool | **`true`** (rename do placeholder, era `false`) | — | — | Cair + ciclo de levantar (item 004). Governado pelo master Trauma 2.0; desligar mid-raid destrava o levantar na hora, cancela quedas pendentes e libera bots (o mancar interim do 003 NÃO volta). OFF com Legs Effects ON: o aviso (toast) da 1ª ocorrência da linha Cair ainda aparece — registry de consumidores é por região (PA-01-14). |

Estado neutro: toggle 004 OFF = linha Cair **sem efeito do mod** (interim do 003 removido permanentemente — corner da funcional); exceção documentada e ACEITA (PA-01-14): o toast de 1ª ocorrência ainda dispara se OUTRO consumidor cobre a região Legs (`AnyActiveFor` é por REGIÃO, TraumaEngineState.cs:137-149; granularizar por linha é mudança de motor — premissa p/ item 011). Timers lidos por `.Value` no INÍCIO de cada fase (deadline absoluto; contagem em andamento nunca re-baseada — AC1). Pisos > 0 documentados no tooltip (premissa da funcional). Nota PROPRIEDADES: linha na tabela **Renomeadas** (`Fall Cycle (item 004)` → `Fall Cycle`, órfã deletada sem copiar valor).

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Patches/Trauma/TraumaFallCycleConsumer.cs` | CRIAR | Consumidor 004: registry + assinaturas do motor; FSM humana (FallPending/Blocked/Released/Rising/Window/Paused) com deadlines absolutos e entrada ciente da ordem StateChanged→OneShot (PA-01-03); derrubar/re-queda via TraumaPose; pausa/retomada blackout+DOWNED; cap N2 da janela via TraumaSpeedCap; API `IsCycleEngaged(p)`/`IsBlockedPhase(p)` p/ patches e absorção D2; edges do toggle; sweeps de fim de raid/world-swap (padrão 003). |
| `modded/Patches/Trauma/TraumaBotFall.cs` | CRIAR | `TraumaDownedLayer : CustomLayer` + `DownedIdleLogic : CustomLogic` + manager estático (holds por profileId, X novo por re-hold, release por cura, bookkeeping limpo em morte/despawn — CR-01-02); registro `AddCustomLayer(..., 90)` gateado por Chainloader; interop SAIN `ActiveLayer=None`. |
| `modded/Patches/Trauma/TraumaVoice.cs` | CRIAR | Helper tipado de voz (forte OnAgony via Speaker.Play importance:100; leve OnBeingHurt via Say demand:true) com anti-spam ≥2s por (player, tipo); reusável pelo 005 (P9). |
| `modded/Patches/Trauma/TraumaSpeedCap.cs` | CRIAR | Helper NOVO exclusivo do 004: causa própria `(ESpeedLimit)1001` (PA-01-01 — nunca compartilhada com a 1000 do 003), Apply (Remove+Add, log calibração; alvo N2 efetivo via `LineTargetPercent` do 003), RemoveGuarded (downed-safe, mesmo contrato CR do 003); coexistência arbitrada pela min-composição nativa (`method_4()` :1798). |
| `modded/Patches/Trauma/TraumaPose.cs` | MODIFICAR | (1) `TryInvoluntaryFall` (prone force + readback + fallback agachado + `PronePending` retry só em BLOQUEIO com cadência ≥0.5s, limpo em Released/Disengage — PA-01-06); (2) fila de adiados ganha `Internal` (re-quedas sem refund; enqueue interno recusado com não-interna pendente do mesmo kind — PA-01-03), dispatch por kind, `CancelKind(kind, reason)` (PA-01-04) e `PumpDeferred` idempotente por frame/agnóstico ao chamador (PA-01-12); (3) absorção D2 no topo de `TryInvoluntaryCrouch`/`BotCrouchDip` (consulta `IsCycleEngaged` → refund + log ABSORB). |
| `modded/Patches/Trauma/TraumaLegsConsumer.cs` | MODIFICAR | Interim REMOVIDO: `IsN2Tier` sem `LegsFallCycle`; `OnTransition` trata `To == LegsFallCycle` como SAÍDA p/ efeitos do 003 (`_applied.Remove` + `RemoveCapGuarded` — handoff explícito ao 004, PA-01-02); poda oportunista poda também `GetLine == LegsFallCycle`; toggle-off cancela só o próprio kind via `CancelKind(InvoluntaryCrouch)` (PA-01-04); ApplyCap/RemoveCapGuarded e causa 1000 INTOCADOS (PA-01-01). |
| `modded/Patches/Trauma/SpeedLimitPatches.cs` | MODIFICAR | `CanSprintPatch`: OR do 004 (`GetLine==LegsFallCycle` + FallCycle ativo → false). `UpdateSpeedLimitByHealthPatch`: re-log consulta também o bookkeeping do 004 (cap da janela). |
| `modded/Patches/Trauma/InputPatches.cs` | MODIFICAR | `CantStandUpPatch` ganha o branch condicional do ciclo (BLOQUEIO; prone OU h>PoseLevel no fallback; IsYourPlayer; reentry flag); branch de blackout intacto. Patch NOVO `FallAttemptCommandPatch` (TranslateCommand, detecção-only). Ambos com try/catch+LogError — padrão do patch atual preservado (PA-01-15). |
| `modded/Patches/Trauma/HealthPatches.cs` | MODIFICAR | Guard no bloco LEGADO de estômago (:97-108): `IsCycleEngaged` → suprime o `SetPoseLevel(0f, true)` + log `stomach legacy suppressed (fall-cycle)` — arbitragem D2 do escritor de pose fora do motor até o 006 (PA-01-09). |
| `modded/TRLImmersiveCombatMedicinePlugin.cs` | MODIFICAR | Binds §3 (seção 8 + rename `Fall Cycle`); órfã do placeholder deletada em `MigrateOrphanedConfigKeys` (:265-287 — padrão pronto); `AddComponent<TraumaFallCycleConsumer>()`; registro BigBrain via `TraumaBotFall.RegisterLayer()`. |
| `modded/TRL-ImmersiveCombatMedicine.csproj` | MODIFICAR | Referência `DrakiaXYZ-BigBrain.dll` (`Private=false`; resolvida pelo compile-mod SÓ após a entrada nova no mapa — linha abaixo, PA-01-10). |
| `.agents/scripts/compile-mod.sh` | MODIFICAR | Mapa `resolve_references()` (hardcoded, :272-302) ganha a entrada `DrakiaXYZ-BigBrain.dll` ← `$spt/BepInEx/plugins/DrakiaXYZ-BigBrain.dll` (mesmo padrão do Fika.Core.dll); hoje grep BigBrain = 0 e o build FALHARIA — parte OBRIGATÓRIA da entrega (PA-01-10). |
| `PROPRIEDADES.md` | MODIFICAR | Seção 8 nova; rename-at-delivery na tabela Renomeadas; tooltip do `Fall Cycle`; gate de entrega. |

## 5. Stubs de código

> Pré-código: assinaturas completas + corpo mínimo plausível. Cada referência tem `// ref:`. Assinaturas do EFT verificadas no dump/assembly real (P4/P5/P6); contrato do motor citado do código implementado.

```csharp
// modded/Patches/Trauma/TraumaFallCycleConsumer.cs
using Comfort.Common;
using EFT;
using TrueTrauma; // TraumaState (blackout legado)
using UnityEngine;

namespace TRLImmersiveCombatMedicine.Trauma
{
    /// <summary>Consumidor do ciclo de queda (item 004). FSM 3 fases no DONO humano local; bots via TraumaBotFall.
    /// Motor intocado: InvoluntaryFall já publicado pela linha Cair (TraumaEngine.cs:572, cooldown por kind :27).</summary>
    public sealed class TraumaFallCycleConsumer : MonoBehaviour
    {
        private enum FallPhase { None, FallPending, Blocked, Released, Rising, Window, Paused } // FallPending: queda de entrada ADIADA (D7) — sem timers/cap/negação (PA-01-03)

        private static TraumaFallCycleConsumer _instance;

        // FSM só do humano LOCAL (peers rodam o próprio ciclo no cliente deles — D16); bots no TraumaBotFall.
        private Player _local;                  // dono humano com ciclo ativo (null = sem ciclo)
        private FallPhase _phase;
        private float _phaseDeadline;           // deadline ABSOLUTO — timers F12 valem na PRÓXIMA fase (AC1)
        private bool _capApplied;               // cap N2 da JANELA (TraumaSpeedCap)
        private bool _releasedFromProne;        // pose no início da LIBERAÇÃO — decisão de levantar lida por POSE (PA-01-05)
        internal const float SlowRiseSeconds = 1.5f; // const interna (decisão 13 cobre só os 3 timers — premissa p/ 011)
        internal static bool StandReentryFlag;  // deixa os SetPoseLevel do próprio mod passarem pelo CanStandAt

        private bool _wasActive;
        private GameWorld _trackedWorld;        // padrão 003: world-swap/transit + null-detect

        private static readonly TraumaRegion[] LegsRegions = { TraumaRegion.Legs };

        private void Awake()
        {
            _instance = this;
            TraumaConsumerRegistry.Register(TraumaConsumerId.FallCycle, LegsRegions, IsActive); // toast (decisão 20)
            TraumaEngine.SubscribeWithSnapshot(OnTransition);   // replay establishing — ref: TraumaEngine.cs:72
            TraumaEngine.OneShotPublished += OnOneShot;         // ref: TraumaEngine.cs:22 (cooldown-gated)
        }

        internal static bool IsActive()
        {
            return TRLImmersiveCombatMedicinePlugin.ConfigMasterEnabled.Value
                && TRLImmersiveCombatMedicinePlugin.ConfigTrauma2Enabled.Value
                && TRLImmersiveCombatMedicinePlugin.ConfigConsumerFallCycle.Value;
        }

        /// <summary>Arbitragem D2 (consumida por TraumaPose): ciclo engajado p/ este player (humano em qualquer
        /// fase ativa, ou bot em hold) → agachar involuntário é ABSORVIDO com refund.</summary>
        internal static bool IsCycleEngaged(Player p)
        {
            TraumaFallCycleConsumer inst = _instance;
            if (inst == null || p is null) return false;
            if (p.IsAI) return TraumaBotFall.IsHeld(p.ProfileId);
            return ReferenceEquals(p, inst._local) && inst._phase != FallPhase.None;
        }

        /// <summary>Consumida pelo CantStandUpPatch (negação na origem) e pelo FallAttemptCommandPatch (som).</summary>
        internal static bool IsBlockedPhase(Player p)
        {
            TraumaFallCycleConsumer inst = _instance;
            return inst != null && ReferenceEquals(p, inst._local) && inst._phase == FallPhase.Blocked;
        }

        private void OnTransition(TraumaTransition t)
        {
            if (t.Region != TraumaRegion.Legs || !IsActive()) return;
            Player p = t.Player;
            if (p is null) return;
            if (p.IsAI) { TraumaBotFall.OnLine(p, t.To); return; }
            if (!p.IsYourPlayer) return;

            if (t.To == TraumaLine.LegsFallCycle)
            {
                // ORDEM REAL: StateChanged dispara ANTES de OneShotPublished (TraumaEngine.cs:565 vs :571-572) —
                // NÃO engajar às cegas (PA-01-03). Establishing (spawn ferido/religar/adoção) = JANELA sem
                // one-shot/toast; já prone → BLOQUEIO (item 5 da funcional). Sem establishing: cooldown ativo
                // (deadline futuro) ⇒ o publish será SUPRIMIDO ⇒ engaja já (corner do thrash de analgésico);
                // senão o OnOneShot do MESMO frame conduz (executa → BLOQUEIO; D7 adia → FallPending).
                // Troca de linha DENTRO do ciclo não passa aqui (From==To não publica — TraumaEngine.cs:548).
                if (_phase != FallPhase.None) return;
                if (t.Establishing) { Engage(p, establishing: true); return; }
                if (TraumaEngine.TryGetOneShotDeadline(p, TraumaOneShotKind.InvoluntaryFall, out float cd) && cd > Time.time)
                    Engage(p, establishing: false); // one-shot NÃO virá — engaja (JANELA; prone → BLOQUEIO)
                // senão: aguarda o OnOneShot deste frame — nenhum estado criado aqui
            }
            else if (ReferenceEquals(p, _local))
            {
                // Saída da linha Cair (cura/analgésico/None): encerra NA HORA (decisão 1) — destrava levantar
                // lento em andamento, cancela adiados (refund), remove cap. 003 assume a linha nova (N1/N2).
                Disengage("line-exit");
            }
        }

        private void OnOneShot(Player p, TraumaOneShotKind kind, TraumaLine line)
        {
            if (!IsActive() || kind != TraumaOneShotKind.InvoluntaryFall) return;
            if (p is null) return;
            if (p.IsAI) { TraumaBotFall.OnFallOneShot(p); return; }
            if (!p.IsYourPlayer) return;

            // D3: Cair + desmaio no MESMO evento → desmaio vence; derrubar absorvido COM refund (nunca silencioso).
            if (TraumaState.BlackoutTimers.ContainsKey(p.ProfileId) || TraumaState.IsFainted)
            {
                if (TraumaEngine.TryGetOneShotDeadline(p, kind, out float d))
                    TraumaEngine.ReportOneShotCanceled(p, kind, d); // ref: TraumaEngine.cs:127
                // log "[Trauma2] fall ABSORB (blackout) <id>" — wake re-avalia e entra em BLOQUEIO (já prone)
                return;
            }
            // Derrubar de entrada: fase FallPending ATÉ o callback (D7 pode ADIAR — escada/BTR/vault; PA-01-03):
            // sem timers, sem cap, negação OFF; encerrada por OnFallExecuted (→ BLOQUEIO), cancel ou saída da linha.
            _local = p;
            _phase = FallPhase.FallPending;
            TraumaPose.TryInvoluntaryFall(p, TraumaRegion.Legs, kind, internalFall: false, OnFallExecuted);
        }

        private void OnFallExecuted(Player p, bool fellProne)
        {
            // Chamado pelo TraumaPose na execução (imediata ou do pump). fellProne=false = fallback agachado
            // (PronePending re-tenta no pump; bloqueio vale p/ a pose corrente — funcional §1).
            _local = p;
            EnterBlocked("fall-executed");
            TraumaVoice.PlayStrong(p); // OnAgony importance:100 — ref: PhraseSpeakerClass.cs:175
        }

        private void Engage(Player p, bool establishing)
        {
            _local = p;
            if (p.MovementContext != null && p.MovementContext.IsInPronePose) EnterBlocked(establishing ? "established-prone" : "engage-prone");
            else EnterWindow(establishing ? "established" : "engage-standing"); // sem one-shot, sem toast
        }

        private void EnterBlocked(string reason)
        {
            _phase = FallPhase.Blocked;
            _phaseDeadline = Time.time + Mathf.Clamp(TRLImmersiveCombatMedicinePlugin.ConfigFallBlockSeconds.Value, 5f, 60f);
            RemoveWindowCap();
            // log "[Trauma2] fall cycle phase=Blocked reason=<reason> <id>"
        }

        private void EnterWindow(string reason)
        {
            _phase = FallPhase.Window;
            _phaseDeadline = Time.time + Mathf.Clamp(TRLImmersiveCombatMedicinePlugin.ConfigFallWindowSeconds.Value, 1f, 10f);
            ApplyWindowCap(); // cap N2 do 004 (independente do toggle 003) — TraumaSpeedCap
            // log "[Trauma2] fall cycle phase=Window reason=<reason> <id>"
        }

        private void ApplyWindowCap()
        {
            // TraumaSpeedCap.Apply(_local, percentN2efetivo) — causa PRÓPRIA (ESpeedLimit)1001 (PA-01-01), Remove+Add,
            // log de calibração; coexiste com a causa 1000 do 003 pela min-composição nativa (method_4 :1798).
            // (ref: MovementContext.cs:1672/:1790/:910; mesmo shape do TraumaLegsConsumer.ApplyCap). _capApplied = true.
        }

        private void RemoveWindowCap()
        {
            // if (_capApplied) { TraumaSpeedCap.RemoveGuarded(_local); _capApplied = false; } — remove SÓ a causa 1001 (downed-safe, 003 CR)
        }

        private void Disengage(string reason)
        {
            RemoveWindowCap();
            TraumaPose.CancelFallsFor(_local, reason);   // adiados de queda: refund quando não-internos
            TraumaPose.ClearPronePending(_local);        // PA-01-06: pendência de prone morre com o ciclo
            StandReentryFlag = false;
            _phase = FallPhase.None;
            _local = null;
            // log "[Trauma2] fall cycle END (<reason>) <id>" — levantar destravado NA HORA (decisão 1)
        }

        private void Update()
        {
            GameWorld gw = Singleton<GameWorld>.Instance;
            if (gw == null)
            {
                // padrão N1/003: mundo morreu — só bookkeeping (pose/caps morrem com o mundo)
                if (_phase != FallPhase.None) Disengage("raid-end");
                TraumaBotFall.ClearAll();
                _trackedWorld = null; _wasActive = IsActive();
                return;
            }
            if (!ReferenceEquals(gw, _trackedWorld))
            {
                if (_phase != FallPhase.None) Disengage("world-swap"); // transit — espelha detecção do motor/003
                TraumaBotFall.ClearAll();
                _trackedWorld = gw;
            }

            bool active = IsActive();
            if (_wasActive && !active)
            {
                // Toggle OFF mid-ciclo: prone deixa de ser forçado NA HORA, agendamentos cancelados, bots liberados
                if (_phase != FallPhase.None) Disengage("toggle-off");
                TraumaBotFall.ReleaseAll("toggle-off");
            }
            else if (!_wasActive && active)
            {
                // Religar = avaliação estabelecedora (JANELA; prone → BLOQUEIO) a partir do snapshot do motor
                Player mp = gw.MainPlayer;
                if (mp != null && TraumaEngine.IsOwnedHere(mp)
                    && TraumaEngine.GetLine(mp, TraumaRegion.Legs) == TraumaLine.LegsFallCycle)
                    Engage(mp, establishing: true);
                TraumaBotFall.EstablishFromSnapshot(gw); // bots com linha Cair → hold estabelecedor
            }
            _wasActive = active;
            if (!active) return;

            TickHumanCycle();
            TraumaPose.PumpDeferred();      // adiados D7 (crouch 003 + fall 004) + re-tentativa de prone do fallback
            TraumaBotFall.Pump();           // re-holds / releases / sweeps de bot
        }

        private void TickHumanCycle()
        {
            if (_phase == FallPhase.None) return;
            Player p = _local;
            // Poda: motor não rastreia mais / linha saiu com toggle off no meio (padrão sweep 003)
            if (p is null || p.MovementContext == null || TraumaEngine.GetLine(p, TraumaRegion.Legs) != TraumaLine.LegsFallCycle)
            { Disengage("stale"); return; }

            // D3/DOWNED: blackout legado OU downed Fika (!IsAlive com record vivo — contrato downed-safe do 003)
            bool paused = TraumaState.BlackoutTimers.ContainsKey(p.ProfileId) || TraumaState.IsFainted
                || p.HealthController == null || !p.HealthController.IsAlive;
            if (paused)
            {
                if (_phase != FallPhase.Paused)
                {
                    RemoveWindowCap(); StandReentryFlag = false; _phase = FallPhase.Paused;
                    // log "[Trauma2] fall cycle PAUSED (blackout|downed) <id>" — nada escreve pose (blackout já força prone)
                }
                return;
            }
            if (_phase == FallPhase.Paused)
            {
                // wake/revive: re-avalia snapshot — linha persiste → BLOQUEIO REINICIADO (acordou ≠ pronto p/ levantar)
                EnterBlocked("resume");
                return;
            }

            var mc = p.MovementContext;
            switch (_phase)
            {
                case FallPhase.FallPending:
                    break; // passivo (PA-01-03): poda stale/pausa acima cobrem; callback do pump/cancel encerram
                case FallPhase.Blocked:
                    if (Time.time >= _phaseDeadline)
                    {
                        _releasedFromProne = mc.IsInPronePose;  // decisão de levantar será lida por POSE (PA-01-05)
                        TraumaPose.ClearPronePending(p);        // fallback: bloqueio acabou — levanta de onde está (PA-01-06)
                        _phase = FallPhase.Released; // estável: sem timers até o jogador DECIDIR levantar
                        // log "[Trauma2] fall cycle phase=Released <id>"
                    }
                    break;
                case FallPhase.Released:
                    // Decisão de levantar por POSE (PA-01-05): bloqueio em prone → saiu de prone; bloqueio em
                    // fallback agachado → PoseLevel SUBIU (>0.05f — só possível com a negação OFF, fim do BLOQUEIO).
                    // Sem decisão = LIBERAÇÃO estável (funcional §2) — nunca auto-Rising no fallback.
                    bool wantsUp = _releasedFromProne ? !mc.IsInPronePose : mc.PoseLevel > 0.05f;
                    if (wantsUp)
                    {
                        StandReentryFlag = true;
                        if (_releasedFromProne) mc.SetPoseLevel(0f, true); // get-up termina agachado — ref: MovementContext.cs:2139
                        _phase = FallPhase.Rising;          // rampa parte da POSE REAL corrente (PA-01-07)
                        TraumaVoice.PlayLight(p);           // OnBeingHurt demand:true — ref: Player.cs:28799
                        // log "[Trauma2] fall cycle phase=Rising <id>"
                    }
                    break;
                case FallPhase.Rising:
                    // Rampa sobre a POSE REAL com readback (PA-01-07): SetPoseLevel recusa sob teto baixo
                    // (CanStandAt vanilla :2149) → a rampa ESTACIONA e retoma quando houver espaço.
                    float next = Mathf.MoveTowards(mc.PoseLevel, p.PoseMemo, Time.deltaTime / SlowRiseSeconds); // ref: Player.cs:23912
                    mc.SetPoseLevel(next);
                    if (mc.PoseLevel >= p.PoseMemo - 0.01f)
                    {
                        StandReentryFlag = false;
                        EnterWindow("rose"); // "de pé efetivo" = pose alvo REAL atingida (premissa técnica — item 011)
                    }
                    break;
                case FallPhase.Window:
                    if (Time.time >= _phaseDeadline)
                    {
                        if (mc.IsInPronePose) { EnterBlocked("window-expired-prone"); break; } // deitou voluntário — sem re-derrubar
                        // Re-queda INTERNA (isenta do cooldown do motor); D7 adia; fallback agachado se CanProne=false.
                        // TraumaPose RECUSA o enqueue interno se houver entrada NÃO-interna pendente (PA-01-03).
                        TraumaPose.TryInvoluntaryFall(p, TraumaRegion.Legs, TraumaOneShotKind.InvoluntaryFall,
                            internalFall: true, OnFallExecuted);
                    }
                    break;
            }
        }
    }
}
```

```csharp
// modded/Patches/Trauma/TraumaPose.cs — ADIÇÕES (fila generalizada por kind + prone forçado + absorção D2)
/// <summary>Derrubar forçado (P4 rec. (2)): guards D7 → prone com readback → fallback agachado.
/// internalFall=true (re-queda da janela / re-hold de bot) NÃO refunda cooldown ao cancelar (não houve publish).</summary>
internal static void TryInvoluntaryFall(Player p, TraumaRegion region, TraumaOneShotKind kind,
    bool internalFall, System.Action<Player, bool> onExecuted)
{
    // 1. p/mc nulos → refund (se !internalFall) — padrão code-review 1 do 003, achado 4
    // 2. mc.IsInPronePose → já no chão: onExecuted(p, true) SEM tocar pose (paridade "já prone → só BLOQUEIO")
    // 3. !CanForcePose(p, out guard) → Defer(p, region, kind, guard, internalFall, onExecuted)  // D7 adia
    // 4. mc.SetPoseLevel(0f, force: true); mc.IsInPronePose = true;   // ref: MovementContext.cs:2139/:676
    //    READBACK: if (!mc.IsInPronePose)  // recusa silenciosa do CanProne (:714-717, :1181-1230)
    //        { mc.SetPoseLevel(0f); /* fallback AGACHADO */ MarkPronePending(p); onExecuted(p, false);
    //          log "[Trauma2] fall FALLBACK-CROUCH <id>" }           // re-tentativa de prone no pump
    //    else { onExecuted(p, true); log "[Trauma2] fall EXECUTED <id>" }
    // 5. if (!internalFall) TraumaEngine.ReportOneShotExecuted(p, kind);  // D7: cooldown conta da execução — :117
}

/// <summary>Absorção D2 — chamada NO TOPO de TryInvoluntaryCrouch e BotCrouchDip (cobre 003 hoje e 006 depois):
/// ciclo engajado (humano em qualquer fase OU bot em hold) → NÃO executa; refund do publish + log ABSORB.</summary>
private static bool AbsorbIfCycleEngaged(Player p, TraumaOneShotKind kind)
{
    // if (!TraumaFallCycleConsumer.IsCycleEngaged(p)) return false;
    // if (TraumaEngine.TryGetOneShotDeadline(p, kind, out float d)) TraumaEngine.ReportOneShotCanceled(p, kind, d);
    // log "[Trauma2] crouch ABSORB (fall-cycle) <id>"; return true;   // nunca descarte silencioso (funcional §4)
    return false;
}

// DeferredCrouch ganha: bool Internal (sem refund no cancel) + System.Action<Player,bool> OnExecuted (callback 004).
//   Enqueue INTERNO é RECUSADO se já existe entrada NÃO-interna pendente do mesmo (player, kind) — a pendente
//   entrega a queda; evita o dedup (:113-131) sobrescrever Internal/refund (PA-01-03).
// PumpDeferred ganha dispatch por kind: InvoluntaryCrouch → SetPoseLevel(0f) (código atual, :135-164);
//   InvoluntaryFall → repete os passos 2-5 acima (prone+readback+fallback). Re-validação de snapshot idêntica
//   (GetLine == RequiredLine; mudou → CANCELA com refund SÓ se !Internal). Idempotente por FRAME e agnóstico ao
//   chamador (003 e 004 chamam — PA-01-12): if (Time.frameCount == _lastPumpFrame) return; _lastPumpFrame = Time.frameCount;
// PronePending (PA-01-06): re-tenta prone SÓ com a FSM do 004 em Blocked, cadência ≥0.5s (timestamp na entrada —
//   CanProne é SphereCast físico, :1209-1234); limpo via ClearPronePending(p) na transição p/ Released e no Disengage.
// CancelFallsFor(Player p, string reason): remove entradas (p, InvoluntaryFall) — refund se !Internal (toggle-off/cura).
// CancelKind(TraumaOneShotKind kind, string reason) (PA-01-04): cancela SÓ o kind — o toggle-off do 003 usa
//   CancelKind(InvoluntaryCrouch) (nunca varre quedas do 004); raid-end/world-swap seguem com CancelAll (a dupla
//   chamada 003+004 é idempotente: a fila já está vazia na segunda).
```

```csharp
// modded/Patches/Trauma/TraumaBotFall.cs
using System.Collections.Generic;
using DrakiaXYZ.BigBrain.Brains; // ref: protótipo P6 compilado 0 erros (scratchpad/spike001/proto-traumadowned/)
using EFT;
using UnityEngine;

namespace TRLImmersiveCombatMedicine.Trauma
{
    /// <summary>Hold de bot do ciclo (decisão 16 + P6): camada BigBrain prio 90 + BotLay + interop SAIN.
    /// Dono-only por construção (camada só existe onde BotOwner vive — host/headless; espelho sem brain).</summary>
    internal static class TraumaBotFall
    {
        private sealed class Hold { internal Player Player; internal float ReleaseAt; internal bool Released; internal bool ForceGetUp; } // ForceGetUp = motivo do release (PA-01-08): cura/toggle=true, X-expiry=false
        private static readonly Dictionary<string, Hold> _holds = new Dictionary<string, Hold>(); // por profileId

        /// <summary>Registro no Awake do plugin, gateado por Chainloader (BigBrain ausente → warn, humano intacto).
        /// Brains = união SAIN/ORBIT/UNTAR sem bosses/followers (premissa p/ 011); prio 90 preempta SAIN/ORBIT/UNTAR/Exfil.</summary>
        internal static void RegisterLayer()
        {
            // if (!BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("xyz.drakia.bigbrain")) { warn; return; }
            // BrainManager.AddCustomLayer(typeof(TraumaDownedLayer),
            //     new List<string> { "PmcBear", "PmcUsec", "PMC", "Assault", "CursAssault", "ExUsec", "ArenaFighter", "Obdolbs" },
            //     90); // ref: scratchpad bigbrain_BrainManager.cs:147-165; prioridades P6 (SAIN≤80, ORBIT 19, UNTAR 4/5, Exfil 79)
        }

        internal static bool IsHeld(string profileId) =>
            _holds.TryGetValue(profileId, out Hold h) && !h.Released;

        internal static bool ShouldForceGetUp(string profileId) =>
            _holds.TryGetValue(profileId, out Hold h) && h.ForceGetUp; // consultado pelo Stop() da camada (PA-01-08)

        internal static void OnFallOneShot(Player bot)
        {
            // Entrada pelo one-shot do motor: hold X (fire-and-forget — sem fila D7 de humano; BotLay tem guards próprios)
            // MovementContext/BotOwner nulos → refund do publish (padrão BotCrouchDip, TraumaPose.cs:182-197)
            // _holds[id] = new Hold { Player = bot, ReleaseAt = Time.time + X() };  // camada vira IsActive no próximo tick
            // TraumaEngine.ReportOneShotExecuted(bot, TraumaOneShotKind.InvoluntaryFall);
            // log "[Trauma2] bot fall HOLD <id> x=<X>"
        }

        internal static void OnLine(Player bot, TraumaLine to)
        {
            // to == LegsFallCycle (transição ESTABELECEDORA — adoção mid-raid/spawn ferido; establishing não publica
            //   one-shot, TraumaEngine.cs:567) → hold estabelecedor idempotente (IsHeld → no-op), sem refund/one-shot
            //   (PA-01-11 — cobre também raid-start com bots já feridos)
            // to != LegsFallCycle (cura/analgésico/None) → release imediato SEM re-hold, ForceGetUp=true
            //   ("a IA levanta sem re-derrubada" — PA-01-08)
        }

        internal static void EstablishFromSnapshot(GameWorld gw) { /* religar toggle: bots com GetLine==FallCycle → hold sem one-shot */ }

        internal static void Pump()
        {
            // 1. ReleaseAt vencido → Released=true, ForceGetUp=false (X-expiry devolve a DECISÃO à IA — PA-01-08;
            //    camada IsActive=false → árbitro devolve — D14)
            // 2. Released && bot levantou (BotLay.IsLay==false && !IsInPronePose) && GetLine ainda FallCycle
            //    → RE-HOLD com X novo (interno, isento do cooldown do motor) — log "bot fall RE-HOLD <id>"
            // 3. Sweep: player morto/despawn/GetLine==None → remove entrada (CR-01-02: sem entrada órfã)
        }

        internal static void ReleaseAll(string reason) { /* toggle-off: ForceGetUp=true → Stop() com GetUp(false) (PA-01-08); _holds limpos após o release */ }
        internal static void ClearAll() { _holds.Clear(); } // mundo morto — objetos destruídos, só bookkeeping
        private static float X() => Mathf.Clamp(TRLImmersiveCombatMedicinePlugin.ConfigBotFallHoldSeconds.Value, 5f, 120f);
    }

    internal class TraumaDownedLayer : CustomLayer
    {
        public TraumaDownedLayer(BotOwner botOwner, int priority) : base(botOwner, priority) { }
        public override string GetName() => "TraumaDowned";
        public override bool IsActive() => TraumaBotFall.IsHeld(BotOwner.ProfileId); // árbitro re-decide por tick (D14)
        public override Action GetNextAction() => new Action(typeof(DownedIdleLogic), "TraumaDowned");
        public override bool IsCurrentActionEnding() => false;

        public override void Start()
        {
            try // corpo no tick de IA: exceção quebraria o brain inteiro (PA-01-15)
            {
                if (BotOwner?.BotLay == null) return;                      // null-guard: despawn durante hold (janela de 1 tick; sweep CR-01-02 limpa)
                BotOwner.BotLay.IsLay = true;                              // ref: scratchpad BotLay.cs:34-72 (pose 0 + DoProne + corta tiro/corrida)
                BotOwner.BotLay.NextPosibleGetUp = Time.time + 999f;       // re-stampado pelo hold; neutraliza BotLay.GetUp (:182-188)
                BotOwner.ShootData?.EndShoot();
                BotOwner.AimingManager?.CurrentAiming?.LoseTarget();       // ref: protótipo P6
                // Interop SAIN: BotComponent.ActiveLayer = ESAINLayer.None via reflection (Enum.ToObject(propType, 0))
                //   — sem isso o SAIN atira/gira caído (estado stale; ref: SAIN BotComponent.cs:183-187, SAINActivationClass.cs:83-98)
            }
            catch (System.Exception) { /* LogError (PA-01-15) */ }
        }

        public override void Stop()
        {
            try // PA-01-15: mesmos guards do Start
            {
                if (BotOwner?.BotLay == null) return;
                BotOwner.BotLay.NextPosibleGetUp = 0f;                     // destrava os BotLay.GetUp da IA
                // Release diferenciado pelo MOTIVO (PA-01-08): X-expiry (ForceGetUp=false) → SEM GetUp forçado —
                // o bot levanta quando alguma camada DECIDIR (SAIN em cover/ORBIT podem mantê-lo deitado; o Pump
                // re-holda ao detectar IsLay==false com linha viva). Cura/analgésico/toggle-off (ForceGetUp=true):
                if (TraumaBotFall.ShouldForceGetUp(BotOwner.ProfileId))
                    BotOwner.BotLay.GetUp(false);                          // devolução imediata — próxima camada assume no tick (D14)
            }
            catch (System.Exception) { /* LogError (PA-01-15) */ }
        }
    }

    internal class DownedIdleLogic : CustomLogic
    {
        public DownedIdleLogic(BotOwner botOwner) : base(botOwner) { }
        public override void Update(CustomLayer.ActionData data)
        {
            // Re-assert por frame (cinto contra IsLay=false direto do SAIN — Rodada 2 do P6); SEM path/steering.
            // try/catch + null-guards (PA-01-15): roda no tick de IA — BotLay/ShootData nulos em despawn.
            try
            {
                if (BotOwner?.BotLay == null) return;
                if (!BotOwner.BotLay.IsLay) BotOwner.BotLay.IsLay = true;
                BotOwner.ShootData?.EndShoot();
            }
            catch (System.Exception) { /* LogError com throttle (PA-01-15) */ }
        }
    }
}
```

```csharp
// modded/Patches/Trauma/InputPatches.cs — CantStandUpPatch ESTENDIDO + patch novo de detecção
[HarmonyPatch(typeof(MovementContext), "CanStandAt")] // ref: MovementContext.cs:3304 (virtual; ObservedMovementContext => true :109-112 — espelho imune AP-03)
class CantStandUpPatch
{
    static bool Prefix(MovementContext __instance, float h, ref bool __result)
    {
        // Corpo segue em try/catch+LogError como no patch atual (InputPatches.cs:46-65 — padrão preservado, PA-01-15)
        // ... branch de blackout EXISTENTE intacto (:54-58) ...
        // Branch NOVO do ciclo (item 004) — NUNCA blanket-false (P5): só dono humano local em BLOQUEIO.
        // if (TraumaFallCycleConsumer.StandReentryFlag) return true;            // SetPoseLevel do próprio mod passa
        // player via TraumaState.PlayerField (padrão existente); if (!player.IsYourPlayer) return true; // FikaBot passa pela base via SetPoseLevel:2149
        // if (TraumaFallCycleConsumer.IsBlockedPhase(player)
        //     && (__instance.IsInPronePose || h > __instance.PoseLevel + 0.05f)) // prone: qualquer h; fallback agachado: só SUBIR
        //     { __result = false; return false; }                                // negação silenciosa — pose nunca muda (peers não veem nada)
        return true;
    }
}

/// <summary>Detecção de TENTATIVA no BLOQUEIO (som + anti-spam). NUNCA bloqueia — a imposição é o CanStandAt;
/// comando não mapeado = no máximo um som perdido, nunca um levantar vazado.</summary>
[HarmonyPatch(typeof(GamePlayerOwner), "TranslateCommand")] // ref: GamePlayerOwner.cs:801 (ponto já patchado em produção — FreezeCommandPatch)
class FallAttemptCommandPatch
{
    static void Prefix(GamePlayerOwner __instance, ECommand command)
    {
        // Corpo INTEIRO em try/catch + LogError (skill csharp §3/§6 — PA-01-15; detecção-only nunca pode quebrar input)
        // if (command != ECommand.ToggleProne && command != ECommand.ToggleDuck
        //     && command != ECommand.Jump && command != ECommand.NextWalkPose) return; // ref: ECommand.cs:32/27/44/34
        // Player p = __instance.Player; if (p == null || !TraumaFallCycleConsumer.IsBlockedPhase(p)) return;
        // TraumaVoice.PlayStrong(p); // anti-spam interno ≥2s — spam de input NÃO repete o som
        // log "[Trauma2] fall attempt BLOCKED <id>"
    }
}
```

```csharp
// modded/Patches/Trauma/HealthPatches.cs — guard no bloco LEGADO de estômago (PA-01-09)
// O agachar legado (:97-108 — SetPoseLevel(0f, true) direto com dano ≥35 e !IsInPronePose, gateado só por
// ConfigStomachEnabled) escreve pose POR FORA do TraumaPose e NÃO passa pela absorção D2 — nenhum one-shot de
// estômago existe antes do item 006. Guard de 1 linha no topo do bloco:
// if (TraumaFallCycleConsumer.IsCycleEngaged(player))
//     log "[Trauma2] stomach legacy suppressed (fall-cycle) <id>";   // JANELA/Rising intactos — AC da funcional
// else { /* bloco legado atual, inalterado */ }
```

```csharp
// modded/Patches/Trauma/TraumaVoice.cs
using System.Collections.Generic;
using EFT;
using UnityEngine;

namespace TRLImmersiveCombatMedicine.Trauma
{
    /// <summary>Voz de dor TIPADA (substitui o caminho reflection do VoiceHelper legado — rec. P5). Peers ouvem o
    /// MESMO clipe via PhrasePacket do Fika (FikaPlayer.cs:1093-1103; sem filtro LocalPhrases). Anti-spam próprio.</summary>
    internal static class TraumaVoice
    {
        private static readonly Dictionary<(string, bool), float> _nextAllowed = new Dictionary<(string, bool), float>();
        private const float SpamCooldown = 2f;

        /// <summary>FORTE (queda + tentativa negada): OnAgony com importance explícita — fura o Busy do Speaker em
        /// tiroteio (demand só fura OnDemandOnly+roll — correção P5). ref: PhraseSpeakerClass.cs:175/206-227.</summary>
        internal static void PlayStrong(Player p)
        {
            // if (!Allowed(p, strong: true)) return;
            // p.Speaker?.Play(EPhraseTrigger.OnAgony, ETagStatus.Combat | ETagStatus.Dying, demand: true, importance: 100); // ref: EPhraseTrigger.cs:6
        }

        /// <summary>LEVE (liberação): OnBeingHurt demand:true (humano local tem OnDemandOnly=true — Player.cs:28670,
        /// inicializador do new PhraseSpeakerClass; Init é chamada separada :28672 — PA-01-13).</summary>
        internal static void PlayLight(Player p)
        {
            // if (!Allowed(p, strong: false)) return;
            // p.Say(EPhraseTrigger.OnBeingHurt, demand: true); // ref: Player.cs:28799-28829; EPhraseTrigger.cs:12
        }

        private static bool Allowed(Player p, bool strong)
        {
            // key (ProfileId, strong); Time.time < next → false; senão stamp Time.time + SpamCooldown → true
            return false;
        }
    }
}
```

```csharp
// modded/TRLImmersiveCombatMedicinePlugin.cs — ADIÇÕES
public static ConfigEntry<float> ConfigFallWindowSeconds;
public static ConfigEntry<float> ConfigFallBlockSeconds;
public static ConfigEntry<float> ConfigBotFallHoldSeconds;

// Awake(): binds §3 (seção "8. Trauma 2.0 (Queda)"); rename-at-delivery:
// ConfigConsumerFallCycle = Config.Bind("6. Trauma 2.0 (Consumidores)", "Fall Cycle", true, "...tooltip §3...");
// MigrateOrphanedConfigKeys(): deletar órfã ("6. Trauma 2.0 (Consumidores)", "Fall Cycle (item 004)") SEM copiar
//   valor + Config.Save (mesmo bloco do 003 — Plugin :265-287; lição CR-03-01);
// gameObject.AddComponent<TraumaFallCycleConsumer>();  // DEPOIS do TraumaEngine (ordem do 003)
// TraumaBotFall.RegisterLayer();                        // gateado por Chainloader ("xyz.drakia.bigbrain")
```

## 6. Fluxo de dados

```
[Quebrar 2 / Z2+Q2 sem analgésico no DONO]
        ▼
[motor 002: ResolveLegs → LegsFallCycle (TraumaMatrixResolver.cs:27-28)]
        │ StateChanged + OneShotPublished(InvoluntaryFall) — TraumaEngine.cs:572 (cooldown por kind :27)
        ▼
[TraumaFallCycleConsumer]
        ├─ humano local → TraumaPose.TryInvoluntaryFall: guards D7 (CanForcePose) → SetPoseLevel(0f,true)+IsInPronePose=true
        │      readback ok → BLOQUEIO (deadline abs) + voz FORTE (Speaker.Play OnAgony imp:100)
        │      readback falhou → fallback AGACHADO + PronePending (re-try só em BLOQUEIO, ≥0.5s); D7 → Defer (fase FallPending; cancel = refund)
        │      pose→peers via PlayerStateData (PoseLevel packed :352, IsProne bit :363 — sync nativo, sem protocolo)
        ├─ BLOQUEIO: CanStandAt prefix nega na origem (prone: sempre; fallback: só h>PoseLevel) — pose NUNCA muda;
        │      tentativa (ToggleProne/Duck/Jump/NextWalkPose no TranslateCommand) → voz FORTE anti-spam (peers ouvem via PhrasePacket)
        │      rastejar livre (locomoção prone não consulta CanStandAt)
        ├─ 15s → LIBERAÇÃO (estável) → jogador levanta → termina agachado (reentry+SetPoseLevel(0f,true))
        │      → voz LEVE + rampa MoveTowards→PoseMemo (1.5s) → "de pé efetivo"
        ├─ JANELA 3s: cap N2 via TraumaSpeedCap (causa PRÓPRIA 1001 — min nativo com a causa 1000 do 003) + CanSprint=false;
        │      expirou → re-queda INTERNA (D7/fallback; sem cooldown do motor); já prone → BLOQUEIO direto
        ├─ desmaio/DOWNED → PAUSED (D3; blackout já força prone — sem double-writer); wake/revive → re-avalia →
        │      Cair persiste → BLOQUEIO reiniciado; curado → END
        └─ cura/analgésico (linha sai de FallCycle) → END na hora (destrava rampa; refund de adiados; cap OFF; 003 assume)

[bot com LegsFallCycle no dono (host/headless)]
        ▼
[TraumaBotFall: hold X] — camada BigBrain prio 90 (IsActive=IsHeld) → Start(): BotLay.IsLay=true +
        NextPosibleGetUp=now+X + EndShoot/LoseTarget + SAIN ActiveLayer=None; Update(): re-assert (sem combate)
        │ X expira → IsActive=false → Stop(): SÓ NextPosibleGetUp=0 (IA decide levantar — PA-01-08; árbitro devolve, D14)
        │ cura/analgésico/toggle-off → Stop(): NextPosibleGetUp=0 + GetUp(false) (levanta sem re-derrubada)
        │ bot DE PÉ + linha persiste → RE-HOLD (X novo, interno) | cura → release sem re-hold
        └ peers veem deitar/levantar via PlayerStateData (sync nativo)

[D2] agachar involuntário (003 hoje / 006 depois) com ciclo engajado (humano OU bot em hold)
        → TraumaPose.AbsorbIfCycleEngaged: NÃO executa + ReportOneShotCanceled(publishDeadline) + log ABSORB
```

Exemplo AC1: quebrar as 2 pernas → motor publica `Legs: * -> LegsFallCycle reason=FractureGained` + one-shot `InvoluntaryFall` → `fall EXECUTED` + `phase=Blocked` + toast 1ª ocorrência ("Your legs collapse under you.") + voz forte; tentar levantar → `fall attempt BLOCKED` (som 1×/2s, pose imóvel); 15 s → `phase=Released`; levantar → voz leve + `phase=Rising` → `phase=Window`; 3 s de pé → re-queda (`fall EXECUTED` interno) → `phase=Blocked`. Analgésico no bloqueio → motor rebaixa p/ `LegsLimpN1/N2` (`PainkillerGained`) → `fall cycle END (line-exit)` ≤1 frame — levanta livre; expiração → re-entra `LegsFallCycle` (`PainkillerLost`) e o motor RE-PUBLica o one-shot (decisão 14) — cai na hora (cooldown do motor decide o thrash).

## 7. Riscos e dependências

- **Patches existentes:** `CantStandUpPatch` (branch novo convive com o de blackout — mutuamente exclusivos: ciclo PAUSED durante blackout); `FreezeCommandPatch` (mesmo alvo `TranslateCommand` — o dele retorna `false` no blackout e o nosso é detecção-only); `CanSprintPatch`/`UpdateSpeedLimitByHealthPatch` (extensões aditivas); blackout legado do `MainLoopPatch` força prone por frame durante desmaio — o ciclo pausado NÃO escreve pose (sem double-writer); `HealthPatches` seta `IsInPronePose=true` na entrada do desmaio (:83) — compatível com o corner Cair+desmaio (wake encontra o jogador prone e entra em BLOQUEIO); o bloco LEGADO de estômago do mesmo arquivo (:97-108) é escritor de pose FORA do motor e ganha o guard `IsCycleEngaged` (PA-01-09 — arbitragem D2 plena por estômago só no 006).
- **Dependência NOVA de build:** referência compile-time a `DrakiaXYZ-BigBrain.dll` (herança `CustomLayer` — reflection inviável). `Private=false`; resolvida de `D:\SPT\BepInEx\plugins` pelo compile-mod **somente após adicionar `DrakiaXYZ-BigBrain.dll` ao mapa `resolve_references()` do `.agents/scripts/compile-mod.sh` (:272-302)** — hoje o mapa NÃO tem a entrada (grep = 0) e o build FALHARIA; a edição do script é parte da entrega (PA-01-10). Registro gateado por Chainloader: BigBrain ausente → bots sem ciclo + warn 1× (humano intacto).
- **Compatibilidade:** SAIN (interop `ActiveLayer=None` por reflection — no-op silencioso se shape mudar, padrão `TrySainSetTargetPose`); ORBIT 1.1.0 instalado (prio 19 < 90 — inalterado); UNTAR coberto por brains PMC/ExUsec (D15); tarkin-ladders (guard D7 já entregue); smoke SAIN/ORBIT do re-derrubar é o escopo reduzido do 009 previsto na funcional.
- **P-3.5 (memória):** o 003/v1.4.1 ainda NÃO foi validado in-game — o 004 estende exatamente esse código (TraumaPose/caps/gates). Se a validação do 003 achar bug estrutural, esta spec herda o retrabalho (risco aceito pela diretiva P-3.4 de seguir o overhaul; validações podem ser combinadas numa raid só).
- **Baseline drift do cap da janela:** mesmo tratamento do 003 (re-derivado a cada aplicação; re-log RECOMPUTE).

### Aberturas explícitas para os reviewers

1. **"De pé efetivo" = fim da rampa do levantar lento** (não a saída do prone): a JANELA começa com o jogador em pose alvo — leitura mais fiel de "saída do prone concluída" e evita janela consumida pela própria animação. Premissa técnica p/ item 011. **Review 01: confirmada e reforçada com readback (PA-01-07)** — "pose alvo" é a pose REAL (`mc.PoseLevel`), nunca a variável da rampa.
2. **Fallback agachado — negação por `h > PoseLevel + 0.05f`:** deixa passar os SetPoseLevel(0f) do mod e o mover agachado, e nega qualquer subida de pose. Efeito colateral: em fallback, `CanInteract` NÃO é negado (só o caso prone nega via CanSit) — colateral da funcional vale só p/ prone. Aceitável? (A funcional fala "interações PODEM ser negadas" — permissivo.)
3. **Extração do `TraumaSpeedCap`:** **RESOLVIDA na review 01 (PA-01-01)** — sem extração e sem compartilhamento: o 004 ganha causa própria `(ESpeedLimit)1001` e o 003 fica INTOCADO no cap (causa 1000); a min-composição nativa do `SpeedLimits` (`method_4()` :1798) arbitra a coexistência — o "undo cruzado" que motivava a extração deixa de existir por construção.
4. **Brains de bot sem bosses/followers** no 004 (lista P6 recomendada): bot-boss com linha Cair fica SEM ciclo (só mancar/cap se aplicável). Premissa p/ 011; estender é 1 linha na lista.
5. **Detecção de tentativa por lista de comandos** (`ToggleProne/ToggleDuck/Jump/NextWalkPose`): best-effort — comando fora da lista = som perdido, nunca levantar vazado (imposição é o CanStandAt). `PreviousWalkPose`/`RestorePose` ficaram fora (raros); incluir se o smoke mostrar tentativa muda.
6. **Voz com `importance: 100`** pode CORTAR fala em andamento do próprio jogador (risco 2 do P5) — aceito p/ garantir o som da tentativa em tiroteio; validar in-game a distinção forte×leve (gate do fallback OGG — limitação Fika registrada na funcional).
7. **Prone forçado durante `Vaulting*`** segue não exercitado (Rodada 2 do P4) — o guard D7 ADIA nesses estados; manter o caso no smoke (herdado do 003, abertura 6).
8. **DOWNED por `!hc.IsAlive`** não distingue downed de morte real por 1 frame — inócuo: morte real dispara `OnPlayerDeadOrUnspawn` → untrack → `GetLine==None` → sweep encerra o ciclo (nunca retoma).

## 8. Checklist de implementação

- [ ] `TraumaSpeedCap.cs`: helper NOVO do 004 com causa própria `(ESpeedLimit)1001` (Apply Remove+Add + log calibração; RemoveGuarded downed-safe); 003 INTOCADO no cap — coexistência por min nativo (PA-01-01).
- [ ] `TraumaPose.cs`: `TryInvoluntaryFall` (prone+readback+fallback; `PronePending` só em BLOQUEIO, cadência ≥0.5s, limpo em Released/Disengage) + fila com `Internal`/callback (enqueue interno recusado com não-interna pendente) + dispatch por kind + pump idempotente por frame + `AbsorbIfCycleEngaged` no topo dos caminhos de agachar + `CancelFallsFor`/`CancelKind`.
- [ ] `TraumaVoice.cs`: forte/leve tipadas + anti-spam 2s.
- [ ] `TraumaFallCycleConsumer.cs`: FSM (deadlines absolutos; entrada ciente da ordem StateChanged→OneShot via `TryGetOneShotDeadline` + fase FallPending; establishing=JANELA/prone→BLOQUEIO; LIBERAÇÃO decide por POSE; Rising com readback; pausa blackout/DOWNED; wake→BLOQUEIO; END em line-exit ≤1 frame) + cap da janela (causa 1001) + sweeps/world-swap + edges do toggle.
- [ ] `TraumaBotFall.cs`: camada+logic+manager (hold X; re-hold interno; release diferenciado — X-expiry sem GetUp forçado × cura/toggle com `GetUp(false)`, flag `ForceGetUp`; hold estabelecedor no `OnLine`; ClearAll/ReleaseAll; sweep CR-01-02) + `RegisterLayer` gateado + interop SAIN + try/catch e null-guards no tick de IA.
- [ ] `InputPatches.cs`: branch do ciclo no `CantStandUpPatch` (reentry flag; prone qualquer h; fallback só subida; IsYourPlayer) + `FallAttemptCommandPatch` detecção-only — ambos com try/catch+LogError (PA-01-15).
- [ ] `SpeedLimitPatches.cs`: OR do FallCycle no `CanSprintPatch`; re-log ciente do cap da janela.
- [ ] `HealthPatches.cs`: guard `IsCycleEngaged` no bloco legado de estômago (:97-108) + log `stomach legacy suppressed (fall-cycle)` (PA-01-09).
- [ ] `TraumaLegsConsumer.cs`: interim REMOVIDO (`IsN2Tier` sem FallCycle); `OnTransition` trata entrada em FallCycle como SAÍDA do 003 (remove cap+bookkeeping — PA-01-02); poda inclui FallCycle; toggle-off via `CancelKind(InvoluntaryCrouch)` (PA-01-04).
- [ ] Plugin: binds seção 8 + rename `Fall Cycle` (ON) + órfã deletada em `MigrateOrphanedConfigKeys` + `AddComponent` + `RegisterLayer`; csproj com BigBrain (`Private=false`) + entrada `DrakiaXYZ-BigBrain.dll` ADICIONADA ao mapa do `compile-mod.sh` (obrigatório — PA-01-10).
- [ ] `PROPRIEDADES.md` (seção 8 + tabela Renomeadas) + `/update-mod-graph` no commit da entrega.
- [ ] Smoke test (greps por AC): AC1 ciclo completo (fall EXECUTED → Blocked → attempt BLOCKED 1 som/2s → Released → Rising → Window → re-queda interna; timers F12 mudados valem na fase SEGUINTE); AC2 analgésico no bloqueio → END ≤1s + levanta livre; expiração → cai ≤1s; AC3 cura 1 fratura → END + linha nova via 003; AC4 D2 (estômago zerado com ciclo ativo → `stomach legacy suppressed (fall-cycle)`, sem agachar — o `crouch ABSORB` do motor por estômago só é exercitável no 006, PA-01-09) + desmaio pausa/wake→Blocked; AC5 bot: HOLD ≥X sem atirar/girar, RE-HOLD ao levantar, cura → release limpo, headless (log); AC6 interim: linha Cair sem cap N2 permanente fora da JANELA (log 003 sem FallCycle); AC7 Fika: peer vê queda/prone/levantar e OUVE forte/leve + tentativa (voz nativa); espelho sem efeito próprio; AC8 reset entre raids + spawn ferido → JANELA sem toast/one-shot.
- [ ] Smoke extra: derrubar DURANTE vault/escada/BTR → DEFERRED e execução/cancelamento correto (abertura 7); extração deitado (D18/P4 (5)); Cair+desmaio no mesmo evento → `fall ABSORB (blackout)` + wake→Blocked; toggle OFF em cada fase (incl. DOWNED) → tudo desfeito, religar → JANELA.

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | ✅ | Consumidor herda o lifecycle do motor (untrack/reset); §5 `Update`: null-detect de GameWorld + world-swap (padrão 003) → `Disengage`/`ClearAll`; deadlines absolutos morrem com o bookkeeping; camada BigBrain é global mas `IsActive` consulta `_holds` (vazio fora de raid). |
| 2 | Filtro MainPlayer/Fika em todo ponto que reage a player — AP-02 | ✅ | Efeitos via motor (só donos — `IsOwnedHere`); FSM exige `IsYourPlayer`; branch do `CanStandAt` gateado `IsYourPlayer` (§2 — FikaBot passa pela base via SetPoseLevel:2149); `GamePlayerOwner` é local-only; bots só no dono (camada exige BotOwner — espelho sem brain, P6). |
| 3 | Alvos ofuscados/virtuais por assinatura; overrides auditados — AP-03 | ✅ | `CanStandAt` (ObservedMovementContext => true sem base-call :109-112; Client/NoInertia não sobrescrevem — P5 corrigida); `CanSprint` (auditado no 003); `TranslateCommand` (HideoutPlayerOwner tem override próprio :558; :564 é o ToggleProne interno — hideout fora do ciclo por construção); zero GClass hardcoded (soft-deps SAIN/BigBrain por nome com no-op/gate). |
| 4 | Mudança de estado via API canônica; side-effects mapeados — AP-04 | ✅ | Pose pelo funil vanilla (`SetPoseLevel`/`IsInPronePose` com readback da recusa `CanProne` — P4); negação replica o shape vanilla do `CanStandAt` (UsingMeds :3304-3309); bot pelo `BotLay` (caminho oficial da IA) + devolução ao árbitro (D14); cap via SpeedLimits + `UpdateSpeedLimitByHealth` no undo; voz via `Say`/`Speaker.Play` (side-effect do importance documentado — abertura 6). |
| 5 | Estado entre raids: raid1→exit→raid2, alt-F4/morte/MIA | ✅ | §5 Update: gw null/world-swap → Disengage+ClearAll; morte → untrack do motor → sweep `stale`; extração prone ok (P4 (5) — timer wall-clock); AC8 no smoke. |
| 6 | ConfigEntry sem ambiguidade (incl. estado neutro) — AP-05 | ✅ | §3: defaults/faixas/pisos>0 justificados no tooltip; semântica "vale na próxima fase" explícita; estado neutro (OFF = linha Cair sem efeito; interim não volta) documentado; rename-at-delivery com órfã deletada. |
| 7 | Reentry-guard em método patcheado re-invocado — AP-07 | ✅ | `StandReentryFlag` + condição `h > PoseLevel` deixam os `SetPoseLevel` do próprio mod atravessarem o prefix do `CanStandAt` (que também é chamado por `SetPoseLevel` :2149); prefix não re-invoca o alvo; rampa roda com lockout OFF (fases Released/Rising); `FallAttemptCommandPatch` é read-only. |
| 8 | Flags/caches validados contra o contexto atual — AP-08 | ✅ | FSM re-valida `GetLine` a cada tick (§5 `TickHumanCycle` — poda `stale`); adiados re-validam `RequiredLine` no pump (003); re-hold de bot re-verifica linha; pausa re-avaliada por frame (blackout/DOWNED); deadlines nunca re-baseados por config viva (lição do relógio do blackout — CR-04). |

## Histórico

| Data | Evento |
|---|---|
| 2026-07-19 | Spec técnica criada via `/create-technical-spec` (motor 002/003 implementados v1.4.1 + trauma-primitives P4/P5/P6 com Rodada 2; memória: P-3.5 003 não validado in-game — risco herdado registrado em §7) |
| 2026-07-19 | Review técnica rodada 1 aplicada — 15 achados (1 bloqueador: identidade de consumidor no cap) |
