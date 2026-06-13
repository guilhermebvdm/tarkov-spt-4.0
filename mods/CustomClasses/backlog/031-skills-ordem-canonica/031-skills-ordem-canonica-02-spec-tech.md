# 031 — Skills em ordem canônica (componente) — Spec técnica

**Mod:** CustomClasses
**Criado:** 2026-06-12
**Refs:** [01-spec](./031-skills-ordem-canonica-01-spec.md) · [00-kickoff](./031-skills-ordem-canonica-00-kickoff.md)

## Arquivos tocados

| Arquivo | Ação |
|---|---|
| `modded/Server/SkillMaster.cs` | CRIADO — ordem canônica derivada de `SkillWeights`, sem números mágicos |
| `modded/Server/Web/Shared/SkillCanonicalList.razor` | CRIADO — componente 3-modos (read-only / edit / compare) |
| `modded/Server/Web/Pages/ClassDetail.razor` | EDITADO — painel "Skills" passa a usar o componente (read-only) |
| `modded/Server/Web/Pages/ClassEdit.razor` | EDITADO — aba "Skills" passa a usar o componente (edit inline); remove o "Add skill" |

Não tocados: `SkillWeights.cs` (só consumido), `CostService.cs` (só consumido), `ClassEditModel.cs` (`SkillLevelRow` reusado como está), aba de multiplicadores do `ClassEdit.razor` (premissa P2).

## `SkillMaster.cs` — ordem canônica derivada

Fonte da verdade: `SkillWeights.Categories` (`SkillWeights.cs:122-148`) e `SkillsExtendedCompat.Skills` (`SkillsExtendedCompat.cs:15-16`). **Nenhuma contagem é escrita** — a lista é montada por código a partir desses dois conjuntos.

Regras de construção:

1. **Seção Special Elite primeiro identificada:** o conjunto Special Elite = nomes de `SkillsExtendedCompat.Skills` (`FirstAid`, `FieldMedicine`, `BearRawpower`, `UsecNegotiations`). Essas 4 vão **sempre** para a seção final, **independentemente** da categoria de custo (premissa P4 — `SkillWeights.Categories` classifica FirstAid/FieldMedicine como `"P"` em `:142` e as duas faction skills como `"S"` em `:147`; aqui isso não importa, o pertencimento à seção é decidido pelo set do compat).
2. **As 4 categorias principais (Ph→M→C→P):** para cada categoria nesta ordem fixa, percorrer **`SkillWeights.Explicit`** (`:49-87`) na **ordem de declaração do dicionário** (que já está em Ph→M→C→P e casa 1:1 com `SKILL_MASTER` do viewer — `profiles.js:12-45`), filtrando por categoria e excluindo o set Special Elite. Usar `Explicit` (não `Categories`) como fonte da ordem porque `Categories` inclui skills "mortas" (SMG/LMG/Sniping/etc.) que não devem aparecer; `Explicit` são exatamente as 31 skills vivas portadas do RZ.
3. **Special Elite por último:** as 4 skills do set, em ordem fixa declarada no próprio `SkillMaster` (FirstAid, FieldMedicine, UsecNegotiations, BearRawpower — espelha o "Skills-Extended" do kickoff).

> Premissa técnica P5: a ordem das categorias principais vem da ordem de inserção do `Dictionary<SkillTypes,double> Explicit`. `Dictionary` em .NET preserva ordem de inserção na enumeração na prática, mas **não é contrato**. Para não depender disso, `SkillMaster` define a ordem das categorias explicitamente (array `["Ph","M","C","P"]`) e, dentro de cada categoria, uma **lista ordenada explícita de `SkillTypes`** copiada da ordem do `Explicit`/`SKILL_MASTER` — derivada do mesmo conteúdo, mas estável. A contagem total continua sem hardcode (é `entries.Count`).

### API

