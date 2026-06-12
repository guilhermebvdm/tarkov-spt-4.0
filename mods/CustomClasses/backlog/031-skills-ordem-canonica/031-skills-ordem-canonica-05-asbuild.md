# 031 — Skills em ordem canônica (componente) — As-built

**Mod:** CustomClasses
**Data:** 2026-06-12
**Refs:** [01-spec](./031-skills-ordem-canonica-01-spec.md) · [02-spec-tech](./031-skills-ordem-canonica-02-spec-tech.md) · [00-kickoff](./031-skills-ordem-canonica-00-kickoff.md)

## Arquivos entregues

| Arquivo | Conteúdo |
|---|---|
| `modded/Server/SkillMaster.cs` | NOVO — `SkillCategory` (Physical/Mental/Combat/Practical/SpecialElite), `SkillMasterEntry(Skill, Name, Category)`, `SkillMaster` estático. `Entries` é DERIVADO em runtime (sem números mágicos): para cada categoria em ordem fixa `[Ph, M, C, P]`, percorre `SkillWeights.Explicit.Keys` (ordem de declaração = ordem do viewer), filtra por `SkillWeights.Categories`, exclui o set Special Elite; depois anexa as 4 skills SE (`SkillsExtendedCompat.Skills`) na ordem fixa FirstAid → FieldMedicine → UsecNegotiations → BearRawpower. Contagem = `Entries.Count`. `ColorOf` (Ph `#c87c50`, M `#7090c8`, C `#c8a35a` accent, P `#6e9a3f`, SpecialElite `#9a6ec8`) e `LabelOf` por categoria. |
| `modded/Server/Web/Shared/SkillCanonicalList.razor` | NOVO — componente 3-modos (read-only / edit inline / compare). `[Parameter]`s: `bool Editable`, `IReadOnlyDictionary<string,int>? Levels`, `IList<SkillLevelRow>? EditRows`, `SkillCostBreakdown? Cost`, `IReadOnlyDictionary<string,double>? Multipliers`, `ClassDefinition? Compare`, `EventCallback OnLevelChanged`. Todas as skills sempre na posição canônica de `SkillMaster.Entries`, nível 0 esmaecido (`opacity:.3`), separador colorido por categoria, barra de progresso `min(1, level/10)*100` na cor da categoria, custo inline (de `Cost`), chip ±% de multiplicador. Lookup case-insensitive único por render (EditRows tem precedência sobre Levels). Edição: `MudNumericField` (Min 0 Max 51) com `ValueChanged` → handler computado `SetLevelAsync` (existe→atribui; ausente e >0→cria row; ausente e ==0→NÃO cria) → `OnLevelChanged`. Seção de transbordo "Outside canonical" para skills fora da master (editável + affordance "Add skill outside canonical" só em modo edit, reusando nomes não-canônicos do enum). Coluna de delta ▲/▼/= quando `Compare` setado. |
| `modded/Server/Web/Pages/ClassDetail.razor` | EDITADO — painel "Skills": `<MudTable>` substituída por `<SkillCanonicalList Levels Cost Multipliers Editable="false"/>`. Total ponderado + chip de budget + warnings mantidos abaixo (agora sob `@if (_skillCost is not null && _skillCost.Skills.Count > 0)`). Painel "XP multipliers" intacto (P2). |
| `modded/Server/Web/Pages/ClassEdit.razor` | EDITADO — aba "Skills": `<MudSimpleTable>` + bloco "Add skill" removidos; `<SkillCanonicalList Editable="true" EditRows="@_model.Skills" Cost Multipliers="@MultiplierLookup()" OnLevelChanged="RecomputeSkillCost"/>`. Rodapé total/chip/warnings mantido. Removidos os membros mortos `_newSkill`, `AddSkill`, `RemoveSkill`, `SkillCostEntryFor`; adicionado `MultiplierLookup()` (agregação tolerante, primeira ocorrência vence — B1). `AvailableSkills` mantido (ainda usado pela aba Multipliers). |

