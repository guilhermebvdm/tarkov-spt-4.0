# 007 — otimizacoes-hud-multithread

**Mod:** TRL-SpeakFromTarkov
**Status:** Backlog
**Criado:** 2026-07-24

## Visão geral

Melhorias de desempenho no pipeline de áudio (decodificação Opus em *ThreadPool* paralela), adição de um HUD visual minimalista e discreto na tela durante o jogo, e sliders de controle de volume individual por jogador no menu F12.

## Comportamento atual

A decodificação Opus é executada sequencialmente e a interface de visualização atual é o painel de Profiler completo no `F9`.

## Comportamento desejado

- **Decodificação Multithread:** Mover o processamento `OpusDecoder.Decode` para tarefas assíncronas na *ThreadPool* evitando quedas de FPS em raids com muitos falantes.
- **HUD Minimalista:** Um pequeno indicador sutil no canto da tela mostrando se o microfone está captando (VAD/PTT), o canal ativo e estado de mute.
- **Controle Individual de Volume:** Sliders no menu de configuração (F12) para ajustar o volume de cada membro do grupo individualmente.

## Critérios de aceite

- [ ] Decodificação Opus realizada fora da thread principal de renderização.
- [ ] HUD minimalista discreto e funcional exibido na tela durante a partida.
- [ ] Ajuste individual de volume por parceiro no menu F12.

## Corner cases

- [ ] Vários jogadores falando ao mesmo tempo sob estresse de rede.
- [ ] Ocultamento automático do HUD em telas de carregamento ou inventário.

## Referências

- ROADMAP.md §6
