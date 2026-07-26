# Matriz de Comportamento Total — Trauma 2.0

> **Data:** 2026-07-19<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [trauma-matrix.md](./trauma-matrix.md) · [trauma-compat-suite.md](./trauma-compat-suite.md) · [trauma-coop-test-protocol.md](./trauma-coop-test-protocol.md)<br>

---

Fonte da verdade dos critérios de aceite do sistema Trauma 2.0 — itens 002-010 do backlog, todos entregues. Consolida a matriz de design original ([trauma-matrix.md](./trauma-matrix.md), aprovada em 2026-07-18) com toda decisão, ajuste e default adotado durante a implementação.

**Como ler este documento:** as seções 1-4 são de **produto** — o que o jogador vê, o que é configurável, o que precisa ser validado. Não citam classe/método/arquivo de código. A seção 5 é o **apêndice técnico** — decisões de implementação, correções de redação e histórico, para quem for tocar o código ou investigar um comportamento estranho.

## 1. Nomenclatura

| Termo usado aqui | Significado | Vanilla ou nosso? |
|---|---|---|
| **Manqueira Leve** (antes "N1") | Cap de velocidade leve, aplicado com 1 condição de perna ativa | **Nosso.** Calibrado por cima do cap vanilla (ver §4.1) |
| **Manqueira Severa** (antes "N2") | Cap de velocidade mais forte, aplicado com 2 condições de perna ativas | **Nosso.** Idem |
| **Cair / Ciclo de Queda** | Derrubar forçado com janela de pé (3s) e bloqueio de levantar (15s) | **Nosso.** O vanilla não derruba por perna ferida |
| **Tremor** | Chacoalho de mira aplicado ao braço ferido | **Nosso lifecycle**, mas usa o mesmo efeito visual que o jogo já tem (ver §4.1) |
| **Desmaio percentual** | Rolagem de desmaio baseada em % da vida atual perdida no tiro | **Nosso.** O vanilla usa limiar fixo de dano |

> **Nota sobre "Manqueira Leve/Severa":** o jogo **não tem** nomes ou níveis de mancar — é um cap numérico de velocidade só por CONTAGEM de pernas feridas (1 perna = 30% da velocidade máxima; 2 pernas = 20%; sem gradação, sem diferença visual por lado). "Manqueira Leve/Severa" é nomenclatura do mod, recalibrada por cima desse cap (ver §4.1). Fora esse cap, o vanilla aplica outras penalidades de perna ferida que o Trauma 2.0 **não cobre hoje** — ver §1.1.

### 1.1 Penalidades vanilla de perna ferida não cobertas pelo mod

Levantadas durante a pesquisa técnica do item 001 e nunca incorporadas ao design. Ficam aqui como candidatas a expansão futura, não como pendência aberta:

| Penalidade vanilla | Quando ocorre | Vale considerar incorporar? |
|---|---|---|
| Dano ao correr (2 HP por perna ferida, a cada ~1-1,5s) | Só durante SPRINT — andar não machuca | Reforçaria o realismo de "fugir ferido dói", mas colide com o desmaio percentual (mais dano = mais chance de desmaiar) — precisaria de calibração conjunta |
| Dano ao pousar de pulo (3 HP por perna ferida) | Ao aterrissar um pulo | Menor impacto de jogo (pulo já é raro em combate); baixo risco de somar com Manqueira |
| Furtividade zerada (passos sempre audíveis) | Contínuo, enquanto a perna estiver ferida | Já é automático e "grátis" — não exige código nosso, só vale documentar como parte da experiência de estar ferido |
| Restrição de escalada por lado (perna esquerda bloqueia um tipo de escalada; direita, outro) | Ao tentar vault/escalar | Já funcional nativamente; não exige trabalho — só falta citar no critério de aceite pra não ser "descoberto" como bug depois |

