# 003 — Itens + hideout + 10 classes reais · Spec Técnica

**Mod:** CustomClasses
**Spec funcional:** [003-starting-items-01-spec.md](003-starting-items-01-spec.md)
**Criado:** 2026-06-07

> Mod **server-side** (SPT 4.0). Fonte: [references/spt-source/](../../../../references/spt-source/). Estende o loader do 002 para montar **árvores de inventário** (equipado + composto) e **hideout**, e gera as **10 classes reais** reusando dados do RZCustomProfiles. Item grande — implementar **incremental** (formato+builders → capacidade → hideout → 10 classes).

## 1. Estratégia

No `RegisterClass` (após clonar a base), além das skills, montar o inventário e o hideout do `Character` (`PmcData`) a partir de novas seções do JSON:
- **equipado:** para cada slot (`EquipmentSlots`), criar um `Item` com `ParentId = Inventory.Equipment` e `SlotId = <slot>`; resolver **composto** por (a) **preset** — `PresetHelper.GetDefaultPreset(tpl)`/`GetPreset(id)` → clonar `Preset.Items` (a árvore), re-raiz no slot; ou (b) **árvore manual** (`mods`/`contents` recursivos por `slotId`).
- **carregador + câmara:** preencher o carregador inserido (item filho `slotId="cartridges"` com `Upd.StackObjectsCount = capacidade`) e bala na câmara (`slotId="patron_in_weapon"` — ver [BotWeaponGenerator.cs:213/245](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Generators/BotWeaponGenerator.cs#L213)).
- **stash:** itens soltos com `ParentId = Inventory.Stash`, `SlotId = "hideout"`, `Location {x,y,r}` (packing na grade) + stack-aware (`Upd.StackObjectsCount`).
- **hideout:** setar `PmcData.Hideout.Areas[].Level` por `Type` (`HideoutAreas`).
- **IDs:** gerar `MongoId` únicos por instância na árvore do template; o SPT re-id no `CreateProfileService` ([CreateProfileService.cs:94](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/CreateProfileService.cs#L94) `ReplaceProfileInventoryIds`), então só precisam ser únicos dentro do template.
- **10 classes:** um **script** gera os 10 JSONs reusando recipes/anchors/skills/hideout do RZ (mapeando o loadout "tudo no stash" para equipado/composto — design por classe).

## 2. Pontos de integração (SPT server)

| Alvo (spt-source) | Uso |
|---|---|
| [`Item.cs` `Item`/`Upd` (`StackObjectsCount` :133)](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Eft/Common/Tables/Item.cs#L112) | Nós da árvore (`Id/Template/ParentId/SlotId/Location/Upd`) |
| [`EquipmentSlots.cs`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Enums/EquipmentSlots.cs#L3) | Slots de equipamento (Headwear, ArmorVest, TacticalVest, Backpack, FirstPrimaryWeapon, Holster…) |
| [`PresetHelper.cs:161 GetDefaultPreset` / `:124 GetPreset`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Helpers/PresetHelper.cs#L124) | Resolver preset de arma |
| [`Globals.cs:4393 Preset` (`Items` :4411, `Parent` :4408)](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Eft/Common/Globals.cs#L4393) | Árvore do preset a clonar |
| [`BotWeaponGenerator.cs:213/245` (chamber `patron_in_weapon`)](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Generators/BotWeaponGenerator.cs#L213) | Convenção de câmara/cartridges |
| [`BotBase.cs:706 Hideout` / `:710 Areas` / `:828 BotHideoutArea`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Eft/Common/Tables/BotBase.cs#L706) | Setar níveis de estação (`HideoutAreas` Type + Level) |
| [`CreateProfileService.cs:94 ReplaceProfileInventoryIds`](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/CreateProfileService.cs#L94) | IDs do template re-id no profile (só únicos no template) |
| `DatabaseService.GetGlobals().ItemPresets` ([PresetController.cs:18](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Controllers/PresetController.cs#L18)) | Catálogo de presets |
| [tools/tarkov-itemdb](../../../../tools/tarkov-itemdb/) + [mods/RZCustomProfiles](../../../RZCustomProfiles/) | tpl/dims/stackMax + recipes/anchors/skills/hideout das 10 classes |

## 3. Novas propriedades F12

Não se aplica (server-side).

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Server/ClassDefinition.cs` | MODIFICAR | + seções `hideout` (estação→nível) e `loadout` (`equipped` por slot, `stash`), com item spec (tpl/count/preset/mods/contents/loadedMag/chambered/ammo). |
| `modded/Server/InventoryBuilder.cs` | CRIAR | Constrói a árvore `Inventory.Items` (equipado/composto/stash), resolve preset, carrega mag+câmara, faz packing no stash, gera IDs únicos, valida integridade. |
| `modded/Server/HideoutBuilder.cs` | CRIAR | Seta `Hideout.Areas[].Level` por `HideoutAreas`. |
| `modded/Server/CustomClassesMod.cs` | MODIFICAR | Chamar Inventory/Hideout builders em `RegisterClass` (após skills). |
| `modded/Server/config/classes/*.jsonc` | CRIAR | As **10 classes reais** (geradas pelo script). Remover/!manter example/test. |
| `scripts/build-class-jsons.js` | CRIAR | Gera os 10 JSONs reusando recipes/anchors/skills/hideout do RZ (porta `build-profile-jsons.js`). |

## 5. Formato JSON (a validar) + stubs

### Formato de classe (o que o usuário edita)

```jsonc
{
  "name": "Fuzileiro",
  "baseEdition": "SPT Zero to hero",
  "description": "Assault rifleman.",
  "skills": { "Assault": 5, "MagDrills": 3 },

  // estação de hideout -> nível (nomes do enum HideoutAreas)
  "hideout": { "Generator": 1 },

  "loadout": {
    // equipado por slot (nomes do enum EquipmentSlots)
    "equipped": {
      "Headwear":   { "tpl": "<helmet_tpl>" },
      "ArmorVest":  { "tpl": "<armor_tpl>" },
      "TacticalVest": {
        "tpl": "<rig_tpl>",
        "contents": [                              // composto: itens dentro da grade
          { "tpl": "<mag_tpl>", "count": 3 },
          { "tpl": "<ammo_tpl>", "count": 120 }
        ]
      },
      "Backpack": { "tpl": "<backpack_tpl>", "contents": [ { "tpl": "<med_tpl>", "count": 2 } ] },
      "FirstPrimaryWeapon": {
        "preset": "<presetId_ou_weapon_tpl>",      // GetPreset(id) → fallback GetDefaultPreset(tpl)  (PA-01-04)
        "loadedMag": true,                          // encher o carregador inserido
        "chambered": true,                          // +1 na câmara
        "ammo": "<cartridge_tpl>"                    // OBRIGATÓRIO quando loadedMag/chambered  (PA-01-03)
      },
      "Holster": { "preset": "<pistol_tpl>", "loadedMag": true }
      // alternativa manual em vez de "preset":
      // "FirstPrimaryWeapon": { "tpl": "<weapon_tpl>", "mods": [ { "slotId": "mod_magazine", "tpl": "<mag>" }, ... ] }
    },
    // itens soltos no stash
    "stash": [
      { "tpl": "<money_tpl>", "count": 250000 },
      { "tpl": "<meds_tpl>", "count": 3 }
    ]
  }
}
```

### Stub — extensão do DTO

```csharp
// modded/Server/ClassDefinition.cs (+ campos)
[JsonPropertyName("hideout")]
public Dictionary<string, int>? Hideout { get; init; }   // station name (HideoutAreas) -> level

[JsonPropertyName("loadout")]
public Loadout? Loadout { get; init; }

public sealed record Loadout
{
    [JsonPropertyName("equipped")]
    public Dictionary<string, ItemSpec>? Equipped { get; init; }   // EquipmentSlots name -> item
    [JsonPropertyName("stash")]
    public List<ItemSpec>? Stash { get; init; }
}

public sealed record ItemSpec
{
    [JsonPropertyName("tpl")]      public string? Tpl { get; init; }
    [JsonPropertyName("preset")]   public string? Preset { get; init; }    // weapon tpl or preset id (default preset)
    [JsonPropertyName("count")]    public int Count { get; init; } = 1;
    [JsonPropertyName("ammo")]     public string? Ammo { get; init; }
    [JsonPropertyName("loadedMag")] public bool LoadedMag { get; init; }
    [JsonPropertyName("chambered")] public bool Chambered { get; init; }
    [JsonPropertyName("contents")] public List<ItemSpec>? Contents { get; init; }   // items inside a container
    [JsonPropertyName("mods")]     public List<ModSpec>? Mods { get; init; }        // manual tree
}

public sealed record ModSpec
{
    [JsonPropertyName("slotId")] public string? SlotId { get; init; }
    [JsonPropertyName("tpl")]    public string? Tpl { get; init; }
    [JsonPropertyName("mods")]   public List<ModSpec>? Mods { get; init; }
}
```

### Stub — builder (esqueleto; detalhes finos de inventário confirmados na implementação)

```csharp
// modded/Server/InventoryBuilder.cs
[Injectable]
public class InventoryBuilder(PresetHelper presetHelper, ItemHelper itemHelper, ICloner cloner, ISptLogger<InventoryBuilder> logger)
{
    // Adiciona o loadout da classe ao Character (equipado/composto/stash). Retorna nº de raízes adicionadas.
    public int Apply(PmcData character, Loadout loadout)
    {
        var items = character.Inventory.Items;            // lista flat
        var equipmentId = character.Inventory.Equipment;  // raiz de equipamento
        var stashId = character.Inventory.Stash;          // raiz do stash
        var added = 0;

        foreach (var (slot, spec) in loadout.Equipped ?? new())
        {
            // valida slot ∈ EquipmentSlots; resolve preset OU árvore manual; carrega mag/câmara;
            // re-raiz no equipmentId com SlotId=slot; gera IDs únicos; valida integridade.
            // ref: PresetHelper.GetDefaultPreset / GetPreset → Preset.Items (Globals.cs:4411)
            added += AddEquipped(items, equipmentId.Value, slot, spec);
        }
        foreach (var spec in loadout.Stash ?? new())
        {
            // packing na grade do stash (Location), stack-aware (Upd.StackObjectsCount)
            added += AddToStash(items, stashId.Value, spec);
        }
        return added;
    }

    // AddEquipped / AddToStash / preset-resolve / load-mag / chamber / grid-pack: TODO confirmar contra
    // BotWeaponGenerator (cartridges/patron_in_weapon) e o packing de stash (Location). Validador rejeita
    // slotId inválido, item que não cabe, preset inexistente — pula + log.
}
```

```csharp
// modded/Server/HideoutBuilder.cs — set station levels
foreach (var (stationName, level) in def.Hideout ?? new())
{
    if (!Enum.TryParse<HideoutAreas>(stationName, true, out var type) || !Enum.IsDefined(typeof(HideoutAreas), type))
    { logger.Warning($"unknown hideout station '{stationName}' — ignored"); continue; }
    var area = character.Hideout?.Areas?.FirstOrDefault(a => a.Type == type);   // ref: BotBase.cs:710/828
    if (area is not null) area.Level = Math.Max(area.Level ?? 0, level);          // Level é int? (PA-01-06)
}
```

## 6. Fluxo de dados

```
RegisterClass(def) [estende 002]
  → clone base → set skills (002)
  → InventoryBuilder.Apply(Character, def.Loadout):
      equipped[slot]: preset? PresetHelper.GetDefaultPreset(tpl).Items (clone, re-id, re-raiz no Equipment/slot)
                      manual? monta árvore por mods/slotId
                      loadedMag/chambered/ammo → cartridges + patron_in_weapon
      stash[]: packing na grade (Location) + stack-aware
  → HideoutBuilder: Hideout.Areas[type].Level = nível
  → templates[name] = sides
[criar perfil] CreateProfileService.ReplaceProfileInventoryIds re-id tudo (CreateProfileService.cs:94)
```

## 7. Riscos e dependências

- **Item grande** — implementar incremental (formato+DTO → equipado simples → preset/composto → mag/câmara → stash packing → hideout → script das 10 classes). Cada fatia testável.
- **Packing no stash (`Location`)** e **cartridges/chamber**: detalhes finos do inventário EFT — confirmar contra `BotWeaponGenerator`/`InventoryHelper` na implementação (marcado TODO no stub).
- **Mapeamento flat→equipado/composto das 10 classes**: design por classe (mesmo conjunto de itens do RZ; disposição nova). Feito no script + revisão por classe.
- **Capacidade do stash**: equipar libera o stash (vs RZ "tudo no stash") — reduz overflow; validador ainda loga excedente.
- **IDs**: únicos só no template (SPT re-id no profile).
- **Coexistência RZ** (clobber) — item 007; testar com RZ desabilitado.
- **Reuso**: `PresetHelper`/`ItemHelper`/`ICloner` do SPT; recipes/anchors/balance do RZ.

## 8. Checklist de implementação

- [x] Estender `ClassDefinition` (+ `hideout`, `loadout`/`ItemSpec`/`ModSpec`). **(fatia 1)**
- [x] `HideoutBuilder` (set Areas levels + Active/Constructing — PA-02-03) + chamar no `RegisterClass`. **(fatia 1)**
- [x] `InventoryBuilder`: equipado simples + slot-occupancy/subtree-removal (fatia 1) → preset + árvore manual (fatia 2) → mag carregado + câmara (fatia 3) → **contents + stash packing (`GridPacker`, Location + stack-aware, fatia 4)**. (Validador "item cabe" = via GridPacker overflow.)
- [x] `/compile-mod` por fatia (todas compilam 0 warn/err; DLL final 38.9 KB + 10 `.jsonc` instaladas).
- [x] `scripts/build-class-jsons.js` (+ `class-recipes.js`): gerou os 10 JSONs (recipes/anchors/skills/hideout do RZ; auto-categorização via items.json p/ placement). **(fatia 5)**
- [ ] Playtest (RZ desabilitado): as 10 classes no launcher; cada uma nasce vestida + arma montada com mag/câmara + stash + hideout; paridade de itens/skills com RZ. **(pendente — validação in-game)**
- [x] Corner cases tratados em código: tpl/slot/estação inválidos (try/catch + IsDefined), preset inexistente (IsPreset/HasPreset→warn), item não cabe/overflow (GridPacker loga + pula), slot já ocupado (substitui), câmara/ammo ausentes (guards). Validação final = playtest.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-07 | Spec técnica criada via `/create-technical-spec` (formato JSON: preset+manual, mag+câmara; builders de inventário/hideout; script p/ 10 classes) |
| 2026-06-07 | Review 01 aplicada (PA-01-01..07): GridPacker first-fit, re-id de preset, `ammo` obrigatório p/ loadedMag, fallback de `preset`, confirmar base, fixes de nullability, grids do contêiner |
