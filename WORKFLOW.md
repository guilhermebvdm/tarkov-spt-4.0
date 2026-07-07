# WORKFLOW — Desenvolvimento de mods neste repo

Visão canônica do ciclo de desenvolvimento de mods e das camadas transversais do harness. Complementa [AGENTS.md](AGENTS.md) (contexto geral), [.agents/conventions.md](.agents/conventions.md) (convenções) e [.agents/resources.md](.agents/resources.md) (fontes) — este documento explica o **fluxo**; as regras vivem nesses arquivos e nas skills (aponta, não duplica).

## O ciclo completo

```
/add-mod-repo-for-modding <git-url>          ← mod novo (clone → original/ + modded/ + PROPRIEDADES.md + grafo)
        │
/add-backlog-item <mod> "<descrição>"        ← cria mods/<mod>/backlog/NNN-<slug>/
        ↓
/create-spec <ref>                           → NNN-<slug>-01-spec.md          (funcional: critérios + corner cases)
        ↓
/review-spec <ref>                           → edição inline da 01-spec       (gaps, contradições, critérios padrão)
        ↓
/create-technical-spec <ref>                 → NNN-<slug>-02-spec-tech.md     (refs ao Assembly + conformidade §9)
        ↓
/review-technical-spec <ref>                 → NNN-<slug>-03-spec-tech-review-NN.md   (NN rounds, até zerar 🔴)
        ↓
/code-mod <ref>                              → código em modded/ + NNN-<slug>-05-asbuild.md
        ↓
/code-review <ref>                           → NNN-<slug>-04-code-review-NN.md        (NN rounds)
        ↓
/apply-code-review <ref> [--review NN]       → aplica achados aceitos + anota // ref: CR-NN-MM
        ↓
/compile-mod <mod>                           → build .dll/.js + instala no SPT
        ↓
  validação in-game                          ← critério de "entregue" (compilar ≠ funcionar — AP-06)
        ↓
  06-fix-NN (se necessário)                  ← via .agents/templates/fix.md.tmpl (checklist de validação obrigatório)
        ↓
/update-memory [<mod>]                       → sessions.md (decisões, lições, pendências P-N.M, GC, promoções)
        ↓
/update-mod-graph <mod>                      → regenera o grafo do mod (quando houve mudança de código)
```

Convenção de artefatos: `NNN-<slug>-MM-tipo[-NN].md` — ordem visual = ordem do ciclo. Detalhes: skill `repo-workflow-best-practices`.

## Papel de cada command

| Command | Pré-condição | Artefato | Nota |
|---|---|---|---|
| [/add-mod-repo-for-modding](.claude/commands/add-mod-repo-for-modding.md) | URL git | `mods/<Nome>/` (original/ + modded/ + mod.json + PROPRIEDADES.md + grafo) | `original/` é intocável; trabalho em `modded/` |
| [/add-backlog-item](.claude/commands/add-backlog-item.md) | mod existe | pasta `NNN-<slug>/` + linha no `mod-backlog.md` | invoca `/create-spec` |
| [/create-spec](.claude/commands/create-spec.md) | item criado | `01-spec.md` | sem código/classes EFT; critérios padrão Fika + estado entre raids obrigatórios |
| [/review-spec](.claude/commands/review-spec.md) | 01 existe | edição inline | lições da memória que a spec ignora = gaps |
| [/create-technical-spec](.claude/commands/create-technical-spec.md) | 01 com conteúdo | `02-spec-tech.md` | grafo antes do Grep; toda ref com `arquivo.cs:linha`; §9 conformidade preenchida |
| [/review-technical-spec](.claude/commands/review-technical-spec.md) | 02 existe | `03-spec-tech-review-NN.md` | 🔴 zera antes do build; valida §9 e auditoria de overrides |
| [/code-mod](.claude/commands/code-mod.md) | 03 sem 🔴 | código + `05-asbuild.md` | só `modded/`; reuso via grafo do mod |
| [/code-review](.claude/commands/code-review.md) | 05 existe | `04-code-review-NN.md` | 6 categorias × 4 impactos; impacto do diff via `affected` |
| [/apply-code-review](.claude/commands/apply-code-review.md) | achados marcados `[x]` | edits + anotações ✅/⏭️ | reviews são imutáveis — só anotadas |
| [/compile-mod](.claude/commands/compile-mod.md) | código pronto | `.dll`/`.js` instalado | path do SPT em `.spt-path` |
| [/update-memory](.claude/commands/update-memory.md) | sessão com conteúdo | `sessions.md` (mod e/ou repo) | lições obrigatórias; GC >30d; propõe promoções e `/update-mod-graph` |
| [/update-mod-graph](.claude/commands/update-mod-graph.md) | graphify instalado | `references/graphs/mods/<mod>/` | pós-grandes atualizações; commit junto com o código |

## Camadas transversais

### Skills (carregadas pelos commands)

| Skill | Quando |
|---|---|
| `spt-mod-best-practices` | spec técnica, reviews, code — lifecycle de raid, patches, leaks, API canônica |
| `csharp-mod-best-practices` | idem — C#/Unity: memória, threading, reflection, virtual dispatch |
| `repo-workflow-best-practices` | todo o ciclo — naming, rastreabilidade PA/CR, sandbox, status |
| `memory-curation` | `/update-memory` (escrita) + passo "Contexto de memória" dos commands (consumo, §14) |
| `graph-code-navigation` | spec técnica, reviews, code — grafo vs Grep, receitas de query, "grafo aponta, leitura prova" |

### Memória

- `mods/<mod>/memory/sessions.md` — narrativa por mod: decisões com porquê, **lições/hipóteses descartadas**, pendências `[P-N.M]` (🔴/🟡/🟢), snapshot delta no topo.
- **Consumo:** todo command de desenvolvimento lê o topo da memória do mod antes de trabalhar (pendência 🔴 → alerta). **Escrita:** `/update-memory`. Lições recorrentes (≥2 sessões) são promovidas para [docs/technical/spt-antipatterns.md](docs/technical/spt-antipatterns.md) ou skills.

### Grafos de código

Grafos AST (graphify) de todas as fontes em [references/graphs/](references/graphs/) — versionados; MCP só para eft-decompiled (grafos de mod via CLI `--graph`); regeneração via `scripts/update-graphs.sh`. Regra: **grafo aponta, leitura do `arquivo.cs:linha` prova**. Ver [references/graphs/README.md](references/graphs/README.md).

### Hierarquia de fontes e antipatterns

- Evidência técnica: [.agents/resources.md](.agents/resources.md) § Hierarquia de evidência (Assembly 🥇 → web 🪛).
- Erros já cometidos: [docs/technical/spt-antipatterns.md](docs/technical/spt-antipatterns.md) (AP-01..AP-08) — leitura obrigatória antes de spec/review técnica; checados na §9 da spec técnica, nos critérios padrão da spec funcional e no checklist do fix.
