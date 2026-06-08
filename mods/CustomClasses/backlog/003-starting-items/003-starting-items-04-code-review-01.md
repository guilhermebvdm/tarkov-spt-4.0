# 003 — Itens + hideout + 10 classes reais · Code Review 01

**Mod:** CustomClasses
**Spec funcional:** [003-starting-items-01-spec.md](003-starting-items-01-spec.md)
**Spec técnica:** [003-starting-items-02-spec-tech.md](003-starting-items-02-spec-tech.md)
**Asbuild:** [003-starting-items-05-asbuild.md](003-starting-items-05-asbuild.md)
**Data:** 2026-06-07

> Review do código das **fatias 1-3** (DTO, HideoutBuilder, InventoryBuilder: equipado-simples/preset/manual/mag+câmara). Fatias 4-5 ainda não implementadas (não revisadas). IDs `CR-01-MM`.

## Resumo

> 🔴 0 · 🟠 0 · 🟡 3 · 🟢 2 · ✅ Aplicados: 5 · Total: 5 (todos aplicados + recompilado 0 warn/err)

**Positivo:** compila 0 warn/err; reusa `PresetHelper`/`ItemHelper`/`FillMagazineWithCartridge` (não reinventa); re-id de preset correto (lê id antigo antes de atribuir); PA-01/PA-02 das reviews técnicas aplicados. Achados são robustez p/ quando tpls/presets reais entrarem (fatia 5).

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | B — Bug latente | 🟡 | tpl inválido aborta a classe inteira (sem try/catch por slot) | ✅ Aplicado |
| CR-01-02 | B — Bug latente | 🟡 | Câmara adicionada mesmo em arma sem slot de câmara | ✅ Aplicado |
| CR-01-03 | B — Bug latente | 🟡 | Carregador do preset já carregado → dupla carga | ✅ Aplicado |
| CR-01-04 | E — Legibilidade | 🟢 | Retorno (itens adicionados) ignorado no `CustomClassesMod` | ✅ Aplicado |
| CR-01-05 | F — Melhoria | 🟢 | `FillMagazineWithCartridge(1.0)` depende de `GetInt(max,max)` | ✅ Aplicado |

## Categorias

- **A — Crítico** · **B — Bug latente** · **C — Gap vs. spec** · **D — Arquitetura** · **E — Legibilidade** · **F — Melhoria**

## Impacto

- 🔴 **Bloqueador** · 🟠 **Forte** · 🟡 **Médio** · 🟢 **Menor**

---

### CR-01-01 · B — Bug latente · 🟡 Médio

**tpl inválido em um slot aborta a classe inteira**

**Local:** [`mods/CustomClasses/modded/Server/InventoryBuilder.cs`](../../modded/Server/InventoryBuilder.cs) — `new MongoId(spec.Tpl/Preset/Ammo/m.Tpl)` no loop de `equipped`.

**Problema:** `new MongoId("xyz")` com string inválida (não 24-hex) lança. Como o loop de slots roda dentro do try/catch **por arquivo** (no `OnLoad`), um único tpl digitado errado num slot derruba o **registro da classe inteira** — não só aquele item.

**Por que importa:** JSON editado à mão erra tpl fácil; uma classe inteira sumir por causa de um item é desproporcional (a spec pede "pular item inválido, resto carrega").

**Sugestão:** envolver a montagem de **cada slot** (e cada item de stash) num `try/catch` que loga e **pula só aquele item**, mantendo o resto da classe. (Idem no loop de stash da fatia 4.)

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-02 · B — Bug latente · 🟡 Médio

**Câmara adicionada mesmo quando a arma não tem slot de câmara**

**Local:** [`InventoryBuilder.cs`](../../modded/Server/InventoryBuilder.cs) — `LoadAmmo`, fallback `?? "patron_in_weapon"`.

**Problema:** se o template da arma não declarar `Chambers` (ou for algo sem câmara padrão), o código ainda adiciona um item `slotId = "patron_in_weapon"`. Isso cria um filho num slot que a arma não aceita → item inválido.

