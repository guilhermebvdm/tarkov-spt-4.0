# CustomClasses — Schema de classe (referência canônica)

> **Data:** 2026-06-09<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [exampleClass.jsonc](../modded/Server/config/classes/_docs/exampleClass.jsonc), [spt4-items-inventory-hideout.md](../../../docs/technical/spt4-items-inventory-hideout.md)<br>

---

Referência completa do **JSON de classe** do mod CustomClasses (1 arquivo = 1 classe = 1 edition no launcher). A **verdade primária é o código** (`ClassDefinition.cs` + loader + builders); esta doc o espelha. Refs de validação são **por símbolo/método** (nunca por linha — o item 021 refatora esses arquivos).

## 1. Onde os arquivos vivem e como são carregados

- Pasta: `config/classes/` dentro da pasta do mod no servidor (`user/mods/CustomClasses/config/classes/`). No repo: `mods/CustomClasses/modded/Server/config/classes/`.
- Extensões aceitas: `*.json` e `*.jsonc` — **comentários são tolerados** (JSONC).
- Leitura **não-recursiva**: só o topo de `config/classes/`; subpastas (ex.: `_docs/`, rascunhos) são ignoradas (`CustomClassesMod.OnLoad`).
- Adicionar classe = soltar um arquivo + reiniciar o servidor (**sem recompilar**) — OU criar/salvar pelo **editor web**, que aplica a quente (ver §6).
- Carga em `PostDBModLoader + 1`: cada arquivo é desserializado em `ClassDefinition`, validado e registrado como edition em `DatabaseService.GetProfileTemplates()`. Arquivo inválido é **pulado com log claro, sem derrubar as demais classes** (try/catch por arquivo em `CustomClassesMod.OnLoad`). No fim, log-resumo: `Loaded N class(es), skipped M`.

## 2. Campos do JSON de classe

DTO: `ClassDefinition` (`modded/Server/ClassDefinition.cs`).

| Campo (JSON) | Tipo | Obrigatório | Default | O que controla |
|---|---|---|---|---|
| `name` | string | **sim** | — | Chave única da edition no launcher + label (identificador PT). Trimado no load. |
| `enabled` | bool | não | `true` | `false` → classe não registra (log info), sem apagar o arquivo. |
| `baseEdition` | string | não | `"SPT Zero to hero"` | Edition **vanilla** clonada (deep clone) como base do perfil. O default tem stash vazio — a classe controla os próprios itens. Não apontar para outra classe custom (dependente de ordem de load). |
| `displayName` | objeto `{ "en", "pt" }` | não | `name` | Nome localizado da classe **in-game** (menu/tela de skills), resolvido pelo **idioma do client EFT**. Não muda a chave da edition (sempre `name`). |
| `description` | string **ou** objeto `{ "en", "pt" }` | não | `name` | Descrição no launcher. String = forma legada (tratada como `en`/fallback). Resolvida no load pela **locale do servidor** (`pt*` → pt, senão en; vazio → `name`). |
| `iconFile` | string | não | — | Nome do PNG do "badge" da classe, em `BepInEx/plugins/CustomClasses/icons/` (só nome de arquivo, sem path). Ausente/arquivo faltando → só o nome (colorido) aparece. |
| `nameColor` | string | não | — | Cor hex `"#RRGGBB"` do nome da classe na UI. Ausente/inválida → cor default (validação no client). |
| `skills` | objeto `nome → int` | não | — | Nível inicial por skill (`SkillTypes`), **clampado em 0..51**. Nome desconhecido → warning + ignorado. |
| `skillMultipliers` | objeto `nome → number` | não | — | Fator de ganho de XP por skill (`1` = vanilla, `1.5` = +50%, `0.5` = −50%), **clampado em ≥ 0**. Aplicado **no client** (servido por edition via router). |
| `hideout` | objeto `estação → int` | não | — | Nível inicial por estação do hideout (`HideoutAreas`). Estação fica **construída/ativa** (não "em construção"). |
| `loadout` | objeto `{ equipped, stash }` | não | — | Itens iniciais: equipados no personagem + soltos no stash. Ver §3. |
| `outfit` | objeto `{ usec, bear }` | não | — | Roupas por facção: `usec`/`bear`, cada um `{ upper, lower }`. Ver §4.4. |