Se algum destes virar item de backlog, o candidato natural é uma extensão do item 003 (Pernas) ou um item novo dedicado — nenhum dos quatro precisa de código do mod para o efeito em si acontecer (já é vanilla); o trabalho seria só de calibração/documentação para não conflitar com Manqueira/Cair/Desmaio.

## 2. Matriz de efeitos (estado atual, pós-010)

| Região | Condição | Sem analgésico | Com analgésico | Status |
|---|---|---|---|---|
| Perna | Zerar 1 | Manqueira Leve (p=100%) | Nada | ✅ Conforme original |
| Perna | Zerar 2 | Agachar involuntário (p=100%, one-shot) + Manqueira Severa contínua | Manqueira Leve | ✅ Conforme original |
| Perna | Quebrar 1 | Manqueira Leve | Nada | ✅ Conforme original |
| Perna | Quebrar 2 | **Cair** (prone forçado, sem dano de queda) + ciclo Janela 3s/Bloqueio 15s | Manqueira Leve | ✅ Conforme original |
| Perna | Zerar 1 + Quebrar 1 | Manqueira Severa | Manqueira Leve | ✅ Conforme original |
| Perna | Zerar 2 + Quebrar 2 | Cair + ciclo Janela/Bloqueio | Manqueira Severa | ✅ Conforme original |
| Estômago | Zerar | Agachar involuntário p=75% (re-rola a cada zerada nova) | p=25% | ✅ Conforme original |
| Tórax | Tiro que remove ≥50% da vida ATUAL (pré-tiro) + piso ≥25 dano absoluto | Desmaia p=50% | **Imune (p=0%)** | ✅ Conforme original |
| Cabeça | Tiro que remove ≥25% da vida ATUAL (pré-tiro) + piso ≥10 dano absoluto | Desmaia p=50% | Desmaia p=25% (nunca imune) | ✅ Conforme original |
| Braço | Zerar 1 | Tremor | Nada | ✅ Conforme original |
| Braço | Zerar 2 | Cancela ADS após 4s + Tremor | Tremor (visível mesmo sob analgésico) | ✅ Conforme original |
| Braço | Quebrar 1 | Tremor | Nada | ✅ Conforme original |
| Braço | Quebrar 2 | Cancela ADS após 3s + Tremor | Tremor | ✅ Conforme original |
| Braço | Zerar 1 + Quebrar 1 | Tremor | Nada | ✅ Conforme original |
| Braço | Zerar 2 + Quebrar 2 | Cancela ADS após 2s + Tremor | Tremor | ✅ Conforme original |

**Bots:** pernas (Manqueira + dip de agachar) e estômago (agachar) incluem bots com o mesmo comportamento do jogador humano. **Braços excluem bots de TUDO** (tremor, cancela-ADS, lockout) — o tremor não tem como aparecer visualmente num bot e a mira dele não é afetada por chacoalho de câmera. Desmaio inclui bots.

## 3. Configuração F12

### Motor (Seção 5)

| Nome exibido | Controla | Padrão | Faixa |
|---|---|---|---|
| Enable Trauma 2.0 | Master do rastreamento. Sem nenhum efeito ligado abaixo = zero efeito de jogo, só log. | Ligado | — |
| Include Adrenaline As Painkiller | Adrenalina/Berserk conta como analgésico. | Ligado | — |
| One-Shot Cooldown Seconds | Intervalo mínimo entre repetições do mesmo aviso involuntário (agachar/cair). | 4,0 s | 3–5 s |
| Reconciliation Polling Hz *(avançado)* | Frequência da checagem de segurança para casos raros (cirurgia total, revive Fika, cura na troca de mapa). | 2,0 Hz | 1–4 Hz |
| Verbose Engine Log *(avançado)* | Detalhe extra de log. | Desligado | — |

### Efeitos (Seção 6)

