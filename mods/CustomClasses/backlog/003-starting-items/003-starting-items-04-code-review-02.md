# 003 — Itens + hideout + 10 classes reais · Code Review 02

**Mod:** CustomClasses
**Asbuild:** [003-starting-items-05-asbuild.md](003-starting-items-05-asbuild.md)
**Data:** 2026-06-07

> Rodada 02 — cobre o que entrou **depois** do code-review 01 (que viu fatias 1-3): fatia 4 (`GridPacker`/packing), resolução de preset via globals (`ResolvePreset`), premium (`ResolvePremiumPreset`), etapa 2 stash-óptica (`ResolveStashPreset`/`EnsureMinimumOptic`), fatia 5 (gerador). CR-01-01..05 já aplicados — não reabertos. IDs `CR-02-MM`.

## Resumo

> 🔴 0 · 🟠 0 · 🟡 3 · 🟢 2 · ✅ Aplicados: CR-02-01/02 · ⏭️ Deferido: CR-02-03 (item 007) · ⬜ Opcionais: CR-02-04/05 · Total: 5

**Positivo:** preset resolvido direto do globals (timing-safe); packing usa `InventoryHelper.GetItemSize` (tamanho montado real); stash com óptica mínima; tudo compila 0 warn/err; arma principal validada in-game. Achados são qualidade da escolha automática de óptica + acoplamento de build.

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-02-01 | B — Bug latente | 🟡 | `ResolvePremiumPreset` (max itens) pode pegar build de evento/thermal | Pendente |
| CR-02-02 | B — Bug latente | 🟡 | `EnsureMinimumOptic` escolhe óptica arbitrária + não preenche sub-slots obrigatórios | Pendente |
| CR-02-03 | D — Arquitetura | 🟡 | Gerador acopla a `anchor-items.json` do RZ (build-time) — planejar p/ 007 | Pendente |
| CR-02-04 | E — Legibilidade | 🟢 | 3 métodos `Resolve*Preset` duplicam o scan do globals | Pendente |
| CR-02-05 | F — Melhoria | 🟢 | Scan O(presets) por chamada — índice tpl→presets opcional | Pendente |

## Categorias

- **A — Crítico** · **B — Bug latente** · **C — Gap vs. spec** · **D — Arquitetura** · **E — Legibilidade** · **F — Melhoria**

## Impacto

- 🔴 **Bloqueador** · 🟠 **Forte** · 🟡 **Médio** · 🟢 **Menor**

---

### CR-02-01 · B — Bug latente · 🟡 Médio

**`ResolvePremiumPreset` (maior nº de itens) pode pegar build de evento/thermal**

