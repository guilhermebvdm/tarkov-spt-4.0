# 006 — Fix 01 · `NullReferenceException` repetida no `ItemPositionSyncer` do FIKA ao lootar item com física preservada

**Mod:** VisceralCombat
**Item raiz:** [006-item-physics-sem-expirar-01-spec.md](006-item-physics-sem-expirar-01-spec.md)
**Asbuild:** [006-item-physics-sem-expirar-05-asbuild.md](006-item-physics-sem-expirar-05-asbuild.md)
**Criado:** 2026-09-21
**Disparado por:** log de erro do usuário durante teste da build 3.12.1 (itens 006+007+008), enquanto validava o fix do `ForceMode` (007/008)

## Contexto

Usuário reportou spam de `NullReferenceException` no log da Unity, repetido a cada `FixedUpdate`:

```
[Error  : Unity Log] NullReferenceException: Object reference not set to an instance of an object
Stack trace:
Fika.Core.Main.Components.ItemPositionSyncer.NotifyDone () (at <...>:0)
Fika.Core.Main.Components.ItemPositionSyncer.FixedUpdate () (at <...>:0)
```

## Causa raiz

`Fika.Core.Main.Components.ItemPositionSyncer` ([references/fika-plugin/Fika.Core/Main/Components/ItemPositionSyncer.cs](../../../../references/fika-plugin/Fika.Core/Main/Components/ItemPositionSyncer.cs)) é um componente do **FIKA** (não deste mod) que sincroniza a posição de um item fisicamente ativo pra outros peers, enquanto `_lootItem.RigidBody != null` ([ItemPositionSyncer.cs:16-22](../../../../references/fika-plugin/Fika.Core/Main/Components/ItemPositionSyncer.cs#L16-L22), propriedade `PhysicsDone`). A cada `FixedUpdate` ([ItemPositionSyncer.cs:78-98](../../../../references/fika-plugin/Fika.Core/Main/Components/ItemPositionSyncer.cs#L78-L98)), se `PhysicsDone` vira `true` (Rigidbody ficou `null`), ele chama `NotifyDone()` ([ItemPositionSyncer.cs:100-125](../../../../references/fika-plugin/Fika.Core/Main/Components/ItemPositionSyncer.cs#L100-L125)), que tenta `_lootItem.ItemOwner.RemoveItemEvent -= ItemOwner_RemoveItemEvent;` (linha 123) antes de se autodestruir (`Destroy(this)`, linha 124).

O FIKA espera que `RigidBody` só vire `null` via **assentamento natural** (item ainda no mundo, `ItemOwner` continua válido nesse momento). Este mod (`LootItemStopPhysicsPatch`, item `006`) mantém o Rigidbody vivo bem além desse ponto — e `LootItemKillCleanupPatch` (também item `006`) zera o Rigidbody dentro de `LootItem.Kill()`, ou seja, **exatamente no momento em que o item está sendo removido do mundo** (jogador lootando). Nesse momento, `ItemOwner` já pode ter sido limpo pela própria remoção em andamento — o `ItemPositionSyncer`, que continuava "vigiando" o item só porque este mod atrasou o assentamento natural dele, detecta `RigidBody == null` no próximo `FixedUpdate` e quebra tentando desinscrever de um `ItemOwner` já nulo. Como a exceção acontece **antes** do `Destroy(this)` (linha 124), o componente nunca se autodestrói — repete a mesma exceção a cada quadro, indefinidamente.

Confirmado que isso não ocorre no vanilla/sem este mod: `Kill()` não toca `_rigidBody` em nenhum ponto (já confirmado na spec técnica original do item `006`), então `RigidBody` só vira `null` via assentamento natural — cenário em que `ItemOwner` continua válido. Este é um efeito colateral genuíno de manter o Rigidbody vivo por mais tempo (item 006) combinado com o cleanup em `Kill()` (também item 006) — não estava previsto na spec técnica original (nem a spec nem a review de `006` cobriam interação com componentes do FIKA que dependem do ciclo de vida do `Rigidbody`).

## Mudanças aplicadas

| Arquivo | Mudança |
|---|---|
| `modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/LootItemPersistPhysicsPatch.cs` | `LootItemKillCleanupPatch.Prefix` agora destrói qualquer `Fika.Core.Main.Components.ItemPositionSyncer` presente no mesmo `GameObject` **antes** de zerar o `Rigidbody` — evita que o syncer chegue a rodar mais um `FixedUpdate` no estado que causa a exceção. O item está sendo removido do mundo de qualquer forma, então sincronizar a posição dele pra outros peers deixa de fazer sentido nesse ponto. |

## Checklist de validação (obrigatório antes de marcar o fix como entregue)

- [x] Compila via `/compile-mod` sem erros (build 3.12.2)
- [ ] **In-raid:** lootar um item com física preservada (assentado há muito tempo, "Item Physics" ligado) não gera mais o erro no log — pendente de novo teste do usuário
- [ ] **Fika/multiplayer:** confirmar que outros peers não ficam com o item "grudado"/desincronizado visualmente ao ser lootado, mesmo sem o pacote final de `NotifyDone()` — ou `N/A` se o item já sai do mundo por outro canal de sincronização (inventário) independente do loot-position-sync
- [ ] **raid1 → exit → raid2:** sem estado vazado entre raids
- [ ] **alt-F4 / morte / MIA:** teardown idempotente, sem exceção no LogOutput.log
- [ ] Memória do mod atualizada (`/update-memory`) com a lição do fix

## Histórico

| Data | Evento |
|---|---|
| 2026-09-21 | Fix criado e aplicado — build 3.12.2 compilada, 0 erros. Aguardando validação em raid do usuário. |
