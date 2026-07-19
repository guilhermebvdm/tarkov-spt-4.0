# 004 — Pernas: Cair + ciclo levantar 3s/15s · Code Review 01

> **Data:** 2026-07-19<br>
> **Status:** 🟢 Vivo (rodada 1 — achados abertos, aguardando `/apply-code-review`)<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [004-pernas-cair-ciclo-02-spec-tech.md](./004-pernas-cair-ciclo-02-spec-tech.md)<br>

---

Revisão adversarial de contexto limpo da implementação **v1.5.0** (commit `fd799426`). Escopo: os 13 arquivos do delta (`git show fd799426 --stat`) contra a spec técnica pós-2-reviews (24 PA aplicados), o motor 002/003 entregue e os decompiles de referência (`references/eft-decompiled/`, `references/fika-plugin/`).

**Contadores:** 🔴 0 · 🟠 0 · 🟡 2 · 🟢 2 — total 4 achados.

## Veredito das 3 divergências registradas pelo implementador

| # | Divergência | Veredito |
|---|---|---|
| (a) | Keys novas na seção `8. Trauma 2.0 (Queda)` (não na 7) | **Não é divergência** — a spec §3 pós-reviews já manda seção 8 (linha 56 da spec); binds, PROPRIEDADES e faixas 1–10/5–60/5–120 batem literalmente. Correto não colidir com a seção `7 (Pernas)`. |
| (b) | Flag `LayerStopped` no `Hold` refinando o PA-02-02 | **Correta e NECESSÁRIA** — sem ela, o sweep "Released com linha morta" (Pump, TraumaBotFall.cs:173-177) poderia remover a entrada marcada por cura NO MESMO frame da marca, ANTES de o árbitro BigBrain chamar `Stop()` no tick de IA seguinte → `ConsumeRelease` não acharia a entrada e o `GetUp(false)` da cura se perderia (exatamente o bug (a) do PA-02-02). O flag só é setado pelo próprio `Stop()` (ConsumeRelease com `ForceGetUp=false`, :88), distinguindo "camada JÁ parada" de "Stop() pendente". `ReleaseAll` (:221-225) usa o mesmo critério. |
| (c) | Religar do 003 pula `LegsFallCycle` no establishing (TraumaLegsConsumer.cs:206) | **Correta e NECESSÁRIA** — `LineTargetPercent` devolve o alvo N2 para QUALQUER linha ≠ N1 (inclusive `LegsFallCycle`, por design do cap da janela); sem o skip, religar `Legs Effects` mid-ciclo re-aplicaria a causa 1000 à linha Cair = interim ressuscitado, violando o handoff PA-01-02. Consistente com o `OnTransition` (:92-98) e a poda (:224). |

## Limpo em

