# 004 — gerenciador-gc-anti-stutter

**Mod:** TRL-CoreSight  
**Status:** Em progresso  
**Criado:** 2026-09-15T21:00:00Z  

## Visão geral

Eliminar os micro-congelamentos (*stutters* e quedas bruscas de 1% low) causados pela coleta de lixo do runtime Mono da Unity (`Garbage Collection`), suprimindo o coletor durante momentos críticos de jogabilidade (tiroteios, disparos de armas e visada com luneta/red dot) e orquestrando coletas preventivas e controladas apenas em momentos seguros (telas de inventário, momentos ociosos e transições calmas).

## Comportamento atual

1. O Tarkov opera sobre a Unity com o coletor de lixo Mono Boehm, que paralisa a *Main Thread* da CPU (*Stop-the-World*) para varrer a memória do heap sempre que atinge o limite de alocação.
2. Cada pausa dura entre 25ms e 85ms. Para o jogador, isso se manifesta como uma travada nítida e frustrante na tela (queda de 60/100 FPS para 10 FPS por 1 frame).
3. Pela natureza do jogo, o maior volume de alocações temporárias (criação de projéteis, áudios de disparos, cálculo de recuo, chamadas de rede e animações de braço) ocorre exatamente no momento em que o jogador abre fogo ou puxa a mira contra um inimigo.
4. Como o coletor roda de forma aleatória e descontrolada pela Unity, é extremamente comum o *stutter* coincidir com o primeiro tiro do tiroteio, fazendo o jogador errar o disparo ou morrer sem chance de reação.

## Comportamento desejado

1. **Supressão Ativa em Combate e Mira:**
   - Durante o estado de visada (`IsAiming == true`), corrida rápida (*Sprint*) ou quando há disparos de arma recentes (janela de graça de 3 a 5 segundos pós-tiro), definir:
     `UnityEngine.Scripting.GarbageCollector.GCMode = GarbageCollector.Mode.Disabled;`
   - O jogo garante 100% de prioridade para a renderização contínua e processamento de física/balística, eliminando o congelamento na troca de tiros.
2. **Coleta Preventiva e Programada em Momentos Seguros:**
   - Identificar momentos neutros onde o jogador não está sob perigo:
     - **Ao abrir o Inventário / Looting:** O jogador está parado gerenciando mochilas ou caixas; o mod reativa o coletor e dispara `System.GC.Collect()` de forma assíncrona/amortizada.
     - **Timer de Segurança (Cooldown Máximo):** Se o jogador passar mais de 120 segundos sem abrir inventário, mas estiver fora de combate e sem inimigos visíveis em um raio de 40 metros, uma coleta preventiva é disparada suavemente.
3. **Proteção contra Estouro de Heap (OOM):**
   - Monitorar o consumo de memória alocada (`GC.GetTotalMemory(false)`). Se a memória atingir um limiar crítico (ex.: > 85% do espaço pré-alocado do heap), o coletor é reativado imediatamente para prevenir falhas de memória (*Out of Memory*).

## Critérios de aceite

- [ ] Mod monitora o estado de combate e bloqueia o `GarbageCollector` durante a visada (ADS) e trocas de tiro.
- [ ] Eliminação dos micro-stutters recorrentes de primeiro tiro em combates contra bots.
- [ ] A coleta de lixo é executada com sucesso e sem impacto perceptível ao abrir o inventário (`InventoryController`).
- [ ] O consumo total de RAM da raid permanece estável, sem crescimento infinito de heap (leak de memória prevenido pela rotina preventiva de 120s).
- [ ] Tecla de telemetria ou log em modo Debug exibe contagem de pausas suprimidas e memória liberada por coleta.
- [ ] Ao encerrar a raid, o `GCMode` é restaurado para `GarbageCollector.Mode.Enabled`.

## Corner cases

- [ ] **Combates Longos e Ininterruptos (> 3 minutos):** Em tiroteios contínuos sem descanso, a trava de emergência por volume de memória deve permitir coletas rápidas parciais antes que a memória do processo esgote.
- [ ] **Alt+Tab e Pausa de Jogo:** Quando a janela do jogo perde o foco, o coletor deve ser liberado para aproveitar o tempo inativo do jogador para limpeza completa.

## Fora de escopo

- Reescrever o runtime Mono da Unity para IL2CPP ou .NET Core nativo.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-15 | Item criado e especificação funcional inicial definida. |