| Nome exibido | Controla | Padrão |
|---|---|---|
| Legs Effects | Manqueira Leve/Severa + agachar involuntário. | Ligado |
| Fall Cycle | Cair + ciclo Janela/Bloqueio. | Ligado |
| Arms Effects | Tremor + cancela-ADS. | Ligado |
| Stomach Effects | Agachar involuntário do estômago. | Ligado |
| Blackout 2.0 | Sub-toggle do gatilho percentual de desmaio — subordinado ao master "Sistema de Desmaio" (Seção 2 legada). | Ligado |
| Debug Test Consumer *(avançado)* | Consumidor de teste sem efeito de jogo — só valida toast/tradução. | Desligado |

### Pernas (Seção 7)

| Nome exibido | Controla | Padrão | Faixa |
|---|---|---|---|
| N1 Target Total Speed Percent | Velocidade TOTAL sentida na Manqueira Leve (já considerando a penalidade vanilla — o pior dos dois prevalece). | 80% | 50–95 |
| N2 Target Total Speed Percent | Idem para Manqueira Severa. Nunca fica mais "leve" que a Leve — clamp automático + warn de log 1×. | 55% | 30–90 |
| Block Sprint On N2 | Sprint bloqueado na Manqueira Severa mesmo sob analgésico (o vanilla libera; o mod não). | Ligado | — |
| Bot Crouch Dip Seconds *(avançado)* | Duração do "dip" de agachar do bot fora de combate. | 0,7 s | 0,3–1,5 |

### Queda (Seção 8)

| Nome exibido | Controla | Padrão | Faixa |
|---|---|---|---|
| Fall Window Seconds | Duração da Janela (de pé, Manqueira Severa, sprint sempre bloqueado). | 3 s | 1–10 |
| Fall Block Seconds | Duração do Bloqueio (não pode levantar; tentativa = som de dor, sem repetir). | 15 s | 5–60 |
| Bot Fall Hold Seconds | Tempo mínimo (X) que o bot fica deitado antes da IA poder tentar levantar de novo. | 15 s | 5–120 |

### Braços (Seção 9)

| Nome exibido | Controla | Padrão | Faixa |
|---|---|---|---|
| ADS Cancel Seconds (Zeroed x2) | Segundos mirando com 2 braços ZERADOS até cancelar. | 4 | 1–10 |
| ADS Cancel Seconds (Fractured x2) | Idem, 2 braços FRATURADOS (fratura dói mais). | 3 | 1–10 |
| ADS Cancel Seconds (Zeroed + Fractured x2) | Idem, misto. Efetivo = mínimo dos 3 timers. | 2 | 1–10 |
| Re-ADS Lockout Seconds | Bloqueio de re-mira pós-cancelamento (persiste à troca de arma). | 1,5 | 1,0–1,5 |

### Estômago (Seção 10)

| Nome exibido | Controla | Padrão | Faixa |
|---|---|---|---|
| Stomach Crouch Chance Percent | Chance de agachar ao zerar o estômago SEM analgésico. | 75% | 0–100 |
| Stomach Crouch Chance Under Painkiller Percent | Idem COM analgésico. Independente do slider acima. | 25% | 0–100 |

### Desmaio (Seção 11 + Seção 3 legada)

| Nome exibido | Controla | Padrão | Faixa |
|---|---|---|---|
| Chest Faint Percent Threshold | % da vida atual pré-tiro do tórax para rolar a chance. | 50% | 0–100 |
| Head Faint Percent Threshold | Idem cabeça. | 25% | 0–100 |
| Chest Faint Absolute Damage Floor | Piso de dano absoluto do tórax. | 25 | 0–100 |
| Head Faint Absolute Damage Floor | Piso de dano absoluto da cabeça. | 10 | 0–100 |
| Duracao Minima do Desmaio | Piso do sorteio uniforme da duração. `min > max` → valores trocados silenciosamente. | 20 s | 5–120 |
| Duracao Maxima do Desmaio | Teto do sorteio. `min == max` = duração fixa. | 20 s | 5–120 |

