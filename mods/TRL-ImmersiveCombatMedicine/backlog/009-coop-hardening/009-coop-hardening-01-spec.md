# 009 — Coop/bots: hardening do Trauma 2.0

**Mod:** TRL-ImmersiveCombatMedicine
**Status:** Backlog
**Criado:** 2026-07-20

## Visão geral

Passe transversal de fechamento do overhaul Trauma 2.0 (itens 002-008, todos entregues): consolida as auditorias de compatibilidade já feitas item a item numa suíte única e citável, formaliza uma lacuna documental real (mod `tarkin-ladders` faltando no D20 escrito), resolve 2 débitos técnicos já registrados durante a implementação (reconciliação de voz 004×005, helper compartilhado do boilerplate de `Update()`), e entrega um protocolo de teste estruturado para a validação manual (solo com bots + 2 PCs coop) que nenhum item do overhaul teve até agora.

Este item tem dois blocos de entrega com natureza diferente, e os critérios de aceite refletem isso:

- **Bloco A — entregável nesta sessão** (código + documentação, verificável estaticamente): itens A1-A4 abaixo.
- **Bloco B — protocolo de teste manual** (o item entrega o roteiro; a EXECUÇÃO é do usuário, fora do escopo desta sessão): itens B1-B2 abaixo.

**Critério de "entregue" (🟢) deste item:** mesmo padrão já usado em 002-008 — 🟢 significa Bloco A implementado/revisado e Bloco B entregue como ROTEIRO PRONTO (não que a execução aconteceu). A EXECUÇÃO real de B1/B2 continua sendo uma pendência viva do mod (mesma natureza de P-4.4 — "VALIDAR IN-GAME"), não uma condição de bloqueio para marcar o item como entregue. Isso evita duplo padrão em relação aos itens anteriores, todos 🟢 sem validação in-game própria.

## Comportamento atual

- As auditorias de compatibilidade com mods de terceiros (SAIN, ORBIT, CustomClasses-Tank, SPTRecoilRework, Fontaine-FOVFix, BringBackConcussion, VisceralCombat) existem, mas estão **espalhadas** em 5+ specs técnicas e reviews diferentes (003, 004, 005, 007) — não há um documento único que alguém possa consultar para saber "este mod está OK, com qual evidência".
- O mod `tarkin-ladders` (escadas interativas reais, distinto do vanilla que não tem esse tipo) já tem um guard funcional implementado desde o item 004 (D7 adia agachar/cair em escada), mas nunca entrou na lista formal de compatibilidade (`docs/trauma-matrix.md` D20, nem no resumo do item 009 em `mod-backlog.md`).
- Dois emissores de voz de dor (item 004: queda/negação de levantar; item 005: lockout de re-ADS) usam o mesmo canal do jogador (`Speaker`) com a mesma prioridade máxima — cada um tem sua própria garantia de não-spam interna, mas não há arbitragem entre os dois quando competem pelo mesmo instante. Registrado como pendência de reconciliação desde a entrega do 005.
- O `Update()` de cada um dos 4 consumidores de estado contínuo (pernas-mancar 003, ciclo-de-queda 004, braços 005, estômago 006) repete quase idêntico o mesmo esqueleto (detectar mundo nulo/troca de raid, detectar toggle ligado→desligado/desligado→ligado) sem um helper compartilhado — identificado e deliberadamente deferido em pelo menos 2 code-reviews (006 CR-01-02, 008 CR-01-01).
- Nenhum item do overhaul (002-011) foi validado rodando o jogo de fato — nem sozinho com bots, nem em coop 2 PCs.

## Comportamento desejado

### Bloco A — entregável nesta sessão

**A1. Formalizar o D20 (suíte de compatibilidade) incluindo `tarkin-ladders`.** `docs/trauma-matrix.md` (decisão D20) e o resumo do item 009 em `mod-backlog.md` passam a listar `tarkin-ladders` explicitamente, com referência ao guard D7 já implementado (item 004) — sem mudança de comportamento, só fechamento de lacuna documental.

**A2. Consolidar a suíte de compat num documento único.** Um novo documento (`docs/trauma-compat-suite.md`, no espírito de `docs/coop-heal-matrix.md`) lista, para cada um dos 8 mods do D20 (SAIN, ORBIT, CustomClasses-Tank, SPTRecoilRework, Fontaine-FOVFix, BringBackConcussion, VisceralCombat, tarkin-ladders): o veredito (sem conflito / conflito mitigado / não aplicável), o mecanismo de convivência (ex.: "postfix-only no mesmo alvo, idempotente por construção Harmony"), e a referência ao artefato original (spec técnica + linha) onde a prova foi feita. Nenhuma prova é refeita — é consolidação, não nova auditoria.

**A3. Reconciliar a voz dupla-fonte 004×005 (ou documentar a decisão de não mudar nada).** Investigar se a colisão entre os dois emissores de voz (mesmo Speaker, mesma prioridade máxima) tem um cenário prático prejudicial (ex.: uma dor real fica sistematicamente engolida) ou se é um corner raro e sem sintoma. Se houver cenário prejudicial real, implementar uma arbitragem mínima entre os dois. Se não houver, documentar a decisão de aceitar a colisão como está (sem adicionar acoplamento entre os dois consumidores para resolver um problema hipotético).

**A4. Extrair o boilerplate de `Update()` comum aos 4 consumidores para um helper compartilhado.** O esqueleto de "detectar `GameWorld` nulo/troca de raid" e "detectar toggle ligado↔desligado" some da duplicação em `TraumaLegsConsumer` (003), `TraumaFallCycleConsumer` (004), `TraumaArmsConsumer` (005) e `TraumaStomachConsumer` (006), substituído por um helper único — **sem NENHUMA mudança de comportamento observável** nos 4 itens já entregues (é puramente uma limpeza de manutenibilidade; cada consumidor mantém seu próprio teardown/callback específico).

