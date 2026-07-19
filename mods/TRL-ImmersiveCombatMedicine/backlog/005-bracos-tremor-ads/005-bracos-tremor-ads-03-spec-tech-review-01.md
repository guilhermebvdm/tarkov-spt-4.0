# 005 — Braços: Tremor + cancelamento de ADS escalonado · Review Técnica 01

**Mod:** TRL-ImmersiveCombatMedicine
**Spec técnica revisada:** [005-bracos-tremor-ads-02-spec-tech.md](005-bracos-tremor-ads-02-spec-tech.md)
**Data:** 2026-07-19

> Análise crítica ADVERSARIAL da spec técnica (rodada 1). Cada ponto recebe um ID `PA-01-MM`. Resolver até zerar bloqueadores antes de `/code-mod`.
>
> `Memória consultada: snapshot Sessão 2 (2026-07-11) + pendências · afetam esta review: [P-3.5 — 003 v1.4.1 ENTREGUE, validação in-game pendente; o 005 reutiliza o mesmo motor/registry/padrões de consumidor], [P-3.4 — diretiva do overhaul 003→008 + rastro de premissas p/ item 011 (P-005-A/B já registradas na spec)] · nenhuma pendência 🔴`

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 8 · Total: 8

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | A — Gap | 🟡 | `TearDownLocal` ambíguo: raid-end/world-swap NÃO pode `ForceResidue` no AHC morto (falta `Discard`) | ✅ Aplicado |
| PA-01-02 | B — Edge | 🟡 | Throttle de voz queima a única voz da janela quando `Speaker.Play` é engolido (Busy de importance ≥100 / Blocker de GRUPO em squad coop) | ✅ Aplicado |
| PA-01-03 | C — Erro | 🟡 | Postfix do PWA gateado em `Active` (só Started) deixa o gap Added→Started descoberto — usar `Existing` | ✅ Aplicado |
| PA-01-04 | A — Gap | 🟡 | `Invoke` de reflection roda DENTRO do dispatch desprotegido do motor (`StateChanged?.Invoke` sem try/catch) — exceção aborta a consolidação do frame | ✅ Aplicado |
| PA-01-05 | B — Edge | 🟢 | Watchdog sem backoff nem supressão da remoção PRÓPRIA (`Remove` anula `Owned` DEPOIS do `ForceResidue`) | ✅ Aplicado |
| PA-01-06 | B — Edge | 🟢 | Âncora do tremor fica no braço JÁ CURADO após cura parcial (idempotência nunca re-ancora) — ícone da UI de saúde no membro errado | ✅ Aplicado |
| PA-01-07 | A — Gap | 🟢 | Gate `FikaBackendUtils.IsHeadless` da recomendação (b) do P9 omitido sem registrar o desvio | ✅ Aplicado |
| PA-01-08 | A — Gap | 🟢 | Âncora "fika :738-751" no §6 sem nome de arquivo (é `ObservedPlayer.cs`) | ✅ Aplicado |

## Veredito das âncoras (foco nº 1)

Verificação completa contra decompiles do assembly REAL em `scratchpad/spike001/` (ActiveHealthController.cs, ProceduralWeaponAnimation.cs, BreathEffector.cs, Player_real.cs, PhraseSpeakerClass.cs, SpeakerManager.cs, GClass2291), `references/eft-decompiled/Assembly-CSharp/EFT/Player.cs`, `references/fika-plugin/` (2.3.4), o código do motor 002/consumidor 003 em `modded/` e `docs/trauma-primitives.md` P2/P5/P9:

| Grupo | Verificadas | Falhas estruturais | Drift menor |
|---|---|---|---|
| PWA/BreathEffector (gate do analgésico, escritores de TremorOn) | 6 | 0 | 0 |
| ActiveHealthController (AddEffect/Tremor/GClass3008/ForceResidue/method_15-16/stim/Pain) | 12 | 0 | 0 |
| Player.cs (funil SetAim, eventos, wiring do flag, Say/Speaker) | 16 | 0 | 0 |
| Fika 2.3.4 (FikaClientFirearmController, Observed*, ToggleAimPacket, FikaPlayer, ObservedPlayer) | 8 | 0 | 1 (PA-01-08 — âncora sem filename) |
| Motor 002 / consumidor 003 / legado (Engine, Resolver, EngineState, Movement/Health/State/Locale, Plugin) | 18 | 0 | 0 |

