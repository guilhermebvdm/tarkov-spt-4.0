---
title: Antipatterns de modding SPT 4.0 — taxonomia de erros reais
date: 2026-06-11
status: 🟢 Vivo
authors: Guilherme + agente
---

# Antipatterns de modding SPT 4.0 — taxonomia de erros reais

Catálogo dos erros que **já cometemos neste repo**, com o caso real, a causa raiz e onde o harness checa cada um. Complementa as skills (`spt-mod-best-practices`, `csharp-mod-best-practices`), que são prescritivas e curtas — aqui fica a evidência e o histórico.

**Como usar:** ler antes de escrever ou revisar spec técnica. Cada `AP-NN` é citável em specs, reviews e na seção "Conformidade com skills" do template de spec técnica.

**Como cresce:** o `/update-memory` propõe promoção de lições recorrentes (≥2 sessões) para cá — ver skill `memory-curation` §"Promoção de lições".

---

## AP-01 — Raid lifecycle hooks ausentes

- **Sintoma:** estado estático (listas, dicionários, flags, multiplicadores) vaza entre raids; comportamento "fantasma" no segundo raid; referências a `Player`/`Profile` impedem GC do raid inteiro.
- **Causa raiz:** spec/código não hookam o caminho de fim de raid — e o fim pode vir por extract, morte, MIA ou alt-F4, por caminhos diferentes.
- **Exemplo real:** item 001 do stances — 3 ocorrências 🔴 nas reviews técnicas, ex. [PA-01-03](../../mods/stancesAndCameraPositionSPT4.0.11/backlog/001-stamina-e-velocidade/001-stamina-e-velocidade-03-spec-tech-review-01.md) (sem patch em `GameWorld.OnDestroy`/`BaseLocalGame.Stop`, `StanceStaminaState` vazaria entre raids).
- **Prevenção:** hookar `GameWorld.OnDestroy` **e** `BaseLocalGame.Stop`, teardown idempotente (`bool _ended`). Skill `spt-mod-best-practices` §2.
- **Onde é checado:** check 1 da seção "Conformidade com skills" da spec técnica; checklist item 1 da skill SPT.

## AP-02 — Filtro MainPlayer/Fika ausente

- **Sintoma:** patch reage a ação de **qualquer** player (bots, outros jogadores em raid Fika) — multiplicadores aplicados a bots, efeitos disparando com tiros alheios.
- **Causa raiz:** métodos virtuais de `Player`/controllers rodam para cada player do mundo; o patch assume implicitamente "sou eu".
- **Exemplo real:** [CR-01-01 do item 002](../../mods/stancesAndCameraPositionSPT4.0.11/backlog/002-ciclo-linear-hotkeys-snap-fogo/002-ciclo-linear-hotkeys-snap-fogo-04-code-review-01.md) — o snap de stance (F4) dispararia com fogo de OUTROS players em raid Fika; descoberto só no code-review, depois do código pronto. Também [PA-01-04 do item 001](../../mods/stancesAndCameraPositionSPT4.0.11/backlog/001-stamina-e-velocidade/001-stamina-e-velocidade-03-spec-tech-review-01.md) (multiplicador de stamina aplicado a bots).
- **Prevenção:** todo patch que reage a ação de player valida `IsYourPlayer` / `__instance == MainPlayer.HandsController` / equivalente. Fika instalado é o cenário default deste repo.
- **Onde é checado:** check 2 da "Conformidade com skills"; critério de aceite padrão "Fika/multiplayer" da spec funcional; checklist item 4 da skill SPT.

## AP-03 — Alvo virtual/ofuscado sem auditar overrides

- **Sintoma:** Prefix/Postfix "não dispara" silenciosamente em parte (ou na maioria) dos caminhos reais; feature parece implementada mas não funciona in-raid.
- **Causa raiz:** Harmony intercepta o IL do método patcheado. Em C#, virtual dispatch executa o IL do **override** — se o override não chama `base.X()`, o patch na base virtual nunca roda naquele caminho. Agravante: alvos ofuscados (`GClass####`, `method_##`) resolvidos por nome literal quebram entre builds do EFT.
- **Exemplo real:** [06-fix-01 do item 002](../../mods/stancesAndCameraPositionSPT4.0.11/backlog/002-ciclo-linear-hotkeys-snap-fogo/002-ciclo-linear-hotkeys-snap-fogo-06-fix-01.md) — dos 14 overrides de `SetTriggerPressed` aninhados em `FirearmController`, só 1 chama a base; o Prefix na base virtual (`Player.cs:3810`) nunca disparava no caminho comum. Bug chegou a produção (testado in-raid pelo usuário) — fix repatcheou no método de roteamento (`Player.cs:13668`).
- **Prevenção:** ao patchear método virtual/abstract, **auditar TODOS os overrides** (quem chama base, quem não chama) e patchear o ponto de roteamento que cobre todos os caminhos; resolver alvos ofuscados por assinatura/predicado estável, nunca por nome literal.
- **Onde é checado:** check 3 da "Conformidade com skills"; checklist item 10 da skill SPT; skill `graph-code-navigation` (query de overrides no grafo de código).

