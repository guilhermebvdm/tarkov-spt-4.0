# 036 — Code review (review-01)

**Mod:** CustomClasses
**Status:** Concluído — sem 🔴; 0 fixes seguros aplicados; build verde
**Criado:** 2026-06-12
**Refs:** [02-spec-tech](./036-comparacao-classes-02-spec-tech.md) · [03-spec-tech-review-01](./036-comparacao-classes-03-spec-tech-review-01.md) · [05-asbuild](./036-comparacao-classes-05-asbuild.md)

Code review do diff do item 036. Política autônoma (usuário ausente): aplico **apenas** achados seguros (correção de bug/crash, build-breaker, fuga de spec com fix inequívoco/local, leak/dispose, fio solto óbvio) marcando `// ref: CR-01-NN` + "✅ Aplicado". Achados de design/layout, fix ambíguo ou cross-território são **adiados** (não tocam código). Convenções de FORMATO: `repo-workflow-best-practices` / `csharp-mod-best-practices`.

## Escopo revisado

| Arquivo | Status no diff |
|---|---|
| `modded/Server/Web/Pages/ClassDetail.razor` | MODIFICADO (+235/−29) — picker, estado compare, `ResolveCompare`, badges A×B, `DeltaChip`, `HideoutBlock`/`OutfitBlock`, 2 colunas |
| `modded/Server/Web/wwwroot/css/customclasses.css` | MODIFICADO (aditivo, +8) — `.cc-cmp-b`, `.cc-cmp-2col`, `.cc-cmp-col__head` |
| `modded/Server/Web/Shared/SkillCanonicalList.razor` | **NÃO modificado** (confirmado por `git status`) — `Compare`/`DeltaCell`/overflow de B já eram 031 |

Build de baseline (pré-fix): **verde** — `Compilação com êxito. 0 Aviso(s) 0 Erro(s)`.

## Veredito

**Nenhum achado seguro.** A implementação compila limpa, segue a spec técnica revisada (02) e respeita as decisões 🔴-R1/R2/R3 do 03. Os corner cases (`?compare=` vazio/=A/inválida/A-não-parseável) estão tratados; `Compute*` de B roda 1× em `ResolveCompare` (não por-render); o estado de compare é zerado no topo de `Reload()`; nada chama `Save`/`Delete` (read-only/efêmero confirmado). Os candidatos abaixo são **quality/design**, não bugs — todos **ADIADOS**.

## ✅ Aplicados (seguros)

Nenhum. (Sem bug/crash, build-breaker, leak ou fuga de spec inequívoca no diff.)

## ⏸ Adiados (não tocam código)

### CR-01-D1 — Campo `_compareEntry` escrito mas nunca lido (dead field)

`ClassDetail.razor:332` declara `_compareEntry`; é atribuído em `:351` (reset), `:428` (resolve) mas **nunca lido** — todo o gating usa `_compareDef`/`IsComparing`. É código morto inócuo (documenta intenção de "B resolvida"). **Por que adiar:** remoção é cleanup de qualidade, não correção de defeito; não altera comportamento nem build. Não se enquadra em "fio solto óbvio" (não há consumidor quebrado — simplesmente não há consumidor). Candidato a limpeza num pass de qualidade, fora da política de fix-seguro.

### CR-01-D2 — `_compareDef!.Name` pode ser `null` no label do picker e no cabeçalho da coluna B

`ClassDefinition.Name` é `string?` (`ClassDefinition.cs:14`). Em `:43` (`$"Comparing: {_compareDef!.Name}"`), `:234` e `:257` (`B — @_compareDef!.Name`) um `Name` nulo renderiza string vazia ("Comparing: " / "B — "). O `MudMenuItem` de candidato (`:55`) já usa fallback `?? Path.GetFileNameWithoutExtension(c.FileName)`, mas o label/cabeçalho de B não. **Não é crash** (`!` é só null-forgiving de compilação; interpolação de `null` → vazio). **Por que adiar:** é inconsistência **cosmética/design** (texto vazio vs fallback p/ nome de arquivo); o fix "correto" (qual fallback usar para B) é decisão de UI, não inequívoco — e na prática toda classe parseável do projeto tem `Name`. Sugestão p/ 035/follow-up: reusar o mesmo fallback `?? Path.GetFileNameWithoutExtension(_compareEntry!.FileName)`.

### CR-01-D3 — Polaridade de cor divergente entre badges (A−B) e coluna de skill (B−A)