Confirmações load-bearing dignas de nota:

- **Gate do analgésico** (`PhysicalConditionUpdated`): spike001/ProceduralWeaponAnimation.cs:1175-1192 — `if (full & OnPainkillers) Breath.TremorOn = false;` em :1182-1186, else `TremorOn = (full & Tremor) != 0` em :1189. **Os ÚNICOS escritores de `TremorOn` no jogo são essas duas linhas** (grep em PWA/BreathEffector/Player_real/LocalPlayer_real: BreathEffector só LÊ em :182/:219). Não existe re-set por frame — o postfix + write direto cobrem todas as rotas.
- **`PhysicalConditionUpdated` é event-driven**, não por frame: assinado em `MovementContext.PhysicalConditionChanged` (Player_real.cs:28659; unsubscribe :30576). Custo do postfix conforme a spec (§7).
- **Dupla assert / shake dobrado: impossível** — consumo é `flag = TremorOn || Fracture` (OR booleano, BreathEffector.cs:182) e escrever `true` duas vezes é idempotente; `TremorOn` só modula a taxa do random (:219), não soma amplitude.
- **`AddEffect<TEffect>(EBodyPart, float?, float?, float?, float?, Action<TEffect>)`** — spike001/ActiveHealthController.cs:3514-3538: assinatura de 6 parâmetros bate com o `Invoke(..., {armPart, 0f, null, null, null, null})` do stub; `GInterface331` (Tremor implementa — :3117) PULA o merge → sempre instância nova ✓; residued do mesmo tipo/parte são force-removidos ANTES do Create ✓ (cobre flap Remove→Apply). Não-ambiguidade do `GetMethod("AddEffect")` provada em RUNTIME contra a DLL real (trauma-primitives P2 §Provas — única sobrecarga com esse nome; `method_14`/overloads MedEffect têm outros nomes).
- **`GClass3008`** nested PÚBLICO com `Existing` (Added|Started), `Active` (Started), `ForceResidue()`/`ForceRemove()` — spike001/ActiveHealthController.cs:29,219-233,550-570,637-662 ✓. `DefaultWorkTime => +Infinity` (:383) ✓; `Tremor` nested `protected` com `DefaultDelayTime` do globals (:3117-3122) ✓ — `delayTime=0f` obrigatório confirmado.
- **Escapes dos lookups vanilla**: tremor-filho do Pain nasce em `Head` (:2103-2114); stim negativo chama `method_16<Tremor>(EBodyPart.Head)` (:2789-2806); `method_15/16` são FirstOrDefault por tipo+parte (:3615-3634) — âncora em braço escapa ✓. **Nenhum removedor por-frame de Tremor existe no vanilla** (analgésico NÃO remove efeito — `DoPainKiller` só adiciona PainKiller; `RemoveNegativeEffects` é edge de stim) — loop infinito do watchdog não tem gatilho vanilla (ver PA-01-05 p/ defesa contra mod externo).
- **Destruição/restauração de membro NÃO purga o tremor**: `DestroyBodyPart` (:3867-3877) só seta IsDestroyed+eventos; `RestoreBodyPart` (cirurgia, :3891-3907) chama `method_44`+`method_36` — e **`method_36` é só SendNetworkSyncPacket** (:4573), não remoção de efeitos (`method_17` remove só Bleeding e é exclusivo do `FullRestoreBodyPart`).
- **Funil ÚNICO de mira — auditoria AP-03 completa**: repo Player.cs — `ToggleAim` :13695-13702 (colateral RemoveLeftHandItem/SetCompassState confirmado), `SetAim(int)` :13705-13709, `SetAim(bool)` público **virtual** :13711-13743 (Blindfire early-return :13713-13716; mounted :13721-13724; `AimingInterruptedByOverlap=false` :13729; `CurrentOperation.SetAiming(value)` :13731). **Grep exaustivo**: o ÚNICO call site de `CurrentOperation.SetAiming(value)` com valor potencialmente true é o próprio SetAim (:13731); TODAS as atribuições diretas `IsAiming = isAiming` estão em overrides `SetAiming` das operations (alcançáveis só via SetAim) ou são `= false` (saída — nunca bloqueada); `ChangeAimingMode` (:13765-13797) só cicla `AimIndex`, não seta mira. Fast-slot re-aim usa `ToggleAim` (:10670-10673); restauração pós-overlap idem (:13062-13070, flag só re-setável com `IsAiming==true` em :13062). Setter `IsAiming` :12136-12168 com `AimingChanged(value)` SÓ em mudança real ✓; evento `OnAimingChanged` público :20037, invocador :20146-20149 ✓. `UsableItemController` é funil separado (:21442-21461) — fora de escopo conforme funcional ✓.
- **Overrides de `SetAim(bool)`** (grep decompile+fika): `FikaClientFirearmController` (fika :216-227) chama `base.SetAim` ✓ e **a condição de pacote foi re-derivada**: `IsAiming != isAiming || (aimingInterruptedByOverlap && IsAlive)` com flag capturado ANTES do base — durante lockout o flag é inalcançável (cleared pelo nosso `SetAim(false)` em :13729; re-set exige `IsAiming==true`) → **skip realmente não emite pacote** ✓ e sem dessync (dono e espelho ambos não-mirando). `BotFirearmController` (sealed : FikaClientFirearmController) só sobrescreve `WeaponDirection` — bots DESCEM ao corpo patchado → guard `IsYourPlayer`/`IsAI` obrigatório e presente ✓; `AIFirearmController` vanilla idem; `ObservedFirearmController` sobrescreve `SetAim(int)`→`SetObservedAim`→`IsAiming` sobrescrito SEM base (fika :49-68,148-161) ✓; `ToggleAimPacket.Execute` → `SetAim(int)` do observed (:28-34) ✓. `RadioTransmitter`/`RangeFinder`/`FikaClientUsableItemController` são família UsableItem (funil separado) ✓.
- **Espelho não vê tremor**: `ObservedPlayer.OnHealthEffectAdded` (fika ObservedPlayer.cs:737-746) só toca som de fratura — nunca seta condição; `OnHealthEffectRemoved` vazio (:748-751); PWA do espelho nunca recebe `PhysicalConditionUpdated` de efeito sincado, e o `ReferenceEquals` do postfix blinda de qualquer forma ✓.
- **Voz**: `PhraseSpeakerClass.Play(EPhraseTrigger, ETagStatus, bool demand, int? importance)` (spike :175) — `Busy && importance <= Int_0` → skip (:206-210) e importance=100 explícita fura ✓; `Player.Say` NÃO repassa importance (:28799-28829) ✓; `Player.Speaker` público (Player.cs:24347) ✓; `FikaPlayer.OnPhraseTold` → PhrasePacket p/ qualquer trigger (fika :1093-1103) ✓. **Gate independente descoberto**: `SpeakerManager.FreeToSpeak` (spike SpeakerManager.cs:106-131) consulta Blockers de GRUPO (GClass2291:50-59, armados por `bankPlayed.Blocker` a cada fala do grupo) e NÃO é furado por demand/importance → PA-01-02.
- **Motor 002**: linhas de braço enum 20-23 = `TraumaEngineState.cs:25-28` ✓; `ResolveArms` = `TraumaMatrixResolver.cs:40-54` (semântica com/sem analgésico idêntica à spec) ✓; avaliação chama ResolveArms em `TraumaEngine.cs:531` ✓, reconciliação :627-648 ✓; `GetLine` :48, `SubscribeWithSnapshot` :72, `IsOwnedHere` :111, `Registry.Register` :132, `TraumaConsumerId.ArmsEffects` existe ✓. **Publicação é consolidada no Update** (`MarkDirty` nos handlers :437-476 → `ConsolidateDirty` :483 → `StateChanged` :565) — `ApplyLine`/`AddEffect` NÃO rodam dentro do dispatch de evento do AHC ✓ (mas ver PA-01-04); `rec.Lines[i] = to` ANTES do Invoke → `GetLine` no watchdog vê a linha nova ✓.
- **Legado**: `MovementPatches.cs:119-139` = bloco exato da fadiga (polling+SetAim(false)+voz "TryAim"), ÚNICO leitor de `AimingFatigueTimers` no modded (grep) ✓; `MovementPatches.cs:47` (SetAim no blackout — de quebra prova o mecanismo do corner D3: desmaio derruba a mira PELO FUNIL → `OnAimingChanged(false)` → timer reseta) e :123 ✓; `HealthPatches.cs:113-119` = voz "Arm" ✓; `TraumaState.cs:22` declaração + :43 ResetAll ✓; `TraumaState.BlackoutTimers` keyed por ProfileId (MovementPatches:21) — guard de blackout da voz viável ✓.
- **Config §8**: seções existentes no Plugin = `1. Geral (Trauma)` / `2. Mecanicas (Trauma)` / `3. Balanceamento (Trauma)` / `4. Keybinds (Medic)` / `5. Trauma 2.0 (Motor)` / `6. Trauma 2.0 (Consumidores)` / `7. Trauma 2.0 (Pernas)` — **"8. Trauma 2.0 (Braços)" NÃO colide** (não existe "5. Debug"; Debug Test Consumer vive na seção 6 com attr advanced) ✓; placeholder `"Arms Effects (item 005)"` default false em Plugin:113 ✓; blocos de migração mojibake/rename em Plugin:239-262/:264-289 ✓; campos `ConfigMasterEnabled`/`ConfigTrauma2Enabled`/`ConfigConsumerArmsEffects` existem com esses nomes ✓; faixa do lockout 1.0–1.5 default 1.5 = decisão 17 + funcional ✓.
- **D13**: RecoilRework/FOVFix POSTFIX-only no mesmo alvo (P9 evidências IL rr.il:1719/ff.il:4283-4316) — postfix roda com skip, idempotente ✓.
- **`IsBodyPartBroken`/`IsBodyPartDestroyed`** existem e são o que o motor já usa (TraumaEngine.cs:524-526) — `PickArmAnchor` viável ✓.
- **Timer/lockout**: âncora `max(edge, entrada na linha)` + re-âncora em TODA mudança de linha (ApplyLine) cobre comportamento 3/AC-5 ✓; desmaio reseta via SetAim(false) do blackout (acima) ✓; lockout por ProfileId sobrevive troca de arma e é zerado nos sweeps ✓; hold/toggle = premissa P-005-A registrada (abertura 3) ✓.

