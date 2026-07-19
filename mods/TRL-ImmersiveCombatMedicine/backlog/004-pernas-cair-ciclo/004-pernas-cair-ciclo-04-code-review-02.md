# 004 — Pernas: Cair + ciclo levantar 3s/15s · Code Review 02

> **Data:** 2026-07-19<br>
> **Status:** ✅ Aprovado (0 🔴 · 0 🟠 — 2 achados 🟢 aplicados em v1.5.2)<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [004-pernas-cair-ciclo-02-spec-tech.md](./004-pernas-cair-ciclo-02-spec-tech.md)<br>

---

Rodada 2 sobre a **v1.5.1** (commit `ac68a21f`, fixes da rodada 1 sobre a v1.5.0 `fd799426`), por revisor adversarial de contexto limpo. Parte A: verificação de que os 3 fixes aplicados (CR-01-01/02/04) estão CORRETOS (não só presentes); CR-01-03 (grafo) segue deferido ao fechamento — fora do escopo desta rodada. Parte B: caça a regressões introduzidas pelos próprios fixes + varredura final do item contra a spec funcional.

Arquivos revisados: `TraumaFallCycleConsumer.cs`, `TraumaBotFall.cs`, `TraumaSpeedCap.cs`, `TraumaVoice.cs`, `TraumaPose.cs`, `TraumaLegsConsumer.cs`, `SpeedLimitPatches.cs`, `InputPatches.cs`, `HealthPatches.cs`, `TRLImmersiveCombatMedicinePlugin.cs`, csproj — com `TraumaEngine.cs` como contrato. Evidência do critério BigBrain extraída da **DLL real instalada** (`D:\SPT\BepInEx\plugins\DrakiaXYZ-BigBrain.dll` via ilspycmd — lição do falso-negativo de decompile incompleto), não de decompile de referência.

**Contadores:** 🔴 0 · 🟠 0 · 🟡 0 · 🟢 2 — total 2 achados novos.

## Verificação dos fixes da rodada 1

### CR-01-01 (gate `_layerRegistered` + `HasLayerFor`) — ✅ fix CORRETO

- **Equivalência do critério confirmada na DLL real:** o BigBrain anexa a camada num prefix de `BaseBrain.Activate` (`BotBaseBrainActivatePatch.PatchPrefix`) quando `LayerInfo.AffectsBot(bo)` → `IsAffectedByLayer` → `ExcludeLayerHelpers.IsAffectedBySettings(brainNames, roles)` = `roles.Contains(Profile.Info.Settings.Role) && brainNames.Contains(Brain.BaseBrain.ShortName())`. O overload de 3 args usado pelo mod (`BrainManager.AddCustomLayer(type, brains, prio)`) delega com `roles = AllWildSpawnTypes` (todos os valores do enum) → o teste de role é vácuo e **só o brain decide** — exatamente o que `HasLayerFor` reproduz (`TraumaBotFall.cs:86-91`). Equivalência exata, sem divergência.
- **Lista única sem divergência:** `LayerBrains` (`:36-37`) é a MESMA `List<string>` passada ao `AddCustomLayer` (`:75`); `LayerBrainSet` (`:38`) deriva dela no static init (ordem de declaração correta). O BigBrain guarda a referência sem mutá-la — impossível divergir.
- **Sem hold fantasma / `IsCycleEngaged` indevido:** hold só nasce em `OnLine` com `HasLayerFor` true (`:128-132`) ou via `EstablishFromSnapshot` → `OnLine` (gate redundante `!_layerRegistered` em `:168`); `IsHeld`/`IsCycleEngaged` só enxergam holds criados. `_layerRegistered` é setado UMA vez, DEPOIS de `AddCustomLayer` retornar (`:78`), dentro do try que captura TypeLoad/FileNotFound — registro falhou ⇒ flag false ⇒ nenhum hold jamais nasce. O flag nunca transita true→false (set único no Awake, plugin carrega 1× por processo) — **não existe** o cenário "hold que já existia quando o flag virou false".
- **Caminho degradado logado + contabilidade fechada:** `bot fall no-layer` (`:130`) só em transição (sem spam por frame); o publish do one-shot é refundado pelo `OnFallOneShot` não-held (`:158-161`), coerente com o padrão BotCrouchDip. Bot no-layer com estômago zerado volta a receber o agachar legado (HealthPatches.cs:110 vê `IsCycleEngaged=false`) — comportamento CORRETO (o bot está de pé combatendo; era exatamente a distorção D2 que o fix eliminou).

### CR-01-02 (cinto do `OnFallExecuted` com predicado completo) — ✅ fix CORRETO

