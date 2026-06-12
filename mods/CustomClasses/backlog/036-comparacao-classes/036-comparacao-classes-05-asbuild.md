# 036 — Modo comparação A×B no dashboard — As-build

**Mod:** CustomClasses
**Status:** Implementado (v1)
**Data:** 2026-06-12
**Refs:** [02-spec-tech](./036-comparacao-classes-02-spec-tech.md) · [00-kickoff](./036-comparacao-classes-00-kickoff.md)

## Arquivos modificados

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Server/Web/Pages/ClassDetail.razor` | MODIFICADO | Picker "Compare with…" no header; estado de compare B; `?compare=` deep-link; badges A vs B (skill cost / loadout ₽ / skills #) com `DeltaChip`; Hideout/Outfit em 2 colunas; passa `Compare` ao `SkillCanonicalList`. |
| `modded/Server/Web/wwwroot/css/customclasses.css` | MODIFICADO (aditivo) | Bloco `/* 036 — comparação A×B */` ao fim: `.cc-cmp-b`, `.cc-cmp-2col`, `.cc-cmp-col__head`. Nada do 033/034 reescrito. |
| `modded/Server/Web/Shared/SkillCanonicalList.razor` | NÃO MODIFICADO | O parâmetro `Compare`, a coluna de delta (`DeltaCell`, B−A), o overflow de B e o `ColumnCount` já nasceram no 031. v1 apenas ATIVA via `Compare="@_compareDef"` no ClassDetail. Confirmado pela leitura: `:104-106`, `:144-156`, `:179-188`, `:366-370`, `:395-410`, `:413-414`. |

## O que foi implementado

1. **Estado de compare (ClassDetail `@code`):** `CompareParam` (`[SupplyParameterFromQuery(Name="compare")]`), `_compareEntry`, `_compareDef`, `_compareSkillCost`, `_compareLoadoutCost`, `_compareCandidates`, `IsComparing`.
2. **`ResolveCompare(entries)`** chamado no fim de `Reload()`, **reusando a lista `entries` já carregada por A** (otimização da spec §Perf — sem 2ª chamada a `ListClassFiles()`). Candidatos = classes parseáveis ≠ A. Match por `FileName` ou nome sem extensão. Custos de B computados **uma vez** aqui (não por-render). Estado de compare zerado no topo de `Reload()`.
3. **Picker no header** (após `<MudSpacer/>`): `MudMenu` "Compare with…" / "Comparing: <B>", itens com ícone + cor; `MudIconButton` de Close quando comparando. `SetCompare`/`ClearCompare` via `Nav.GetUriWithQueryParameter("compare", …)`.
4. **Badges A vs B:** Skill cost, Loadout ₽ e um badge novo "Skills #" (só no modo compare). Cada um mostra `A`, `vs B` (`.cc-cmp-b`) e um `DeltaChip`.
5. **`DeltaChip(a, b, fmt)`:** Δ = A − B do ponto de vista de A — ▲ verde (A maior) / ▼ vermelho (A menor) / `=`.
6. **Hideout / Outfit em 2 colunas:** render fragments `HideoutBlock`/`OutfitBlock` extraídos (033 markup reusado), exibidos lado a lado (`.cc-cmp-2col`) com cabeçalho A/B quando comparando; idênticos ao 033 fora do modo.
7. **`SkillCanonicalList` recebe `Compare="@_compareDef"`** — `null` fora do compare ⇒ comportamento 033 inalterado; ≠ null ⇒ coluna de delta de nível (B−A) ativa.

## Premissas / decisões registradas (política autônoma — sem aprovação)

- **PA-036-01 / "B fixa enquanto A navega pelo sidebar":** implementado **via query param `?compare=`** apenas. O NavMenu (território do 030) NÃO foi tocado. Como o `?compare=` persiste na URL e os links do sidebar trocam só o segmento `FileName` da rota (não a query), B permanece ativa ao navegar A **se os links do NavMenu preservarem a query** — comportamento que depende do 030. Não foi possível garantir/verificar aqui sem mexer no 030. **Limitação v1 registrada**; preferências de navegação ficam no 035.
- **🔴-R1 (polaridade de cor):** a coluna de delta por skill (componente 031) pinta **B−A** (verde = B tem mais). Os `DeltaChip` de resumo pintam **A−B** (verde = número de A maior). São intencionalmente opostos; rotulados ("Skill cost (A vs B)", etc.) para leitura inequívoca. Componente NÃO reescrito (PA-036-03).
- **🔴-R2 (multiplicadores lado a lado):** v1 = opção (A) da spec — multiplicadores de B **não** entram na linha de skill; a comparação fica pela coluna de delta de nível + badges de resumo. NÃO mexeu no contrato 031. Follow-up: `CompareMultipliers` no componente.
- **`higherIsA`:** removido o parâmetro da spec; as três métricas usam a mesma polaridade (verde = A maior). Leitura comparativa, não veredito de balanceamento.
- **Nomes de roupa de B:** sem catálogo dedicado — `ClothingLabel` de B cai no fallback "id cru" (corner aceitável, fora de escopo aprofundar).
- **Coluna direita do 034 (gear/stash visual):** NÃO entra no modo compare (fora de escopo 01) — só header + coluna esquerda + hideout/outfit.

## Corner cases tratados

- `?compare=` ausente/vazio ⇒ single.
- `?compare=` = A, inexistente ou inválida (não parseia) ⇒ ignorado, volta a single.
- A não parseável ⇒ `ResolveCompare` nem roda (guard pelo `if (_entry?.Definition is not { } def) return;` antes do cálculo de A).
- Skill só de B aparece na seção "Outside canonical" do componente (overflow do 031).

## Não verificado

- `dotnet build` NÃO executado (instrução do item). Validação de compilação e visual ficam para o passo de compile/review.
- Comportamento real do "B fixa ao navegar A pelo sidebar" depende do 030 (ver PA-036-01).
