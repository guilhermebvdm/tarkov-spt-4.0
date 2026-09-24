# 003 — Chuva Fraca Mais Visível (Tamanho Mínimo de Gota)

**Mod:** TRL-WeatherSync  
**Status:** Em progresso  
**Criado:** 2026-09-12  

## Visão geral

Permite ajustar a visibilidade e espessura das gotas de chuva durante chuvas fracas (intensidade baixa/nível 1). Sob essas condições no jogo base, as gotas são muito finas e praticamente imperceptíveis ao olhar de frente, sendo notadas quase que exclusivamente de canto de olho (visão periférica). Este item adiciona um piso mínimo de tamanho da gota configurável pelo usuário via menu F12 (Configuration Manager do BepInEx), com atualização ao vivo durante a raid, sem necessidade de reiniciar a partida ou o jogo.

## Comportamento atual

Em condições de chuva fraca (intensidade 1 ou baixa precipitação), o motor gráfico do jogo calcula dimensões reduzidas para as partículas de chuva em queda. O resultado visual é que as gotas tornam-se fios extremamente tênues, quase transparentes quando o jogador olha diretamente para a frente contra fundos escuros ou cenários urbanos/florestais. Jogadores frequentemente relatam a sensação de que não está chovendo ou que só percebem a chuva pelos efeitos periféricos e sons ambientes. Não existe opção nas configurações nativas do jogo para reforçar visualmente a espessura da chuva leve.

## Comportamento desejado

O jogador terá uma opção de configuração no menu F12 (BepInEx Configuration Manager) que estabelece um piso mínimo para a escala/tamanho das partículas de gota de chuva.
- Quando a intensidade do clima estiver baixa e o cálculo padrão do jogo resultar em gotas menores que o piso configurado, o jogo aplicará o tamanho mínimo definido pelo usuário, tornando as gotas claramente visíveis de frente.
- O ajuste surte efeito de imediato ("ao vivo"), permitindo que o usuário teste valores confortáveis dentro de uma raid em andamento sem descarregar a partida ou reiniciar o cliente.
- Caso a chuva aumente naturalmente para níveis moderados ou fortes (tempestade), onde as gotas nativas já superam esse tamanho mínimo, o cálculo padrão do jogo prevalece, sem distorcer nem exagerar o aspecto visual de chuvas pesadas.

## Critérios de aceite

- [x] O usuário pode definir um valor mínimo para o tamanho da gota de chuva através de uma propriedade no menu F12.
- [x] O ajuste no menu F12 tem aplicação imediata (ao vivo) dentro da raid, sem exigir recarregar a cena, reconectar ou reiniciar o jogo.
- [x] Em condições de chuva fraca, as gotas mantêm pelo menos a dimensão configurada no piso mínimo, permitindo clara percepção visual frontal.
- [x] Em condições de chuva forte/intensa, onde o tamanho calculado pelo jogo já seja igual ou superior ao piso mínimo, a escala nativa é mantida inalterada.
- [x] Se a propriedade for definida em 0 (ou valor neutro/mínimo da escala), o comportamento visual do jogo permanece estritamente idêntico ao original (vanilla).
- [x] **Fika/multiplayer:** N/A: puramente visual/local, cada client aplica o próprio ajuste independentemente, sem efeito de rede ou necessidade de sincronização entre peers.
- [x] **Estado entre raids:** Persistência garantida automaticamente pelo sistema de configuração do BepInEx (arquivo `.cfg`), mantendo o valor selecionado entre sucessivas raids e reinicializações do jogo sem necessidade de lógica própria de persistência.

## Corner cases

- [x] **Chuva de intensidade máxima:** Quando a chuva atinge intensidade máxima, o piso mínimo não deve somar nem expandir artificialmente as gotas além do seu tamanho natural de tempestade (o piso atua como limitador inferior via valor máximo entre o calculado e o mínimo, nunca como multiplicador aditivo).
- [x] **Ajuste durante raid sem chuva (tempo seco):** Se o jogador alterar a configuração em tempo aberto/seco, nada é alterado visualmente na tela de imediato; assim que a chuva começar a cair, ela respeitará o novo piso configurado.
- [x] **Configuração definida com valor zero ou inferior:** O piso mínimo é desconsiderado e a renderização opera exatamente como no jogo original, sem overhead nem artefatos visuais.
- [x] **Diferentes resoluções e proporções de tela (Aspect Ratio):** A espessura horizontal da gota deve manter proporção correta com a resolução de tela do jogador (ex: 16:9, 21:9 ultrawide) para não achatar nem distorcer as gotas.

## Fora de escopo

- [ ] Ajuste ou otimização de partículas de neve (`SnowFlakes.cs` — escopo reservado para a pendência futura P-1.4 de otimização de partículas).
- [ ] Modificação da densidade de partículas, velocidade de queda ou quantidade total de gotas na tela.
- [ ] Alteração de efeitos sonoros de chuva ou efeitos de respingo no chão/lentes.

## Referências

- Relato de experiência do usuário: gotas de chuva fraca invisíveis de frente.
- Itens 001 e 002 do backlog de `TRL-WeatherSync`.

## Histórico

| Data | Evento |
|---|---|
| 2026-09-12 | Item criado via handoff Gemini (especificação funcional elaborada). |
| 2026-09-12 | Revisão inline: adicionados corner cases sobre aspect ratio e desativação neutra (valor 0); clarificados critérios de Fika/multiplayer e persistência via BepInEx. |
