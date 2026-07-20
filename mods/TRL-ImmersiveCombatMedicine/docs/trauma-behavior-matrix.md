# Matriz de Comportamento Total — Trauma 2.0

> **Data:** 2026-07-19<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [trauma-matrix.md](./trauma-matrix.md)<br>

---

Fonte da verdade dos critérios de aceite do sistema Trauma 2.0 (itens 002-008, entregues). Consolida a matriz de design original ([trauma-matrix.md](./trauma-matrix.md), aprovada em 2026-07-18) com TODA decisão, ajuste, premissa e default adotado durante a implementação — em linguagem de comportamento (o que o jogador observa e o que é configurável no F12), sem nomes de classe/método/arquivo do EFT. Escrito ao fim do ciclo 003-008, varrendo as specs funcionais/técnicas, todas as reviews (técnicas e de código) e a memória do mod desses 7 itens.

Itens 009 (hardening coop) e 010 (migração de configs + release) ainda não foram implementados — não são cobertos aqui além de menções de escopo futuro já sinalizadas pelos itens entregues.

## 1. Matriz de efeitos (estado atual, pós-008)

| Região | Condição | Sem analgésico | Com analgésico | Status |
|---|---|---|---|---|
| Perna | Zerar 1 | Mancar N1 (p=100%) | Nada | ✅ Conforme original |
| Perna | Zerar 2 | Agachar involuntário (p=100%, one-shot) + Mancar N2 contínuo | Mancar N1 | ✅ Conforme original |
| Perna | Quebrar 1 | Mancar N1 | Nada | ✅ Conforme original |
| Perna | Quebrar 2 | **Cair** (prone forçado, sem dano de queda) + ciclo Janela 3s/Bloqueio 15s | Mancar N1 | ✅ Conforme original — nota: D18 corrigido (§3.9) |
| Perna | Zerar 1 + Quebrar 1 | Mancar N2 | Mancar N1 | ✅ Conforme original |
| Perna | Zerar 2 + Quebrar 2 | Cair + ciclo Janela/Bloqueio | Mancar N2 | ✅ Conforme original |
| Estômago | Zerar | Agachar involuntário p=75% (re-rola a cada zerada nova) | p=25% | ✅ Conforme original — sliders SEM trava entre si (§3.5) |
| Tórax | Tiro que remove ≥50% da vida ATUAL (pré-tiro) + piso ≥25 dano absoluto | Desmaia p=50% | **Imune (p=0%)** | ✅ Conforme original |
| Cabeça | Tiro que remove ≥25% da vida ATUAL (pré-tiro) + piso ≥10 dano absoluto | Desmaia p=50% | Desmaia p=25% (nunca imune) | ✅ Conforme original |
| Braço | Zerar 1 | Tremor | Nada | ✅ Conforme original |
| Braço | Zerar 2 | Cancela ADS após 4s + Tremor | Tremor (visível mesmo sob analgésico) | ✅ Conforme original |
| Braço | Quebrar 1 | Tremor | Nada | ✅ Conforme original |
| Braço | Quebrar 2 | Cancela ADS após 3s + Tremor | Tremor | ✅ Conforme original |
| Braço | Zerar 1 + Quebrar 1 | Tremor | Nada | ✅ Conforme original |
| Braço | Zerar 2 + Quebrar 2 | Cancela ADS após 2s + Tremor | Tremor | ✅ Conforme original |

**Bots:** pernas (mancar + dip de agachar) e estômago (agachar) incluem bots com o mesmo comportamento do jogador humano. **Braços excluem bots de TUDO** (tremor, cancela-ADS, lockout) — mudança em relação à intenção original (ver §3.6). Desmaio inclui bots (mesmo predicado de analgésico, genérico).

## 2. Configuração F12 completa (Trauma 2.0)

### Seção 5 — Trauma 2.0 (Motor)

| Nome exibido | Controla | Padrão | Faixa |
|---|---|---|---|
| Enable Trauma 2.0 | Master do motor de rastreamento. Sem consumidor ligado = zero efeito de jogo, só log. | Ligado | — |
| Include Adrenaline As Painkiller | Adrenalina/Berserk conta como analgésico (paridade com o critério nativo do jogo). | Ligado | — |
| One-Shot Cooldown Seconds | Intervalo mínimo entre repetições do mesmo aviso involuntário (agachar/cair), por jogador e por tipo — cortado o loop de spam por analgésico. | 4,0 s | 3–5 s |
| Reconciliation Polling Hz *(avançado)* | Frequência da checagem de segurança para os raros casos sem aviso nativo (restauração total de cirurgia, revive Fika, cura em transição de mapa). | 2,0 Hz | 1–4 Hz |
| Verbose Engine Log *(avançado)* | Detalhe extra de log; transições e supressões são sempre logadas independente desta opção. | Desligado | — |