`DeltaChip` (`:545-565`) pinta da ótica de A (▲ verde = A maior); o `DeltaCell` do `SkillCanonicalList` (031) pinta B−A (▲ verde = B maior). **Não é bug** — é **decisão de design registrada** (🔴-R1 no 03, comentada in-code em `:537-543` e `:200-202`), com rótulos explícitos ("vs B" / "Δ B−A") para desambiguar. **Por que adiar:** unificar a polaridade exigiria reescrever o contrato 031→036 (PA-036-03 proíbe) ou inverter os badges — mudança de design, não correção. Reavaliação fica no 035 (🟡-Y2).

### CR-01-D4 — `higherIsA` removido; 3 métricas usam verde = "A maior"

A spec-tech (`:191`) previa um parâmetro `higherIsA` no `DeltaChip`; o as-build o removeu (todas as métricas: verde = A tem o número maior). Skill cost / loadout ₽ maiores não são inequivocamente "bons". **Por que adiar:** é a decisão v1 registrada (🟡-Y2 no 03, premissa no 05) — leitura comparativa, não veredito de balanceamento; mitigada pelo `SkillTotalChip` (budget) ao lado. Mudar a semântica de cor é design, não fix seguro.

### CR-01-D5 — Multiplicadores de B não aparecem lado a lado (limitação v1)

`SkillCanonicalList` mostra ±% só de A; B não tem coluna de multiplicador. **Por que adiar:** decisão 🔴-R2 (opção A) — implementar exigiria parâmetro aditivo `CompareMultipliers` no componente 031 (território compartilhado, fora da v1). Follow-up nomeado.

### CR-01-D6 — Nomes de roupa de B caem no fallback "id cru"

`OutfitBlock(_compareDef.Outfit)` usa `ClothingLabel`, que resolve nomes só via `_clothingNames` (populado p/ A em `Reload`). Ids de B exibem o id cru. **Por que adiar:** 🟡-Y4 registrado — comparação textual de outfit é fora de escopo aprofundar; popular catálogo de B é custo sem valor proporcional.

### CR-01-D7 — `BuildOverflowEntries` chama `BuildCompareLookup()` 2×/render (031, fora de território)

Em `SkillCanonicalList.razor:181`, `BuildOverflowEntries` chama `BuildCompareLookup()` de novo (o `@{}` de topo já montou `compareLevels`), reconstruindo o dicionário de B uma 2ª vez por render. **Por que adiar:** (a) código **pré-existente do 031**, não está no diff do 036; (b) é micro-ineficiência (≤~70 skills), não bug; (c) tocar `SkillCanonicalList` é **cross-território** (PA-036-03 / fronteira 031). Registrado p/ quem mexer no 031.

## Notas de confirmação (sem ação)

- **Tipos batem:** `def.Skills`/`def.Hideout` são `Dictionary<string,int>?` ⇒ atribuíveis a `IReadOnlyDictionary<string,int>?` de `CountSkills`/`HideoutBlock`; `_compareDef` (`ClassDefinition?`) ⇒ `SkillCanonicalList.Compare`. OK.
- **Null-safety dos branches:** `HideoutBlock`/`OutfitBlock`/`DeltaChip` tratam entrada `null` internamente (`hideout is { Count: > 0 }`, `outfit is null`, `a is not { } av`). `_compareDef!` nos blocos 2-col é seguro: só renderizam sob `if (IsComparing)` ⇒ `_compareDef is not null`.
- **Reset de estado:** todos os 5 campos de compare zerados no topo de `Reload()` (`:351-355`) antes de qualquer early-return — sem vazamento de B entre navegações de A.
- **Navegação:** `SetCompare`/`ClearCompare` usam `Nav.GetUriWithQueryParameter` (extensão net9, sem `WebUtilities`); `null` remove a query. `[SupplyParameterFromQuery]` + `[Parameter] FileName` ambos chegam em `OnParametersSet → Reload`. OK.
- **CSS aditivo:** bloco final não redefine classe alguma do 033/034; cor de ▲/▼ continua vindo do `MudChip Color` (🔴-R3 respeitado).

## Build pós-review

Nenhum fix aplicado ⇒ a árvore permanece idêntica ao baseline verde. `dotnet build ... -c Release --no-incremental`: **Compilação com êxito, 0 erros, 0 avisos.**

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-06-12 | Guilherme | Criação. Code review do diff 036: 0 achados seguros, 7 adiados (D1 dead field, D2 Name nulo no label/head, D3 polaridade, D4 higherIsA, D5 multiplicadores B, D6 roupa B, D7 BuildCompareLookup 2× em 031). Build verde mantido. |
