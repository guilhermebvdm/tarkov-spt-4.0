# 003 — Itens + hideout + 10 classes reais · As-Built

**Mod:** CustomClasses
**Spec funcional:** [003-starting-items-01-spec.md](003-starting-items-01-spec.md)
**Spec técnica:** [003-starting-items-02-spec-tech.md](003-starting-items-02-spec-tech.md)
**Reviews técnicas:** [review-01](003-starting-items-03-spec-tech-review-01.md) · [review-02](003-starting-items-03-spec-tech-review-02.md)
**Build inicial:** 2026-06-07

> Item **grande, implementação incremental**. **Fatias 1-5 implementadas + deployadas; playtest pendente** → 🟡 até validar in-game.
> Doc canônica de referência: [docs/technical/inventario-itens-spt4.md](../../../../docs/technical/inventario-itens-spt4.md).

## Fatias

| Fatia | Conteúdo | Status |
|---|---|---|
| **1** | DTO (loadout/hideout/ItemSpec/ModSpec) + HideoutBuilder + InventoryBuilder **equipado-simples** (slot-occupancy + subtree-removal) | ✅ compilado/instalado |
| **2** | InventoryBuilder: **preset** (IsPreset/HasPreset → clone + re-id, PA-01-02/04) + **árvore manual** (mods por slot) | ✅ compilado/instalado |
| **3** | **Carregador carregado** (`FillMagazineWithCartridge`) **+ câmara** (`ammo` obrigatório, PA-01-03) | ✅ compilado/instalado |
| **4** | **Stash + contents packing** — `GridPacker` first-fit + rotação, `Location {x,y,r}`, stack-aware (`StackMaxSize`), overflow logado | ✅ compilado/instalado |
| **5** | **Script `build-class-jsons.js`** → gerou as **10 classes reais** (recipes/skills/hideout do RZ; auto-categorização via items.json → equipa arma/pistola/armadura/capacete/rig/mochila, resto no stash) | ✅ gerado/instalado |

## Arquivos alterados (todas as fatias)

| Ação | Path | Resumo |
| --- | --- | --- |
| MODIFICADO | `modded/Server/ClassDefinition.cs` | + `hideout` (estação→nível) e `loadout` (`equipped`/`stash`) + `ItemSpec`/`ModSpec`/`Loadout`. (f1) |
| CRIADO | `modded/Server/HideoutBuilder.cs` | Seta `Areas[].Level` por `HideoutAreas` + `Active/Constructing/CompleteTime` (PA-02-03). (f1) |
| CRIADO | `modded/Server/InventoryBuilder.cs` | Equipado-simples + slot-occupancy/subtree-removal (f1); preset+re-id + árvore manual (f2); `LoadAmmo` carregador+câmara (f3); `PackSpecsIntoGrids`/`GetGrids` stash+contents (f4); try/catch por slot + guards (CR-01). |
| CRIADO | `modded/Server/GridPacker.cs` | Packer first-fit + rotação de 1 grade. (f4) |
| MODIFICADO | `modded/Server/CustomClassesMod.cs` | Injeta + chama Inventory/Hideout builders no `RegisterClass`; log com contagens (CR-01-04). (f1) |
| CRIADO | `scripts/build-class-jsons.js` + `scripts/class-recipes.js` | Gerador das 10 classes (auto-categoriza via items.json → equipa; resto no stash). (f5) |
| CRIADO | `modded/Server/config/classes/<10>.jsonc` | 10 classes reais geradas (medicoDeCombate, cacador, fuzileiro, batedor, operadorFurtivo, armeiro, operadorTatico, sobrevivencialista, saqueador, gerenteDeOperacoes). (f5) |
| MOVIDO/REMOVIDO | `config/classes/exampleClass.jsonc` → `_docs/` (não carrega); `testClass.jsonc` removido | Deixa só as 10 reais carregando. (f5) |

## PA resolvidos (reviews 01 + 02)