### Seção 6 — Trauma 2.0 (Consumidores)

| Nome exibido | Controla | Padrão |
|---|---|---|
| Legs Effects | Mancar N1/N2 + agachar involuntário (item 003). | Ligado |
| Fall Cycle | Cair + ciclo Janela/Bloqueio (item 004). | Ligado |
| Arms Effects | Tremor + cancela-ADS (item 005). | Ligado |
| Stomach Effects | Agachar involuntário do estômago (item 006). | Ligado |
| Blackout 2.0 | Sub-toggle do gatilho percentual de desmaio (item 007) — subordinado ao master "Sistema de Desmaio", seção 2. | Ligado |
| Debug Test Consumer *(avançado)* | Consumidor de teste sem efeito de jogo — só valida toast/tradução nas 3 regiões de estado contínuo. | Desligado |

Todos os 5 toggles reais (Legs/Fall/Arms/Stomach/Blackout2) nasceram como placeholders desligados no item 002 e foram **renomeados-e-ligados** na entrega do item correspondente (padrão "rename-at-delivery" — ver §4.1).

### Seção 7 — Trauma 2.0 (Pernas)

| Nome exibido | Controla | Padrão | Faixa |
|---|---|---|---|
| N1 Target Total Speed Percent | Velocidade TOTAL sentida em N1 (já considerando penalidade vanilla — o pior dos dois prevalece). | 80% | 50–95 |
| N2 Target Total Speed Percent | Idem para N2. Nunca fica mais "leve" que N1 — clamp automático + warn de log 1×. | 55% | 30–90 |
| Block Sprint On N2 | Sprint bloqueado em N2 mesmo sob analgésico (o vanilla libera; o mod não). | Ligado | — |
| Bot Crouch Dip Seconds *(avançado)* | Duração do "dip" de agachar do bot fora de combate. | 0,7 s | 0,3–1,5 |

### Seção 8 — Trauma 2.0 (Queda)

| Nome exibido | Controla | Padrão | Faixa |
|---|---|---|---|
| Fall Window Seconds | Duração da Janela (de pé, N2, sprint sempre bloqueado — mesmo com "Block Sprint On N2" desligado, §3.2). | 3 s | 1–10 |
| Fall Block Seconds | Duração do Bloqueio (não pode levantar; tentativa = som de dor, sem repetir). | 15 s | 5–60 |
| Bot Fall Hold Seconds | Tempo mínimo (X) que o bot fica deitado antes da IA poder tentar levantar de novo. | 15 s | 5–120 |

### Seção 9 — Trauma 2.0 (Braços)

| Nome exibido | Controla | Padrão | Faixa |
|---|---|---|---|
| ADS Cancel Seconds (Zeroed x2) | Segundos mirando com 2 braços ZERADOS até cancelar. | 4 | 1–10 |
| ADS Cancel Seconds (Fractured x2) | Idem, 2 braços FRATURADOS (fratura dói mais — decisão 3). | 3 | 1–10 |
| ADS Cancel Seconds (Zeroed + Fractured x2) | Idem, misto. Efetivo = mínimo dos 3 timers (nunca mais lento que os menos severos). | 2 | 1–10 |
| Re-ADS Lockout Seconds | Bloqueio de re-mira pós-cancelamento (persiste à troca de arma). Faixa fixada pela decisão 17. | 1,5 | 1,0–1,5 |

### Seção 10 — Trauma 2.0 (Estômago)

| Nome exibido | Controla | Padrão | Faixa |
|---|---|---|---|
| Stomach Crouch Chance Percent | Chance de agachar ao zerar o estômago SEM analgésico. | 75% | 0–100 |
| Stomach Crouch Chance Under Painkiller Percent | Idem COM analgésico ativo no instante da zerada. Independente do slider acima — **sem trava entre os dois** (§3.5). | 25% | 0–100 |

### Seção 11 — Trauma 2.0 (Desmaio)

| Nome exibido | Controla | Padrão | Faixa |
|---|---|---|---|
| Chest Faint Percent Threshold | % da vida atual pré-tiro do tórax para rolar a chance. | 50% | 0–100 |
| Head Faint Percent Threshold | Idem cabeça. | 25% | 0–100 |
| Chest Faint Absolute Damage Floor | Piso de dano absoluto do tórax (além do percentual). | 25 | 0–100 |
| Head Faint Absolute Damage Floor | Piso de dano absoluto da cabeça. | 10 | 0–100 |

