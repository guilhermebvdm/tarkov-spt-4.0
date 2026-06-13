# 033 — Detalhe single-screen (dashboard) — Spec técnica

**Mod:** CustomClasses
**Criado:** 2026-06-12
**Refs:** [01-spec](./033-detalhe-single-screen-01-spec.md) · [00-kickoff](./033-detalhe-single-screen-00-kickoff.md) · [031-02-spec-tech](../031-skills-ordem-canonica/031-skills-ordem-canonica-02-spec-tech.md)

## Arquivos tocados

| Arquivo | Ação | Refs reais |
|---|---|---|
| `modded/Server/Web/wwwroot/css/customclasses.css` | **CRIAR** — folha de densidade do mod (wwwroot ainda não existe; criar a árvore) | porta tokens de `tools/tarkov-itemdb/viewer/profiles.css:72-86,152-214` |
| `modded/Server/Web/Layouts/BaseLayout.razor` | **MODIFICAR** — 1 linha `<link>` no `<HeadContent>` | `BaseLayout.razor:18-21` (após `MudBlazor.min.css:19`) |
| `modded/Server/Web/Pages/ClassDetail.razor` | **MODIFICAR** — trocar `MudExpansionPanels` por dashboard 2 colunas; preservar adoção do `SkillCanonicalList` | `ClassDetail.razor:93-356` (markup), header `:22-64`, `@code` `:362-566` intacto exceto remoções |

Não tocados (consumidos como estão): `SkillCanonicalList.razor` (031), `ClassViewItemSpec.razor`, `CostService`, `ClassEditorService`, `CatalogService`, `ClassRegistrar`, `NavMenu.razor` (030), os diálogos de lifecycle.

## `customclasses.css` (CRIAR)

Caminho: `modded/Server/Web/wwwroot/css/customclasses.css`. A pasta `wwwroot/` ainda **não existe** no mod (confirmado: `Glob mods/CustomClasses/modded/Server/Web/wwwroot/**` → vazio); criar `wwwroot/css/` junto.

> **Servir o estático (FATO confirmado, não premissa):** `CustomClassesMetadata.cs:9-11` documenta que o host **monta a pasta `wwwroot/` inteira sob `/CustomClasses-Server/`** (`ref: SPTWeb.cs InitializeSptBlazor/UseSptBlazor`), por o metadata implementar `IModWebMetadata`. Os ícones em `wwwroot/icons/` já são servidos assim (`ClassDetail.razor:29,140`). Logo `wwwroot/css/customclasses.css` é servido em `/CustomClasses-Server/css/customclasses.css` pelo **mesmo** mecanismo — não é uma nova rota, é uma subpasta da que já funciona. Nenhuma premissa pendente.

Classes (nomes estáveis, **reusadas pelo 034** — ver contrato):

```css
/* Layout do dashboard de detalhe — porta de profiles.css:72-86 */
.cc-dash            { display:flex; gap:16px; align-items:flex-start; }
.cc-dash__left      { flex:0 0 300px; display:flex; flex-direction:column; gap:12px; }
.cc-dash__right     { flex:1; min-width:0; display:flex; flex-direction:column; gap:12px; }

/* Densidade tipográfica — 12-14px, line-height 1.3, paddings 4-8px */
.cc-dense           { font-size:13px; line-height:1.3; }
.cc-dense td,
.cc-dense th        { padding:3px 6px !important; }      /* sobrepõe o padding do MudSimpleTable/MudTable denso */
.cc-section         { margin:0; }
.cc-section__title  { font-size:11px; font-weight:600; text-transform:uppercase;
                      letter-spacing:.5px; opacity:.7; margin:0 0 4px; }

/* Badges do header (custos/status) — porta de .meta-badge profiles.css:133-150 */
.cc-badges          { display:flex; gap:16px; flex-wrap:wrap; align-items:flex-end; }
.cc-badge           { display:flex; flex-direction:column; gap:1px; }
.cc-badge__label    { font-size:10px; text-transform:uppercase; letter-spacing:.5px; opacity:.6; }
.cc-badge__value    { font-size:15px; font-weight:600; font-variant-numeric:tabular-nums; }

/* Chips compactos de hideout */
.cc-hideout-grid    { display:flex; flex-wrap:wrap; gap:4px; }

/* Slot de equipado (coluna direita textual) — ponto de extensão do 034 */
.cc-equip-slot      { margin-top:6px; }
.cc-equip-slot__label { font-size:11px; opacity:.7; text-transform:uppercase; letter-spacing:.4px; }
```

