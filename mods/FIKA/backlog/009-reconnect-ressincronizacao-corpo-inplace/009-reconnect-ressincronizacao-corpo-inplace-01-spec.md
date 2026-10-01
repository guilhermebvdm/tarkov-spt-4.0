# 009 — Correção Definitiva de Reconnect no FIKA (Ressincronização de Corpo In-Place)

**Mod:** FIKA  
**Status:** Concluído  
**Criado:** 2026-09-13  

## Visão geral

Em raids multiplayer cooperativas no SPT 4.0 via FIKA, quando um cliente sofre uma queda de conexão ou crash do jogo e tenta reconectar à partida em andamento ("Reconnect"), ocorrem falhas graves de sincronização física e visual:
1. Para o Host e outros jogadores, o corpo do jogador desconectado permanece permanentemente congelado no local exato da queda.
2. Na nova posição em que o jogador retorna, seu personagem fica completamente invisível (malha 3D desabilitada pelo sistema de oclusão do EFT).
3. O buffer de interpolação de movimento do cliente passa a descartar 100% dos pacotes de movimentação recebidos se o processo do jogo foi reiniciado.

Este item implementa a ressincronização autoritativa *in-place* do personagem, mantendo o `ObservedPlayer` intacto na memória do Host — preservando todos os itens coletados, saúde dos membros, danos na blindagem e consumo de munição — e restabelecendo o teletransporte autoritativo, o reposicionamento da oclusão (culling) e a auto-recuperação do relógio de snapshots.

Esta entrega também atua como fundação técnica indispensável para a funcionalidade subsequente de "Invasão / Entrada em Raid em Andamento" (Join In Progress).

## Comportamento atual

- Ao desconectar, o Host mantém a instância de `ObservedPlayer` registrada no `CoopGame` e no `CoopHandler`.
- Ao reconectar, o cliente obtém seu perfil do servidor, instancia o jogador local (`FikaPlayer`) e executa um teletransporte local para a última posição conhecida salva no servidor (`FikaBackendUtils.ReconnectPosition`).
- **Falha 1 (Falta de Teletransporte Autoritativo):** O cliente envia um `ClearSnapshotterPacket`, mas esse pacote só contém o `NetId`. O Host e os outros clientes apenas chamam `Snapshotter.Clear()` e `RemoveAllActiveEffects()`. Nenhum teletransporte é acionado na instância do `ObservedPlayer` nos peers remotos. O corpo físico e o `CharacterController` permanecem no ponto de desconexão.
- **Falha 2 (Armadilha do Culling e Transform raiz):** Em `ObservedPlayer.ManualStateUpdate`, quando o jogador não é visto pelo observador (`!_cullingHandler.IsVisible`), o código atualiza `Position = CurrentPlayerState.Position` (que no EFT afeta exclusivamente `PlayerBones.BodyTransform.position`) e dá um retorno antecipado (`return`), nunca atualizando `Transform.position`. O GameObject raiz do jogador fica preso no local da queda. Como o sistema de oclusão do EFT (`LocalPlayerCullingHandlerClass`) avalia a visibilidade baseando-se no GameObject raiz, ele determina que o jogador está fora de visão e aplica `forceRenderingOff = true`. O jogador torna-se invisível na posição real para qualquer um que se aproxime dele.
- **Falha 3 (Descarte por Salto Temporal no Snapshotter):** O timestamp de rede (`NetworkTimeSync.NetworkTime`) é baseado em `Time.unscaledTimeAsDouble` (tempo desde que o executável da Unity abriu). Se o jogador reconecta após fechar/reiniciar o jogo, seu tempo local recomeça próximo de `0.0s`. No Host e nos outros clientes, o buffer guardava o último tempo recebido antes do crash (ex: `1400.0s`). Em `PlayerSnapshotter.AddSnapshot`, a checagem `if (snapshot.RemoteTime <= newestTime) return;` descarta incondicionalmente todos os novos pacotes como duplicatas/fora-de-ordem.
- **Falha 4 (Ausência de Broadcast do Reset):** O `ClearSnapshotterPacket` recebido pelo Host em `FikaServer.Callbacks.cs` nunca é retransmitido para os demais clientes da sala cooperativa, fazendo com que terceiros permaneçam com o buffer e posição dessincronizados.

## Comportamento desejado