- O cinto (`TraumaFallCycleConsumer.cs:146-149`) espelha o predicado de pausa do `TickHumanCycle` (`:263-264`) termo a termo: `_phase == Paused` ∨ `BlackoutTimers.ContainsKey` ∨ `IsFainted` ∨ `HealthController == null` ∨ `!IsAlive` — fecha a janela pump-antes-do-tick (003 roda `PumpDeferred` antes do tick do 004 pela ordem de AddComponent, Plugin.cs:179-180): execução chegada com blackout já iniciado retorna SEM fase e SEM voz; o tick seguinte pausa (`CancelFallsFor` :270 no-opa, entrada já consumida) e o wake conduz (`EnterBlocked("resume")` :282). DOWNED do Fika coberto pelo mesmo `!IsAlive` (lição CR-02-01 do 003) — e o revive termina em BLOQUEIO, nunca em fase órfã.
- **Contabilidade correta no caminho do cinto:** quando o retorno dispara, `ReportOneShotExecuted` JÁ rodou dentro do `ExecuteFall` (a queda aconteceu de fato — already-prone do prone forçado do blackout); nenhum refund é devido, cooldown ancorado na execução. Sem double-report.
- **Nenhum caminho residual de voz com jogador inconsciente:** `PlayStrong` tem 2 chamadores — `OnFallExecuted` (agora gateado) e `FallAttemptCommandPatch` (exige `IsBlockedPhase`, e sob blackout/downed a fase é `Paused`); `PlayLight` só na transição Released→Rising, inalcançável com o tick pausado. Fechado.
- Nota: o predicado é uma CÓPIA inline (era a sugestão literal da rodada 1), não um helper compartilhado — hoje idêntico, risco futuro registrado como CR-02-02.

### CR-01-04 (try/catch nos entry points) — ✅ fix CORRETO

- Wrapper padrão Core-method nos 4 pontos: `TraumaFallCycleConsumer.OnTransition`/`OnOneShot` (`:64-73`/`:105-113`) e `TraumaLegsConsumer.OnTransition`/`OnOneShot` (`:86-95`/`:118-126`) — todos com `LogError` identificando o handler (nada engolido sem log; `ex.Message` segue o padrão da casa nos handlers de camada).
- **Semântica de refund preservada no caminho de erro:** exceção entre o publish e o `ReportOneShotExecuted/Canceled` deixa o cooldown ancorado no PUBLISH — **não é órfão novo**: (i) pré-fix, a mesma exceção pulava o Report* igualmente E ainda matava a publicação das regiões restantes no `EvaluatePlayer` (o fix estritamente melhora); (ii) o cooldown auto-expira em 3–5 s (`_cooldownUntil` por deadline, TraumaEngine.cs:590-595) — nunca permanente; (iii) o guard `Mathf.Approximately` do cancel (:132) continua impedindo refund cruzado. Residual aceito (mesma classe E da rodada 1, sem caminho de throw concreto): exceção no meio de `TryInvoluntaryFall` pode deixar a FSM em `FallPending` até a saída da linha (poda stale), pausa (re-conduz via Paused→Blocked) ou toggle — sem timers, sem cap, sem negação; degradação segura.

## Verificações adicionais (Parte B — limpas)

- **Ordem de init do gate:** `RegisterLayer()` roda no Awake do plugin (Plugin.cs:181), após `ModLogger` (:66) e após os AddComponent (:178-180) — mas antes de qualquer raid: o snapshot do motor está vazio no subscribe, nenhum `OnLine` pode preceder o registro. `EstablishFromSnapshot` (toggle religar) passa pelo mesmo `OnLine` por-bot.
- **Predicado × downed sem transição espúria:** o motor NÃO publica para `!IsAlive` (`EvaluatePlayer` early-return, TraumaEngine.cs:511-513) — por isso o subset D3 do `OnOneShotCore` (:123, blackout/fainted sem IsAlive) permanece suficiente: publish não chega para downed; o cinto e o tick cobrem o resto.
- **Religar mid-blackout:** `Engage` estabelecedor não consulta o predicado de pausa — no pior caso 1 frame de fase `Blocked` (sem voz — `EnterBlocked` não toca som) e o tick seguinte pausa; wake termina em `Blocked` correto. Churn cosmético de log, sem efeito de gameplay — aceito.
- **Interações dos fixes com os sweeps:** `Pump`/`ReleaseAll`/`ClearAll` intocados pelos fixes; entradas Released retidas continuam armando RE-HOLD apenas para holds legítimos (nascidos com camada). `bot fall no-layer` não cria entrada — nada para os sweeps limparem.
- **Exclusão teórica de camada por terceiros:** `BrainManager.RemoveLayer("TraumaDowned", ...)` de outro mod descolaria camada×`HasLayerFor` — nenhum mod da carga faz isso; registrado como não-cenário.
- **ACs da spec funcional (gap Categoria C):** varridos os 8 ACs + 13 corners verificáveis por leitura — fases/timers com deadline absoluto (F12 vale na próxima fase), janela conta do "de pé efetivo" (`rose`), bloqueio nega só subida no fallback agachado, deitar voluntário na janela → BLOQUEIO sem re-derrubar (:328), saída da linha destrava na hora (`Disengage("line-exit")`), extração/raid-end/world-swap limpam (Update :210-228), toggle-off downed-safe (`RemoveGuarded` do 1001), interim do 003 fora (`IsN2Tier` sem FallCycle + poda), bots X s sem combate (camada prio 90 + `DownedIdleLogic` sem steering), coop dono-only (voz nativa tipada; espelhos sem override alcançável). **Nenhum gap novo** — os únicos critérios não fecháveis por leitura são os de validação in-game já conhecidos (distinção audível dos sons; meio-levantar visual).