As chances de roll em si (50%/50%/25%/0%) são **constantes fixas**, não configuráveis — só os 4 números acima são expostos.

### Seção 3 — Balanceamento (Trauma) — duração do desmaio

| Nome exibido | Controla | Padrão | Faixa |
|---|---|---|---|
| Duracao Minima do Desmaio | Piso do sorteio uniforme da duração do desmaio. `min > max` → valores trocados silenciosamente antes do sorteio. | 20 s | 5–120 |
| Duracao Maxima do Desmaio | Teto do sorteio. Com `min == max`, comportamento idêntico a duração fixa (caso degenerado, não especial). | 20 s | 5–120 |

**Master do pipeline de desmaio:** diferente de todos os outros itens, **"Sistema de Desmaio"** (seção 2, legado) continua sendo o interruptor mestre de TODO o pipeline (gatilho + temporizador + despertar + graça + sincronização) — decisão explícita do item 007 (§3.7). "Blackout 2.0" é sub-toggle, não substituto.

### Seção 2 — Mecânicas (Trauma) — legados INERTES

`Sistema de Pernas`, `Sistema de Braços`, `Sistema de Estomago` permanecem no F12 sem nenhum efeito (tooltip avisa) — remoção definitiva das keys é escopo do item 010. `Sistema de Desmaio` é a EXCEÇÃO: continua ativo como master (ver acima).

## 3. Decisões e defaults consolidados

### 3.1 Motor / geral (item 002)

- Estados contínuos (mancar/tremor/agachar) revertem ao curar — inclusive cura remota via rede (D17); desmaio é evento único, não reverte.
- Combos mistos resolvem pela coluna/linha mais severa (decisão 2, D1).
- **D2 — conflito de POSE entre regiões resolve pela mais severa** (prone vence agachar) — regra geral que sustenta, por exemplo, o ciclo de queda (004) absorver um agachar de estômago (006) sem somar um segundo efeito de pose.
- **D4 — Zerar+Quebrar no MESMO membro conta como Z1+Q1** (contagem por CONDIÇÃO, não por membro) — uma perna com as duas condições ativas não conta como "dois membros comprometidos".
- Autoridade dono-only: motor avalia só quem o processo possui (humano local; bots no host/headless); espelhos nunca aplicam efeito (D16).
- Reavaliação "na hora" (decisão 14) formalizada como **≤1 quadro de jogo** — não é simultaneidade perfeita garantida.
- Toast de 1ª ocorrência só aparece se existir consumidor ativo para a região; sem consumidor, o motor loga a supressão mas não "queima" a primeira ocorrência (aparece quando o consumidor for ligado). Note que o registro de consumidor é por REGIÃO, não por linha específica — ver §3.3 para o corner concreto (toast do "Cair" podendo aparecer com "Fall Cycle" OFF).
- Anti-thrash (decisão 19): 3–5s (default 4s) entre repetições do mesmo one-shot, por jogador+tipo — vale só para o barramento do motor, não para ciclos internos de um consumidor (ex.: hold de bot do item 004).
- Estabelecimento "silencioso" (spawn ferido, religar mid-raid, transição de mapa): constata estado existente sem toast/one-shot.
- Fronteira de ativação (achado só em code-review, não estava na spec original): motor fica inerte em hideout/tela de loading; nunca "trava desligado" por instabilidade de inicialização.
- Analgésico = efeito nativo Painkiller ativo, opcionalmente incluindo Adrenalina/Berserk (config); bots com Painkiller PERMANENTE (bosses em certas dificuldades) ficam estáveis na coluna "com analgésico" sem timer de expiração — não é bug.
- **Decisão 21 — injeção legacy aposentada:** a regra antiga (levantar com as 2 pernas zeradas tinha 30% de chance de fraturar + causava 15 de dano extra) foi removida neste item — a matriz passou a ser puramente reativa (fratura só vem de combate/vanilla; Zerar 2 nunca escala sozinho ao Cair).
- **Decisão 22 — i18n:** todo texto exibido ao jogador (toasts, avisos de estado) nasce em inglês padrão, com tradução para português quando o idioma do jogo é português. Textos legados do mod (hoje fixos em PT) migram só no item 010.

### 3.2 Pernas — mancar (item 003)

