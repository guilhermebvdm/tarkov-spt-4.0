# 023 — Pickers de item (MudBlazor) — Spec técnica

**Mod:** CustomClasses
**Criado:** 2026-06-10
**Origem:** [023-item-picker-ui-01-spec.md](./023-item-picker-ui-01-spec.md)

## 1. Decisão: imagem de item (spike obrigatório)

**Decisão: URL do tarkov.dev com fallback automático para texto puro (`onerror` esconde a `<img>`).**

Evidência avaliada:

1. **Server SPT não serve ícone por item.** `ImageRouter` (`references/spt-source/.../Routers/ImageRouter.cs`) é key-value: só responde URLs registradas via `AddRoute`. Quem registra é `DatabaseImporter.CreateRouteMapping` (`Utils/DatabaseImporter.cs:39-64`), que mapeia **apenas** o conteúdo de `SPT_Data/images/` sob `/files/...`. Inspeção do diretório instalado (`D:/SPT/SPT/SPT_Data/images/`): `achievement/`, `banners/`, `handbook/` (83 PNGs — **ícones de categoria**, não de item), `hideout/`, `quest/`, `trader/` etc. Não existe ícone por tpl.
2. **tarkov.dev tem padrão estável por tpl.** `tools/tarkov-itemdb/data/items.json` (gerado por `scripts/fetch-tarkov-dev.js`) confirma os padrões: `https://assets.tarkov.dev/{tpl}-icon.webp` (usado — 32px ok), além de `-grid-image.webp` e `-512.webp`.
3. **Fallback texto.** Itens adicionados por outros mods não existem no tarkov.dev; offline nenhuma imagem carrega. Implementação: `<img src="https://assets.tarkov.dev/{tpl}-icon.webp" loading="lazy" onerror="this.style.display='none'">` — a imagem quebrada se esconde e a linha continua 100% funcional só com texto. Zero JS interop, zero dependência do circuit.

Consequência aceita: thumbs exigem internet no browser do usuário (não no server). Aceitável — é editor de configuração, não UI in-game.

## 2. Contratos dos componentes (`Web/Shared/`)

### `ItemPicker.razor`

| Parâmetro | Tipo | Default | Notas |
|---|---|---|---|
| `OnItemSelected` | `EventCallback<string>` | — | tpl selecionado (disparado nos dois modos) |
| `Title` | `string` | `"Select item"` | título no modo diálogo |
| `ShowCategoryFilter` | `bool` | `true` | exibe dropdown de categoria |
| `CategoryId` | `string?` | `null` | filtro de categoria pré-aplicado |
| `FilterTpls` | `Func<string,bool>?` | `null` | predicado pós-busca (hook do 026) |
| `Limit` | `int` | `100` | cap do `CatalogService.Search` |

- **Modo dual:** `[CascadingParameter] IMudDialogInstance?` — non-null ⇔ aberto via `IDialogService.ShowAsync<ItemPicker>(...)`; nesse caso o corpo é envolvido em `<MudDialog>` e a seleção chama `Dialog.Close(DialogResult.Ok(tpl))`. Inline, o corpo renderiza direto. Corpo único via templated RenderFragment (`@<MudStack>...`).
- **Busca:** `MudTextField` `Immediate` + `DebounceInterval=300` + `OnDebounceIntervalElapsed`; a chamada `Catalog.Search` roda em `Task.Run` (varre todos os templates + locales — não pode segurar o circuit). Token de versão (`_searchVersion`) descarta resultados obsoletos de buscas sobrepostas.
- **Resultados:** `MudVirtualize` (`ItemSize=48`) em container com altura fixa/scroll; linha = thumb 32px + nome + shortname/tpl + categoria + `price.ToString("N0") ₽`.
- **Categorias:** árvore do handbook achatada depth-first com indentação por nbsp no `MudSelect`.

### `PresetPicker.razor`

| Parâmetro | Tipo | Notas |
|---|---|---|
| `WeaponTpl` | `string?` | tpl raiz; revalidado em `OnParametersSet` |
| `OnPresetSelected` | `EventCallback<string>` | id do preset |

`MudTable` com chips **Default** (`Color.Info`) e **Premium** (`Color.Warning`) — flags vêm prontas do `CatalogService.GetPresetsFor`. Guard `MongoId.IsValidMongoId` antes de construir `MongoId` (o ctor lança em string malformada — derrubaria o circuit com tpl meio digitado).

### `AmmoPicker.razor`

| Parâmetro | Tipo | Notas |
|---|---|---|
| `WeaponTpl` | `string?` | calibre resolvido via `GetCaliber` |
| `Caliber` | `string?` | precedência sobre `WeaponTpl` |
| `OnAmmoSelected` | `EventCallback<string>` | tpl do cartucho |

`MudTable` fixa (320px) com Dmg/Pen/preço; chip com o calibre resolvido. Mesmo guard de `MongoId`.

### `CustomizationPicker.razor`

| Parâmetro | Tipo | Default | Notas |
|---|---|---|---|
| `Side` | `string` | `"Usec"` | nome de side do DB (`Usec`/`Bear`) |
| `SlotKind` | `string` | `"Upper"` | `Upper`→slot `upper`, `Lower`→`lower` |
| `OnClothingSelected` | `EventCallback<string>` | — | id da customization |

Lista de `GetClothing(Side)` filtrada pelo slot, ordenada por nome, com filtro de texto (debounce 200ms) + `MudVirtualize`.

## 3. APIs adicionadas ao `CatalogService` (read-only)

O kickoff previa a adição caso o serviço não expusesse calibre — não expunha:

- **`record CatalogAmmo`** — `Tpl, Name, ShortName, Price, Damage, Penetration, Caliber` (dano/pen direto de `_props.Damage`/`_props.PenetrationPower` — "barato", sem balística).
- **`GetCaliber(MongoId tpl)`** — `_props.ammoCaliber` (arma, `TemplateItem.cs:708`) com fallback `_props.Caliber` (munição, `:1352`).
- **`GetAmmoForWeapon(MongoId weaponTpl)`** — calibre da arma → `GetAmmoByCaliber`.
- **`GetAmmoByCaliber(string caliber)`** — varre `GetItems()`, exige baseclass `AMMO` (exclui ammo box / "calibres" de granada que pegam carona no prop), ordena por penetração desc.

## 4. Notas MudBlazor 8.13.0 (APIs verificadas no XML do pacote NuGet)

- Cascading param de diálogo é **`IMudDialogInstance`** (interface — v8 renomeou de `MudDialogInstance`).
- **`MudVirtualize<T>`** existe (`Items`, `ItemSize`, `OverscanCount`); precisa de container pai com altura fixa + `overflow-y: auto`.
- Debounce nativo em `MudDebouncedInput`: `DebounceInterval` + `OnDebounceIntervalElapsed` (com `Immediate="true"`).
- `MudChip`/`MudSelect`/`MudSelectItem`/`MudTextField` exigem `T="..."` explícito.
- `DialogService.ShowAsync<T>` + `DialogParameters<T>` (expression-based) para passar `Func<>` como parâmetro.

## 5. Riscos / limitações conhecidas

- Thumbs dependem de internet no browser; itens de mods terceiros ficam sem thumb (fallback texto cobre).
- `Search` é O(n) por chamada sobre o DB vivo (decisão do 022 — sem cache para refletir mods); mitigado por debounce + `Task.Run` + cap de resultados.
- `FilterTpls` é aplicado **pós-busca** — com filtros muito restritivos o nº de resultados pode ficar abaixo de `Limit` (aceitável para o 026; se incomodar, subir `Limit`).
