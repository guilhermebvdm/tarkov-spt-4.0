# 036 — Modo comparação A×B no dashboard — Spec técnica

**Mod:** CustomClasses
**Status:** Especificado (pós auto-review 03)
**Criado:** 2026-06-12
**Refs:** [01-spec](./036-comparacao-classes-01-spec.md) · [031-02-spec-tech](../031-skills-ordem-canonica/031-skills-ordem-canonica-02-spec-tech.md) · [033-02-spec-tech](../033-detalhe-single-screen/033-detalhe-single-screen-02-spec-tech.md)

## Arquivos tocados

| Arquivo | Ação | Refs reais |
|---|---|---|
| `modded/Server/Web/Pages/ClassDetail.razor` | **MODIFICAR** — picker "Compare with…" no header; passar `Compare` ao `SkillCanonicalList`; badges A vs B com delta; hideout/outfit em 2 colunas no modo compare; ler/escrever `?compare=`; resolver/limpar B | header `:39-63`, badges `:94-129`, `<SkillCanonicalList>` `:146-149`, hideout `:170-185`, outfit `:187-213`, `@code` `:264-465` (campos `:268-273`, `Reload()` `:278-326`, `FormatRub` `:413`, `StatusChip` `:416`, `SkillTotalChip` `:436`) |
| `modded/Server/Web/Shared/SkillCanonicalList.razor` | **NÃO MODIFICAR (v1)** — `Compare` + coluna de delta + transbordo de B JÁ existem (`:104-106` param, `:144-156` `BuildCompareLookup`, `:179-188` overflow de B, `:366-370` delta cell, `:395-410` `DeltaCell`, `:414` `ColumnCount`). Eventual chip de multiplicador-de-B é aditivo e fica para o code-mod **só se** couber sem reescrita; default v1 = não tocar (ver "Multiplicadores lado a lado"). |
| `modded/Server/Web/wwwroot/css/customclasses.css` | **MODIFICAR (ADITIVO)** — bloco `/* 036 — compare A×B */`: estilos do badge duplo A vs B e do delta de resumo. NÃO reescrever nada do 033/034. | apêndice ao fim do arquivo (último bloco hoje: `.cc-item-tip*`) |

Não tocados (consumidos como estão): `CostService` (`ComputeSkillCost`/`ComputeLoadoutCost`), `ClassEditorService.ListClassFiles()`, `CatalogService`, `ClassRegistrar`, `SkillWeights.BudgetMin/Max`, `BaseLayout.razor`, `NavMenu.razor` (030 — ver PA-036-01), `GearPanel`/`StashPanel` (034).

## Contratos reais reusados

### `SkillCanonicalList` — parâmetro `Compare` (031, já implementado)

```csharp
// SkillCanonicalList.razor:104-106
[Parameter] public ClassDefinition? Compare { get; set; }
```

Quando `Compare != null`, o componente:
- monta `BuildCompareLookup()` case-insensitive de `Compare.Skills` (`:144-156`);
- adiciona a coluna de delta por linha: `bLevel - aLevel` via `DeltaCell(level, bLevel)` (`:366-370`, `:395-410`) — ▲ verde (`Color.Success`, delta>0 ⇒ B>A) / ▼ vermelho (`Color.Error`, delta<0 ⇒ B<A) / `=` (`Color.Default`);
- inclui skills que só B tem na seção "Outside canonical" (`BuildOverflowEntries` `:179-188`);
- ajusta `ColumnCount` (`:414`) para o colspan dos separadores.

> **Nota de semântica de cor (registrada):** o `DeltaCell` do 031 pinta **delta = B−A**, logo **B>A** fica verde e **B<A** vermelho. A spec funcional (01) descreve a vantagem **do ponto de vista de A** ("▲ verde quando A>B"). Como o componente NÃO será reescrito (PA-036-03), a v1 **adota a convenção do componente** (verde = B tem mais que A) e **rotula o cabeçalho da coluna** de forma inequívoca ("Δ B−A") no header do dashboard / via legenda no header dos badges, para não inverter o sinal mentalmente. Resolve o achado 🔴-R1 (ver 03). Inverter o sinal exigiria editar o componente (territorialmente possível, mas é reescrita do contrato 031→036) — fora da v1.