- N1/N2 = velocidade TOTAL sentida (calibrada sobre a penalidade vanilla — nunca soma, nunca acelera; o pior prevalece, com clamp logado).
- Agachar involuntário (Zerar 2) é one-shot — não trava a pose, jogador pode ficar de pé livremente depois.
- Sprint bloqueado em N2 **mesmo sob analgésico** — decisão do mod além da paridade vanilla (vanilla libera sprint com analgésico).
- N2 nunca fica mais "leve" que N1 mesmo com config invertida — `min(N2,N1)`, warn de log 1×.
- Guards de contexto (D7): agachar adia em escada/corda/BTR/vault; se a condição some antes de executar, cancela silenciosamente sem consumir cooldown.
- Handoff com o item 004 nas duas direções: entrando na linha "Cair", o 003 cede o cap de velocidade ao 004; saindo, o 004 devolve ao 003.

### 3.3 Pernas — ciclo de queda (item 004)

- Ciclo de 3 fases: Janela (3s, de pé, N2, sprint SEMPRE bloqueado) → re-cai automaticamente → Bloqueio (15s, não pode levantar, só rastejar) → Liberação (levanta quando quiser, devagar, som leve).
- Janela só começa a contar quando o jogador está EFETIVAMENTE de pé (fim da rampa de levantar), não no momento do input.
- **Mudar os 3 timers (Janela/Bloqueio/Bot Hold) no F12 nunca rebase a contagem de um ciclo já em andamento** — só vale a partir da próxima fase que começar depois da mudança (mesma garantia de "relógio único" dos itens 002/008).
- Fallback agachado se o prone for recusado por motivo que não seja contexto protegido (ex.: sem espaço físico) — mod tenta reengatar o prone depois; **enquanto isso, ficar de pé continua NEGADO (mesma trava do Bloqueio normal), mas mover-se agachado é livre** (equivalente a rastejar) até o prone ser conseguido.
- **Wake (desmaio/downed) sempre reinicia no Bloqueio**, nunca retoma de onde parou — mesmo saindo da Janela ou da Liberação. *(Nota: isso é uma correção de redação em relação à decisão 3 original — "pausa o ciclo... retoma no wake" sugeria retomar a MESMA fase; ver §3.9.)*
- **Establishing (spawn ferido, religar, adoção) sempre começa na Janela** — como se tivesse acabado de levantar, sem drama de entrada; só vai direto ao Bloqueio se já estiver deitado no instante.
- Cura parcial (linha continua "Cair") não reinicia o ciclo — segue de onde estava.
- Qualquer outro one-shot de agachar (estômago) é absorvido enquanto o ciclo está ativo, em qualquer fase — nunca soma um segundo efeito de pose.
- **Cap de velocidade N2 da Janela é território EXCLUSIVO deste item — independe do toggle "Legs Effects" (003).** Desligar "Legs Effects" não remove o cap da Janela; só "Fall Cycle" (004) controla isso.
- **Toast de 1ª ocorrência é por REGIÃO, não por linha específica da matriz:** com "Fall Cycle" OFF e "Legs Effects" ON, o aviso da linha "Cair" ainda pode aparecer (o registro de consumidor cobre a região Pernas inteira, não a linha exata) — granularizar por linha exigiria mudar o motor, fora de escopo.
- **Colisão no MESMO evento de dano — Cair + Desmaio simultâneos** (ex.: uma explosão zera as 2 pernas E atinge o tórax/cabeça no mesmo golpe): o desmaio VENCE — o derrubar do ciclo é absorvido (com devolução do cooldown), e ao acordar o jogador entra direto no Bloqueio (já está prone pelo desmaio, não pelo ciclo).
- **Efeito colateral aceito do Bloqueio:** interações de mundo que dependem de estar de pé (abrir porta, pegar item do chão) também podem ficar negadas durante o bloqueio — parte da incapacitação; a Liberação restaura tudo.
- Bot: hold mínimo de X segundos (config) sem combater; ao expirar, controle devolvido à camada de decisão da IA (não necessariamente "SAIN" especificamente — decisão 16 refinada); se a IA decidir levantar e a fratura persistir, re-derruba (ciclo próprio, sem janela/sons do humano). **Bosses e seguidores especiais (bots "chefe") ficam de FORA do ciclo inteiro** — só recebem manqueira, se aplicável, nunca são derrubados/deitados por este item.
- D18 corrigido: não existe dano vanilla "de andar" no jogo — o "dano aceito" da Janela é só de vault/escalada (o sprint já está bloqueado).
- Sons de dor (queda/tentativa bloqueada/levantar leve) audíveis a peers via voz nativa; fallback de áudio local existe mas só o dono ouve nesse caminho.