### Bloco B — protocolo de teste manual (entregue como roteiro, execução é do usuário)

**B1. Smoke test SAIN/ORBIT do re-derrubar de bot.** Roteiro executável em raid SOLO (host com bots, sem precisar de 2º PC): bot com as 2 pernas quebradas cai, SAIN/ORBIT tentam levantá-lo depois de X segundos, a condição persiste, o bot é re-derrubado — confirma que a camada BigBrain do mod e as camadas de SAIN/ORBIT convivem sem travar o bot num estado inconsistente.

**B2. Protocolo de teste 2 PCs.** Reaproveitando o esqueleto de `docs/coop-heal-matrix.md` (protocolo de teste in-game já usado antes do Trauma 2.0), um roteiro cobrindo os cenários que só são observáveis com um segundo humano: mancar/tremor-exclusão/queda visíveis ao peer via sync nativo; vozes de dor audíveis ao peer; bots do host aparecendo coerentes ao client. Este roteiro referencia diretamente os cenários 40-44 (transversais) e os cenários específicos por item do plano de teste em `docs/trauma-behavior-matrix.md §5`.

## Critérios de aceite

- [ ] **A1:** `tarkin-ladders` aparece explicitamente na decisão D20 de `docs/trauma-matrix.md` e no resumo do item 009 em `mod-backlog.md`, com referência ao guard D7 já implementado.
- [ ] **A2:** `docs/trauma-compat-suite.md` existe, cobre os 8 mods do D20, cada um com veredito + mecanismo + referência ao artefato original — nenhuma reafirmação sem evidência citável.
- [ ] **A3:** decisão sobre a voz dupla-fonte tomada com evidência (investigação real do cenário, não suposição) e documentada — seja como "implementado" (com o mecanismo descrito) seja como "aceito sem mudança" (com a razão).
- [ ] **A4:** os 4 consumidores compilam e usam o helper compartilhado; nenhuma mudança de comportamento nos testes/smoke já documentados de 003-006 (verificação estática linha a linha do que muda vs. do que permanece).
- [ ] **B1/B2:** os dois roteiros existem como documentos claros, prontos para execução — não é aceitável fechar o item alegando "coop validado" sem a execução real (ver corner cases).
- [ ] **Fika/multiplayer:** é o assunto central deste item — coberto pelos blocos A2 (compat estática) e B2 (protocolo real).
- [ ] **Estado entre raids:** N/A para A1/A2/A3 (documentação/decisão); para A4, o helper extraído preserva o reset entre raids que cada consumidor já tinha (nenhuma mudança).

## Corner cases

- [ ] **A4 é uma refatoração de código já entregue e testado (003-006) — risco de regressão silenciosa.** Cada consumidor precisa manter seu comportamento EXATO após a extração (ex.: a ordem em que `TearDownLocal`/callbacks específicos rodam em relação à detecção de troca de mundo não pode mudar). Corner a testar: toggle OFF mid-raid em cada um dos 4 itens continua desfazendo o efeito correspondente sem resíduo.
- [ ] **A3, se optar por implementar arbitragem:** não pode enfraquecer a garantia "1 voz por janela" que cada consumidor já tem individualmente — a arbitragem entre os dois é uma camada ACIMA dessa garantia, não uma substituição dela.
- [ ] **B1 pode ser executado nesta sessão** (não exige 2º PC) — mas ainda assim exige abrir o jogo, o que está fora da capacidade de execução autônoma; o item entrega o roteiro pronto.
- [ ] Nenhum item deste backlog (A1-A4) deve reabrir ou contradizer uma decisão já fechada em 003-008 sem uma razão nova e concreta (ex.: não é para "melhorar" a exclusão de bots do item 005 — isso já é uma decisão fechada, não um gap).

## Fora de escopo

- [ ] Re-auditar mods do D20 que já têm prova fechada (SAIN/ORBIT/CustomClasses-Tank/SPTRecoilRework/FOV-Fix/BringBackConcussion/VisceralCombat) — só consolidar, não refazer.
- [ ] Qualquer mudança de comportamento de jogo nos itens 003-008 além de A3/A4 — este item é hardening/consolidação, não uma nova feature.
- [ ] Executar de fato o Bloco B (B1/B2) — é entregue como roteiro; a execução é do usuário, fora desta sessão.

## Referências

- [docs/trauma-matrix.md](../../docs/trauma-matrix.md) — D14/D15/D20
- [docs/trauma-behavior-matrix.md](../../docs/trauma-behavior-matrix.md) — matriz de comportamento total (item 011), §5 plano de teste
- [docs/coop-heal-matrix.md](../../docs/coop-heal-matrix.md) — protocolo de teste in-game pré-existente (esqueleto para B2)
- [docs/trauma-primitives.md](../../docs/trauma-primitives.md) — P1/P6/P9 (evidências de compat SAIN/ORBIT/RecoilRework/FOVFix)

## Histórico

| Data | Evento |
|---|---|
| 2026-07-20 | Item criado via `/create-spec`, escopo mapeado por pesquisa dedicada (o que já está coberto vs. o que falta) antes de escrever os critérios de aceite. |
| 2026-07-20 | Revisão `/review-spec` — esclarecido o critério de "entregue" (🟢) para um item com blocos de natureza mista (código+doc entregue vs. protocolo de teste manual pendente de execução), evitando duplo padrão em relação aos itens 002-008. |