As chances de roll em si (50%/50%/25%/0%) são constantes fixas, não configuráveis. **Master do pipeline:** "Sistema de Desmaio" (legado) continua sendo o interruptor mestre de TUDO (gatilho + temporizador + despertar + graça + sincronização) — "Blackout 2.0" é sub-toggle, não substituto.

### Interação médica (item 010)

| Nome exibido | Controla | Padrão |
|---|---|---|
| Medic Interact Distance | Distância para as ações "Examinar (Médico)" / "Tocar no ombro" aparecerem. | 3,5 m |

### Legados removidos (item 010)

`Sistema de Pernas`, `Sistema de Braços`, `Sistema de Estômago` foram **removidos do F12** (não é mais "presente, mas sem efeito" — a key deixou de existir). `Sistema de Desmaio` é a EXCEÇÃO: continua ativo como master (ver acima).

## 4. Critérios de aceite

Cobertura mínima: 1 cenário por linha da matriz (§2) + os corners mais arriscados identificados durante a implementação. Roteiro pensado para validação manual in-game (solo primeiro, depois 2 PCs Fika — protocolo completo em [trauma-coop-test-protocol.md](./trauma-coop-test-protocol.md)).

### 4.1 Pernas — Manqueira

- Levantar com as 2 pernas zeradas NUNCA fratura nem causa dano extra — comportamento puramente reativo (fratura só vem de combate/vanilla, nunca do mod).
- Zerar 1 perna → Manqueira Leve, log confirma; tomar analgésico → nada; expirar → Manqueira Leve de novo.
- Zerar 2 pernas → agacha 1×, Manqueira Severa, sprint bloqueado mesmo com analgésico.
- Curar (própria/aliado/cirurgia) → Manqueira some em ≤1s.
- Bot zera perna → manca/dip visível ao host e a peers.
- Toggle "Legs Effects" OFF mid-raid → desfaz caps na hora, inclusive em jogador DOWNED.

### 4.2 Pernas — Ciclo de Queda

- Quebrar 2 pernas sem analgésico → cai (prone, sem dano de queda).
- Janela ~3s de pé → anda com Manqueira Severa, sprint bloqueado → re-cai sozinho ao expirar.
- Tentar levantar durante o Bloqueio (15s) → som de dor, negado, sem repetir o som a cada tentativa.
- Liberação → levanta devagar com som leve; ficar deitado indefinidamente também é válido.
- Analgésico → levanta livre, Manqueira conforme severidade restante.
- Desmaiar em qualquer fase do ciclo → ao acordar, sempre no Bloqueio (nunca retoma a fase anterior).
- Bot cai/re-cai respeitando X segundos configurável; controle não trava se a IA decidir levantar de novo com a fratura persistindo.
- Bosses e bots-líder NÃO entram no ciclo de queda (não são derrubados/deitados) — só recebem Manqueira, se aplicável.
- Extração funciona prone; transit/fim de raid reseta o ciclo sem resíduo.
- Toggle "Fall Cycle" OFF mid-raid → solta tudo na hora; cap da Janela some (é território exclusivo deste efeito, não da Manqueira).
- Toggle "Fall Cycle" OFF, "Legs Effects" ON, jogador entra na condição "Cair" → o toast de 1ª ocorrência ainda pode aparecer mesmo sem o ciclo executar (o registro de aviso é por REGIÃO — pernas —, não por linha exata da matriz).
- Mudar "Fall Window/Block Seconds" no F12 no meio de um ciclo em andamento → não afeta o ciclo corrente, só o próximo a começar.
- Mesmo evento de dano zera as 2 pernas E causa desmaio (ex.: explosão) → desmaio vence; ciclo de queda é absorvido; ao acordar, entra direto no Bloqueio.
- Prone recusado por falta de espaço físico (não é escada/BTR/vault) → fallback agachado; ficar de pé negado, mover-se agachado livre, até o prone ser conseguido na re-tentativa.

### 4.3 Braços — Tremor e ADS