### 3.4 Braços — tremor e cancela-ADS (item 005)

- Tremor contínuo por estado (substitui a fadiga de mira legada de 1s).
- Analgésico remove tremor de 1-braço; em 2-braços, o tremor continua visível mesmo sob analgésico (o mod contorna a supressão visual nativa só para o efeito dele — o tremor-por-dor nativo continua se escondendo normalmente).
- Cancela-ADS escalonado (4s/3s/2s) com o mesmo timer sempre reiniciado (nunca retroativo) se a severidade mudar mid-mira.
- Lockout de re-ADS (1-1,5s) é do JOGADOR, não da arma — persiste à troca de arma; tentativa durante o bloqueio = grito de dor (1×/bloqueio), suprimido se o jogador estiver incapacitado (desmaiado/caído).
- Efeito colateral cosmético aceito: re-tentar mirar no lockout pode recolher bússola/item da mão esquerda (raro).
- Degradação graciosa: se o mecanismo interno do tremor falhar (ex.: mudança de versão do jogo quebrando o acesso reflexivo usado), o tremor vira um no-op silencioso — mas o cancelamento de ADS e o lockout continuam funcionando normalmente (falha parcial, não total do item).

### 3.5 Estômago — agachar probabilístico (item 006)

- Re-rola a cada zerada nova (curar→zerar de novo = novo roll); permanecer zerado não re-rola.
- Analgésico do instante da zerada (latch) — nunca re-consultado depois.
- Sliders (75%/25%) **sem trava entre si** — diferente do `min(N2,N1)` das pernas, aqui inverter é permitido (não há invariante de severidade a proteger).
- Cooldown compartilhado com o agachar de pernas na mesma janela anti-thrash — colapsam em um só; a reserva do cooldown é feita no instante da DECISÃO de tentar (não na execução), fechando o corner de "dois agachares" no caminho adiado.
- Chamada direta na primitiva de agachar (nunca pelo barramento de eventos do motor) — evita vazamento entre consumidores por construção.
- Sem voz própria (paridade com o agachar silencioso de pernas).
- **Roll no mesmo frame de um desmaio/downed:** nenhuma pose é forçada sobre um jogador inconsciente — o agachar vira NOOP com devolução de cooldown. Premissa registrada como condicional: se o item 007 (desmaio) mudar como a pose do desmaio funciona, este ponto precisa ser revisitado.

### 3.6 Braços/bots — decisão revisada (D9)

A matriz original (D9) previa "tremor aplicado (cosmético) a bots; cancela-ADS não". Durante o item 005 essa decisão foi **refutada como escrita**: bots ficaram **excluídos de TUDO**, inclusive do tremor — o "tremor cosmético" não tem canal de exibição real (efeito só-primeira-pessoa) e o desvio de pontaria correspondente não é medido nem confiável. É a única mudança de comportamento observável em relação à matriz original.

### 3.7 Desmaio percentual (item 007)

- Percentual da vida atual pré-tiro (não pós-tiro, não vida máxima) + piso absoluto de segurança, independente do percentual.
- Sem agregação de pellets — cada pellet avaliado contra a vida imediatamente anterior a ele.
- Tórax imune total sob analgésico; cabeça só reduz a chance pela metade (nunca imune).
- **Decisão de design nova, divergente do padrão dos demais itens:** "Sistema de Desmaio" continua sendo o master de todo o pipeline (não só do gatilho) — decisão tomada deliberadamente para não mexer no master de um pipeline com histórico de bugs de timing. "Blackout 2.0" é sub-toggle.
- Desmaio é EVENTO (roll único no instante do tiro), não estado contínuo — não há "revert" quando o analgésico expira depois (diferente de mancar/tremor/agachar).
- Sem toast — o desmaio nunca foi registrado no barramento de observabilidade do motor (esse cobre só as 3 regiões de estado contínuo).
- **D5/D6 (invariantes herdados, implementados fielmente):** o dano considerado é sempre o EFETIVO pós-armadura (não o dano bruto do tiro); só 5 tipos de dano disparam o gatilho (Bullet/Explosion/Sniper/Landmine/GrenadeFragment) — dano de queda ou melee, por exemplo, nunca desmaia.

### 3.8 Duração do desmaio (item 008)

