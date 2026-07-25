# 006 — walkie-talkie-radio-hideout

**Mod:** TRL-SpeakFromTarkov
**Status:** Backlog
**Criado:** 2026-07-24

## Visão geral

Mecanismo de comunicação à longa distância via Rádio / Walkie-Talkie equipável no inventário, com aplicação de efeitos sonoros característicos (chiado de frequência, estática, som de *squelch*) e integração de transmissão P2P no menu desbloqueada via construção do Intelligence Center no Hideout.

## Comportamento atual

O VOIP é estritamente posicional e limitado à distância de propagação de voz humana (~30m-60m).

## Comportamento desejado

- **Item Equipável:** Permite transmissão de rádio ilimitada na raid desde que o jogador possua o item Walkie-Talkie equipado.
- **Efeitos de Áudio de Rádio:** Aplicação de efeito Bandpass + filtro de estática/squelch ao enviar e receber transmissões de rádio.
- **Integração Hideout:** Transmissão P2P no menu do jogo vinculada ao nível de construção do **Intelligence Center** no Hideout.

## Critérios de aceite

- [ ] Transmissão de rádio longa distância ativa apenas quando o Walkie-Talkie está equipado.
- [ ] Sons de estática e efeito squelch de início/fim de transmissão aplicados.
- [ ] Verificação da construção do Intelligence Center no Hideout para desbloqueio do canal no menu.

## Corner cases

- [ ] Perder ou dropar o Walkie-Talkie durante a transmissão de rádio.
- [ ] Bateria/durabilidade do rádio (se aplicável).

## Referências

- ROADMAP.md §5