- Zerar 1 braço → tremor visível; analgésico remove.
- Zerar 2 braços → tremor visível MESMO sob analgésico; segurar mira 4s → cancela sozinho.
- Quebrar 2 braços → cancela em 3s; misto (1 zerado + 1 fraturado) → cancela em 2s (o menor dos três).
- Re-mirar durante o lockout (1-1,5s) → bloqueado + grito de dor (1× por bloqueio); trocar de arma durante o lockout NÃO libera.
- Desmaiar durante o lockout → bloqueio continua, mas sem grito de dor.
- Bot com braços feridos → SEM tremor, SEM cancela-ADS, SEM lockout (confirma exclusão total).

### 4.4 Estômago

- Zerar estômago sem analgésico → ~75% de chance de agachar (rodar ~20 vezes, contar no log — esperar 11-19 sucessos).
- Mesma zerada com analgésico ativo → ~25% de chance (esperar 1-9 em 20).
- Curar e zerar de novo na mesma raid → novo roll (log mostra 2 rolls distintos).
- Zerar estômago enquanto o Ciclo de Queda está ativo → absorvido, sem segundo efeito de pose.
- Estômago + pernas zerando quase juntos → 1 agachar só (log mostra a supressão do segundo).
- Roll de estômago no mesmo frame de um desmaio/downed → NOOP, nenhuma pose forçada sobre jogador inconsciente.
- Toggle "Stomach Effects" OFF, "Legs Effects" ON → zerar estômago não agacha nem rola; inverso também.

### 4.5 Desmaio

- Tórax removendo ≥50% da vida atual + ≥25 de dano absoluto, sem analgésico → rola 50%.
- Mesmo hit com analgésico ativo → NUNCA desmaia (tórax imune).
- Cabeça removendo ≥25% + ≥10 de dano, sem analgésico → rola 50%; com analgésico → rola 25% (nunca imune).
- Hit que atinge o percentual mas fica abaixo do piso absoluto → não rola nada.
- Rajada de espingarda → cada pellet avaliado contra a vida imediatamente anterior a ele (sem soma).
- Hit simultâneo tórax+cabeça (ex.: explosão) → nunca dois desmaios/duração sobreposta.
- Toggle "Blackout 2.0" OFF, "Sistema de Desmaio" ON → nenhum desmaio por dano dispara (sem fallback ao limiar fixo antigo).
- "Sistema de Desmaio" OFF → desliga TUDO.
- Configurar min=5/max=60, causar ~10 desmaios → durações espalhadas (não concentradas numa ponta).
- min == max → duração sempre exatamente esse valor.
- min > max configurado → normalizado sem erro.
- Atualizar de uma versão anterior com "Duração do Desmaio" customizada (inclusive fracionária, ex. 47.5) → Min e Max nascem iguais ao valor antigo, mesmo com o Windows em pt-BR.
- Mudar min/max no F12 durante um desmaio em curso → não afeta esse desmaio, só o próximo.

### 4.6 Interação médica e textos (item 010)

- Distância de interação do médico é 3,5 m (antes 5 m) — "Examinar (Médico)"/"Tocar no ombro" só aparecem dentro desse raio.
- Nenhuma sonda de debug ([DEBUG-ICM]) aparece no log em jogo normal.
- Todo texto do fluxo médico (toasts, HUD, ActionPanel, motivos de recusa) aparece em inglês por padrão e em português quando o idioma do jogo é português — inclusive os que antes eram fixos em PT.
- Legados `Sistema de Pernas/Braços/Estômago` não aparecem mais no F12.

### 4.7 Corners transversais (multi-item)