```csharp
namespace CustomClasses;

public enum SkillCategory { Physical, Mental, Combat, Practical, SpecialElite }

/// <summary>One skill in canonical position. Color/label derive from Category.</summary>
public sealed record SkillMasterEntry(SkillTypes Skill, string Name, SkillCategory Category);

public static class SkillMaster
{
    /// <summary>All canonical skills in fixed order (Ph→M→C→P, then Special Elite).
    /// Count/list are derived from SkillWeights + SkillsExtendedCompat — no magic numbers.</summary>
    public static IReadOnlyList<SkillMasterEntry> Entries { get; }   // built once in a static ctor

    /// <summary>Hex color per category (port: Ph #c87c50 / M #7090c8 / C accent / P #6e9a3f / SpecialElite reuses a neutral accent).</summary>
    public static string ColorOf(SkillCategory category);

    /// <summary>Section label ("Physical (Ph)", "Mental (M)", "Combat (C)", "Practical (P)", "Special Elite").</summary>
    public static string LabelOf(SkillCategory category);
}
```

Cores portadas de `profiles.css:170-205`: Ph `#c87c50`, M `#7090c8`, C accent (usar a cor de acento do tema MudBlazor; fallback `#c8a35a` como no resto do mod), P `#6e9a3f`. Special Elite: tom neutro distinto (premissa P6 — `#9a6ec8`, roxo, fora das 4 cores existentes; cosmético, ajustável).

`Name` = `Skill.ToString()` (mesma string que o serviço de custo usa como chave — `CostService.cs:114,158-159`).

## `SkillCanonicalList.razor` — componente 3-modos (CONTRATO 031→033/036)

Documentar os 3 modos num comentário no topo do componente. Parâmetros projetados para suportar os três sem reescrita:

```csharp
@code {
    /// <summary>Modo edição: campo numérico inline por linha (031/ClassEdit). false = read-only (033/ClassDetail).</summary>
    [Parameter] public bool Editable { get; set; }

    /// <summary>Níveis atuais por nome de skill (chave = SkillTypes.ToString()). A classe A.
    /// Em edição, a fonte é a lista mutável de SkillLevelRow (ver binding abaixo) — este dict é o snapshot read-only.</summary>
    [Parameter] public IReadOnlyDictionary<string, int>? Levels { get; set; }

    /// <summary>Edição: linhas mutáveis (ClassEdit liga direto). Quando setado, tem precedência sobre Levels e habilita o campo inline.</summary>
    [Parameter] public IList<SkillLevelRow>? EditRows { get; set; }

    /// <summary>Custo por skill já calculado (022). Chave = nome. Null = não exibe custo/peso inline.</summary>
    [Parameter] public SkillCostBreakdown? Cost { get; set; }

    /// <summary>Multiplicadores de XP por skill (nome → fator). Gera o chip ±% (fator 1 = sem chip).</summary>
    [Parameter] public IReadOnlyDictionary<string, double>? Multipliers { get; set; }

    /// <summary>MODO COMPARAÇÃO (036): classe B opcional. Quando != null, cada linha ganha uma coluna
    /// de delta ▲/▼ (B.level − A.level) por skill. Read-only por natureza.</summary>
    [Parameter] public ClassDefinition? Compare { get; set; }

    /// <summary>Disparado quando um nível muda em modo edição (ClassEdit recomputa o custo). </summary>
    [Parameter] public EventCallback OnLevelChanged { get; set; }
}
```

### Os 3 modos

| Modo | Como ativar | Render |
|---|---|---|
| **read-only** (033 dashboard, ClassDetail) | `Editable=false`, `Levels` setado | número/"—" por linha, barra, custo inline, chip ±% |
| **edit inline** (031, ClassEdit) | `Editable=true`, `EditRows` setado | `MudNumericField` por linha ligado a `SkillLevelRow.Level`; `@bind-Value:after` → `OnLevelChanged` |
| **compare A×B** (036) | `Compare` setado (sobre read-only) | coluna extra: delta `B − A` por skill (ambos via lookup case-insensitive seguro — B2), ícone ▲ verde / ▼ vermelho / "=" neutro |

