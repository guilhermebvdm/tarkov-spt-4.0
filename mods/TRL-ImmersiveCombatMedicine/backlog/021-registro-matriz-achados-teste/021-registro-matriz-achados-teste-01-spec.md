# 021 — Registro na matriz: achados do 1º teste in-game

**Mod:** TRL-ImmersiveCombatMedicine
**Status:** Entregue
**Criado:** 2026-07-26

## Visão geral

Item de **documentação pura**. O 1º teste in-game (2026-07-26) produziu três classes de informação que não são bug e não geram código, mas que se perdem se não forem registradas na fonte da verdade dos critérios de aceite:

1. **Um limite técnico descoberto** — a fronteira entre o que o mod controla e o que o vanilla controla nas pernas, que muda a leitura da matriz inteira.
2. **Dois comportamentos corretos reportados como suspeitos** — se não ficarem registrados, voltam a consumir sessão de diagnóstico.
3. **Um veredito por escrito** sobre o que trazer do mod ancestral, pedido explicitamente pelo usuário (*"Tem mais pontos que podemos trazer de lá?"*).

A regra do item 011 vale aqui: *toda decisão nova ou premissa adotada DEVE ser incorporada à matriz — nada fica só em spec técnica ou commit*.

## Comportamento desejado

Três blocos novos na [matriz de comportamento](../../docs/trauma-behavior-matrix.md):

**§1.2 — A fronteira mod ↔ vanilla nas pernas.** O vanilla concentra toda a penalidade de perna ferida dentro da condição "não está sob analgésico" e, fora dela, não aplica nada. Consequências: (a) correr com uma perna comprometida é bloqueio do vanilla, e liberar isso exigiria o mod acelerar acima do vanilla — recusado; (b) sem analgésico, os alvos de velocidade do mod são engolidos pela penalidade vanilla, mais severa, porque a composição é pelo mínimo; (c) com analgésico, o mod é a única penalidade existente. Fecha com a leitura de log que confirma qual dos dois governa em cada coluna.

**§5.5 — Esclarecimentos.** Os dois tremores que coexistem por design (o do mod no braço ferido, o nativo sempre na cabeça, derivado do efeito de Dor) e por que o analgésico não faz o da cabeça desaparecer. E que ferimento no spawn é persistência vanilla, não do mod.

**§5.6 — Veredito dos 15 candidatos do TrueTrauma 3.11**, cada um com portar / avaliar / não portar e a razão, mais a lista de anti-candidatos que não devem voltar e um vestígio ainda ativo que é bug a remover.

## Critérios de aceite

- [x] A §2 (matriz de efeitos) reflete a mudança do item 017, marcada como alterada e não como "conforme original".
- [x] A §4.1 (critérios de aceite de pernas) diz explicitamente o que muda com analgésico e o que continua sendo bloqueio vanilla.
- [x] O limite do S1.1 está registrado como **decisão do usuário**, com a razão técnica, e não como pendência aberta — para não ser reaberto como bug.
- [x] Cada um dos 15 candidatos do mod ancestral tem veredito e razão; nenhum fica sem decisão.
- [x] Os anti-candidatos (removidos por decisão consciente) estão nomeados, para não voltarem por engano numa próxima leitura do fonte antigo.
- [x] A pendência de validação in-game deixa de dizer "nada foi testado" e passa a refletir o que o 1º teste cobriu e onde os achados foram parar.
- [x] **Fika/multiplayer** e **Estado entre raids:** N/A — é documentação.

## Fora de escopo

- [x] Implementar qualquer um dos candidatos — cada um está endereçado ao seu item (015, 019) ou marcado como "avaliar".
- [x] Revogar cenários da §4 — segue como referência de corner cases (item 014).
- [x] Retrofitar o `trauma-matrix.md` original, que continua sem a correção do D12 — dívida já registrada na §5.4 e anterior a este item.

## Referências

- [docs/trauma-behavior-matrix.md](../../docs/trauma-behavior-matrix.md) (entregável: §1.2, §2, §4.1, §5.4, §5.5, §5.6)
- [mods/TrueTrauma - FINALIZADO/](../../../TrueTrauma%20-%20FINALIZADO/) (fonte auditada)

## Histórico

| Data | Evento |
|---|---|
| 2026-07-26 | Item criado e entregue. A §1.2 nasceu de uma verificação no Assembly feita para responder ao pedido S1.1 ("liberar correr com a perna doendo") e acabou revelando que a coluna "sem analgésico" da matriz de pernas é, na prática, vanilla — achado maior que o pedido original. |