- Fika/coop com 2 PCs: peer vê Manqueira/tremor/pose/queda pelo sync nativo; bots do host aparecem coerentes ao client; nenhum efeito aplicado por espelho.
- Religar o mod inteiro no meio da raid com múltiplas condições já ativas (pernas + braços + estômago zerados) → todos os estados se re-estabelecem sem toast/one-shot duplicado.
- Raid1 → extração → Raid2: nenhum estado, cooldown ou toast "já visto" sobrevive entre raids.
- Hideout: nenhum log de tracking do motor aparece fora de raid.
- Toast de 1ª ocorrência aparece 1×/estado/raid, em EN por padrão e PT quando o idioma do jogo é português.
- **Nenhum item do overhaul (002-010) foi validado in-game de fato ainda** — só validação estática/spike + code-review. Este roteiro (§4) é o que falta rodar.

## 5. Apêndice técnico

### 5.1 Glossário de decisões

Toda decisão referenciada no texto abaixo, num só lugar. "D-N" vem do documento de design original ([trauma-matrix.md](./trauma-matrix.md)); "Decisão N" (sem D) vem das rodadas de validação de backlog/FMEA do mesmo documento.

| Código | Resumo em 1 linha |
|---|---|
| D1 | Ranking de severidade em combos mistos: Cair+ciclo > Agachar+Severa > Manqueira Severa > Manqueira Leve > Nada |
| D2 | Conflito de POSE entre regiões resolve pela mais severa (prone vence agachar) |
| D3 | Desmaio pausa o ciclo de queda; wake sempre reinicia no Bloqueio (refinado — ver §5.3) |
| D4 | Zerar+Quebrar no MESMO membro conta como Z1+Q1 (por condição, não por membro) |
| D5 | Dano do desmaio = efetivo pós-armadura, comparado à vida atual pré-tiro |
| D6 | Tipos de dano que disparam desmaio: Bullet/Explosion/Sniper/Landmine/GrenadeFragment |
| D7 | Guards de contexto: agachar/cair não disparam em escada/corda/BTR/vault (adiam) |
| D8 | Roll do estômago usa o analgésico do INSTANTE da zerada (sem re-roll depois) |
| D9 | Braços em bots: tremor cosmético previsto — **refutado na prática, ver §5.3** (exclusão total) |
| D10 | Substituição incremental: cada item desliga o sistema antigo na própria entrega |
| D11 | Tremor com lifecycle próprio do mod, não depende do tremor-por-dor vanilla |
| D12 | Composição de velocidade é por MÍNIMO do dicionário, não multiplicativa (corrigido — matriz original dizia "multiplicativa") |
| D13 | Cancela-ADS pelo caminho vanilla, testado contra RecoilRework/FOV-Fix |
| D14 | Pontos seguros de interferência em SAIN/ORBIT mapeados no spike 001 |
| D15 | UNTAR segue as mesmas regras de trauma (são bots) |
| D16 | Autoridade dono-only: motor só avalia quem o processo possui; espelhos nunca aplicam efeito |
| D17 | Estado reverte também por cura REMOTA (rede), não só cura própria |
| D18 | Cair sem dano de queda; corrigido — não existe "dano vanilla de andar" (só vault/escalada) |
| D19 | Todo roll de probabilidade é logado, para balanceamento |
| D20 | Suíte de compatibilidade (item 009): SAIN, ORBIT, UNTAR, CustomClasses-Tank, RecoilRework, FOV-Fix, BringBackConcussion, Visceral Combat, tarkin-ladders — ver [trauma-compat-suite.md](./trauma-compat-suite.md) |
| Decisão 14 | Expirar analgésico reavalia TUDO na hora, inclusive disparando one-shots pendentes |
| Decisão 15 | Pisos absolutos de dano no desmaio (tórax 25, cabeça 10), sem agregação de pellets |
| Decisão 16 | Bots no ciclo de queda: interferência cirúrgica, devolve controle à IA a cada vez; X configurável |
| Decisão 17 | Lockout de re-ADS pós-cancelamento: 1-1,5s configurável |
| Decisão 18 | Manqueira Leve/Severa = velocidade TOTAL experienciada (por cima do cap vanilla, não somada) |
| Decisão 19 | Anti-thrash: mesmo one-shot não repete em menos de 3-5s |
| Decisão 20 | Feedback: som de dor + toast discreto na 1ª ocorrência |
| Decisão 21 | Injeção legacy de fratura/dano ao levantar (30%/15 dano) aposentada — matriz 100% reativa |
| Decisão 22 | i18n: textos nascem em inglês, traduzidos para PT quando o idioma do jogo é português |