## Categorias

- **A — Gaps de Especificação:** informações ausentes que ambiguam a implementação
- **B — Edge Cases:** cenários válidos não cobertos
- **C — Erros de Lógica:** pressupostos errados, contradições, código incompatível com SPT 4.0+

## Impacto

- 🔴 **Bloqueador** — impede implementar ou causa bug garantido em ponto central
- 🟠 **Forte** — comportamento errado garantido em cenário relevante
- 🟡 **Médio** — comportamento errado em cenário plausível / gap que ambigua a implementação
- 🟢 **Menor** — qualidade/clareza, não bloqueia

---

## Pontos

### PA-01-01 · A — Gap de Especificação · 🟡 Médio

**`TearDownLocal` ambíguo: no raid-end/world-swap NÃO pode haver `ForceResidue` num AHC morto — falta um `Discard` separado do `Remove`**

**Problema:** O stub do `Update` (spec §5) comenta no branch world-null "padrão N1: mundo morreu — efeito/hooks morreram com o Player; **só limpar bookkeeping**" mas chama `TearDownLocal("raid-end")` — a MESMA função usada em `ApplyLine` para `To == None` ("state-exit") e no toggle-off, caminhos que EXIGEM `TraumaTremor.Remove` → `Owned.ForceResidue()`. O stub de `TraumaTremor.Remove` sempre tenta `ForceResidue` quando `Owned.Existing` (spec §5, TraumaTremor.cs). A spec não define qual comportamento `TearDownLocal` tem em cada razão — o implementador vai escolher um dos dois e errar o outro. O padrão do 003 é explícito no código entregue: raid-end/world-swap = bookkeeping-only (`TraumaLegsConsumer.cs:157-175` — "caps morrem com os MovementContexts; só limpar bookkeeping"), toggle-off = desfaz na hora (:179-190).

