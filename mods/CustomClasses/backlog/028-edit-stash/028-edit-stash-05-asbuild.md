# 028 — Editor de inventário (stash) — As-built

**Mod:** CustomClasses
**Data:** 2026-06-10
**Refs:** [01-spec](./028-edit-stash-01-spec.md) · [02-spec-tech](./028-edit-stash-02-spec-tech.md)

## Arquivos entregues

| Arquivo | Conteúdo |
|---|---|
| `modded/Server/CostService.cs` | EDITADO — seção **"Stash capacity (item 028)"**: record `StashCapacityResult`, `CheckStashCapacity(ClassDefinition)` (resolução da grade da `baseEdition` → materialização espelhando `PackSpecsIntoGrids` → `GridPacker.Place` first-fit+rotação), `ResolveBaseStashGrids`, `TryPlace`. Ctor ganhou `InventoryHelper` (dimensão montada) e `DatabaseService` (profile templates). |
| `modded/Server/Web/ClassEditModel.cs` | EDITADO — `Stash` deixou de ser pass-through (`List<ItemSpec>?`) e virou `List<ItemSpecModel>` editável; `FromDefinition` materializa via `FromSpec`, `BuildLoadout` reconstrói via `ToSpec` (vazia → null, omitida na serialização). |
| `modded/Server/Web/Pages/ClassEdit.razor` | EDITADO — aba Stash real: card por linha (`#N` + duplicar + remover c/ confirmação) com `ItemSpecEditor AllowCount=true`; "Add item" via `ItemPicker` (sem filtro); totais (linhas/unidades) + "Stash value" ₽; `StashCapacityAlert()` (verde fits / laranja overflow / info não-resolvida + warnings extras); `RecomputeLoadoutCost` computa `_stashCost` (definição só-stash) e `_stashCapacity`; setter de `BaseEditionSelect` dispara recompute. |

Componente `Stash*` novo não foi necessário (o `ItemSpecEditor` do 026 cobre a linha). **Não tocados** (território do 029): `README.md`, `docs/`.

## Estratégia do dry-run (resumo + evidência)

Grade resolvida pelo mesmo caminho do `InventoryBuilder.Apply`: `GetProfileTemplates()[baseEdition]` → `character.Inventory.Stash` → item no `Inventory.Items` → `_props.Grids`. **Evidência (DB do install):** `SPT Zero to hero` → stash tpl `566abbc34bdc2d92178b4576` = "Standard stash **10x30**" (grade `hideout`, 300 células, stash base vazia). Materialização espelha `PackSpecsIntoGrids`: `ResolveStashPreset` p/ compostos (1 colocação/unidade, dimensão montada via `InventoryHelper.GetItemSize`), stack-split por `StackMaxSize` p/ simples; contents não consomem células (ficam dentro do contêiner). `GridPacker` vazio por grade (paridade com o builder; base com itens pré-existentes vira warning de overlap). Warning-only — nunca bloqueia o save.

## Build

- `dotnet build -c Release` — **0 erros, 0 warnings**.
- `compile-mod.sh CustomClasses` — instalado em `D:/SPT`; guard: "install matches repo — no divergence".

## Evidências (server real SPT 4.0 + browser via Chrome DevTools MCP)

Server up, `Loaded 11 class(es), skipped 0`. `https://…:6969/customclasses/classes/cacador/edit`:

- **curl GET → HTTP 200** (36 KB); label `Stash (27)` no HTML (contagem vem do novo modelo editável).
- **Aba Stash renderizada (browser, circuito vivo):** 27 cards com `ItemSpecEditor` completo — counts (Roubles ×100000, LPS ×200…), mods c/ chips `required` (PACA, SSh-68, Mosin), ammo por calibre (Makarov 9x18PM, Mosin 7.62x54R c/ switches desabilitados sem ammo), Contents em rig/backpack, botão "ADD ITEM" no fim. Alert **verde**: "Fits — 54% occupied (162/300 cells, stash 10×30 of base 'SPT Zero to hero')". Totais: 27 line(s) · 100314 unit(s) · Stash value 3.525.147 ₽. Screenshot: `evidence-stash-fits.png`.
- **Overflow on-change (DoD do kickoff):** count do Mosin 2 → 60 → alert **laranja**: "6 item line(s) won't fit: Mosin ×12, PACA Soft Armor ×2, SSh-68 ×2, BlackRock chest rig ×2, Scav backpack ×2, Pack of apple juice ×1 — the server will skip them with a warning (100% occupied…)"; Loadout total 4.770.122 → 21.400.752 ₽ ao vivo. Screenshot: `evidence-stash-overflow.png`. **Discard** → verde/54%/custo original de volta (aviso "some ao reduzir" ✔).
- **Duplicar:** linha Salewa → `Stash (27)→(28)`, 54% → 55.3% (162→166 células — Salewa 1×2 ×2), loadout +122.627 ₽. **Remover** (diálogo CANCEL/REMOVE) → de volta a 27/54%/custo original.
- Console do browser: só o erro pré-existente `MudPointerEventsNone` (script MudBlazor 2×, infra 020 — já anotado no 026; não relacionado).
- Pós-teste: **nenhum save** (arquivo intacto, sem `.bak` novo; único delta install↔repo é o `_audit.log` runtime pré-existente). Server finalizado (`Stop-Process`); porta 6969 livre. **Repo==install ao final.**

## Observações / pendências

- [ ] Validar em jogo que um perfil novo nasce com o stash editado (cadeia editor→arquivo→hot-apply→`InventoryBuilder` coberta; passo launcher→jogo segue manual — memória `feedback_spt_validation`).
- `EnsureMinimumOptic` do builder não é espelhado no dry-run (óptica injetada não tem `ExtraSize` — footprint idêntico; comentário no código).
- First-fit do dry-run = mesmo algoritmo do runtime (não-ótimo de propósito: o aviso reflete o packing real).
- Linha preset-only no stash não é empacotada pelo builder (usa só `tpl`) — o dry-run espelha e avisa; a UI permite o modo Preset (herdado do `ItemSpecEditor`), candidato a hint futuro.
- Housekeeping `MudPointerEventsNone` (script MudBlazor 2×) segue pendente — candidato ao 029/futuro.

## Pós-review (2026-06-10, CR-EP-01)

O code review do épico ([epico-editor-04-code-review-01.md](../029-docs-e-fechamento/epico-editor-04-code-review-01.md)) estendeu o builder: **stash/contents honram `preset` explícito (premium), árvore manual (`mods`), `ammo` (loadedMag/chambered) e `contents` recursivo** — a observação "linha preset-only no stash não é empacotada" acima ficou OBSOLETA (o builder agora a monta e empacota). `CheckStashCapacity` foi atualizado em paridade (preset explícito/mods medidos como árvore montada; contents/ammo seguem sem consumir células do stash) e o `CostService` espelha o builder final (count>1 equipado = 1×, chambered só com `_props.Chambers`, mag procurado recursivamente).
