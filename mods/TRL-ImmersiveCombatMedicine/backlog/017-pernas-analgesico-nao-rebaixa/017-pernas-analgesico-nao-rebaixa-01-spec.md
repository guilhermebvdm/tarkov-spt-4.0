# 017 — Pernas: analgésico não rebaixa 2 pernas zeradas

**Mod:** TRL-ImmersiveCombatMedicine
**Status:** Entregue
**Criado:** 2026-07-26

## Visão geral

Achado **S1.7** do 1º teste in-game (2026-07-26): com as duas pernas zeradas e analgésico ativo, o jogador **conseguia correr**. Pedido do usuário: *"Com analgésico → Aplicar N2 e não deixar correr, igual acontece quando tem as 2 pernas zeradas"*.

Não é bug de implementação — é o comportamento que a matriz especificava. A matriz aprovada em 2026-07-18 rebaixava `2 pernas zeradas + analgésico` de Manqueira Severa para Manqueira **Leve**, e o bloqueio de sprint do mod só vale no tier Severa. A decisão de produto mudou.

## Comportamento atual

- Duas pernas zeradas **sem** analgésico → Manqueira Severa + um agachar involuntário, e o sprint fica bloqueado (inclusive se o jogador tomar analgésico depois, porque o bloqueio do tier Severa é mantido de propósito).
- Duas pernas zeradas **com** analgésico no momento da avaliação → rebaixa para Manqueira **Leve**. Como o bloqueio de sprint é uma regra do tier Severa, a Leve não bloqueia nada: o jogador corre.
- Efeito colateral do rebaixamento: tomar analgésico com duas pernas zeradas era a forma mais eficiente de recuperar mobilidade total no mod — mais eficaz que curar uma das pernas.

## Comportamento desejado

- Duas pernas zeradas passa a ser **sempre Manqueira Severa**, com ou sem analgésico, e portanto **sempre com sprint bloqueado**.
- O analgésico continua removendo o **agachar involuntário** — é essa a diferença que ele passa a fazer nessa linha, em vez de rebaixar a manqueira inteira. Quem toma analgésico com duas pernas zeradas deixa de cair de joelhos, mas continua sem correr.
- A velocidade-alvo é a do tier Severa (default 55%), não a do Leve (80%).
- As demais linhas da matriz de pernas **não mudam**: 1 zerada, 1 quebrada, 1+1, 2 quebradas e 2 zeradas+2 quebradas seguem como estão.

## Critérios de aceite

- [x] Com as duas pernas zeradas e analgésico ativo, o jogador **não corre** e manca com a intensidade da Manqueira Severa.
- [x] Nessa mesma condição, **não** há agachar involuntário — o analgésico continua suprimindo o one-shot.
- [x] Tomar o analgésico **depois** de já estar com as duas pernas zeradas não devolve a corrida (antes devolvia).
- [x] Deixar o analgésico expirar nessa condição volta ao comportamento completo (Severa **com** agachar), respeitando o intervalo anti-repetição.
- [x] Nenhuma outra linha da matriz de pernas muda de resultado — verificável pelos testes de mesa no próprio resolvedor.
- [x] **Fika/multiplayer:** sem impacto de rede. O estado é calculado por quem é dono do personagem e o peer só observa pose e velocidade pelo sync nativo; nenhum pacote muda.
- [x] **Estado entre raids:** sem impacto. A linha é recalculada na entrada da raid a partir do estado real dos membros.

## Corner cases

- [x] **1 zerada + 1 quebrada com analgésico** continua em Manqueira Leve — é uma linha diferente e não foi tocada.
- [x] **2 zeradas + 2 quebradas com analgésico** já resultava em Manqueira Severa antes desta mudança; continua igual (agora por dois caminhos que concordam, em vez de um só).
- [x] **Bots** seguem a mesma matriz, sem exceção — bots com analgésico permanente (bosses) ficam estáveis na Severa em vez da Leve. É consequência esperada, não bug.
- [x] **Sem analgésico, o vanilla domina a velocidade.** O EFT já impõe sua própria penalidade de perna, mais severa que a do mod, e a composição é pelo mínimo — ver o registro da fronteira mod↔vanilla no item 021. A calibração do mod só é observável **com** analgésico, que é exatamente a coluna que este item corrige.

## Fora de escopo

- [x] Permitir correr com **uma** perna comprometida (S1.1) — investigado e **inviável** sem o mod forçar sprint acima do vanilla, o que a regra de projeto proíbe. Registrado como limitação assumida no item 021.
- [x] Mudar as velocidades-alvo (80% / 55%) ou tornar o bloqueio de sprint configurável por linha — o toggle `Block Sprint On N2` continua como está.
- [x] Mudar a linha "Cair" (2 pernas quebradas) ou o ciclo de queda.

## Referências

- [Patches/Trauma/TraumaMatrixResolver.cs](../../modded/Patches/Trauma/TraumaMatrixResolver.cs) (única mudança de código)
- [docs/trauma-behavior-matrix.md](../../docs/trauma-behavior-matrix.md) (matriz a atualizar)
- [docs/happy-flow-test-plan.md](../../docs/happy-flow-test-plan.md) (cenário **H2**)

## Histórico

| Data | Evento |
|---|---|
| 2026-07-26 | Item criado a partir do achado S1.7 do 1º teste in-game. Implementado como troca de uma constante: `LegsLimpN2` já existia no enum de linhas (produzido por `Z2+Q2` sob analgésico), já é reconhecido como tier Severa pelo gate de sprint e já mapeia na tabela de velocidade-alvo — nenhum valor de enum, texto de idioma ou mapeamento novo. |