**Por que importa:** inventário inválido para armas sem `patron_in_weapon` (ou com chambers nomeados diferentes e sem o fallback válido).

**Sugestão:** só chambrear quando o template **declara** uma câmara: `var chamber = wpn.Value.Properties?.Chambers?.FirstOrDefault()?.Name;` e, se `chamber is null`, **pular** a câmara com `Warning` (em vez do fallback cego). Manter `patron_in_weapon` só se realmente existir nos Chambers.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-03 · B — Bug latente · 🟡 Médio

**Carregador do preset já carregado → dupla carga**

**Local:** [`InventoryBuilder.cs`](../../modded/Server/InventoryBuilder.cs) — `LoadAmmo`, antes do `FillMagazineWithCartridge`.

**Problema:** alguns presets default já vêm com cartuchos no carregador. `FillMagazineWithCartridge` avisa "already has cartridges" ([ItemHelper.cs:1387](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Helpers/ItemHelper.cs#L1387)) e ainda adiciona — resultando em cartuchos duplicados/contagem errada.

**Por que importa:** carregador com munição inconsistente / itens duplicados.

**Sugestão:** antes de carregar, checar se o mag já tem filho `slotId == "cartridges"` na árvore; se já tiver, **pular** o fill (ou limpar os cartuchos existentes antes). Recomendo pular + `Debug` log.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-04 · E — Legibilidade · 🟢 Menor

**Retorno de `Apply` (itens adicionados) ignorado**

**Local:** [`mods/CustomClasses/modded/Server/CustomClassesMod.cs`](../../modded/Server/CustomClassesMod.cs) — chamadas `inventoryBuilder.Apply(...)`/`hideoutBuilder.Apply(...)`.

**Problema:** os retornos (nº de itens/estações aplicados) são descartados; o log de "Registered" não menciona itens/hideout.

**Por que importa:** observabilidade — difícil confirmar no log quantos itens/estações entraram por lado.

**Sugestão:** capturar os retornos e incluir no `logger.Info` do `RegisterClass` (ex.: `items usec=.. bear=..`, `hideout=..`).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-05 · F — Melhoria · 🟢 Menor

**`FillMagazineWithCartridge(1.0)` depende de `GetInt(max,max)`**

**Local:** [`InventoryBuilder.cs`](../../modded/Server/InventoryBuilder.cs) — `LoadAmmo`, `minSizeMultiplier: 1.0`.

**Problema:** com `1.0`, o helper faz `GetInt(round(1.0*max), max)` = `GetInt(max, max)`. Funciona se `GetInt` for inclusivo (e o `CreateMagazineWithAmmo` do SPT usa `1`, então é o caminho abençoado), mas o "cheio" depende desse detalhe.

**Por que importa:** baixo — só se `GetInt` mudar para exclusivo. Documentar a intenção.

**Sugestão:** manter `1.0` (precedente `CreateMagazineWithAmmo`) e adicionar comentário "1.0 = cheio (GetInt inclusivo)". Sem mudança funcional.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

## Resolução (2026-06-07)

Todos os 5 aplicados em `InventoryBuilder.cs` / `CustomClassesMod.cs`; recompilado (0 warn/err, 34.8 KB):
- **CR-01-01** ✅ — try/catch por slot (tpl inválido pula só o item).
- **CR-01-02** ✅ — câmara só quando o template declara `Chambers` (sem fallback cego).
- **CR-01-03** ✅ — pula fill se o carregador já tem `cartridges`.
- **CR-01-04** ✅ — log de `RegisterClass` agora inclui `items usec/bear` e `hideout`.
- **CR-01-05** ✅ — comentário "1.0 = cheio (GetInt inclusivo)".

## Histórico

| Data | Evento |
| --- | --- |
| 2026-06-07 | Code review 01 (fatias 1-3) criada via `/code-review` |
| 2026-06-07 | 5 achados aplicados (CR-01-01..05) + rebuild |
