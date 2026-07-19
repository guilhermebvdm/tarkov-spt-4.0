# 004 — Pernas: Cair + ciclo levantar 3s/15s

**Mod:** TRL-ImmersiveCombatMedicine
**Status:** Backlog
**Criado:** 2026-07-19

## Visão geral

Segundo consumidor do motor (002) e fechamento da região PERNAS: implementa as linhas **Cair** da matriz (Quebrar 2 e Zerar 2+Quebrar 2, sem analgésico) — derrubar forçado + ciclo de levantar 3s/15s com sons de dor — para humanos e bots (interferência cirúrgica, decisão 16). Entrega também a **arbitragem de pose D2/D3** (prone > agachar; desmaio pausa o ciclo) e assume do 003 o estado interim (Cair → N2 deixa de existir).

## Comportamento atual

Após o item 003 (v1.3.1): o motor publica a linha Cair corretamente, mas o consumidor 003 a mapeia para o **interim N2** (manca forte, não cai). O legado de pernas (prone forçado do "Sistema de Pernas" + regra de bot 90s) já está aposentado desde o 003 — hoje **ninguém cai** por pernas quebradas. As colunas com-analgésico das linhas Cair (Q2→N1, Z2+Q2→N2) já são responsabilidade permanente do 003.

## Comportamento desejado

1. **Derrubar forçado (one-shot de entrada):** ao entrar numa linha Cair (sem analgésico), o jogador é posto em prone imediatamente — sem dano de queda (D18), respeitando guards D7 (escada/corda/BTR/vault → adia; cancelado se o snapshot não mais exigir na execução) e anti-thrash do motor (decisão 19).
2. **Ciclo de levantar (enquanto a linha Cair persistir):**
   - **Fase JANELA:** o jogador PODE levantar e andar por **3 s** (configurável); ao expirar, cai automaticamente de novo. Andar na janela manca no cap N2 (premissa nova: a matriz não definia a locomoção da janela; N2 = estado mais severo de pé, coerente com o ranking D1 — registrar no item 011). O dano vanilla de andar com perna zerada na janela é aceito (D18).
   - **Fase BLOQUEIO:** após a queda, nova tentativa de levantar só após **15 s** (configurável). Tentar levantar no bloqueio → **som de dor** + tentativa frustrada simulada (avaliar na spec técnica: leve subida de pose que volta ao prone; fallback aceito = só o som). O input de levantar é negado — o jogador permanece prone.
   - **Fase LIBERAÇÃO:** ao expirar o bloqueio, volta à fase JANELA. O primeiro levantar pós-bloqueio é **lento** com som de dor mais leve (distinto do som de bloqueio).
3. **Saída do ciclo (reversão contínua, decisão 1):** curar a condição (própria, remota — D17 — ou cirurgia) ou tomar analgésico (decisão 14) encerra o ciclo NA HORA: prone deixa de ser forçado (o jogador levanta quando quiser, sem lentidão extra), e a linha nova da matriz assume (N1/N2 via 003). Expiração do analgésico com Q2 persistente → **cai na hora** (decisão 14), respeitando anti-thrash e D7.
4. **Arbitragem de pose (D2/D3), entregue aqui como regra do conjunto:**
   - Prone do ciclo > agachar involuntário: com ciclo ativo, one-shots de agachar (003/006) são absorvidos (não executam nem consomem cooldown).
   - Desmaio tem precedência (D3): desmaiar pausa o ciclo; no wake, o ciclo retoma **reiniciando a fase BLOQUEIO** (premissa nova: acordou ≠ pronto para levantar — registrar no item 011).
5. **Avaliação estabelecedora (premissa nova — registrar no item 011):** spawn ferido / religar toggle / adoção adiada com linha Cair NÃO derruba abruptamente: o ciclo estabelece na fase JANELA (como se tivesse acabado de levantar) — sem one-shot de derrubar, sem toast; 3 s depois a queda natural do ciclo ocorre. Paridade com a regra do motor (estabelecer sem one-shot).
6. **Bots (decisão 16 — interferência cirúrgica):** bot em linha Cair é derrubado (deitar forçado) e o controle volta ao SAIN imediatamente; quando a IA decidir levantar, a reavaliação re-derruba enquanto a condição persistir, com intervalo mínimo **X configurável** (separado dos timers humanos; default 15 s). Sem janela de 3 s nem sons de tentativa para bots. Funcional no headless (dono dos bots); UNTAR incluso (D15).
7. **Substituição do interim:** na entrega, o mapa do 003 deixa de traduzir Cair→N2 (o 004 assume a linha); nada mais do legado a desligar (já inerte desde o 003). Toggle do consumidor via **rename-at-delivery** do placeholder (padrão registrado no PROPRIEDADES): key nova nasce ON, órfã deletada.
8. **Feedback:** toast de 1ª ocorrência via infra do motor (EN/PT, decisão 22); sons de dor diegéticos (decisão 20) nos 3 pontos: queda/tentativa bloqueada (mais forte) e liberação (mais leve) — vozes do P5.
9. **Configurável (decisão 13):** janela (3 s), bloqueio (15 s), X do bot, toggle do consumidor. Timers com faixas sane no F12.

