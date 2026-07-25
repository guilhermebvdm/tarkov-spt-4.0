# 002 — canais-comunicacao-spectator

**Mod:** TRL-SpeakFromTarkov
**Status:** Backlog
**Criado:** 2026-07-24

## Visão geral

Implementação de regras estritas de transmissão de áudio e canais de comunicação com base no estado do jogador (Lobby, Raid-Vivo e Espectador-Morto). Espectadores conversam entre si com efeito de reverb espectral e ouvem os vivos, mas os jogadores vivos **JAMAIS** ouvem os mortos.

## Comportamento atual

Atualmente o mod transmite áudio via P2P/FIKA na raid sem separação estrita de canal para espectadores mortos ou efeito espectral.

## Comportamento desejado

- **Canal Lobby:** Comunicação global da party enquanto ajusta inventário e stash.
- **Canal Raid (Vivos):** Comunicação 3D posicional restrita aos jogadores vivos da raid.
- **Canal Raid (Espectador):** Quando o jogador morre (`OnDead`), ele é migrado automaticamente para o canal de espectador.
  - Espectadores ouvem uns aos outros com filtro de reverb espectral suave.
  - Espectadores continuam ouvindo o áudio dos jogadores vivos na raid.
  - Jogadores vivos não recebem o áudio vindo do canal de espectadores.

## Critérios de aceite

- [ ] Transição automática para o canal de espectador ao morrer (`OnDead`).
- [ ] Vivos não escutam áudio de jogadores mortos sob hipótese alguma.
- [ ] Efeito de reverb espectral suave aplicado na saída do alto-falante de espectadores.
- [ ] Limpeza correta de canais ao retornar ao menu.

## Corner cases

- [ ] Jogador morre durante a transmissão ativa de uma frase/fala.
- [ ] Reconexão em partida cooperativa já no estado morto (modo espectador).

## Fora de escopo

- [ ] Efeitos de rádio ou walkie-talkie (cobertos na task 006).

## Referências

- ROADMAP.md §1
