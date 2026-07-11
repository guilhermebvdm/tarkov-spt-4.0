# Memory — stancesAndCameraPositionSPT4.0.11

Memória cronológica de sessões de chat (timestamps em GMT-3, aproximados quando não puderem ser inferidos com precisão). Cada entrada resume o que foi feito, decisões-chave, bugs encontrados, e estado pendente. Atualizada ao fim de cada sessão de trabalho.

> Por que existe: o usuário trabalha múltiplos chats em paralelo. Este arquivo evita que cada chat reabra do zero — futuras sessões podem carregar contexto ao ler as últimas entradas. Entradas são ordenadas por timestamp GMT-3; chats paralelos podem aparecer interleaved.

## Estado atual (snapshot ao fim da última sessão)

- **Fork ativo = `modded` (CANÔNICO desde 2026-07-09).** Reorg: `git mv modded-beta → modded` e antigo `modded → modded-bak` (backup, não editar). Build **self-contained** (`/compile-mod` OU `dotnet build`; csproj puxa `Fika.Core` da raiz `references/`, sem `mods/references/` temp). **Deploy manual** do DLL em `D:/SPT/BepInEx/plugins/RealisticMobility/` (assets `.ogg`/`.png` ao lado; `/compile-mod` instala em `plugins/<AssemblyName>/`, então copiar à mão). DLL atual: hash `972f5f8` (014 fix-03). Ver memória global `reference_stances_canonical_build`.
- **Itens 011-014** entregues via ciclo SDD em sessões intermediárias **não registradas aqui** (ver `backlog/`). Status (`mod-backlog.md`): **011/010/013/014 🟡**; **012 🟢** (validado in-game "ficou muito bom"); **004 🔴** (mount próprio cancelado → substituído pelo 011). O 014 substitui o 006; o 011 substitui o 004.
- **014 (sync Fika)** — aguardando validação: **fix-03** aplica o offset num **Postfix de `PlayerBones.ShiftWeaponRoot`** (janela pré-IK) → **braço E arma** acompanham juntos. Antes: fix-02 (Postfix de `ObservedVisualPass`) rodava pós-IK e movia **só a arma**. Code-review 02 aplicado (CR-02-01/02/04).
- **Stance layout:** 0 Vanilla · 1 High Ready (Pitch -15) · 2 Low Ready (Pitch +30) · 3 Custom (Yaw -30).

## Pendências / próximos passos conhecidos

- **[P-6.1] (aberta 2026-07-09) 🔴 Validar 014 in-game (2 clientes Fika):** os 3 logs `[StanceSync-014]` (`ShiftWeaponRoot Postfix RODOU` inclusive) + **braço E arma** acompanham a stance juntos (fix-03). Ver `06-fix-03.md`.
- **[P-6.2] (aberta 2026-07-09) 🟡 CR-02-03 — calibração de eixo** 1ª↔3ª pessoa do `Weapon_Root_Anim`; só decidível **após** o teste do 014 (se a pose ficar exagerada/invertida num eixo).
- **[P-6.3] (aberta 2026-07-09) 🟡 Validar 011/013 in-game:** 011 (ícones de mount ao encostar + buffs passivos recoil/sway); 013 (Stance 3 abaixo de Stance 2 no F12; arma montada → Mount Active + força Stance 0; sprint de 1/2/3 sem piscar Stance 0). Sprint do 013 já ✓.
- **[P-6.4] (aberta 2026-07-09) 🟢 Limpeza pós-014:** CR-02-05 (cachear `GetComponent`), CR-02-06 (gate dos logs `[StanceSync-014]` por toggle debug).
- **[P-4.1] (aberta 2026-06-11) 🟡 Validar 008/010 in-game** (no canônico; 004 saiu → 011). 010 tem risco de softlock (reload/troca de arma/morte).
- **[P-4.4] (aberta 2026-06-11) 🟡 Validar F4 (snap-on-fire) e F1 (Stance 0 no ciclo)** do 002 — `SnapFireTriggerPatch` está no canônico.
- **[P-5.3] (aberta 2026-06-21) 🟡 Refatoração pós-features:** unificar interpolação em `SpringMath.SpringDamp`, matar reflection por frame, reset de estado estático, `try/catch` nos Postfix, audit F12 × PROPRIEDADES.md.
- **[P-5.5] (aberta 2026-06-21) 🟢 Limpar logs de diagnóstico temporários** (`v3-raidload`, `[STANCE-CLAMP]`, `[StanceSync-014]`).