## Critérios de aceite

- [ ] Quebrar as 2 pernas sem analgésico → cai imediatamente (prone, sem dano de queda); levantar e andar → após ~3 s cai de novo; tentar levantar no bloqueio → som de dor e permanece prone; após ~15 s levanta de novo (lento + som leve). Timers alterados no F12 valem no ciclo seguinte.
- [ ] Tomar analgésico durante o BLOQUEIO → levanta livremente na hora (sem lentidão), mancar N1 (Q2) ou N2 (Z2+Q2) assume via 003; expiração do analgésico ainda com Q2 → cai na hora (≤1 s da expiração).
- [ ] Curar 1 fratura (tala/cirurgia, própria ou remota) durante o ciclo → ciclo encerra ≤1 s; linha nova da matriz assume (Z-count residual manda).
- [ ] Com ciclo ativo, zerar o estômago NÃO executa agachar (absorvido pela arbitragem D2, log registra); desmaiar pausa o ciclo e o wake retoma na fase BLOQUEIO.
- [ ] Bot com 2 pernas quebradas cai; ao levantar (decisão da IA), é re-derrubado enquanto a condição durar, respeitando o intervalo X; curar o bot (host ou médico client) encerra o ciclo do bot; headless idêntico (log).
- [ ] Interim do 003 removido: linha Cair não produz mais N2-sem-queda em nenhum caminho (log do consumidor).
- [ ] **Fika/multiplayer:** peers veem a queda, o prone contínuo, as tentativas e o levantar do dono (pose sync nativo) e ouvem os sons de dor; espelhos nunca aplicam efeito próprio (D16); bots do host/headless vistos pelos clients caindo/levantando sem rubber-banding.
- [ ] **Estado entre raids:** transit/fim de raid reseta o ciclo via motor; spawn ferido com Q2 estabelece na fase JANELA sem derrubar abrupto, sem toast; morte durante o ciclo não vaza estado para a raid seguinte.

## Corner cases

- [ ] Entrar na linha Cair DURANTE vault/escada/corda/BTR (D7): derrubar adiado para o próximo contexto válido; se curar/analgésico antes da execução, disparo cancelado sem consumir cooldown.
- [ ] Extração deitado: extrair prone durante o ciclo funciona (D18) — fim de raid limpa tudo.
- [ ] Alternância rápida de analgésico (tomar→expirar→tomar): anti-thrash impede re-queda em < 3–5 s; estados contínuos (mancar da janela) seguem o snapshot.
- [ ] Desligar o toggle do 004 mid-ciclo: prone deixa de ser forçado na hora, agendamentos cancelados, cooldowns não vazam; religar = avaliação estabelecedora (fase JANELA).
- [ ] Desmaiar durante a JANELA (de pé) vs durante o BLOQUEIO (prone): ambos pausam o ciclo sem double-prone/conflito de pose; wake sempre retoma em BLOQUEIO.
- [ ] Morrer durante o ciclo (qualquer fase): sem exceção, sem lock de input residual no observer, limpeza no despawn.
- [ ] Q2 estabelecido + terceira condição chegando (ex.: zerar perna já quebrada → Z2+Q2): linha muda dentro do próprio ciclo sem re-derrubar (já está no chão) e sem reset dos timers.
- [ ] Bot derrubado morto/despawnado no meio do intervalo X: bookkeeping limpo (sem entrada órfã, padrão CR-01-02 do 003).
- [ ] Compat: ORBIT/SAIN comandando o bot no instante do re-derrubar — interferência não mata camada de decisão (D14: pausar/retomar deixa a camada re-decidir).

## Fora de escopo

- [x] Colunas com-analgésico das linhas Cair (N1/N2) — permanecem no 003.
- [x] Estômago/braços/desmaio (006/005/007-008).
- [x] Migração de configs legadas e i18n dos textos antigos (010).
- [x] Suíte de compat completa (009) — aqui só o smoke SAIN/ORBIT do re-derrubar de bot.

## Referências

- [docs/trauma-matrix.md](../../docs/trauma-matrix.md) — decisões 6, 13, 14, 16, 19, 20, 21, 22; D1, D2, D3, D7, D10, D14, D15, D16, D17, D18, D19
- [003-pernas-mancar/](../003-pernas-mancar/) — primitiva de agachar, caps N1/N2, interim a substituir, padrão rename-at-delivery
- [002-motor-estados/](../002-motor-estados/) — eventos/snapshot/one-shot cooldown/avaliação estabelecedora
- [001-spike-primitivas/](../001-spike-primitivas/) — P4 (pose/prone/guards), P5 (vozes de dor), P6 (bots/SAIN/deitar)

## Histórico

| Data | Evento |
|---|---|
| 2026-07-19 | Spec funcional criada via `/create-spec` (memória: snapshot Sessão 2 + P-3.4 diretiva do overhaul; premissas novas marcadas p/ item 011: locomoção N2 na janela, wake retoma em BLOQUEIO, estabelecer na fase JANELA) |