## AP-04 — Mutação direta de estado vs API canônica

- **Sintoma:** o valor muda mas o jogo não reage — HUD não atualiza, sons não tocam, animação/state machine dessincroniza, Fika não propaga.
- **Causa raiz:** escrever um field/property interno pula os side-effects que o entry point canônico do EFT dispara (eventos, thresholds, notificação de rede).
- **Exemplos reais:**
  - [PA-02-01 do item 001](../../mods/stancesAndCameraPositionSPT4.0.11/backlog/001-stamina-e-velocidade/001-stamina-e-velocidade-03-spec-tech-review-02.md) — `hands.Current = ...` direto pula `Consume()`: sem HUD, sem sons de stamina baixa, sem estado `HandsExhausted`. Resolvido com buffer + `UpdateStamina()`.
  - [06-fix-01 do item 004](../../mods/stancesAndCameraPositionSPT4.0.11/backlog/004-apoiar-arma-superficie/004-apoiar-arma-superficie-06-fix-01.md) — mount estável só via `ECommand.WeaponMounting (140)` em `TranslateCommand` (caminho canônico), não setando estado do controller.
- **Prevenção:** antes de mutar um field, grep no Assembly pelo setter/command/operation que o próprio jogo usa para aquela transição e listar os side-effects na spec técnica; bypass consciente → documentar o porquê e o que se abre mão.
- **Onde é checado:** check 4 da "Conformidade com skills"; skill SPT §8; checklist item 9 da skill SPT.

## AP-05 — Ambiguidade de semântica/defaults em specs

- **Sintoma:** implementador "decide sozinho" um default ou uma semântica; comportamento entregue contradiz a spec funcional; retrabalho de review para realinhar.
- **Causa raiz:** spec define conceito novo que colide com semântica existente do código, ou tabela de defaults diverge dos stubs.
- **Exemplos reais (item 001 do stances, 3 ocorrências):** "Stance 0" introduzida como stance nomeada colidindo com `_currentStance == 0` = "sem stance" ([PA-02-03](../../mods/stancesAndCameraPositionSPT4.0.11/backlog/001-stamina-e-velocidade/001-stamina-e-velocidade-03-spec-tech-review-02.md)); default `None` no stub vs `Drain`/`Recovery` na tabela (PA-01-10). Consequência viva: `Stance 0 Stamina Multiplier default = 0.5` drenando stamina em hipfire (pendência real na memória do mod).
- **Prevenção:** cada `ConfigEntry` com unidade, faixa, default e comportamento em **todos** os estados (incluindo o "estado neutro"); conceitos novos confrontados com a semântica existente do código.
- **Onde é checado:** check 6 da "Conformidade com skills"; `/review-spec` (critérios vagos) e `/review-technical-spec` Categoria A/C.

## AP-06 — Fix entregue sem validação padronizada

- **Sintoma:** fix "pronto" que nunca foi observado funcionando; pilha de itens 🟡 aguardando validação; regressão descoberta semanas depois.
- **Causa raiz:** `06-fix-NN` não tinha checklist de validação — "compila" era tratado como "funciona".
- **Exemplo real:** pendência 🔴 atual do stances — 4 itens (004/008/009/010) compilados e nenhum validado in-game (`mods/stancesAndCameraPositionSPT4.0.11/memory/sessions.md`, snapshot).
- **Prevenção:** todo fix nasce de `.agents/templates/fix.md.tmpl` e só é marcado entregue com o checklist preenchido: compila, in-raid, Fika (ou N/A), raid1→exit→raid2, alt-F4/morte/MIA, memória atualizada com a lição.
- **Onde é checado:** template `fix.md.tmpl`; skill `repo-workflow-best-practices` (artefato 06); output do `/apply-code-review`; **gate de pre-commit** `check-delivered-validation.sh` (item 🟢 com caixa in-raid desmarcada bloqueia o commit).

## AP-07 — Self-reentry: re-invocar o próprio método patcheado