- Sorteio uniforme min/max a cada novo desmaio, gravado uma única vez no deadline absoluto — todo o resto do pipeline (relógio de despertar, rampa, sincronização) lê esse deadline de forma opaca.
- `min > max` normalizado silenciosamente (troca os dois), sem warning.
- **Migração por CÓPIA do valor legado do usuário** — diferente do padrão "rename-at-delivery com descarte" dos itens 003-007. Motivo: o campo antigo era um valor REAL ajustado pelo usuário (histórico de tuning ao vivo documentado), não um placeholder nunca escolhido — descartá-lo seria regressão de UX. Consequência para o item 010: dois padrões de migração distintos (descarte de placeholder vs. cópia de valor real) coexistem hoje no código e precisarão ser consolidados.

### 3.9 Correções de redação sobre a matriz original

- **D12** (composição de velocidade): a matriz original dizia "multiplicativa"; a implementação (item 003) corrigiu para "por MÍNIMO do dicionário de limites de velocidade" — o `trauma-matrix.md` original **ainda não foi retrofitado** com essa correção (pendência viva a resolver).
- **D18** (dano de queda na Janela): a matriz original dizia "toma o dano vanilla de andar"; não existe dano vanilla por só andar — o dano aceito na prática é de vault/escalada (item 004 corrigiu a redação).
- **Decisão 16** (bots no ciclo de queda): "devolver ao SAIN" foi refinado para "devolver à camada de decisão da IA" (pode ser SAIN, ORBIT ou outra, dependendo do bot) — mesma intenção, redação mais precisa.
- **D9** (tremor cosmético em bots): refutada — ver §3.6.
- **Decisão 3** (desmaio pausa o ciclo de queda e "retoma no wake"): a redação original sugere retomar a MESMA fase; a implementação decidiu que o wake **sempre reinicia no Bloqueio**, nunca retoma a fase anterior (Janela/Liberação) — refinamento de segurança (acordar não é o mesmo que estar pronto para ficar de pé), não uma leitura literal da palavra "retoma". Ver §3.3.

## 4. Premissas e interims históricos (rastro, não mais ativos)

### 4.1 Rename-at-delivery (padrão dominante 002→007)

Todo toggle de consumidor nasceu como placeholder desligado no item 002 (`X Effects (item NNN)`) e foi renomeado + ligado por padrão na entrega real, com a key órfã deletada **sem copiar valor** (o `false` do placeholder nunca foi escolha do usuário). Aplicado em 003, 004, 005, 006, 007. O item 008 rompeu esse padrão deliberadamente (§3.8) por lidar com um valor real, não um placeholder.

### 4.2 Handoff "Cair → N2" (003 → 004)

Antes do item 004 existir, a linha "Cair" da matriz (Quebrar 2 pernas sem analgésico) era temporariamente tratada pelo item 003 como apenas mancar N2 (sem derrubar de fato) — decisão explicitamente marcada como interim. Removido na entrega do 004, que assumiu a linha "Cair" por completo (handoff nas duas direções, entrando e saindo da linha).

### 4.3 Evolução da chave de dedup da fila de one-shots adiados

- Item 003/004: fila de adiados (contexto D7) fazia dedup por `(jogador, tipo)`.
- Item 006: ampliada para `(jogador, tipo, região)` — sem isso, um agachar de pernas e um de estômago adiados colidiriam numa única entrada, e curar a região errada cancelaria a intenção da outra. Mudança aditiva, confirmada bit-idêntica para o tráfego de pernas/quedas já existente.

### 4.4 Voz dupla-fonte (004 × 005) — reconciliação pendente

O item 004 entregou o utilitário de voz de dor (grito forte/leve); o item 005 o reusou por extensão, sem tocar no anti-spam do 004. Os dois emissores podem, em teoria, competir pelo mesmo "locutor" quando pernas e braços sofrem no mesmo instante — nenhum bug conhecido, mas a reconciliação formal dos dois canais fica **registrada como pendência para trabalho futuro** (candidato: item 009).

### 4.5 Boilerplate de `Update()` duplicado (débito técnico, 4 consumidores)

O esqueleto de "detectar troca de raid / toggle ligado-desligado" se repete quase idêntico em cada consumidor (003, 004, 005, 006) sem um helper compartilhado. Identificado e deliberadamente **deferido** — candidato natural: item 009 (que já varre os 4 consumidores) ou uma limpeza dedicada.

### 4.6 Padrões de migração de config divergentes (a consolidar no item 010)

- Placeholder inerte → rename-at-delivery com descarte (003-007).
- Valor real do usuário → migração por cópia com parse culture-invariant (008, primeiro parse de `float` numa migração — risco de cultura numérica documentado).
- O código de busca de config órfã já tem 6 cópias quase idênticas do mesmo idioma (bool/rename × float/cópia) — extração de um helper compartilhado deferida para o item 010, que de qualquer forma vai mexer nesse método para as remoções finais.

