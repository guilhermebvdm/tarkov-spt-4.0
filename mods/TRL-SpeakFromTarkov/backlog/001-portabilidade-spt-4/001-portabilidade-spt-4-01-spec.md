# 001 — portabilidade-spt-4

**Mod:** TRL-ImmersiveVoip
**Status:** Backlog
**Criado:** 2026-07-16

## Visão geral

Transição do mod do SPT 3.11 para a API do SPT 4.0.x e EFT 0.16.9, adaptando BepInEx, Fika hooks e referências da Unity. O mod continuará provendo uma solução de VOIP com Opus encoder e cliente UDP alternativo.

## Comportamento atual

O código do mod (baseado em SPT 3.11) utiliza classes do Fika antigas (como `Fika.Core.Networking.VOIP.FikaVOIPClient`) para aplicar patch. Ele intercepta métodos do EFT da build antiga, como `ActiveHealthController.Kill` e `EFT.Player.Init`. O cliente de rede está fixo em enviar e receber dados UDP para `127.0.0.1`.

## Comportamento desejado

O mod deve rodar limpo sob o ambiente SPT 4.0.x e arquitetura do Fika atual (onde o Core e Plugin foram desmembrados). Os patches (Harmony) devem ser ajustados para os novos namespaces introduzidos no Tarkov 0.16.9. A comunicação de rede deve ser preparada para cenários cooperativos, resolvendo o problema do localhost hardcoded.

## Critérios de aceite

- [ ] O projeto deve compilar sem erros usando referências do SPT 4.0.x e do assembly do EFT 0.16.9.
- [ ] Hooks e patches não devem gerar exceções de MissingMethodException ou TypeLoadException durante o carregamento.
- [ ] Interface (OnGUI) do VAD/VOIP deve exibir o estado real do microfone.
- [ ] **Fika/multiplayer:** A captura de voz Opus e o cliente precisam conversar corretamente entre hosts/clientes em raid cooperativa, ou enviar o pacote para o Fika Headless Server.
- [ ] **Estado entre raids:** O canal deve alternar corretamente para Lobby, Raid e Spectator, e limpar instâncias Opus em `GameWorld.Dispose` ou ao sair de uma partida cooperativa, evitando memory leaks.

## Corner cases

- [ ] O jogador tenta iniciar sem nenhum microfone conectado (array de devices vazio).
- [ ] Retorno imediato ao lobby após disconnect/abort (abort no meio da transmissão).
- [ ] Alteração de dispositivos de áudio default do Windows com o jogo em andamento.

## Fora de escopo

- [ ] Refatoração do OnGUI para Unity UI/Canvas.

## Referências

- PROPRIEDADES.md

## Histórico

| Data | Evento |
|---|---|
| 2026-07-16 | Item criado via `/add-backlog-item` |
