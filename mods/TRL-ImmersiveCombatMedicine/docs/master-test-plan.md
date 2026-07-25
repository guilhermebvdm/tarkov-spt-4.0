# Plano de Teste Mestre — Trauma 2.0 (v1.10.0)

> **Data:** 2026-07-25<br>
> **Status:** 🟢 Vivo<br>
> **Responsáveis:** Guilherme<br>
> **Referências:** [trauma-behavior-matrix.md](./trauma-behavior-matrix.md), [trauma-coop-test-protocol.md](./trauma-coop-test-protocol.md), [trauma-compat-suite.md](./trauma-compat-suite.md)<br>

---

## Escopo

Este documento consolida, resequencia e numera de forma única **todo** cenário de teste do overhaul Trauma 2.0 (itens de backlog 002-010, versão **v1.10.0**) que ainda não foi validado in-game (pendência [P-4.4] da memória do mod). Substitui a necessidade de navegar entre `trauma-behavior-matrix.md` §5 (44 cenários originais) e `trauma-coop-test-protocol.md` (B1/B2) como duas listas separadas.

**Fora de escopo deste plano:** o sistema de cura Band-Aid/torniquete "clássico" (pré-overhaul) — já validado em sessões anteriores (memória do mod, pendências P-2.1 a P-2.15, a maioria VALIDADA). Este plano assume que a cura básica já funciona e testa apenas o que o Trauma 2.0 e o item 010 adicionaram ou mudaram por cima dela.

**Revisão adversarial aplicada (2026-07-25):** a 1ª versão deste plano (baseada apenas nos 2 documentos-fonte) passou por uma releitura cética de todas as 9 specs funcionais (002-010) + a matriz de comportamento completa. Isso encontrou e corrigiu: (a) 2 cenários que a consolidação original tinha **dropado silenciosamente** da matriz (fila de adiados multi-região, roll de estômago durante desmaio); (b) 1 cenário de risco real já documentado em memória do mod que também tinha sido dropado (migração de config fracionária em SO pt-BR — risco de corrupção por cultura numérica); (c) ausência total de testes de "spawn ferido" (persistência de dano entre raids) em 5 sistemas diferentes; (d) o único dos 4 consumidores de estado contínuo sem teste de toggle-OFF dedicado (Braços/005); (e) gaps de sequenciamento entre fases (nenhuma instrução de "curar antes de avançar", risco de um teste contaminar o próximo); (f) 3 cenários novos do item 010 que a spec pedia mas nenhum documento cobria (distância em ações secundárias, config órfã no `.cfg`, geração do zip de release); (g) cenários coop ausentes que exigem 2 peers (reconexão Fika, cura remota revertendo efeito, DOWNED durante o ciclo de queda). Itens de baixíssima testabilidade manual (race conditions de frame único, divisão por zero, idioma indisponível no boot) foram deliberadamente deixados de fora — ver nota em cada fase onde se aplica.

**Convenção de numeração:** `S<fase>.<n>` = teste **SOLO** (1 jogador, host ou raid offline-like); `C<fase>.<n>` = teste **COOP** (exige 2 PCs Fika, ou 1 PC + 1 headless onde indicado). Cada teste tem uma caixa de seleção `[ ]` — é o que vira checklist no cartão do Trello.

## Pré-requisitos (fazer uma vez, antes de qualquer teste)

- [ ] **P0.1** — Confirmar a versão instalada: painel F12 → BepInEx → `TRLImmersiveCombatMedicine` deve mostrar **1.10.0**. Se não bater, reinstalar/sincronizar antes de continuar.
- [ ] **P0.2** — Abrir `LogOutput.log` uma vez após o boot e confirmar que **nenhuma linha contém `[DEBUG-ICM]`**.
- [ ] **P0.3** — No F12, seção "6. Trauma 2.0 (Consumidores)", confirmar que os 5 toggles reais (Legs/Fall/Arms/Stomach/Blackout 2.0 Effects) estão **Ligados** e ativar **"Debug Test Consumer"** (avançado) — destrava o toast de 1ª ocorrência nas 3 regiões sem efeito de gameplay, útil para S7.
- [ ] **P0.4** — Confirmar que a seção "2. Mecanicas (Trauma)" do F12 **não lista mais** `Sistema de Pernas`, `Sistema de Braços` nem `Sistema de Estomago` — só `Sistema de Desmaio` deve restar.
- [ ] **P0.5** — Confirmar `Medic Interact Distance` com default **3,5** e tooltip sem menção a "testes".
- [ ] **P0.6 (setup para S7.7)** — Se possível, guardar uma cópia do `.cfg` de uma instalação **anterior à v1.10.0** (com as 3 keys legadas ainda gravadas) para o teste de config órfã em S7.7. Se não tiver uma cópia real, pode simular: editar manualmente o `.cfg` atual, adicionando de volta as 3 linhas `Sistema de Pernas`/`Sistema de Braços`/`Sistema de Estomago` sob a seção `[2. Mecanicas (Trauma)]`, antes do próximo boot.