**Por que importa:** `ForceResidue` no fim de raid dispara `method_0` → eventos do AHC de um Player destruído + `HealthController.method_31`/sync de rede (AHC:462-476; method_36=SendNetworkSyncPacket, AHC:4573) no meio do teardown da sessão Fika — no mínimo eventos/pacotes em objeto morto a cada fim de raid, no pior exceção logada por raid. O inverso (discard-only no state-exit) deixaria tremor permanente após cura — violando AC-1.

**Sugestão:** Especificar em §5 dois pontos de saída no `TraumaTremor`: `Remove(reason)` (com `ForceResidue` — usado em state-exit, rebaixamento p/ None e toggle-off, inclusive downed `IsAlive=false` — lição CR-02) e `Discard(reason)` (só `Owned = null; OwnedPlayer = null;` + log — usado nos sweeps world-null/world-swap do Update). `TearDownLocal(reason)` escolhe por parâmetro (ex.: `bool worldDead`). Espelhar a redação do §9 check 1/5 ("bookkeeping zerado sem tocar objetos destruídos") que já descreve a intenção correta.

**Decisão:**
- `[x]` Aceitar sugestão — ✅ Aplicado: spec §1.3/§4/§5/§8/§9 check 1 — `TraumaTremor` ganhou `Discard(reason)` (bookkeeping-only) separado do `Remove` (ForceResidue no AHC vivo) e `TearDownLocal(reason, worldDead)` escolhe por parâmetro: raid-end/world-swap = `Discard`, state-exit/toggle-off = `Remove` (padrão TraumaLegsConsumer.cs:157-175).

