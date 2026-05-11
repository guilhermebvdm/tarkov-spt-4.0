---
name: repo-workflow-best-practices
description: Repository conventions and integration rules for the SPT mod backlog workflow. Use during /create-spec, /create-technical-spec, /review-spec, /review-technical-spec, /code-mod, /code-review, /apply-code-review to ensure consistent artifact naming, traceability between specs/reviews/code, sandbox isolation (modded/ vs original/), and status transitions in mod-backlog.md. Complements `spt-mod-best-practices` (lifecycle) and `csharp-mod-best-practices` (language).
---

# Repo Workflow Best Practices

Convenções de organização e integração entre commands/specs/reviews/implementação no fluxo de backlog deste repo. Pair com `spt-mod-best-practices` (lifecycle/raid) e `csharp-mod-best-practices` (linguagem).

## 1. Sandbox e hierarquia de fontes

- **`mods/<mod>/modded/`** é o **único** diretório editável dentro de cada mod. Todo trabalho de implementação acontece aqui.
- **`mods/<mod>/original/`** é o upstream intocado. **Nunca modificar.** Se precisar de algo de lá, copiar para `modded/` e marcar com `// ref: original/<arquivo>:<linha>`.
- **Hierarquia de fontes obrigatória** ao buscar evidência para spec técnica ou code review:
  1. 🥇 **Assembly descompilado** — `references/eft-decompiled/Assembly-CSharp/` (cite `arquivo.cs:linha` exato).
  2. 🥈 **Código do mod** — `mods/<mod>/original/` (padrões upstream) e `mods/<mod>/modded/` (fork em progresso).
  3. 🥉 **Wiki SPT** — `wiki/spt/` para questões de modding/profile/server.
  4. 🪛 **Web** — só último recurso. Marcar `[fonte externa]` no texto.

## 2. Convenção de nomenclatura de artefatos

Todo artefato de um item de backlog vive em `mods/<mod>/backlog/NNN-<slug>/` e usa prefixo numérico de **ordem cronológica de geração**:

| Ordem | Sufixo | Tipo | Iterativo | Quem gera |
| --- | --- | --- | --- | --- |
| 01 | `-01-spec.md` | Spec funcional | não | `/create-spec` |
| 02 | `-02-spec-tech.md` | Spec técnica | não | `/create-technical-spec` |
| 03 | `-03-spec-tech-review-NN.md` | Review da spec técnica | sim (NN) | `/review-technical-spec` |
| 04 | `-04-code-review-NN.md` | Review do código | sim (NN) | `/code-review` |
| 05 | `-05-asbuild.md` | Documentação pós-build | não | `/code-mod` cria; `/apply-code-review` atualiza |
| 06 | `-06-fix-NN.md` | Correção pontual posterior | sim (NN) | manual |

- **Ordem visual = ordem do ciclo.** Listar a pasta mostra o histórico do item.
- **`/review-spec` não recebe número** — edita inline a spec funcional, sem gerar novo arquivo.
- **NN sempre 2 dígitos zero-padded** (`01`, `02`, …, `99`). Slug com `-` (kebab-case, sem stopwords nem acentos).

## 3. Fluxo do ciclo de backlog

```
/add-backlog-item
        ↓
/create-spec        → 01-spec.md
        ↓
/review-spec        (inline na 01-spec.md)
        ↓
/create-technical-spec → 02-spec-tech.md
        ↓
/review-technical-spec → 03-spec-tech-review-NN.md (iterativo)
        ↓ (zerar 🔴)
/code-mod           → edita modded/ + cria 05-asbuild.md + atualiza PROPRIEDADES.md + mod-backlog.md → 🟢
        ↓
/code-review        → 04-code-review-NN.md (iterativo)
        ↓ (achados aceitos)
/apply-code-review  → edita modded/ + marca achados ✅ Aplicado + atualiza 05-asbuild.md
        ↓
/compile-mod        (gera .dll, fora do ciclo de backlog)
```

Cada etapa **exige** o artefato da anterior. Cada review iterativa **exige resolução de todos os 🔴** antes da próxima etapa avançar.

## 4. Rastreabilidade entre artefatos

Cada achado de review recebe um **ID permanente, nunca reutilizado**, com prefixo distinto por tipo:

- **PA-NN-MM** — achado de spec-tech-review (Plano de Análise). `NN` = rodada, `MM` = ordem do ponto naquela rodada.
- **CR-NN-MM** — achado de code-review (Code Review). Mesmo esquema.