### 2.1 `name`, `displayName`, `description` (i18n)

- `name` é **a chave**: launcher edition key + identificador interno (registries de multiplicador/identidade visual são indexados por ele). É também o label no launcher.
- `displayName` e `description` usam `LocalizedText` (`LocalizedText.cs`): no JSON aceitam **string** (legado = `en`/fallback) **ou objeto** `{ "en": "...", "pt": "..." }` (`LocalizedTextConverter.Read`).
- Resolução (`LocalizedText.Resolve`): locale `pt*` → `pt` (vazio → `en`); outra locale → `en` (vazio → `pt`); tudo vazio → null.
- **Momentos diferentes:** `description` é resolvida **no load, pela locale do servidor** (`CustomClassesMod.RegisterClass` via `LocaleService.GetDesiredServerLocale`) e gravada como texto literal na edition (não há API pública para registrar locale key). `displayName` vai cru (`en`+`pt`) para o `ClassVisualRegistry` e é resolvido **no client, pelo idioma do EFT**.

```jsonc
"name": "Caçador",
"displayName": { "en": "Hunter", "pt": "Caçador" },
"description": { "en": "Sniper. Patient and precise.", "pt": "Sniper. Paciente e preciso." }
// forma legada ainda aceita:
// "description": "English-only fallback"
```

### 2.2 `skills`

- Chave = nome do enum `SkillTypes` (case-insensitive). Exemplos: `Endurance`, `Strength`, `Sniper`, `Perception`, `Attention`, `CovertMovement`, `Search`, `Surgery`, `HideoutManagement`, `Crafting`…
- Validação em `CustomClassesMod.ApplySkills`: `Enum.TryParse` (ignoreCase) **+ `Enum.IsDefined`** (rejeita valor numérico/fantasma). Desconhecida → warning + ignorada (não invalida a classe).
- Nível clampado `Math.Clamp(level, 0, 51)`; progresso gravado = `nível × 100` (5100 = level 51).
- Aplicado **nos dois lados** (USEC e BEAR). Skill que não existe no template base é **adicionada** com `LastAccess = agora` (sanidade de fadiga/decay). Se um lado aplicar 0 skills com skills configuradas → warning (`CustomClassesMod.RegisterClass`).

### 2.3 `skillMultipliers`

- Chave = nome de `SkillTypes` (case-insensitive; o nome é **normalizado** via enum antes de registrar). Desconhecida → warning + ignorada (`CustomClassesMod.RegisterClass`).
- Fator clampado em `≥ 0` (negativo vira 0). `1` = vanilla; `2` = +100% (buff, seta/borda verde na UI); `0.5` = −50% (debuff, vermelho).
- Registrado em `SkillMultiplierRegistry.Set(name, …)` (singleton, indexado pela edition = `name`) e servido ao client pelo `SkillMultipliersRouter`. **O efeito é client-side** (multiplicador de XP aplicado pelo plugin BepInEx).
- **Skills-Extended (soft):** `FirstAid`, `FieldMedicine`, `BearRawpower`, `UsecNegotiations` são membros vanilla de `SkillTypes`, mas só ganham XP com o mod Skills-Extended (`com.cj.SkillsExtended`) instalado. Detecção em `SkillsExtendedCompat.IsPresent` (lista em `SkillsExtendedCompat.Skills`). Sem o SE: o multiplicador é registrado mesmo assim (**inócuo** — a skill nunca ganha XP) e o servidor loga um warning.

### 2.4 `hideout`

- Chave = nome do enum `HideoutAreas` (case-insensitive): `Vents`, `Security`, `WaterCloset`, `Stash`, `Generator`, `Heating`, `WaterCollector`, `MedStation`, `Kitchen`, `RestSpace`, `Workbench`, `IntelligenceCenter`, `ShootingRange`, `Library`, `ScavCase`, `Illumination`, `PlaceOfFame`, `AirFilteringUnit`, `SolarPower`, `BoozeGenerator`, `BitcoinFarm`, `ChristmasIllumination`, `EmergencyWall`, `Gym`, `WeaponStand`, `WeaponStandSecondary`, `EquipmentPresetsStand`, `CircleOfCultists`.
- Semântica (`HideoutBuilder.Apply`): nível `≤ 0` é ignorado silenciosamente; estação desconhecida ou ausente no template base → warning + ignorada; nível final = `max(nível do base, nível da classe)` (nunca rebaixa); a estação fica **`Active = true`, `Constructing = false`, `CompleteTime = 0`** (construída e ativa, não "em construção"). Aplicado nos dois lados.