### PA-01-02 · B — Edge Case · 🟡 Médio

**Throttle de voz queima a única voz da janela quando `Speaker.Play` é ENGOLIDO — Busy com importance ≥100 ou Blocker de GRUPO em squad coop**

**Problema:** O stub `TryBlockReAds` (spec §5) seta `inst._lockoutVoicePlayed = true` ANTES de chamar `p.Speaker.Play(...)`, e ignora o retorno. Mas `Play` retorna `null` SEM tocar nada em dois gates que importance=100 NÃO fura: (a) `Busy && importance <= Int_0` — se outra frase de importance ≥100 estiver tocando, `100 <= 100` → skip (spike001/PhraseSpeakerClass.cs:206-210); (b) `SpeakerManager.FreeToSpeak(trigger, Id)` — Blockers por GRUPO de fala, armados por `bankPlayed.Blocker` a cada frase do grupo (spike001/SpeakerManager.cs:106-131 + GClass2291:50-59), checado ANTES de importance e sem bypass por demand (:211). O P5 registra exatamente isso como risco 3 ("Blockers de grupo podem raramente suprimir um trigger em squads"). O servidor é Fika Coop PVE multiplayer (memória: squads são o ambiente primário) — e o item 004 vai usar o MESMO trigger `OnAgony` no ciclo de queda (P5 Recomendação), alimentando o Blocker do grupo.

**Por que importa:** AC-6 exige "1 voz por janela de lockout" e "sem furo": no cenário squad + OnAgony com Blocker de bank ativo (ou frase importance-100 em andamento), a tentativa bloqueada fica MUDA a janela inteira — o flag já foi consumido. O log também mente (`voice=true` sem áudio), contaminando a validação in-game do AC-6.

**Sugestão:** No stub de `TryBlockReAds`: só marcar `_lockoutVoicePlayed = true` se `Speaker.Play(...)` retornar não-null (a assinatura retorna `TagBank` — prova P5); em retorno null, logar `voice=skipped(busy|blocked)` e deixar a PRÓXIMA tentativa da mesma janela tentar de novo (o custo é 1 chamada de Play por tentativa, cadência de input — sem hot path). Atualizar o texto do log em §5/§8.

**Decisão:**
- `[x]` Aceitar sugestão — ✅ Aplicado: spec §1.6/§5 (`TryBlockReAds`)/§6/§7/§8 — `_lockoutVoicePlayed = bank != null` (só consome a janela se o `Play` retornou TagBank); engolido → log `voice=skipped(busy|blocked)` e a próxima tentativa da mesma janela re-tenta; smoke AC-6 ganhou o cenário squad.

