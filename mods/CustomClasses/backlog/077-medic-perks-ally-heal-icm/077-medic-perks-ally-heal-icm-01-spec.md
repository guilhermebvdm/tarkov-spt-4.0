# 077 — Médico: perks de tempo/movimento valem na cura de aliado do ICM

**Mod:** CustomClasses
**Status:** Backlog
**Criado:** 2026-07-19

## Visão geral

Os três perks de assinatura do **Médico de Combate** que afetam **tempo** e **movimento** durante procedimentos médicos — **Cirurgia Ágil** (cirurgia mais rápida), **Cuidado Rápido** (curativos/estabilizações mais rápidos) e **Cirurgia em Movimento** (andar durante a própria cirurgia) — hoje só valem quando o Médico trata a **si mesmo**. Quando o tratamento é entre **aliados** (mecânica fornecida pelo mod irmão TRL-ImmersiveCombatMedicine, "ICM"), esses perks se comportam errado: o benefício de **movimento vaza para qualquer operador** (todo mundo pode andar durante a cirurgia de um aliado, Médico ou não), e o benefício de **velocidade não vale para ninguém** (nem o próprio Médico acelera). Este item estende os três perks para a cura de aliado, sempre gateando pela classe de **quem opera** — não do paciente.

## Comportamento atual

**Auto-tratamento (Médico tratando a si mesmo):** os três perks funcionam — cirurgia e curativos mais rápidos, e o Médico pode andar durante a própria cirurgia (os demais operadores ficam imobilizados, como no vanilla).

**Tratamento de aliado (via ICM):**
- **Movimento:** o operador **nunca** é imobilizado durante a cirurgia de um aliado — então **qualquer** operador (Médico ou não) pode andar. O "andar durante a cirurgia" deixa de ser exclusivo do Médico.
- **Velocidade:** a duração do procedimento de aliado é **fixa** e igual para todos. Os perks de tempo do Médico são ignorados no tratamento de aliado — nem ele acelera.

**Observado in-game (raid coop, 2026-07-19):** um operador **sem** a classe Médico operou um paciente-Médico, pôde **andar** e pareceu **curar rápido**, dando a falsa impressão de ter "herdado" os perks do Médico. (A penalidade de HP máximo — item 076 — já se comportou corretamente na mesma raid; este item é sobre os **outros** perks do Médico.)

## Comportamento desejado

- **Movimento na cirurgia de aliado:** por padrão o operador fica **imobilizado** durante a cirurgia de um aliado — **não anda**, igual à cirurgia em si mesmo no vanilla. **Somente** o operador que é Médico de Combate com **Cirurgia em Movimento** ativa pode **andar** durante a cirurgia de aliado, mantendo a restrição de **não correr/pular** (idêntico ao que o perk já faz na auto-cirurgia).
- **Velocidade na cura/cirurgia de aliado:** quando o operador é Médico de Combate com os perks de tempo ativos, o procedimento de aliado fica mais rápido na **mesma proporção** do auto-tratamento (Cirurgia Ágil na cirurgia; Cuidado Rápido nos demais itens). Para operadores não-Médicos, a duração permanece a padrão.
- Em ambos os eixos, o gate é a classe de **quem opera**.
- Se o mod de classes (CustomClasses) estiver **ausente**, o mod de cura de aliado mantém o comportamento **seguro**: ninguém se move durante a cirurgia de aliado e a duração é a padrão.

## Critérios de aceite

- [ ] Médico com **Cirurgia em Movimento** ativa consegue **andar** (sem correr/pular) enquanto opera a cirurgia de um aliado; um operador **não-Médico não consegue andar** (fica imobilizado). *(Verificável apertando andar durante a cirurgia de aliado, com cada perfil.)*
- [ ] Médico com **Cirurgia Ágil** ativa **completa a cirurgia de um aliado em tempo menor** que um operador não-Médico usando o mesmo item de cirurgia. *(Verificável cronometrando/comparando.)*
- [ ] Médico com **Cuidado Rápido** ativa **completa curativos/estabilizações de aliado em tempo menor** que um operador não-Médico com o mesmo item.
- [ ] Operador **não-Médico** opera cura/cirurgia de aliado **no tempo padrão e imobilizado** — nenhum dos três perks altera o comportamento dele.
- [ ] A **animação** do procedimento termina em sincronia com o fim real da operação (a mão não fica presa no gesto após o efeito acabar, nem o gesto corta antes), **inclusive** com os perks de tempo ativos.
- [ ] **Fika/multiplayer:** o comportamento correto vale nos 3 cenários coop — Médico **host** operando um **client**, Médico **client** operando o **host**, e Médico **client** operando **outro client** — e o movimento/velocidade do operador são **visíveis** para os demais players (replicados).
- [ ] **Estado entre raids:** a imobilização (quando aplicada) é **sempre liberada** ao fim de cada procedimento e ao sair da raid; o operador **nunca** termina a raid ou entra na próxima preso/sem poder andar — mesmo se o procedimento for cancelado, o item dropado, ou o médico/paciente morrer no meio.