| ID | Resolução |
| --- | --- |
| PA-01-01..07 | Abordagens dobradas na spec (GridPacker, re-id de preset, `ammo` obrigatório, fallback `preset`, confirmar base, nullability, grids). Implementação nas fatias 2-4. |
| PA-02-01 | InventoryBuilder remove ocupante do slot antes de equipar. **(fatia 1)** |
| PA-02-02 | `RemoveItemAndChildren` recursivo. **(fatia 1)** |
| PA-02-03 | HideoutBuilder seta Active/Constructing/CompleteTime. **(fatia 1)** |
| PA-02-04 | Packer próprio na fatia 4 (checar InventoryHelper antes). |
| PA-02-05 | count>1 ignorado em slot equipado. **(fatia 1)** |

## Pendências

1. ✅ `/compile-mod` — compila (0 warn/err, DLL 38.9 KB) + as 10 classes `.jsonc` instaladas no servidor.
2. **Playtest das 10 classes** (RZ desabilitado): no log do server deve aparecer `Loaded 10 class(es)`; launcher mostra as 10 edições; criar uma classe → personagem nasce **equipado** (arma c/ mag+câmara, pistola, armadura/capacete/rig/mochila), resto no **stash** (sem overflow), skills + hideout corretos.
3. **Ressalvas conhecidas:** descrições pt-BR mostradas literais no launcher (i18n = item 008). Mapeamento de placement é heurístico (1º de cada categoria equipa) — validar se faz sentido por classe no playtest.
4. Após playtest OK → **🟢**.

## Mudanças posteriores

**2026-06-07 — code review 02 aplicada (óptica):** CR-02-01 (premium evita preset térmico/NV), CR-02-02 (`PickSimpleOptic`: red dot > assault scope, determinístico, evita térmica). **+ Extensão:** `EnsureMinimumOptic` agora roda também na **arma equipada** sem óptica (antes só no stash) — corrige arma principal sem mira (ex.: AKMS do Op. Furtivo, SV98 com mount sem scope). CR-02-03 deferido p/ 007; 04/05 opcionais. Recompilado 0 warn/err (51.7 KB).

**2026-06-07 — code review 01 aplicada** (fatias 1-3): CR-01-01 (try/catch por slot), CR-01-02 (câmara só se template tem `Chambers`), CR-01-03 (não recarrega mag já cheio), CR-01-04 (log com items/hideout por lado), CR-01-05 (comentário). Recompilado (0 warn/err, 34.8 KB).

**2026-06-07 — munição da pistola backup + auditoria final de calibre.** `backupKit` agora inclui `AMMO_9x18_PST` x60 (a pistola backup MAKAROV vinha sem munição 9x18 — afetava Fuzileiro e Op. Tático, cujas pistolas primárias são MP443 9x19). Auditoria automática arma×munição (contando caixas de munição) nas 10 classes: **✓ toda arma tem munição do calibre certo**, equipada e no stash. Regenerado + recompilado + instalado.

**2026-06-07 — Caçador: Mosin-infantry → Mosin Sniper (+ auditoria de calibre).** Backup do Caçador trocado p/ Mosin Sniper. **Atenção:** o 1º tpl tentado (`5bfea6e90db834001b7347f3`) tinha nome "Mosin" mas é **7.62x51 (.308)** — incompatível com a munição 762x54R. Corrigido p/ o Mosin Sniper real **`5ae08f0a5acfc408fb1398a1`** (762x54R, tem `mod_mount` + preset com óptica). Novo anchor `MOSIN_SNIPER`. Auditoria de calibre nas 10 classes: swap consistente (Caçador 762x54R em tudo); 20g do TOZ-106 OK (munição em caixa — `AMMO_20_70_BUCK` é caixa 20x70 apesar do nome interno "556"); gap pré-existente do RZ: pistola backup MAKAROV sem munição 9x18 sobressalente (não relacionado ao swap). Regenerado + recompilado (0 warn/err).

**2026-06-07 — mira mínima nas armas do stash (etapa 2):** armas do stash resolvem o **menor preset que já tenha óptica real** (`ResolveStashPreset` + `IsRealOptic` via baseclasses ASSAULT_SCOPE/COLLIMATOR/COMPACT_COLLIMATOR/OPTIC_SCOPE/SPECIAL_SCOPE); sem preset com óptica, `EnsureMinimumOptic` monta uma mira no 1º slot vazio compatível (óptica direta ou mount→óptica, validado pelo `_props.Slots` filter). Armaduras seguem default (placas). Cobertura: AKM/M4A1/AK74N/SV98/SAIGA12 (preset com óptica), AKS74U (mount→óptica). **Sem óptica possível (template sem ponto de montagem): Mosin-infantry, AKMS, SAIGA9** → mira de ferro. Recompilado (0 warn/err, 44.5 KB). **Mosin (exemplo do usuário) não scopável** sem trocar p/ variante Mosin Sniper (tem trilho) — decisão pendente.

