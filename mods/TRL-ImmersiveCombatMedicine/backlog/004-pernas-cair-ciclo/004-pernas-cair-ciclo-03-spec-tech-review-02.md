# 004 — Pernas: Cair + ciclo levantar 3s/15s · Review Técnica 02

**Mod:** TRL-ImmersiveCombatMedicine
**Spec técnica revisada:** [004-pernas-cair-ciclo-02-spec-tech.md](004-pernas-cair-ciclo-02-spec-tech.md)
**Data:** 2026-07-19

> Análise crítica ADVERSARIAL da spec técnica (rodada 2 — pós-aplicação da rodada 1). Cada ponto recebe um ID `PA-02-MM`. Resolver até zerar bloqueadores antes de `/code-mod`.
>
> `Memória consultada: snapshot "Estado atual" + pendências de mods/TRL-ImmersiveCombatMedicine/memory/sessions.md · afetam esta review: [P-3.5 — 003 v1.4.1 entregue, VALIDAÇÃO IN-GAME PENDENTE; o 004 estende TraumaPose/caps/gates — risco já registrado na spec §7], [P-3.4 — diretiva do overhaul + rastro de premissas p/ item 011] · nenhuma pendência 🔴`

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 9 · Total: 9

## Veredito da aplicação da rodada 1 (foco nº 1 desta rodada)

**Rodada 1 APLICADA com coerência: 15/15 achados propagados, 0 texto órfão da era "helper compartilhado", 0 âncora nova falsa.** Verificação feita contra o código do mod, `references/eft-decompiled/`, `references/fika-plugin/`, `scratchpad/spike001/` (BotLay/bigbrain/PhraseSpeaker/protótipos), `docs/trauma-primitives.md` P4/P5/P6, `D:\SPT\BepInEx\plugins` e `.agents/scripts/compile-mod.sh`:

| Verificação (r2) | Resultado |
|---|---|
| Causas 1000/1001 (PA-01-01/02) propagadas: §1.7, §2, §4, §5 (Apply/RemoveWindowCap "remove SÓ a 1001"), §6, §7 (abertura 3), §8 — sem resíduo de "extração do TraumaSpeedCap" | ✅ coerente |
| Handshake 003↔004 nas 2 direções no MESMO `StateChanged` (003 subscrito antes — Plugin.cs:156): Cair→N1/N2 = 003 `ApplyCap(1000)` + 004 `Disengage`(remove só 1001); N1/N2→Cair = 003 remove 1000 (`To==FallCycle` como saída) + 004 aguarda `OnOneShot` — sem undo cruzado nem cap duplo (min-composição `method_4()` :1798) | ✅ correto por construção — **exceto o gate de sprint (PA-02-01)** |
| Âncoras NOVAS da r1: `compile-mod.sh` mapa `resolve_references()` **:272-302** (grep BigBrain=0 confirmado; padrão Fika.Core.dll :295 replicável; csproj usa `References\` + HintPath — a edição descrita compila); `HealthPatches.cs` **:97-108** (bloco legado de estômago com `SetPoseLevel(0f,true)` :106, dano ≥35 + `!IsInPronePose` :103); Plugin **:265-287** (bloco rename-at-delivery "Legs Effects (item 003)"); `TraumaLocale.cs` **:18/:29/:66**; Plugin **:402** WakeLocalPlayer | ✅ todas reais |
| PA-01-07 (rampa com readback): `SetPoseLevel` recusa em `!CanStandAt(poseLevel)` (MovementContext.cs:2149 confirmado) e o `StandReentryFlag` no prefix **retorna true = roda o original** — o teto vanilla continua valendo durante a rampa | ✅ mecanismo correto |
| PA-01-03 (ordem StateChanged→OneShot): `StateChanged?.Invoke` :565 ANTES de `TryPublishOneShot` :571-572; predicado `TryGetOneShotDeadline + cd > Time.time` ≡ exatamente a condição de supressão do motor (:590) — engage-por-cooldown sem falso positivo/negativo p/ o humano | ✅ correto (bot: ver PA-02-06) |
| Bot (P6): brains {PmcBear,PmcUsec,PMC,Assault,CursAssault,ExUsec,ArenaFighter,Obdolbs} + prio 90 = recomendação literal do P6; `AddCustomLayer(Type,List<string>,int)` existe (bigbrain_BrainManager.cs:147-165); `BotLay.IsLay` setter :34-72, `NextPosibleGetUp` :22-23, `GetUp` ignora `withCheck` :182-188; GUID **`xyz.drakia.bigbrain`** confirmado (bigbrain_full/BigBrainPlugin.cs:10, v1.4.0) | ✅ âncoras corretas |
| Voz: `Player.Say(EPhraseTrigger, bool demand = false, ...)` — defaults reais (Player.cs:28799), o call `Say(OnBeingHurt, demand: true)` do stub compila; `Speaker.Play` c/ importance provado no p5-proto | ✅ |
| Config §3: faixas 1-10/5-60/5-120 = clamps dos stubs = spec funcional; rename `Fall Cycle (item 004)`→`Fall Cycle` bate com o bind real (Plugin.cs:111, default false) e o padrão :265-287 | ✅ consistente |

Os 9 achados abaixo são de segunda ordem: interações que a aplicação da r1 abriu (fases novas × pump × pausa; flag `ForceGetUp` × Stop() assíncrono; independência do 003 não propagada ao patch de sprint; mecânica de soft-dep do BigBrain).

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

### PA-02-01 · C — Erro de Lógica · 🟡 Médio

**Extensão do `CanSprintPatch` herda os early-returns do 003 — sprint na JANELA volta a depender do toggle/config do 003, contradizendo o contrato "independe do toggle 003"**

**Local:** spec §1.7 ("Gate de sprint estendido: `CanSprintPatch` também força `false` quando `GetLine == LegsFallCycle` + consumidor 004 ativo") e §4 ("`CanSprintPatch`: OR do 004") × `SpeedLimitPatches.cs:21-22` — o Postfix atual sai cedo em `if (!ConfigBlockSprintOnN2.Value) return;` e `if (!TraumaLegsConsumer.IsActive()) return;` ANTES de qualquer check de linha. Idem `UpdateSpeedLimitByHealthPatch` (`:41` — `if (!TraumaLegsConsumer.IsActive()) return;`) para o re-log que a spec manda "consultar também o bookkeeping do 004".

**Problema:** A spec descreve a extensão como "OR do 004" sem especificar POSIÇÃO. O OR natural (somar a condição do 004 ao check de linha em `:28`) fica ATRÁS dos dois early-returns do 003: com `Legs Effects` OFF ou `Block Sprint On N2` OFF, o branch do 004 nunca roda — o jogador SPRINTA na JANELA com 2 pernas quebradas. Isso contradiz três contratos da própria spec/funcional: "o cap da janela independe do toggle 003", "cap N2 bloqueia sprint na JANELA" e a premissa D18 revisada ("o dano aceito da janela é vault/climb PORQUE sprint está bloqueado"). O mesmo vale (só observabilidade) p/ o re-log RECOMPUTE: com 003 OFF o cap 1001 da janela nunca aparece no re-log.

**Por que importa:** A independência 003×004 foi o núcleo da resolução do PA-01-01/PA-01-02 — ela foi propagada ao cap (causas distintas) mas NÃO ao gate de sprint, que é metade do contrato "N2". Cenário: usuário desliga `Legs Effects` (ou `Block Sprint On N2`) mid-raid → JANELA vira corrida normal; AC7/AC1 falham num estado de config plausível.

**Sugestão:** Especificar no §4 (linha SpeedLimitPatches.cs) a ESTRUTURA, não só o OR: o branch do 004 avalia ANTES dos early-returns do 003 — ex.: no topo do Postfix, `if (__result && TraumaFallCycleConsumer.IsActive() && player != null && TraumaEngine.GetLine(player, TraumaRegion.Legs) == TraumaLine.LegsFallCycle) { __result = false; return; }` (resolvendo `player` uma vez, compartilhado com o caminho do 003), deixando os gates `ConfigBlockSprintOnN2`/`TraumaLegsConsumer.IsActive` valendo SÓ para o branch N1/N2 do 003. Decidir e documentar de propósito: o sprint da JANELA **não** respeita `Block Sprint On N2` (config do 003, seção 7) — é contrato fixo do ciclo (premissa p/ item 011). No `UpdateSpeedLimitByHealthPatch`, o re-log do cap 1001 consulta o bookkeeping do 004 antes do gate `TraumaLegsConsumer.IsActive()`.

**Decisão:**
- `[x]` Aceitar sugestão

**✅ Aplicado** — Branch do 004 movido para o TOPO do Postfix, ANTES dos early-returns do 003 (§1.7, §2, §4, §8 + smoke com `Legs Effects`/`Block Sprint On N2` OFF); re-log do cap 1001 consulta o bookkeeping do 004 ANTES do gate `IsActive` (:41); decisão registrada: sprint da JANELA NÃO respeita `Block Sprint On N2` (premissa p/ item 011).

### PA-02-02 · C — Erro de Lógica · 🟠 Forte

**Ciclo de vida do `Hold` × `Stop()` assíncrono: `ReleaseAll` limpa `_holds` antes de o `Stop()` consumir `ForceGetUp` — toggle-off NÃO levanta os bots; e o sweep nunca remove entradas `Released` de bot curado**

**Local:** spec §5 stub `ReleaseAll` ("toggle-off: ForceGetUp=true → Stop() com GetUp(false) (PA-01-08); **_holds limpos após o release**") × `TraumaDownedLayer.Stop()` (consulta `TraumaBotFall.ShouldForceGetUp(BotOwner.ProfileId)` → `TryGetValue` em `_holds`) × lifecycle do BigBrain: `Stop()` NÃO é chamado por `ReleaseAll` — é o árbitro que o chama no PRÓXIMO tick de IA do bot, quando `IsActive()` (=`IsHeld`) vira false (P6 evid.: bigbrain_BotBaseBrainUpdatePatch.cs:46-71). × `Pump()` step 3 (sweep remove só "morto/despawn/`GetLine==None`").

**Problema:** (a) Se `ReleaseAll` remove as entradas de `_holds` sincronamente (leitura literal de "_holds limpos após o release"), quando o árbitro chamar `Stop()` um tick depois, `ShouldForceGetUp` não acha a entrada → retorna false → `GetUp(false)` NUNCA roda — o corner explícito da funcional ("desligar o toggle do 004 mid-ciclo … libera bots") não entrega o levantar forçado; o bot fica deitado até alguma camada da IA decidir. (b) Se, para evitar (a), a entrada é RETIDA (Released=true + ForceGetUp=true), nenhum caminho a remove no release por cura: o sweep do `Pump` só limpa `GetLine==None`, e um bot curado tem `GetLine==LegsLimpN1/N2` — entrada `Released` órfã até `ClearAll` (viola o padrão CR-01-02 que a própria linha do §4 invoca). A spec não define QUANDO a entrada sai de `_holds` em relação ao `Stop()` assíncrono — e as duas leituras possíveis estão erradas em algum caminho.

**Por que importa:** O mecanismo `ForceGetUp` foi a resolução do PA-01-08 — mas ele só funciona se a entrada sobreviver até o `Stop()` consumi-la. Como especificado, ou o toggle-off quebra (AC do corner), ou o bookkeeping vaza (CR-01-02). Comportamento errado garantido em pelo menos uma das duas leituras.

**Sugestão:** Definir o ciclo de vida da entrada ATRAVESSANDO o Stop(): (1) `ReleaseAll`/`OnLine`(cura) apenas MARCAM `Released=true; ForceGetUp=<motivo>` — nunca removem; (2) o próprio `Stop()` remove a entrada de `_holds` após consumir `ShouldForceGetUp` (ponto único de consumo; `Stop()` roda no dono do bot, mesmo processo); (3) o sweep do `Pump` vira o cinto: além de morto/despawn/`GetLine==None`, remove entradas `Released` cujo `GetLine != LegsFallCycle` (cura processada com camada já parada) — mantendo `Released` com linha VIVA retida de propósito (é ela que arma o RE-HOLD do step 2). Documentar que `ClearAll` (mundo morto) segue removendo tudo sem `Stop()` (objetos destruídos). Atualizar os stubs `ReleaseAll`/`OnLine`/`Pump`/`Stop` e o §1.9.

**Decisão:**
- `[x]` Aceitar sugestão (com 1 precisão: o `Stop()` só REMOVE quando `ForceGetUp=true` — no X-expiry a entrada `Released` com linha viva fica retida, senão o item (3) da própria sugestão não teria o que reter p/ armar o re-hold)

**✅ Aplicado** — Ciclo de vida redesenhado em §1.9/§4/§5/§6: releases só MARCAM (`Released`+`ForceGetUp`); `Stop()` é o ponto ÚNICO de consumo via novo `ConsumeRelease(profileId)` (lê `ForceGetUp`, remove quando true; X-expiry retém a entrada que arma o RE-HOLD); sweep do `Pump` remove também `Released && GetLine != LegsFallCycle` (bot curado com camada parada) e `ReleaseAll` remove direto entradas já `Released` (nenhum `Stop()` virá); `ClearAll` documentado sem `Stop()`; smoke cobre toggle-off com bots deitados → `GetUp(false)`.

### PA-02-03 · B — Edge Case · 🟡 Médio

**`PumpDeferred` continua executando quedas adiadas com a FSM em PAUSED — queda "executa" como no-op de prone DURANTE o blackout/DOWNED: `EnterBlocked` + voz OnAgony com o jogador inconsciente**

**Local:** stub `Update` (chama `TraumaPose.PumpDeferred()` incondicionalmente após `TickHumanCycle`, inclusive com `_phase == Paused`) × `TryInvoluntaryFall` passo 2 ("`mc.IsInPronePose` → já no chão: `onExecuted(p, true)` SEM tocar pose") × `MovementPatches.cs:35-39` (blackout força prone POR FRAME) × stub `OnFallExecuted` (faz `EnterBlocked` + `TraumaVoice.PlayStrong` incondicionalmente) × spec §1.8(b)/funcional D3 ("desmaio pausa o ciclo em QUALQUER fase").

**Problema:** Cenário: queda adiada por D7 (FallPending da entrada, ou re-queda da JANELA durante vault/escada/BTR) → desmaio/DOWNED começa antes de a queda executar → FSM entra em `Paused`, mas a ENTRADA continua na fila e o pump continua rodando. O blackout força prone; no frame seguinte o pump re-valida (linha ainda `LegsFallCycle`, guards passam com o jogador no chão) e cai no passo 2: `IsInPronePose` → executa como no-op → `OnFallExecuted` → `EnterBlocked("fall-executed")` sobrepondo o `Paused` (1 tick de churn até o `TickHumanCycle` re-pausar) + `PlayStrong` — **grito de OnAgony com o jogador desmaiado**, replicado aos peers via PhrasePacket. Viola o D3 ("pausa em qualquer fase") e a absorção com refund do §1.8(c) (que só cobre o one-shot que CHEGA durante blackout, não a entrada já enfileirada).

**Por que importa:** O corner Cair+desmaio foi tratado no `OnOneShot` (absorção com refund), mas a fila adiada é um segundo caminho de entrada que a pausa não cobre — comportamento audível errado (voz de dor de um inconsciente) + churn de fase/log em cenário de combate plausível (vault + tiro no tórax).

**Sugestão:** Espelhar o predicado de pausa no dispatch de queda do pump: entradas `InvoluntaryFall` de humano ficam ADIADAS (sem cancelar) enquanto `TraumaState.BlackoutTimers.ContainsKey(id) || TraumaState.IsFainted || hc == null || !hc.IsAlive` — no wake, o `EnterBlocked("resume")` do TickHumanCycle assume e a entrada pendente executa como no-op de prone em seguida (aí sim `fall-executed` re-baseia o deadline de forma inócua, mesmo frame do resume) OU, mais simples: cancelar a entrada com refund no momento em que a FSM entra em `Paused` (mesmo tratamento do §1.8(c) — o wake re-avalia e entra em BLOQUEIO já prone, sem precisar da queda pendente). Qualquer um dos dois; especificar também que `OnFallExecuted` não toca voz quando a execução foi no-op por já-prone vindo do pump pós-pausa (ou condicionar `PlayStrong` a `_phase != Paused`).

**Decisão:**
- `[x]` Aceitar sugestão (variante simples: cancelamento com refund na entrada da pausa)

**✅ Aplicado** — Entrada da pausa cancela quedas pendentes do jogador com refund (`TraumaPose.CancelFallsFor(p, "paused")` no TickHumanCycle — mesmo tratamento do §1.8(c); wake re-avalia já prone → BLOQUEIO) + cinto: `OnFallExecuted` retorna inerte com `_phase == Paused` (sem fase, sem voz); §1.8(b), stubs, §6 e smoke atualizados.

### PA-02-04 · C — Erro de Lógica · 🟡 Médio

**Re-queda da JANELA adiada por D7: o case `Window` re-chama `TryInvoluntaryFall` TODO FRAME enquanto o deadline está vencido — log DEFERRED por frame e re-enqueue contínuo (a fase de espera criada no PA-01-03 só cobre a queda de ENTRADA)**

**Local:** stub `TickHumanCycle` case `Window` (`if (Time.time >= _phaseDeadline) { … TryInvoluntaryFall(…, internalFall: true, …) }` — nenhuma transição de estado quando a queda é ADIADA) × `TraumaPose.Defer` (hit de dedup ATUALIZA a entrada e LOGA `LogInfo` a cada chamada — TraumaPose.cs:118-126, :130) × spec §1.3 (fase `FallPending` definida SÓ para "queda de entrada ADIADA") × funcional corner 1 ("queda automática da janela DURANTE vault/escada/corda/BTR: adiada").

**Problema:** Quando a janela expira com o jogador em contexto D7 (vault/escada/BTR), `TryInvoluntaryFall` adia e a FSM permanece em `Window` com `Time.time >= _phaseDeadline` verdadeiro — no frame seguinte o case chama `TryInvoluntaryFall` DE NOVO: dedup-hit + `LogInfo "fall DEFERRED"` a cada frame (vault ≈ 60-100 logs; BTR = minutos de spam), violando a regra "nunca LogInfo por frame" (skill spt §6) e re-capturando `PublishDeadline` por frame. A r1 criou `FallPending` exatamente p/ esse shape na ENTRADA (PA-01-03), mas a re-queda da janela ficou fora.

**Por que importa:** Corner explícito da funcional com comportamento degradado garantido (spam de log em caminho quente + re-enqueue por frame); inconsistência interna da máquina de fases (mesma situação — "queda pendente na fila" — representada por fase dedicada num caminho e por busy-loop no outro).

**Sugestão:** Unificar: ao adiar a re-queda da janela, transicionar para `FallPending` (generalizar a doc da fase de "queda de entrada adiada" para "queda PENDENTE na fila — entrada ou re-queda"), removendo o cap da janela na transição (`RemoveWindowCap` — coerente com "sem timers, sem cap, negação OFF"); o fluxo existente já cobre o resto (callback → BLOQUEIO; poda stale; pausa; cancel). Registrar a decisão de o cap N2 NÃO valer durante o adiamento (contextos D7 não têm locomoção normal — premissa p/ item 011). Alternativa mínima: flag `_fallRequested` que impede re-chamar `TryInvoluntaryFall` enquanto houver entrada pendente do jogador (consulta à fila), mantendo a fase `Window`.

**Decisão:**
- `[x]` Aceitar sugestão (caminho principal — fase unificada, não a alternativa da flag)

**✅ Aplicado** — `FallPending` generalizada para "queda PENDENTE na fila — entrada OU re-queda da janela" (§1.3, enum); case `Window` transiciona `RemoveWindowCap()` + `_phase = FallPending` ANTES de chamar `TryInvoluntaryFall` (mesmo shape da entrada — execução imediata re-entra BLOQUEIO via callback; adiada não re-chama/loga por frame); decisão registrada: cap N2 não vale durante o adiamento (premissa p/ item 011); §1.4, §6 e smoke (1 log DEFERRED) atualizados.

### PA-02-05 · C — Erro de Lógica · 🟡 Médio

**Soft-dep do BigBrain quebra nas duas pontas: o guard do Chainloader vive no MESMO método que os tipos do BigBrain (TypeLoadException ANTES do guard com BigBrain ausente) e o plugin não declara `[BepInDependency]` (gate pode falso-negativar com BigBrain instalado)**

**Local:** stub §5 `RegisterLayer` (o `if (!Chainloader.PluginInfos.ContainsKey("xyz.drakia.bigbrain"))` e o `BrainManager.AddCustomLayer(typeof(TraumaDownedLayer), …)` estão no MESMO corpo) × `TraumaDownedLayer : CustomLayer` (herda tipo do BigBrain) × `TRLImmersiveCombatMedicinePlugin.cs:13` (só `[BepInPlugin]` — NENHUM `[BepInDependency]` no plugin hoje) × spec §7 ("Registro gateado por Chainloader: BigBrain ausente → bots sem ciclo + warn 1× (humano intacto)").

**Problema:** (a) No Mono, o JIT resolve os tokens do corpo INTEIRO ao compilar o método na primeira chamada: `typeof(TraumaDownedLayer)` exige carregar `TraumaDownedLayer`, cuja base `CustomLayer` vem da DLL do BigBrain — com o BigBrain AUSENTE, `TraumaBotFall.RegisterLayer()` lança `TypeLoadException`/`FileNotFoundException` NA ENTRADA do método, antes de o guard executar; a exceção estoura dentro de `Plugin.Awake` e derruba o restante do Awake — o "humano intacto" prometido pela spec vira plugin inteiro quebrado. (b) Sem `[BepInDependency("xyz.drakia.bigbrain", SoftDependency)]`, a ordem de load do BepInEx 5 não garante o BigBrain ANTES do ICM — `Chainloader.PluginInfos` é populado plugin a plugin, e se o ICM carregar primeiro o gate retorna false COM o BigBrain instalado (bots silenciosamente sem ciclo; hoje só funciona por sorte alfabética de path).

**Por que importa:** O caminho de degradação graciosa é promessa explícita da spec (§1.9/§7) e, como stubado, entrega o oposto (crash do Awake) no exato cenário p/ que foi desenhado; o (b) é o cenário dominante real (BigBrain SEMPRE instalado — dependência do SAIN) dependendo de ordem não contratada.

**Sugestão:** (a) Isolar o código que toca tipos do BigBrain num método separado `RegisterLayerCore()` anotado `[MethodImpl(MethodImplOptions.NoInlining)]`, chamado SÓ depois do guard do Chainloader; envolver a chamada em `try/catch (TypeLoadException/FileNotFoundException)` com warn (cinto para inlining/edge de runtime). O mesmo isolamento já é desnecessário no resto do `TraumaBotFall` (nenhum outro método referencia tipos BigBrain — manter assim, documentar a regra no §5). (b) Adicionar `[BepInDependency("xyz.drakia.bigbrain", BepInDependency.DependencyFlags.SoftDependency)]` ao `TRLImmersiveCombatMedicinePlugin` (GUID confirmado em bigbrain_full/BigBrainPlugin.cs:10) e registrar no §4 (linha do Plugin).

**Decisão:**
- `[x]` Aceitar sugestão

**✅ Aplicado** — (a) `RegisterLayer()` ficou só com o guard (zero tipos BigBrain no corpo) e chama `RegisterLayerCore()` `[MethodImpl(NoInlining)]` com try/catch `TypeLoadException`/`FileNotFoundException`; regra "nenhum outro método do TraumaBotFall referencia tipos BigBrain" documentada no §5; (b) `[BepInDependency("xyz.drakia.bigbrain", SoftDependency)]` adicionado ao stub do plugin e às linhas do Plugin em §4/§8; §1.9 e §7 explicam as duas pontas.

### PA-02-06 · B — Edge Case · 🟡 Médio

**Entrada de BOT com publish SUPRIMIDO pelo cooldown (re-quebra ≤3-5s após cura) cai no vazio: `OnLine` não recebe `Establishing` e a spec o descreve como caminho só-estabelecedor — o equivalente bot do corner PA-01-03 ficou sem tratamento definido**

**Local:** stub `OnLine(Player bot, TraumaLine to)` (assinatura SEM `establishing`; comentário trata `to == LegsFallCycle` como "transição ESTABELECEDORA — adoção mid-raid/spawn ferido") × consumidor `OnTransition` (`if (p.IsAI) { TraumaBotFall.OnLine(p, t.To); return; }` — repassa TODA transição, estabelecedora ou não) × motor: publish suprimido quando `now < deadline` (TraumaEngine.cs:590-593) sem re-publicação enquanto a linha persistir × §1.3 (o corner do cooldown foi resolvido p/ o HUMANO via `TryGetOneShotDeadline`).

**Problema:** Cenário não-estabelecedor com one-shot suprimido: bot cai (cooldown re-ancorado na execução), é CURADO da fratura e RE-QUEBRA a perna em ≤3-5s (tiroteio) → transição N1/N2→FallCycle publica `StateChanged` mas o `TryPublishOneShot` suprime → `OnFallOneShot` nunca chega. O texto do `OnLine` sugere gate em establishing — mas a assinatura nem recebe o flag, então há duas implementações plausíveis e DIVERGENTES: (i) implementador infere o gate por fora (ex.: consultar `t.Establishing` no consumidor antes de chamar) → bot com 2 pernas quebradas fica DE PÉ sem hold até a linha sair e re-entrar; (ii) `OnLine` segura em TODO `to == LegsFallCycle` → funciona, mas na entrada não-suprimida os DOIS caminhos rodam no mesmo frame (`OnLine` cria hold sem `ReportOneShotExecuted`; `OnFallOneShot` sobrescreve `_holds[id]` com Hold novo) — colisão benigna porém não especificada, e o refund de "MovementContext/BotOwner nulos" do `OnFallOneShot` fica contraditório com um hold já criado.

**Por que importa:** O humano ganhou o predicado de cooldown na r1 exatamente p/ este corner; o bot ficou com semântica ambígua onde uma das leituras produz bot imune ao ciclo por toda a duração da linha (AC5 falha).

**Sugestão:** Especificar `OnLine` como caminho ÚNICO e idempotente de entrada por transição: `to == LegsFallCycle` → `if (IsHeld(id)) no-op; else hold` SEM depender de establishing (cobre estabelecedora, suprimida-por-cooldown E a publicada — nesse último caso o `OnFallOneShot` do mesmo frame vira: `if (IsHeld) { ReportOneShotExecuted; return; }` — só re-ancora o cooldown, sem recriar o Hold). Atualizar §1.9 e os stubs `OnLine`/`OnFallOneShot`; registrar a premissa (entrada de bot é dirigida por transição; o one-shot só re-ancora cooldown) p/ item 011.

**Decisão:**
- `[x]` Aceitar sugestão

**✅ Aplicado** — `OnLine` especificado como caminho ÚNICO e idempotente de entrada por transição (`IsHeld` → no-op; guards BotOwner/MovementContext nulos → no-op SEM refund, não há publish atrelado); `OnFallOneShot` com hold existente SÓ re-ancora o cooldown (`ReportOneShotExecuted` + return) e o refund fica coerente (só quando os guards do OnLine falharam — nenhum hold contradiz); `EstablishFromSnapshot` reusa o mesmo caminho; §1.9 e premissa p/ item 011 registrados.

### PA-02-07 · C — Erro de Lógica (coerência interna) · 🟢 Menor

**§1.9 e stub divergem no valor de `NextPosibleGetUp` do Start(): `Time.time + X` vs `Time.time + 999f`**

**Local:** spec §1.9 ("Derrubar (`Start()`): … `NextPosibleGetUp = Time.time + X`") × stub §5 `TraumaDownedLayer.Start()` (`NextPosibleGetUp = Time.time + 999f; // re-stampado pelo hold` — e NADA no `Pump`/`Update` re-stampa).

**Problema:** Dois textos normativos dão valores diferentes para o mesmo write, e o comentário "re-stampado pelo hold" descreve um mecanismo que não existe em nenhum stub. Ambos os valores funcionam (999f depende de `Stop()` sempre zerar — garantido pelo lifecycle do BigBrain; X expira junto com o hold naturalmente), mas o implementador não sabe qual é o contrato — e com 999f um bug em `Stop()` (ex.: PA-02-02) deixa o bot 999s sem get-up vanilla.

**Sugestão:** Unificar em `Time.time + X` (§1.9): auto-consistente com o hold (expira junto), sem depender de `Stop()` p/ destravar a IA no caminho X-expiry, e o `Stop()` mantém o `NextPosibleGetUp = 0f` só como antecipação nos releases por cura/toggle. Remover o comentário "re-stampado pelo hold" do stub. (Nota: o setter vanilla `IsLay=true` já stampa `Time.time + DELTA_GETUP` — BotLay.cs:46-55 — o write do mod deve vir DEPOIS do `IsLay = true`, como o stub já ordena.)

**Decisão:**
- `[x]` Aceitar sugestão

**✅ Aplicado** — Stub do `Start()` unificado com o §1.9: `NextPosibleGetUp = Time.time + TraumaBotFall.X()` (`X()` promovido a internal); comentário "999f / re-stampado pelo hold" removido; nota do setter vanilla mantendo o write DEPOIS do `IsLay = true` (verificado no decompile real: references/eft-decompiled/Assembly-CSharp/BotLay.cs:46-54).

### PA-02-08 · A — Gap · 🟢 Menor

**`TraumaVoice._nextAllowed` (estático, keyed por ProfileId) sem ponto de limpeza declarado**

**Local:** stub §5 `TraumaVoice` (`private static readonly Dictionary<(string, bool), float> _nextAllowed`) × skill csharp §2 ("every static collection … documented clear point") × padrão do próprio 004 (`_holds` tem `ClearAll`; fila do TraumaPose tem `CancelAll`).

**Problema:** O dict de anti-spam cresce por (ProfileId, tipo) e nunca é limpo — entre raids acumula entradas mortas (ProfileIds de raids anteriores; timestamps no passado tornam o comportamento correto, mas o crescimento é aberto). Inofensivo em volume (~2 entradas/raid p/ o humano local hoje), mas o helper é declarado "reusável pelo 005 (P9)" — braços podem ampliar o uso, e a regra da skill pede o ponto de limpeza explícito.

**Sugestão:** Adicionar `TraumaVoice.Clear()` chamado no sweep de raid-end/world-swap do consumidor (junto de `TraumaBotFall.ClearAll()`), e uma linha no §5 documentando.

**Decisão:**
- `[x]` Aceitar sugestão

**✅ Aplicado** — `TraumaVoice.Clear()` adicionado ao stub como ponto de limpeza DECLARADO do dict estático (doc citando skill csharp §2 e o reuso pelo 005/P9), chamado nos DOIS blocos de sweep do `Update` do consumidor (gw null e world-swap), junto de `TraumaBotFall.ClearAll()`; refletido em §4, §8 e §9 (check 1). 

### PA-02-09 · A — Gap · 🟢 Menor

**`TraumaSpeedCap.Apply` não replica o corte de sprint EM CURSO (`EnableSprint(false)`) do padrão 003 — entrada na JANELA já sprintando (engage por cooldown/establishing) mantém o sprint corrente até o gate agir**

**Local:** spec §4 (TraumaSpeedCap: "Apply (Remove+Add, log calibração…)" — sem menção a sprint) × `TraumaLegsConsumer.ApplyCap` (`:134-135`: `if (IsN2Tier(line) && ConfigBlockSprintOnN2) mc.EnableSprint(false); // corta sprint EM CURSO; quem SEGURA é o gate CanSprint`) × §1.3 (engage direto em JANELA no thrash de analgésico — o jogador pode estar SPRINTANDO no instante em que o analgésico expira e a linha re-entra).

**Problema:** O contrato do 003 tem duas metades: o getter `CanSprint` SEGURA sprint novo e o `EnableSprint(false)` CORTA o sprint em andamento no momento do apply. O helper novo especifica só a primeira via `CanSprintPatch` (§1.7); no engage-de-pé (cooldown ativo ou establishing) um sprint em curso persiste até o state-machine re-consultar o getter — janela de frames correndo em velocidade plena com 2 pernas quebradas.

**Sugestão:** Adicionar ao contrato do `TraumaSpeedCap.Apply` (§4 + stub `ApplyWindowCap`): após o Remove+Add, `mc.EnableSprint(false)` incondicional ao aplicar o cap da janela (sem gate no `ConfigBlockSprintOnN2` — coerência com PA-02-01), citando o padrão `TraumaLegsConsumer.cs:134-135`.

**Decisão:**
- `[x]` Aceitar sugestão

**✅ Aplicado** — Contrato do `TraumaSpeedCap.Apply` ganhou `mc.EnableSprint(false)` INCONDICIONAL pós-Add (sem gate no `ConfigBlockSprintOnN2` — coerência com PA-02-01; corta o sprint em curso no engage-de-pé, quem segura é o `CanSprintPatch`), citando `TraumaLegsConsumer.cs:134-135`; propagado a §1.7, §4, stub `ApplyWindowCap`, §6 e §8.

---

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-02-01 | C | 🟡 | CanSprintPatch: OR do 004 atrás dos early-returns do 003 — sprint da JANELA dependente do toggle 003 | ✅ Aplicado |
| PA-02-02 | C | 🟠 | Hold × Stop() assíncrono: ReleaseAll limpa _holds antes do consumo de ForceGetUp + sweep sem caso p/ bot curado | ✅ Aplicado |
| PA-02-03 | B | 🟡 | PumpDeferred executa queda durante PAUSED — EnterBlocked + OnAgony com jogador inconsciente | ✅ Aplicado |
| PA-02-04 | C | 🟡 | Re-queda da janela adiada (D7) re-chama TryInvoluntaryFall todo frame — FallPending não cobre a re-queda | ✅ Aplicado |
| PA-02-05 | C | 🟡 | Soft-dep BigBrain: TypeLoad antes do guard (mesmo método) + falta [BepInDependency] | ✅ Aplicado |
| PA-02-06 | B | 🟡 | Entrada de bot com publish suprimido por cooldown — OnLine ambíguo (sem Establishing) | ✅ Aplicado |
| PA-02-07 | C | 🟢 | §1.9 (X) × stub (999f) divergem no NextPosibleGetUp do Start() | ✅ Aplicado |
| PA-02-08 | A | 🟢 | TraumaVoice._nextAllowed sem ponto de limpeza | ✅ Aplicado |
| PA-02-09 | A | 🟢 | TraumaSpeedCap.Apply sem o corte EnableSprint(false) na entrada da JANELA | ✅ Aplicado |

## Histórico

| Data | Evento |
|---|---|
| 2026-07-19 | Review 02 criada via `/review-technical-spec` (adversarial, contexto limpo, rodada pós-aplicação da r1). Veredito: r1 aplicada com coerência (15/15 propagados, 0 texto órfão, 0 âncora nova falsa — compile-mod.sh :272-302, HealthPatches :97-108, Plugin :265-287, TraumaLocale :18, GUID xyz.drakia.bigbrain, brains P6, SetPoseLevel :2149 verificados). 9 achados de 2ª ordem: 0 🔴 · 1 🟠 · 5 🟡 · 3 🟢 — interações fase×pump×pausa, ForceGetUp×Stop() assíncrono, independência 003 não propagada ao sprint, mecânica de soft-dep. |
| 2026-07-19 | Aplicação da rodada 2 via `/apply-code-review`: 9/9 achados ✅ Aplicados (0 refutados) — contadores zerados. Destaques: 🟠 PA-02-02 redesenhado com `ConsumeRelease` no `Stop()` (ponto único de consumo) + sweep p/ bot curado; PA-02-03 na variante simples (cancelamento com refund na entrada da pausa); PA-02-04 unificado na fase `FallPending` generalizada. Âncoras re-verificadas antes de citar (SpeedLimitPatches :21-22/:28/:41, TraumaLegsConsumer :134-135, TraumaPose :118-126/:130, BotLay.cs:46-54 no decompile real). Spec promovida a "Pronto para /code-mod (0 pendências)". |
