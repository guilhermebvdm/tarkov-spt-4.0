# 033 — Detalhe single-screen (dashboard) — Auto-review da spec técnica (01)

**Mod:** CustomClasses
**Criado:** 2026-06-12
**Revisa:** [02-spec-tech](./033-detalhe-single-screen-02-spec-tech.md) · [01-spec](./033-detalhe-single-screen-01-spec.md)
**Método:** leitura adversarial do código real tocado/consumido (`ClassDetail.razor`, `SkillCanonicalList.razor`, `ClassViewItemSpec.razor`, `BaseLayout.razor`, `CustomClassesMetadata.cs`, `CustomClasses.Server.csproj`, `SkillWeights.cs`, `CostService.cs`).

Legenda: 🔴 bloqueador (resolver antes de codar) · 🟡 ajuste recomendado · 🟢 confirmação.

## 🔴 Bloqueadores

Nenhum aberto. Os candidatos a 🔴 levantados na revisão foram verificados contra o código real e resolvidos no 02 (ver abaixo). **Bloqueadores abertos: 0.**

### 🔴→✅ R1 — Onde o `<link>` do CSS vai buscar o arquivo (rota estática)

**Suspeita:** a spec assumia um prefixo `/CustomClasses-Server/css/...` por analogia com os ícones — se o host só expusesse `wwwroot/icons` (e não `wwwroot/` inteiro), o CSS daria 404 e o item não entregaria densidade nenhuma. Seria 🔴 (DoD inteiro depende do CSS carregar).

**Verificação:** `CustomClassesMetadata.cs:9-11` documenta explicitamente que o host **monta `wwwroot/` inteira** sob `/CustomClasses-Server/` (`ref: SPTWeb.cs InitializeSptBlazor/UseSptBlazor`), porque o metadata implementa `IModWebMetadata`. Os ícones funcionam por serem subpasta (`wwwroot/icons/`), não por um mapeamento dedicado.

**Resolução:** rebaixado de premissa a **fato** no 02 (seção `BaseLayout.razor` / nota "Servir o estático"). `wwwroot/css/customclasses.css` → `/CustomClasses-Server/css/customclasses.css` pelo mesmo mecanismo. Sem premissa pendente.

### 🔴→✅ R2 — `customclasses.css` chega ao output do build?

**Suspeita:** criar o arquivo na árvore-fonte não basta se ele não for copiado para a pasta instalada do mod (onde o host serve os estáticos — `ClassEdit.razor:608-611` confirma que os assets servidos vêm da pasta INSTALADA). Sem cópia, 404 em runtime → 🔴.

**Verificação:** o `.csproj` (`CustomClasses.Server.csproj:1`) usa `Sdk="Microsoft.NET.Sdk.Web"`. Nesse SDK, `wwwroot/` é o web root convencional e seus arquivos são publicados como static web assets **automaticamente** — é exatamente por isso que `wwwroot/icons/*` já é servido hoje sem nenhuma entrada manual no csproj. Uma subpasta nova (`wwwroot/css/`) segue a mesma convenção.

**Resolução:** **nenhuma mudança de csproj é necessária**; registrado como 🟢 G1 abaixo. (Risco residual coberto pela verificação em jogo do estágio de validação — política do repo: escrita em arquivo SPT exige validação no jogo, não só build+hash.)

## 🟡 Ajustes recomendados (aplicados/registrados no 02)