**2026-06-07 — premium na arma principal (etapa 1):** a `FirstPrimaryWeapon` equipada agora usa o preset **mais kitado** da arma (mira/foregrip/tac), não o default. Flag `premium` no `ItemSpec`; `InventoryBuilder.ResolvePremiumPreset` (max nº de itens entre os presets daquela arma); gerador marca a primária. Armas do stash seguem preset default (etapa 2 = mira mínima em snipers do inventário). Recompilado (0 warn/err, 41 KB). Nota: armas só com preset base (AKS74U/AKMS/SAIGA9/Mosin/Makarov) caem no melhor disponível (= base).

**2026-06-07 — fix de playtest #2 (composto montado em todo lugar):** usuário reportou armas/armaduras saindo "só a base" (sem mods/placas), equipadas e no stash. Fix: `InventoryBuilder` agora **auto-completa com o preset default** qualquer item que tenha um (`BuildItemTree` p/ equipado; `PackSpecsIntoGrids` materializa árvore no stash) — armadura/capacete/rig vêm com `Soft_armor_*`+placas, armas com mira/cano/coronha/bipé/mag. Stash posiciona pela dimensão do item **montado** via `InventoryHelper.GetItemSize` (ExtraSize dos mods). Sem preset → item simples. Injetado `InventoryHelper`. Recompilado (0 warn/err, 39.9 KB). **Nuance:** acessórios premium da recipe (ex.: PSO-1, bipé AI Harris) ainda ficam soltos no stash (não montados na arma) — preset default já traz mira/bipé padrão; montar os premium = refinamento futuro.

**2026-06-07 — fix de playtest (presets):** no 1º playtest todos os presets de arma/pistola logaram "não encontrado". Causa: o `PresetCache` do `PresetHelper` só é hidratado por `PresetController.Initialize()` (roda DEPOIS do `PostDBModLoader+1`), então `IsPreset`/`HasPreset` viam cache vazio. Fix: `InventoryBuilder` agora resolve o preset direto de `databaseService.GetGlobals().ItemPresets` (default por `Items[0].Template==tpl` + `Encyclopedia!=null`) e **clona** antes de re-id (`ICloner`). Trocado `PresetHelper` por `DatabaseService`+`ICloner` no ctor. Recompilado (0 warn/err, 39.4 KB).

## Histórico

| Data | Evento |
| --- | --- |
| 2026-06-07 | Fatia 1 via `/code-mod` — DTO + HideoutBuilder + InventoryBuilder equipado-simples. Compilado (0 warn/err) + instalado. Reviews 01+02 (PA-01-*, PA-02-*) aceitas; fatia 1 já aplica PA-02-01/02/03/05. |
| 2026-06-07 | Fatia 2 — InventoryBuilder: preset (IsPreset/HasPreset + re-id, PA-01-02/04) + árvore manual de mods. Compilado (0 warn/err, 32.3 KB). contents movido p/ fatia 4 (precisa do packer); mag/câmara = fatia 3 (logados). |
| 2026-06-07 | Fatia 3 — carregador (`ItemHelper.FillMagazineWithCartridge`, capacidade do template) + câmara (`Chambers[0].Name` ou `patron_in_weapon`); `ammo` obrigatório. Compilado (0 warn/err, 33.8 KB). |
| 2026-06-07 | Code review 01 aplicada (CR-01-01..05). |
| 2026-06-07 | Fatia 4 — `GridPacker.cs` (first-fit + rotação) + packing de stash e contents (`Location`, stack-aware via `StackMaxSize`, overflow logado). Compilado (0 warn/err, 38.9 KB). |
| 2026-06-07 | Fatia 5 — `scripts/build-class-jsons.js` + `class-recipes.js`: geradas as **10 classes reais** (auto-categorização via items.json → equipa arma/pistola/armadura/capacete/rig/mochila; resto no stash). testClass removido, example movido p/ `_docs/`. Recompilado + 10 `.jsonc` instaladas. Playtest pendente. |