### `CostService` (item 022) — totais para os deltas de resumo

```csharp
public SkillCostBreakdown ComputeSkillCost(ClassDefinition def);   // .Total (double), .WithinBudget, .Skills, .Warnings
public LoadoutCostBreakdown ComputeLoadoutCost(ClassDefinition def);// .TotalRub (double), .Items, .Warnings
```

Já chamados para A em `Reload()` (`:304-305`). Para B serão chamados **uma vez** ao resolver B (sem por-render).

### `ClassEditorService.ListClassFiles()` + `ClassFileEntry`

```csharp
public List<ClassFileEntry> ListClassFiles();                 // cacheado por file-stamp (perf)
public sealed record ClassFileEntry(
    string FileName, ClassDefinition? Definition, bool Enabled, bool Registered, List<ClassDiagnostic> Diagnostics);
```

Mesma fonte que A usa (`:294`). O picker e a resolução de B reusam a **mesma** lista já carregada em `Reload()`.

### `ClassDefinition` (campos usados)

```csharp
public string? Name; public string? NameColor; public string? IconFile;
public Dictionary<string,int>? Skills; public Dictionary<string,double>? SkillMultipliers;
public Dictionary<string,int>? Hideout; public Outfit? Outfit;
```

## `ClassDetail.razor` — mudanças

### 1. Estado novo (`@code`, junto de `:268-273`)

```csharp
// ── Compare mode (036) — B is read-only & ephemeral (URL only; no persistence — PA-036-02) ──
/// <summary>Deep-link: ?compare=<fileName-sem-extensão>. Null/empty/igual a A/inválida ⇒ single.</summary>
[Parameter, SupplyParameterFromQuery(Name = "compare")] public string? CompareParam { get; set; }

private ClassFileEntry?      _compareEntry;     // B resolvida (ou null = single)
private ClassDefinition?     _compareDef;       // _compareEntry?.Definition — passado ao SkillCanonicalList
private SkillCostBreakdown?  _compareSkillCost; // ComputeSkillCost(B) — uma vez
private LoadoutCostBreakdown? _compareLoadoutCost; // ComputeLoadoutCost(B)
private List<ClassFileEntry> _compareCandidates = []; // classes válidas ≠ A para o picker

private bool IsComparing => _compareDef is not null;
```

> **`[SupplyParameterFromQuery]`** (net9 Blazor interactive server — host confirmado `net9.0` em 033/BaseLayout) lê a query sem parse manual de URI. Combina com o `[Parameter] FileName` da rota; ambos chegam em `OnParametersSet` → `Reload()` já é o único ponto de entrada (`:275`). Resolve a leitura do deep-link sem novo `NavigationManager` boilerplate.

### 2. `ResolveCompare()` — chamado no fim de `Reload()` (após A pronta)

`Reload()` (`:278-326`) hoje termina populando `_clothingNames`. Acrescentar, no fim (e zerar o estado de compare no topo junto dos outros resets `:280-285`):

```csharp
private void ResolveCompare()
{
    _compareEntry = null; _compareDef = null;
    _compareSkillCost = null; _compareLoadoutCost = null;

    // Candidatos: classes parseáveis e ≠ A (PA-036-05). Reusa a lista já carregada por A em Reload().
    var all = EditorService.ListClassFiles();
    var aBare = Path.GetFileNameWithoutExtension(_entry!.FileName);
    _compareCandidates = all
        .Where(e => e.Definition is not null
                 && !string.Equals(Path.GetFileNameWithoutExtension(e.FileName), aBare, StringComparison.OrdinalIgnoreCase))
        .ToList();

    if (string.IsNullOrWhiteSpace(CompareParam)) return;             // single
    var b = _compareCandidates.FirstOrDefault(e =>
        string.Equals(e.FileName, CompareParam, StringComparison.OrdinalIgnoreCase)
     || string.Equals(Path.GetFileNameWithoutExtension(e.FileName), CompareParam, StringComparison.OrdinalIgnoreCase));
    if (b?.Definition is null) return;                               // inexistente/inválida/=A ⇒ ignora (corner 1,2)

    _compareEntry = b;
    _compareDef = b.Definition;
    _compareSkillCost = CostService.ComputeSkillCost(b.Definition);
    _compareLoadoutCost = CostService.ComputeLoadoutCost(b.Definition);
}
```

