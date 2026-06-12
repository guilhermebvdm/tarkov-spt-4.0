# 032 — Matriz de skills (classes × skills, heatmap) — As-built

**Mod:** CustomClasses
**Criado:** 2026-06-12
**Refs:** [02-spec-tech](./032-matriz-skills-02-spec-tech.md) · [00-kickoff](./032-matriz-skills-00-kickoff.md)

## Arquivos

| Arquivo | Ação |
|---|---|
| `modded/Server/Web/Pages/SkillsMatrix.razor` | CRIADO — página `@page "/customclasses/skills"`, matriz heatmap skills×classes + rodapé de custo + 2 toggles |
| `modded/Server/Web/Shared/NavMenu.razor` | EDITADO — 1 `MudNavLink` "Skills matrix" (`GridOn`) entre "Classes" e o `<MudDivider/>`; nada mais mudou |

`dotnet build` NÃO foi rodado (estágio dedicado). `mod-backlog.md`/`PROPRIEDADES.md` não tocados. Sem commit.

## O que foi implementado (conforme spec)

- **Linhas = `SkillMaster.Entries`** na ordem canônica (031), com separador de categoria via `SkillMaster.ColorOf`/`LabelOf` quando a categoria muda — ordem NÃO redefinida aqui.
- **Colunas = `EditorService.GetCachedEntries()`** projetadas 1× em `OnInitialized` (`LoadColumns`), espelho exato de `NavMenu.LoadRows`: 1 `CostService.ComputeSkillCost` por classe, ícone `/CustomClasses-Server/icons/{icon}`, nome na `NameColor`.
- **Células heatmap** por tier (`TierOf`: ≤3 low / ≤6 mid / >6 high), vazio = nível 0; clicáveis (mesmo vazias) → `NavigateTo` detalhe da classe.
- **Header vertical** (`writing-mode: vertical-rl`) com o ícone FORA do bloco rotacionado (R2), clicável.
- **Rodapé de custo** (`tfoot`): "—" quando sem custo, valor com `cc-cost--ok`/`cc-cost--over` conforme `WithinBudget`.
- **Toggles**: "Mostrar desabilitadas" (default on) filtra colunas em `VisibleColumns()` só no render; "Multiplicadores XP" (default off) adiciona chip ±% por célula. Nenhum recomputa custo (CA7).
- **Overflow**: skills definidas por alguma classe fora do `SkillMaster` listadas em seção própria (espelho de `BuildOverflowEntries`).
- **Vazio**: `MudAlert` quando `_columns.Count == 0`.
- **colspan dinâmico** (`ColumnSpan`, R1) e **fade por célula** via `cc-cell--disabled` a partir de `col.Enabled` (R3, não `nth-child`).
- **Link no NavMenu** apenas (sem mexer no resto da sidebar).

## Decisões / premissas registradas durante a implementação

- **Lookup de multiplicador simplificado** — usei `col.Multipliers.TryGetValue` (o dicionário já é construído com `StringComparer.OrdinalIgnoreCase` em `LoadColumns`), em vez do laço linear de `SkillCanonicalList.MultiplierOf`. Mesmo resultado case-insensitive, sem alocação por célula. (Premissa: equivalente funcional ao contrato B2.)
- **`title` nas células e header** (não previsto explicitamente na spec, presente no viewer `profiles-skills.js:65`) — mantido para paridade de UX (tooltip "Classe: skill nível"). Cosmético.
- **Tokens do viewer substituídos**: `--accent-bright`→`#e7c46a` literal, `--fg-*`/`--bg-hover`/`--border-subtle`→`--mud-palette-*` ou `rgba`. Cores de tier (`rgba(166,124,0,...)`/`rgba(200,160,0,...)`) portadas literais de `profiles-skills.css:96-107`.
- **Sem `IDisposable`/`LocationChanged`** (D4) — página de destino; re-monta em navegação com o cache 037 já fresco.
- **Custo formatado `0`** no rodapé (inteiro, como a sidebar); tooltip mostra `0.##` para precisão.

## Validação pendente (fora do escopo deste estágio)

- Compilar (estágio dedicado) e abrir `/customclasses/skills` no servidor SPT para conferir render do header rotacionado, heatmap e clique→detalhe (feedback de memória: escritas SPT exigem validação no jogo, não só write).

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-06-12 | Guilherme | As-built da implementação do 032 (SkillsMatrix.razor criado, link no NavMenu, premissas registradas). |
