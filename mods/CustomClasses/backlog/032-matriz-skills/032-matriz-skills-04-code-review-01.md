# 032 — Matriz de skills — Code-review (01)

**Mod:** CustomClasses
**Criado:** 2026-06-12
**Refs:** [05-asbuild](./032-matriz-skills-05-asbuild.md) · [02-spec-tech](./032-matriz-skills-02-spec-tech.md) · [03-spec-tech-review-01](./032-matriz-skills-03-spec-tech-review-01.md)

Revisão de código do item 032 sobre o diff real:
- `modded/Server/Web/Pages/SkillsMatrix.razor` (CRIADO — untracked, revisado na íntegra)
- `modded/Server/Web/Shared/NavMenu.razor` (EDITADO — 1 `MudNavLink` "Skills matrix")

Código de referência lido para validar assinaturas e contratos (NÃO inventado): `SkillMaster.cs`,
`CostService.cs` (`ComputeSkillCost`/`SkillCostBreakdown`), `ClassEditorService.cs`
(`GetCachedEntries`/`ClassFileEntry`), `ClassDefinition.cs` (`Skills`/`SkillMultipliers`/`IconFile`/
`NameColor`), `NavMenu.razor` (`LoadRows`/`SidebarRow`) e `SkillCanonicalList.razor` (`MultiplierChip`,
overflow, lookups case-insensitive). Build de baseline: **verde** (0 warning / 0 erro).

Política autônoma: aplico só achados SEGUROS (bug null/crash, build-breaker, fuga de spec com fix
inequívoco/local, leak/dispose, fio solto óbvio). Design/layout, fixes ambíguos e cross-território → ADIADOS.

## Veredito

Nenhum achado seguro para aplicar. A página é um port fiel e cuidadoso de `NavMenu.LoadRows` +
`SkillCanonicalList` + `profiles-skills.*`: nulos guardados (`def?.Skills ?? new(...)`, `col.IconUrl is
not null`, `MultiplierOf` retorna `null`), dicionários `OrdinalIgnoreCase` (contrato B2), custo computado
1×/classe em `OnInitialized` (D3/CA7), sem `IDisposable`/`LocationChanged` por ser página de destino (D4),
`colspan` dinâmico (R1), fade por célula (R3), ícone fora do bloco rotacionado (R2). Os bloqueadores R1–R3
do review da spec técnica estão implementados como descrito. Build permanece verde.

Todos os achados abaixo são qualidade/cosmético → **ADIADOS** (não tocam código neste estágio).

## ✅ Aplicados (seguros)

Nenhum.

## ⏸️ Adiados

### CR-01-01 — `@using CustomClasses.Web` não utilizado (cosmético)
**Onde:** `SkillsMatrix.razor:20`.
**Achado:** o `@using CustomClasses.Web` foi copiado de `SkillCanonicalList.razor` (que precisa dele para
`SkillLevelRow`), mas `SkillsMatrix` não referencia nenhum tipo de `CustomClasses.Web` — todos os tipos
usados (`SkillMaster`, `SkillCategory`, `ClassDefinition`, `CostService`, `ClassEditorService`) vivem no
namespace raiz `CustomClasses` (`RootNamespace` do csproj), visível por padrão no componente. Razor não
emite warning de using não usado, então o build segue verde.
**Por que adiar:** não é bug/crash/build-breaker/leak; é higiene. Remover é seguro mas opcional; fora das
categorias aplicáveis neste estágio.

### CR-01-02 — `ColumnSpan` re-executa `VisibleColumns()` (`.Where().ToList()`) por separador (eficiência)
**Onde:** `SkillsMatrix.razor:339` (`ColumnSpan => 1 + VisibleColumns().Count`), consumido em
`CategoryHeader`/`OverflowHeader`.
**Achado:** o bloco principal já calcula `var visible = VisibleColumns();` uma vez e o repassa às linhas.
`ColumnSpan`, porém, chama `VisibleColumns()` de novo a cada header de categoria/overflow — uma alocação
`List` extra por separador (≈5 categorias + overflow). Correção funcional: ambos dependem só de
`_showDisabled` (estável dentro de um render), então o colspan SEMPRE bate com a contagem real de colunas
renderizadas — sem desalinhamento. É puramente desperdício de alocação, irrelevante na escala realista
(risco Y1 já registrado no review da spec).
**Por que adiar:** eficiência, não correção. Fix natural (passar `visible.Count` para um campo/derivar o
colspan do `visible` já computado) é uma reorganização de render, não um bug-fix local inequívoco.

