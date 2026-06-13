# 024 — Viewer de classes — Spec técnica

**Mod:** CustomClasses
**Criado:** 2026-06-10
**Refs:** [01-spec](./024-class-viewer-01-spec.md) · [00-kickoff](./024-class-viewer-00-kickoff.md)

## Rotas

| Rota | Página | Conteúdo |
|---|---|---|
| `/customclasses/classes` | `Web/Pages/Classes.razor` | lista (MudTable, linha clicável) |
| `/customclasses/classes/{FileName}` | `Web/Pages/ClassDetail.razor` | detalhe read-only |

Render: o host (`SPTarkov.Server.Web/SPTWeb.cs:28,39`) aplica `InteractiveServer` globalmente → `OnRowClick`/eventos funcionam sem configuração extra.

## Resolução do `FileName` na rota

O nome real do arquivo tem extensão (`cacador.jsonc`); um "." no último segmento da URL pode ser interceptado como request de arquivo estático. Decisão: **a rota carrega o nome SEM extensão** (`/customclasses/classes/cacador`); a lista navega com `Path.GetFileNameWithoutExtension` + `Uri.EscapeDataString`. O detalhe resolve em `OnParametersSet` contra `ListClassFiles()`: match exato do `FileName` primeiro (aceita URL com extensão também), depois match extensionless (OrdinalIgnoreCase). Colisão teórica `x.json` × `x.jsonc` no mesmo nome-base: vence a primeira na ordem determinística do ListClassFiles (caso degenerado aceito — o boot também registraria colisão de edition).

## Fonte de dados (decisões)

- **Lista e detalhe usam `ClassEditorService.ListClassFiles()`** (nunca `Load` nem registries): é a única fonte que enumera disabled/inválidas E traz os MESMOS diagnostics do boot (dry-run `ValidateAndBuild`, allowReplace=true) + flag `Registered`. Custo: 1 parse + dry-run por arquivo por visita — ok p/ ~12 classes (kickoff); carregado 1× em `OnInitialized`/`OnParametersSet`.
- **Custos**: `CostService.ComputeSkillCost`/`ComputeLoadoutCost` (022) — zero lógica de custo na UI. Budget exibido a partir de `SkillWeights.BudgetMin/Max` (28–32). Classe sem skills (Peladão): chip neutro "0"/"no skills" (CostService já suprime warnings nesse caso).
- **Status** (lista e header do detalhe): `Definition == null` OU qualquer diagnostic `Error` → **Invalid** (vermelho, tooltip com `[Code] Message`); senão `!Enabled` → **Disabled** (cinza); senão `Registered` → **Registered** (verde); senão **Not registered** (laranja — ex.: colisão de edition resolvida a favor de outro arquivo).
- **Ícones**: `<img src="/CustomClasses-Server/icons/{iconFile}">` (estáticos do wwwroot, convenção do 020 — `/{AssemblyName}/`). `iconFile` ausente → sem imagem (fallback texto, mesma degradação do client).
- **Outfit — nomes**: `CatalogService.GetItemName` NÃO cobre customization com confiabilidade (locale pode ter a key, mas o fallback de template lê só items.json) → o detalhe constrói um dict id→nome a partir de `CatalogService.GetClothing("Usec"/"Bear")` (mesmas regras de aceitação do OutfitBuilder); id desconhecido degrada pro próprio id.
- **Stash no detalhe**: linhas com `Context == "stash"` do `ComputeLoadoutCost`; contents/ammo/equipped aparecem no breakdown completo dentro de "Cost summary" (coluna Context). Stash declarado mas sem linha precificada → alerta apontando pros warnings.

## Componentes

| Arquivo | Papel |
|---|---|
| `Web/Shared/ClassViewItemSpec.razor` | renderer recursivo de `ItemSpec` (raiz por slot/stash/contents). Resolução espelha `CostService.AddSpec`: `preset` (id ou tpl → `Catalog.ResolveDefaultPreset`/`ResolvePremiumPreset` — internos, mesma assembly) > `tpl`. Chips: preset (nome + nº partes), premium, ×count, ammo; ícones loadedMag/chambered; contents via auto-recursão; "unresolved" quando o builder pularia a linha. Indent = aninhamento DOM (24px/nível); `Depth` só limita recursão (cap 12). |
| `Web/Shared/ClassViewModSpec.razor` | renderer recursivo de árvore manual de `ModSpec` (slotId + nome do item). Mesmo esquema de indent/cap. |

Prefixo `ClassView*` conforme divisão de território com o item 023 (que trabalha em `Web/Shared/` em paralelo).

MongoId malformado em qualquer resolução de nome: try/catch → exibe o raw string (o CostService já reporta como warning; a UI nunca lança).

## Arquivos tocados

| Arquivo | Ação |
|---|---|
| `Web/Pages/Classes.razor` | CRIADO — lista |
| `Web/Pages/ClassDetail.razor` | CRIADO — detalhe (MudExpansionPanels: General/Skills/XP multipliers/Hideout/Outfit/Equipped/Stash/Cost summary; diagnostics no topo) |
| `Web/Shared/ClassViewItemSpec.razor` | CRIADO |
| `Web/Shared/ClassViewModSpec.razor` | CRIADO |
| `Web/Shared/NavMenu.razor` | EDITADO — link "Classes" → `/customclasses/classes` (era placeholder p/ Home) |
| `Web/Pages/Home.razor` | EDITADO — card "Class editor" + botão pra lista (Home vira entrada secundária/smoke) |

Não tocados: `CatalogService.cs` (pode receber edits do agente 023), `ClassEditorService.cs`, `CostService.cs`, registries, builders, csproj.

## Decisões de UI

- Texto da UI em inglês (consistente com Home/020); docs pt-BR.
- MudBlazor 8.13.0 (transitivo do `SPTarkov.Server.Web` — confirmado em `obj/project.assets.json`): `MudChip` com `T="string"`, `MudExpansionPanel` `Text`/`Expanded`, `TableRowClickEventArgs<T>.Item`.
- `RenderFragment` helpers no `@code` (padrão `__builder`) p/ chips de status/custo — evita componente extra por chip.
- Formatação: pesos/custos de skill `0.00` invariant; ₽ `N0` invariant + sufixo " ₽".
- Multiplicador: verde `>1`, vermelho `<1`, neutro `=1`; badge "Skills-Extended" via `SkillsExtendedCompat.Skills`; aviso de SE ausente via `ClassRegistrar.SkillsExtendedInstalled`.