## Fase S0 — Verificação de build (release, item 010)

> Não exige raid — é checagem de artefato de build. Pode ser feita pelo desenvolvedor antes de distribuir a versão para o teste de produto propriamente dito.

- [ ] **S0.1** — Rodar `bash mods/TRL-ImmersiveCombatMedicine/scripts/package-release.sh` → confirmar que gera `dist/trl-icm-release-v1.10.0.zip` sem erro.
- [ ] **S0.2** — Abrir o zip e confirmar que contém **só** `BepInEx/plugins/TRL-ImmersiveCombatMedicine/{TRLImmersiveCombatMedicine.dll,.pdb}` — sem `.cfg`, sem `LogOutput.log`, sem nenhum artefato de usuário.

---

## Parte A — Testes SOLO

Ordem sugerida: sistemas mais simples/isolados primeiro, sistemas com mais estado por último. **Regra geral de higiene entre fases:** ao terminar uma fase, cure a condição testada (cirurgia, médico, ou tala/torniquete conforme aplicável) antes de avançar para a próxima — várias fases abaixo dependem de começar com o personagem saudável para o teste fazer sentido (ex.: testar "Quebrar 2 pernas" em S2 só é válido se as pernas não estiverem ainda Zeradas de um teste anterior de S1; testar o desmaio percentual em S5 exige posicionamento controlado do tiro, difícil de calibrar se o personagem já estiver mancando/tremendo de fases anteriores).

### Fase S1 — Pernas: mancar (item 003)

- [ ] **S1.1** — Zerar 1 perna → manca N1 (log confirma); tomar analgésico → manqueira some; expirar → manca N1 de novo.
- [ ] **S1.2** — Zerar 2 pernas → agacha 1× (one-shot, não trava pose), manca N2, sprint bloqueado mesmo com analgésico ativo.
- [ ] **S1.3** — Curar (própria, por aliado, ou cirurgia) → manqueira some em ≤1s.
- [ ] **S1.4** — Bot com 1 perna zerada → manca; curado (host ou médico client via cura coop) → normaliza. Bot com 2 pernas zeradas → dip de agachar 1×.
- [ ] **S1.5** — Toggle "Legs Effects" OFF no meio da raid → desfaz caps na hora, inclusive com o próprio jogador desmaiado/downed.
- [ ] **S1.6 (decisão 21)** — Levantar com as 2 pernas zeradas **nunca** fratura nem causa dano extra (a injeção legada foi aposentada).
- [ ] **S1.7 (novo)** — Com 2 pernas zeradas sob analgésico (manqueira N1, sem agachar): deixar o analgésico expirar → o estado re-escala para N2 **e dispara um NOVO agachar involuntário** (respeitando o cooldown anti-thrash se disparado recentemente) — não é só a manqueira que re-escala, o one-shot também pode disparar de novo na reavaliação.
- [ ] **S1.8 (novo)** — Quebrar as 2 pernas (não zerar) **com analgésico ativo** → manca N1, sem cair; deixar o analgésico expirar → o jogador **cai pela primeira vez** (handoff para o ciclo de queda do item 004, testado na Fase S2) — diferente de S2.5, que testa o caminho oposto (analgésico tomado com o ciclo já em andamento).
- [ ] **S1.9 (novo, corner D7)** — Zerar a 2ª perna enquanto em contexto protegido (subindo escada, pendurado em corda, dentro de BTR, ou durante um vault) → o agachar involuntário fica **adiado**, não dispara na hora; ao sair do contexto, dispara. Se a condição for curada ANTES de sair do contexto protegido, o agachar adiado é **cancelado silenciosamente sem consumir o cooldown** (confirmar tentando de novo logo em seguida — deve funcionar normalmente).
- [ ] **S1.10 (novo, opcional)** — Se `Skills Extended` estiver instalado: confirmar que os multiplicadores de velocidade compõem corretamente com o cap do 003 (mesma lógica de composição por mínimo já validada com CustomClasses-Tank em S9.5).

