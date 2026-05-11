# Memory — stancesAndCameraPositionSPT4.0.11

Memória cronológica de sessões de chat (timestamps em GMT-3, aproximados quando não puderem ser inferidos com precisão). Cada entrada resume o que foi feito, decisões-chave, bugs encontrados, e estado pendente. Atualizada ao fim de cada sessão de trabalho.

> Por que existe: o usuário trabalha múltiplos chats em paralelo. Este arquivo evita que cada chat reabra do zero — futuras sessões podem carregar contexto ao ler as últimas entradas. Entradas são ordenadas por timestamp GMT-3; chats paralelos podem aparecer interleaved.

## Estado atual (snapshot ao fim da última sessão)

- **Status no mod-backlog.md:** item 001 🟢, item 002 🟢, item 003 🟢.
- **Build atual:** `D:/SPT/BepInEx/plugins/shwngFpsCameraStances4.dll` (~70KB), com 06-fix-01 aplicado (F4 patch target corrigido + Stance 2↔3 swap).
- **F4 (Snap on Fire):** corrigida em 06-fix-01, mas **não validada in-raid pelo usuário ainda** após o fix. Causa raiz do bug original documentada: virtual dispatch em C# bypassava Harmony quando override não chamava `base.SetTriggerPressed()` (apenas 1 de 14 overrides chama, ver `Player.cs:3184`). Patch target trocado para `Player.FirearmController.SetTriggerPressed` (`Player.cs:13668`), que roteia para CurrentOperation antes da virtual dispatch.
- **Stance layout pós-swap:** Stance 0 - Vanilla, Stance 1 - High Ready (Pitch -15), Stance 2 - Low Ready (Pitch +30, era Custom), Stance 3 - Custom (Yaw -30, era Low Ready).
- **F12 ordering:** alfabético natural — usuário aceitou que "Low Ready" fique abaixo de "High Ready" (decisão informada).

## Pendências / próximos passos conhecidos

- **Validar F4 in-raid após 06-fix-01** — se ainda não funcionar, pedir `BepInEx/LogOutput.log` para diagnosticar via logs `[F4] Resolved ...` e Prefix entries.
- **F1 (Include Stance 0 in Cycle) não foi testado pelo usuário in-raid** — recomendar teste.
- **Migração de `.cfg` antigo** — usuário pode ter customizações em seções obsoletas (`Stance 2 - Custom`, `Stance 3 - Low Ready` pré-swap) que viraram órfãs no `.cfg`. Migração manual documentada nos avisos do PROPRIEDADES.md.
- **ADS Transition Speed só atua em stance customizada (1/2/3)** — quando o usuário testa em Stance 0, o `SpringGetPatch` faz early-return sem consultar o slider. Documentado, mas pode virar `06-fix-03` se quiser expor um toggle "Aplicar ADS Speed Override mesmo em Stance 0".
- **Stance 0 Stamina Multiplier default = 0.5** drena stamina em hipfire — provavelmente causa do "ADS lento" percebido (HandsStamina baixa → tired aim no EFT). Workaround: usuário pode setar `1.0` no F12. Eventual `06-fix-XX` poderia mudar o default para `1.0` (drain como opt-in).
- **Backlog livre.** Nenhum item ⚪ ou 🟡 pendente.

## 2026-05-09 ~16:00 (GMT-3) — Sessão 1: item 002 backlog (criação + reviews)

Tarefas executadas neste dia (em ordem):