### 2.5 `iconFile` / `nameColor` (identidade visual)

- Registrados em `ClassVisualRegistry.Set` **somente no registro efetivo da classe** (após todas as validações) — o router só expõe identidade de classes realmente registradas (`CustomClassesMod.RegisterClass`).
- Toda classe registrada entra no registry, mesmo sem ícone/cor — ele também serve de fonte de "esta edition é classe do mod?".
- Validações de existência do PNG e de formato da cor acontecem **no client** (ausente/inválido → visual default; nada falha no servidor). Trocar o PNG não exige recompilar.

## 3. `loadout` — itens iniciais

```jsonc
"loadout": {
  "equipped": { "<EquipmentSlot>": <ItemSpec>, ... },   // 1 item por slot do personagem
  "stash":    [ <ItemSpec>, ... ]                       // itens soltos no stash (posição OPCIONAL via x/y/rotated)
}
```

> **Políticas baseline-v2 (2026-07-06, decisões do usuário):** o extrator (`scripts/extract-from-profile.mjs`)
> emite `SecuredContainer` = **Alpha** para todas as classes extraídas e aplica overrides de `Pockets` por
> classe (saqueador → Pockets 1x4 TUE/Unheard); `Scabbard` é **copiado do perfil-fonte**; rublos são
> **normalizados** (stacks do perfil descartados; a classe nasce com o valor fixo — default **300k**); itens
> `DEFAULT_EXCLUDE` (DSP transmitter) nunca são extraídos. Essas políticas vivem no extrator para sobreviver
> a re-extrações. Exceção: **Peladão** não é extraído e usa `"SecuredContainer": { "remove": true }` —
> nasce **SEM** secure container (a base daria um Alpha).

Slots válidos em `equipped` (enum `EquipmentSlots`, case-insensitive): `Headwear`, `Earpiece`, `FaceCover`, `ArmorVest`, `Eyewear`, `ArmBand`, `TacticalVest`, `Pockets`, `Backpack`, `SecuredContainer`, `FirstPrimaryWeapon`, `SecondPrimaryWeapon`, `Holster`, `Scabbard`. Slot desconhecido → warning + ignorado (`InventoryBuilder.Apply`).

### 3.1 `ItemSpec`

| Campo | Tipo | Default | Significado |
|---|---|---|---|
| `tpl` | string (MongoId) | — | Template do item. Sozinho num slot equipado: se o item tem **preset default** (arma, armadura/capacete/rig com placas), a árvore completa é montada automaticamente; senão, item simples. |
| `preset` | string (MongoId) | — | **Id de preset** OU **tpl de arma**. Id → aquele preset exato; tpl → preset default da arma (o que tem `Encyclopedia`), senão o primeiro. |
| `premium` | bool | `false` | Com `preset` = tpl de arma: usa o preset **mais kitado** (mais itens), preferindo builds **sem óptica térmica/NV**. Com `preset` = id exato, é irrelevante. |
| `count` | int | `1` | Quantidade. **Ignorado em slots equipados** (log debug). Em `stash`/`contents`: honrado, stack-aware. |
| `ammo` | string (MongoId) | — | Tpl do cartucho. **Obrigatório** quando `loadedMag` e/ou `chambered` é `true`. |
| `loadedMag` | bool | `false` | Enche o carregador (`mod_magazine`) com `ammo` até a capacidade (`ItemHelper.FillMagazineWithCartridge`). |
| `chambered` | bool | `false` | Põe 1 cartucho de `ammo` na câmara (slot real lido do template da arma — `patron_in_weapon`/variantes). |
| `contents` | `ItemSpec[]` | — | Itens **dentro** do contêiner (rig/mochila — equipado OU no stash), empacotados nas grades dele; recursivo. |
| `mods` | `ModSpec[]` | — | Árvore **manual** de mods (alternativa a `preset`). Exige `tpl` raiz. |
| `x`, `y` | int? | — | (item 038) Posição EXPLÍCITA na grade do stash (célula superior-esquerda). Se não couber, cai no auto-pack (nunca dropa). Só no nível do `stash` — em `contents` é ignorado pelo fluxo atual. |
| `rotated` | bool? | `false` | (item 038) Item rotacionado (vertical) na posição pinada. |
| `remove` | bool | `false` | (baseline-v2) **Só em slot equipado:** REMOVE o ocupante herdado da base edition (+ subárvore) sem equipar nada — ex.: Peladão sem secure container. Demais campos são ignorados. |