### Fase S1b — Persistência entre raids (spawn ferido) — itens 002-006

> Cobre um gap comum às 5 specs: nenhum cenário do plano original testava entrar numa raid **já ferido** (dano persistido de uma raid anterior sem cura) — só "curar/religar mid-raid". SPT persiste o dano do personagem entre raids; isso precisa ser testado uma vez por sistema.

- [ ] **S1b.1** — Zerar 1 perna, **não curar**, extrair → entrar na raid seguinte: log confirma o estado estabelecido (manqueira já presente desde o spawn) **sem** toast nem one-shot de entrada (é reconhecimento de estado existente, não uma transição nova).
- [ ] **S1b.2** — Quebrar as 2 pernas sem analgésico, entrar no ciclo de queda, **não curar**, extrair prone → raid seguinte: personagem estabelece diretamente na fase **Janela** (como se tivesse acabado de levantar), sem o "drama" de um derrubar abrupto na entrada.
- [ ] **S1b.3** — Zerar/quebrar um braço, não curar, extrair → raid seguinte: tremor estabelece sem toast nem voz de dor.
- [ ] **S1b.4** — Zerar o estômago, não curar, extrair → raid seguinte: o estado "zerado" é reconhecido, mas **não rola, não agacha e não toasta de novo** (só rola uma vez na transição real de entrada, não no reconhecimento de estado já existente).
- [ ] **S1b.5** — Com os 5 consumidores (Legs/Fall/Arms/Stomach/Blackout 2.0) **todos desligados simultaneamente**, causar qualquer condição de trauma → confirma gameplay estritamente vanilla (fora a injeção legada removida, decisão 21) e zero toast — fecha o CA do item 002 sobre "consumidor nenhum ligado = gameplay idêntico".

### Fase S2 — Pernas: ciclo de queda (item 004)

> **Pré-condição desta fase:** começar com as pernas saudáveis (curar via cirurgia/médico antes, se vier de S1/S1b com uma condição residual).

- [ ] **S2.1** — Quebrar as 2 pernas sem analgésico → cai (prone forçado, sem dano de queda).
- [ ] **S2.2** — Janela (~3s de pé, N2, sprint bloqueado) → re-cai sozinho ao expirar.
- [ ] **S2.3** — Tentar levantar durante o Bloqueio (15s) → som de dor + negado, sem repetir o som a cada tentativa.
- [ ] **S2.4** — Liberação → levanta devagar com som leve; ficar deitado indefinidamente também é válido.
- [ ] **S2.5** — Tomar analgésico durante o ciclo já em andamento → levanta livre, manca conforme severidade restante (N1/N2).
- [ ] **S2.6** — Desmaiar em qualquer fase do ciclo → ao acordar, **sempre** reinicia no Bloqueio (nunca retoma a fase anterior).
- [ ] **S2.7** — Bot cai/re-cai respeitando o hold configurável (`Bot Fall Hold Seconds`); controle não trava se a IA decidir levantar de novo com a fratura persistindo.
- [ ] **S2.8** — Extração funciona estando prone; transit/fim de raid reseta o ciclo sem resíduo na raid seguinte (comparar com S1b.2, que é o caminho de establishing, não de reset).
- [ ] **S2.9** — Toggle "Fall Cycle" OFF no meio da raid → solta tudo na hora; o cap de velocidade N2 da Janela some (território exclusivo deste item).
- [ ] **S2.10** — Toggle "Fall Cycle" OFF + "Legs Effects" ON, jogador entra na condição "Cair" → o toast de 1ª ocorrência ainda pode aparecer (registro por região, não por linha) — **comportamento esperado, não é bug**.
- [ ] **S2.11** — Mudar "Fall Window/Block Seconds" no F12 com um ciclo em andamento → não afeta o ciclo corrente, só o próximo a começar.
- [ ] **S2.12** — Mesmo evento de dano zera as 2 pernas E causa desmaio (ex.: explosão) → o desmaio VENCE; o ciclo é absorvido (cooldown devolvido); ao acordar, entra direto no Bloqueio.
- [ ] **S2.13** — Prone recusado por falta de espaço físico (não é escada/BTR/vault) → fallback agachado; ficar de pé negado, mover-se agachado livre, até o prone ser conseguido na re-tentativa.
- [ ] **S2.14** — Bot-boss ou seguidor especial com as 2 pernas quebradas → **não** entra no ciclo de queda, só recebe manqueira se aplicável.
- [ ] **S2.15 (novo)** — Curar 1 das 2 fraturas (tala ou cirurgia, própria ou por médico remoto) com o ciclo ativo → o ciclo **encerra em ≤1s** (a condição "Cair" só existe com as duas pernas quebradas simultaneamente).
- [ ] **S2.16 (novo, corner D7)** — Derrubar disparado enquanto em contexto protegido (escada/corda/BTR/vault) → adiado até sair do contexto; se curado antes de sair, cancelado sem consumir cooldown (mesmo padrão de S1.9, aplicado ao derrubar em vez do agachar).
- [ ] **S2.17 (novo)** — Alternar analgésico rapidamente (tomar → deixar expirar → tomar de novo) em sucessão rápida → o anti-thrash do motor impede um re-derrubar em menos de 3-5s mesmo que a condição matemática permita.