Guard: `ResolveCompare()` só roda quando `_entry?.Definition is not null` (já dentro do branch que computou `_skillCost`); se A não parseia, não há comparação (chama-se logo após `:308` ou no fim do método, ainda dentro do `if (_entry?.Definition is not { } def) return;` superado).

### 3. Picker no header (`:39-63`, depois do `<MudSpacer/>`, antes/junto dos botões)

```razor
@if (_entry?.Definition is not null)
{
    <MudMenu Label="@(IsComparing ? $"Comparing: {_compareDef!.Name}" : "Compare with…")"
             StartIcon="@Icons.Material.Filled.Compare" Variant="Variant.Outlined" Size="Size.Small"
             title="Compare this class with another (read-only)">
        @foreach (var c in _compareCandidates)
        {
            <MudMenuItem OnClick="@(() => SetCompare(c.FileName))">
                @if (c.Definition!.IconFile is { Length: > 0 } ic)
                {
                    <img src="@($"/CustomClasses-Server/icons/{ic}")" alt="" width="18" height="18"
                         style="object-fit:contain;margin-right:6px;vertical-align:middle;"/>
                }
                <span style="@(string.IsNullOrWhiteSpace(c.Definition.NameColor) ? null : $"color:{c.Definition.NameColor};")">
                    @(c.Definition.Name ?? Path.GetFileNameWithoutExtension(c.FileName))
                </span>
            </MudMenuItem>
        }
    </MudMenu>
    @if (IsComparing)
    {
        <MudIconButton Icon="@Icons.Material.Filled.Close" Size="Size.Small"
                       OnClick="ClearCompare" title="Clear comparison"/>
    }
}
```

Navegação via query (escrita) — reusa o `Nav` já injetado (`:18`):

```csharp
private void SetCompare(string fileName)
{
    var bare = Path.GetFileNameWithoutExtension(fileName);
    Nav.NavigateTo(Nav.GetUriWithQueryParameter("compare", bare));   // dispara OnParametersSet → Reload → ResolveCompare
}

private void ClearCompare() =>
    Nav.NavigateTo(Nav.GetUriWithQueryParameter("compare", (string?)null)); // remove a query → volta a single
```

> `GetUriWithQueryParameter` é extensão de `NavigationManager` (`Microsoft.AspNetCore.Components`, net9) — não exige `WebUtilities`. Passar `null` remove o parâmetro. Não há recarga de página (interactive server faz re-render); o estado de antes é exatamente `IsComparing == false`.

### 4. Badges A vs B (`:94-129`)

Os três primeiros badges (Skill cost `:96-101`, Loadout ₽ `:102-105`, e um novo "Skills #") passam a renderizar via um helper `CompareBadge` quando `IsComparing`; sem comparação, ficam **idênticos** ao 033 (sem delta). Padrão:

```razor
<div class="cc-badge">
    <span class="cc-badge__label">Skill cost</span>
    <span class="cc-badge__value">
        @(_skillCost?.Total.ToString("0.00", CultureInfo.InvariantCulture) ?? "—") @SkillTotalChip()
        @if (IsComparing)
        {
            <span class="cc-cmp-b">vs @(_compareSkillCost?.Total.ToString("0.00", CultureInfo.InvariantCulture) ?? "—")</span>
            @DeltaChip(_skillCost?.Total, _compareSkillCost?.Total, "0.00", higherIsA: true)
        }
    </span>
</div>
```

