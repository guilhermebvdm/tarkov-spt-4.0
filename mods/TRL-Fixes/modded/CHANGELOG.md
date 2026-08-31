# Changelog — TRL-Fixes

Versões mais recentes primeiro.

## v1.4.0 (2026-08-31)

### Correções de Desync de Inventário, Itens Fantasmas e Quick-Move
- **`FikaInventoryDesyncSafetyPatch`**: Previne e corrige desyncs de grid e rejeições de inventário (`is taken by another item` / `GClass1543` / `SlotTakenError`) no modo cooperativo do FIKA e SPT:
  - **Reserva Virtual Preemptiva de Slots (`QuickMoveSlotReservation`)**: Intercepta `StashGridClass.FindFreeSpace` e mantém um registro transitório de alocações em rajadas de `Ctrl+Click` rápido, garantindo que dois itens despachados em sequência não disputem as mesmas coordenadas `(x, y)` no servidor.
  - **Auto-Recuperação Visual Instantânea (`InventoryRejectionAutoRecovery`)**: Intercepta rejeições de rede em `ClientInventoryOperationHandler.ReceiveStatusFromServer` e despacha na Main Thread a reconstrução geométrica do grid e o evento `RaiseRefreshEvent` no contêiner pai, fazendo o item que estava invisível no cliente reaparecer imediatamente na interface sem que o jogador precise jogar a mochila no chão.
  - **`MainThreadDispatcher`**: Componente dedicado para despacho thread-safe de eventos de UI do EFT a partir dos callbacks assíncronos do LiteNetLib com limite de segurança defensivo contra vazamento de memória.

---

## v1.3.0 (2026-08-16)

### Correções de Compatibilidade Fika e Sincronização de Inventário
- **`FikaProceedEmptyHandsSafetyPatch`**: Intercepta `FikaServer.OnProceedRequestPacketReceived` para transições de mãos vazias (`EProceedType.EmptyHands`). Resolve a rejeição indevida do servidor ao buscar itens com `MongoID` vazio, eliminando o erro `[HandleCallbackResponse]: Could not execute callback with id XX on the server` que ocorria durante recargas contínuas fora do inventário (ex.: `SPT-ContinuousLoadAmmo` / `LoadAmmoAnim`).
- **`FikaRefreshSlotViewsSafetyPatch`**: Corrige a colisão de chaves em `ObservedPlayer.RefreshSlotViews`. Substitui a indexação vulnerável por dicionário de `slot.FullId` por uma estrutura de pares chave-valor segura, eliminando o log de `CRITICAL ERROR DICTIONARY: mod_tactical` ao sincronizar armas com múltiplos slots/trilhos táticos.

---

## v1.2.4 (2026-08-12)

### Correções de Montagem de Armas e Gerenciador de Armas
- **`BotMountWeaponFixPatch`**: Correções para montagem de bots em armas estacionárias e sincronização no Fika.
- **`BotWeaponManagerSafetyPatch`**: Validações nulas defensivas no gerenciador de armas dos bots.
- **`FikaMainThreadUISafetyPatch`**: Execução segura de mensagens de UI do Fika despachadas para a Main Thread.

---



### Nova correção — trava de controles ao pegar item do chão

- **`PickupAimingSafetyPatch`**: ao pegar ou equipar um item do chão pelo menu de ação nativo (mais comum com
  coletes e rigs), o corpo do personagem podia **congelar** — não anda, não agacha, não vira a visão — enquanto
  o inventário e a troca de arma continuavam respondendo. O patch impede a trava.
- O patch **já existia** neste mod, foi movido para `stancesAndCameraPositionSPT4.0.11` em 2026-07-25 sem
  registro, e voltou para cá: é remendo sobre bug do jogo base, e o mod de stances está sendo preparado para
  publicação pública.
- **Logging forense**: a primeira ocorrência sai no console com a **pilha de chamadas completa**; as seguintes
  saem com throttle de 5 s e contador acumulado. A causa raiz descrita no diagnóstico
  ([`docs/handoff-pickup-aiming-safety.md`](../docs/handoff-pickup-aiming-safety.md)) é coerente com o
  decompilado mas **nunca foi capturada em raid** — esse primeiro registro é o que confirma ou refuta.
- Reescrito no estilo do mod (Harmony direto, sem SPT.Reflection).

### Manutenção

- Versão passa a ser declarada também no `.csproj` (`Version`/`AssemblyVersion`/`FileVersion`). Sem isso a DLL
  saía marcada como `1.0.0.0` independentemente da versão do plugin.

---

## v1.0.0

Versão inicial: `FlashbangBotPatch`, `FlashbangRadiusPatch`, `Patch_PoolManagerCreateItem` e
`FixFikaReviveRagdollPatch`.