> **Antes de avançar:** curar as 2 pernas (cirurgia/médico) para não contaminar as fases S3/S4/S5 seguintes com a condição "Cair" ainda ativa (velocidade limitada / prone periódico atrapalha o posicionamento controlado exigido por S5).

### Fase S3 — Braços: tremor e cancela-ADS (item 005)

- [ ] **S3.1** — Zerar 1 braço → tremor visível; tomar analgésico → tremor some; deixar expirar → tremor volta; curar o braço → tremor some em ≤1s.
- [ ] **S3.2** — Zerar 2 braços → tremor visível **mesmo sob analgésico**; segurar mira 4s → ADS cancela sozinho; soltar a mira antes dos 4s e re-mirar → o timer reseta do zero (não é cumulativo).
- [ ] **S3.3** — Quebrar 2 braços → cancela em 3s; misto (1 zerado + 1 fraturado) → cancela em 2s (o menor dos três timers).
- [ ] **S3.4** — Re-mirar durante o lockout (1-1,5s) → bloqueado + grito de dor (1×/bloqueio); trocar de arma durante o lockout **não** libera a mira. Testar nos dois modos de mira (hold e toggle-aim, se configurado) — mesmo comportamento nos dois.
- [ ] **S3.5** — Desmaiar/ficar incapacitado durante o **lockout pós-cancelamento** → bloqueio continua, mas **sem** grito de dor (suprimido).
- [ ] **S3.6** — Bot com braços feridos → **sem** tremor, **sem** cancela-ADS, **sem** lockout (confirma a exclusão total de bots).
- [ ] **S3.7 (novo — fecha o gap crítico do item 009/A4)** — Toggle "Arms Effects" OFF no meio da raid com tremor/lockout ativos → tremor remove e lockout cancela na hora; religar o toggle → o estado é reestabelecido a partir do snapshot atual, **sem** toast duplicado (mesmo padrão de toggle já coberto para Legs/Fall/Stomach — Arms era o único dos 4 consumidores sem este teste).
- [ ] **S3.8 (novo)** — Mirando com 1 braço zerado há ~3s (contando pro cancelamento de 4s), quebrar o 2º braço no meio da mira → o timer **reinicia do zero** na nova severidade (cancela ~2s DEPOIS da mudança, não 1s). Com o timer correndo, tomar analgésico → o timer é **descartado** (cancelamento não ocorre).
- [ ] **S3.9 (novo)** — Desmaiar/ficar incapacitado **durante a contagem** do timer de ADS (antes do cancelamento acontecer, diferente de S3.5 que testa depois) → a mira cai e o timer reseta como se tivesse soltado a mira normalmente.
- [ ] **S3.10 (novo, se aplicável)** — Se `SPTRecoilRework` e `Fontaine-FOVFix` estiverem instalados: repetir 3 ciclos seguidos de mira-sustentada→cancelamento e confirmar que o FOV/zoom nunca fica "preso" num estado intermediário.

> **Antes de avançar:** curar os braços para não contaminar S4/S5.

### Fase S4 — Estômago: agachar probabilístico (item 006)