- **Loadout ₽:** A `FormatRub(_loadoutCost.TotalRub)` (`:104`, `FormatRub :413`) vs B `FormatRub(_compareLoadoutCost.TotalRub)`; `DeltaChip` com formato ₽.
- **Skills #** (badge novo): `CountSkills(def.Skills)` vs `CountSkills(_compareDef.Skills)` — contagem de níveis `> 0`. Helper:

```csharp
private static int CountSkills(IReadOnlyDictionary<string,int>? skills) =>
    skills?.Count(kv => kv.Value > 0) ?? 0;
```

`DeltaChip` — sinal/cor do ponto de vista de **A** (vantagem de A = verde), independente da convenção interna da coluna de skills (que é B−A — ver nota 🔴-R1):

```csharp
/// <summary>Chip Δ = (A − B) do ponto de vista de A: A maior ⇒ verde, A menor ⇒ vermelho, igual ⇒ neutro.
/// higherIsA controla a polaridade de cor (true: A maior é "bom"/verde — vale p/ skill cost? ver nota).</summary>
private RenderFragment DeltaChip(double? a, double? b, string fmt, bool higherIsA) => __builder =>
{
    if (a is not { } av || b is not { } bv) return;
    var d = av - bv;
    if (Math.Abs(d) < 0.005) { <MudChip T="string" Size="Size.Small" Color="Color.Default">=</MudChip>; return; }
    var aIsHigher = d > 0;
    var good = higherIsA ? aIsHigher : !aIsHigher;
    var color = good ? Color.Success : Color.Error;
    var arrow = aIsHigher ? "▲" : "▼";
    <MudChip T="string" Size="Size.Small" Color="@color">
        @arrow @((d > 0 ? "+" : "") + d.ToString(fmt, CultureInfo.InvariantCulture))
    </MudChip>
};
```

> **`higherIsA` — polaridade por métrica (decisão):** "vantagem" não é universal. Para **nº de skills** mais é "mais forte" ⇒ `higherIsA: true` (verde). Para **skill cost ponderado** e **loadout ₽**, "maior" não é claramente bom (custo maior pode estourar budget; ₽ maior = loadout mais caro). **Decisão v1:** as três usam `higherIsA: true` (verde = A tem o número maior) e o significado fica por conta do label + do chip de budget que já existe (`SkillTotalChip`). É **só leitura comparativa**, não um veredito de balanceamento; manter neutro e consistente evita inventar regra de "bom/ruim" não pedida. Registrado p/ revisão no 035 se o usuário quiser semântica de budget.

### 5. Hideout / outfit em 2 colunas (modo compare) — `:170-213`

Em `IsComparing`, os blocos Hideout (`:170-185`) e Outfit (`:187-213`) renderizam A e B lado a lado (2 sub-colunas textuais compactas, classe `.cc-cmp-2col`). Reusam o mesmo markup de chips de hideout / `MudSimpleTable` de outfit que já existe, duplicado para `_compareDef`. Sem comparação, ficam idênticos ao 033. `ClothingLabel` (`:403`) usa `_clothingNames` populado para A; para B os ids de roupa caem no fallback "id cru" (PA: nomes de roupa de B não precisam de catálogo dedicado — comparação textual simples, fora de escopo aprofundar; corner aceitável).

### 6. `SkillCanonicalList` passa `Compare`

```razor
<SkillCanonicalList Levels="@def.Skills" Cost="@_skillCost"
                    Multipliers="@def.SkillMultipliers"
                    Compare="@_compareDef"        @* 036 — null fora do modo compare *@
                    Editable="false"/>
```

`_compareDef == null` ⇒ comportamento idêntico ao 033 (sem coluna de delta). Sem mudança no componente.

## Multiplicadores lado a lado (resolução)

A spec funcional pede "chips ±% das duas classes lado a lado na mesma linha". O `SkillCanonicalList` (031) já renderiza o chip ±% **de A** por linha (`MultiplierChip`, `:374-393`), mas **não** tem coluna de multiplicador de B. Duas opções:

- **(A) v1 — sem tocar o componente (DEFAULT, escolhida):** os multiplicadores de B **não** entram na linha de skill; a comparação de multiplicadores é entregue pela coluna de **delta de nível** (o foco do kickoff é "diferença de skills") + os deltas de resumo. Cumpre o DoD essencial ("comparar skill a skill em 2 cliques") sem reescrever o contrato 031. A frase "multiplicadores lado a lado" é satisfeita parcialmente; registrar como **limitação v1** e item de follow-up.
- **(B) follow-up — coluna de multiplicador de B no componente:** adicionar `[Parameter] CompareMultipliers` + uma célula `MultiplierChip(bFactor)` na `BuildRow`, condicional a `Compare != null`. É **aditivo** (não reescrita) mas mexe no componente do 031 — território compartilhado, maior superfície de regressão. **Fora da v1**; proposto explicitamente no 03 como decisão a ratificar.

> **Decisão registrada (🔴-R2):** v1 = opção (A). O kickoff lista multiplicadores lado a lado no escopo, mas o item "nasceu" com o delta de **nível** como entregável central (kickoff §Escopo, DoD §"deltas coloridos"). Entregar (A) e abrir follow-up evita reabrir o contrato 031 nesta wave. Premissa autônoma — sem aprovação disponível.

## `customclasses.css` — apêndice ADITIVO

Ao fim do arquivo (após `.cc-item-tip*`), bloco novo — não reescreve nada:

```css
/* ── 036 — comparação A×B (header badges + 2 colunas hideout/outfit) ─────────── */
.cc-cmp-b      { margin-left: 6px; opacity: .75; font-weight: 600; font-variant-numeric: tabular-nums; }
.cc-cmp-2col   { display: flex; gap: 16px; align-items: flex-start; }
.cc-cmp-2col > * { flex: 1; min-width: 0; }
.cc-cmp-col__head { font-size: 10px; text-transform: uppercase; letter-spacing: .5px; opacity: .6; margin-bottom: 2px; }
```

A cor/ícone do delta de skill (▲/▼) continua vindo do `MudChip Color` (Success/Error) e do `DeltaCell` do 031 — o CSS **não** redefine cor de delta (evita divergir do componente). Resolve a divergência de fonte-de-cor (🔴-R3).

## Sequência (resolver B no deep-link)

```
URL /classes/cacador?compare=batedor
  → OnParametersSet → Reload()
      → resolve A (ListClassFiles + Compute*)            (existente :294-325)
      → ResolveCompare()                                  (novo)
          → candidatos = parseáveis ≠ A
          → match CompareParam → B (FileName ou bare)
          → ComputeSkillCost(B) / ComputeLoadoutCost(B)   (1×, não por-render)
  → render: picker mostra "Comparing: <B.Name>"; SkillCanonicalList recebe Compare=B;
            badges mostram A vs B + DeltaChip; hideout/outfit em 2 colunas
SetCompare(x) / ClearCompare → Nav.GetUriWithQueryParameter("compare", …) → re-render (sem reload)
```

## Perf

- `ListClassFiles()` é cacheado por file-stamp (`ClassEditorService :80-90`); a 2ª chamada em `ResolveCompare()` é hot (já carregada por A no mesmo `Reload()`). **Otimização do code-mod:** reutilizar a `List` local de A em vez de chamar `ListClassFiles()` de novo (passar a lista para `ResolveCompare` ou guardá-la num campo do `Reload`). Não-bloqueante (cache hot), mas trivial.
- `ComputeSkillCost`/`ComputeLoadoutCost` de B rodam **uma vez** por resolução (em `ResolveCompare`, não no render). Mesmo custo de uma classe A. OK.

## Decisões de UI

- Texto da UI em inglês; docs pt-BR (consistente com 024/031/033).
- A coluna direita do 034 (gear/stash visual) **não** entra no modo compare (fora de escopo 01). Só header + coluna esquerda + hideout/outfit.
- Sem componente Mud novo além de `MudMenu`/`MudMenuItem`/`MudIconButton` (já usados no projeto).
- Cores de categoria/delta de skill seguem o `SkillCanonicalList`/`SkillMaster` (031); o `customclasses.css` não as redefine.