### 5.2 Decisões e defaults por item

**Motor (item 002):** estados contínuos revertem ao curar (D17); combos resolvem pela linha mais severa (D1); toast de 1ª ocorrência só aparece com consumidor ativo para a região (registro é por REGIÃO, não por linha — ver corner do "Cair" em §4.2); motor fica inerte em hideout/loading; bots com Painkiller permanente (bosses) ficam estáveis na coluna "com analgésico" sem expirar (não é bug).

**Pernas — Manqueira (item 003):** calibrada sobre a penalidade vanilla via `min()`, nunca soma (D12, decisão 18); agachar é one-shot, não trava pose; sprint bloqueado na Severa mesmo sob analgésico (decisão do mod além da paridade vanilla); guards de contexto adiam em vez de cancelar (D7); handoff bidirecional com o item 004 na linha "Cair".

**Pernas — Ciclo de Queda (item 004):** 3 fases (Janela → Bloqueio → Liberação); Janela só conta a partir do "de pé efetivo" (fim da rampa de levantar); mudar timers no F12 nunca rebase um ciclo em andamento; fallback agachado se o prone for recusado por falta de espaço; wake sempre reinicia no Bloqueio, nunca retoma a fase anterior (D3 refinado); establishing (spawn ferido, religar) sempre começa na Janela; bot: hold mínimo de X segundos, devolvido à camada de decisão da IA (SAIN/ORBIT/outra — decisão 16 refinada); bosses e bots-líder fora do ciclo inteiro; colisão Cair+Desmaio no mesmo evento → desmaio vence.

**Braços — Tremor e ADS (item 005):** tremor contínuo por estado, remove com analgésico em 1-braço, continua visível em 2-braços; cancela-ADS reinicia sempre que a severidade muda mid-mira; lockout é do jogador, não da arma; degradação graciosa — se o tremor falhar por mudança de versão do jogo, vira no-op silencioso e o resto continua funcionando.

**Braços/bots (D9 refutado):** a matriz original previa tremor cosmético em bots; a implementação constatou que não há canal de exibição real para tremor num bot — bots ficaram excluídos de TUDO (tremor, cancela-ADS, lockout). Única mudança de comportamento observável vs. a matriz original.

**Estômago (item 006):** re-rola a cada zerada nova; analgésico do instante (latch); sliders SEM trava entre si (diferente do `min()` das pernas — aqui inverter é permitido); cooldown compartilhado com o agachar de pernas na mesma janela anti-thrash; roll durante desmaio/downed vira NOOP.

**Desmaio percentual (item 007):** percentual da vida atual pré-tiro (não pós-tiro, não vida máxima) + piso absoluto independente; sem agregação de pellets; tórax imune total sob analgésico, cabeça só reduz a chance pela metade; "Sistema de Desmaio" continua sendo o master de todo o pipeline (decisão deliberada, para não mexer num pipeline com histórico de bugs de timing — P-2.13/14/15); desmaio é EVENTO (roll único), não reverte quando o analgésico expira depois; dano considerado é sempre pós-armadura (D5); só 5 tipos de dano disparam (D6).

**Duração do desmaio (item 008):** sorteio uniforme min/max a cada novo desmaio, gravado uma única vez; `min > max` normalizado silenciosamente; migração do valor legado do usuário feita por CÓPIA (não descarte) — era um valor real ajustado pelo jogador, não um placeholder.