- **Sintoma:** recursão infinita → stack overflow / crash-to-desktop na primeira vez que o caminho dispara (não no boot — só quando a ação acontece).
- **Causa raiz:** chamar de volta o método patcheado (via `MethodInfo.Invoke`, ressurreição de operação, ou forwarding) re-entra o próprio Prefix/Postfix do Harmony, que chama de novo, sem fundo de pilha.
- **Exemplo real:** [PA-02-01 do item 002](../../mods/stancesAndCameraPositionSPT4.0.11/backlog/002-ciclo-linear-hotkeys-snap-fogo/002-ciclo-linear-hotkeys-snap-fogo-03-spec-tech-review-02.md) — ressuscitar a operação de trigger via `MethodInfo.Invoke` re-entrava o Prefix → stack overflow no primeiro hold ≥ threshold. Fix: guard `[ThreadStatic] bool _inSyntheticCall` que faz o Prefix retornar cedo durante a chamada sintética.
- **Prevenção:** ao re-invocar/forward um alvo patcheado, ou usar `[HarmonyReversePatch]`/delegate para a original, ou um reentry-guard `[ThreadStatic]` que o Prefix checa antes de agir. Nunca chamar o método patcheado diretamente de dentro do patch.
- **Onde é checado:** check 7 da "Conformidade com skills"; checklist item 13 da skill C# (reentrância); skill `csharp-mod-best-practices` §3.

## AP-08 — Estado stale através de troca de contexto

- **Sintoma:** efeito/intercept dispara no alvo errado depois de uma troca (arma, operação, tela) — ex.: um tiro espúrio na arma NOVA.
- **Causa raiz:** flag/cache de estado sobrevive a uma mudança de operação/contexto e é lido como se ainda fosse válido para o contexto antigo.
- **Exemplo real:** [CR-01-02 do item 002](../../mods/stancesAndCameraPositionSPT4.0.11/backlog/002-ciclo-linear-hotkeys-snap-fogo/002-ciclo-linear-hotkeys-snap-fogo-04-code-review-01.md) — `_snapInterceptActive` sobrevivia à troca de arma e disparava snap na arma nova. Fix: cachear a identidade da operação (`_interceptOperationInstance`) no início e comparar antes de agir.
- **Prevenção:** cachear a identidade do contexto (operação/controller/arma) no início do intercept e comparar antes de cada ação; limpar flags de estado no teardown de cada operação.
- **Onde é checado:** check 8 da "Conformidade com skills"; checklist item 12 da skill C# (estado stale em troca de contexto); skill `csharp-mod-best-practices` §3.

## AP-09 — Recon/decompile curado tratado como verdade pinada

- **Sintoma:** patch-point "confirmado" no recon não existe no assembly (nome/assinatura diferente ou inventado) → tempo perdido codando contra um alvo fantasma; o patch não aplica (ou compila e falha em runtime).
- **Causa raiz:** "confiança" de recon (humano ou subagente) é um **candidato**, não um fato — pode citar um membro plausível porém inexistente. Agravante: membros ofuscados (`method_##`, `GClass####`) variam entre builds, e o mapping 4.1 é **rótulo** (aponta o conceito; não prova assinatura, e cobre tipos, não membros).
- **Exemplos reais:** [CustomClasses Sessão 10](../../mods/CustomClasses/memory/sessions.md) — recon citou `WeaponRecoil.CalculateRecoil` (marcado ✅) que **não existe**; o ponto real era `ProceduralWeaponAnimation.Shoot(str)`. Item 005 do stances (Sessão 5/6) — validar membros via compilação. **Contexto histórico:** até 2026-07-19 havia uma segunda causa — o dump era **parcial** (102 namespaces vazios), e tipos existentes eram dados como inexistentes; isso foi **resolvido** (dump completo: 8.683 tipos, 0 pastas vazias — ver `references/eft-decompiled/README.md`).
- **⚠️ Ausência não se infere de um grep vazio.** O dump é **gitignored** (só o índice é versionado), então "não achei" tem **três** significados distintos:

  | `references/eft-decompiled/types-index.json` | `.cs` em disco | Significado | Ação |
  |---|---|---|---|
  | tem o tipo | presente | existe | ler o `.cs` e provar a assinatura |
  | tem o tipo | **ausente** | existe — **o dump não está nesta máquina** | `bash scripts/decompile-eft.sh` (ou `ilspycmd -t` pontual) |
  | **não tem** | — | não existe no assembly | investigar / reportar falha do harness |

- **Prevenção:** tratar todo ponto de recon como **candidato até reconfirmar** — pelo `.cs` do dump, e definitivamente **pela compilação**. O compile pega tipo/membro inexistente; **runtime** pega injeção de campo (`___field`) e método ofuscado errado → envolver `Enable()` em try/catch + gate de validação in-game. Antes de concluir "não existe", **confira o `types-index.json`**. `ilspycmd -t` segue legítimo em três casos: tipo marcado `// DECOMPILE-ERROR` (são 8), fora do índice, ou dump ausente na máquina.
- **Onde é checado:** skill `graph-code-navigation` ("grafo aponta, leitura prova"); `/create-technical-spec` e `/code-mod` (reconfirmar patch-point antes de codar); skill `spt-mod-best-practices`.

