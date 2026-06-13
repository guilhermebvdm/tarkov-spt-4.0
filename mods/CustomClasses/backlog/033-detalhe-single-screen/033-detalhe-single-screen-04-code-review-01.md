# 033 — Detalhe single-screen (dashboard) — Code review (01)

**Mod:** CustomClasses
**Criado:** 2026-06-12
**Revisa:** diff de `customclasses.css` (CRIAR), `BaseLayout.razor` (+2 linhas), `ClassDetail.razor` (dashboard 2 colunas substitui `MudExpansionPanels`)
**Refs:** [02-spec-tech](./033-detalhe-single-screen-02-spec-tech.md) · [03-spec-tech-review-01](./033-detalhe-single-screen-03-spec-tech-review-01.md)
**Método:** leitura adversarial do diff real (`git diff` dos 3 arquivos + arquivo novo `customclasses.css`) contra o código consumido (`SkillCanonicalList.razor` 031, `ClassViewItemSpec.razor`, `CustomClassesMetadata.cs`, `CostService`), o `@code` remanescente de `ClassDetail.razor` e a spec/auto-review.

Legenda: 🔴 bug/crash/build-breaker · 🟠 fuga de spec/fio solto · 🟡 design/ambíguo (ADIADO) · 🟢 confirmação.

## Resumo

Implementação fiel à spec técnica. Zero bloqueadores. O único achado "seguro" candidato (membro órfão `FactorColor`) **já estava resolvido no diff** — foi removido junto com a tabela de multiplicadores, exatamente como o R3 do 03 instruía. Nenhuma correção de código foi necessária neste review; os pontos abertos são todos de design/cosmético e ficam ADIADOS.

## 🟢 Confirmações

- **G1 — `FactorColor` órfão já removido.** O R3 do 03 alertava que `FactorColor` (único caller era a tabela de XP multipliers removida) viraria membro não usado. O diff já o remove (linhas `-488..-490`). Sem warning residual. Nada a aplicar.
- **G2 — `@using System.Globalization` ainda necessário.** `CultureInfo.InvariantCulture` permanece em uso (badges `:98`, weighted `:153`, tabela stash `:271-274`, `FormatRub :443`). Não é using órfão.
- **G3 — Sem null-deref novo.** Todos os acessos a `def.*` ficam dentro do branch `_entry.Definition is not { } def → return` (já presente, `@code :328`). `def.Description!.En` no `cc-desc` é guardado por `!string.IsNullOrWhiteSpace(def.Description?.En)` imediatamente acima (`:131`) — o `!` é seguro. `_skillCost`/`_loadoutCost` usam `?.`/`is null` em todos os pontos dos badges.
- **G4 — `SkillCanonicalList` reusado, não reimplementado.** A tag (`:146-149`) é a mesma do 031 (`Levels/Cost/Multipliers/Editable=false`), só movida para `cc-dash__left`. Adoção preservada (objetivo do kickoff).
- **G5 — `ClassViewItemSpec` e `MudTable` de `_stashLines` reusados tal qual.** Equipado itera `def.Loadout?.Equipped` (`:222-231`); stash usa `_stashLines` já filtrado por `Context=="stash"` no `Reload()`. Contrato 033→034 honrado (dois `cc-section` com `id` + comentário `EXTENSION POINT 034`).
- **G6 — Warnings de loadout represervados (R4 do 03).** `_loadoutCost?.Warnings` agora renderiza como `MudAlert` fino acima da tabela de stash (`:242-245`), em vez do "Cost summary" removido. `MissingPriceBadge` continua por linha (`:269`). Nenhum sinal de preço faltante perdido.
- **G7 — Estático servido pelo mesmo mecanismo dos ícones.** `CustomClassesMetadata.cs:9-13` (`IModWebMetadata` → host monta `wwwroot/` inteira sob `/CustomClasses-Server/`); o `<link href="/CustomClasses-Server/css/customclasses.css">` (`BaseLayout :21`) é subpasta da rota que já serve `wwwroot/icons/`. Csproj `Sdk="Microsoft.NET.Sdk.Web"` publica `wwwroot/` automaticamente. Sem rota/entrada manual nova.
- **G8 — Território do 030 intacto.** `BaseLayout` só ganhou o `<link>` + comentário no `<HeadContent>`; drawer/appbar/guard/`CascadingValue`/`NavigationLock` inalterados.
- **G9 — `cc-desc` + tooltip coerentes.** `white-space:nowrap` + `text-overflow:ellipsis` trunca em 1 linha; texto completo no `MudTooltip` que envolve o `div`. Sem fio solto.

## 🟡 ADIADOS (design / ambíguo / fora do território seguro)

- **DEF-1 — `MaxWidth.False` full-width pode espremer colunas em viewport estreito.** `MaxWidth.Large → MaxWidth.False` (`:22`). O CSS já tem `@media (max-width:960px)` que empilha (`cc-dash{flex-direction:column}`), mas entre ~960px e a largura plena as duas colunas `flex` podem ficar apertadas. Cosmético, desktop-first (mesmo pressuposto do viewer antigo); R6 do 03 já delega ajuste fino ao 035. **Design — não toco.**
- **DEF-2 — `!important` em `.cc-dense td/th`.** Escopado a `.cc-dense` (PT-2/R7 do 03), aceitável e documentado. Trocar por especificidade pura seria frágil contra o CSS do MudBlazor. **Design — não toco.**
- **DEF-3 — Ordem dos slots em `equipped` / chips de hideout.** Depende da ordem do dicionário desserializado (R5 do 03). Comportamento idêntico ao painel anterior; ordem canônica de slots é meta do 034 (`GearPanel`). **Cross-território (034) — não toco.**
- **DEF-4 — Estilos inline remanescentes nos badges.** Vários `style="font-size:12px;"` / `width:14px...` inline em `cc-badge__value` (`:108,116,123,124`). Poderiam virar classes na folha de densidade, mas é refino cosmético sem bug; 035 (densidade global) é o lugar. **Design — não toco.**

## Verificação obrigatória (política do repo)

Build verde **não** prova que o `<link>` carrega o CSS em runtime. A validação em jogo (estágio dedicado) deve confirmar: (a) `/CustomClasses-Server/css/customclasses.css` retorna 200 (não 404); (b) o dashboard 2 colunas renderiza denso; (c) o detalhe cabe em ≤1 scroll com uma classe real carregada. Conforme `feedback_spt_validation` — escrita em arquivo SPT exige validação no jogo.

## Veredito

**0 achados seguros para aplicar** (o único candidato — `FactorColor` órfão — já vinha removido no diff). 4 itens ADIADOS, todos design/cosmético/cross-território. Diff aprovado do ponto de vista de correção; pendente apenas a validação em jogo da política do repo.
