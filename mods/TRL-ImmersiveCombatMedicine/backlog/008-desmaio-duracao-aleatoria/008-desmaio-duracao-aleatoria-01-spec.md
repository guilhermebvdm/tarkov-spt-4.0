# 008 — Desmaio: duração aleatória min–max

**Mod:** TRL-ImmersiveCombatMedicine
**Status:** Backlog
**Criado:** 2026-07-19

## Visão geral

Substitui a duração FIXA do desmaio (um único valor configurável) por um sorteio uniforme entre um mínimo e um máximo configuráveis, feito uma única vez no ponto de entrada do desmaio (já marcado no código como `RANGE-READY`). Todo o resto do pipeline (relógio de wake, rampa visual, contusão, pacote de sincronização, espelhos Fika) já deriva do deadline absoluto gravado nesse ponto — nenhuma outra parte do código precisa mudar.

## Comportamento atual

Ao disparar o desmaio, o código grava um deadline absoluto (`BlackoutTimers[id] = Time.time + duration`) onde `duration` é lido diretamente de um único `ConfigEntry<float>` fixo ("Duração do Desmaio", 5-120s, default 20). Esse deadline é a ÚNICA fonte que o resto do pipeline consulta (wake no `MainLoopPatch`, rampa visual, contusão, sincronização Fika) — não há recálculo em nenhum outro ponto (lição já registrada: recalcular com a config ao vivo durante um desmaio em curso deslocava o wake e divergia entre os leitores).

## Comportamento desejado

No mesmo ponto onde a duração é lida hoje, sortear um valor uniforme entre um mínimo e um máximo configuráveis (em vez de ler o único valor fixo) e gravar esse valor sorteado no deadline, exatamente como já acontece hoje. Nenhum outro ponto do pipeline muda — o deadline gravado é opaco para todos os leitores subsequentes, sorteado ou não.

## Critérios de aceite

- [ ] Cada novo desmaio sorteia uma duração uniformemente entre o mínimo e o máximo configurados, de forma independente de sorteios anteriores (sem memória entre desmaios).
- [ ] Com `min == max`, o comportamento é idêntico ao fixo de hoje (sempre a mesma duração) — nenhum caso especial necessário, é só o caso degenerado do sorteio uniforme.
- [ ] Configurar `min > max` (config inválida) não trava nem lança exceção — produz um resultado definido e documentado (ex.: os dois valores são trocados antes do sorteio, ou o maior vira o piso).
- [ ] Mudar min/max no F12 **durante um desmaio já em curso** não afeta a duração desse desmaio (o deadline já foi gravado e ancorado) — só o PRÓXIMO desmaio sorteia com os novos valores. Mesma garantia de "relógio único" que a duração fixa já tem hoje.
- [ ] **Fika/multiplayer:** o sorteio acontece no processo DONO (humano local ou bot no host/headless) no mesmo ponto que já grava o deadline hoje — nenhum protocolo novo; o deadline sorteado é sincronizado aos peers pelo MESMO pacote que já sincroniza o deadline fixo atual.
- [ ] **Estado entre raids:** nenhum estado novo persiste — o sorteio acontece a cada disparo de desmaio, sem cache entre raids; os timers existentes já resetam na fronteira de raid.
- [ ] Amostra estatística: rodar N desmaios com min/max bem separados (ex.: 5s/60s) produz durações distribuídas ao longo de todo o intervalo (não um valor concentrado numa ponta) — verificável por log das durações sorteadas.

## Corner cases

- [ ] **`min` ou `max` fora da faixa aceitável** (ex.: negativo, zero) — a faixa do `ConfigEntry` (`AcceptableValueRange`) já impede isso no F12 nativo do BepInEx; os dois novos campos herdam o MESMO piso de 5s do campo fixo atual (lição de UX documentada — desmaio abaixo de ~5s colapsava com o grace num flap instantâneo) e o mesmo teto de 120s.
- [ ] **`min == max == 0` ou faixa muito estreita** — sorteio degenera para um valor quase-constante; não deve causar comportamento diferente de uma duração fixa baixa (já suportada hoje, piso 5s existente por lição de UX documentada — desmaio curto demais "flapava").
- [ ] **Troca da fonte de aleatoriedade** — usar o mesmo gerador (`UnityEngine.Random`) já padrão no resto do mod, não introduzir uma segunda fonte de RNG.

## Fora de escopo

- [ ] Qualquer mudança nos GATILHOS que decidem SE o jogador desmaia — isso é o item 007 (execução: 007 primeiro, depois 008, para não haver dois itens tocando o mesmo trecho de código em paralelo).
- [ ] Qualquer mudança no relógio de wake, rampa visual, contusão, ou sincronização — todos continuam lendo o deadline de forma opaca, como hoje.

## Referências

- [mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/HealthPatches.cs](../../modded/Patches/Trauma/HealthPatches.cs) — ponto `RANGE-READY` (comentário já existente no código, marcando exatamente onde este item deve atuar)
- [007-desmaio-percentual/](../007-desmaio-percentual/) — item anterior, entrega antes deste para não haver dois itens tocando o mesmo Postfix ao mesmo tempo

## Histórico

| Data | Evento |
|---|---|
| 2026-07-19 | Item criado via `/create-spec` (retomada do overhaul Trauma 2.0, P-3.7/P-3.4) |
| 2026-07-19 | Revisão `/review-spec` — piso de 5s explicitado como herdado (lição de UX do CR-04); critério de verificação estatística adicionado. Sem gaps adicionais — item pequeno e bem contido (ponto único de mudança). |
