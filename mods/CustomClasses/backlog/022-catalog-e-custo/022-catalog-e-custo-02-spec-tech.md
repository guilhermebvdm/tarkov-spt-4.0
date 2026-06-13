# 022 — Catálogo de itens + custo (port RZ) — Spec técnica

**Mod:** CustomClasses
**Criado:** 2026-06-10
**Refs:** [01-spec](./022-catalog-e-custo-01-spec.md) · RZ: `mods/RZCustomProfiles/scripts/build-profile-jsons.js` (SKILL_MULTS/weightedCost/loadoutTotalRub) · `mods/RZCustomProfiles/backlog/002-custom-profiles/002-custom-profiles-00-multiplicadores.md`

## Arquivos novos (sem tocar em território dos itens 020/021)

| Arquivo | Papel |
|---|---|
| `modded/Server/SkillWeights.cs` | Tabela estática de pesos (port RZ) + derivadas SE + fallback por categoria (`ResolveWeight`) |
| `modded/Server/CatalogService.cs` | Catálogo read-only sobre `DatabaseService` (busca, nomes, preços, categorias, presets, roupas, editions) |
| `modded/Server/CostService.cs` | `ComputeSkillCost` + `ComputeLoadoutCost` (port RZ) |
| `scripts/check-skill-costs.mjs` | Validação de paridade dos pesos/fórmula sem `dotnet build` |

Grafo de dependência: `CostService → CatalogService → DatabaseService/ItemHelper/LocaleService` (sem ciclo; preço e resolução de preset centralizados no `CatalogService`). Ambos `[Injectable(InjectionType.Singleton)]` — todas as dependências SPT também são Singleton (verificado no spt-source), sem captive dependency.

## Decisão de preço ₽

**Primário: tabela de flea efetiva do server — `DatabaseService.GetPrices()`** (acessada via `ItemHelper.GetDynamicItemPrice`, ref `ItemHelper.cs:470`). É o análogo server-side do avg24h usado pelo RZ e, no momento da consulta (request-time, pós-boot), **já reflete os overrides de flea deste repo** (override aditivo + piso trader + teto unreasonable — ver memória `project_flea_price_formula`).

**Fallback: handbook — `ItemHelper.GetStaticItemPrice`** (ref `ItemHelper.cs:454`) para item fora da tabela de flea (ex.: itens banidos do flea, alguns itens de mods).

**Moeda:** valor facial — `GetStaticItemPrice` (rublo = 1 ₽; USD/EUR = conversão do handbook), fonte `currency`. Nunca flea para moeda.

**Sem preço algum:** 0 + fonte `missing` + flag `missingPrice` na linha + warning agregado — nunca 0 silencioso.

Nota: a ordem é **invertida** vs `ItemHelper.GetItemPrice` (que é handbook-first). Por isso o `CatalogService.GetPrice` implementa a ordem flea→handbook explicitamente em vez de delegar.

## Tabela de pesos

- **31 explícitas:** port 1:1 do `SKILL_MULTS` do RZ (BASELINE 15, clamp [0.25, 5.00], 2 casas). Fonte: personagem-referência lvl 43.
- **4 derivadas (Skills-Extended)** — fórmula RZ aplicada à mecânica de XP do source vendored do SE (`SkillsConfig.json` default + `SkillClassCtorPatch` factors + patches de evento). Progresso de skill é linear (100/nível), então nível esperado ≈ ações na carreira × XP/ação × factor ÷ 100. Premissa de carreira: ~400 raids até lvl 43 (mesmo referencial da tabela RZ).

| Skill | Mecânica de XP (SE) | XP efetivo/ação | Nível esperado @ lvl 43 | Peso |
|---|---|---:|---:|---:|
| `FirstAid` | 6.25 XP por uso de item de cura (MedEffect: medkit/bandagem/tala — `OnGameStarted.ApplyMedicalXp`) × factor 0.35 | ≈2.19 | ≈16 (cura ~2×/raid → ~800 usos; cadência tipo CovertMovement) | **0.94** |
| `FieldMedicine` | 5.50 XP por uso de stim/painkiller (`Stimulator`/`PainKiller`) × factor 0.35 | ≈1.93 | ≈8 (~1 uso/raid → ~400 usos) | **1.88** |
| `UsecNegotiations` | 2.25 XP por kill de PMC BEAR (`OnEnemyKillPatch`, FactionLocked USEC) × factor 1.0 | 2.25 | ≈6 (~300 kills de facção oposta na carreira) | **2.50** |
| `BearRawpower` | 2.25 XP por kill de PMC USEC (FactionLocked BEAR) — simétrica | 2.25 | ≈6 | **2.50** |