Quando um fix dispara mudança em código, o commit/edit inclui o ID no comentário inline:

```csharp
// ref: CR-01-03   // ou: ref: PA-02-05
SomeMethod();
```

Isso permite navegar do código → review → spec que originou. A seção "Resolução" de cada ponto de review documenta o que foi feito; o histórico da spec resume a rodada inteira em uma linha.

## 5. Edição inline vs. arquivo novo

| Comando | Modo | Razão |
| --- | --- | --- |
| `/review-spec` | **inline** na `01-spec.md` | Gaps de spec são corrigíveis na fonte; manter uma única versão evita drift. |
| `/review-technical-spec` | **arquivo novo** `03-spec-tech-review-NN.md` | Análise crítica é histórica; nunca reescrever uma rodada anterior. |
| `/code-review` | **arquivo novo** `04-code-review-NN.md` | Mesmo motivo do anterior. |
| `/apply-code-review` | edita `modded/` + **marca** pontos no `04-code-review-NN.md` como ✅ Aplicado | Preserva o achado original; só adiciona resolução. |

Regra geral: **artefatos de análise (reviews) são imutáveis** — só ganham anotações de resolução. Artefatos de produto (spec, código) são mutáveis e versionados pelo git.

## 6. Status do item no `mod-backlog.md`

Cada mod tem um `mods/<mod>/backlog/mod-backlog.md` com tabela. Coluna **Status** segue o emoji:

| Emoji | Significado | Quem transita |
| --- | --- | --- |
| ⚪ | Backlog | criado por `/add-backlog-item` |
| 🟡 | Em progresso | transição em `/code-mod` no início; também durante reviews iterativos prolongados |
| 🟢 | Entregue | `/code-mod` ao concluir build; mantém 🟢 mesmo após `/code-review` (a menos que apareça 🔴) |
| 🔴 | Cancelado | manual; preserva pasta para histórico |

Se `/code-review` levantar bloqueadores 🔴, voltar o status para 🟡 no `mod-backlog.md` até `/apply-code-review` resolver.

## 7. `PROPRIEDADES.md` como single source

Toda nova `ConfigEntry` exposta no F12 do BepInEx exige update em `mods/<mod>/PROPRIEDADES.md` (tabela com Nome EN, Tradução pt-BR, Tipo, Padrão, Faixa, Tooltip pt-BR). `/code-mod` e `/apply-code-review` ambos têm essa responsabilidade — se o checklist da spec técnica adiciona uma `ConfigEntry`, o documento precisa refletir.

Section renames em `Config.Bind(section, key, ...)` são **breaking changes** — BepInEx casa por `(section, key)` literal; renomear seção recria a entrada com default e descarta o valor do usuário. Documentar em changelog do mod + instrução de migração manual.

## 8. `mod-backlog.md` como índice por mod

Um arquivo por mod, na raiz de `mods/<mod>/backlog/`. Cada linha é um item:

```markdown
| NNN | Título | Resumo curto | [NNN-slug/](./NNN-slug/) | <emoji> |
```

A **ordem da tabela representa a ordem de execução desejada**, não a ordem cronológica de criação. Renumerar pastas é caro (pasta é citada em git history), então a ordem é controlada por reordenamento da tabela, não da pasta.

## Checklist (use ao escrever/revisar artefatos do backlog)

1. **Nomenclatura:** artefato segue `NNN-<slug>-MM-tipo[-NN].md`?
2. **Sandbox:** edits estão somente em `modded/`? `original/` foi tocado por engano?
3. **Refs ao Assembly:** todo `arquivo.cs:linha` foi conferido contra `references/eft-decompiled/`?
4. **Rastreabilidade:** se há fix de código, o comentário inline cita PA-NN-MM ou CR-NN-MM?
5. **Reviews imutáveis:** review anterior foi editado retroativamente (revisionismo)? Pontos resolvidos foram marcados com ✅ + Resolução em vez de reescritos?
6. **Status:** o `mod-backlog.md` reflete a etapa atual do item?
7. **PROPRIEDADES.md:** novas `ConfigEntry` foram documentadas?
8. **Section renames em `Config.Bind`:** breaking change foi sinalizado no changelog?

Se algum item falha, parar e corrigir antes de avançar para a próxima etapa do fluxo.