Precedência na montagem (`InventoryBuilder.BuildItemTree`): `preset` > `mods` > `tpl`. Sem nenhum dos três → slot pulado com warning.

### 3.2 `ModSpec` (árvore manual, recursiva)

| Campo | Tipo | Significado |
|---|---|---|
| `slotId` | string | Slot do EFT onde o mod encaixa no pai (ex.: `mod_magazine`, `mod_stock`, `mod_scope`, `mod_muzzle`). |
| `tpl` | string (MongoId) | Template do mod. |
| `mods` | `ModSpec[]` | Sub-mods (recursivo). |

Entrada sem `tpl` ou `slotId` → warning + ignorada (`InventoryBuilder.AddMods`). Não há validação de compatibilidade slot×item na árvore manual — o autor é responsável.

### 3.3 Exemplos de cada forma

**(a) tpl simples** — item sem preset (mochila, meds) entra "cru"; item com preset default (armadura com placas, arma) entra **montado** automaticamente:

```jsonc
"ArmorVest": { "tpl": "5df8a2ca86f7740bfe6df777" }
```

**(b) preset (default)** — `preset` com tpl de arma resolve o preset default dela:

```jsonc
"Holster": { "preset": "5448bd6b4bdc2dfc2f8b4569" }
```

**(c) preset premium** — build mais kitada da arma (sem térmica/NV quando houver alternativa):

```jsonc
"FirstPrimaryWeapon": { "preset": "55801eed4bdc2d89578b4588", "premium": true }
```

**(d) árvore manual de mods** — `tpl` raiz + `mods` recursivo por `slotId`:

```jsonc
"FirstPrimaryWeapon": {
  "tpl": "5644bd2b4bdc2d3b4c8b4572",
  "mods": [
    { "slotId": "mod_magazine", "tpl": "564ca99c4bdc2d16268b4589" },
    {
      "slotId": "mod_handguard",
      "tpl": "5648b1504bdc2d9d488b4584",
      "mods": [ { "slotId": "mod_tactical_001", "tpl": "57fd23e32459772d0805bcf1" } ]
    }
  ]
}
```

**(e) contêiner com contents** — rig/mochila equipado com itens empacotados nas grades dele:

```jsonc
"TacticalVest": {
  "tpl": "5c0e746986f7741453628fe5",
  "contents": [
    { "tpl": "5887431f2459777e1612938f", "count": 60 },
    { "tpl": "5751a25924597722c463c472", "count": 2 }
  ]
}
```

**(f) mag carregado + câmara** — `ammo` obrigatório:

```jsonc
"FirstPrimaryWeapon": {
  "preset": "55801eed4bdc2d89578b4588",
  "premium": true,
  "loadedMag": true,
  "chambered": true,
  "ammo": "5887431f2459777e1612938f"
}
```

**(g) stash** — lista plana; cada entrada aceita a **mesma semântica** de um slot equipado (`preset`/`premium`/`mods`/`ammo`/`contents`), além de `count`:

```jsonc
"stash": [
  { "tpl": "5449016a4bdc2d6f028b456f", "count": 100000 },   // roubles (stack-aware)
  { "tpl": "55801eed4bdc2d89578b4588", "count": 1 },         // arma: entra MONTADA (stash-preset auto)
  { "preset": "55801eed4bdc2d89578b4588", "premium": true,   // preset explícito + mag carregado
    "loadedMag": true, "ammo": "5887431f2459777e1612938f" }
]
```