## AP-10 — Buffar/depender de skill EFT inerte

- **Sintoma:** feature que escala/buffa uma skill "funciona" no código mas tem **zero efeito** in-game (XP não sobe, bônus não aplica).
- **Causa raiz:** várias masteries do EFT têm `SkillsSettings` vazio (`[]`) no `globals.json` — sem ação de XP, sem efeito. Buffar/setar uma skill inerte é teatro.
- **Exemplo real:** [CustomClasses](../../mods/CustomClasses/docs/class-skill-catalog.md) (Sessão 8 + 10) — SMG/LMG/HMG/Launcher/AttachedLauncher inertes; o redesign moveu a maestria de armas pesadas do Tanque de skill 🎯 para um **perk 🔧** (Bunker, patch direto em recoil/ergo). A lista concreta de inertes vive em `class-skill-catalog.md §6` — **dado versionado** (muda entre builds do EFT), por isso não é replicada aqui.
- **Prevenção:** antes de escolher uma skill como lever, confirmar que ela **não é inerte** (`globals.json` `SkillsSettings` não-`[]`, ou efeito real no `SkillManager`); skill inerte → entregar o efeito por **patch direto (perk)**, não pela skill. Lista atual no catálogo do mod, não no antipattern (evita stale).
- **Onde é checado:** `/create-technical-spec` (ao decidir lever skill vs patch); skill `spt-mod-best-practices`.

---

## Mapa: antipattern → onde o harness checa

| AP | Spec funcional | Spec técnica (Conformidade §9) | Reviews | Skills | Outro |
|---|---|---|---|---|---|
| AP-01 | — | check 1 (lifecycle) + check 5 (estado entre raids) | `/review-technical-spec` Cat. A | SPT §2, checklist 1 | — |
| AP-02 | critério padrão Fika/multiplayer | check 2 | `/code-review` Cat. A/B | SPT §2, checklist 4 | — |
| AP-03 | — | check 3 | `/review-technical-spec` Cat. C | SPT checklist 10; C# (virtual dispatch) | grafo de código (overrides) |
| AP-04 | — | check 4 | `/review-technical-spec` Cat. C | SPT §8, checklist 9 | — |
| AP-05 | critérios verificáveis | check 6 | `/review-spec`; `/review-technical-spec` Cat. A | — | — |
| AP-06 | — | — | `/apply-code-review` (output) | repo-workflow (artefato 06) | `fix.md.tmpl`; gate `check-delivered-validation.sh` |
| AP-07 | — | check 7 | `/review-technical-spec` Cat. C | C# checklist 13 (reentrância) | — |
| AP-08 | — | check 8 | `/code-review` Cat. B | C# checklist 12 (estado stale) | grafo de código (`affected`) |
| AP-09 | — | reconfirmar patch-point | `/create-technical-spec`; `/code-mod` | `graph-code-navigation`; SPT | `ilspycmd`/`dnSpy` vs `Assembly-CSharp.dll` |
| AP-10 | — | lever skill vs patch | `/create-technical-spec` | SPT | catálogo de skills do mod (`globals.json` `SkillsSettings`) |

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-06-11 | Guilherme | Criação — AP-01..AP-06 a partir da taxonomia de erros reais do mod stances (reviews/fixes dos itens 001, 002, 004) |
| 2026-06-12 | Guilherme | docs(CustomClasses): session memory — UX epic 030-037 executed |
| 2026-06-12 | Guilherme | docs(harness): add SPT antipatterns taxonomy (AP-01..06) wired into skills |
| 2026-06-13 | Guilherme | Adicionados AP-07 (self-reentry/ThreadStatic, PA-02-01) e AP-08 (estado stale em troca de contexto, CR-01-02); fechando gaps D4-01/D4-02 da revisão de valor |
| 2026-06-13 | Guilherme | fix(harness): correct artifact-name and hook-target naming bugs |
| 2026-06-23 | Guilherme | Adicionados AP-09 (recon/decompile curado tratado como verdade — item 005 + CustomClasses S10) e AP-10 (buffar skill EFT inerte — CustomClasses S8/S10), promovidos da memória do CustomClasses (memory-curation §15) |
| 2026-06-23 | Guilherme | feat(CustomClasses): implement 050 signature perks/drawbacks (client) |
| 2026-07-19 | Guilherme | docs(launcher): review 01 da spec tecnica do 030 — 3 bloqueadores |