> **Premissa técnica PT-2 (escopo do `!important`):** o `!important` em `.cc-dense td/th` existe porque o MudBlazor injeta padding via CSS próprio com especificidade alta; sem ele a densidade não vence o tema. O escopo é restrito à classe `.cc-dense` (aplicada só nas tabelas do dashboard), então não vaza para o resto do editor. Cosmético, ajustável no 035.

## `BaseLayout.razor` (MODIFICAR — 1 linha)

Adicionar **uma** linha no `<HeadContent>` (`BaseLayout.razor:18-21`), logo após o link do MudBlazor (`:19`), sem tocar em mais nada do que o 030 montou:

```razor
<HeadContent>
    <link href="@Assets["_content/MudBlazor/MudBlazor.min.css"]" rel="stylesheet" />
    <link href="/CustomClasses-Server/css/customclasses.css" rel="stylesheet" />   @* item 033 *@
    <meta name="robots" content="noindex, nofollow">
</HeadContent>
```

O `<link>` é estático (não usa `@Assets[...]` porque o arquivo é do mod, servido pelo prefixo `/CustomClasses-Server/`, não um asset RCL do framework). Nenhuma outra parte do layout (drawer, appbar, guard, `CascadingValue`) é alterada.

## `ClassDetail.razor` (MODIFICAR)

### Header compacto + badges (`:22-64`)

Manter a `MudStack Row` do header (back, ícone, nome `NameStyle()`, `StatusChip`, spacer, botões Edit/Duplicate/Delete) — **handlers e navegação intactos** (`OpenDuplicateDialogAsync :435`, `OpenDeleteDialogAsync :457`, Edit href `:45`). Abaixo da linha de ações, quando `_entry?.Definition is not null`, inserir a faixa de badges:

```razor
<div class="cc-badges mb-2">
    <div class="cc-badge">
        <span class="cc-badge__label">Skill cost</span>
        <span class="cc-badge__value">
            @(_skillCost?.Total.ToString("0.00", CultureInfo.InvariantCulture) ?? "—")
            @SkillTotalChip()   @* reusa o fragment existente :537 *@
        </span>
    </div>
    <div class="cc-badge">
        <span class="cc-badge__label">Loadout ₽</span>
        <span class="cc-badge__value">@(_loadoutCost is null ? "—" : FormatRub(_loadoutCost.TotalRub))</span>
    </div>
    <div class="cc-badge">
        <span class="cc-badge__label">Base edition</span>
        <span class="cc-badge__value" style="font-size:12px;">
            @(string.IsNullOrWhiteSpace(def.BaseEdition) ? "Zero to hero" : def.BaseEdition)
        </span>
    </div>
</div>
```

Reusa `_skillCost`/`_loadoutCost` (já computados em `Reload()` `:402-403`), `SkillTotalChip()` (`:537`), `FormatRub()` (`:514`). A descrição (`def.Description?.En`) vira 1 linha com `MudTooltip` + `text-overflow:ellipsis` (CSS `.cc-badge`/inline). `enabled` continua via `StatusChip` (`:517`) que já cobre Disabled.

### Substituir `MudExpansionPanels` (`:93-356`) por grid 2 colunas

Remover o `<MudExpansionPanels>` inteiro (`:93-356`). Os diagnostics (`:74-80`) e o branch de `Definition is null` (`:82-88`) **permanecem** acima do grid (corner case 1). Quando há `def`, renderizar:

```razor
<div class="cc-dash cc-dense mt-2">
    @* ── COLUNA ESQUERDA: skills + hideout + outfit ───────────────── *@
    <div class="cc-dash__left">
        <div class="cc-section">
            <div class="cc-section__title">Skills (@(def.Skills?.Count ?? 0))</div>
            @* PRESERVA a adoção do 031 — NÃO reimplementar *@
            <SkillCanonicalList Levels="@def.Skills"
                                Cost="@_skillCost"
                                Multipliers="@def.SkillMultipliers"
                                Editable="false"/>
            @if (_skillCost is { Skills.Count: > 0 })
            {
                <MudStack Row="true" AlignItems="AlignItems.Center" Spacing="2" Class="mt-1">
                    <MudText Typo="Typo.caption"><b>Weighted:</b> @_skillCost.Total.ToString("0.00", CultureInfo.InvariantCulture)</MudText>
                    @SkillTotalChip()
                    <MudText Typo="Typo.caption">budget [@SkillWeights.BudgetMin.ToString("0"), @SkillWeights.BudgetMax.ToString("0")]</MudText>
                </MudStack>
                @foreach (var w in _skillCost.Warnings) { <MudAlert Severity="Severity.Warning" Dense="true" Class="mt-1">@w</MudAlert> }
            }
            @if (!_seInstalled && def.SkillMultipliers?.Keys.Any(IsSeSkill) == true)
            {
                <MudText Typo="Typo.caption" Color="Color.Warning" Class="mt-1">
                    Skills-Extended not installed — its multipliers are inert.
                </MudText>
            }
        </div>

        <div class="cc-section">
            <div class="cc-section__title">Hideout (@(def.Hideout?.Count ?? 0))</div>
            @if (def.Hideout is { Count: > 0 } hideout)
            {
                <div class="cc-hideout-grid">
                    @foreach (var (station, lvl) in hideout)
                    {
                        <MudChip T="string" Size="Size.Small" Color="Color.Default">@station L@lvl</MudChip>
                    }
                </div>
            }
            else { <MudText Typo="Typo.caption">No hideout overrides.</MudText> }
        </div>

        <div class="cc-section">
            <div class="cc-section__title">Outfit</div>
            @* mantém a MudSimpleTable de outfit (:237-253), agora denso via .cc-dense; ClothingLabel :504 intacto *@
            ...
        </div>
    </div>

    @* ── COLUNA DIREITA: equipado + stash (TEXTUAL — contrato 033→034) ─ *@
    <div class="cc-dash__right">
        @* ░░ EXTENSION POINT 034 ░░ trocar este bloco "Equipped" por <GearPanel Equipped="@def.Loadout?.Equipped"/> *@
        <div class="cc-section" id="cc-equipped">
            <div class="cc-section__title">Equipped (@(def.Loadout?.Equipped?.Count ?? 0) slots)</div>
            @if (def.Loadout?.Equipped is { Count: > 0 } equipped)
            {
                @foreach (var (slot, spec) in equipped)
                {
                    <div class="cc-equip-slot">
                        <div class="cc-equip-slot__label">@slot</div>
                        <ClassViewItemSpec Spec="@spec"/>   @* renderer recursivo existente — reusado tal qual *@
                    </div>
                }
            }
            else { <MudText Typo="Typo.caption">No equipped items.</MudText> }
        </div>

        @* ░░ EXTENSION POINT 034 ░░ trocar este bloco "Stash" por <StashPanel Lines="@_stashLines"/> *@
        <div class="cc-section" id="cc-stash">
            <div class="cc-section__title">Stash (@(def.Loadout?.Stash?.Count ?? 0) lines)</div>
            @* mantém a MudTable de _stashLines (:288-305) com as 6 colunas; agora densa via .cc-dense *@
            ...
        </div>
    </div>
</div>
```