- O `ClearSnapshotterPacket` passa a transportar as coordenadas completas de re-ancoragem (`Vector3 Position` e `Vector2 Rotation`).
- O Host e todos os clientes recebem o pacote e executam `ForceTeleport(Position, Rotation)` no `ObservedPlayer`, sincronizando a raiz (`Transform.position`), os ossos (`PlayerBones.BodyTransform.position`), o `CharacterController` e acionando `Physics.SyncTransforms()`.
- O sistema de oclusão do EFT (`LocalPlayerCullingHandlerClass`) é re-ancorado via `ApplyVisibleState()`, tornando o corpo imediatamente visível na nova posição.
- Em `ObservedPlayer.ManualStateUpdate`, a posição da raiz do GameObject (`Transform.position`) é sincronizada mesmo quando `!_cullingHandler.IsVisible`, impedindo que a caixa de oclusão fique defasada quando o jogador se move sem visibilidade direta.
- O `PlayerSnapshotter.AddSnapshot` detecta automaticamente saltos temporais negativos acentuados (`snapshot.RemoteTime < newestTime - 5.0d`) e executa `Clear()` interno defensivo, permitindo estabilização imediata após reinício do jogo.
- Todo o progresso da raid é rigorosamente preservado (itens na mochila/bolsos, integridade de blindagem, vida/fraturas e munição gasta).

## Critérios de aceite

- [x] O corpo do jogador reconectado move-se instantaneamente para a posição real da reconexão, eliminando o corpo congelado no ponto de queda.
- [x] O jogador reconectado é 100% visível para o Host e para todos os outros participantes da raid na sua posição real.
- [x] O buffer de snapshots do `ObservedPlayer` aceita os novos pacotes imediatamente após a reconexão, mesmo após reinício completo do executável do cliente (Alt+F4).
- [x] O Host retransmite o pacote de re-ancoragem via broadcast confiável para todos os outros clientes conectados na partida.
- [x] O inventário, estado de saúde e equipamentos do jogador reconectado continuam idênticos ao momento pré-queda, sem reinicialização de perfil.
- [x] **Fika/multiplayer:** Validado em topologia Host + Clientes múltiplos; todos os observadores enxergam a re-ancoragem simultaneamente sem fantasmas visuais.
- [x] **Estado entre raids:** O fluxo de limpeza padrão do `CoopGame` ao finalizar ou abandonar a partida continua operando normalmente, sem vazamento de referências de rede ou instâncias de jogadores.

## Corner cases

- [x] **Queda com fechamento do executável (crash/Alt+F4):** O timestamp do cliente reseta para zero; a condição de auto-recuperação do `PlayerSnapshotter` limpa o buffer e sincroniza com o novo relógio do executável.
- [x] **Queda transitória de socket (desconexão rápida sem fechar o jogo):** O timestamp não retrocede; o `ClearSnapshotterPacket` limpa dados residuais e realiza o teletransporte com sucesso.
- [x] **Reconexão enquanto outros jogadores não estão olhando (oculto/atrás de paredes):** A correção em `ManualStateUpdate` garante que o `Transform.position` da raiz acompanhe o movimento sob oclusão, de modo que quando um colega se aproximar ou olhar para ele, a malha seja renderizada sem atraso.
- [x] **Múltiplos reconects sucessivos na mesma raid:** O método `ForceTeleport` e a limpeza do snapshotter são idempotentes e podem ser acionados repetidamente sem corrupção de estado.

## Fora de escopo

- [x] Sistema de Invasão / Entrada em Raid em Andamento para jogadores não-iniciais (este item é o pré-requisito arquitetural que viabiliza a futura feature).
- [x] Modificação no protocolo de serialização de inventário durante a raid.
- [x] Alterações no SPT Server (escopo 100% Client C# / BepInEx).

## Referências

- [`references/eft-decompiled/Assembly-CSharp/EFT/Player.cs:31308`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L31308) (`Player.Teleport`)
- [`references/eft-decompiled/Assembly-CSharp/EFT/Player.cs:24637`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L24637) (`Player.Position`)
- [`references/eft-decompiled/Assembly-CSharp/LocalPlayerCullingHandlerClass.cs:16`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/LocalPlayerCullingHandlerClass.cs#L16) (`ApplyVisibleState`)
- [`mods/FIKA/memory/sessions.md`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/memory/sessions.md) (Pendência `P-1.2`)

## Histórico

| Data | Evento |
|---|---|
| 2026-09-13 | Item criado e especificado. Revisão inline confirmou abordagem in-place sem destruição de entidade. |
