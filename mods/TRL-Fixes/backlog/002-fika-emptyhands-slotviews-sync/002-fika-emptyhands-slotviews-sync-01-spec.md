# 002 — fika-emptyhands-slotviews-sync · Spec Funcional

**Mod:** TRL-Fixes  
**Status:** 🟢 Vivo  
**Data:** 2026-08-16  
**Autores:** [Antigravity]

---

## 1. Contexto e Problema

Ao carregar ou descarregar munições fora do inventário (ex.: via atalho rápido de recarga ou fechamento da tela do inventário com mods como `SPT-ContinuousLoadAmmo` e `LoadAmmoAnim`), o cliente envia uma instrução para colocar as mãos vazias (`SetEmptyHands`).

No ambiente cooperativo Fika, ocorrem três falhas encadeadas:
1. **Erro de Callback do Servidor:** O servidor Fika (`FikaServer`) rejeita a transição para mãos vazias porque tenta localizar no mundo um item associado a um `MongoID` vazio, gerando `[Error : Fika.Core] [HandleCallbackResponse]: Could not execute callback with id XX on the server`.
2. **Dessincronização de Inventário:** A falha no callback quebra a máquina de estados das mãos e faz com que operações subsequentes de munição falhem na validação (`HandleInventoryPacket::Unable to process descriptor`).
3. **Colisão de Chaves em Armas Táticas:** A tentativa de recuperação do Fika (`SetInventory` -> `RefreshSlotViews`) dispara o erro `[Error : Fika.Core] [RefreshSlotViews]: CRITICAL ERROR DICTIONARY: mod_tactical` devido à indexação por chave simples (`slot.FullId`) em armas com múltiplos slots/trilhos táticos.

---

## 2. Critérios de Aceite (AC)

- **AC-01:** Transições para `EProceedType.EmptyHands` originadas de qualquer cliente devem ser confirmadas com sucesso pelo servidor Fika sem tentar buscar itens com `MongoID` vazio.
- **AC-02:** A recarga contínua fora do inventário não deve disparar erros de callback (`HandleCallbackResponse`) no console ou log do BepInEx.
- **AC-03:** A sincronização de armas com múltiplos acessórios e trilhos táticos (`mod_tactical`) em `ObservedPlayer.RefreshSlotViews` não deve disparar erros críticos de colisão de dicionário.
- **AC-04:** O mod deve manter carregamento resiliente via `SoftDependency`, funcionando normalmente sem o Fika instalado.