### Resolução de nível por linha (precedência)

Construir **uma vez por render** um lookup case-insensitive (B2): `var levels = new Dictionary<string,int>(StringComparer.OrdinalIgnoreCase)` populado de `EditRows` (chave = `row.Skill`) ou, na ausência, de `Levels`. Toda leitura de nível usa `levels.TryGetValue(entry.Name, out var lvl) ? lvl : 0` — nunca indexação direta (evita `KeyNotFoundException` e casa `endurance` com `Endurance`).

Em **modo edição** o `MudNumericField` de cada linha NÃO liga direto a `SkillLevelRow.Level` (B4). Liga a uma **propriedade/handler computado por linha**:

- **get** → nível resolvido pelo lookup acima (0 quando não há row).
- **set(value)** → achar o row por `Skill` (OrdinalIgnoreCase) em `EditRows`:
  - row existe → `row.Level = value`;
  - row não existe **e** `value > 0` → criar `SkillLevelRow { Skill = entry.Name, Level = value }` e adicionar a `EditRows`;
  - row não existe **e** `value == 0` → **não criar** (round-trip: não nasce zero novo — critério de aceite 5);
  - depois: `await OnLevelChanged.InvokeAsync()` e `StateHasChanged` do componente (a linha permanece na posição canônica de `SkillMaster.Entries`, independente da ordem de `EditRows` — B4).

Em **read-only**, o nível vem só do lookup; sem campo editável.

> Premissa técnica P7 (round-trip de zeros): zeros pré-existentes ficam preservados **de graça** porque `ClassEditModel.FromDefinition` (`ClassEditModel.cs:205-208`) já materializa **todo** par `skill→level` do arquivo em `SkillLevelRow`, inclusive os 0. O componente apenas evita **criar** rows novos com 0. Em `ToDefinition` (`ClassEditModel.cs:237`) o dict é montado de TODOS os rows existentes — logo um 0 pré-existente sobrevive e um 0 nunca-criado nunca aparece. Nenhuma mudança em `ClassEditModel` é necessária. (Confirmado: hoje o "Add skill" cria row com `Level=0` em `ClassEdit.razor:779` — esse caminho é justamente o que sai.)

### Linhas fora da tabela canônica + adicionar skill fora da master (corner case 3 / B3)

Skills presentes em `Levels`/`EditRows` cujo nome **não** está em `SkillMaster.Entries` (nome desconhecido, enum "morto" tipo SMG/Sniping) não têm posição canônica. Render numa seção de transbordo "Outside canonical (loader ignores / unmapped)" após Special Elite, esmaecida, espelhando o warning que `CostService.ComputeSkillCost` (`CostService.cs:138-143`) já emite. Não são silenciosamente descartadas.

**B3 — paridade de edição:** removendo o "Add skill", o autor perderia o único caminho da UI para adicionar/editar skills do enum que estão **fora** da master (ex.: SMG). Portanto, quando `Editable=true`:
- as linhas da seção de transbordo também têm `MudNumericField` inline (mesmo handler computado acima);
- um affordance mínimo "Add skill outside canonical" (dropdown reduzido) reaparece **apenas** com os nomes de `Enum.GetNames<SkillTypes>()` que **não** estão em `SkillMaster.Entries` nem já em `EditRows` (reusa `AvailableSkills`, `ClassEdit.razor:759`, intersectado com "não-canônicas"). Adicionar cria um `SkillLevelRow` que cai na seção de transbordo.

Em read-only a seção de transbordo é apenas exibida (sem campo/add). Assim a edição via componente cobre **tudo** que o "Add skill" cobria hoje, sem regressão.

### Barra de progresso

`pct = level <= 0 ? 0 : Math.Min(1.0, level / 10.0) * 100` (porta `profiles.js:154` com saturação para o corner case 2). Cor = `SkillMaster.ColorOf(entry.Category)`. Linha esmaecida (`opacity:.3`, porta `.skill-row--zero` `profiles.css:182`) quando `level <= 0`.