1. **Criação do item 002** via `/add-backlog-item stancesAndCameraPositionSPT4.0.11 "Ciclo linear, hotkeys e snap fogo"`. Item registrado em `mod-backlog.md`.
2. **Spec funcional** (`/create-spec`) com 5 features: F1 Include Stance 0 in Cycle, F2 Mouse Wheel Scroll Mode (Cycle/Linear), F3 hotkeys dedicadas por stance, F4 Snap to Stance 0 on Fire, F5 Start In Low Ready On Raid Begin.
3. **`/review-spec` rodada 1** — gaps corrigidos: critérios vagos reescritos, corner cases adicionados.
4. **`/review-spec` rodada 2** — refinamento dos ACs de F2 (enum), corner case de Stance 3 em Linear mode, AC F3 sobre ADS.
5. **`/review-spec` rodada 3** — †visibilidade condicional adicionada na tabela F12, contagem do delta corrigida (4→5), corner case de burst fire.
6. **Decisão hotkey + ADS:** ignorar silenciosamente quando em ADS (Opção A).
7. **Renomeação de stances pelos eixos:** spec funcional ganhou "Stance 1 - High Ready" / "Stance 2 - Custom" / "Stance 3 - Low Ready" baseado em Pitch/Yaw reais do código. Esta convenção depois foi alterada novamente no 06-fix-01 (Stance 2 ↔ Stance 3 swap).
8. **`/review-spec` rodada 4 + 5** — pontos restantes de gaps + 11 [NOVO] no delta.

**Trabalho paralelo nesta sessão:** o usuário também criou item 003 (Stamina Multiplier faixa até 10) — implementação trivial em 1 linha de código, sem passar pelas etapas formais de tech-spec/review (exceção documentada no próprio `003-…-01-spec.md`).

**Outro trabalho paralelo:** adição do mod `SPT-Realism-Mod-Client` via `/add-mod-repo-for-modding` (ver `mods/SPT-Realism-Mod-Client/memory/sessions.md`). Não impacta este mod.

**Debug session paralela:** usuário relatou bug "shoulder swap durante lean" no SPT — investigado e identificado culpado como mod externo `hazelify.StanceSync.dll`. Solução: desabilitar config `Sync leaning with shoulder swapping?` no F12 desse outro mod. **Não relacionado ao stances mod**.

## 2026-05-10 ~14:00 (GMT-3) — Sessão 2: implementação completa item 002 + code review + correções

Dia mais denso. Em ordem aproximada:

1. **`/create-technical-spec 002`** — spec técnica gerada. Estratégia inicial F4: patch em operation-base nested de `Player.FirearmController` via reflection (Estratégia A).
2. **`/review-technical-spec` rodadas 01-04** — 24 pontos PA-NN-MM levantados e aceitos pelo usuário:
   - **Round 01:** patch target via reflection da operation-base; race condition do timer; CM dependency; hotkey priority por menor índice; snap state leak; nullability; checklist refinements.
   - **Round 02:** `[ThreadStatic]` reentry guard contra recursão infinita; resolução `IsAbstract` + `GetBaseDefinition` fallback; defer 1-frame para resurrect; sem closure; Enable condicional; AC F5+F4 simultâneos.
   - **Round 03:** 2-frame pulse (synthetic false em N+2 para parar fullauto); validação `CurrentOperation` entre frames; AC ChangeFireMode mid-hold; SettingChanged unsubscribe; stub `BuildStanceConfig`.
   - **Round 04:** Order 59 collision fix (ScrollMode → 58); hotkeys antes de V no Update; natural-pressed guard no reset; HideoutPlayer guard em F5.
3. **`/code-mod 002`** — implementação. Arquivos modificados: `Plugin.cs`, `StanceConfig.cs`, `StanceManager.cs`, `Patches/RaidLifecyclePatches.cs`. Criado: `Patches/SnapFireTriggerPatch.cs`. PROPRIEDADES.md atualizado (89 props).
4. **`/compile-mod stancesAndCameraPositionSPT4.0.11 --flat`** — 1ª tentativa falhou (faltava `using CameraRotationMod.Patches;` em StanceManager.cs); corrigido; 2ª tentativa passou.
5. **`/code-review 002`** — 1ª code review. 6 achados:
   - **CR-01-01 (🟠):** F4 disparava em fogo de outros players em Fika multiplayer; faltava guard `__instance == MainPlayer.HandsController`.
   - **CR-01-02 (🟡):** Weapon swap entre button-down e button-up causava tiro espúrio; anti-swap via `_interceptOperationInstance` cacheado.
   - **CR-01-03 (🟢):** `TryInterceptTriggerDown` ignorava parâmetro — agora usado para anti-swap.
   - **CR-01-04 (🟢):** `IsHoldingFirearm()` redundante (caller já validou) — removido.
   - **CR-01-05 (🟢):** XMLDOC explícito do null sentinel em `SnapToStance0OnFire`.
   - **CR-01-06 (🟢):** `Snap Stale Timeout (s)` exposto como Advanced ConfigEntry (90ª prop).