**Local:** [`mods/CustomClasses/modded/Server/InventoryBuilder.cs:223-251`](../../modded/Server/InventoryBuilder.cs#L223)

**Problema:** "premium = preset com mais itens". Mas o maior preset de uma arma às vezes é um build de **evento** ou **thermal/silenced** — ex. (do DB): AKM → `akm_kreb_thermal_silenced` (19 itens, **óptica térmica**) e M4A1 → `M4A1 2017 New year` (23, holiday). A arma principal da classe pode nascer com **mira térmica** (overkill/cheaty) ou peças de evento.

**Por que importa:** "premium" deveria ser kitado, não necessariamente térmico/holiday — pode desbalancear e destoar do conceito da classe.

**Sugestão:** ao escolher o "premium", **preferir o maior preset que NÃO contenha óptica térmica/NV** (checar baseclass dos itens contra `THERMAL_VISION`/`NIGHT_VISION`) **e** ignorar presets de evento (nome contendo "year"/"event"/"halloween"…). Empate → o de mais itens. Se só houver build térmico, usar o default. (Decidir: aceitar térmico ou filtrar.)

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (filtrar térmico/evento)
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (aceitar térmico/evento como "premium"): _________________

---

### CR-02-02 · B — Bug latente · 🟡 Médio

**`EnsureMinimumOptic` escolhe óptica arbitrária e não preenche sub-slots obrigatórios da óptica**

**Local:** [`InventoryBuilder.cs:337`](../../modded/Server/InventoryBuilder.cs#L337) e [`:365`](../../modded/Server/InventoryBuilder.cs#L365)

**Problema:** `filter.Where(IsRealOptic)...FirstOrDefault()` pega a **primeira** óptica do filtro — e `filter` é `HashSet<MongoId>` (**sem ordem garantida**). Então a mira escolhida é arbitrária: pode cair numa **térmica/NV/scope caro** em vez de um red dot simples. Além disso, a óptica é adicionada **sem preencher os slots obrigatórios dela** (ex.: scope que exige anel/mount próprio) → item pode ficar "incompleto/ inválido" in-game.

**Por que importa:** backups do stash podem nascer com mira térmica aleatória, ou com óptica faltando sub-peça obrigatória.

**Sugestão:** escolher determinístico e simples: entre as ópticas do filtro, **preferir `COLLIMATOR`/`COMPACT_COLLIMATOR`** (red dots, sem sub-slots) → senão `ASSAULT_SCOPE` → evitar `OPTIC_SCOPE/SPECIAL_SCOPE` (que costumam exigir mount/ring). Se a óptica candidata tiver slot `_required` vazio, pular. **Reavaliar no playtest** (o teste do stash sniper cobre isso).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir): _________________

---

### CR-02-03 · D — Arquitetura · 🟡 Médio

**Gerador acopla à `anchor-items.json` do RZCustomProfiles (build-time)**

**Local:** [`mods/CustomClasses/scripts/build-class-jsons.js`](../../scripts/build-class-jsons.js) (lê `mods/RZCustomProfiles/backlog/anchor-items.json` + `tools/tarkov-itemdb`).

**Problema:** o gerador das 10 classes depende de arquivos do **RZCustomProfiles** (que o item 007 vai aposentar) e do tarkov-itemdb. Runtime está OK (os `.jsonc` gerados têm os tpls embutidos, self-contained), mas **regenerar** as classes exige o RZ presente.

**Por que importa:** quando o 007 retirar o RZ, o gerador quebra; o `MOSIN_SNIPER` que adicionei já foi pro `anchor-items.json` do RZ (fora do nosso mod).

**Sugestão:** no **item 007** (ou antes), **copiar `anchor-items.json` (+ o slice necessário do item-data) para dentro de `mods/CustomClasses/scripts/`** e apontar o gerador pra cópia local. Registrar como dependência conhecida até lá. (Não bloqueia o 003.)

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão (mover anchors p/ o mod no 007)
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar: _________________

---

### CR-02-04 · E — Legibilidade · 🟢 Menor

**`ResolvePreset` / `ResolvePremiumPreset` / `ResolveStashPreset` duplicam o scan do globals**

**Local:** [`InventoryBuilder.cs:189`](../../modded/Server/InventoryBuilder.cs#L189), [`:223`](../../modded/Server/InventoryBuilder.cs#L223), [`:273`](../../modded/Server/InventoryBuilder.cs#L273)

**Problema:** os três iteram `itemPresets.Values` filtrando por `Items[0].Template == key`, mudando só o critério de seleção (default / max-itens / menor-com-óptica).

**Por que importa:** manutenção — uma mudança no shape do preset precisa ser replicada em 3 lugares.

**Sugestão:** extrair `IEnumerable<Preset> PresetsForTpl(MongoId key)` (+ o atalho `TryGetValue` p/ id de preset) e fazer cada método selecionar sobre essa lista (`FirstOrDefault(Encyclopedia)`, `MaxBy(Items.Count)`, etc.).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar: _________________

---

### CR-02-05 · F — Melhoria · 🟢 Menor

**Scan O(presets) por chamada — índice tpl→presets opcional**

**Local:** mesmos métodos `Resolve*Preset` + `EnsureMinimumOptic` ([`:304`](../../modded/Server/InventoryBuilder.cs#L304)).

**Problema:** cada resolução varre todos os `ItemPresets`; `EnsureMinimumOptic` ainda varre slots×filtros×baseclass por arma. É só no load (não em raid), então aceitável, mas multiplica por (classes × itens).

**Por que importa:** baixo — só tempo de load.

**Sugestão:** se o load ficar lento, construir uma vez um `Dictionary<MongoId, List<Preset>>` (tpl-raiz → presets) e reusar. Caso contrário, deixar como está.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deixar como está): _________________

---

## Resolução (2026-06-07)

- **CR-02-01** ✅ Aplicado — `ResolvePremiumPreset` agora prefere o maior preset **sem** óptica térmica/NV (só usa térmico se não houver outro).
- **CR-02-02** ✅ Aplicado — `PickSimpleOptic` (red dot/collimator > assault scope > resto, determinístico, evita térmica/NV) no `EnsureMinimumOptic` (caminho direto + mount). **+ Extra:** `EnsureMinimumOptic` agora roda também na **arma equipada** sem óptica (ex.: AKMS do Op. Furtivo) — não só no stash.
- **CR-02-03** ⏭️ Deferido p/ **item 007** (mover `anchor-items.json` pra dentro do mod ao aposentar o RZ).
- **CR-02-04 / CR-02-05** ⬜ Opcionais (dedup dos `Resolve*Preset` / índice tpl→presets) — abertos, sem urgência.

Recompilado 0 warn/err (51.7 KB).

## Histórico

| Data | Evento |
| --- | --- |
| 2026-06-07 | Code review 02 criada via `/code-review` (pós fatias 4-5 + premium + etapa 2) |
| 2026-06-07 | CR-02-01/02 aplicados (+ óptica na arma equipada); CR-02-03 deferido p/ 007; 04/05 opcionais |
