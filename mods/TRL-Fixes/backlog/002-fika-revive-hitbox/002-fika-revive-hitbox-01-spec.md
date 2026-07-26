---
title: Fika Revive Hitbox Loss Fix
date: 2026-07-26
status: 🟢 Vivo
authors: [Claude, Guilherme]
---

# 002 — Fika: hitbox perdida após revive

## Descrição do Problema

Achado do 1º teste in-game do TRL-ImmersiveCombatMedicine (2026-07-26, sessão Fika de 2 PCs): *"Toda vez que alguém volta do desmaio ou coma, ele fica sem hitbox, a gente consegue ver o player, mas ao atirar o tiro nunca pega, como se ele não tivesse hitbox"*.

O jogador revivido continua renderizando normalmente e continua jogando normalmente, mas **balas de outros jogadores o atravessam**. O estado dura até o fim da raid.

A causa é no Fika, não no mod que reportou. Quando um jogador é derrubado, o Fika move a hierarquia inteira do modelo para a layer de cadáver, cria o ragdoll e desativa as placas de armadura. No revive, ele devolve a hierarquia para a layer de jogador — mas as hitboxes balísticas do EFT **não vivem na layer de jogador**: elas vivem numa layer própria, e é só essa layer que a máscara de traçado de projétil enxerga. Devolver tudo para "jogador" apaga essa distinção, e as hitboxes deixam de ser encontradas por qualquer bala.

O próprio EFT nunca separa essas duas operações: na inicialização do jogador, a promoção das hitboxes para a layer correta acontece **na linha imediatamente seguinte** à atribuição de layer da hierarquia. O Fika faz a primeira e omite a segunda — é uma omissão do par, não uma decisão de design.

Dois detalhes que ampliam o alcance:

- **É por observador.** Cada cliente aplica isso ao modelo que ele observa, no seu próprio processo. Quem foi revivido não percebe nada de errado em si mesmo; quem olha para ele é que não consegue acertá-lo. Todos os observadores passam pelo mesmo caminho — quem executou o revive por um ponto do código, os demais peers por outro.
- **Vale para qualquer coisa que use o estado "derrubado" do Fika**, não só para o coma. Um mod que reaproveite esse estado (como o desmaio do TRL-ImmersiveCombatMedicine faz hoje) herda o mesmo defeito.

Há um segundo efeito, menor e independente: as **placas de armadura** desativadas na criação do ragdoll também não são reativadas no revive, então elas param de registrar impacto e de ricochetear até que alguma troca de equipamento aconteça.

## Critérios de Aceite

* Um jogador revivido volta a ser atingível por bala e por faca, para **todos** os outros participantes da raid, imediatamente após o revive e sem depender de nenhuma ação dele (trocar de arma, se curar, lootar).
* O comportamento é verificado nos **dois sentidos**: quem revive atirando em quem foi revivido, e um terceiro observador atirando também — o defeito é por observador, então um único sentido não prova a correção.
* As placas de armadura do jogador revivido voltam a registrar impacto sem depender de uma troca de equipamento.
* Enquanto o jogador está **derrubado** (antes do revive), ele continua atingível como hoje — o comportamento durante o downed é correto e não deve mudar.
* O mod não faz nada se o Fika não estiver instalado: ausência do alvo é registrada no log como informação, não como erro.
* Em servidor headless, o mod não interfere — o Fika já não monta nem desmonta o ragdoll nesse caso.
* Nenhuma exceção pode escapar para o fluxo de revive do Fika: uma falha aqui não pode cancelar um revive.

## Verificação em jogo

Cenário **C2** do [roteiro happy-flow do ICM](../../../TRL-ImmersiveCombatMedicine/docs/happy-flow-test-plan.md).

Antes de aplicar o fix, vale registrar a linha de base, porque o relato do teste tem um ponto que o código não explica: **os bots aparentemente acertavam o jogador revivido, enquanto outro jogador não conseguia**. Os bots rodam no host, que é também um observador e portanto está sujeito ao mesmo defeito. Uma hipótese inicial — de que uma troca de equipamento restauraria a hitbox — foi **refutada na leitura do código**: o caminho de recálculo de equipamento reativa as placas de armadura, mas não repromove as hitboxes balísticas.

Então observar e anotar separadamente, no mesmo alvo revivido e na mesma janela de tempo:

1. tiro de outro **jogador** → aplica dano?
2. tiro de **bot** → aplica dano?

Se (2) aplicar e (1) não, existe um mecanismo ainda não identificado e a spec técnica precisa ser revisitada antes de considerar o item fechado — mesmo que o fix resolva (1).

## Fora de Escopo

* Os rigidbodies do ragdoll nunca voltam ao estado não-simulado e os joints nunca são removidos (o ragdoll é criado sem a opção de adormecer). É desperdício de física e um vazamento por ciclo de derrubada/revive, mas não afeta hit registration — avaliar como item próprio.
* Qualquer mudança no tempo, no custo ou nas regras do revive do Fika.
* O estado de desmaio do TRL-ImmersiveCombatMedicine deixar de reaproveitar o downed do Fika — é o item 015 daquele mod, e é complementar: resolve o caso do desmaio, enquanto este item resolve o caso do coma de verdade.

## Histórico

| Data | Evento |
|---|---|
| 2026-07-26 | Item criado a partir do achado do 1º teste in-game do TRL-ImmersiveCombatMedicine. Alocado neste mod (e não no ICM) porque o defeito reproduz em qualquer servidor Fika com revive habilitado, independentemente do ICM estar instalado. |