## 4. Semântica dos builders

### 4.1 `InventoryBuilder` — equipados

- **Substituição de slot (não merge):** antes de equipar, o ocupante atual do slot no template base é removido **com toda a subárvore** (`InventoryBuilder.RemoveSlotOccupant` / `RemoveItemAndChildren`).
- **Presets clonados e re-identificados:** os itens do preset são deep-clonados e recebem ids novos preservando os links pai-filho (`InventoryBuilder.ClonePresetTree`); a raiz é re-raizada no slot (`RebaseClonedPreset`).
- **Resolução de preset** (`InventoryBuilder.ResolvePreset` / `ResolvePremiumPreset`): lê direto de `Globals.ItemPresets` (não usa `PresetHelper` — o cache dele ainda está vazio em `PostDBModLoader+1`). Default = preset com `Encyclopedia`; premium = mais itens, preferindo sem térmica/NV.
- **Mira mínima** (`InventoryBuilder.EnsureMinimumOptic`): arma equipada (ou montada no stash) **sem óptica real** ganha uma mira simples (red dot > assault scope > resto; nunca térmica/NV) no 1º slot compatível — direta ou via mount (2 níveis), sempre validada pelo filter de `_props.Slots`. Sem slot compatível → mantém mira de ferro.
- **Munição** (`InventoryBuilder.LoadAmmo`): `loadedMag` enche o `mod_magazine` da árvore (se o preset já trouxe cartuchos, não mexe); **carregador AVULSO** (raiz da linha de stash/contents é o próprio magazine — baseline-v2 2026-07-06) também é enchido; `chambered` cria 1 cartucho no slot de câmara declarado no template da arma. Sem `mod_magazine`/câmara → warning, segue sem.
- **Falha isolada por slot:** exceção ao montar um item (ex.: tpl malformado) pula **só aquele slot**, com warning — a classe e os demais slots seguem (`InventoryBuilder.Apply`).
- Tudo é aplicado **nos dois lados** (USEC e BEAR), a partir do mesmo `loadout`.

### 4.2 `GridPacker` — stash e contents (posicionamento em runtime)

- Posição é **opt-in por item** (`x`/`y`/`rotated`, item 038): specs com coordenada são colocados PRIMEIRO (`GridPacker.TryPlaceAt`) e caem no auto-pack se a célula não couber — nunca dropam. Sem coordenada, o posicionamento é 100% runtime (`InventoryBuilder.PackSpecsIntoGrids` + `GridPacker.Place`). Desde a baseline-v2 o extrator PINA a posição do nível do stash (espelho do perfil-fonte); `contents` seguem auto-pack.
- Algoritmo: **first-fit com rotação** — varre as grades do contêiner na ordem, célula a célula; tenta o item sem rotação e depois rotacionado; primeira posição livre ganha. A dimensão usada é a **real do item montado** (`InventoryHelper.GetItemSize`, considera `ExtraSize` dos mods).
- **Stack-aware:** item simples com `StackMaxSize > 1` (munição, dinheiro) é dividido em stacks de até o máximo; cada stack ocupa uma célula/posição.
- Entradas de `stash`/`contents` honram a **mesma semântica dos slots equipados** (CR-EP-01): `preset` explícito (com `premium`), árvore manual (`mods`), `ammo`/`loadedMag`/`chambered` e `contents` **recursivo** (empacotado nas grades do item colocado). Sem `preset`/`mods`, o `tpl` auto-completa com o **stash-preset** (`InventoryBuilder.ResolveStashPreset` — prefere o **menor** preset que já tenha óptica real; senão o default) ou vira item simples; armas montadas também passam por `EnsureMinimumOptic`. `count > 1` em árvore composta = N árvores montadas.
- **Sem espaço** na grade → warning e as unidades restantes daquele spec são puladas (a classe segue). Contêiner sem grades ou stash não encontrado → warning + itens pulados.

### 4.3 `HideoutBuilder`

Ver §2.4 — estação validada por enum + presença no base; nível nunca rebaixa; marca construída/ativa (`HideoutBuilder.Apply`).

### 4.4 `OutfitBuilder`

