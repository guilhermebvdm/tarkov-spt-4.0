# Handoff — Executar o épico UX do editor de classes (CustomClasses, itens 030–037)

> **Data:** 2026-06-10 · **Sessão de origem:** implementou o épico do editor (018–029), 2 rodadas de code-review, testes de UI no Chrome MCP, e planejou + revisou (UX + desempenho) o épico UX.

## ⚡ Próxima ação (a única coisa que importa)

Executar as waves do épico UX **na ordem**, item a item pelo workflow do repo (`/create-spec NNN` → `/review-spec` → `/create-technical-spec` → `/review-technical-spec` → `/code-mod` → `/compile-mod` → `/code-review`):

```
UX-W0: [037]         ← PERFORMANCE PRIMEIRO (a lentidão atual; 030+ consomem o cache)
UX-W1: [030] [031]   ← sidebar × skills canônicas (paralelos, territórios disjuntos)
UX-W2: [032] [033]   ← matriz × dashboard (paralelos)
UX-W3: [034] [036]   ← loadout visual × comparação A×B (paralelos)
UX-W4: [035]         ← solo (densidade + cliques + regressão Chrome MCP)
```

Começar por: **`/create-spec 037`** (kickoff: [037-performance-cache-00-kickoff.md](../mods/CustomClasses/backlog/037-performance-cache/037-performance-cache-00-kickoff.md)).

## 📋 O plano (não duplicar — ler na fonte)

- **Visão geral + waves + métricas-alvo:** [mods/CustomClasses/backlog/mod-backlog.md](../mods/CustomClasses/backlog/mod-backlog.md), seção **"Épico: UX do editor (030–037)"** (tabela de itens nas linhas 030–037).
- **Kickoffs por item** (escopo, deps, riscos, refs, DoD — insumo direto do `/create-spec`):
  - [030 sidebar](../mods/CustomClasses/backlog/030-sidebar-classes/030-sidebar-classes-00-kickoff.md) · [031 skills canônicas](../mods/CustomClasses/backlog/031-skills-ordem-canonica/031-skills-ordem-canonica-00-kickoff.md) · [032 matriz](../mods/CustomClasses/backlog/032-matriz-skills/032-matriz-skills-00-kickoff.md) · [033 dashboard](../mods/CustomClasses/backlog/033-detalhe-single-screen/033-detalhe-single-screen-00-kickoff.md) · [034 loadout visual](../mods/CustomClasses/backlog/034-loadout-visual/034-loadout-visual-00-kickoff.md) · [035 densidade+cliques](../mods/CustomClasses/backlog/035-densidade-cliques/035-densidade-cliques-00-kickoff.md) · [036 comparação A×B](../mods/CustomClasses/backlog/036-comparacao-classes/036-comparacao-classes-00-kickoff.md) · [037 performance](../mods/CustomClasses/backlog/037-performance-cache/037-performance-cache-00-kickoff.md)
- **Contexto/decisões acumuladas:** [mods/CustomClasses/memory/sessions.md](../mods/CustomClasses/memory/sessions.md) (entradas de 2026-06-10) · doc do editor: [docs/class-editor.md](../mods/CustomClasses/docs/class-editor.md) · schema: [docs/class-schema.md](../mods/CustomClasses/docs/class-schema.md).

## 🔧 Modelo de execução que funcionou no épico anterior (replicar)

1. **Sub-agents paralelos por wave** com **territórios de arquivo explícitos** no prompt (quem toca o quê; "NÃO toque em X") + **exclusividade de `dotnet build` para UM agente** por wave (os outros verificam símbolos contra o código real; o orquestrador roda o build integrado depois).
2. Cada agente entrega: implementação + `NNN-...-01-spec.md` + `02-spec-tech.md` + `05-asbuild.md` na pasta do item, e marca a própria linha 🟢 no mod-backlog.
3. **Entre waves:** build integrado (`dotnet build mods/CustomClasses/modded/Server/CustomClasses.Server.csproj -c Release --no-incremental`) → `bash .agents/scripts/compile-mod.sh CustomClasses` → smoke no server real.
4. **Fechamento:** code-review consolidado (2 rodadas no épico anterior acharam 22 bugs reais) + bateria de UI no Chrome MCP + fixes + re-teste.

## 🖥️ Receita de teste no ambiente (fatos que custaram a descobrir)

- **Subir o server:** `cd D:/SPT/SPT && DISABLE_VIRTUAL_TERMINAL=1 ./SPT.Server.exe` em background (sem a env var ele crasha com stdout redirecionado). Boot ~45s; log esperado: `Loaded 11 class(es)`.
- **URL:** `https://26.207.194.149:6969/customclasses/...` — o bind é o IP do **fika-server** (`user/mods/fika-server/assets/configs/fika.jsonc`), NÃO o `http.json` (que diz 127.0.0.1). Cert self-signed: digitar `thisisunsafe` no interstitial do Chrome.
- **Chrome MCP:** MudTable/MudTextField **não respondem a eventos JS sintéticos** — usar `fill`/`press_key` (teclado real) para interações; cliques em linha de tabela via `evaluate_script` com `row.click()` funcionam.
- **Matar o server:** `Stop-Process` no PID do `SPT.Server` (kill bash não basta); conferir porta 6969 livre.
- **Guard rails ativos:** `compile-mod.sh` bloqueia clobber de `config/classes/` divergente (`--force-config` para forçar; `/sync-classes` traz install→repo). Gerador `build-class-jsons.js` congelado (`--force`).
- Console do editor tem **1 erro cosmético conhecido** (`MudPointerEventsNone has already been declared`) — padrão upstream do SE/host, documentado, NÃO tentar corrigir (tentativa com loader inline já falhou e foi revertida).

## 🧠 Skills sugeridas para a próxima sessão

`/create-spec`, `/review-spec`, `/create-technical-spec`, `/review-technical-spec`, `/code-mod`, `/compile-mod`, `/code-review`, `/apply-code-review` (fluxo por item) · `spt-mod-best-practices` + `csharp-mod-best-practices` + `repo-workflow-best-practices` (consultadas pelas specs) · `/sync-classes` (se houver edição via editor durante testes) · `/update-memory` no fim.

## ⚠️ Pendências fora do épico (não esquecer)

1. **Working tree gigante não commitado** — épico 018–029 inteiro + fixes + este planejamento + blocos 010–017 de sessões paralelas. Decisão de commit é do usuário (sugerir agrupar: épico editor / épico UX-plan / identidade visual 010–017).
2. **Validação in-game pendente** do builder pós-CR-EP-01 (stash com preset/mods/ammo/contents agora spawna montado — criar perfil novo no launcher e conferir nascimento).
3. Housekeeping deferido: CR-EP-10 (ícones client×server sem validação cruzada), CR2-EP-05 (óptica mínima não precificada), página `/customclasses/picker-test` (rota de dev).

## Estado do backlog ao gerar este handoff

018–029 🟢 (editor completo, testado) · 030–037 ⚪ (épico UX, kickoffs prontos) · 013 🟡 e 017 🟢 são de outras frentes (não tocar).