- **Ordem StateChanged→OneShot (PA-01-03):** predicado de engage `TryGetOneShotDeadline + cd > Time.time` (TraumaFallCycleConsumer.cs:82) ≡ condição de supressão do motor (TraumaEngine.cs:590) — sem falso engage; `FallPending` generalizada cobre entrada E re-queda da janela (transição ANTES da chamada, cap removido — :307-310, PA-02-04); troca de linha intra-ciclo invisível (From==To não publica).
- **Contabilidade do cooldown one-shot:** todos os caminhos executam OU refundam — null-mc (TraumaPose.cs:144-149), absorção blackout (consumer :102-107), cancel de adiado por linha/pausa/disengage/raid-end (TraumaPose.cs:246/:332/:349/:365), bot não-held (TraumaBotFall.cs:131-134); entradas `Internal` nunca refundam (não houve publish) e nunca re-ancoram (`ExecuteFall` :168/:179/:184); refund casa por deadline (`Mathf.Approximately`, TraumaEngine.cs:132) e a entrada sai da fila no cancel → double-refund impossível; `DEFER-SKIP` protege a entrada não-interna do dedup (TraumaPose.cs:200-207).
- **CanStandAt (PA-01-05/P5):** branch condicional, nunca blanket — prone nega qualquer `h` (fecha também o `CanSit` do setter de `IsInPronePose`, MovementContext.cs:688-696 — porta/loot negados SÓ no caso prone, colateral aceito), fallback agachado nega só subida (`h > PoseLevel + 0.05f`); `IsYourPlayer` obrigatório; `StandReentryFlag` com try/finally no retry de prone (TraumaPose.cs:306-312); espelhos imunes por construção (`ObservedMovementContext.CanStandAt => true` sem base-call, fika ObservedMovementContext.cs:109-112; Client/NoInertia não sobrescrevem); branch de blackout intacto; downed não vaza (branch dentro de `IsAlive`, ciclo PAUSED cobre).
- **BigBrain (PA-02-05):** `[BepInDependency("xyz.drakia.bigbrain", SoftDependency)]` no plugin; guard do Chainloader em `RegisterLayer()` SEM tipos BigBrain no corpo; tipos isolados em `RegisterLayerCore()` `[MethodImpl(NoInlining)]` + try/catch TypeLoad/FileNotFound; patching por classe tolera tipos não-carregáveis (Plugin.cs:198-208, padrão CR-01-14/CR-02); brains = lista literal do P6 (bosses/followers fora — abertura 4).
- **Ciclo de vida Hold×Stop (PA-02-02/PA-01-08):** releases só MARCAM; `ConsumeRelease` no `Stop()` é o ponto único de consumo (remove só com `ForceGetUp`); X-expiry retém a entrada que arma o RE-HOLD; `NextPosibleGetUp = now + X` no `Start()` (PA-02-07, write depois do `IsLay=true`); sweep CR-01-02 cobre morto/despawn/`GetLine==None` com fake-null (`p == null`).
- **Handoff 003↔004:** duas direções sem cap duplo nem undo cruzado (causas 1000/1001 distintas, min-composição nativa); gate de sprint do 004 NO TOPO do Postfix, antes dos early-returns do 003 (SpeedLimitPatches.cs:29-34, PA-02-01); re-log do cap 1001 antes do gate `IsActive` do 003 (:53-63); `EnableSprint(false)` incondicional no Apply (TraumaSpeedCap.cs:42, PA-02-09); sweeps dos dois consumidores não interferem (`CancelKind` no toggle-off do 003 — PA-01-04).
- **Config/entrega:** seção 8 com defaults/faixas/tooltips = spec §3; rename-at-delivery com órfã DELETADA sem copiar valor + `Config.Save` (Plugin.cs:314-334, lição CR-03-01); PROPRIEDADES fiel aos `Config.Bind` literais (tabela Renomeadas atualizada); versão 1.5.0 nos 3 pontos (BepInPlugin/csproj/log); entrada `DrakiaXYZ-BigBrain.dll` no mapa do compile-mod.sh idêntica ao padrão Fika.Core (idempotente, PA-01-10).
- **Higiene entre raids / fake-null:** `TraumaVoice.Clear()` + `TraumaSpeedCap.Clear()` + `TraumaBotFall.ClearAll()` nos DOIS blocos de fronteira (gw null e world-swap, consumer :182-199, PA-02-08); `p is null` × `p == null` usados nos pontos certos (gerenciado × destruído); `_pronePending`/`_deferred` limpos por `CancelAll`/`Disengage`; deadlines absolutos morrem com o bookkeeping; remoção de cap downed-safe (TraumaSpeedCap.cs:50-58, lição CR-02-01 do 003).
- **Coop:** efeitos dono-only em todos os pontos novos (FSM `IsYourPlayer`; `FallAttemptCommandPatch` em `GamePlayerOwner` = local por construção; bots só onde `BotOwner` vive); voz pelo caminho nativo (`Speaker.Play`/`Say` tipados — peers via PhrasePacket), zero protocolo custom no wire; `pause` por `!IsAlive` cobre DOWNED com record vivo.

## Achados

