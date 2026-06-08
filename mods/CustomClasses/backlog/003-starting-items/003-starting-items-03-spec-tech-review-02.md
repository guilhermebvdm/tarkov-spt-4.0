# 003 — Itens + hideout + 10 classes reais · Review Técnica 02

**Mod:** CustomClasses
**Spec técnica revisada:** [003-starting-items-02-spec-tech.md](003-starting-items-02-spec-tech.md)
**Data:** 2026-06-07

> Segunda passada, agora com a doc canônica [docs/technical/inventario-itens-spt4.md](../../../../docs/technical/inventario-itens-spt4.md) como referência. IDs `PA-02-MM`.

## Resumo

> 🔴 0 · 🟡 3 · 🟢 2 · ✅ Resolvidos: 5 · Pendentes: 0 · Total: 5 (todos aceitos; PA-02-01/02/03 dobrados nos builders, 04/05 anotados)

**Review-01 fechada:** PA-01-01..07 todos aceitos e dobrados na spec técnica — **não reabertos**. A doc canônica agora **lastreia** os detalhes que antes eram TODO (slotIds exatos `"hideout"`/`"main"`/`"cartridges"`/`"patron_in_weapon"`, `location {x,y,r}`, capacidade de mag via `_props.Cartridges._max_count`, re-id ao clonar) — risco de implementação reduzido. Esta passada cobre o que a doc evidenciou de novo.

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-02-01 | B — Edge | 🟡 | Slot de equipamento já ocupado no base (Pockets/SecuredContainer/Scabbard) | ✅ Resolvido |
| PA-02-02 | B — Edge | 🟡 | Remover item de slot deve remover a subárvore inteira | ✅ Resolvido |
| PA-02-03 | B — Edge | 🟡 | Hideout: setar `Level` pode não bastar (Active/Construção) | ✅ Resolvido |
| PA-02-04 | A — Gap | 🟢 | Reusar placement do `InventoryHelper` vs packer próprio | ✅ Resolvido |
| PA-02-05 | B — Edge | 🟢 | `count` > 1 em slot equipado não faz sentido | ✅ Resolvido |

## Categorias

- **A — Gaps** · **B — Edge Cases** · **C — Erros de Lógica**

## Impacto

- 🔴 **Bloqueador** · 🟡 **Importante** · 🟢 **Menor**

---

### PA-02-01 · B — Edge · 🟡 Importante

**Slot de equipamento já ocupado no template base**

**Problema:** a base "SPT Zero to hero" já traz itens em alguns slots de equipamento (tipicamente `Pockets`, `SecuredContainer`, possivelmente `Scabbard`/faca). O builder da fatia 1 **só adiciona** — se a classe equipar nesses slots, cria **dois** itens com o mesmo `parentId=Equipment` + `slotId` → inventário inválido (doc §3: "Pockets/SecuredContainer normalmente já existem — cuidado para não duplicar").

**Por que importa:** duplicar slot quebra o inventário do personagem.

**Sugestão:** antes de adicionar um item equipado, **remover o item existente naquele slot** (`parentId==Equipment && slotId==slot`) — e sua subárvore (ver PA-02-02) — ou pular com aviso se a política for "não sobrescrever". Recomendo **substituir** (remove o antigo + adiciona o novo).

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar sugestão · `[ ]` Caminho alternativo: _______

---

### PA-02-02 · B — Edge · 🟡 Importante

**Remover item de slot deve remover a subárvore inteira**

**Problema:** ao substituir um item equipado/contêiner (PA-02-01) ou ao validar, remover só o item-raiz deixa **filhos órfãos** (ex.: remover uma rig sem remover o conteúdo dela; remover a arma base sem os mods). A lista é flat — filhos referenciam o pai por `parentId`.

**Por que importa:** itens órfãos = inventário inválido / itens fantasma.