- [ ] **S4.1** — Zerar estômago sem analgésico → ~75% de chance de agachar (rodar ~20 vezes, contar no log — esperar 11-19 sucessos).
- [ ] **S4.2** — Mesma zerada com analgésico ativo → ~25% de chance (esperar 1-9 sucessos em 20).
- [ ] **S4.3** — Curar e zerar de novo na mesma raid → novo roll independente (log mostra 2 rolls distintos).
- [ ] **S4.4** — Zerar estômago enquanto o ciclo de queda (004) está ativo (quebrar as 2 pernas primeiro, SEM curar, depois zerar o estômago) → absorvido, sem segundo efeito de pose.
- [ ] **S4.5** — Estômago e pernas zerando quase juntos (mesma rajada ou tiros muito próximos) → 1 agachar só (log mostra a supressão do segundo, cooldown compartilhado).
- [ ] **S4.6** — Toggle "Stomach Effects" OFF, "Legs Effects" ON → zerar estômago não agacha nem rola; inverso (Stomach ON, Legs OFF) também isolado corretamente.
- [ ] **S4.7 (novo)** — Configurar `Stomach Crouch Chance Percent` = 0% → nunca agacha, mesmo repetindo várias zeradas (rolls seguem logados). Configurar = 100% → sempre agacha.
- [ ] **S4.8 (novo)** — Bot dono (host/headless) zera o estômago → rola e dipa igual ao jogador humano; se o bot estiver em hold do ciclo de queda (004), o roll é absorvido do mesmo jeito que no jogador (S4.4).
- [ ] **S4.9 (novo, recuperado do cenário 25b da matriz original — não estava no plano consolidado)** — Um agachar de estômago adiado (D7, ex. numa escada) coexistindo com um agachar de PERNAS também adiado ao mesmo tempo → as duas intenções coexistem na fila sem interferência cruzada; curar uma região não cancela a intenção pendente da outra.
- [ ] **S4.10 (novo, recuperado do cenário 25c)** — Roll de estômago acontecendo no mesmo frame de um desmaio/downed → vira NOOP, nenhuma pose forçada sobre um jogador inconsciente (com devolução do cooldown).
- [ ] **S4.11 (novo)** — Com um agachar do ESTÔMAGO adiado (D7) E um agachar de PERNAS também adiado ao mesmo tempo: desligar "Stomach Effects" mid-raid → só o adiado do ESTÔMAGO é cancelado com refund; o adiado de PERNAS continua na fila intacto.

> **Antes de avançar:** curar o estômago (some sozinho ao curar a região) e confirmar as pernas saudáveis para S5.

### Fase S5 — Desmaio: gatilho percentual (item 007)

> **Pré-condição:** pernas/braços/estômago saudáveis — o posicionamento controlado do tiro (necessário para calibrar o dano exato) fica mais difícil se o personagem já estiver mancando/tremendo/agachando de fases anteriores.

- [ ] **S5.1** — Tórax removendo ≥50% da vida atual + piso ≥25 de dano absoluto, sem analgésico → rola 50%.
- [ ] **S5.2** — Mesmo hit no tórax com analgésico ativo → **nunca** desmaia (imune).
- [ ] **S5.3** — Cabeça removendo ≥25% + piso ≥10 de dano, sem analgésico → rola 50%; com analgésico → rola 25% (nunca imune).
- [ ] **S5.4** — Hit que atinge o percentual mas fica abaixo do piso absoluto → não rola nada (log confirma "piso", não "percentual").
- [ ] **S5.5** — Rajada de espingarda → cada pellet avaliado contra a vida imediatamente anterior a ele (sem soma de pellets).
- [ ] **S5.6** — Toggle "Blackout 2.0" OFF, "Sistema de Desmaio" ON → nenhum desmaio por dano dispara (sem fallback ao limiar fixo legado).
- [ ] **S5.7** — "Sistema de Desmaio" OFF → desliga TUDO (gatilho, temporizador, despertar, sync) — confirma que essa key legada segue sendo o master real.
- [ ] **S5.8 (novo)** — Aplicar o mesmo teste de S5.1/S5.3 num **bot** como alvo (tórax/cabeça) → rola conforme os mesmos percentuais/pisos do jogador (comportamento genérico, sem gate especial de headless neste item).

### Fase S6 — Desmaio: duração aleatória (item 008)