### PA-01-03 · C — Erro de Lógica · 🟡 Médio

**Postfix do PWA gateado em `Owned.Active` (== Started) deixa o gap Added→Started descoberto — usar `Owned.Existing`**

**Problema:** O stub `TremorVisualReassertPatch` (spec §5) early-returna com `!TraumaTremor.Owned.Active` ("Active = Started — AHC:231"). Mas `AddEffect` cria a instância em estado `Added` e a transição p/ `Started` acontece no tick do AHC — janela de até 1 frame em que `Existing==true` porém `Active==false` (AHC:219-233: `Existing` = Added|Started). Se um `PhysicalConditionUpdated` disparar NESSA janela com `OnPainkillers` setado (ex.: outra condição muda no mesmo frame da aplicação — `UsingMeds`, dano em perna), o vanilla força `TremorOn=false` (:1182-1186), o postfix pula, e o write direto do `Apply` é sobrescrito. Como o flag `EPhysicalCondition.Tremor` pode já estar true (tremor-por-dor coexistindo — cenário explícito do AC-2), o `Started` seguinte NÃO gera novo `PhysicalConditionUpdated` (flag sem mudança) — `TremorOn` fica false até a PRÓXIMA mudança de condição, que pode demorar minutos.

**Por que importa:** AC-2 ("tremor permanece visível sob analgésico") falha de forma intermitente e difícil de reproduzir — exatamente o tipo de bug que o smoke test 1 (abertura 5) não pega se o frame não colidir.

**Sugestão:** Trocar o gate do postfix para `TraumaTremor.Owned == null || !TraumaTremor.Owned.Existing` (§5, ArmsAimPatches.cs). `Existing` cobre Added+Started e continua false em Residued (fade não re-asserta — mesmo comportamento de hoje). Com `delayTime=0f` o estado Added dura ≤1 tick, então não há risco de assert prolongado pré-efeito.

**Decisão:**
- `[x]` Aceitar sugestão — ✅ Aplicado: spec §1.4/§2/§5 — gate do postfix trocado p/ `Owned == null || !Owned.Existing` (Added|Started — AHC:219-233) nos três pontos; Residued segue fora (fade não re-asserta).

### PA-01-04 · A — Gap de Especificação · 🟡 Médio

**`Invoke` de reflection roda dentro do dispatch DESPROTEGIDO do motor — exceção no consumidor aborta a consolidação do frame para o jogador**

**Problema:** `TraumaEngine.EvaluatePlayer` invoca `StateChanged?.Invoke(t)` SEM try/catch (TraumaEngine.cs:565; idem :312 no disable do master). O `OnTransition` do 005 chama `ApplyLine` → `TraumaTremor.Apply` → `_addTremor.Invoke(...)` — reflection sobre nomes ofuscados. A premissa P-005-B (abertura 4) cobre FALHA DE RESOLUÇÃO (EnsureResolved → no-op logado), mas não cobre `TargetInvocationException` em runtime (resolução OK, internals do AHC mudaram/estado inesperado): a exceção sobe pelo delegate, aborta o loop de regiões do `EvaluatePlayer` no meio (pula `TryPublishOneShot`/toast/regiões restantes daquele frame) e pode quebrar outros consumidores assinados depois. O precedente do 003 não tem esse risco (só APIs tipadas no handler).

**Por que importa:** Um throw recorrente no Apply (ex.: pós-update do EFT) degrada o MOTOR inteiro (pernas/estômago inclusos), não só o tremor — o oposto da degradação parcial que a P-005-B promete.

**Sugestão:** Especificar em §5: corpo de `TraumaTremor.Apply`/`Remove` com try/catch próprio — no catch, `LogError` 1× e `_resolveOk = false` (degrada tremor p/ no-op permanente da sessão, mantendo cancela-ADS/lockout — coerente com P-005-B). Opcionalmente citar no §9 check 7 que o handler `OnTransition` não deixa exceção escapar para o dispatch do motor.