### 4.7 Reflexo residual dos itens legados

`Sistema de Pernas`, `Sistema de Braços`, `Sistema de Estomago` seguem no F12 como keys INERTES (tooltip avisa); remoção definitiva é escopo do item 010. O caso de voz "Gut" (estômago legado) também ficou órfão no código auxiliar de voz, sem call site — mesma fila de limpeza do 010.

## 5. Plano de teste estruturado

Cobertura mínima: 1 cenário por linha da matriz (§1) + os corners mais arriscados identificados durante a implementação. Roteiro pensado para validação manual in-game (solo primeiro, depois 2 PCs Fika).

### 5.1 Pernas — mancar (003)

0. **(Decisão 21)** Levantar com as 2 pernas zeradas NUNCA fratura nem causa dano extra — a injeção legacy foi aposentada; comportamento puramente reativo (fratura só vem de combate/vanilla).
1. Zerar 1 perna → manca N1, log confirma; tomar analgésico → nada; expirar → manca N1 de novo.
2. Zerar 2 pernas → agacha 1×, manca N2, sprint bloqueado mesmo com analgésico.
3. Curar (própria/aliado/cirurgia) → manca some em ≤1s.
4. Bot zera perna → manca/dip visível ao host e a peers.
5. Toggle "Legs Effects" OFF mid-raid → desfaz caps na hora, inclusive em jogador DOWNED.

### 5.2 Pernas — ciclo de queda (004)

6. Quebrar 2 pernas sem analgésico → cai (prone, sem dano de queda).
7. Janela ~3s de pé → anda com N2, sprint bloqueado → re-cai sozinho ao expirar.
8. Tentar levantar durante o Bloqueio (15s) → som de dor, negado, sem repetir o som a cada tentativa.
9. Liberação → levanta devagar com som leve; ficar deitado indefinidamente também é válido.
10. Analgésico → levanta livre, manca N1/N2 conforme severidade restante.
11. Desmaiar em qualquer fase do ciclo → ao acordar, sempre no Bloqueio (nunca retoma a fase anterior).
12. Bot cai/re-cai respeitando X segundos configurável; controle não trava se a IA decidir levantar de novo com a fratura persistindo.
13. Extração funciona prone; transit/fim de raid reseta o ciclo sem resíduo.
14. Toggle "Fall Cycle" OFF mid-raid → solta tudo na hora, sem retorno ao mancar interino do 003; cap N2 da Janela some (é território exclusivo deste item, não do 003).
14b. Toggle "Fall Cycle" OFF, "Legs Effects" ON, e o jogador entra na condição "Cair" pela matriz → o toast de 1ª ocorrência ainda pode aparecer (registro é por região, não por linha) mesmo sem o ciclo executar.
14c. Mudar "Fall Window/Block Seconds" no F12 no meio de um ciclo em andamento → não afeta o ciclo corrente, só o próximo a começar.
14d. Mesmo evento de dano zera as 2 pernas E causa desmaio (ex.: explosão) → desmaio vence; ciclo de queda é absorvido (cooldown devolvido); ao acordar, entra direto no Bloqueio.
14e. Prone recusado por falta de espaço físico (não é escada/BTR/vault) → fallback agachado; ficar de pé negado, mover-se agachado livre, até o prone ser conseguido na re-tentativa.
14f. Bot-boss/seguidor especial com as 2 pernas quebradas → NÃO entra no ciclo de queda (não é derrubado/deitado), só recebe manqueira se aplicável.

### 5.3 Braços — tremor e ADS (005)

15. Zerar 1 braço → tremor visível; analgésico remove.
16. Zerar 2 braços → tremor visível MESMO sob analgésico; segurar mira 4s → cancela sozinho.
17. Quebrar 2 braços → cancela em 3s; misto (1 zerado + 1 fraturado) → cancela em 2s (o menor dos três).
18. Re-mirar durante o lockout (1-1,5s) → bloqueado + grito de dor (1× por bloqueio); trocar de arma durante o lockout NÃO libera.
19. Desmaiar durante o lockout → bloqueio continua, mas sem grito de dor.
20. Bot com braços feridos → SEM tremor, SEM cancela-ADS, SEM lockout (confirma exclusão total).

### 5.4 Estômago — agachar probabilístico (006)