## Corner cases

- [ ] **Interrupção com imobilização ativa** — o operador solta/dropa o item, cancela, se afasta do paciente, ou o médico/paciente morre **enquanto imobilizado**: a imobilização é liberada em **todos** os caminhos de encerramento (o operador não pode ficar preso).
- [ ] **Curativo (não cirurgia) em aliado** — o operador **não** deve ser imobilizado (imobilização é exclusiva da cirurgia; curativo nunca prende as pernas), mas **Cuidado Rápido** ainda acelera se ele for Médico.
- [ ] **Toggle do F12 no meio da raid** — trocar o estado de um perk entre procedimentos vale no **próximo** procedimento; um procedimento já em andamento não muda no meio.
- [ ] **Auto-cirurgia vs. cura de aliado em sequência** — o estado de imobilização/velocidade de um procedimento não pode vazar para o outro (o Médico opera a si e depois um aliado, ou vice-versa, em curtos intervalos).
- [ ] **Um dos mods ausente** — ICM presente e CustomClasses ausente (fail-safe: ninguém anda, tempo padrão); CustomClasses presente e ICM ausente (não há cura de aliado — só auto-tratamento, já coberto pelo 072).
- [ ] **Médico operando um bot** — o operador (Médico local) legitimamente ganha os perks (anda/rápido); o bot é só o paciente e não recebe nada. Sem vazamento (diferente do 076, o efeito aqui é no operador, não no paciente).
- [ ] **Aliado remoto vs. aliado local** — o comportamento (velocidade + imobilização) vale igual quando o paciente é um **player humano remoto** e quando é um **bot local do host** (os dois passam por sub-caminhos diferentes do procedimento de aliado; ambos devem respeitar o gate).
- [ ] **Troca de classe entre raids** — o operador troca a classe do perfil (editor web) entre uma raid e outra: o próximo procedimento reflete a classe nova; o gate lê a classe vigente no início do procedimento, não uma cacheada de raid anterior.
- [ ] **Sincronia da duração acelerada em coop** — quando o Médico acelera o procedimento, o momento em que ele termina não pode divergir entre o processo do operador e o dos peers que o observam (o gesto não pode "acabar" em tempos diferentes em telas diferentes).

## Fora de escopo

- [ ] O comportamento da **auto-cirurgia/auto-cura** (já coberto pelo item 072) — não muda.
- [ ] A **penalidade de HP máximo** na cura de aliado (item 076) — já entregue e validada; item separado.
- [ ] Rebalancear os **valores** dos perks (múltiplos de tempo, etc.) — herda os valores do 072/F12.

## Referências

- Item **072** (perks de tempo/movimento do Médico — a base que este item estende).
- Item **076** (review #2 + validação in-game de 2026-07-19 que revelou este gap; a arquitetura cross-mod "opção B" é espelhada aqui).
- Memória `reference_icm_ally_heal_operator_not_immobilized` (diagnóstico do sintoma).
- Mod **TRL-ImmersiveCombatMedicine** (dono da mecânica de cura de aliado).
- ⚠️ **Pré-condição:** os perks 072 ainda aguardam validação in-game (pendência **P-16.1** do mod) — este item assume que o 072 funciona na auto-cirurgia.

## Histórico

| Data | Evento |
|---|---|
| 2026-07-19 | Item criado (decisões de produto do usuário: imobilizar não-Médico + acelerar Médico na cura de aliado) |
| 2026-07-19 | Revisão `/review-spec` — semântica de movimento refinada (andar sem correr/pular vs. imobilizar) + 3 corner cases (aliado remoto vs. bot local · troca de classe entre raids · sincronia da duração em coop) |
