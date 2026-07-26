# Roteiro de Re-teste — Happy Flow

> **Data:** 2026-07-26<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [master-test-plan.md](./master-test-plan.md), [trauma-behavior-matrix.md](./trauma-behavior-matrix.md)<br>

---

## Escopo

Roteiro **curto** de validação in-game, derivado item a item do [master-test-plan.md](./master-test-plan.md). Um cenário por comportamento, resultado observável em uma linha.

O plano mestre (44 solo + 12 coop) continua sendo a fonte de **corner cases** e a referência de cobertura formal — não foi substituído nem revogado. Ele é impraticável como roteiro de sessão, e foi essa impraticabilidade que fez o 1º teste (2026-07-25/26) parar no meio. Este documento existe para ser executável numa sessão de jogo.

**Nada aqui inventa cenário novo:** cada linha rastreia para um teste do mestre ou para um achado do 1º teste in-game. A tabela de rastreabilidade no fim é a prova.

**Coluna `Leva`:** qual entrega precisa estar implantada para o cenário fazer sentido. `—` = já testável na v1.10.0. Ver [plano de correções](../backlog/mod-backlog.md) itens 013–021.

---

## Bloco 0 — Pré-requisitos

- [ ] **P1** — F12 → BepInEx → a versão de `TRLImmersiveCombatMedicine` bate com a que você acabou de implantar. Se não bater, é build velha no jogo: pare aqui.
- [ ] **P2** — F12 → "6. Trauma 2.0 (Consumidores)": os 5 toggles (Legs / Fall / Arms / Stomach / Blackout 2.0) **Ligados**.
- [ ] **P3** — Em coop: **todos** os PCs na mesma build, e o `TRL-Fixes` implantado nos dois (a partir da Leva 1).
- [ ] **P4** — Guardar o `LogOutput.log` de **cada máquina** ao fim da sessão. O Bloco C depende dele, e sem ele três diagnósticos viram chute.

---

## Bloco A — SOLO

| | Cenário | Resultado esperado | Leva |
|---|---|---|---|
| **H1** | Zerar 1 perna; depois curar | Manca; ao curar volta ao normal em ≤1s | — |
| **H2** | Zerar 2 pernas; depois tomar analgésico | Agacha 1× **na hora**, manca forte e **não corre** — e continua sem correr com o analgésico ativo | 3 |
| **H3** | Quebrar 2 pernas sem analgésico | Cai; levanta e após ~3s de pé cai sozinho; tentar levantar durante o bloqueio dá grito de dor e é negado; ao liberar, levanta devagar | — |
| **H4** | Zerar 2 braços e segurar a mira | Tremor visível; a mira cai sozinha em ~4s; re-mirar em seguida é bloqueado, com grito | — |
| **H5** | Zerar o estômago 4× (curando entre cada) | Agachou na maioria das vezes; o log mostra um roll por zerada | — |
| **H6** | Tomar um tiro forte no tórax sem analgésico | Desmaia; a duração bate com a config; acorda e recupera o controle sem prone fantasma | — |
| **H7** | Já desmaiado, tomar dano até zerar o HP | O dano **aplica**; ao zerar, entra em **coma** — não morre | 2 |
| **H8** | Sair da raid ferido e entrar na próxima | Log mostra a purga com contagem; nenhum efeito **do mod** ativo no spawn. Ferimento vanilla presente é **esperado** | 3 |
| **H9** | Aproximar de um bot ferido a ~3,5 m; trocar o idioma do jogo | Prompt médico aparece a 3,5 m e não a 5-6 m; os textos trocam de idioma sem reiniciar o jogo | — |
| **H10** | Zerar a 2ª perna **enquanto corre** | Agacha imediatamente **ou** o log registra o cancelamento — nunca uma agachada fantasma segundos depois | 3 |

---

## Bloco B — COOP (2 PCs)

`A` = quem sofre. `B` = quem observa/age.

| | Cenário | Resultado esperado | Leva |
|---|---|---|---|
| **C1** | B revive A **em coma** usando desfibrilador | O desfibrilador sai do inventário de B, sem piscar, e o espaço é liberado — confirmar nos **dois** PCs | 1 |
| **C2** | **Bissecção da hitbox** — A em coma, B revive A, e B atira em A: (a) **imediatamente**, sem A tocar em nada; (b) depois de A trocar de arma | Registrar (a) e (b) **separadamente**: dano aplica ou atravessa? Repetir invertendo os papéis. Confirmar também se os bots acertam A | 1 |
| **C3** | B se aproxima de A **desmaiado**; depois de A **em coma** | Desmaiado → ação **"Acordar"**, sem item, e A acorda. Coma → ação **"Reviver"**, exige desfibrilador. Nunca as duas ao mesmo tempo | 2 |
| **C4** | A manca, cai e agacha; A grita de dor | B vê a manqueira, a queda e o agachar, e ouve o grito. B **não** vê o tremor de A — isso é o comportamento correto, não é bug | — |
| **C5** | A desmaia perto de bots, e B atira em A | Os bots perdem o alvo de A; o tiro de B **aplica dano** | 2 |
| **C6** | A com o jogo em PT aplica torniquete; B com o jogo em EN observa | B vê a notificação **em inglês** — a tradução acontece em quem exibe, não em quem originou | — |