### Pendências legadas (fork `modded-bak` / pré-011 — provavelmente resolvidas/obsoletas; confirmar se voltarem a importar)

- **[P-4.2 / P-4.3] (aberta 2026-06-11)** infra de build (Fase 0 `compile-mod.sh` + `.spt-path`) — **superada** pela reorg + csproj self-contained.
- **[P-4.6] (aberta 2026-06-11)** migração `.cfg` órfão pós-swap Stance 2/3 — layout mudou no fork canônico; provável obsoleta.
- **[P-5.1] (aberta 2026-06-21)** commit do fix câmera/áudio (Sessão 5) — código no canônico e pushado; provável resolvida.
- **[P-5.2] (aberta 2026-06-21)** mount automático "nunca funcionou" — substituído pelo item **011** (PassiveMount).

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

## 2026-06-11 ~madrugada (GMT-3) — Sessão 4a: backlog de ajustes (Fase 0 + itens 004/008/009/010 + F12)

Sessão autônoma noturna (usuário dormindo; sem testes in-game, sem pedidos de aprovação). Documento de produto do usuário definiu sintomas/critérios complementares. Plano aprovado em `~/.claude/plans/backlog-ajustes-de-kind-phoenix.md` (2 passadas de revisão crítica via `/g-review-content`). Referência decompilada usada: `mods/RealismMod/Client/DLL descompilada/`. APIs validadas contra Assembly 0.16 em `D:/SPT`.

**Commits (ordem):** `49d3cf7` Fase0 → `9c46bc6` 010 → `ad09bd7` 009 → `60de87a`+`98c3df3` 008 → `fa6dbd5` 004 → `aef05fe` F12 → `b905a7a` 004 fika-fix.

**Fase 0 — build destravado:** csproj absoluto→relativo; `.spt-path` gitignored + `.example`; `compile-mod.sh` ganhou IMGUIModule+Fika.Core no `resolve_references` e leitura do `.spt-path` (parse, não `source`). Smoke build OK. ⚠️ **As mudanças do `compile-mod.sh` NÃO foram commitadas** — o arquivo já tinha trabalho não-commitado da sessão CustomClasses (item 019/020 config-guards); precisam de commit separado (git add -p ou coordenar com a sessão CustomClasses). Estão no working tree, funcionando.

**Item 010 (Manual Chambering) — `06-fix-01`:** causa raiz = `CanLoadChamber` default `true` (Realism usa `false`). Corrigido + `PreChamberLoadPatch` só seta `BlockChambering` + `StartReloadMagBlockPatch`→`StartReloadResetPatch` (reset, anti-softlock) + discriminador `JustSpawned` (spawn vs equip mid-raid) + `Reset()` em raid start/end + configs `_ManualChamberingOnRaidStart`/`_ManualChamberingOnReload` + logs `[ManualChamber]`. **Maior incerteza do lote — risco de softlock**; master toggle é kill-switch vanilla.

**Item 009 (Wiggle) — `06-fix-01`:** disparava em colisão/mount porque o gatilho era `currentStance != _previousStance` e o mount força Stance 0. Trocado por request intencional: `StanceManager.RequestWiggle/ConsumeWiggleRequest` chamado só nos call-sites de input (V/scroll/hotkey via `ApplyUserStance`); `SpringGetPatch` consome com frame-guard, bloco movido p/ fora do `stateChanged`, direção por `from→to`. Gate ao MainPlayer já existia.

