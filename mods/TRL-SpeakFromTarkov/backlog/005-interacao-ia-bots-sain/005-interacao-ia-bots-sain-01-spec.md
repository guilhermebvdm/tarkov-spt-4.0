# 005 — interacao-ia-bots-sain

**Mod:** TRL-SpeakFromTarkov
**Status:** Backlog
**Criado:** 2026-07-24

## Visão geral

Integração da fala do jogador pelo microfone com a inteligência artificial dos bots (Scavs, PMCs, Bosses e SAIN). O mod utiliza o disparador nativo `EPhraseTrigger.OnMutter` silenciado para ouvidos humanos ( volume 0% local), mas captado 100% pelos sensores de audição 3D dos bots, fazendo-os virar a cabeça e responder verbalmente.

## Comportamento atual

Os bots do jogo são nativamente "surdos" ao microfone dos jogadores humanos.

## Comportamento desejado

- Ao falar no microfone com transmissão ativa (`IsTransmitting == true`), o mod emite um gatilho de fala nativa silenciado localmente a 0% de volume.
- O sensor de audição da IA (`BotHearingSensor.cs` / SAIN) capta o sinal posicional no mundo 3D no local exato de onde o jogador falou.
- Os bots reagem virando na direção da voz, buscando cobertura ou **respondendo verbalmente** em 3D (ex: Scavs gritando *"Cheki Breki!"*, *"Opachki!"*).

## Critérios de aceite

- [ ] Bots próximos viram a cabeça ou reagem investigando a posição de onde o jogador falou no microfone.
- [ ] O gatilho de áudio nativo não produz duplicação de voz humana audível para os jogadores da partida (0% volume local).
- [ ] Resposta verbal dos bots em 3D integrada com sucesso.
- [ ] Compatibilidade testada com SAIN e bot controller vanilla.

## Corner cases

- [ ] Falar continuamente no microfone (debounce/cooldown para não floodar os sensores da IA).
- [ ] Bots surdos ou sob efeito de concussão por granada.

## Referências

- ROADMAP.md §4