6. **`/apply-code-review 002`** — todos os 6 achados aplicados; `05-asbuild.md` criado retroativamente (item 002 foi entregue antes de `/code-mod` passar a gerar asbuild automaticamente).
7. **`/compile-mod` 2ª vez** — sucesso, ~71KB de .dll.

**Teste in-raid pelo usuário** ao fim do dia. Feedback:
- F1 não testado.
- F2 ✓ funcionando.
- F3 ✓ funcionando.
- **F4 ❌ não funcionou**. Usuário cogitou abolir, depois optou por revisar.
- F5 ✓ funcionando.
- Reclamação de confusão entre Stance 2 (Custom) e Stance 3 (Low Ready) nos docs; quis trocar.

**Investigação de F4:** descobrimos pelo Assembly que dos 14 overrides de `SetTriggerPressed` aninhados em `FirearmController`, apenas **1** (linha 3184) chama `base.SetTriggerPressed()`. C# virtual dispatch executa o IL do override diretamente, então o Prefix patcheado na base virtual (3810) nunca disparava para 13 dos 14 caminhos. **Estratégia A do PA-01-01 review-01 estava errada** baseado em premissa incorreta sobre dispatch.

**Solução encontrada:** patchear `Player.FirearmController.SetTriggerPressed` na linha 13668 (método de roteamento da FC que chama `CurrentOperation.SetTriggerPressed(pressed && method_53())`). Captura todos os fire inputs ANTES da virtual dispatch.

8. **Plan + execução do 06-fix-01:**
   - **Phase A:** F4 patch target trocado. `ResolveFirearmOperationBase` → `ResolveFirearmControllerSetTrigger`. `SnapFireTriggerPatch` reescrito com `Player.FirearmController` tipado. Signatures de `StanceManager` simplificadas. `CurrentOperationGetter` removido (não precisa mais — staleness check vira `HandsController == fc` direto).
   - **Phase B:** Stance 2 ↔ Stance 3 swap completo (section constants, `_stanceDefaults`, hand rotation defaults Pitch/Yaw/Forward, F5 target). Após swap: Stance 2 = Low Ready, Stance 3 = Custom.
   - **Phase C:** F12 ordering alfabético aceito pelo usuário.
   - **Phase D:** `06-fix-01.md` criado com análise técnica completa do bug F4. `05-asbuild.md` atualizado. Entrada de meta-rastreabilidade no Histórico de `04-code-review-01.md` (CRs preservados; PA-01-01 reversal documentado).
9. **`/compile-mod` final do dia 10** — passou, ~70KB (~1.5KB menor; F4 simplificada).

## 2026-05-11 ~00:30 (GMT-3) — Sessão 3: meta-infraestrutura + debug ADS

Trabalho paralelo a sessões anteriores: o usuário criou infraestrutura de workflow geral. Embora não seja específico deste mod, este mod foi a cobaia.

**Mudanças repo-wide neste dia:**