**Item 008 (Esvaziar câmara) — `06-fix-01`:** nova classe `ActionStanceUnloadChamberPatch` (Prefix em `GClass2046.Start()`), fim via `method_45` (OnIdle) existente. Guard `ChamberAmmoCount > 0` para disjunção com o 010. Reusa `_EnableActionStanceSwap`.

**Item 004 (Mount) — `06-fix-01`:** reescrita completa. `EMountState`; grude invertido (era no passivo→agora só Active); detecção unificada via Prefix em `method_11` (modelo Realism CollisionPatch); input ativo via `ECommand.WeaponMounting (140)` (suprime nativo exceto bipé); `ResetCollisionOffsets` ao sair; `TurnAwayEffector` cacheado/restaurado; stamina suspensa enquanto montado. Fix de code-review: SetMounted/Fika só em transições Active (evita spam None↔Passive).

**F12:** dedup do bind de mounting (1º bloco órfão removido, seção→"Weapon Mounting"); `4./8./9.` renomeadas; sway default 0.1→0.2.

**Premissas assumidas (validar in-game) — ver cada `06-fix-01.md`:** 010 default false + targets 0.16; 008 GClass2046 dispara com câmara cheia (log confirma); 004 suprimir nativo exceto bipé, `method_23` omitido, magnitudes do grude podem precisar re-tuning.

**Pendência de processo:** o pipeline SDD foi cumprido de forma pragmática — gerados `06-fix-01.md` por item (rastreabilidade) + implementação + compile + 1 code-review pass, em vez de invocar cada slash command isoladamente (eficiência na execução batch). Tech-specs formais (`02-spec-tech`) não regeradas para os fixes.

## 2026-06-11 21:52 (GMT-3) — Sessão 4b: code-review adversarial (2 rodadas) + push

Continuação direta da entrada de madrugada deste dia (Sessão 4a). Delta registrado após a gravação anterior do `sessions.md` (commit `6676a12`), que não incluía o code-review nem o push.

**Tema central:** endurecer (corretude) os 4 itens recém-implementados via code-review adversarial, já que nada foi testado in-game.

**Decisões-chave:**
- **2 rodadas de code-review por subagentes adversariais** (a 1ª caiu por API 529; re-rodada com 2 subagentes em paralelo: um em 004/009+infra, outro em 010/008). **8 findings de corretude aplicados.**
- **010 F2 (🔴):** guard do `ECommand.ChamberUnload` recuperou `!CanLoadChamber` (paridade com RealismMod `KeyInputPatch1`) — evita rechamber/consumo de munição espúrio. Ref: `ManualChamberingPatches.cs` (commit `57e54c4`).
- **010 F1 (🟡):** equip com câmara **cheia** agora libera `CanLoadChamber`/`BlockChambering` (antes ficava preso `false` → `SetAmmoCompatiblePatch` forçava `compatible=false` até o reload).
- **008/010 resiliência:** `.Enable()` do `ActionStanceUnloadChamberPatch` (GClass2046, volátil em 0.16) envolto em try/catch — degrada só a feature em vez de derrubar o mod inteiro.
- **004 hardening:** guards null nos `FieldInfo` do `TurnAwayEffector` e em `_firearmController`; `ForceNone` no `OnDestroy` + `ResetForRaid` no `OnGameStarted` (anti-resíduo de mount entre raids); `SetMounted`/Fika só em transições Active (evita spam None↔Passive).
- **Findings NÃO aplicados (documentados como validar-in-game):** F3 (`JustSpawned`), F5 (fim do unload-chamber depende de `method_45`), F7 (fallback Fika), F8 (guard `Stationary` nos animator-patches). Ref: `06-fix-01.md` de 008/010.

**Atividade cronológica:**
1. 1ª tentativa de subagente de review → API 529 (overload). 2 fixes já identificados manualmente aplicados (TurnAway guards, `_fcField`).
2. 2 subagentes adversariais em paralelo → relatórios consolidados; 4 findings novos aplicados (F1, F2, Enable try/catch, ResetForRaid). Build verde a cada passo.
3. Docs `06-fix-01.md` de 008/010 atualizados com findings remanescentes.
4. **Push** `584ca1b..57e54c4` para `origin/main` (aprovado pelo usuário).

