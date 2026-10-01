# 004 — Gotas na Lente Reativas ao Ângulo da Câmera

**Mod:** TRL-WeatherSync  
**Status:** Em progresso  
**Criado:** 2026-09-12  

## Visão geral

Torna a formação de gotas de chuva na lente da câmera do jogador dinâmica e reativa ao ângulo vertical de inclinação da cabeça (pitch). No jogo base, a formação de gotas na tela parece acontecer quase que de forma estática: olhando para a frente ou para o céu a quantidade de gotas parece insuficiente, e ao olhar para o chão as gotas não param de cair de forma imediata, ficando presas na tela por até 25 segundos. Este item introduz ajustes configuráveis no menu F12 para: (1) aumentar expressivamente a taxa de gotas ao olhar para cima (para o céu), (2) garantir uma taxa confortável e visível de gotas ao olhar para a frente/horizonte mesmo em chuvas leves/garoa, (3) zerar o surgimento de novos pingos ao olhar para o chão e (4) acelerar o escorrimento/secagem das gotas existentes ao inclinar a cabeça para baixo (simulando a proteção da aba do capacete/boné e a chuva caindo na nuca do operador).

## Comportamento atual

1. **Pouca sensibilidade angular:** A fórmula original de ângulo varia muito pouco entre olhar para a frente e olhar para o céu (uma variação de apenas ~66% no multiplicador interno). Para o jogador, olhar diretamente para cima em direção à chuva caindo produz quase a mesma impressão de estar olhando para a frente.
2. **Gotas escassas em chuva leve / garoa:** O número de gotas que nascem simultaneamente é severamente reduzido quando a intensidade climática é baixa. Em garoa, raramente se vê mais de 1 pingo a cada vários segundos, fazendo parecer que a lente está completamente seca.
3. **Persistência excessiva na tela (Gotas "congeladas"):** As gotas têm um tempo de vida nativo de 25 segundos (quando com óculos) e um limite de pool de apenas 32 gotas simultâneas. Quando essas 32 gotas são alocadas, novos pingos deixam de nascer, criando uma tela estática que não se renova.
4. **Sem alívio ao olhar para o chão:** Olhar para baixo não limpa a visão; as gotas existentes permanecem cobrindo a tela por quase meio minuto, mesmo que o jogador esteja olhando para os próprios pés.

## Comportamento desejado

O jogador terá opções no menu F12 (sob a seção `Lens Drops`) para calibrar ao vivo a resposta das gotas na lente:
- **Ao olhar para o céu (pitch positivo elevado):** A frequência e a quantidade de gotas por rajada aumentam significativamente (configurável via `Look Up Multiplier`, padrão 2.5x a 3x), transmitindo a sensação realista e imersiva de tomar chuva diretamente no rosto/viseira.
- **Ao olhar para a frente (horizonte):** Aplica-se um multiplicador base (`Forward Rate Multiplier`) que assegura que mesmo em garoas ou chuvas leves os pingos continuem se formando com regularidade perceptível.
- **Ao olhar para o chão (pitch negativo):** O surgimento de novas gotas é imediatamente cortado a zero.
- **Secagem rápida ao olhar para o chão (`Look Down Drain Effect`):** Se ativado, as gotas que já estavam na tela desvanecem rapidamente enquanto a cabeça estiver inclinada para baixo, proporcionando uma mecânica tática intuitiva onde o jogador pode abaixar a cabeça por 1 ou 2 segundos para "limpar" a visão em meio a um combate sob chuva.
- **Tempo de vida dinâmico (`Max Drop Lifetime Seconds`):** Reduz o tempo máximo de permanência de cada gota na tela (de 25s para cerca de 6s a 8s), permitindo uma renovação contínua e orgânica do pool de gotas.

## Critérios de aceite

- [x] O usuário pode ativar ou desativar o ajuste de gotas na lente via `Enable Lens Drops Tuning` no F12.
- [x] Ao olhar para cima em direção ao céu sob chuva aberta, a taxa de novos pingos na tela é visivelmente superior à taxa observada ao olhar para o horizonte.
- [x] Ao olhar para a frente no horizonte, a presença de gotas na tela é regular e perceptível, inclusive em chuvas de intensidade baixa (garoa).
- [x] Ao inclinar a visão para baixo (olhando para o chão), a geração de novos pingos na tela cessa completamente.
- [x] Com o efeito de secagem ativado (`Look Down Drain Effect`), as gotas existentes na tela evaporam/desaparecem com velocidade acelerada enquanto o jogador mantiver a visão voltada para o chão.
- [x] Todas as alterações de configuração no menu F12 possuem efeito imediato (ao vivo) dentro da raid, sem exigir reinicialização.
- [x] Se `Enable Lens Drops Tuning` for desmarcado (`false`), o comportamento da câmera retorna 100% ao padrão original do jogo (vanilla).
- [x] **Fika/multiplayer:** N/A: Efeito puramente visual e local na câmera do cliente; não envolve rede nem sincronização de pacotes entre peers.
- [x] **Estado entre raids:** Persistência automática das opções pelo arquivo de configuração `.cfg` do BepInEx.

## Corner cases

- [x] **Jogador abrigado sob teto / cobertura:** O efeito respeita `RainController.IsCameraUnderRain`. Se o jogador estiver dentro de uma casa ou sob marquise, nenhuma gota pinga na câmera, mesmo olhando para cima.
- [x] **Transição rápida de ângulo (chacoalhar a mira):** A aceleração de secagem só ocorre enquanto a inclinação estiver mantida para baixo, sem saltos abruptos ou flickering visual.
- [x] **Tempo seco (sem chuva):** Em tempo aberto sem precipitação, a rotina não executa spawn nem afeta o pós-processamento da câmera.
- [x] **Raid solo vs. coop:** Funciona de maneira idêntica em qualquer modo de jogo (solo SPT ou coop FIKA).

## Fora de escopo

- [ ] Modificação das texturas das gotas de tela (usadas pelo shader nativo `_DudvMap`).
- [ ] Alteração de efeitos visuais de sangramento na tela (`bloodOnScreen_0`) ou efeito de visão dupla/tontura.

## Referências

- [`RainScreenDrops.cs:23-53`](../../../../references/eft-decompiled/Assembly-CSharp/RainScreenDrops.cs#L23)
- [`GClass986.cs:114-138`](../../../../references/eft-decompiled/Assembly-CSharp/GClass986.cs#L114)
- [`GClass987.cs:114-132`](../../../../references/eft-decompiled/Assembly-CSharp/GClass987.cs#L114)

## Histórico

| Data | Evento |
|---|---|
| 2026-09-12 | Item criado via solicitação do usuário para aprimorar resposta angular e frequência das gotas na lente. |
| 2026-09-12 | Revisão inline: adicionados critérios de escorrimento/secagem tática e tempo de vida dinâmico para renovação orgânica do pool de partículas. |
