# 010 — afinidade-cpu-topologia-threads

**Mod:** TRL-CoreSight
**Status:** Backlog
**Criado:** 2026-09-19

## Visão geral

O motor do jogo sofre de severas quedas de desempenho e micro-travamentos (*stutters*) decorrentes de decisões subótimas do agendador do sistema operacional, que frequentemente espalha as tarefas críticas do ciclo de jogo entre núcleos virtuais secundários, cruza barramentos de alta latência entre blocos físicos distintos de cache (como os complexos de núcleos em processadores multi-bloco) ou direciona processamento pesado para núcleos de baixa eficiência energética em arquiteturas híbridas. Este módulo implementa um orquestrador universal e seguro de afinidade de hardware que interroga a topologia física da máquina e restringe as tarefas principais do jogo aos núcleos físicos mais rápidos e com menor latência de memória, prevenindo gargalos na execução do ciclo de jogo e na simulação de inteligência artificial.

## Comportamento atual

- O agendador do sistema operacional distribui as tarefas do jogo aleatoriamente por todos os processadores lógicos disponíveis (incluindo threads secundárias hiper-thread e núcleos de eficiência energética).
- Em processadores com múltiplos blocos físicos de cache, a linha de execução principal do jogo frequentemente transita entre blocos distintos, gerando penalidades de latência de barramento a cada troca de contexto.
- Em processadores de arquitetura híbrida, tarefas pesadas de simulação e renderização podem ser atribuídas temporariamente a núcleos lentos de economia de energia.
- A opção nativa existente nas configurações de jogo falha frequentemente ao trocar de mapas ou ao ingressar em novas partidas, além de não reconhecer topologias de cache de múltiplos blocos.

## Comportamento desejado

- O sistema deve interrogar a topologia real do processador na inicialização da partida e construir uma máscara de afinidade ótima para a máquina do jogador.
- Deve permitir a restrição das tarefas do processo aos núcleos físicos reais, ignorando as threads lógicas secundárias que concorrem pelo mesmo cache de primeiro e segundo nível.
- Em arquiteturas multi-bloco, deve oferecer a capacidade de fixar a linha de execução principal do jogo no bloco de cache de menor latência e maior velocidade.
- Em processadores híbridos, deve isolar e garantir que a carga do jogo utilize exclusivamente os núcleos de alta performance, desativando o uso de núcleos de eficiência para o executável do jogo.
- A configuração de afinidade deve ser restabelecida automaticamente a cada início de partida, assegurando que rotinas internas do motor de jogo não desfaçam a alocação de núcleos durante telas de carregamento.

## Critérios de aceite

- [ ] Identificar automaticamente a topologia física da CPU (núcleos físicos, threads lógicas, clusters de cache de terceiro nível e classes de eficiência) sem exigir configuração técnica manual do jogador.
- [ ] Aplicar a máscara de afinidade selecionada diretamente ao processo do jogo no início de cada partida de forma atômica e silenciosa.
- [ ] Sobrescrever de forma confiável a opção nativa do jogo ("Usar apenas núcleos físicos" da BSG) caso esteja marcada nas configurações gerais do EFT, assegurando que a máscara inteligente do mod prevaleça no ciclo de raid.
- [ ] Reagir dinamicamente a mudanças de configuração no menu F12 durante a partida, recalculando e reaplicando a máscara no mesmo quadro sem exigir reinício da raid ou do cliente.
- [ ] Restaurar a afinidade completa original do sistema operacional (todas as threads lógicas ativas) caso o usuário selecione a opção "Desativado".
- [ ] Implementar salvaguarda que impeça o isolamento de menos de 4 núcleos lógicos, protegendo processadores de entrada contra asfixia de recursos.
- [ ] Disponibilizar no menu de configurações opções acessíveis para o usuário: modo Automático (recomendado), Apenas Núcleos Físicos (sem threads virtuais), Priorizar Núcleos de Performance (híbridos), Fixar Bloco Principal (AMD CCX/CCD) e Desativado.
- [ ] **Fika/multiplayer:** Operar de maneira totalmente transparente tanto em computadores atuando como servidor/host quanto em computadores de clientes conectados, garantindo que o isolamento de núcleos beneficie a simulação local de IA sem estrangular a linha de execução de rede UDP.
- [ ] **Estado entre raids:** Manter a afinidade estável e consistente ao longo de ciclos repetidos de jogo (Raid 1 → Pós-Raid / Menus → Raid 2), revalidando a máscara imediatamente na transição de cenas sem vazamentos ou erros de alocação.

## Corner cases

- [ ] **Processadores de entrada (4 núcleos ou menos):** O sistema deve abortar automaticamente qualquer corte de threads virtuais caso a máquina possua 4 ou menos núcleos lógicos, mantendo todas as threads disponíveis para não estrangular a taxa de quadros.
- [ ] **Opção nativa da BSG ativa nas preferências de jogo:** Caso o usuário tenha deixado "Usar apenas núcleos físicos" marcado nas opções de jogo da BSG, o mod deve sobrepor a máscara ingênua da BSG após o carregamento da cena.
- [ ] **Alt+Tab e perda de foco:** O agendador do sistema operacional pode tentar redistribuir afinidades ao alternar janelas em segundo plano; a máscara deve persistir íntegra ao retornar o foco para a janela do jogo.
- [ ] **Processadores com Cache 3D assimétrico (ex: 7950X3D):** Em chips com múltiplos blocos onde apenas um dos blocos possui cache vertical expandido, o sistema deve priorizar a alocação dos núcleos do bloco enriquecido com cache.
- [ ] **Sistemas com mais de 64 processadores lógicos (Processor Groups):** Em estações de trabalho de altíssimo desempenho, o sistema deve respeitar o limite de bits da arquitetura de grupo de processadores sem lançar exceções de estouro de ponteiro.

## Fora de escopo

- [ ] Modificações em frequências de clock, tensões elétricas, perfis de ventoinhas ou quaisquer parâmetros de overclocking via BIOS.
- [ ] Alteração na afinidade de processos externos do sistema operacional (navegadores, aplicativos de comunicação ou utilitários em segundo plano).

## Referências

- API do Sistema Operacional: `GetLogicalProcessorInformationEx`, `SetProcessAffinityMask` e `SetThreadAffinityMask`.
- [mod-backlog.md](../mod-backlog.md)

## Histórico

| Data | Evento |
|---|---|
| 2026-09-19 | Item criado via `/add-backlog-item` |
| 2026-09-19 | Revisão `/review-spec` — Adicionada sobreposição à opção vanilla da BSG, reatividade in-live no F12, restauração total em modo desativado e corner case de processadores >64 threads |