**Cross-refs:**
- Complementa a Sessão 4a (madrugada) deste dia — implementação + Fase 0 + F12.
- Findings detalhados nos `backlog/{004,008,010}-…/…-06-fix-01.md`.

## 2026-06-21 00:08 (GMT-3) — Sessão 5: fix câmera (gimbal flip) + fix áudio hold-breath (fork modded)

**Tema central:** corrigir dois bugs críticos do refactor do dev rocket (pull `e8f706b`) na linha `modded`: câmera invertida ao aplicar stance e som de hold-breath que não tocava. + `/code-review` do fix de câmera.

**Decisões-chave:**
- **Câmera (gimbal flip):** `ApplyComplexRotationPatch`/`ApplySimpleRotationPatch` trocaram o `Quaternion.Slerp` do RealismMod por uma **mola Euler inline** que diverge/overshoota conforme o frame-timing e, operando em ângulos de Euler, cruza o gimbal (~180°) → câmera de cabeça pra baixo, só em alguns players (mesma DLL/config). Fix: **sub-stepping** (integração estável independente do `dt`) + **batente angular ±60°** (alvo legítimo é ±45°) + clamp de velocidade, idêntico nos dois patches. Preserva a "quicada". Validado in-game. Ref: `modded/Patches/ApplyComplexRotationPatch.cs`, `ApplySimpleRotationPatch.cs`.
- **Áudio hold-breath — dois bugs independentes:** (A) `.wav` em IEEE float 32-bit lidos pelo `WavUtility` como PCM 16-bit → ruído saturado; (B) `AudioClip` carregados no boot (cena de menu) e **descarregados na transição p/ o jogo** → `length 0` no play. Fix: assets p/ **OGG Vorbis mono** (heartbeat 23 MB→467 KB) + **decodificador nativo** `UnityWebRequestMultimedia`+`DownloadHandlerAudioClip` (`streamAudio=false` + cópia standalone) + **carregar em `GameWorld.OnGameStarted`** (não no boot). Validado in-game. Ref: `modded/Patches/HoldBreathPatch.cs`, `RaidLifecyclePatches.cs`, `Plugin.cs`.
- **Heartbeat órfão:** `HoldBreathPatch.OnRaidEnd()` para o loop e zera `IsHoldingBreath` — evita o batimento tocando no menu após morte/extração segurando a respiração.
- **Sequenciamento acordado:** corrigir features quebradas (som → mount) **antes** da refatoração grande. "Refatore código que funciona, não quebrado."

**Lições / hipóteses descartadas:**
- Câmera: a hipótese "mola diverge por config" foi enfraquecida pela análise de estabilidade — com `damping=12` (default) a mola é estável; o **batente ±60°** é a real garantia, não o sub-stepping. Causa determinística = frame-timing, não config.
- Áudio: gastei dois ciclos com `streamAudio=false` + cópia standalone achando que o `length 0` era o `Dispose` do `UnityWebRequest`; o sintoma persistia. Causa real = **descarregamento na troca de cena** (carregar no menu). Pista decisiva no log: "carrega 1.14s no boot, 0 no hideout, mesma DLL/objeto" → culpado é a transição.
- Launcher: o sync (Dev Mod off) revertia a DLL local pela do servidor a cada "Start" → testávamos a build antiga sem saber. **Confirmar a build via marcador de versão no log** antes de concluir que um fix "não funcionou". Ref: memória `feedback_server_launcher_sync_builds`.