- [ ] **S6.1** — Configurar min=5/max=60, causar ~10 desmaios → durações espalhadas no log (não concentradas numa ponta).
- [ ] **S6.2** — min == max → duração sempre exatamente esse valor.
- [ ] **S6.3** — min > max configurado → normalizado sem erro (log mostra os valores trocados).
- [ ] **S6.4** — Mudar min/max no F12 durante um desmaio em curso → não afeta esse desmaio, só o próximo.
- [ ] **S6.5 (novo, ALTA PRIORIDADE — recuperado do cenário 38 da matriz original, dropado na consolidação; risco de cultura numérica já documentado na memória do mod)** — Simular uma atualização de versão anterior (pré-008) com `Duracao do Desmaio` = um valor **fracionário** (ex. `47.5`) gravado no `.cfg`, testado com o **Windows configurado em pt-BR** → após o boot, `Duracao Minima do Desmaio` e `Duracao Maxima do Desmaio` devem nascer **ambos iguais a 47,5** — **não** 475 (o bug que este teste existe para pegar: vírgula decimal pt-BR lida como separador de milhar, inflando o valor 10× e sendo clampado ao teto 120).

### Fase S7 — Migração de config, distância e i18n (item 010)

- [ ] **S7.1** — Interagir com um player/bot ferido a ~3,5m → prompt "Examine (Medic)"/"Shoulder tap" aparece; a ~5-6m (distância antiga) **não** aparece mais.
- [ ] **S7.1b (novo)** — Confirmar que **outras** ações além do prompt principal (aplicar torniquete, Emergency Drop) respeitam a MESMA distância nova (3,5m + margem) — não deve sobrar nenhum caminho de código ainda usando a distância antiga de teste.
- [ ] **S7.2** — Trocar o idioma do JOGO para português → reabrir o menu médico: título do HUD, rótulos de membro, rodapé de atalhos aparecem em português. Trocar para inglês → tudo em inglês, **sem precisar reiniciar o jogo**.
- [ ] **S7.3** — Com o jogo em português: aplicar um torniquete → notificação "Torniquete aplicado: {membro}..."; remover → "Torniquete removido..."; deixar necrosar → aviso ⚠ e depois ☠. Trocar para inglês e repetir → mesmas notificações em inglês, com os mesmos ícones.
- [ ] **S7.4** — Tocar no ombro de um aliado → notificação "✈ Shoulder tap → {nickname}" (ou PT); o aliado recebe "✈ You received a shoulder tap from {nickname}" (ou PT).
- [ ] **S7.5** — Curar um bot com item incompatível com o ferimento → notificação de recusa aparece traduzida corretamente (confirma que `DenyReason`→`DenyReasonId` não quebrou a mensagem exibida).
- [ ] **S7.6** — Iniciar e completar um tratamento (com e sem item) → texto "► TREATING: {membro}" / "► {ITEM} → {membro}" aparece no idioma correto.
- [ ] **S7.7 (novo)** — Com um `.cfg` de uma instalação anterior à v1.10.0 (ou simulado conforme P0.6, com as 3 keys legadas ainda gravadas): iniciar o jogo → boot limpo, **sem** erro nem warning no `LogOutput.log` relacionado às keys órfãs.

### Fase S8 — Corners transversais do motor (solo)

- [ ] **S8.1** — Religar o mod inteiro (toggle master OFF→ON) no meio da raid com múltiplas condições já ativas (pernas + braços + estômago zerados) → todos os estados se re-estabelecem sem toast/one-shot duplicado.
- [ ] **S8.2** — Raid 1 → extração → Raid 2: nenhum estado, cooldown ou toast "já visto" sobrevive entre raids (comparar com S1b, que testa persistência de DANO, não de cooldown/toast).
- [ ] **S8.3** — Hideout: nenhum log de tracking do motor aparece fora de raid.
- [ ] **S8.4** — Toast de 1ª ocorrência aparece exatamente 1×/estado/raid (usar "Debug Test Consumer" pra confirmar rápido, ligando/desligando).

> **Não incluído neste plano (baixa testabilidade manual, aceito como verificação estática de código):** múltiplas transições em regiões diferentes no mesmo frame exato (consolidação single-pass), bot nascendo no meio da raid (spawn mid-raid) sendo estabelecedor na primeira avaliação, divisão por zero com vida pré-tiro zerada/negativa, idioma indisponível durante a race de boot. Esses corners exigem instrumentação/debug, não são práticos de forçar manualmente em raid real.

### Fase S9 — Smoke test de bot com IA de terceiros (item 009, executável solo)

> Reaproveita o roteiro B1 de `trauma-coop-test-protocol.md` — não exige 2º PC, só bots com SAIN/ORBIT ativos.

