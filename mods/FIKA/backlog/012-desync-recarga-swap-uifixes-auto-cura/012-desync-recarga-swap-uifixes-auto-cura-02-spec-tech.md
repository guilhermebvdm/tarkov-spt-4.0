# 012 — Desync de Recarga Rápida (Swap UIFixes) e Auto-Cura de Slots de Inventário · Especificação Técnica

**Mod:** FIKA  
**Target / Fork:** `mods/FIKA/modded-V2/`  
**Status:** 🟢 Concluído (Retroativo v2.4.2)  
**Data:** 2026-09-15  

---

## 1. Problema Original (Caso "Manodavis" / Interchange)

Em suporte do servidor Tarkov Red Line (Headless), o jogador "Manodavis" relatou que ao realizar uma recarga de emergência com a tecla "R" em uma AK-12 5.45:
1. O carregador ejetado da arma e o novo carregador do colete tático entraram em estado de deadlock.
2. O colete bloqueou o slot correspondente; o jogador não conseguia inserir munições manualmente, a tecla "END" (cancelamento de animação de mãos) não respondia e o jogador não conseguia lootear outros carregadores.
3. No log do Headless (`LogOutput.log`), foram registradas falhas encadeadas:
   - `"Cannot perform Move: Item is not in request"`
   - `"GClass1538: Source address mismatch"`
   - `"OperationCallback timed out"` no Watchdog local do cliente com `Strict Inventory Sync = true`.

### 1.1 Causa Raiz Investigada
- O mod `UIFixes` executa uma rotina de *swap atômico* (`InteractionsHandlerClass.Swap`) no cliente e cancela a recarga padrão do Tarkov, despachando um `SwapOperationClass` no canal de inventário.
- Por desvios de sincronização de milissegundos entre o estado do cliente e a simulação no Headless, o slot de destino onde o carregador velho deveria entrar já constava como ocupado no Headless ou o descritor de origem apontava para um slot divergente.
- Quando o Headless rejeita a operação com erro, o cliente com `Strict Inventory Sync` aguardava o callback do servidor. Ao estourar o timeout de 5s, o callback era drenado mas a operação não concluía um `RollBack` limpo no inventário nativo do EFT, travando o flag `WaitingForCallback` e paralisando as ações de mãos.

---

## 2. Solução Técnica Escolhida

A arquitetura de auto-cura e resiliência adotada atua em 4 frentes coordenadas:

### 2.1 Auto-Cura de Slot no Headless (`ReloadMagPacket.cs`)
- Antes de invocar a rotina de recarga no Headless, o pacote verifica se o slot de destino (`gridItemAddress`) onde o carregador ejetado deve ser inserido está livre.
- Se estiver ocupado por desync:
  1. Busca um novo slot livre no colete/mochila utilizando `QuickFindAppropriatePlace`.
  2. Se o colete estiver 100% entupido (sem espaço), passa `gridItemAddress = null`. No EFT nativo, passar `null` instrui o sistema a descartar o carregador no chão com segurança.
- **Resultado:** A recarga nunca aborta em silêncio e as mãos nunca ficam presas.

### 2.2 Garantia de RollBack em Rejeições do Servidor (`ClientInventoryOperationHandler.cs`)
- Quando o Headless responde com rejeição explícita (`!packet.Succeeded`), o cliente força `operation.Status = EOperationStatus.Failed` antes do `Dispose()`.
- Isso dispara a rotina nativa de `RollBack` do EFT, restaurando os itens às suas posições originais e liberando a trava de `WaitingForCallback`.

### 2.3 Reconciliação de Respostas Tardias (`FikaPlayer.cs`)
- Se o pacote `InventoryOperationResultPacket` chegar após o timeout do Watchdog local de 5s, o cliente verifica se a resposta do host foi `Succeeded`. Se foi sucesso, aceita o estado do host em vez de descartá-lo como "operação desconhecida".

### 2.4 Auto-Reconciliação de Origem (`SplitOperationDescriptorPatch.cs` e `MoveOperationDescriptorPatch.cs`)
- Em vez de rejeitar `Move` ou `Split` com `GClass1538` quando a coordenada de origem no pacote divergir do host, o patch valida se o item pertence comprovadamente ao inventário do jogador. Confirmada a posse, simula a operação a partir do local real do item no host.