**Sugestão:** implementar um `RemoveItemAndChildren(items, id)` que remove recursivamente todos os itens cujo `parentId` (transitivo) seja o `id` removido. Usar ao substituir slots e ao pular itens inválidos parcialmente montados. (Pode haver helper no `ItemHelper` — checar antes de escrever.)

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar sugestão · `[ ]` Caminho alternativo: _______

---

### PA-02-03 · B — Edge · 🟡 Importante

**Hideout: setar `Level` pode não bastar para a estação ficar construída/ativa**

**Problema:** o stub do `HideoutBuilder` só ajusta `BotHideoutArea.Level`. A doc §10 lista também `Active`, `CompleteTime`, `Constructing` no `BotHideoutArea`. Setar só o `Level` pode deixar a estação "não construída" in-game (aparece nível 0, ou em construção).

**Por que importa:** o critério "hideout reflete os níveis definidos in-game" pode falhar se faltar `Active=true`/estado de construção concluída.

**Sugestão:** ao setar `Level > 0`, também garantir `Active = true`, `Constructing = false` e `CompleteTime = 0` (ou o que os templates vanilla usam). **Confirmar** comparando com uma `Area` já construída do template base (ler o estado de uma área de nível >0 da base e espelhar).

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar sugestão · `[ ]` Caminho alternativo: _______

---

### PA-02-04 · A — Gap · 🟢 Menor

**Reusar placement do `InventoryHelper` vs packer próprio**

**Problema:** o `InventoryHelper` tem lógica de placement em grade (slot default `"hideout"`, mapa da grade) — [InventoryHelper.cs:270](../../../../references/spt-source/Libraries/SPTarkov.Server.Core/Helpers/InventoryHelper.cs#L270). Mas a API principal (`AddItemToStash`) é orientada a eventos de item de um perfil vivo (recebe sessionId/output), não a montagem de **template**.

**Por que importa:** evitar reimplementar packing se houver entrada reaproveitável; mas se não houver, o packer próprio (PA-01-01) é o caminho.

**Sugestão:** checar se há um método de placement do `InventoryHelper`/`ItemHelper` utilizável sem contexto de evento (só `pmcData` + item). Se não, manter o `GridPacker` próprio (PA-01-01), seguindo a doc §4/§5.

**Decisão:** `[ ]` Pendente · `[ ]` Aceitar sugestão · `[ ]` Caminho alternativo: _______

---

### PA-02-05 · B — Edge · 🟢 Menor

**`count` > 1 em slot equipado não faz sentido**

**Problema:** `ItemSpec.Count` se aplica a stash/stack, mas um slot de equipamento aceita **um** item (não dá pra equipar 2 capacetes). O builder deve ignorar `count>1` em `equipped`.

**Por que importa:** evita confusão/itens inválidos se o JSON trouxer `count` num slot equipado.

**Sugestão:** no `equipped`, ignorar `count` (tratar como 1) e logar `Debug` se `count>1`. `count` vale para `stash` e `contents`.

**Decisão:** `[ ]` Pendente · `[x]` Aceitar sugestão · `[ ]` Caminho alternativo: _______

---

## Resolução (2026-06-07)

Todos aceitos:
- **PA-02-01** ✅ — `InventoryBuilder` remove o ocupante do slot antes de equipar (substitui).
- **PA-02-02** ✅ — `RemoveItemAndChildren` recursivo (remove subárvore por `parentId`).
- **PA-02-03** ✅ — `HideoutBuilder` seta `Active=true`/`Constructing=false`/`CompleteTime=0` ao dar `Level>0`.
- **PA-02-04** ✅ — manter packer próprio (PA-01-01) na fatia de stash; checar `InventoryHelper` antes.
- **PA-02-05** ✅ — `count>1` ignorado em slot equipado (Debug log).

## Histórico

| Data | Evento |
|---|---|
| 2026-06-07 | Review técnica 02 criada via `/review-technical-spec` (com a doc canônica) |
| 2026-06-07 | Todos os 5 aceitos e dobrados nos builders |