- [ ] **S9.1** — Pré-condição: confirmar SAIN e ORBIT instalados/ativos (ver `trauma-compat-suite.md`).
- [ ] **S9.2** — Quebrar as 2 pernas de um bot sem matá-lo → cai (prone) e fica pelo menos `Bot Fall Hold Seconds` (default 15s) sem atirar/se locomover (log `[Trauma2]` confirma o hold).
- [ ] **S9.3** — Após o hold, a IA (SAIN/ORBIT) tenta levantar o bot; se a fratura persistir, ele é **re-derrubado** (não fica preso de pé nem preso deitado indefinidamente).
- [ ] **S9.4** — Repetir por pelo menos 2 ciclos completos (cair → hold → tentar levantar → re-cair) sem travar em nenhuma das duas pontas.
- [ ] **S9.5** — Repetir com um bot Tank (CustomClasses-Tank) se possível, para validar o mecanismo de composição de velocidade num cenário real.

---

## Parte B — Testes COOP (2 PCs)

**Pré-requisito obrigatório e não-negociável:** todas as máquinas na sessão coop devem rodar **exatamente a mesma build** (v1.10.0) — o item 010 mudou o **wire format** do handshake de recusa de cura, então uma build antiga falha ao desserializar esse pacote especificamente (não é só uma diferença cosmética como nos itens 003-009).

- [ ] **C0.1** — Confirmar a versão 1.10.0 em **todos** os PCs antes de iniciar qualquer cenário abaixo.

### Fase C1 — Visibilidade e sincronização entre peers

- [ ] **C1.1** — Mancar (003) visível ao peer: P1 zera/quebra uma perna; P2 observa a manqueira (N1/N2) pelo sync nativo de pose do Fika, sem lag perceptível além do esperado da rede.
- [ ] **C1.2** — Ciclo de queda (004) visível ao peer: P1 quebra as 2 pernas sem analgésico; P2 observa a queda, a janela de 3s de pé, e o re-cair automático, tudo sincronizado.
- [ ] **C1.3** — Tremor (005) — **confirmar que o peer NÃO vê** (limitação aceita por design): P1 sente o tremor na própria mira; P2 não vê nenhum indicativo visual em P1. O cancelamento de ADS em si (arma abaixando) **deve** ser visível a P2.
- [ ] **C1.4** — Agachar do estômago (006) visível ao peer: P1 zera o estômago e tem sucesso no roll; P2 observa o agachar involuntário.
- [ ] **C1.5** — Desmaio percentual (007) + duração aleatória (008): P1 sofre um hit que atinge o gatilho; P2 observa P1 desmaiando, e a duração percebida por AMBOS é a mesma (sincronizada via pacote).
- [ ] **C1.6** — Vozes de dor audíveis ao peer: P1 sofre uma queda forçada (004) ou um bloqueio de re-ADS (005); P2 deve ouvir a voz de dor de P1 pela voz nativa do jogo.
- [ ] **C1.7** — Bots do host coerentes ao client: com P1 como host e bots ativos, P2 (client) vê os bots mancando/caindo/agachando igual ao que P1 (host) vê.
- [ ] **C1.8** — Toggle OFF/ON mid-raid replicado nos 2 PCs: desligar um consumidor (ex. "Fall Cycle") em P1 → os efeitos ativos somem na tela de P1 **e** de P2 simultaneamente, sem residual em nenhum dos dois lados.
- [ ] **C1.9 (novo)** — Reconexão/entrada tardia de peer Fika: P1 (dono de um bot ou de si mesmo com uma condição ativa) se desconecta e reconecta → re-avalia do zero (sem herdar estado de espelho); P2, que estava presente o tempo todo, deve ver o estado ATUAL correto assim que P1 reconecta.
- [ ] **C1.10 (novo)** — Cura remota revertendo efeito de trauma, visível a AMBOS os peers: P2 (médico) cura remotamente a perna zerada de P1 (paciente) → a manqueira de P1 some, observável tanto na tela de P1 quanto na de P2 simultaneamente.
- [ ] **C1.11 (novo)** — Jogador DOWNED (revive Fika) durante o ciclo de queda (004): P1 fica downed enquanto no ciclo → o ciclo pausa (mesmo tratamento que um desmaio); P2 revive P1 → o ciclo é re-avaliado a partir do snapshot atual (não retoma cegamente a fase anterior).
- [ ] **C1.12 (novo)** — P2 (médico) morre ou desconecta no MEIO da animação de cura de P1 (que está caído/downed) → a cura aborta de forma limpa, sem travar P1 num estado de "sendo curado" permanente.

