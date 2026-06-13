# 022 — Catálogo de itens + custo (port RZ) · Kickoff

**Mod:** CustomClasses · **Data:** 2026-06-09 · **Origem:** plano aprovado do editor web de classes (`~/.claude/plans/`, sessão 2026-06-09; renumerado 021→022)
**Wave:** W1 (paralelo a 019/021 desde o início) · **Deps:** —

> Brief de kickoff — insumo para `/create-spec 022`. Não é a spec. Só C#, sem UI.

## Objetivo

Camada de dados do editor: catálogo de itens do **DB vivo** do server e o serviço de custo da classe (fórmula do RZCustomProfiles).

## Escopo

- **`CatalogService`** (read-only sobre `DatabaseService`): templates de itens com nome localizado (locales pt/en), preços, categorias (handbook), presets de `globals.ItemPresets` (default + premium/mais kitado — reusar a lógica do `InventoryBuilder.ResolvePremiumPreset`), customization upper/lower por facção, lista de editions base. Inclui itens de outros mods instalados automaticamente (DB vivo).
- **`CostService`** — port **fiel** do RZ:
  - Custo ponderado de skills = Σ nível×peso (`SKILL_MULTS` 31 skills, BASELINE 15, clamp 0.25–5.00, budget alvo 28–32, regras: ≥1 ponto por categoria, máx 6 skills >0, teto sugerido 10 níveis).
  - `loadoutTotalRub` (moeda = basePrice; item = preço; sem preço = 0 + badge de aviso — nunca 0 silencioso no UI).
  - **Multiplicadores de XP ficam FORA do custo** (decisão do usuário — só exibidos).
- **Extensão da tabela de pesos** (skills sem peso no RZ — decisão do usuário):
  - As 4 do Skills-Extended (`FirstAid`, `FieldMedicine`, `BearRawpower`, `UsecNegotiations`): derivar peso pela **mecânica de upagem** — analisar o source vendored do SE (qual evento dá XP, frequência, velocidade de upar) → estimar "nível esperado no personagem de referência lvl 43" → peso = BASELINE/estimado, clamp. Racional documentado por skill.
  - **Fallback por categoria/tipo** para qualquer skill futura sem peso explícito (documentado).
- **Preço ₽ — decisão registrada na tech-spec:** recomendação = flea **efetivo** do server (análogo do avg24h do RZ; ver memória da fórmula do flea SPT 4.0 — override aditivo + piso + teto); fallback handbook p/ item fora do flea.

## Refs

- `mods/RZCustomProfiles/scripts/build-profile-jsons.js` — `SKILL_MULTS`/`weightedCost`/`loadoutTotalRub` (fonte do port)
- `mods/RZCustomProfiles/backlog/002-custom-profiles/002-custom-profiles-00-multiplicadores.md` — doc da fórmula (clamp 5.00)
- `mods/Skills-Extended/modded/` — mecânica de XP das 4 skills (derivação de pesos)
- [modded/Server/InventoryBuilder.cs](../../modded/Server/InventoryBuilder.cs) — resolução de presets a reusar

## DoD (resumo)

- **Paridade restrita ao custo ponderado de skills:** ≡ fórmula RZ nas 11 classes atuais (Peladão = 0).
- ₽: teste de **sanidade** (sem itens a 0 inesperados) — a fonte de preço difere do RZ por design.
- Catálogo resolve nome/preço/preset/customization; tabela de pesos estendida com racional documentado.