21. Zerar estômago sem analgésico → ~75% de chance de agachar (rodar ~20 vezes, contar no log — esperar 11-19 sucessos).
22. Mesma zerada com analgésico ativo → ~25% de chance (esperar 1-9 em 20).
23. Curar e zerar de novo na mesma raid → novo roll (log mostra 2 rolls distintos).
24. Zerar estômago enquanto no ciclo de queda (004) ativo → absorvido, sem segundo efeito de pose.
25. Estômago + pernas zerando quase juntos → 1 agachar só (log mostra a supressão do segundo).
25b. Estômago adiado (D7, ex. escada) coexistindo com um agachar de pernas TAMBÉM adiado ao mesmo tempo → as duas intenções coexistem na fila sem interferência cruzada; curar uma região não cancela a intenção pendente da outra.
25c. Roll de estômago no mesmo frame de um desmaio/downed → NOOP, nenhuma pose forçada sobre jogador inconsciente.
26. Toggle "Stomach Effects" OFF, "Legs Effects" ON → zerar estômago não agacha nem rola (grep de log ausente); inverso também.

### 5.5 Desmaio — gatilho percentual (007)

27. Tórax removendo ≥50% da vida atual + ≥25 de dano absoluto, sem analgésico → rola 50%.
28. Mesmo hit com analgésico ativo → NUNCA desmaia (tórax imune).
29. Cabeça removendo ≥25% + ≥10 de dano, sem analgésico → rola 50%; com analgésico → rola 25% (nunca imune).
30. Hit que atinge o percentual mas fica abaixo do piso absoluto → não rola nada (log confirma "piso", não "percentual").
31. Rajada de espingarda → cada pellet avaliado contra a vida imediatamente anterior a ele (sem soma).
32. Hit simultâneo tórax+cabeça (ex.: explosão) → nunca dois desmaios/duração sobreposta.
33. Toggle "Blackout 2.0" OFF, "Sistema de Desmaio" ON → nenhum desmaio por dano dispara (sem fallback ao limiar fixo antigo).
34. "Sistema de Desmaio" OFF → desliga TUDO (gatilho, temporizador, despertar, sync) — master continua sendo essa key legada, não o novo sub-toggle.

### 5.6 Desmaio — duração aleatória (008)

35. Configurar min=5/max=60, causar ~10 desmaios → durações espalhadas no log (não concentradas numa ponta).
36. min == max → duração sempre exatamente esse valor.
37. min > max configurado → normalizado sem erro (log mostra os valores trocados).
38. Atualizar de uma versão anterior com "Duracao do Desmaio" customizada (inclusive valor fracionário, ex. 47.5, testado com o SO em pt-BR) → Min e Max nascem iguais ao valor antigo, sem distorção pela vírgula/ponto decimal.
39. Mudar min/max no F12 durante um desmaio em curso → não afeta esse desmaio, só o próximo.

### 5.7 Corners transversais (multi-item)

40. Fika/coop com 2 PCs: peer vê mancar/tremor/pose/queda pelo sync nativo; bots do host aparecem coerentes ao client; nenhum efeito aplicado por espelho.
41. Religar o mod inteiro no meio da raid com múltiplas condições já ativas (pernas + braços + estômago zerados) → todos os estados se re-estabelecem sem toast/one-shot duplicado.
42. Raid1 → extração → Raid2: nenhum estado, cooldown ou toast "já visto" sobrevive entre raids.
43. Hideout: nenhum log de tracking do motor aparece fora de raid.
44. Toast de 1ª ocorrência aparece 1×/estado/raid, em EN por padrão e PT quando o idioma do jogo é português.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-19 | Guilherme (com Claude) | Criação — consolida `trauma-matrix.md` + toda decisão/premissa adotada durante os itens 002-008, varrendo specs, reviews e memória do mod. Item 011 do backlog. |
| 2026-07-19 | Guilherme (com Claude) | Verificação de completude independente (releitura cética das fontes primárias, não confiando na síntese inicial): 9 premissas de prioridade alta incorporadas (a maioria do item 004 — timers não rebaseiam mid-ciclo, cap N2 independente do toggle 003, toast por região, colisão Cair+Desmaio, fallback agachado com pose-lock, efeito colateral do Bloqueio, bosses fora do ciclo, decisão 21 aposentada, roll de estômago durante desmaio/downed); decisões 21/22 e defaults D2/D4/D5/D6 adicionados explicitamente; correção de tag `(avançado)` em `Bot Crouch Dip Seconds`; 6 cenários novos no plano de teste (§5.1/§5.2/§5.4). |