- Estrutura: `outfit.usec` / `outfit.bear`, cada um com `upper` (camisa/jaqueta) e `lower` (calça). Valores = **ids de customization item** (catálogo: `scripts/suits-catalog.json`).
- `upper` seta `Customization.Body` **+ `Hands`** (quando a peça traz); `lower` seta `Customization.Feet`. **Head e Voice não são controláveis** — o jogador escolhe na criação do perfil (`OutfitBuilder.Apply`).
- **Dois padrões de peça** (`OutfitBuilder.ApplyPiece`): (a) **vanilla/suite** — `_props.Body`/`_props.Feet` referenciam OUTRA customization (a malha); (b) **"aparência direta"** (ex.: mod AllTheClothes) — `_props.Body`/`Feet` nulos, mas `_props.BodyPart == "Body"`/`"Feet"` → a própria peça é a malha (usa o próprio id). Peça que não casa com nenhum padrão para o lado pedido (upper sem Body/BodyPart=Body etc.) → warning + pulada.
- **Validação de facção:** `_props.Side` contendo `"Usec"`/`"Bear"` restringe; peça da facção errada → warning + pulada. `Side` nulo/vazio = sem restrição (aplica nos dois lados — lenient).
- A peça aplicada é adicionada a `Suits` do lado → vira **OBTAINED** na criação do perfil (`AddSuitsToProfile` do SPT).
- Falha isolada por peça: id malformado, inexistente ou sem `_props` → warning + pula **só a peça**.

## 5. Regras de validação do loader (por símbolo)

| # | Regra | Efeito | Símbolo |
|---|---|---|---|
| 1 | JSON ilegível / exceção no parse ou registro | arquivo pulado, erro logado | `CustomClassesMod.OnLoad` (try/catch por arquivo) |
| 2 | `name` ausente/em branco | arquivo pulado | `CustomClassesMod.OnLoad` |
| 3 | `enabled: false` | classe não registra (log **info**, conta como skipped) | `CustomClassesMod.OnLoad` |
| 4 | `name`/`baseEdition` com espaços acidentais | trimados | `CustomClassesMod.RegisterClass` |
| 5 | Colisão de edition (vanilla, outro mod ou arquivo duplicado) | classe pulada com warning — **nunca sobrescreve** | `CustomClassesMod.RegisterClass` |
| 6 | `baseEdition` inexistente | classe pulada com erro + lista das editions disponíveis | `CustomClassesMod.RegisterClass` |
| 7 | Clone do base retorna null | classe pulada | `CustomClassesMod.RegisterClass` |
| 8 | Skill desconhecida em `skills` | warning + skill ignorada | `CustomClassesMod.ApplySkills` (`Enum.TryParse` ignoreCase + `Enum.IsDefined`) |
| 9 | Nível de skill fora de 0..51 | clampado (progresso = nível×100) | `CustomClassesMod.ApplySkills` |
| 10 | Skill nova (não existe no base) | adicionada com `LastAccess` = agora | `CustomClassesMod.ApplySkills` |
| 11 | Um lado aplicou 0 skills com skills configuradas | warning (classe segue) | `CustomClassesMod.RegisterClass` |
| 12 | Skill desconhecida em `skillMultipliers` | warning + ignorada; nome normalizado via enum | `CustomClassesMod.RegisterClass` |
| 13 | Fator de multiplicador negativo | clampado em 0 | `CustomClassesMod.RegisterClass` |
| 14 | Multiplicador de skill do SE sem o SE instalado | warning; registrado mesmo assim (inócuo) | `CustomClassesMod.RegisterClass` + `SkillsExtendedCompat.Skills` |
| 15 | Slot de equipamento desconhecido | warning + slot ignorado | `InventoryBuilder.Apply` |
| 16 | Exceção ao montar item de um slot (tpl malformado etc.) | pula **só o slot**, warning | `InventoryBuilder.Apply` |
| 17 | `count > 1` em slot equipado | ignorado (log debug) | `InventoryBuilder.Apply` |
| 18 | `preset` não encontrado | slot/item pulado com warning | `InventoryBuilder.BuildItemTree` / `PackSpecsIntoGrids` |
| 19 | `mods` sem `tpl` raiz | slot/item pulado com warning | `InventoryBuilder.BuildItemTree` / `PackSpecsIntoGrids` |
| 20 | Sem `tpl`/`preset`/`mods` | slot pulado com warning | `InventoryBuilder.BuildItemTree` |
| 21 | `loadedMag`/`chambered` sem `ammo` | munição ignorada, warning | `InventoryBuilder.LoadAmmo` |
| 22 | Árvore sem `mod_magazine` / arma sem câmara | warning, segue sem mag/câmara | `InventoryBuilder.LoadAmmo` |
| 23 | `ModSpec` sem `tpl` ou `slotId` | entrada ignorada, warning | `InventoryBuilder.AddMods` |
| 24 | Entrada de stash/contents sem `tpl`/`preset`/`mods` | item pulado, warning | `InventoryBuilder.PackSpecsIntoGrids` |
| 25 | Sem espaço na grade / contêiner sem grades / stash ausente | unidades restantes puladas, warning | `InventoryBuilder.PackSpecsIntoGrids` / `InventoryBuilder.Apply` |
| 26 | Base sem `Inventory.Equipment/Items` ou sem `Hideout.Areas` | loadout/hideout pulado, warning | `InventoryBuilder.Apply` / `HideoutBuilder.Apply` |
| 27 | Estação de hideout desconhecida / ausente no base / nível ≤ 0 | ignorada (warning nas duas primeiras) | `HideoutBuilder.Apply` |
| 28 | Roupa com id malformado / inexistente / sem `_props` | peça pulada, warning | `OutfitBuilder.ApplyPiece` |
| 29 | Roupa do tipo errado (upper sem Body/BodyPart=Body; idem lower/Feet) | peça pulada, warning | `OutfitBuilder.ApplyPiece` |
| 30 | Roupa da facção errada (`_props.Side`) | peça pulada, warning | `OutfitBuilder.ApplyPiece` |
| 31 | Identidade visual só de classe registrada | `ClassVisualRegistry.Set` chamado junto do registro efetivo | `CustomClassesMod.RegisterClass` |

