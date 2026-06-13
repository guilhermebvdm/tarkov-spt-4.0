# 022 — Catálogo de itens + custo (port RZ) — As-built

**Mod:** CustomClasses
**Data:** 2026-06-10
**Refs:** [01-spec](./022-catalog-e-custo-01-spec.md) · [02-spec-tech](./022-catalog-e-custo-02-spec-tech.md)

## Arquivos entregues

| Arquivo | Conteúdo |
|---|---|
| `modded/Server/SkillWeights.cs` | `SkillWeightOrigin` (Explicit/Derived/CategoryFallback/UnmappedFallback) + `SkillWeights` estático: 31 pesos RZ, 4 derivados SE (racional por skill em comentário), categorias, medianas por categoria computadas em runtime, `ResolveWeight(SkillTypes)` — nunca 0 silencioso |
| `modded/Server/CatalogService.cs` | Singleton read-only: `Search`, `GetItemName` (en/pt→"po"), `GetPrice` (flea→handbook; moeda=facial), `GetCategories`, `GetPresetsFor`, `GetClothing`, `GetEditionKeys`; mirrors internos dos resolvers de preset do `InventoryBuilder` (não editado — território do item 021) |
| `modded/Server/CostService.cs` | Singleton: `ComputeSkillCost` (Σ nível×peso, budget 28–32, regras informativas como warnings) + `ComputeLoadoutCost` (equipped+stash, presets expandidos, mods/contents/ammo, `missingPrice` flag) + records de breakdown serializáveis |
| `scripts/check-skill-costs.mjs` | Paridade da fórmula/pesos sem `dotnet build` (node puro) — pesos espelhados de `SkillWeights.cs` |

Não tocados (conforme divisão de trabalho com os agentes 020/021): `CustomClasses.Server.csproj`, `CustomClassesMod.cs`, `CustomClassesMetadata.cs`, registries, `InventoryBuilder.cs`, `Web/**`.

## Saída do `check-skill-costs.mjs` (2026-06-10)

```text
check-skill-costs — 11 class file(s), budget [28, 32]

Armeiro                  cost  29.49  OK
Batedor                  cost  30.00  OK
Caçador                  cost  29.38  OK
Fuzileiro                cost  29.04  OK
Gerente de Operações     cost  29.88  OK   (note: 7 skills > max 6)
Médico de Combate        cost  31.83  OK
Operador Furtivo         cost  28.71  OK
Operador Tático          cost  28.61  OK
Peladão                  cost   0.00  OK (no skills — intentional)
Saqueador                cost  29.98  OK   (note: 7 skills > max 6)
Sobrevivencialista       cost  30.61  OK   (note: 7 skills > max 6)

RESULT: all classes with skills are inside the [28, 32] budget (parity with the RZ formula).
```

(Saída completa com o detalhamento por skill disponível rodando `node mods/CustomClasses/scripts/check-skill-costs.mjs` — exit code 0.)

Os `skills` das 10 classes portadas são idênticos aos `skillOverrides` do RZ (`build-profile-jsons.js`), e a tabela de pesos é o port 1:1 do `SKILL_MULTS` ⇒ os custos acima são exatamente os valores do RZ. As notas "7 skills > max 6" são a regra informativa do kickoff (não-bloqueante) — o RZ original também tinha 7 skills nessas três classes.

## Verificação de símbolos SPT (sem build)

Todos os membros usados foram conferidos contra `references/spt-source/`:

- `DatabaseService`: `GetItems`/`GetPrices`/`GetHandbook`/`GetGlobals`/`GetCustomization`/`GetProfileTemplates`/`GetLocales` (DatabaseService.cs:117–141) — todos Singleton.
- `ItemHelper`: `GetItem`, `IsOfBaseclass(es)`, `GetStaticItemPrice` (:454), `GetDynamicItemPrice` (:470) — Singleton.
- `LocaleService.GetLocaleDb(lang)` (:20, fallback en embutido) — Singleton; locale pt do EFT = `"po"` (confirmado em `SPT_Data/database/locales/global/`).
- Locale keys: item = `"{tpl} Name"`/`"{tpl} ShortName"`; categoria do handbook = o próprio id (validado contra `en.json` real).
- `HandbookBase.Items[].Price/ParentId`, `Preset` (`_items`/`_encyclopedia`), `Slot.MaxCount` (`_max_count`, TemplateItem.cs:1733), `Upd.StackObjectsCount` (`double?`), `BaseClasses.MONEY` (:78), `SkillTypes.FirstAid/FieldMedicine/BearRawpower/UsecNegotiations`.

## Pendências

- [ ] **Build integrado (`dotnet build`)** — não rodado nesta entrega por conflito com o agente 020 (conversão do csproj p/ Sdk.Web em paralelo). Rodar `/compile-mod` quando a wave W1 fechar e validar que os 3 arquivos novos compilam (esperado: sim — só usam API verificada + padrões já compilados no mod).
- [ ] Sanidade ₽ in-game / via rota do editor (item 023): conferir que nenhuma classe tem linha `missingPrice` inesperada.
- [ ] Conferir com o item 021 se o `ClassEditorService` quer consumir `CostService` direto (grafo atual: CostService → CatalogService, sem dependência de registries).
- [ ] Lembrete de manutenção: qualquer ajuste em `SkillWeights.cs` exige espelhar `scripts/check-skill-costs.mjs` (comentário cruzado nos dois arquivos).