---

## Bloco C — Leituras de log (fazer depois da sessão)

Não são cenários de jogo. São três diagnósticos que só o log responde.

- [ ] **L1** — Procurar `legs cap RECOMPUTE`. O campo `clamped=` diz se a penalidade vanilla está engolindo o cap do mod. **Esperado:** `true` sem analgésico, `false` com analgésico. Se vier `true` nos dois, os defaults de calibração N1/N2 precisam ser repensados.
- [ ] **L2** — Procurar `crouch DEFERRED`. O motivo entre parênteses (`airborne`, `internal-guard`, `ladder`, `btr`) identifica qual guard atrasou o agachar do H10. **É a evidência que o item 018 precisa para calibrar o TTL** em vez de chutar.
- [ ] **L3** — Procurar `[Blackout]` → confere a duração sorteada contra a config.
- [ ] **L4** — Nenhuma linha contém `[DEBUG-ICM]`.

---

## Observação pendente do 1º teste

Uma pergunta do teste anterior ficou sem resposta e muda o veredito de um achado. No momento em que **"o Umbigo te reviveu"**:

- havia prompt de revive disponível enquanto você estava **consciente-mas-imóvel** (desmaio do mod sendo revivido → é o bug CR-01-21, e é o mesmo evento que gastou o desfibrilador à toa); **ou**
- você estava **efetivamente morto**, com tela de morte (coma do Fika → comportamento correto, nada a corrigir)?

Se der para reproduzir na próxima sessão, anotar qual dos dois. A partir da Leva 2 a pergunta perde sentido: o desmaio deixa de oferecer revive.

---

## Rastreabilidade

Cada cenário deste roteiro e o que ele cobre no backlog. Serve como meta-verificação: todo item 🟢 do overhaul tem pelo menos uma linha.

| Cenário | Cobre | Origem no plano mestre |
|---|---|---|
| H1 | 002 (motor), 003 (mancar N1) | S1.1, S1.3 |
| H2 | 003 (N2 + agachar), **017** | S1.2, S1.7 |
| H3 | 004 (ciclo de queda completo) | S2.1, S2.2, S2.3, S2.4 |
| H4 | 005 (tremor + cancela-ADS + lockout) | S3.2, S3.4 |
| H5 | 006 (roll do estômago) | S4.1, S4.3 |
| H6 | 007 (gatilho), 008 (duração) | S5.1, S6.1 |
| H7 | **015** (vulnerabilidade + coma) | achado #3 do 1º teste |
| H8 | **020** (purga), 002 (reset entre raids) | S1b.1, S8.2 |
| H9 | 010 (distância 3,5 m + i18n) | S7.1, S7.2 |
| H10 | **018** (TTL do adiamento) | S1.9, achado #8 do 1º teste |
| C1 | **013** (consumo do desfibrilador) | achado #1 do 1º teste |
| C2 | **TRL-Fixes 002** (hitbox pós-revive) | achado #2 do 1º teste |
| C3 | **016** (Acordar x Reviver) | achado #4 do 1º teste, CR-01-21 |
| C4 | 009 (visibilidade cross-peer) | C1.1, C1.2, C1.3, C1.4, C1.6 |
| C5 | **015** (IA + vulnerabilidade em coop) | C1.5, achado #3 do 1º teste |
| C6 | 010 (i18n cross-peer) | C2.1 |
| L1 | 003 (calibração) — fronteira mod↔vanilla | novo (achado transversal) |
| L2 | 018 (diagnóstico do adiamento) | novo (achado #8) |

**Fora deste roteiro, por decisão:** os corners do plano mestre (fila de adiados multi-região, migração de config fracionária em pt-BR, bot-boss fora do ciclo, toggles OFF mid-raid, reconexão Fika, versões divergentes entre peers, verificação estatística de 20 rolls). Continuam válidos e cercados na spec e no código — só não entram no roteiro de sessão. Quando um deles for suspeito de estar quebrado, puxar o cenário correspondente do mestre.

---

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-26 | Guilherme | Criação (item 014). Roteiro curto derivado item a item do `master-test-plan.md` após o 1º teste in-game ter parado no meio por excesso de cenários. Acrescenta a bissecção da hitbox pós-revive, as três leituras de log obrigatórias e a ambiguidade pendente do "revive" do 1º teste. |
