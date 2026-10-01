# 012 — otimizacao-stutter-spawn

**Mod:** TRL-DynamicSpawn  
**Status:** 🟢 Entregue  
**Criado:** 2026-09-15T01:30:00-03:00  

> **Perfil desta spec:** Performance e Correção Arquitetural de Esquadrão. O contrato funcional aborda a eliminação de congelamentos de tela (*stutterings* de 200ms–350ms) no instante do spawn de bots e a garantia de coesão tática de esquadrões (líder e seguidores cooperando como uma equipe sob a mesma mente coletiva). Origem técnica: [diagnostico-stutter-e-otimizacao-spawn.md](../../docs/diagnostico-stutter-e-otimizacao-spawn.md).

---

## 1. Visão Geral

Durante o nascimento de bots no **Escape From Tarkov 0.16.9 / SPT 4.0.13**, o cliente do jogo sofria congelamentos de quadro severos (*frame drops* de 90 FPS para 5-10 FPS por múltiplos quadros consecutivos). A investigação técnica no `Assembly-CSharp` revelou três causas convergentes:
1. **Montagem Síncrona de Malhas (`LocalPlayer.cs:81`):** A flag `async: false` na inicialização do jogador força a thread principal da Unity a montar ossos, vestimentas e centenas de componentes de armas no mesmo frame. Ao spawnar um esquadrão atômico de 3 a 4 bots, a retenção da thread ultrapassava 300ms contínuos.
2. **I/O e Descompressão de Bundles em Runtime:** Peças de armaduras e acessórios não presentes na memória da raid eram lidos do disco e descompactados síncronamente durante o frame do spawn.
3. **Disparos Maciços de Raycast de Linha de Visão (LoS):** A checagem de visibilidade contra todos os jogadores vivos disparava dezenas a centenas de `Physics.Linecast` contínuos sem cache espacial ou filtro de distância.
4. **Desagregação de Esquadrões de PMC:** A instanciação atômica gerava uma corrida de estado: os seguidores ativavam antes do Líder ser registrado como chefe (`bot.Boss.SetBoss`), resultando em seguidores sem chefe (`BossToFollow == null`) que se dispersavam individualmente pela zona com táticas solo.

---

## 2. Comportamento Desejado

1. **Pré-Carregamento Assíncrono (Pre-Warming):** Antes de posicionar os bots fisicamente no mapa, o mod carrega assincronamente todos os prefabs e bundles de equipamentos dos perfis gerados através do `PoolManagerClass`, de modo que no instante da instanciação física tudo já resida na RAM/VRAM.
2. **Spawning Escalonado no Tempo (Staggered Spawning):** Esquadrões (`groupSize > 1`) mantêm a geração atômica de perfis, mas a chamada de instanciação física (`TryToSpawnInZoneAndDelay`) é espaçada por um intervalo configurável de 1.2s a 1.5s entre cada membro, eliminando o acúmulo de processamento no mesmo frame.
3. **Coesão e Vínculo de Esquadrão (Líder / Seguidor / SAIN):**
   - O primeiro integrante é spawnado como Líder.
   - O intervalo de 1.2s–1.5s assegura que o Líder atinja `EBotState.Active` e configure `IamBoss = true`.
   - Os seguidores são spawnados em pontos adjacentes próximos ao Líder e vinculam-se como subordinados (`OfferSelf` / `SetToFollow`), adotando a tática militar `Protect` sob o mesmo `BotsGroup` (e mesmo esquadrão do SAIN).
   - Sucessão de Liderança: caso o Líder inicial seja eliminado ou falhe durante o intervalo de spawn, o próximo integrante herda a liderança para os membros subsequentes.
4. **Cache de LoS com Filtro Estrito de 150m:**
   - Jogadores a mais de 150m da zona são imediatamente descartados para testes de Raycast.
   - Teste rápido de cone de visão/viewport precede o disparo de `Physics.Linecast`.
   - Resultados são cacheados em grade quantizada com expiração de 1.5s e teto estrito de 1.000 entradas com purga automática.
5. **Configuração Opcional via F12:**
   - Logs de diagnóstico `[SPY]` sob o toggle `Enable Debug Logs` (desativado por padrão).
   - Delay de escalonamento integrado aos controles existentes `Smooth Spawning` do menu F12.

---

## 3. Critérios de Aceite

### Não-Regressão
- [x] **NR-1:** A quantidade total de bots gerados por onda e respeitando o `MaxBot` da raid permanece inalterada.
- [x] **NR-2:** Spawns de bots individuais (`groupSize == 1`) mantêm o fluxo rápido e não sofrem atrasos artificiais.
- [x] **NR-3:** Bosses e seguidores nativos do jogo (`bossBully`, `bossGluhar`, etc.) permanecem governados pelo subsistema nativo `BossSpawnerClass`.
- [x] **NR-4:** Compatibilidade total com SAIN: bots agrupados no mesmo `BotsGroup` são reconhecidos nativamente pelo SAIN sob o mesmo `BotSquad`.
- [x] **NR-5:** Compatibilidade com Fika: instâncias guest client continuam ignorando o loop de spawn (`FikaHelper.IsClient()`); hosts e instâncias headless processam a IA com estabilidade máxima.

### Metas Mensuráveis
- [x] **AC-M1:** Ausência de travamento consecutivo de tela (> 100ms) durante o spawn de esquadrões de 3 a 5 bots (frame time spike diluído para < 30ms por bot).
- [x] **AC-M2:** Redução de mais de 90% na quantidade de raycasts síncronos de linha de visão executados a cada tick em zonas ativas.
- [x] **AC-M3:** 100% dos integrantes de esquadrões de PMC spawnados juntos mantêm o vínculo com o Líder (`BossToFollow != null`) e atuam em formação conjunta de patrulha/combate.
- [x] **AC-M4:** O cache estático de LoS não ultrapassa 1.000 entradas mesmo em raids de 60 minutos em mapas extensos (Streets/Woods).
