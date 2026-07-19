# 002 — Motor de estados de trauma

**Mod:** TRL-ImmersiveCombatMedicine
**Status:** Entregue
**Criado:** 2026-07-18

## Visão geral

Substrato do Trauma 2.0: um motor central que rastreia, por jogador (humanos locais e bots deste processo), as condições de trauma da matriz ([docs/trauma-matrix.md](../../docs/trauma-matrix.md)) — quantas pernas/braços zerados e quebrados, estômago zerado, analgésico ativo — e resolve a **linha mais severa aplicável por região**, publicando transições de estado e expondo consultas que os itens 003–007 consomem (003–006 consomem eventos/snapshot; o 007 consome a **consulta de analgésico** e as infras de log/i18n). O motor não aplica nenhum efeito de gameplay; ele é a fonte única de verdade sobre "em que estado de trauma cada jogador está".

## Comportamento atual

As checagens hoje são espalhadas e por-sistema: o loop de movimento lê "parte destruída" todo frame para pernas/braços, fratura nunca é rastreada como estado, analgésico não é considerado em nada, e a "injeção legacy" (30% fratura / 15 de dano ao tentar levantar com 2 pernas zeradas) cria gatilhos artificiais. Não há eventos de transição, reavaliação na expiração de efeitos, nem reversão de estado por cura remota. Textos exibidos são PT fixo.

## Comportamento desejado

1. **Rastreamento por dono:** o motor avalia SOMENTE jogadores cuja autoridade é deste processo (humano local; bots no host/headless). Espelhos de peers nunca geram efeito. O conjunto rastreado é dirigido por evento: registro no spawn (inclusive bots que spawnam DURANTE a raid) e remoção quando o jogador sai de jogo — morte (inclusive do humano local, com a raid seguindo em coop), extração individual, desconexão, despawn — sempre com limpeza SEM transições espúrias.
2. **Estados derivados da matriz:** por região, o motor conta condições (zeradas/quebradas), lê o analgésico e resolve a linha mais severa. **Ranking por região:** pernas = D1 (`Cair+ciclo > Agachar+N2 > N2 > N1 > Nada`); braços = derivado da decisão 3 (`Z2+Q2 (2s) > Q2 (3s) > Z2 (4s) > Tremor > Nada`); estômago = linha única. Combos mistos seguem a decisão 2; mesmo-membro conta como as duas condições (D4).
3. **Contrato de publicação e consulta:** consumidores assinam **eventos de transição** in-process (entrada/saída de estado, com jogador, região, linha da matriz e motivo) E têm **consulta de snapshot** do estado corrente por jogador/região (necessária ao ciclo do 004 — decisão 16: condição persistente não re-emite entrada — e a consumidor ligado tardiamente, que ao assinar recebe o snapshot dos estados já ativos). O motor também expõe a consulta **"analgésico ativo agora"** por jogador (consumida pelo 007 — decisões 9/15 — sem reimplementar a detecção do P3).
4. **Transições por evento onde importa:** entrada/saída de condição (dano, fratura, cura própria, **cura REMOTA via rede do mod** — D17, cirurgia restaurando parte zerada) e **aplicação/expiração de analgésico** (decisão 14 — reavaliação imediata e completa) disparam reavaliação na hora; polling de segurança ≤4 Hz (D19) cobre o que não tiver evento. **Exceção (D8/decisão 7):** rolls já resolvidos (agachar do estômago) NÃO re-rolam por mudança de analgésico — nova chance só em NOVA entrada da condição.
5. **Avaliação inicial estabelecedora:** no boot da raid, ao religar o master mid-raid e na chegada de transit, o motor ESTABELECE os estados contínuos pré-existentes (SPT persiste dano entre raids — spawn com perna zerada é normal) **sem disparar one-shots e sem toasts** — one-shots e toasts só em transições ocorridas em jogo.
6. **Anti-thrash:** o mesmo one-shot involuntário não re-dispara em <3–5 s configuráveis (decisão 19), por jogador e por tipo. **Fronteira:** o cooldown governa apenas one-shots PUBLICADOS pelo motor; re-disparos internos aos ciclos dos consumidores (auto-queda da janela de 3 s — decisão 6; re-derrubada do bot após X s — decisão 16, mesmo com X < cooldown) são isentos. O adiamento D7 (escada/BTR/vault) é responsabilidade do consumidor; em disparo adiado, o cooldown conta da EXECUÇÃO.
7. **Feedback e i18n:** infra de strings EN/PT do motor (decisão 22; idioma do jogo via P8, fallback EN na race de boot). **Toast de primeira ocorrência é gateado pelo consumidor ativo** (decisão 20 — feedback acompanha efeito): sem consumidor ligado, o motor apenas LOGA a ocorrência suprimida.
8. **Observabilidade como infra:** o motor loga toda transição com contexto e OFERECE a infra de log de rolls (jogador/região/condição/p/resultado — D19) para os consumidores 003/006/007 — no 002 ela é exercitada por log de transição (nenhum roll existe ainda).
9. **Toggles:** master "Trauma 2.0" no F12 + toggle POR consumidor. Motor publica sempre que o master estiver on; cada consumidor se auto-gateia e, ao ser desligado mid-raid, desfaz os próprios efeitos (lição do toggle preso do review-04 — regra do corner do master aplicada por consumidor). Consumidores nascem DESLIGADOS até os itens 003+ entregarem.
10. **Aposentadoria da injeção legacy** (decisão 21) — motor puramente reativo.
11. **Reset por raid:** todo estado zera na troca/fim de raid, incluindo transit (seguido da avaliação inicial estabelecedora do item 5).