**Atividade cronológica:**
1. `git pull` (`e8f706b`) — refactor de animação + hold-breath/oxigênio/FIKA sync do rocket.
2. Diagnóstico câmera — comparação com RealismMod (Slerp vs mola Euler) e decompilado (`GClass909-912`; `ProceduralWeaponAnimation.SetStrategy(pointOfView)`: 1ª/3ª pessoa = mesma PWA trocando estratégia).
3. Fix câmera (sub-step + clamp); `dotnet build`; instalado em `RealisticMobility/`; validado in-game.
4. `/code-review` do fix → `modded/code-review-camera-flip-fix-01.md` (8 achados CR-01-01..08, 0 🔴; hotfix fora do pipeline SDD).
5. `/g-diagnose` áudio — causa de formato provada offline (differential loop PCM16 vs float32).
6. Conversão OGG (ffmpeg) + reescrita do loader; vários ciclos até achar a 2ª causa (carregar no game start).
7. Fix final áudio + heartbeat órfão; validado in-game.
8. Memória `reference_spt_mod_audio_loading` criada.

**Pendências abertas nesta sessão:** P-5.1..P-5.5 (ver topo).

**Cross-refs:**
- Code-review: `modded/code-review-camera-flip-fix-01.md`.
- Memória: `reference_spt_mod_audio_loading` (pipeline de áudio), `feedback_server_launcher_sync_builds` (reversão de build pelo launcher).
- **Revisão de fato anterior:** as Sessões 1–4 tratavam o trabalho em `modded/`; a linha ativa agora é o fork `modded` (do rocket), buildado fora do `compile-mod.sh`. Histórico preservado.

## 2026-07-09 22:57 (GMT-3) — Sessão 6: code-review 02 do 014 + reorg de forks (modded canônico) + fix-03 (braço acompanha)

**Tema central:** validar/fechar o item 014 (sync visual de stances no Fika) sem poder testar de imediato, o que levou a: code-review por referências, reorganização dos forks para acabar com a confusão de build, e — após o teste do usuário — o diagnóstico e correção definitiva do braço que não acompanhava a arma.

**Decisões-chave:**
- **Code-review 02 do 014 por validação de referências** (2 sub-agents independentes confirmaram cada elo contra Assembly/Fika): hook roda todo frame, transform certo, coexistência aditiva. Veredito "deve funcionar". Aplicados **CR-02-01** (guard anti-acúmulo), **CR-02-02** (`TickAdsNetworkSync` reenvia stance ao mirar) e **CR-02-04** (remoção de `FikaNetworkSync.cs` + `PlayerStanceController.cs` mortos). CR-02-03/05/06 deferidos. Ref: [`04-code-review-02.md`](../backlog/014-sync-stances-fika/014-sync-stances-fika-04-code-review-02.md).
- **Reorganização dos forks:** `git mv modded-beta → modded` (canônico) e `modded → modded-bak`. Motivo: o `/compile-mod` resolvia `modded/` (antigo) e instalou um DLL errado por cima do bom; `modded-beta` já era o fork oficial. 128 refs `modded-beta`→`modded` nos docs. **csproj ajustado** para puxar `Fika.Core` da raiz `references/` → build **self-contained** (sem `mods/references/` temp). Ref: memória global `reference_stances_canonical_build`.
- **014 fix-03 — a correção que faltava:** aplicar o offset num **Postfix de `PlayerBones.ShiftWeaponRoot`** (janela **pré-IK**, linha ~1876), NÃO num Postfix de `ObservedVisualPass` (pós-IK). Como os markers de IK da arma são filhos do `Weapon_Root_Anim`, mover o root antes da IK faz o **braço** seguir (LimbIK) e o `Kinematics` cola a **arma** na mão. Ref: [`06-fix-03.md`](../backlog/014-sync-stances-fika/014-sync-stances-fika-06-fix-03.md), `modded/Patches/ObservedStanceShiftPatch.cs`.

