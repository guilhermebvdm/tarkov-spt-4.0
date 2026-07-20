# Roadmap — TRL-SpeakFromTarkov

> Lista de funcionalidades futuras planejadas além do MVP inicial (portabilidade para SPT 4).

## Canais de Comunicação
- **Menu/Lobby:** Jogadores no menu do jogo podem se comunicar (canal global da party).
- **Raid-Vivo:** Comunicação posicional 3D restrita a jogadores vivos dentro da mesma raid.
- **Raid-Morto (Spectator):** Canal de comunicação exclusivo para os jogadores mortos na raid (não ouvidos pelos vivos).

## Walkie-Talkie / Rádio
- **Objeto Equipável:** Necessário ter um item "Walkie-Talkie" no inventário/slot específico para se comunicar à distância.
- **Efeitos de Áudio:** Adição de chiados de rádio, estática, e efeitos de *squelch* (abertura e fechamento de transmissão).
- **Atenuação por Distância e Frequência:** Canais configuráveis e perda de sinal baseada na distância no mapa.

## Otimizações Arquiteturais
- **Decodificação Multithread:** Deslocar o `OpusDecoder.Decode` da Main Thread (no `Update` atual) para *Threads Paralelas* (Task.Run / ThreadPool) para suportar dezenas de jogadores com mic aberto simultaneamente sem custo perceptível de CPU.

## Interação com IA (Bots)
- **Detecção de Voz pelos Bots:** Fazer com que o som do microfone do jogador "chame a atenção" dos bots próximos.
- **Implementação "Isca Silenciosa":** Como a IA nativa do Tarkov/SAIN não reage a áudio injetado na Unity via rede, o mod deve instanciar um som fantasma (ex: som de cura ou de tiro silencioso) na posição do jogador sempre que ele falar alto no microfone. O som deve ser silenciado para o próprio cliente (para não quebrar a imersão), mas disparar o gatilho de investigação da IA nativa, fazendo os bots virarem e caminharem na direção da voz.