### CR-01-01 · B — Bug latente · 🟡 Médio
**Holds fantasma de bot sem camada: BigBrain ausente OU brain fora da lista (boss/follower/Marksman) ganham bookkeeping de ciclo sem efeito real.**
**Local:** `TraumaBotFall.cs:43-60` (`RegisterLayer` não grava nenhum flag de sucesso), `:96-108` (`OnLine` cria hold sem consultar registro/brain), `:180-203` (`Pump`: X-expiry → `RELEASE`, bot de pé → `RE-HOLD` imediato) × `HealthPatches.cs:110` e `TraumaPose.cs:96` (consumidores de `IsCycleEngaged`).
**Problema:** `OnLine` segura QUALQUER bot dono com linha Cair. Se a camada não existe para aquele bot — (i) BigBrain ausente (registro nem tentado) ou (ii) BigBrain presente mas o brain do bot fora da lista de `AddCustomLayer` (bosses/followers, e também `Marksman`, o scav sniper, que não está na lista :68) — ninguém deita o bot: a entrada expira o X, marca `Released`, o Pump vê o bot DE PÉ com linha viva e RE-HOLDa no frame seguinte → par de logs `RELEASE (x-expiry)`/`RE-HOLD` a cada X s por bot enquanto a linha durar. Pior: `IsHeld` fica true nesses intervalos → `IsCycleEngaged(bot)` true → o guard de estômago (HealthPatches) e a absorção D2 (`AbsorbIfCycleEngaged`, que o 006 vai exercitar) tratam como "bot no chão" um bot que está de pé combatendo — arbitragem D2 errada por estado fantasma. `OnFallOneShot` também re-ancora cooldown de uma queda que nunca aconteceu (:126-129).
**Por que importa:** A spec promete "BigBrain ausente → bots sem ciclo" (§1.9/§7) e "bot-boss com linha Cair fica SEM ciclo" (abertura 4) — "sem ciclo" deveria significar sem bookkeeping com efeitos colaterais, não hold fantasma + churn de log. Cenário concreto: scav sniper (Marksman) com 2 pernas quebradas em raid normal, BigBrain presente.
**Sugestão:** (1) flag estático `_layerRegistered` setado no fim de `RegisterLayerCore()`; `OnLine`/`EstablishFromSnapshot` retornam cedo sem ele (o refund do `OnFallOneShot` não-held já cobre o publish). (2) Para o caso (ii), guardar a lista de brains num `HashSet<string>` estático e comparar com `bo.Brain.BaseBrain.ShortName()` no `OnLine` (mesmo critério que o BigBrain usa para anexar a camada) — ou registrar como premissa aceita no item 011 se o churn de boss for tolerado de propósito (aí só o (1) é obrigatório).
**Resolução:** ✅ Resolvido (v1.5.1) — aplicadas as DUAS partes: `_layerRegistered` no fim do `RegisterLayerCore` + `HasLayerFor(bot)` (lista única `LayerBrains`/`LayerBrainSet` compartilhada com o `AddCustomLayer`, critério `Brain.BaseBrain.ShortName()` ≡ `IsAffectedBySettings` do BigBrain) gateando `OnLine`/`EstablishFromSnapshot`; sem camada → `bot fall no-layer` logado, sem hold, sem `IsCycleEngaged` fantasma.

### CR-01-02 · B — Bug latente · 🟡 Médio
**Corrida pump-antes-da-pausa: queda adiada executa no frame de entrada do blackout ANTES de a FSM pausar — `EnterBlocked` + grito OnAgony com o jogador inconsciente (residual do PA-02-03).**
**Local:** `TraumaLegsConsumer.cs:233` (`TraumaPose.PumpDeferred()` no Update do 003) × ordem de `AddComponent` (Plugin.cs:179-180 — 003 ANTES do 004 ⇒ Update do 003 roda primeiro no frame) × `TraumaFallCycleConsumer.cs:120` (cinto do `OnFallExecuted` só checa `_phase == Paused`) × `:237-249` (entrada da pausa — e o `CancelFallsFor` que protegeria — só rodam no `TickHumanCycle` do 004, DEPOIS do pump do 003).
**Problema:** Com uma queda adiada por D7 na fila (entrada ou re-queda da janela em vault/escada/BTR), se o blackout começa entre o `TickHumanCycle` do frame N-1 e o Update do 003 do frame N (janela = quase o frame inteiro), o pump do 003 processa a entrada ANTES de a FSM entrar em `Paused`: o blackout já forçou prone (HealthPatches.cs:84 no próprio hit; MainLoopPatch :35-39 por frame), o guard D7 pode já ter liberado (vault terminou) → `ExecuteFall` cai no passo already-prone → `onExecuted` → `_phase` ainda é `FallPending` (não `Paused`) → cinto não dispara → `EnterBlocked("fall-executed")` + `TraumaVoice.PlayStrong` — **OnAgony de um desmaiado, replicado aos peers via PhrasePacket** — e churn Blocked→Paused no tick seguinte. O PA-02-03 fechou o one-shot que CHEGA no blackout (consumer :102) e a fila após a ENTRADA da pausa; a janela entre "blackout começou" e "FSM pausou" ficou aberta porque o pump é chamado por OUTRO componente antes do tick do 004.
**Por que importa:** É exatamente o sintoma que o PA-02-03 quis eliminar (voz de dor de inconsciente + churn de fase), num cenário de combate plausível (re-queda adiada no vault + tiro no tórax no mesmo segundo). Sem dano permanente (o wake re-pausa e depois re-Blocked), mas comportamento audível errado garantido quando a coincidência ocorre.
**Sugestão:** Espelhar o predicado de pausa no cinto do `OnFallExecuted`: `if (_phase == FallPhase.Paused || TraumaState.BlackoutTimers.ContainsKey(p.ProfileId) || TraumaState.IsFainted || p.HealthController == null || !p.HealthController.IsAlive) return;` (mesmo predicado de :235-236 — sem fase, sem voz; o wake conduz, já prone). Alternativa equivalente: gate no dispatch de fall do `PumpDeferred` (entrada de humano fica adiada enquanto o predicado valer), mantendo o cinto atual.
**Resolução:** ✅ Resolvido (v1.5.1) — cinto do `OnFallExecuted` espelha o predicado de pausa do `TickHumanCycle` (blackout/IsFainted/downed direto, além de `_phase == Paused`): execução chegada na janela pump-antes-da-pausa fica sem fase e sem voz, o wake conduz.