### CR-01-03 — Chip de multiplicador em célula vazia/estreita pode estourar layout (design/layout)
**Onde:** `SkillsMatrix.razor:385-389` e `397-400` (`MudChip` dentro de `<td class="cc-skill-cell">`,
`min-width:52px`).
**Achado:** com "Multiplicadores XP" ligado, cada célula ganha um `MudChip` `Size.Small` num `<td>` de
~52px de largura; em colunas estreitas o chip "+NN%"/"−NN%" pode transbordar ou empurrar a grade. É
fidelidade ao viewer (`profiles-skills.js`), mas o viewer não tinha a restrição de largura de coluna
rotacionada do editor.
**Por que adiar:** layout/design — exige validação visual no servidor SPT (a própria as-built lista isso
como validação pendente). Não é crash; ADIADO.

### CR-01-04 — `title` em células/headers não previsto explicitamente na spec (fuga de spec benigna)
**Onde:** `SkillsMatrix.razor:53`, `394`, `415`, `458` (atributos `title=`).
**Achado:** a as-built já registra essa decisão (tooltip "Classe: skill nível", paridade com
`profiles-skills.js:65`). É um superset cosmético da spec, sem efeito colateral.
**Por que adiar:** fuga de spec é ADITIVA e cosmética, já documentada como premissa; não há "fix
inequívoco" a aplicar (remover o `title` seria piorar a UX combinada do viewer).

## Notas de verificação (sem achado)

- **Nulos:** `LoadColumns` usa `def?.Skills ?? new Dictionary<string,int>()` e idem para
  `SkillMultipliers`; `cost = def is null ? null : CostService.ComputeSkillCost(def)` com `?? 0`/`?? false`
  nos derivados. Sem caminho de `NullReferenceException`/`KeyNotFoundException`.
- **Contrato 037→032:** consome `EditorService.GetCachedEntries()` (não dispara dry-run pesado) e
  `CostService.ComputeSkillCost` 1×/classe em `OnInitialized`, espelhando `NavMenu.LoadRows` — confere com
  as assinaturas reais (`GetCachedEntries(): IReadOnlyList<ClassFileEntry>`,
  `ComputeSkillCost(ClassDefinition): SkillCostBreakdown` com `.Total`/`.WithinBudget`).
- **Ordem canônica (031):** linhas iteram `SkillMaster.Entries`; separador via `ColorOf`/`LabelOf`; ordem
  NÃO redefinida na página. Overflow espelha `BuildOverflowEntries` (case-insensitive, primeira aparição).
- **Tier:** `TierOf` = `≤3 low / ≤6 mid / >6 high`, fiel ao comentário e a `profiles-skills.js:64`.
- **Lifecycle (D4):** sem `IDisposable`/`LocationChanged` — correto para página de destino; sem fio solto
  (nenhuma subscription a liberar). Diferente do `NavMenu` (componente persistente) de propósito.
- **NavMenu:** o único delta é o `MudNavLink` "Skills matrix" (`Icons.Material.Filled.GridOn`,
  `Match="NavLinkMatch.Prefix"`, `Href="/customclasses/skills"`) entre "Classes" e o `<MudDivider/>`; nada
  mais mudou. Rota bate com `@page "/customclasses/skills"`.

## Build

`dotnet build mods/CustomClasses/modded/Server/CustomClasses.Server.csproj -c Release --no-incremental
--nologo` → **Compilação com êxito. 0 Aviso(s) / 0 Erro(s)** (antes e depois — nenhum fix aplicado).

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-06-12 | Guilherme | Code-review 01 do item 032. Nenhum achado seguro aplicado; 4 itens adiados (1 using não usado, 1 eficiência de colspan, 2 design/spec aditiva). Build verde. |
