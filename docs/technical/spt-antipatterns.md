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
- **Onde é checado:** template `fix.md.tmpl`; skill `repo-workflow-best-practices` (artefato 06); output do `/apply-code-review`.

---

## Mapa: antipattern → onde o harness checa

| AP | Spec funcional | Spec técnica (Conformidade §9) | Reviews | Skills | Outro |
|---|---|---|---|---|---|
| AP-01 | — | check 1 (lifecycle) + check 5 (estado entre raids) | `/review-technical-spec` Cat. A | SPT §2, checklist 1 | — |
| AP-02 | critério padrão Fika/multiplayer | check 2 | `/code-review` Cat. A/B | SPT §2, checklist 4 | — |
| AP-03 | — | check 3 | `/review-technical-spec` Cat. C | SPT checklist 10; C# (virtual dispatch) | grafo de código (overrides) |
| AP-04 | — | check 4 | `/review-technical-spec` Cat. C | SPT §8, checklist 9 | — |
| AP-05 | critérios verificáveis | check 6 | `/review-spec`; `/review-technical-spec` Cat. A | — | — |
| AP-06 | — | — | `/apply-code-review` (output) | repo-workflow (artefato 06) | `fix.md.tmpl` |

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-06-11 | Guilherme | Criação — AP-01..AP-06 a partir da taxonomia de erros reais do mod stances (reviews/fixes dos itens 001, 002, 004) |
| 2026-06-12 | Guilherme | docs(CustomClasses): session memory — UX epic 030-037 executed |
| 2026-06-12 | Guilherme | docs(harness): add SPT antipatterns taxonomy (AP-01..06) wired into skills |