### CR-01-03 · C — Gap de entrega · 🟢 Menor
**`/update-mod-graph` não rodou no commit da entrega (item do checklist §8 da spec).**
**Local:** spec §8 ("`PROPRIEDADES.md` … + `/update-mod-graph` no commit da entrega") × `git log references/graphs/` — última regeneração do grafo do ICM é do item 003 (`899d87a8`, 637 nós); `fd799426` não toca `references/graphs/`.
**Problema:** O grafo navegável do mod ficou 4 arquivos novos + ~1.1k linhas atrás do código (TraumaFallCycleConsumer/TraumaBotFall/TraumaSpeedCap/TraumaVoice invisíveis às queries de `/code-review`/`/create-technical-spec` futuras).
**Por que importa:** O item 005 (braços) declara reuso do `TraumaVoice` — a spec dele vai nascer consultando um grafo desatualizado.
**Sugestão:** Rodar `scripts/update-graphs.sh` (ou o skill `/update-mod-graph`) e commitar junto da aplicação desta review.
**Resolução:** ⏸ Deferido ao fechamento — o orquestrador regenera e commita o grafo no fechamento do item 004.

### CR-01-04 · E — Robustez · 🟢 Menor
**Handlers de evento do motor sem try/catch nos entry points — exceção de consumidor aborta a publicação do motor no meio.**
**Local:** `TraumaFallCycleConsumer.OnTransition`/`OnOneShot` (:64/:94 — corpo direto, sem isolamento) × `TraumaEngine.cs:565/:597` (`StateChanged?.Invoke`/`OneShotPublished?.Invoke` dentro de `EvaluatePlayer`, sem try/catch no publisher).
**Problema:** Uma exceção em qualquer ponto da cadeia `OnOneShot → TryInvoluntaryFall → SetPoseLevel/IsInPronePose` (vanilla) ou `OnFallExecuted → Speaker.Play` propaga para dentro de `EvaluatePlayer` e mata a publicação das regiões/records restantes daquele frame (e o `LogTransition`/toast da própria transição). O PA-01-15 blindou patches e camada de IA; os subscribers do motor ficaram de fora (precedente herdado do 003 — o 004 só ampliou a superfície com chamadas vanilla de pose e voz dentro do handler).
**Por que importa:** O motor é o barramento de TODOS os consumidores (003/004, 005+ futuros); um consumidor não deveria conseguir derrubar a publicação dos outros. Nenhum caminho de throw concreto foi demonstrado (guards de null cobrem os óbvios) — por isso E, não B.
**Sugestão:** try/catch + `LogError` no corpo de `OnTransition` e `OnOneShot` dos dois consumidores (padrão dos patches). Opcional (motor, fora do escopo 004): invocar subscribers isoladamente.
**Resolução:** ✅ Resolvido (v1.5.1) — `OnTransition`/`OnOneShot` dos DOIS consumidores (004 e 003) ganharam wrapper try/catch + `LogError` (padrão Core-method); o isolamento de subscribers no motor fica como opcional fora do escopo do 004.

## Resolução

Aplicada em **v1.5.1**: CR-01-01 ✅ · CR-01-02 ✅ · CR-01-04 ✅ (build 0 erros, sem warnings novos — só os 16 Harmony003 pré-existentes de HealthPatches; DLL implantada em D:\SPT). CR-01-03 ⏸ deferido ao fechamento do item (orquestrador regenera/commita o grafo).

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-19 | Guilherme | Rodada 1 criada (revisor adversarial de contexto limpo): 0 🔴 · 0 🟠 · 2 🟡 · 2 🟢; veredito das 3 divergências do implementador — (a) não-divergência, (b) e (c) corretas e necessárias. |
| 2026-07-19 | Guilherme | Aplicação da rodada 1 (v1.5.1): CR-01-01 (gate `_layerRegistered` + `HasLayerFor` por brain), CR-01-02 (predicado de pausa completo no cinto do `OnFallExecuted`), CR-01-04 (try/catch nos handlers dos DOIS consumidores); CR-01-03 deferido ao fechamento. |