## Achados novos

### CR-02-01 · B — Bug latente · 🟢 Menor
**Janela sem retry do `HasLayerFor`: brain ainda não vinculado no instante da transição = bot fica sem hold pelo episódio inteiro da linha.**
**Local:** `TraumaBotFall.cs:89` (`bot.AIData?.BotOwner?.Brain?.BaseBrain` — null-conditional vira `false`) × `:128-132` (`OnLine` consome a transição com `no-layer` e retorna) × ausência de caminho de re-tentativa (a linha não re-publica; `Pump` só gerencia holds existentes; `EstablishFromSnapshot` só roda no religar do toggle).
**Problema:** `StandartBotBrain.Activate` atribui `BaseBrain` e o attach do BigBrain acontece no MESMO call stack (prefix de `BaseBrain.Activate`) — mas se a transição de linha Cair chegar ANTES da ativação do brain (bot adotado já ferido na janela de spawn), `HasLayerFor` devolve false para um bot que TERÁ a camada milissegundos depois, e o episódio inteiro daquela linha fica sem hold (bot de pé com 2 pernas quebradas até a linha trocar).
**Por que importa:** janela minúscula (bot precisa entrar na linha Cair entre a criação do BotOwner e o `Activate` — na prática exige spawn ferido + adoção imediata) e o caminho degrada LOGADO com refund correto — mas é uma perda permanente por episódio, não por frame.
**Sugestão:** nenhuma ação obrigatória. Se quiser fechar: em `OnLine`, tratar `BaseBrain == null` (brain ainda não vinculado) como caso distinto de "fora da lista" — ex.: não consumir (deixar o próximo evento/religar tentar) ou logar `no-layer (brain-pending)` para diagnóstico. Registrável como premissa no item 011.
**Resolução:** ✅ Resolvido (v1.5.2) — `ProbeLayer` distingue `Pending` (BaseBrain null) de `NoLayer`: entrada `brain-pending` re-checada no `Pump` com backoff 0.5s até o BaseBrain vincular (→ hold atrasado, shape estabelecedor) ou o episódio sair da linha/morrer; `NoLayer` definitivo mantém o log/refund atuais; pendências limpas em `ReleaseAll`/`ClearAll`.

### CR-02-02 · D — Arquitetura · 🟢 Menor
**Predicado de pausa duplicado inline em dois pontos — divergência futura por edição parcial.**
**Local:** `TraumaFallCycleConsumer.cs:146-148` (cinto do `OnFallExecuted`) × `:263-264` (`TickHumanCycle`) — mesma expressão de 4 termos copiada, sem helper comum.
**Problema:** hoje idênticos (verificado termo a termo); o risco é o item 006/007 (estômago/desmaio 2.0) tocar a definição de "pausado" num ponto e não no outro — exatamente a classe de bug que o CR-01-02 corrigiu.
**Sugestão:** extrair `private static bool IsPauseCondition(Player p)` e consumir nos dois pontos (o subset D3 do `OnOneShotCore` fica fora — é intencionalmente menor porque o motor não publica para `!IsAlive`). Uma linha de refactor, pode ir junto de qualquer entrega futura.
**Resolução:** ✅ Resolvido (v1.5.2) — helper `IsPauseCondition(p)` extraído (doc-comment registra que o subset D3 do `OnOneShotCore` fica FORA de propósito) e consumido nos dois pontos (cinto do `OnFallExecuted` + `TickHumanCycle`).

## Veredito

Os 3 fixes da rodada 1 estão **corretos** — CR-01-01 com equivalência provada contra a DLL real do BigBrain, CR-01-02 fechando a corrida sem caminho residual de voz/fase, CR-01-04 sem mascarar refund (cooldown auto-expira; pré-fix perdia o mesmo Report* E a publicação do motor). Nenhuma regressão introduzida pelos fixes; varredura final da spec sem gap Categoria C. Os 2 achados novos são 🟢 opcionais (janela teórica de brain-pending; higiene de predicado duplicado) — **não bloqueiam o fechamento do item**, que segue pendente apenas do CR-01-03 (regeneração do grafo, já a cargo do orquestrador no fechamento).

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-19 | Guilherme | Rodada 2 (revisor adversarial de contexto limpo): 3 fixes da rodada 1 verificados ✅ corretos (critério BigBrain provado na DLL real via ilspycmd); 0 🔴 · 0 🟠 · 0 🟡 · 2 🟢 novos (CR-02-01 janela brain-pending sem retry; CR-02-02 predicado de pausa duplicado). Item liberado para fechamento. |
| 2026-07-19 | Guilherme | Aplicação dos 2 🟢 (v1.5.2, regra do usuário de aplicar todos): CR-02-01 (retry `brain-pending` com backoff 0.5s no Pump — `ProbeLayer` Pending×NoLayer×Ready) e CR-02-02 (helper `IsPauseCondition` como fonte única do predicado de pausa). Build 0 erros, sem warnings novos; DLL 1.5.2 implantada. |