Pontos-chave:
- A tag `<SkillCanonicalList ... Editable="false"/>` é **a mesma** que o 031 já colocou (`:153-156`) — apenas movida para a coluna esquerda. NÃO reescrever o componente.
- `ClassViewItemSpec` (equipado) e a `MudTable` de `_stashLines` (stash) são reusados como estão (`:268`, `:288-305`).
- O painel "XP multipliers" (`:172-206`) é **removido** (P2): os ±% já saem no `SkillCanonicalList`; o aviso de SE-ausente vira a nota fina mostrada acima.
- O painel "Cost summary" full breakdown (`:310-354`) é **removido** (P3): os dois totais migram para os badges do header; warnings de loadout (`:327-330`) podem ir como `MudAlert` fino na coluna direita acima do stash (preserva o sinal de preço faltante).

### `@code` — o que muda e o que fica

Intacto: `Reload()` (`:376`), todos os handlers de lifecycle (`:435,457`), `NameStyle :488`, `MapSeverity :491`, `IsSeSkill :498`, `FactorColor :500`, `ClothingLabel :504`, `FormatRub :514`, `StatusChip :517`, `SkillTotalChip :537`, `MissingPriceBadge :557`.

Possível remoção pós-código: `FactorColor` (`:500`) só era usado pela tabela de multiplicadores removida — verificar uso residual no code-mod e remover **só se** zero callers (não bloqueia a spec). `_stashLines` (`:369`) e `_loadoutCost` (`:368`) continuam usados (stash + badge ₽ + warnings). `_clothingNames`/`_clothingNames` populados em `Reload()` `:408-423` continuam (outfit mantido).

## CONTRATO 033 → 034 (coluna direita)

O 034 troca a **coluna direita** textual por painéis visuais, sem mexer no layout/header/coluna esquerda. Pontos fixados aqui:

1. **Dois blocos nomeados e isolados:** `<div class="cc-section" id="cc-equipped">` e `<div class="cc-section" id="cc-stash">`, cada um marcado com um comentário `░░ EXTENSION POINT 034 ░░`. O 034 substitui o **conteúdo** desses dois `cc-section` (não o container `cc-dash__right`).
2. **Assinaturas de dados estáveis para o 034:**
   - Equipado: `def.Loadout?.Equipped` (`IReadOnlyDictionary<string, ItemSpec>` — slot → spec), o mesmo que alimenta `ClassViewItemSpec` hoje.
   - Stash: `_stashLines` (`List<LoadoutCostEntry>`, já filtrado por `Context == "stash"` em `Reload()` `:404-406`) — o 034 consome a mesma lista precificada.
   - O 034 pode introduzir `GearPanel`/`StashPanel` que recebam exatamente esses tipos como parâmetro; nenhuma transformação de dados nova é exigida do 033.
3. **CSS de densidade reutilizável:** as classes `.cc-dash*`, `.cc-dense`, `.cc-section*`, `.cc-equip-slot*` vivem em `customclasses.css` e são **consumidas pelo 034** (mesma folha; o 034 só adiciona regras de grid de ícones, não reescreve as de densidade/layout).
4. **Meta de scroll:** o 033 aceita que a coluna direita textual possa gerar scroll interno (corner case 3). O "single-screen ≤1 scroll" é DoD do 034 ao compactar equipado/stash em grids — o 033 entrega a estrutura e a densidade que tornam isso possível.

## Decisões de UI

- Texto da UI em inglês; docs pt-BR (consistente com 024/031).
- `MaxWidth` do `MudContainer` (`:22`) passa de `Large` para um container largo / full-width (P4) para a coluna direita respirar. Cosmético.
- Densidade vem de `.cc-dense` no CSS + `Dense="true"` já presente nas tabelas; nenhum componente Mud novo é introduzido.
- Cores de categoria das skills continuam vindo do `SkillMaster.ColorOf` via o `SkillCanonicalList` (031) — o `customclasses.css` **não** redefine cores de skill (evita divergência com o 031).