- **Fallback por categoria** (`SkillWeightOrigin.CategoryFallback`): skill com categoria conhecida mas sem peso → **mediana dos pesos explícitos da categoria**, calculada em runtime de uma única fonte (a própria tabela): `Ph=1.00, M=0.60, C=1.50, P=0.94`. O mapa de categorias inclui as skills mortas categorizadas pelo doc do RZ (SMG/LMG/… → C; Sniping/Lockpicking/… → P) para um eventual revival cair num fallback são.
- **Fora de qualquer mapa** (`UnmappedFallback`): 1.00 + warning (skills Trading mortas, faction skills não-SE, Misc/DrawMaster/etc.). **Nunca 0 silencioso.**
- Categoria das SE: FirstAid/FieldMedicine → `P` (aba Practical); UsecNegotiations/BearRawpower → `S` (special/facção — fora da regra de cobertura Ph/M/C/P).

## Contratos (records, serializáveis em camelCase via `JsonPropertyName`)

```text
CostService.ComputeSkillCost(ClassDefinition) → SkillCostBreakdown
  { skills: [ { skill, level, weight, origin: Explicit|Derived|CategoryFallback|UnmappedFallback, cost } ],
    total, withinBudget (28–32), warnings[] }
  Warnings (não-bloqueantes, só quando a classe tem pontos): budget fora de [28,32],
  categorias Ph/M/C/P sem cobertura, >6 skills com pontos, nível > teto 10. Classe sem skills
  (Peladão) → total 0 sem ruído de warnings.

CostService.ComputeLoadoutCost(ClassDefinition) → LoadoutCostBreakdown
  { items: [ { tpl, name, context: equipped:<slot>|stash|contents|ammo, qty, unitPrice,
               priceSource: flea|handbook|currency|missing, subtotal, missingPrice } ],
    totalRub, warnings[] }

CatalogService:
  Search(query, parentCategoryId?, limit=50) → [ { tpl, name, shortName, price, priceSource, categoryId } ]
  GetItemName(tpl, lang="en"|"pt")           // "pt" → locale EFT "po"; fallback en → _name → tpl
  GetPrice(tpl) → (price, source)
  GetCategories(lang) → [ { id, parentId, name } ]   // nome = locale key = id da categoria
  GetPresetsFor(tpl) → [ { id, name, itemCount, isDefault, isPremium } ]
  GetClothing("Usec"|"Bear") → [ { id, name, slot: upper|lower } ]
  GetEditionKeys() → [string]                // todas as editions; caller filtra classes (021 é dono do registry)
```

## Paridade com o InventoryBuilder (custo = o que o builder spawna)

A resolução de preset foi **duplicada de propósito** (métodos privados de `InventoryBuilder.cs`, arquivo de propriedade do item 021 — não editar), com comentário "mirrors InventoryBuilder.X" em cada método:

- `ResolveDefaultPreset` ≡ `ResolvePreset` (encyclopedia → primeiro match; aceita id de preset).
- `ResolvePremiumPreset` ≡ idem (mais kitado, evita térmica/NV — CR-02-01).
- `ResolveStashPreset` ≡ idem (menor preset com óptica real → default) — usado para stash/contents.
- Equipado com `tpl` cru → auto-completa com preset default (≡ `BuildItemTree`); stash/contents → stash preset (≡ `PackSpecsIntoGrids`).
- `loadedMag`: enche o carregador do preset até `_props.Cartridges[0]._max_count` — **pulado** se o preset já traz cartuchos (≡ CR-01-03; já contados como itens do preset). `chambered`: +1.
- Não modelado no custo (delta pequeno, documentado): a mira mínima adicionada por `EnsureMinimumOptic` (CR-02) — o builder pode adicionar um red dot barato que não entra no total.

## Validação de paridade sem build

`scripts/check-skill-costs.mjs` (node puro, sem deps): lê os 11 `.jsonc`, aplica a MESMA tabela/fórmula (pesos copiados — manter em sync com `SkillWeights.cs`) e compara com [28, 32]. Os `skillOverrides` do RZ são idênticos aos `skills` das 10 classes portadas, então custo igual ⇒ paridade com os valores do RZ por construção. Saída completa no as-built.