Não tocados: `SkillWeights.cs`, `CostService.cs`, `ClassEditModel.cs` (`SkillLevelRow` reusado como está — round-trip de zeros vem de graça, P7), aba de multiplicadores do `ClassEdit.razor` (P2).

## Contrato 031→033/036 (entregue)

`SkillCanonicalList` nasce com os 7 parâmetros previstos. **033** instancia `Editable=false` + `Levels`/`Cost` (sem novos params). **036** instancia read-only + `Compare=<classe B>` para a coluna de delta. Os 3 modos estão documentados no comentário do topo do `.razor`.

## Decisões e premissas

- **P5 (ordem de categorias estável):** a ordem das 4 categorias é um array explícito `[Ph, M, C, P]` em `SkillMaster`; dentro de cada uma, a ordem vem da iteração de `SkillWeights.Explicit.Keys`. Em .NET o `Dictionary` preserva ordem de inserção na enumeração na prática; como `Explicit` é montado uma única vez e em ordem Ph→M→C→P (1:1 com o viewer), isso é estável o suficiente. A contagem total continua derivada (`Entries.Count`), sem hardcode.
- **P4 (Special Elite por set, não por categoria):** o pertencimento à seção final é decidido por `SkillsExtendedCompat.Skills`, independente de `SkillWeights.Categories` (que bucketa FirstAid/FieldMedicine em "P" e as faction skills em "S"). As 4 são excluídas das categorias principais e re-anexadas no fim.
- **P6 (cor Special Elite):** `#9a6ec8` (roxo), fora das 4 cores existentes — cosmético, ajustável.
- **P7 (round-trip de zeros):** preservado de graça — `ClassEditModel.FromDefinition` já materializa todos os pares `skill→level` (inclusive 0) em `SkillLevelRow`; o componente só evita CRIAR rows com 0. `ToDefinition` reconstrói de todos os rows existentes. Nenhuma mudança em `ClassEditModel`.
- **B3 (paridade de edição fora da master):** ao remover o "Add skill", o único caminho para skills fora da canônica (ex.: SMG/Sniping) seria perdido. Reintroduzido como affordance "Add skill outside canonical" que aparece só em modo edit, oferecendo nomes do enum `SkillTypes` que não estão na master nem já em `EditRows`. A skill adicionada cai na seção de transbordo.
- **Premissa nova registrada aqui (P-031A — add fora da canônica nasce com `Level=1`):** `AddOutsideAsync` cria o `SkillLevelRow` já com `Level=1` (não 0). Razão: um row novo com 0 seria descartado por `ToDefinition`/`ToDict` (entra no dict mas vira ruído sem efeito) e a regra "não criar zeros novos" tornaria o add inócuo. Nascendo em 1, a skill fica definida e o autor ajusta inline. Não afeta o round-trip de zeros pré-existentes (esses chegam via `FromDefinition`, não por este caminho).
- **Custo inline vs. peso/origem:** o componente mostra apenas o **custo** por linha (coluna compacta), não as colunas Weight/Origin que a `<MudTable>` antiga do detalhe exibia. Decisão de UX (kickoff: "custo por skill inline"); peso/origem detalhados continuam disponíveis no painel "Cost summary" do detalhe e nos warnings. Premissa registrada — reversível se o 033 pedir as colunas de volta.
- **Seção de transbordo em compare:** em modo `Compare`, skills que só B possui (fora da canônica) também entram no transbordo, para um delta B-only não ficar invisível.

## Build / validação

- `dotnet build` NÃO executado nesta etapa (estágio dedicado faz). Sem validação em jogo/browser aqui — pendência abaixo.

## Pendências

- [ ] Compilar (estágio dedicado) e validar no browser real (circuito SignalR; MudTabs só pré-renderiza a aba ativa, então a aba Skills do editor precisa de browser, não curl) — espelha a nota de evidência do 026.
- [ ] Confirmar visualmente as cores/separadores e o esmaecimento de nível 0 contra o viewer RZ original.
- [ ] Save real: editar um nível inline → Save → conferir no `.jsonc` que zeros pré-existentes sobrevivem e nenhum zero novo aparece (critério de aceite 5).