**Hardening coop (item 009):** boilerplate de detecção de troca de raid/toggle, antes duplicado em 4 consumidores, extraído para um helper compartilhado (`TraumaConsumerLifecycle`); voz dupla-fonte entre o Ciclo de Queda (004) e Tremor/ADS (005) — a colisão (dois emissores de grito de dor no mesmo instante) foi avaliada e **aceita sem arbitragem** (nenhum bug conhecido, o custo de reconciliar não se justificou); suíte de compatibilidade formalizada em [trauma-compat-suite.md](./trauma-compat-suite.md); protocolo de teste 2 PCs entregue em [trauma-coop-test-protocol.md](./trauma-coop-test-protocol.md) — a EXECUÇÃO do roteiro segue pendente (validação manual).

**Migração de configs + release (item 010):** `Sistema de Pernas/Braços/Estômago` removidos do F12 por completo (não só desativados); distância de interação do médico 5m → 3,5m; sondas de debug `[DEBUG-ICM]` removidas; todo texto do fluxo médico (não só do motor de trauma) migrado para i18n EN/PT via classe `MedicLocale` dedicada; script de empacotamento de release criado e testado.

### 5.3 Correções de redação sobre a matriz original

- **D12** (composição de velocidade): a matriz original dizia "multiplicativa"; a implementação corrigiu para "por MÍNIMO do dicionário de limites de velocidade".
- **D18** (dano de queda na Janela): a matriz original dizia "toma o dano vanilla de andar"; não existe dano vanilla por só andar — o dano aceito na prática é de vault/escalada.
- **Decisão 16** (bots no ciclo de queda): "devolver ao SAIN" foi refinado para "devolver à camada de decisão da IA" (pode ser SAIN, ORBIT ou outra).
- **D9** (tremor cosmético em bots): refutada — ver §5.2, "Braços/bots".
- **Decisão 3/D3** (desmaio pausa o ciclo de queda): a redação original sugere retomar a MESMA fase; a implementação decidiu que o wake sempre reinicia no Bloqueio — acordar não é o mesmo que estar pronto para ficar de pé.

### 5.4 Débito técnico e pendências vivas

- **Roteiro de teste ainda não executado** — nenhum item do overhaul (002-010) foi validado in-game de fato; §4 é o roteiro pendente.
- **`trauma-matrix.md` original não foi retrofitado** com a correção do D12 (composição por mínimo, não multiplicativa) — só este documento reflete a correção.
- **Padrões de migração de config divergentes**: placeholder inerte → rename-at-delivery com descarte (003-007); valor real do usuário → migração por cópia culture-invariant (008). O código de busca de config órfã tinha 6 cópias quase idênticas desse idioma antes do 010 fazer a limpeza final de key.
- **Chave de dedup da fila de one-shots adiados** evoluiu de `(jogador, tipo)` para `(jogador, tipo, região)` no item 006 — sem isso, um agachar de pernas e um de estômago adiados colidiam numa única entrada.

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-19 | Guilherme (com Claude) | Criação — consolida `trauma-matrix.md` + toda decisão/premissa adotada durante os itens 002-008. |
| 2026-07-19 | Guilherme (com Claude) | Verificação de completude independente: 9 premissas de prioridade alta incorporadas (maioria do item 004). |
| 2026-07-25 | Guilherme (com Claude) | Reescrita completa: (1) atualização com os itens 009 (hardening coop) e 010 (migração de configs + release), ambos entregues — remoção real dos legados do F12, distância de interação 3,5m, i18n completo do fluxo médico, helper compartilhado de lifecycle, voz dupla-fonte aceita sem arbitragem; (2) reorganização em produto (§1-4, critérios de aceite e configuração, sem jargão de código) + apêndice técnico (§5, decisões/histórico); (3) renomeação de "N1/N2" para "Manqueira Leve/Severa" com nota explícita de que não é terminologia vanilla; (4) glossário único de códigos de decisão (D1-D20, Decisão 1-22); (5) nova seção §1.1 listando penalidades vanilla de perna ferida não cobertas pelo mod (dano ao correr, dano ao pousar, furtividade, escalada por lado), como candidatas a expansão futura, não pendência aberta. |