**Decisão:**
- `[x]` Aceitar sugestão — ✅ Aplicado: spec §4/§5 (stubs Apply/Remove)/§7 abertura 4/§9 check 7 — try/catch próprio em `TraumaTremor.Apply`/`Remove` com `LogError` 1× + `_resolveOk = false` no catch (degradação p/ no-op da sessão mantendo cancela-ADS/lockout — extensão da P-005-B a runtime), citado no check 7.

### PA-01-05 · B — Edge Case · 🟢 Menor

**Watchdog sem backoff nem supressão da remoção própria (`Remove` anula `Owned` DEPOIS do `ForceResidue`)**

**Problema:** (a) `TraumaTremor.Remove` chama `Owned.ForceResidue()` e SÓ DEPOIS anula `Owned` (stub §5) — o `EffectResidualEvent` dispara SINCRONAMENTE dentro do ForceResidue, o watchdog vê `IsOurs(e)==true` e, no toggle-off (linha ainda ativa no motor), latcha `_reestablishPending=true` para uma remoção INTENCIONAL. Hoje é auto-corrigido (o religar re-valida `GetLine` e re-aplicar é idempotente/desejado), mas é fluxo acidental, não especificado. (b) Não há backoff no re-apply: nenhum removedor por-frame existe no vanilla (veredito das âncoras), mas um mod externo que remova Tremor por lookup a cada tick geraria churn AddEffect+HealthSyncPacket+log POR FRAME sem nenhum limite.

**Por que importa:** (a) é comportamento emergente que a rodada 2 do code-review vai tropeçar; (b) é defesa barata contra o risco residual que o próprio P2 registra ("mod/stim futuro que remova Tremor por lookup").

**Sugestão:** No §5: `Remove` copia `Owned` p/ local, anula os campos ANTES do `ForceResidue` (o watchdog então falha o `ReferenceEquals` contra `Owned==null` → sem pending espúrio); e o branch `_reestablishPending` do Update ganha piso de 0,5 s entre re-applies + `LogWarning` 1×/sessão quando exceder 3 re-applies na mesma linha (diagnóstico de conflito externo).

**Decisão:**
- `[x]` Aceitar sugestão — ✅ Aplicado: spec §1.3/§5 — `Remove` copia `Owned` p/ local e anula os campos ANTES do `ForceResidue` (Residual síncrono da remoção própria não latcha pending); campos `_nextReestablishAt`/`_reestablishCount`/`_reestablishStormWarned` novos com piso de 0,5 s + warn 1×/sessão após 3 re-applies na mesma linha (contagem zerada na mudança de linha).

### PA-01-06 · B — Edge Case · 🟢 Menor

**Âncora do tremor fica no braço JÁ CURADO após cura parcial — ícone da UI de saúde no membro errado**

**Problema:** Cenário: Z2 (âncora = LeftArm, o primeiro comprometido) → jogador cura/cirurgia o LeftArm com o RightArm ainda zerado → motor publica `ArmsTremorAdsCancel4s → ArmsTremor` → `ApplyLine` → `TraumaTremor.Apply` é NO-OP pela idempotência (`Owned.Existing==true` — a cirurgia não remove o efeito, ver veredito: RestoreBodyPart só sync) → a instância permanece ancorada no braço SAUDÁVEL. O wiring do flag é part-agnostic (visual correto), mas o efeito aparece na UI de saúde do membro errado (`DisplayableVariations` por parte — AHC:36) e, academicamente, a âncora curada re-entra no alcance de um futuro `method_16<Tremor>(LeftArm)` de terceiros.

**Por que importa:** A abertura 6 discute a ESCOLHA inicial da âncora, mas não o drift dela ao longo das curas — o critério "braço comprometido" fica falso silenciosamente.

**Sugestão:** Adicionar à abertura 6 / §5: em transição com instância viva, se `PickArmAnchor(p) != âncora atual` e a parte atual não está mais comprometida, re-ancorar (Remove+Apply na parte nova — o AddEffect já força-remove o residued da parte antiga) OU aceitar explicitamente o drift como limitação cosmética documentada (alternativa "sempre LeftArm" da abertura 6 elimina o problema por definição).

