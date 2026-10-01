# 009 — Correção Definitiva de Reconnect no FIKA (Ressincronização de Corpo In-Place) · Review Técnica 01

**Mod:** FIKA  
**Spec técnica revisada:** [009-reconnect-ressincronizacao-corpo-inplace-02-spec-tech.md](009-reconnect-ressincronizacao-corpo-inplace-02-spec-tech.md)  
**Data:** 2026-09-13  

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM` permanente.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 4 · Total: 4

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | A — Gap | 🔴 Bloqueador | Destruição de ObservedPlayer causaria perda de inventário e saúde | ✅ Resolvido |
| PA-01-02 | B — Edge Case | 🟡 Importante | Rejeição contínua de pacotes por reinício de timestamp após crash | ✅ Resolvido |
| PA-01-03 | C — Erro de Lógica | 🟡 Importante | Desvio de Transform.position e armadilha de culling do EFT | ✅ Resolvido |
| PA-01-04 | B — Edge Case | 🟢 Menor | Ausência de broadcast do ClearSnapshotterPacket para terceiros | ✅ Resolvido |

---

## Pontos

### PA-01-01 · A — Gap · 🔴 Bloqueador

**Destruição de ObservedPlayer causaria perda de inventário e saúde**

**Problema:** Propostas ingênuas de correção de reconnect frequentemente sugerem deletar o GameObject do `ObservedPlayer` na desconexão e recriá-lo do zero na reconexão. No entanto, no FIKA com SPT, a instância do `ObservedPlayer` em memória no Host é a guardiã autoritativa de todo o loot coletado na raid atual, da vida remanescente dos membros e da durabilidade das armaduras. Destruir essa entidade causaria reset do personagem para o estado inicial de carregamento da raid.

**Por que importa:** Se implementado com destruição, o jogador perderia todo o progresso da raid até o momento do crash, gerando corrupção severa na experiência de jogo.

**Sugestão:** Adotar estritamente a ressincronização in-place (`ForceTeleport`), mantendo o `ObservedPlayer` e seu inventário intactos, atualizando apenas a posição física, visibilidade e os buffers de rede.

**Decisão:**
- `[x]` Aceitar sugestão
- **Resolução:** ✅ Resolvido em 2026-09-13. A spec técnica 02 formalizou a preservação integral da entidade do jogador, operando o teletransporte e re-ancoragem diretamente na instância ativa.

---

### PA-01-02 · B — Edge Case · 🟡 Importante

**Rejeição contínua de pacotes por reinício de timestamp após crash**

**Problema:** Quando o jogador fecha o executável após um crash e reconecta, o relógio local da Unity recomeça em `0s`, enviando pacotes com `RemoteTime` muito inferior ao `newestTime` gravado no Host e nos outros clientes (ex: `1500s`). O `PlayerSnapshotter.AddSnapshot` descarta pacotes onde `snapshot.RemoteTime <= newestTime`, tornando impossível qualquer movimentação subsequente.

**Por que importa:** O jogador reconectado ficaria permanentemente congelado na visão dos outros jogadores, mesmo com o teletransporte funcionando.

**Sugestão:** Implementar detecção de salto temporal negativo (`snapshot.RemoteTime < newestTime - 5.0d`) no `AddSnapshot` para acionar `Clear()` defensivamente e rearmar a interpolação imediatamente.

**Decisão:**
- `[x]` Aceitar sugestão
- **Resolução:** ✅ Resolvido em 2026-09-13. Mecanismo de auto-recuperação especificado na Seção 5 e incorporado ao stub de código da spec técnica.

---

### PA-01-03 · C — Erro de Lógica · 🟡 Importante

**Desvio de Transform.position e armadilha de culling do EFT**

**Problema:** Em `ObservedPlayer.ManualStateUpdate`, quando o jogador está fora de visão direta (`!_cullingHandler.IsVisible`), o código executa `Position = CurrentPlayerState.Position` e retorna imediatamente. No EFT, `Player.Position` altera somente `PlayerBones.BodyTransform.position`, nunca a raiz `Transform.position`. O GameObject raiz fica retido no local da queda, fazendo o sistema de oclusão (`LocalPlayerCullingHandlerClass`) marcar o jogador como ocluso (`forceRenderingOff = true`), gerando invisibilidade na nova posição.

**Por que importa:** O jogador permanece invisível para todos os outros participantes que se aproximam de sua localização real.

**Sugestão:** Atribuir explicitamente `Transform.position = CurrentPlayerState.Position` dentro do bloco `!_cullingHandler.IsVisible` antes do retorno, e chamar `_cullingHandler.ApplyVisibleState()` no `ForceTeleport`.

**Decisão:**
- `[x]` Aceitar sugestão
- **Resolução:** ✅ Resolvido em 2026-09-13. Especificado na Seção 1 e Seção 5 da spec técnica.

---

### PA-01-04 · B — Edge Case · 🟢 Menor

**Ausência de broadcast do ClearSnapshotterPacket para terceiros**

**Problema:** O método `OnClearSnapshotterPacketReceived` no `FikaServer.Callbacks.cs` processava o pacote apenas localmente no Host, sem retransmiti-lo para os demais clientes da raid cooperativa.

**Por que importa:** Em partidas com 3 ou mais jogadores, apenas o Host sincronizaria a re-ancoragem do jogador reconectado, enquanto os outros clientes continuariam vendo o corpo congelado ou desincronizado.

**Sugestão:** No Host, após processar o teletransporte do `ObservedPlayer`, enviar o pacote para todos os outros clientes conectados via `SendData(ref packet, DeliveryMethod.ReliableOrdered, peer)`.

**Decisão:**
- `[x]` Aceitar sugestão
- **Resolução:** ✅ Resolvido em 2026-09-13. Adicionado o broadcast com exclusão do peer remetente no `FikaServer.Callbacks.cs`.

---

## Histórico

| Data | Evento |
|---|---|
| 2026-09-13 | Review técnica 01 concluída com 4 pontos levantados e todos resolvidos na spec técnica. |