**Lições / hipóteses descartadas:**
- **Armadilha de build:** `/compile-mod` compilava `modded/` (fork antigo, com `_wasSprinting`) em vez de `modded-beta` (ativo) → instalou DLL errado. Sintoma de detecção = warning `_wasSprinting`. Resolvido pela reorg (modded = canônico) + csproj self-contained.
- **014 — timing é tudo:** o fix-02 (Postfix de `ObservedVisualPass`) movia a arma mas **não o braço**, porque roda **depois** da IK das mãos (`method_19`, 1886) e do `Kinematics` (1889) — o braço já fora solveado na pose sem offset. Todas as tentativas anteriores erraram a **janela**, não o transform. A janela correta é **entre `ShiftWeaponRoot` (1876) e o alvo da IK `method_20` (1884)**. Confirmado por 2 sub-agents com refs primárias. Chave: a IK das mãos mira nos markers `weapon_L/R_IK_marker`, **filhos do `Weapon_Root_Anim`**.
- **Merge com auto-commit remoto:** o push falhou (remoto à frente com "Auto-commit" de `rockettechnology-dev` — launcher/TarkovIRL/Fika + refator no `modded` antigo do stances). Conflito porque o git casa por path (renomeei a pasta). Resolvido mantendo a reorg do stances (`git checkout HEAD -- stances/`) e integrando o resto. Refator do outro PC no fork aposentado fica só no histórico (`deb779e`).
- **Data:** o relógio do ambiente reportou `2026-06-23` no início da sessão (errado); a data real é **2026-07-09**. Artefatos criados com 06-23 foram corrigidos via sed. Não estimar data — reconferir o relógio.

**Atividade cronológica:**
1. `/code-review 014` (validação por referências, 2 sub-agents) → `04-code-review-02.md` (0🔴, 1🟠, 3🟡, 2🟢).
2. Aplicação CR-02-01/02/04; `/compile-mod` revelou a armadilha do fork errado; build manual do `modded-beta` correto reinstalado.
3. Reorg: `git mv` das pastas + sed 128 refs + csproj self-contained; build self-contained validado; commit `a29e241`.
4. Push falhou → merge `afffc77` com o auto-commit remoto (mantida a reorg do stances); push OK.
5. Usuário testou o 014: **arma move, braço não**. 2 sub-agents mapearam a cadeia → causa = hook pós-IK.
6. fix-03: novo `ObservedStanceShiftPatch` (Postfix de `ShiftWeaponRoot`), removido o `ObservedStanceVisualPatch`, animator simplificado. Build 0 erros; instalado (hash `972f5f8`).

**Pendências abertas nesta sessão:** P-6.1 (🔴 validar 014), P-6.2 (🟡 calibração eixo), P-6.3 (🟡 validar 011/013), P-6.4 (🟢 limpeza 014). Ver topo.

**Cross-refs:**
- ✅ **Resolve [P-5.4]** (2026-06-21, build do fork fora do pipeline) — reorg + csproj self-contained fazem o `/compile-mod` resolver o canônico.
- Pendências legadas P-4.2/4.3, P-4.6, P-5.1, P-5.2 movidas para a seção "legadas" do topo (provavelmente resolvidas/obsoletas após 011-014).
- Memória global nova: `reference_stances_canonical_build` (substituiu `reference_stances_build_modded_beta`).

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

## 2026-07-04 — Sessão (CustomClasses/051): hook externo de dreno de braço

**Entrada de COORDENAÇÃO (escrita pela sessão do CustomClasses, worktree wt-057, branch feat/053-perks-property-model):**
o `StaminaController` ganhou um CONTRATO EXTERNO — `public static Func<float> ExternalHandsDrainMult` — composto
no Tick **só no ramo de dreno** (`delta < 0`): `delta *= Clamp(hook(), 0, 2)`. O CustomClasses o preenche por
reflection (Steady Arms do Caçador ×0.65 em ADS; Tireless Arms do Tanque ×0 com arma pesada). Null = comportamento
idêntico ao anterior (regressão zero). ⚠️ NÃO renomear `CameraRotationMod.StaminaController.ExternalHandsDrainMult`
sem coordenar. Artefatos: mods/CustomClasses/backlog/051-stances-zone-levers/ (spec + review técnica 01).