### Fase C2 — i18n cross-peer e wire format (item 010)

- [ ] **C2.1** — P1 com o jogo em português aplica um torniquete em si mesmo ou num aliado; P2 com o jogo em **inglês** observa a notificação correspondente **em inglês** — confirma que a tradução acontece no cliente que EXIBE, não no que originou a ação.
- [ ] **C2.2** — Inverso do C2.1: P1 em inglês, P2 em português → P2 vê tudo em português.
- [ ] **C2.3** — P1 (médico) tenta curar P2 (paciente) com um item incompatível com o ferimento → P1 vê a notificação de recusa traduzida no PRÓPRIO idioma de P1, independente do idioma de P2 — confirma que `MedicDenyReasonId` (byte) trafegou corretamente pela rede.
- [ ] **C2.4** — Toque no ombro de P1 para P2 → P2 recebe a notificação traduzida no PRÓPRIO idioma de P2 (ícone ✈ presente).

### Fase C3 — Testes negativos / robustez

- [ ] **C3.1** — Uma raid com **versão do mod diferente** entre os peers (ex.: um numa build anterior ao item 010, wire format antigo) → confirmar comportamento gracioso (sem exception que descarte outros pacotes de rede do frame) — não se espera que a coop funcione corretamente, só que falhe de forma contida.
- [ ] **C3.2 (separado de C3.1 — modo de falha diferente)** — Uma raid com o mod **totalmente ausente** num dos peers → mesmo critério de robustez (sem exception generalizada), reconhecendo que é uma falha de natureza distinta (sem handler nenhum para os pacotes, em vez de handler com wire format divergente).

---

## Sequenciamento recomendado

1. **P0** (setup) + **S0** (verificação de build) — sempre primeiro.
2. **S1 → S1b → S2 → S3 → S4 → S5 → S6 → S7**, nessa ordem, respeitando a instrução de **curar a condição testada antes de avançar de fase** (destacado inline em S2/S3/S4) — evita que uma fratura/condição residual de uma fase distorça o setup controlado da próxima (ex.: testar o desmaio percentual em S5 exige posicionamento livre, difícil de calibrar se o personagem ainda estiver mancando de S1-S4).
3. **S8** — pode ser intercalado com S1-S7 (aproveitando os mesmos raids) ou feito à parte.
4. **S9** — precisa de bots ativos com SAIN/ORBIT; fazer numa raid dedicada.
5. **C0 → C3** — só depois de TODA a Parte A estar ✅ e com os 2 PCs confirmados na mesma build.

## Checklist de cobertura (meta-verificação)

- [ ] Todo item marcado 🟢 no `mod-backlog.md` (002-010) tem pelo menos 1 teste correspondente neste documento.
- [ ] Todo critério de aceite **Fika/multiplayer** de cada spec funcional (002-010) tem um teste na Parte B.
- [ ] Todo critério de aceite **Estado entre raids** de cada spec funcional tem um teste correspondente (S1b cobre a persistência de dano; S2.8/S8.2 cobrem reset de cooldown/estado transitório).

## Histórico de Alterações

| Data | Autor | Alteração |
|---|---|---|
| 2026-07-25 | Guilherme (com Claude) | Criação — consolida `trauma-behavior-matrix.md` §5 (44 cenários, itens 002-008) + `trauma-coop-test-protocol.md` (B1/B2, item 009) numa sequência única, mais cenários novos do item 010. |
| 2026-07-25 | Guilherme (com Claude) | Revisão adversarial (releitura cética das 9 specs funcionais + matriz completa): recuperados 3 cenários dropados silenciosamente na consolidação (fila de adiados multi-região, roll de estômago durante desmaio, migração fracionária pt-BR — risco de cultura já documentado em memória); nova fase S1b (persistência entre raids, gap em 5 sistemas); S3.7 fecha o único gap de toggle-OFF entre os 4 consumidores de estado contínuo; 4 cenários coop novos (reconexão Fika, cura remota cross-peer, DOWNED durante ciclo de queda, médico desconectando mid-cura); 2 cenários novos do item 010 (distância em ações secundárias, config órfã no `.cfg`); instruções explícitas de "curar antes de avançar" entre fases solo; C3.1 dividido em 2 modos de falha distintos. |
