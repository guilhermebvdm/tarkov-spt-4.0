# 014 — Roteiro de re-teste happy-flow

**Mod:** TRL-ImmersiveCombatMedicine
**Status:** Entregue
**Criado:** 2026-07-26

## Visão geral

O 1º teste in-game do overhaul Trauma 2.0 aconteceu em 2026-07-25/26 (Guilherme como client, Umbigo como host, Fika coop) e **parou no meio**. A causa não foi bug: foi o roteiro. O [master-test-plan.md](../../docs/master-test-plan.md) tem 44 cenários solo + 12 coop, muitos exigindo posicionamento controlado de tiro, séries estatísticas de 20 rolls, ou curar completamente o personagem entre fases. Isso não é executável numa sessão de jogo com duas pessoas.

Diretiva do usuário: *"impossível conseguir seguir todos cenários que você montou, inclusive precisamos simplificar para os casos mais importantes (foco no happy flow), apesar de ser importante cercar corner cases na spec e no código"*.

Item de **documentação pura** — nenhuma linha de código de produção.

## Comportamento atual

- Existe um único plano de teste, o mestre, que serve simultaneamente como (a) prova de cobertura formal dos critérios de aceite de 9 specs e (b) roteiro de execução em jogo. Ele é bom no primeiro papel e impraticável no segundo.
- Cada fase do mestre tem instruções de higiene entre fases ("curar a condição testada antes de avançar") que, somadas, tornam uma passada completa uma sessão de várias horas.
- Não há nenhum documento que responda "o que eu testo hoje, em 40 minutos, para saber se a entrega funcionou".

## Comportamento desejado

- Um roteiro curto, um cenário por comportamento, resultado observável em uma linha, executável numa sessão.
- O mestre **preservado e não revogado** — repositionado explicitamente como referência de corner cases, com o vínculo entre os dois documentos declarado nos dois lados.
- Rastreabilidade obrigatória: cada linha do roteiro curto aponta para o cenário do mestre (ou o achado do teste) que ela cobre. Sem isso a simplificação perde cobertura silenciosamente — que é exatamente a lição registrada na memória do mod ("consolidar documentos de teste sem comparar item-a-item com a fonte original perde cobertura silenciosamente"), e o motivo de a revisão adversarial de 2026-07-25 ter recuperado 3 cenários dropados.
- O roteiro precisa dizer **qual entrega habilita cada cenário**, porque as correções pós-teste saem em 3 levas e o mesmo documento serve aos 3 re-testes.

## Critérios de aceite

- [x] Existe um roteiro curto (~10 solo + 6 coop) com resultado observável por cenário, em `docs/happy-flow-test-plan.md`.
- [x] Cada item do backlog marcado 🟢 no overhaul (002–010) tem pelo menos um cenário correspondente no roteiro curto — verificável pela tabela de rastreabilidade.
- [x] Cada cenário do roteiro curto declara sua origem: o cenário do mestre que ele condensa, ou o achado do 1º teste que ele passou a cobrir.
- [x] O que ficou de fora está **listado explicitamente**, não omitido — corner cases nomeados, com a razão e a instrução de puxar do mestre quando houver suspeita.
- [x] O mestre declara, no topo, que deixou de ser roteiro de sessão, e aponta para o roteiro curto; o roteiro curto aponta de volta.
- [x] Cada cenário indica a leva de entrega que o habilita, para o documento servir aos 3 re-testes sem virar 3 documentos.
- [x] **Fika/multiplayer:** os cenários coop declaram os papéis (quem sofre, quem observa) e o que confirmar em cada PC — a falha do 1º teste na hitbox só apareceu porque o bug é por-observador.
- [x] **Estado entre raids:** há cenário de sair ferido e entrar de novo, com o critério que separa purga do mod (deve estar limpa) de persistência vanilla (deve estar presente).

## Corner cases

- [x] **Perda de cobertura na condensação** — mitigado pela tabela de rastreabilidade + lista explícita do que ficou fora. Foi o modo de falha real da consolidação anterior.
- [x] **Cenário não executável na leva atual** — a coluna `Leva` evita que o testador conclua "está quebrado" ao testar um comportamento que ainda não foi implantado.
- [x] **Diagnóstico que exige log, não observação** — três achados do 1º teste (atraso do agachar, calibração de velocidade, duração do desmaio) não são verificáveis a olho. Viraram um bloco próprio de leituras de log, com pré-requisito de guardar o `LogOutput.log` de cada máquina.
- [x] **Achado ambíguo do 1º teste** — o "morri e o umbigo me reviveu" admite duas leituras com vereditos opostos. Registrado como observação pendente com o observável que as distingue, em vez de ser resolvido por suposição.

## Fora de escopo

- [x] Revogar, arquivar ou remover cenários do plano mestre — ele segue vivo como referência de corner cases.
- [x] Cobrir o sistema de cura Band-Aid/torniquete clássico (pré-overhaul) — já validado em sessões anteriores, mesma exclusão que o mestre já fazia.
- [x] Automatizar qualquer teste — não há harness de teste in-game neste repo, e nada aqui é verificável por compilação.

## Referências

- [docs/happy-flow-test-plan.md](../../docs/happy-flow-test-plan.md) (entregável)
- [docs/master-test-plan.md](../../docs/master-test-plan.md) (fonte da derivação; repositionado)
- [docs/trauma-behavior-matrix.md](../../docs/trauma-behavior-matrix.md) (fonte dos critérios de aceite)

## Histórico

| Data | Evento |
|---|---|
| 2026-07-26 | Item criado após o 1º teste in-game ter parado no meio por excesso de cenários. Entregue como `docs/happy-flow-test-plan.md`, derivado item a item do mestre (não de memória), com tabela de rastreabilidade, coluna de leva de entrega, bloco de leituras de log e registro da ambiguidade pendente do "revive". Mestre repositionado como referência de corner cases nos dois sentidos. |
