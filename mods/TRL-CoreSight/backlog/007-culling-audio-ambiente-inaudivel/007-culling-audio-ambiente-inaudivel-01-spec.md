# 007 — culling-audio-ambiente-inaudivel

**Mod:** TRL-CoreSight  
**Status:** Em progresso  
**Criado:** 2026-09-15T21:48:30Z  

## Visão geral

Otimizar o subsistema de áudio da Unity e as threads de mixagem/espacialização em raid, pausando ou desativando temporariamente fontes de áudio ambiente estáticas e em loop ([`AudioSource`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/EFT/Audio/)) cujas posições estão muito além do seu raio máximo de audibilidade (`maxDistance`) ou completamente ocluídas pela geometria do mapa.

## Problema e custo de CPU

1. Os mapas do Tarkov (como Interchange, Reserve, Customs e Streets) possuem centenas de emissores de áudio ambiente contínuos:
   - Zumbidos de reatores de lâmpadas fluorescentes;
   - Geradores a diesel e bombas industriais;
   - Goteiras e dutos de ventilação;
   - Fogueiras e estalos de fogo estáticos.
2. Mesmo quando o jogador está a 150 metros de distância e a atenuação acústica da Unity reduz o volume audível a zero absoluto, todos esses emissores continuam com `isPlaying = true`.
3. O mixer de áudio da Unity e o plugin de espacialização acústica continuam calculando curvas de atenuação de distância, reflexões e atualizações vetoriais para centenas de emissores inaudíveis a cada ciclo, gerando consumo constante e desnecessário de CPU.

## Comportamento desejado

1. **Varredura e Registro de Emissores Ambientes Contínuos:**
   - No início da raid (após carregamento do mapa), identificar `AudioSource` estáticos marcados com `loop = true` pertencentes ao ambiente do cenário.
2. **Culling Inteligente por Distância Efetiva:**
   - Para cada emissor ambiente em loop:
     - Se `distância(MainPlayer, AudioSource) > (maxDistance + 10m)`: Pausar o áudio (`audioSource.Pause()` ou desativar o componente).
     - Se `distância(MainPlayer, AudioSource) <= maxDistance`: Retomar a reprodução (`audioSource.UnPause()`).
3. **Imunidade Estrita para Áudios Táticos e Dinâmicos:**
   - **100% Imunes:** Passos de jogadores/bots, tiros, estalos de projéteis, recargas de armas, vozes de bots (voicelines/scav mumbles), explosões de granadas e acionamento de sirenes de extração NUNCA são tocados por este sistema.
   - O sistema atua exclusivamente em emissores ambientais estáticos de cena.

## Critérios de aceite

- [ ] Fontes contínuas de áudio ambiente que estejam inaudíveis devido à distância têm sua reprodução pausada na CPU.
- [ ] Ao aproximar-se de um gerador ou lâmpada que estava pausada, o som é retomado perfeitamente sem cliques, cortes abruptos ou estouros sonoros.
- [ ] Nenhum som emitido por bots, jogadores ou projéteis sofre interferência ou atraso.
- [ ] Redução da carga de trabalho no profiler da thread de áudio da Unity (`AudioMixer::Update`, `FMOD/UnityAudio`).

## Fora de escopo

- Alterar o volume, pitch ou características acústicas dos efeitos sonoros do jogo.
- Modificar o sistema de passos ou oclusão de combate.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-15 | Item aprovado pelo usuário; especificação funcional inicial criada. |
