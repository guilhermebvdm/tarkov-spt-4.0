# 004 — Contador de vidas na tela

**Mod:** TRL-PvpMode
**Status:** Backlog
**Criado:** 2026-08-01
**Depende de:** [002 — Renascer em spawn aleatório](../002-renascer-spawn-aleatorio/002-renascer-spawn-aleatorio-01-spec.md)

## Visão geral

Mostrar quantas vidas restam. O momento que mais importa é **estando caído** — é ali que a decisão de
gastar uma vida é tomada, e hoje o jogador decide no escuro.

## Comportamento atual

O número de vidas só aparece em duas mensagens passageiras: no início da partida (no log) e logo depois
de renascer. Enquanto o jogador está caído decidindo, nada na tela diz quantas ainda tem.

## Comportamento desejado

1. Um indicador discreto no canto da tela mostra as vidas restantes durante a partida.
2. Estando caído, o indicador fica **destacado** — é a informação mais relevante do momento.
3. Com vidas ilimitadas, o indicador mostra o símbolo de infinito em vez de um número.
4. Com o modo desligado ou fora de partida, nada aparece.
5. Uma opção no F12 permite esconder o indicador para quem prefere a tela limpa.

## Critérios de aceite

- [ ] O indicador aparece ao entrar na partida e some ao sair, sem sobrar na tela do menu.
- [ ] O número cai em um a cada renascimento e bate com o comportamento real (renascer com o contador
      em zero não é possível).
- [ ] Com `Lives Per Raid = -1`, o indicador mostra infinito e nunca diminui.
- [ ] Estando caído, o indicador fica visualmente distinto de quando se está de pé.
- [ ] Desligando a opção no F12, o indicador some imediatamente, sem exigir reiniciar a partida.
- [ ] **Fika/multiplayer:** o indicador mostra **as vidas de quem está olhando** — nunca as de outro
      jogador. `N/A` para sincronia: é informação puramente local, nada trafega na rede.
- [ ] **Estado entre raids:** o indicador reflete o valor reiniciado a cada partida.

## Corner cases

- [ ] **Modo desativado por pré-requisito ausente** (resgate do Fika desligado no servidor, PlayerLives
      instalado). O indicador não pode aparecer prometendo vidas que não existem.
- [ ] **Esconderijo e menu.** Nada desenhado.
- [ ] **Tela de fim de raid.** O indicador não pode ficar por cima.

## Fora de escopo

- [ ] Mostrar as vidas dos companheiros.
- [ ] Personalização de posição, cor ou tamanho além de ligar/desligar.

## Histórico

| Data | Evento |
|---|---|
| 2026-08-01 | Item criado via `/add-backlog-item` |
| 2026-08-01 | Spec funcional criada via `/create-spec` |