### Chip de multiplicador

`Multipliers.TryGetValue(name, out var f)` (lookup case-insensitive — B2; nunca indexar direto); ausente ou `f == 1` → sem chip; senão chip `+{(f-1)*100:0}%` verde se `f>1`, `−{(1-f)*100:0}%` vermelho se `f<1` (espelha `FactorColor` de `ClassDetail.razor:515`).

## `ClassDetail.razor` — adoção read-only

Substituir o bloco `<MudTable>` do painel "Skills" (`ClassDetail.razor:151-184`) por:

```razor
<SkillCanonicalList Levels="@def.Skills"
                    Cost="@_skillCost"
                    Multipliers="@def.SkillMultipliers"
                    Editable="false" />
```

Manter abaixo o total ponderado + chip de budget + warnings (`:174-182`) como estão (já consomem `_skillCost`). O painel "XP multipliers" (`:187-221`) permanece (P2).

## `ClassEdit.razor` — adoção edit inline

Na aba "Skills" (`ClassEdit.razor:213-282`):

- **Remover** a `<MudSimpleTable>` de skills (`:220-254`) e o bloco "Add skill" (`:256-270`).
- **Inserir** o componente em modo edit:

```razor
<SkillCanonicalList Editable="true"
                    EditRows="@_model.Skills"
                    Cost="@_skillCost"
                    Multipliers="@MultiplierLookup()"
                    OnLevelChanged="RecomputeSkillCost" />
```

`MultiplierLookup()` (B1 — `ToDictionary` lança em chave duplicada; um arquivo à mão pode ter fatores repetidos): agregar tolerante, primeira ocorrência vence, espelhando `ClassEditModel.ToDict` (`ClassEditModel.cs:297-311`):

```csharp
private Dictionary<string, double> MultiplierLookup()
{
    var d = new Dictionary<string, double>(StringComparer.OrdinalIgnoreCase);
    foreach (var m in _model.Multipliers) d.TryAdd(m.Skill, m.Factor);
    return d;
}
```

- Manter o rodapé total + chip + warnings (`:272-281`) como está.
- Remover o helper `AddSkill`/`RemoveSkill` e o campo `_newSkill` **só se** não houver outro uso (verificar; `RemoveSkill` deixa de ser chamado, `_newSkill` idem). `AvailableSkills` continua usado pela aba de multiplicadores (`:349`) — **manter**.

> O `MudNumericField` por linha replica os limites de hoje (`Min=0 Max=51`, `ClassEdit.razor:239`) e dispara `OnLevelChanged` via `@bind-Value:after` (mesmo padrão de `:238`).

## Decisões de UI

- Texto da UI em inglês; docs pt-BR (consistente com 024).
- MudBlazor 8.13.0 (transitivo). Linha = `MudSimpleTable` ou grid CSS (3 col: nome / barra / nível, igual `.skill-row` de `profiles.css:175-181`); separador de categoria = linha com label colorido (porta `.skill-cat-sep`).
- Componente sem estado próprio de nível — fonte única é o parâmetro (`Levels` ou `EditRows`); em edição liga direto na lista mutável do `ClassEditModel` (mesmo padrão dos outros editores inline, ex. hideout em `ClassEdit.razor:386`).
- Formatação de custo: `0.00` invariant (igual `ClassDetail.razor:169-171`).

## Contrato de API (resumo para 033/036)

`SkillCanonicalList` nasce com `Editable`, `Levels`, `EditRows`, `Cost`, `Multipliers`, `Compare`, `OnLevelChanged`. 033 instancia com `Editable=false` + `Levels`/`Cost` (read-only, sem novos params). 036 instancia read-only + `Compare=<classe B>` para a coluna de delta. Nenhum dos dois precisa de novo parâmetro além dos já definidos aqui.
</content>