## Critérios de aceite

- [ ] Com o motor ativo e nenhum consumidor ligado, o gameplay é IDÊNTICO ao atual — sem toasts (gate por consumidor), sem efeitos; únicas mudanças observáveis: injeção legacy removida (levantar com 2 pernas zeradas não fratura/dá dano) e logs novos.
- [ ] Log demonstra: zerar uma perna gera transição de entrada; reverter a condição (cura própria, cura por médico REMOTO ou cirurgia restaurando parte zerada) gera transição de saída ≤1 s após a aplicação.
- [ ] Tomar analgésico com 2 pernas quebradas rebaixa o estado publicado na hora; a EXPIRAÇÃO re-escala na hora (timestamps no log no mesmo segundo); o estado do estômago NÃO re-rola em nenhuma das duas mudanças (D8 — verificável por ausência de log de roll).
- [ ] Spawn já com perna zerada (dano persistido) e chegada de transit: estado contínuo estabelecido no log SEM one-shot e SEM toast.
- [ ] Infra i18n: com consumidor de teste ligado (ou stub), toast de 1ª ocorrência sai em EN com jogo em inglês e PT com jogo em português, 1× por estado por raid; sem consumidor, o log mostra a supressão.
- [ ] Cooldown anti-thrash: duas publicações do mesmo one-shot em <3 s → 1 disparo + log de supressão; snapshot query reflete o estado corrente consultado por um assinante tardio.
- [ ] **Fika/multiplayer:** em raid com 2+ processos, cada jogador é avaliado só pelo seu dono (log de cada processo mostra apenas os próprios players/bots); espelhos não publicam transição; bots avaliados no host/headless mesmo com dano vindo de client.
- [ ] **Estado entre raids:** raid1 → exit/morte/MIA/alt-F4 → raid2: nenhum estado, cooldown ou toast-visto sobrevive; transit reseta e re-estabelece via avaliação inicial.

## Corner cases

- [ ] Flicker de analgésico: contínuos re-escalonam a cada mudança; one-shots respeitam cooldown; estômago imune a re-roll (D8).
- [ ] Múltiplas transições no mesmo frame (rajada zera perna E quebra braço): reavaliação única consolidada, eventos em ordem determinística por região.
- [ ] Qualquer jogador rastreado saindo de jogo (bot morre/despawna, humano local morre com raid seguindo, extração individual, desconexão): limpeza sem transições espúrias.
- [ ] Bot spawnado mid-raid entra no rastreamento por evento de spawn (sem varredura por frame); primeira avaliação dele é estabelecedora (sem one-shot espúrio).
- [ ] Jogador desmaiado (sistema atual) durante transição de trauma: motor continua rastreando; precedência é dos consumidores (D3).
- [ ] Desligar "Ativar Mod"/master mid-raid: motor publica saída de todos os estados ativos e para; religar → avaliação inicial estabelecedora. Desligar UM consumidor: ele desfaz os próprios efeitos (motor segue publicando).
- [ ] Reconexão Fika de um peer: o dono re-avalia do zero; nenhum estado herdado de espelho.
- [ ] Idioma indisponível no boot (race de locale): fallback EN sem crash.
- [ ] Headless: motor roda para BOTS sem jogador local; feedback é no-op silencioso; sem dependência de câmera/render.
- [ ] Dois one-shots de TIPOS diferentes no mesmo instante: cooldown por tipo; arbitragem de pose (D2 — prone vence agachar) vive nas primitivas compartilhadas — agachar entregue no 003 (reusado pelo 006), derrubar + arbitragem D2 entregues no 004 (primeiro consumidor real — sync da rodada 2 do 003).
- [ ] Orçamento: evento + polling ≤4 Hz por jogador rastreado; nunca varredura por frame; sem alocações por frame no caminho quente.

## Fora de escopo

- [x] Os EFEITOS de gameplay (mancar/agachar/cair/tremor/ADS/desmaio) — itens 003–007.
- [x] Novos gatilhos de desmaio (item 007) — o desmaio atual segue intocado.
- [x] Migração das configs antigas do usuário (item 010).
- [x] Sync de rede novo — o motor consome os pacotes existentes (cura remota) e o sync nativo.

## Referências

- [docs/trauma-matrix.md](../../docs/trauma-matrix.md) — decisões 1, 2, 3, 7, 12, 14, 16, 19, 20, 21, 22; defaults D1, D2, D4, D7, D8, D10, D16, D17, D19
- [001-spike-primitivas/](../001-spike-primitivas/) — APIs (P3 analgésico, P8 idioma, P10 observação de estado)
- `memory/sessions.md` — lições: autoridade dono-only, toggle preso, dump incompleto

## Histórico

| Data | Evento |
|---|---|
| 2026-07-18 | Item criado via backlog Trauma 2.0; spec funcional criada via `/create-spec` |
| 2026-07-18 | Revisão rodada 1 (inline) — +3 corners: headless, colisão de one-shots, orçamento de execução |
| 2026-07-18 | Revisão rodada 2 (adversarial) — 13 achados aplicados: toast gateado por consumidor (resolve bloqueador AC1×AC4), avaliação inicial estabelecedora (spawn ferido/transit/religar), contrato de assinatura+snapshot+consulta de analgésico (007), infra de rolls compartilhada, carve-out D8/decisão 7, fronteira do cooldown vs ciclos (X do bot isento) e adiamento D7 no consumidor, ranking por região (braços via decisão 3), saída de jogo generalizada, bots mid-raid, semântica de toggles master+consumidor, arbitragem D2 na primitiva de pose, AC de cirurgia |
