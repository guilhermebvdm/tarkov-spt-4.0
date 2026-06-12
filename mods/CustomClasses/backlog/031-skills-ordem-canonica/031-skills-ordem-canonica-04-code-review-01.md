# 031 — Skills em ordem canônica — Code-review (01)

**Mod:** CustomClasses
**Criado:** 2026-06-12
**Refs:** [02-spec-tech](./031-skills-ordem-canonica-02-spec-tech.md) · [03-spec-tech-review-01](./031-skills-ordem-canonica-03-spec-tech-review-01.md) · [01-spec](./031-skills-ordem-canonica-01-spec.md)

Revisão do código entregue no território do 031:
`SkillMaster.cs`, `Web/Shared/SkillCanonicalList.razor`, `Web/Pages/ClassDetail.razor`, `Web/Pages/ClassEdit.razor`.

Build de baseline antes da revisão: **0 warnings, 0 erros** (Release, `--no-incremental`).

Convenção: 🔴 bug/crash/build-breaker · 🟡 fuga de spec / risco · 🟢 confirmação · ⚪ dead code / fio solto.
Coluna **Decisão**: ✅ Aplicado (fix local seguro) · ⏸ Adiado (design/ambíguo/cross-território).

## Premissas autônomas (usuário ausente)

- **PA-01:** não rodei o circuito Blazor (sem servidor SPT aqui); validação é estática + build. Coerente com a memória `feedback_spt_validation.md` — as marcações de aceite no jogo ficam para o estágio de validação.
- **PA-02:** corner cases 5 (multiplicador em skill nível 0) e 6 (Special Elite sem compat) foram lidos como satisfeitos pelo código atual; ver achados CR-01-05/06.

## Achados

### 🔴 / build × correção

Nenhum bug de null/crash nem build-breaker encontrado.

- Tipos batem: `def.Skills` (`Dictionary<string,int>?`) → `Levels` (`IReadOnlyDictionary<string,int>?`) e `def.SkillMultipliers` (`Dictionary<string,double>?`) → `Multipliers` são atribuições covariantes válidas.
- Todos os lookups (`BuildLevelLookup`, `CostOf`, `MultiplierOf`, `BuildCompareLookup`) são `OrdinalIgnoreCase` e degradam a 0/null em chave ausente — sem `KeyNotFoundException` (B2 da spec-tech honrado).
- `MultiplierLookup()` no `ClassEdit` usa `TryAdd` (primeira ocorrência vence) — não lança em skill duplicada num arquivo manual (B1 honrado).
- `SetLevelAsync` com `value == 0` e sem row não cria entrada — round-trip de zeros (aceite 5 / corner case 4) preservado.

### ⚪ CR-01-03 — `LevelOf(name)` é dead code — ✅ Aplicado

`SkillCanonicalList.razor:209` definia `private int LevelOf(string name)` sem nenhum chamador (grep no projeto inteiro retorna só a própria definição). Além de morto, reconstruía o dicionário (`BuildLevelLookup()`) a cada chamada. As linhas leem o lookup `levels` já montado uma vez no bloco `@{}`. **Removido**, comentário `// ref: CR-01-03` no lugar. Fio solto óbvio, local, sem efeito de comportamento.

### 🟡 CR-01-01 — `MultiplierOf` faz varredura linear por linha — ⏸ Adiado

`MultiplierOf(name)` itera todo o dicionário `Multipliers` por linha renderizada (O(linhas × multiplicadores)). Mesmo padrão em `CostOf` (`FirstOrDefault` na lista de custos). Em escala real (≤35 linhas, poucos multiplicadores) é irrelevante; um fix "correto" seria pré-indexar num dicionário case-insensitive por render, espelhando `BuildLevelLookup`. **Decisão:** otimização, não correção — adiado para não introduzir risco sem ganho mensurável.

### 🟡 CR-01-02 — `BuildCompareLookup()` chamado duas vezes por render — ⏸ Adiado

No bloco `@{}` (linha 27) e dentro de `BuildOverflowEntries` (linha 181) o lookup de comparação é reconstruído. Caminho exclusivo do modo Compare (036), que ainda não tem chamador real (fora de escopo aqui). Consolidar é trivial mas cruza a fronteira conceitual do 036. **Adiado** — território do item 036.

