# 005 — camuflagem-natural-e-percepcao-ia

**Mod:** TRL-CoreSight  
**Status:** Em progresso  
**Criado:** 2026-09-15T21:00:00Z  
**Atualizado:** 2026-09-15T21:38:00Z  

## Visão geral

Implementar no `TRL-CoreSight` o **Sistema de Camuflagem Natural e Oclusão de Visão de IA** (*Natural Concealment & AI Vision Occlusion*), complementado por **Time-Slicing suave de decisão de bots distantes**. 

O sistema elimina os dois maiores problemas crônicos de combate à distância do Tarkov:
1. Bots enxergando o jogador através da copa e folhas de árvores a 150m–200m de distância (decorrente da ausência de colisor nas copas pela BSG).
2. Bots cravando tiros na cabeça de jogadores deitados em capinzais e grama alta (decorrente da grama ser apenas um desenho visual da GPU sem presença física).

A tecnologia opera garantindo a "verdade do entorno do jogador" (*Player Proximity Shield*), projetando barreiras de oclusão de visão física no layer `LayerMaskClass.AI` no raio imediato do jogador, permitindo que o SAIN reaja de forma 100% orgânica com tiros de supressão, granadas e buscas na última posição conhecida.

## Comportamento atual

1. **Copas de Árvores sem Colisor:** A BSG configurou colisor físico rígido apenas nos troncos de madeira das árvores. As copas (folhas e galhos) não possuem colisor no layer `Foliage`. Para o raycast de visão da IA ([`VisionRaycastJob.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SAIN/modded-multithread/SAIN/Classes/BotManager/Jobs/VisionRaycastJob.cs)), as folhas são transparentes como o ar.
2. **Grama Alta sem Física:** Todo o gramado e capim do mapa é desenhado em lote na GPU via [`GPUInstancerDetailManager`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/GPUInstancer/GPUInstancerDetailManager.cs). Nenhum tufo de grama possui colisor físico. Ao deitar no capim alto de 1 metro, a visão do jogador fica 100% obstruída por folhas, mas o bot o enxerga com clareza total.
3. **Desperdício de CPU em Bots Distantes:** Bots em estado de paz ou a mais de 100 metros processam varreduras contínuas a 30–60 Hz na *Main Thread*, gerando gargalo desnecessário de CPU.

## Comportamento desejado

### 1. Escudo de Copa de Árvores e Arbustos (Raio de 10 metros)
- Monitorar árvores e arbustos próximos ao redor do jogador (`MainPlayer`):
  - Arbustos do tipo `ObstacleCollider` (prefabs `filbert`, `fibert`, `swamp`).
  - Árvores do cenário com copas densas.
- Garantir que qualquer arbusto ou copa de árvore a até 10m do jogador projete uma barreira física no layer de visão da IA (`LayerMaskClass.AI`).
- **Efeito:** O raio de visão de qualquer bot no horizonte (a 50m, 150m ou 300m) viaja livremente pelo mapa, mas ao alcançar o entorno do jogador, bate na folhagem da árvore/moita e bloqueia a visão (`CanBeSeen = false`).

### 2. Camuflagem em Grama Alta com Base no Tamanho Real (Raio de 2 metros)
- Ler a presença e altura da grama diretamente dos dados de memória do [`GPUInstancerDetailManager`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/GPUInstancer/GPUInstancerDetailManager.cs) (`threadDetailMapData`):
  - **Deitado (`IsInPronePose == true`):** Se o solo abaixo do jogador for grama/terra com vegetação e houver capim alto registrado, ativa uma barreira volumétrica rasteira de 2 metros de raio até a altura real da ponta da grama (ex.: 0,85m). O jogador fica **100% oculto** para bots a média/longa distância.
  * **Agachado (`Crouched`):** O tronco inferior fica protegido; a cabeça permanece parcialmente exposta caso o bot tenha linha de visada desobstruída por cima.
  * **Em Pé (`Standing`):** A barreira rasteira não cobre o tronco/cabeça; o bot detecta o jogador normalmente.
  * **Terreno Limpo / Asfalto / Pisos Internos:** Se a densidade de grama for zero, nenhuma proteção é gerada (zero escudo invisível fora do capim).

### 3. Integração Orgânica com a Memória e Supressão do SAIN
- Como a barreira apenas interrompe a física do raycast (`RaycastHit` no layer `AI`), o SAIN reage de maneira 100% nativa:
  - **Memória de Posição:** O bot grava [`LastKnownPosition`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SAIN/modded-multithread/SAIN/Classes/Bot/EnemyClasses/Position/EnemyKnownPlaces.cs) no local onde viu o jogador antes de ele sumir no capim/arbusto.
  - **Tiro de Supressão:** O bot executa rajadas de supressão ([`SuppressPosition`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SAIN/modded-multithread/SAIN/Classes/Bot/WeaponFunction/SAINBotSuppressClass.cs)) cortando a vegetação na direção da última posição conhecida.
  - **Granadas e Busca:** Lança granadas de fragmentação e inicia avanço tático (`SearchAction`) até a posição.

### 4. Regras de Fairplay e Quebra de Camuflagem
- **Distância de Tropeço (< 4 metros):** Se um bot se aproximar a menos de 4 metros no mato, a proximidade extrema quebra a camuflagem e o bot detecta o jogador deitado.
- **Disparo sem Silenciador:** Tiros ruidosos revelam a coordenada sonora para os sensores de audição dos bots ([`BotHearing`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SAIN/modded-multithread/SAIN/Patches/HearingPatches.cs)).
- **Passagem de Balas Intacta:** A barreira de vegetação afeta **apenas** o layer de visão; projéteis balísticos continuam atravessando normalmente.

### 5. Time-Slicing Suave de Decisão (Alívio de CPU)
- Bots pacíficos (`IsPeace == true`) ou a mais de 100 metros têm sua checagem de visão espaçada para 2 a 3 Hz (a cada 0,3s–0,4s).
- Ao primeiro disparo ou contato, o bot entra em combate ativo e o time-slicing é desativado imediatamente, mantendo tiroteios distantes vivos e dinâmicos no mapa.

## Critérios de aceite

- [ ] Jogador deitado em capinzal alto não é detectado por bots a média/longa distância (> 15 metros).
- [ ] Jogador agachado atrás de arbusto denso (`filbert`/`swamp`) ou copa de árvore não toma tiros milagrosos através das folhas.
- [ ] Se o bot vir o jogador entrando na moita/capim, ele dispara tiros de supressão e arremessa granadas na última posição vista.
- [ ] Ao ficar em pé na grama, o bot volta a detectar o jogador visualmente.
- [ ] Ao deitar no asfalto, concreto ou assoalho sem grama, a camuflagem não é aplicada.
- [ ] Bots a menos de 4 metros no capim detectam o jogador por proximidade imediata.
- [ ] Tiros de armas continuam penetrando a vegetação normalmente (balística e dano preservados).
- [ ] Redução de consumo de CPU de IA em raids lotadas via time-slicing em bots distantes fora de combate.
- [ ] Nenhum impacto adverso nos tiroteios de fundo entre bots PMC e Scavs.

## Corner cases

- [ ] **Troca Rápida de Postura (Spam de Prone/Stand):** A ativação/desativação da barreira deve ser amortizada para não gerar micro-stutters ou vazamento de colisores.
- [ ] **Morte do Jogador na Grama:** Limpeza imediata da barreira no frame da morte (`!IsAlive`).
- [ ] **Luzes e Lasers:** Feixes de lanternas visíveis ou lasers apontados pelo jogador de dentro da moita denunciam a posição.

## Fora de escopo

- Alteração na densidade ou no shader visual da grama renderizada pelo `GPUInstancer`.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-15 | Item estruturado no backlog. |
| 2026-09-15 | Escopo consolidado: adicionada camuflagem de árvores/arbustos (10m), grama alta por GPUInstancer (2m), validação orgânica com SAIN e time-slicing suave. Status atualizado para Em progresso. |
