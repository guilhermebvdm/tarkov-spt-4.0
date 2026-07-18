# Backlog — TRL-ImmersiveCombatMedicine

> Índice de itens de backlog. Cada linha aponta para uma pasta `NNN-<slug>/` com a spec funcional, técnica e revisões. Escopo Trauma 2.0 derivado da matriz aprovada em [docs/trauma-matrix.md](../docs/trauma-matrix.md) (2026-07-18). O trabalho pré-backlog (merge Band-Aid+TrueTrauma, cura coop, reviews 01–04) está documentado em `memory/sessions.md` e `reviews/`.

| # | Título | Resumo | Pasta | Status |
|---|---|---|---|---|
| 001 | Spike: primitivas vanilla de trauma | Pesquisa técnica (sem código de produção): tipos de mancar vanilla + animação por lado da perna; efeito Tremor nativo; detecção de Painkiller ativo; agachar/derrubar involuntário (pose APIs); controle de levantar (bloquear + levantar lento); vozes de dor. Deliverable: doc de referência com APIs provadas (ilspycmd) + recomendação N1/N2 + mapa de pontos seguros de interferência no SAIN 4.4.3 e ORBIT (D14) + inventário das penalidades vanilla de perna/tremor (base do D11/decisão 18). | [001-spike-primitivas/](./001-spike-primitivas/) | ⚪ |
| 002 | Motor de estados de trauma | Tracker por player (humano+bot): conta Zerar/Quebrar por região, estado de analgésico, resolve a **coluna mais severa** da matriz, dispara entrada/saída de estado com **revert ao curar/operar**. Scaffolding de config F12 (probabilidades e timers), autoridade dono-only (D16), revert por cura remota (D17), anti-thrash de one-shots (decisão 19), toasts de 1ª ocorrência com i18n EN/PT (decisões 20/22), log de rolls (D19). Substrato dos itens 003–007. Aposenta a injeção legacy de fratura/dano (decisão 21). | [002-motor-estados/](./002-motor-estados/) | ⚪ |
| 003 | Pernas: Mancar N1/N2 + agachar involuntário | Estados contínuos de mancar (mapeados aos vanilla do spike 001, lado correto se possível) para Zerar/Quebrar 1–2 com gate de analgésico; agachar involuntário one-shot (não trava pose) no Zerar 2. Bots inclusos. | [003-pernas-mancar/](./003-pernas-mancar/) | ⚪ |
| 004 | Pernas: Cair + ciclo levantar 3s/15s | Quebrar 2 (e Z2+Q2) sem analgésico: prone forçado; janela de levantar+andar 3s → cai; nova tentativa após 15s; som de dor ao tentar no bloqueio (avaliar simular tentativa frustrada); levantar lento + som leve ao liberar. Timers configuráveis. **Bots: interferência cirúrgica devolvendo controle ao SAIN — reavaliação re-derruba enquanto a condição persistir; X do bot configurável (decisão 16).** | [004-pernas-cair-ciclo/](./004-pernas-cair-ciclo/) | ⚪ |
| 005 | Braços: Tremor + cancelamento de ADS escalonado | Tremor nativo contínuo por estado; com 2 braços comprometidos, ADS é cancelado após 4s (Z2) / 3s (Q2) / 2s (Z2+Q2); analgésico rebaixa conforme matriz; **lockout de re-ADS 1–1,5s configurável (decisão 17)**. Substitui a fadiga de mira atual (1s). | [005-bracos-tremor-ads/](./005-bracos-tremor-ads/) | ⚪ |
| 006 | Estômago: agachar probabilístico | Zerar estômago → agachar involuntário com p=75% (25% sob analgésico); **re-rola a cada zerada** (curou→zerou de novo). Reusa a primitiva do 003. Substitui o "sem ar" atual. | [006-estomago-agachar/](./006-estomago-agachar/) | ⚪ |
| 007 | Desmaio 2.0: gatilhos percentuais | Tórax: tiro ≥50% da vida ATUAL (pré-tiro) → p=50%, **imune com analgésico**; Cabeça: ≥25% → p=50% (25% com analgésico). **Pisos absolutos: tórax ≥25 dano, cabeça ≥10 (decisão 15).** Substitui thresholds fixos (35/10) mantendo todo o pipeline validado (relógio único, wake, grace, sync coop). | [007-desmaio-percentual/](./007-desmaio-percentual/) | ⚪ |
| 008 | Desmaio: duração aleatória min–max | Configs min/max + roll no ponto único `RANGE-READY` (HealthPatches); todo o resto (wake, rampa, pacote, espelhos) já deriva do deadline. | [008-desmaio-duracao-aleatoria/](./008-desmaio-duracao-aleatoria/) | ⚪ |
| 009 | Coop/bots: hardening do Trauma 2.0 | Passe transversal: estados novos visíveis/coerentes nos peers (pose/mancar via sync nativo Fika, vozes ouvidas, espelhos), bots com paridade nas regras (incl. UNTAR — D15), teste 2 PCs dedicado + suíte de compat D20 (SAIN/ORBIT/CustomClasses-Tank/RecoilRework/FOV-Fix/BringBackConcussion/Visceral Combat). | [009-coop-hardening/](./009-coop-hardening/) | ⚪ |
| 010 | Migração de configs + release | Aposentar/migrar `Sistema de Pernas/Braços/Estomago` antigos (padrão de migração one-time do CR-02/CR-03), PROPRIEDADES.md consolidado, distância de interação final, remoção [DEBUG-ICM], migração dos textos existentes p/ i18n EN/PT (decisão 22), zip de release. | [010-migracao-release/](./010-migracao-release/) | ⚪ |

## Legenda

- ⚪ Backlog · 🟡 Em progresso · 🟢 Entregue · 🔴 Cancelado

## Sequência e fases

- **Fase 0 — Fundações:** 001 (de-riscar as primitivas — a nota da matriz sobre animação por lado nasce aqui) → 002 (motor comum; tudo depois pluga nele).
- **Fase 1 — Pernas:** 003 → 004 (as primitivas mais difíceis; o agachar do 003 é reusado pelo 006).
- **Fase 2 — Braços:** 005 (independente da Fase 1; pode paralelizar após 002).
- **Fase 3 — Estômago:** 006 (pequeno; depende do agachar do 003).
- **Fase 4 — Desmaio:** 007 → 008 (montam sobre o pipeline de desmaio já validado; deixados para o fim por serem os de menor risco).
- **Fase 5 — Consolidação:** 009 → 010 (fecham o escopo com coop + release).

## Fluxo

1. `/add-backlog-item <mod> <descrição>` → cria entrada + invoca `/create-spec`
2. `/create-spec <ref>` → spec funcional (critérios de aceite + corner cases)
3. `/review-spec <ref>` → editor crítico da spec funcional
4. `/create-technical-spec <ref>` → pré-código com refs ao Assembly
5. `/review-technical-spec <ref>` → cria review-NN.md (incremental); resolver até zerar
6. `/code-mod <ref>` → implementa em `modded/`
7. `/code-review <ref>` → análise crítica; `/apply-code-review <ref>` até zerar bloqueadores