- **Renomeação de convenção:** todos os artefatos de backlog passaram a usar prefixo numérico de ordem (`NNN-<slug>-01-spec.md`, `-02-spec-tech.md`, `-03-spec-tech-review-NN.md`, `-04-code-review-NN.md`, `-05-asbuild.md`, `-06-fix-NN.md`). Script `scripts/migrate-backlog-naming.sh` aplicado: 16 arquivos renomeados, 14 .md com refs atualizadas via sed.
- **Nova skill `repo-workflow-best-practices`** em `.claude/skills/repo-workflow-best-practices/SKILL.md` — formaliza convenção de naming, fluxo do ciclo, rastreabilidade PA-NN-MM/CR-NN-MM, imutabilidade de reviews.
- **Novo command `/code-review`** em `.claude/commands/code-review.md` + template `.agents/templates/code-review.md.tmpl`. 6 categorias × 4 impactos.
- **Novo command `/apply-code-review`** em `.claude/commands/apply-code-review.md`.
- **Novo template `asbuild.md.tmpl`** para `05-asbuild.md`.
- **`/code-mod` atualizado** — passa a gerar `05-asbuild.md` ao final (mudança comportamental).
- **Commands existentes atualizados** para nova convenção: `create-spec`, `review-spec`, `create-technical-spec`, `review-technical-spec`, `code-mod`.
- **Item 003 ganhou nota de "exceção documentada"** no `01-spec.md` (pulou etapas formais por trivialidade — não vira precedente).

**Debug ADS no fim da sessão:**

Usuário reportou "ADS lento" in-raid. Investigado:

- Nossas mudanças (002 + 06-fix-01) **não tocaram `_ADSTransitionSpeed`** ou caminhos de ADS speed.
- **Causa provável identificada:** `Stance 0 Stamina Multiplier = 0.5` por padrão (backlog 001) drena HandsStamina mesmo em hipfire vanilla → EFT aplica penalty de tired aim → percepção de "ADS lento". Workaround: setar `1.0` no F12.
- **Por que o slider `ADS Transition Speed` parecia "morto"** quando testado em Stance 0: `SpringGetPatch.cs:200-208` faz early-return quando NÃO há feature ativa (`isInAnyStance == false && !resetOnADSEnabled`). Em Stance 0 com flags Advanced desligadas, o slider nem é consultado. Slider só atua em Stance 1/2/3, ou quando `Reset Positions When Aiming = true`. Documentado mas sem fix de código.

**Sugestão pendente (não executada):** `06-fix-02` opcional para expor toggle "Aplicar ADS Speed Override mesmo em Stance 0".

**Aviso de drift no asbuild.md (linha 14):** existe uma referência a um `06-fix-02.md` ("Labels das hotkeys Stance 2/3 + ordem F12 via Order bump em BindStance") que **não corresponde a trabalho registrado** nesta sessão. Pode ter sido criado em chat paralelo. Investigar antes de criar novo fix-02 com numeração duplicada.

## Arquivos-chave do mod (referência rápida)

- `modded/Plugin.cs` — Awake, ConfigEntries, helpers de F2 (CM cache) e F4 (`ResolveFirearmControllerSetTrigger`), section constants, `_stanceDefaults`, `BindStance`.
- `modded/StanceManager.cs` — Update tick, F1 IsStanceEnabled, F2 HandleLinearScroll, F3 HandleStanceHotkeys, F4 estado snap + 6 helpers, F5 QueueInitialStance/TryApplyPendingInitialStance.
- `modded/StanceConfig.cs` — StanceConfig record (ConfigEntries por stance, com SnapToStance0OnFire nullable para Stance 0).
- `modded/Patches/SnapFireTriggerPatch.cs` — F4 Prefix com `[ThreadStatic]` reentry guard, intercept-and-resurrect, 2-frame pulse.
- `modded/Patches/RaidLifecyclePatches.cs` — Postfix de `GameWorld.OnGameStarted` (StanceManager.OnRaidStart + F5 QueueInitialStance(Stance2)) e `GameWorld.OnDestroy` (OnRaidEnd).
- `modded/Patches/SpringGetPatch.cs` — pré-existente, controla transição de mãos via Spring; early-return quando nenhuma feature ativa (relevante para o slider `ADS Transition Speed`).
- `modded/Patches/StanceStaminaRecoveryPatch.cs` — pré-existente do backlog 001; controla drain/recovery de HandsStamina por stance.
- `PROPRIEDADES.md` — 90 props documentadas em pt-BR (era 79 antes do 002; 89 após CR-01-06 com Snap Stale Timeout).
- `backlog/` — todos os artefatos do ciclo (01-spec, 02-spec-tech, 03-spec-tech-review-NN, 04-code-review-NN, 05-asbuild, 06-fix-NN).