### 🟡 CR-01-04 — `_outsideToAdd` no predicado de exibição do overflow é ramo morto — ⏸ Adiado

`SkillCanonicalList.razor:48`: `@if (overflow.Count > 0 || (Editable && !string.IsNullOrEmpty(_outsideToAdd)))`. `AddOutsideAsync` zera `_outsideToAdd` **e** adiciona o row (level 1) a `EditRows`, então o item já entra em `overflow` e `overflow.Count > 0` cobre o caso; o segundo termo nunca é decisivo. Inofensivo (não causa render incorreto). Remover o termo é uma simplificação de layout/legibilidade. **Adiado** — design, sem impacto funcional.

### 🟡 CR-01-05 — chip de multiplicador some para skill fora da canônica com nível 0 — ⏸ Adiado

Corner case 5 ("multiplicador para skill com nível 0 → linha exibe o chip ±%") está satisfeito para skills **canônicas** (a linha sempre existe). Mas `BuildOverflowEntries` monta a seção de transbordo só a partir de `levels.Keys` (níveis), não das chaves de `Multipliers`. Uma classe que defina apenas um multiplicador de XP para uma skill **fora da tabela canônica** e nível 0 não teria linha — o chip ficaria invisível. É a borda de uma borda (skill não-canônica + sem nível + só multiplicador); o `CostService` já emite warning para skills fora do mapa. Fix exigiria decidir o comportamento (unir chaves de `Multipliers` ao overflow) — escolha de design. **Adiado.**

### 🟡 CR-01-06 — duplicata de skill em `EditRows`: display (last-wins) ≠ edição (first-wins) — ⏸ Adiado

`BuildLevelLookup` usa `lookup[row.Skill] = row.Level` (última ocorrência vence na exibição), enquanto `SetLevelAsync` faz `FirstOrDefault` (edita a primeira) e `ClassEditModel.ToDict` mantém a primeira ao salvar. Se um arquivo manual tiver a mesma skill duas vezes, o número exibido pode divergir do que é editado/salvo. A UI impede duplicatas no fluxo normal e o código já documenta "UI prevents duplicate skills"; alinhar tudo para first-wins é defensável mas é mudança de comportamento ambígua sobre arquivo malformado. **Adiado** — fix ambíguo.

### 🟢 Confirmações

- **Sem magic numbers (P5):** `SkillMaster.Entries` é derivado de `SkillWeights.Explicit` (31 skills) + `SkillsExtendedCompat.Skills` (4 SE), com `MainCategoryOrder`/`SpecialEliteOrder` como única ordem explícita. `Entries.Count` nunca é escrito. Os 4 nomes de `SpecialEliteOrder` casam 1:1 com `SkillsExtendedCompat.Skills`.
- **Exclusão Special Elite no loop principal é no-op seguro:** FirstAid/FieldMedicine/UsecNegotiations/BearRawpower estão em `Derived`, não em `Explicit`, então `specialEliteSet.Contains` nunca dispara no loop das categorias principais — mas a guarda é correta e barata (defensiva contra um futuro move para `Explicit`).
- **Corner case 2 (saturação):** `BarPercent` clampa em 100% (`Math.Min(1.0, level/10.0)`) e a célula de nível mostra o valor real, não o saturado.
- **Aceite 4 / corner case 4 (round-trip de zero):** coberto por `SetLevelAsync` (não cria zero) + `ToDict` (descarta tabela vazia). Zero pré-existente vira um `SkillLevelRow` em `FromDefinition` e sobrevive ao `ToDict`.
- **`AvailableSkills`/`CultureInfo`/`using System.Globalization`** continuam em uso após o refactor (aba de multiplicadores e formatações) — sem using/membro órfão. `SkillCostEntryFor`, `AddSkill`, `RemoveSkill`, `_newSkill` foram removidos junto com a tabela antiga e não têm referências remanescentes.

## Resultado

- **Aplicado:** CR-01-03 (remoção de `LevelOf` dead code).
- **Adiado:** CR-01-01, CR-01-02, CR-01-04, CR-01-05, CR-01-06.
- **Build após aplicar:** ver estágio de build dedicado (rodado: 0/0 esperado, mudança é só remoção de método não referenciado).
