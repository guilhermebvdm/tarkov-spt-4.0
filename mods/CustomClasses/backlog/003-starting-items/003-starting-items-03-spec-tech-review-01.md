# 003 — Itens + hideout + 10 classes reais · Review Técnica 01

**Mod:** CustomClasses
**Spec técnica revisada:** [003-starting-items-02-spec-tech.md](003-starting-items-02-spec-tech.md)
**Data:** 2026-06-07

> Análise crítica. IDs `PA-01-MM`. O spec é propositalmente esqueleto (TODOs marcados) por ser item grande/incremental — os achados abaixo transformam esses TODOs em abordagens concretas. Sem bloqueadores: dá pra **começar** pela fatia "equipado simples" e ir resolvendo o resto.

## Resumo

> 🔴 0 · 🟡 5 · 🟢 2 · ✅ Resolvidos: 7 · Pendentes: 0 · Total: 7 (todos aceitos e dobrados na spec técnica)

**Verificado:** `Inventory.Equipment/Stash/Items` ([BotBase.cs:368/371/375](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Eft/Common/Tables/BotBase.cs#L368)) e `BotHideoutArea.Level` (`int?`, [BotBase.cs:835](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Eft/Common/Tables/BotBase.cs#L835)) existem; `PresetHelper`, `Preset.Items`, `EquipmentSlots`, chamber ids confirmados na pesquisa. Nenhuma ref inexistente.

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | A — Gap | 🟡 | Packing em grade (`Location`) de stash/contêineres não especificado | ✅ Resolvido |
| PA-01-02 | A — Gap | 🟡 | Re-raiz + re-id da árvore de preset (algoritmo) | ✅ Resolvido |
| PA-01-03 | A — Gap | 🟡 | `loadedMag`: capacidade do carregador + munição default | ✅ Resolvido |
| PA-01-04 | B — Edge | 🟡 | `preset`: ambíguo entre tpl de arma e id de preset | ✅ Resolvido |
| PA-01-05 | A — Gap | 🟡 | Base "Zero to hero": confirmar Equipment/Stash/Hideout.Areas pré-populados | ✅ Resolvido |
| PA-01-06 | C — Lógica | 🟢 | Nullability (`Inventory.*`, `BotHideoutArea.Level`) — stub do hideout não compila como está | ✅ Resolvido |
| PA-01-07 | A — Gap | 🟢 | Validador "item cabe" precisa dos grids do contêiner (`_props.Grids`) | ✅ Resolvido |

## Categorias

- **A — Gaps** · **B — Edge Cases** · **C — Erros de Lógica**

## Impacto

- 🔴 **Bloqueador** · 🟡 **Importante** · 🟢 **Menor**

---

### PA-01-01 · A — Gap · 🟡 Importante

**Packing em grade (`Location`) de stash e contêineres não especificado**

**Problema:** itens soltos no stash e dentro de rig/mochila precisam de `Location {x,y,r}` na grade (templates vanilla trazem isso setado). O spec marcou "packing na grade" como TODO sem algoritmo.

**Por que importa:** sem isso, itens de stash/contêiner não têm posição → não aparecem ou quebram o inventário.

**Sugestão:** implementar um `GridPacker` **first-fit** usando dims (width/height do tarkov-itemdb), tentando rotação (r=1) quando não couber; logar e pular o excedente (corner case de overflow). Reusar a lógica de dims que o RZCustomProfiles já consultava. Aplicar tanto ao stash (grade 10x28 do "Zero to hero") quanto a `contents` de rig/mochila (grade do `_props.Grids`).

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar sugestão · `[ ]` Caminho alternativo: _______

---

### PA-01-02 · A — Gap · 🟡 Importante

**Re-raiz + re-id da árvore de preset (algoritmo)**

**Problema:** ao clonar `Preset.Items` ([Globals.cs:4411](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Eft/Common/Globals.cs#L4411)), é preciso (a) achar a **raiz** da árvore (o item-arma), (b) setar `ParentId = Inventory.Equipment`, `SlotId = <slot>`, e (c) **remapear todos os Ids** preservando os links pai-filho. O spec diz "re-raiz" sem o como.

**Por que importa:** se os Ids não forem remapeados consistentemente, a árvore quebra (mods órfãos) ou colide com outros itens.

**Sugestão:** algoritmo: identificar a raiz (item cujo `ParentId` não aponta p/ outro item da lista, ou `Preset.Parent`==tpl da raiz); construir um mapa `oldId→newId` (`new MongoId()`) para todos; reescrever `Id` e `ParentId` por esse mapa; setar na raiz `ParentId=equipmentId`/`SlotId=slot`. (IDs só precisam ser únicos no template — SPT re-id no profile, CreateProfileService.cs:94.)

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar sugestão · `[ ]` Caminho alternativo: _______

---

### PA-01-03 · A — Gap · 🟡 Importante

**`loadedMag`: capacidade do carregador + munição default**

**Problema:** encher o carregador exige (a) a **capacidade** (StackObjectsCount) — vem do template do mag (`_props.Cartridges[0]._max_count`) ou do itemdb; e (b) se `ammo` for omitido, uma **munição default** compatível com o calibre.

**Por que importa:** sem capacidade, não dá pra preencher; sem default de munição, `loadedMag` sem `ammo` não tem o que carregar.

**Sugestão:** (1) capacidade via template do mag (itemHelper/itemdb); (2) **exigir `ammo` explícito** quando `loadedMag/chambered` (mais simples e determinístico) — ou, se quiser default, resolver via filtros do mag. Documentar no formato que `ammo` é obrigatório p/ carregar.

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar sugestão · `[ ]` Caminho alternativo: _______

---

### PA-01-04 · B — Edge · 🟡 Importante

**`preset` ambíguo entre tpl de arma e id de preset**

**Problema:** o campo `preset` aceita "weapon tpl ou preset id" — ambos são MongoId 24-hex, indistinguíveis pelo formato. `GetDefaultPreset(tpl)` vs `GetPreset(id)` esperam coisas diferentes.

**Por que importa:** passar um preset id para `GetDefaultPreset` (ou vice-versa) retorna null → arma não monta.

**Sugestão:** resolver com fallback: tentar `GetPreset(valor)` (id de preset); se null, `GetDefaultPreset(valor)` (tpl da arma → preset default). Documentar essa ordem no formato. (Alternativa: dois campos `presetId` vs `weaponTpl`.)

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar sugestão · `[ ]` Caminho alternativo: _______

---

### PA-01-05 · A — Gap · 🟡 Importante

**Base "Zero to hero": confirmar Equipment/Stash/Hideout.Areas pré-populados**

**Problema:** o builder assume que o `Character` clonado já tem `Inventory.Equipment` (raiz), `Inventory.Stash` (contêiner) e `Hideout.Areas` (lista de estações p/ setar nível). Não confirmado para a base "SPT Zero to hero".

**Por que importa:** se `Stash`/`Equipment` forem null ou `Hideout.Areas` vazio, equipar/stash/hideout falham silenciosamente.

**Sugestão:** no 1º teste, logar o estado do `Character` base (tem Equipment? Stash? quantas Areas?). Se faltar, criar os contêineres/áreas necessários (ou escolher outra base). Tratar `Inventory.*` como possivelmente nulos no código.

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar sugestão · `[ ]` Caminho alternativo: _______

---

### PA-01-06 · C — Lógica · 🟢 Menor

**Nullability — stub do hideout não compila como está**

**Problema:** `BotHideoutArea.Level` é `int?` ([BotBase.cs:835](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Models/Eft/Common/Tables/BotBase.cs#L835)); o stub `area.Level = Math.Max(area.Level, level)` (int? vs int) não compila. `Inventory.Equipment/Stash` são `MongoId?` e `Items` é `List<Item>?`.

**Por que importa:** erro de compilação + possíveis NREs.

**Sugestão:** `area.Level = Math.Max(area.Level ?? 0, level);` e guardas `?? `/null-check em `Inventory.Items/Equipment/Stash` (inicializar `Items` se null).

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar sugestão · `[ ]` Caminho alternativo: _______

---

### PA-01-07 · A — Gap · 🟢 Menor

**Validador "item cabe" precisa dos grids do contêiner**

**Problema:** validar que um item cabe numa rig/mochila exige a grade do contêiner (`_props.Grids` do template do contêiner) além das dims do item.

**Por que importa:** sem a grade, o packer (PA-01-01) não sabe o tamanho disponível → não dá pra validar "cabe".

**Sugestão:** obter `_props.Grids` do template do contêiner (via itemHelper/itemdb) e usar no `GridPacker`. Faz par com PA-01-01.

**Decisão:** `[ ]` Pendente · `[x]` Aceitar sugestão · `[ ]` Caminho alternativo: _______

---

## Resolução (2026-06-07)

Todos os 7 aceitos e dobrados na spec técnica 02 (abordagens concretas):
- **PA-01-01** ✅ — `GridPacker` first-fit (dims itemdb) + rotação + log overflow, p/ stash e `contents`.
- **PA-01-02** ✅ — re-id da árvore de preset via mapa `oldId→newId` (preserva ParentId); raiz no slot.
- **PA-01-03** ✅ — `ammo` **obrigatório** quando `loadedMag`/`chambered`; capacidade do mag via template/itemdb.
- **PA-01-04** ✅ — `preset`: `GetPreset(id)` → fallback `GetDefaultPreset(tpl)`.
- **PA-01-05** ✅ — logar estado do `Character` base no 1º teste; criar contêineres/áreas se faltarem; tratar `Inventory.*` nulo.
- **PA-01-06** ✅ — `Level ?? 0`; guardas de null em `Inventory.*`.
- **PA-01-07** ✅ — usar `_props.Grids` do contêiner no packer.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-07 | Review técnica 01 criada via `/review-technical-spec` |
| 2026-06-07 | Todos os 7 aceitos e dobrados na spec técnica |