**Decisão:**
- `[x]` Aceitar sugestão (caminho principal: RE-ÂNCORA) — ✅ Aplicado: spec §1.3/§4/§5 (`Apply` + campo `OwnedAnchor`)/§7 abertura 6/§8 — com instância viva, `PickArmAnchor != OwnedAnchor` E parte antiga não mais comprometida → `Remove("re-anchor")`+`Apply` na parte nova; ambos comprometidos mantém (sem churn); corner de smoke test novo (ícone da UI migra).

### PA-01-07 · A — Gap de Especificação · 🟢 Menor

**Gate `FikaBackendUtils.IsHeadless` da recomendação (b) do P9 omitido sem registrar o desvio**

**Problema:** O P9 (§Headless) recomenda guard DUPLO: (a) prefix inerte p/ bots (adotado ✓) e (b) "o motor de cancela-ADS não deve nem armar no headless: gate por `FikaBackendUtils.IsHeadless` (fika FikaBackendUtils.cs:49) ... evita trabalho morto e voz acidental do player-shell do headless". A spec 005 declara "headless é no-op por construção" (§1.2) e não menciona o gate nem por que o dispensou. O argumento implícito (o player-shell do headless nunca recebe transição de braço porque não toma dano) é plausível mas não está escrito, e o consumidor AINDA assina eventos/roda Update no headless (trabalho morto que o P9 quis evitar).

**Por que importa:** Desvio silencioso de recomendação de primitiva verificada — a suíte do 009/D20 e o item 011 não vão saber se foi decisão ou esquecimento.

**Sugestão:** Registrar em §7 (ou abertura nova): ou adiciona o gate `IsHeadless` no `IsActive()` (1 linha, dependência já presente via FikaBridge), ou documenta o descarte com o argumento explícito ("shell do headless não recebe dano de braço → nenhuma transição Arms com IsYourPlayer chega ao consumidor; custo do Update residual aceito").

**Decisão:**
- `[x]` Aceitar sugestão (caminho principal: ADICIONAR o gate) — ✅ Aplicado: spec §1.2/§2/§5 (`IsActive()`)/§7 bullet novo/§8/§9 check 2 — `!FikaBackendUtils.IsHeadless` como 1ª condição do `IsActive()` (fika FikaBackendUtils.cs:49 confirmado neste worktree; `Fika.Core` já é referência hard do csproj), decisão registrada em §7.

### PA-01-08 · A — Gap de Especificação · 🟢 Menor

**Âncora "fika :738-751" no §6 sem nome de arquivo**

**Problema:** O fluxo de dados (§6) cita "peer: efeito sinca como DADO (HealthSyncPacket) mas ObservedPlayer descarta o visual (fika :738-751)" sem nomear o arquivo. A âncora é real — `references/fika-plugin/Fika.Core/Main/Players/ObservedPlayer.cs:737-751` (`OnHealthEffectAdded` só toca som de fratura; `OnHealthEffectRemoved` vazio) — mas é a única citação do documento sem caminho, num claim load-bearing do AC-9 (a convenção do repo é `arquivo.cs:linha`).

**Por que importa:** Âncora não-resolvível quebra a verificação mecânica da rodada 2 e do code-review (grep por arquivo falha).

**Sugestão:** Trocar por `fika ObservedPlayer.cs:737-751` no §6 (mesma forma usada nas demais citações Fika da spec).

**Decisão:**
- `[x]` Aceitar sugestão — ✅ Aplicado: spec §6 — âncora completada p/ `fika ObservedPlayer.cs:737-751` (range corrigido junto: 737, não 738).

## Histórico

| Data | Evento |
|---|---|
| 2026-07-19 | Review técnica 01 criada via `/review-technical-spec` (rodada 1 adversarial: verificação de âncoras contra spike001 + decompile do repo + Fika 2.3.4 + motor/consumidor implementados; 0 falhas estruturais de âncora; 8 achados — 4 🟡, 4 🟢) |
| 2026-07-19 | Rodada 1 APLICADA na spec técnica — 8/8 achados aplicados (0 refutados), contadores zerados; PA-01-06 resolvido por re-âncora e PA-01-07 pelo gate `IsHeadless` (caminhos principais das sugestões) |