**Princípio geral:** falha granular — campo inválido derruba só o campo, item inválido só o item/slot/peça, classe inválida só a classe. O servidor nunca deixa de subir por causa de um JSON de classe.

## 6. Limites conhecidos

- **Skills-Extended:** `FirstAid`, `FieldMedicine`, `BearRawpower`, `UsecNegotiations` só ganham XP com o SE instalado (soft-detect por GUID `com.cj.SkillsExtended` em `SkillsExtendedCompat.IsPresent`). Sem o SE, multiplicadores dessas skills são registrados mas **inócuos** (warning no log).
- **`enabled: false`** desliga a classe sem apagar o arquivo (info no log, conta em "skipped"). Perfis já criados com a edition não são afetados — a edition só some da lista do launcher para perfis novos.
- **Hot-apply só pelo editor web:** o boot carrega os arquivos uma vez; um **save pelo editor** aplica a quente (`ClassRegistrar.Commit` substitui a edition; `enabled: false` hot-remove). Arquivo editado/solto **à mão** no install NÃO registra sozinho — só no próximo reinício do servidor ou num save daquele arquivo pelo editor. Hot-apply vale para **perfis novos**; perfis existentes e o client aberto não mudam.
- **`baseEdition` deve ser vanilla:** apontar para outra classe custom funciona ou não conforme a ordem de load — não suportado.
- **Equipped não suporta `count`** (1 item por slot; log debug). Stash/contents honram a semântica completa do `ItemSpec` (preset/mods/ammo/contents — ver §4.2, CR-EP-01).
- **Árvore manual (`mods`) não valida compatibilidade** slot×item — erro do autor produz item quebrado in-game.
- A pasta `_docs/` (e qualquer subpasta) **não é carregada** — o `exampleClass.jsonc` é documentação viva, não uma classe ativa.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-06-09 | Guilherme | Criação (item 018) — schema completo, builders, validação por símbolo, limites. |
| 2026-06-10 | Claude (apply-review) | CR-EP-01: stash/contents honram a semântica completa do `ItemSpec` (§3.1/§3.3g/§4.2/§5/§6). CR-EP-11: §1/§6 — hot-apply pelo editor web; edição à mão segue exigindo restart. |