- **🟡 R3 — `FactorColor` órfão.** Após remover a tabela de multiplicadores (`:172-206`), `FactorColor` (`:500`) perde seu único caller (era usado em `:199`). Deixá-lo gera warning de membro não usado. → 02 já instrui remover **só se** zero callers, no code-mod (não na spec). OK.
- **🟡 R4 — Warnings de loadout cost órfãos.** O painel "Cost summary" removido (P3) era o único lugar que exibia `_loadoutCost.Warnings` (`:327-330`) e os `MissingPriceBadge`/⚠ no breakdown. O stash textual já mostra `MissingPriceBadge` por linha (`:298`), mas os warnings agregados de loadout sumiriam. → 02 instrui reposicionar `_loadoutCost.Warnings` como `MudAlert` fino acima do stash (coluna direita). Sem perda de sinal.
- **🟡 R5 — `def.Loadout?.Equipped` é dicionário ordenado?** A coluna direita itera `equipped` com `foreach (var (slot, spec) in equipped)`. A ordem dos slots depende da ordem do dicionário desserializado — hoje o painel atual (`:265`) já faz isso e ninguém reclamou. → não-bloqueante; registrado para o 034 considerar ordem canônica de slots ao montar o `GearPanel` visual.
- **🟡 R6 — `MaxWidth` full-width pode quebrar em telas estreitas.** Trocar `MaxWidth.Large` (`:22`) por full-width (P4) ajuda a coluna direita, mas em viewport estreito as duas colunas de `flex` sem wrap podem espremer. → aceitável neste item (editor é desktop-first, mesmo pressuposto do viewer antigo); 035 (densidade global) pode adicionar um `flex-wrap`/breakpoint se necessário. Não-bloqueante.
- **🟡 R7 — `!important` na densidade.** Documentado (PT-2) e escopado a `.cc-dense`. Aceitável; alternativa (subir especificidade sem `!important`) é frágil contra o CSS do MudBlazor. Mantido.

## 🟢 Confirmações (verificadas no código)

- **G1 — Estáticos automáticos:** `Sdk="Microsoft.NET.Sdk.Web"` + `wwwroot/` convencional → CSS publicado sem csproj manual (`CustomClasses.Server.csproj:1`; ícones já provam o caminho).
- **G2 — `SkillWeights.BudgetMin/Max` existem e são `const`:** `SkillWeights.cs:38-39` (28.0 / 32.0); já consumidos em `ClassDetail.razor:162` e `CostService.cs:169`. Os badges/total do 02 os usam igual ao código atual — sem invenção de assinatura.
- **G3 — `SkillCanonicalList` read-only é exatamente o que o 031 entregou:** parâmetros `Levels`/`Cost`/`Multipliers`/`Editable=false` (`SkillCanonicalList.razor:88-102`); a tag do 02 é a **mesma** já presente em `ClassDetail.razor:153-156`, só movida de coluna. O contrato 031→033 (`031-02-spec-tech.md:183-185`) previa exatamente este consumo sem novos parâmetros. Adoção preservada, não duplicada.
- **G4 — `ClassViewItemSpec` reusável tal qual:** parâmetro `Spec` (`ClassViewItemSpec.razor:80`); o 02 o instancia por slot igual ao painel atual (`:268`). Sem mudança.
- **G5 — `_stashLines` já filtrado:** `Reload()` (`:404-406`) materializa `_loadoutCost.Items.Where(Context=="stash")`; o contrato 033→034 entrega essa lista pronta para o `StashPanel` do 034. Tipos: `List<LoadoutCostEntry>`.
- **G6 — Header handlers intactos:** Edit href (`:45`), `OpenDuplicateDialogAsync` (`:435`), `OpenDeleteDialogAsync` (`:457`), `StatusChip` (`:517`), `SkillTotalChip` (`:537`), `FormatRub` (`:514`) — todos preservados; os badges apenas reusam os fragments/helpers existentes.
- **G7 — `BaseLayout` toca 1 linha:** o `<link>` entra no `<HeadContent>` (`:18-21`) sem alterar drawer/appbar/guard/`CascadingValue` (`:38-71`) — território do 030 intacto. Não usa `@Assets[...]` (asset do mod, não RCL do framework).
- **G8 — Corner case "Definition null":** o branch de erro (`:66-71`) e o "fix on disk" (`:82-88`) ficam acima do grid; o `cc-dash` só renderiza com `def` presente (corner case 1 da 01). Coerente.

## Veredito

Spec técnica **pronta para code-mod**, 0 bloqueadores. As duas suspeitas de 🔴 (rota estática R1, cópia para output R2) caíram após verificação no código (metadata monta `wwwroot/` inteira; SDK.Web publica estáticos automaticamente). Ajustes 🟡 R3–R7 já estão refletidos no 02 ou delegados explicitamente ao code-mod/034/035. Restrição operacional do repo: validar em jogo que o `<link>` carrega o CSS (não confiar só em build verde), conforme política de escrita em arquivos SPT.
