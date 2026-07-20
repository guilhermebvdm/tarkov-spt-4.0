# 007 — Desmaio 2.0: gatilhos percentuais

**Mod:** TRL-ImmersiveCombatMedicine
**Status:** Backlog
**Criado:** 2026-07-19

## Visão geral

Substitui os gatilhos de desmaio por limiar de dano ABSOLUTO fixo (tórax ≥35, cabeça ≥10) por um gatilho PERCENTUAL sobre a vida que a parte tinha imediatamente ANTES do tiro, com pisos absolutos de segurança e um gate de analgésico que reduz ou zera a chance. Mantém intocado todo o pipeline de desmaio já validado (relógio único de duração, wake, grace, sincronização coop) — só a CONDIÇÃO de entrada muda.

## Comportamento atual

Hoje (`DamageTriggerPatch.Postfix`, patch em `Player.ApplyDamageInfo`) o desmaio dispara quando:
- Tórax recebe um hit de dano ≥35 (absoluto, ignora quanto de vida a parte tinha), OU
- Cabeça recebe um hit de dano ≥10 (mesma regra).

Não há gate de analgésico — o jogador desmaia independente de estar sob efeito de analgésico. Não há relação com a vida atual da parte: um tórax com 5 de vida restante e um tórax com 85 de vida restante reagem ao mesmo limiar fixo de 35 de dano. O restante do pipeline (registro em `BlackoutTimers`, duração fixa, wake, grace, sincronização Fika) é reaproveitado sem mudança.

Já existe hoje um toggle placeholder reservado para este item na seção de consumidores do Trauma 2.0 ("Blackout 2.0 (item 007)"), atualmente OFF e sem função — o gatilho ativo continua exclusivamente no toggle antigo ("Sistema de Desmaio"). O predicado "analgésico ativo agora" do motor Trauma 2.0 (o mesmo que os itens 003-006 usam para reduzir/suprimir seus efeitos) já está documentado no código como reservado para este item.

## Comportamento desejado

O gatilho passa a comparar o dano do hit contra a vida que a parte tinha IMEDIATAMENTE ANTES desse hit (não a vida atual pós-dano, não a vida máxima da parte):

- **Tórax:** um hit que remove ≥50% da vida atual (pré-tiro) do tórax rola p=50% de desmaiar — **exceto se o jogador estiver sob efeito de analgésico no instante do hit, caso em que fica IMUNE (0% de chance)**.
- **Cabeça:** um hit que remove ≥25% da vida atual (pré-tiro) da cabeça rola p=50% de desmaiar; sob analgésico no instante do hit, a chance cai para p=25% (cabeça NÃO fica imune — só reduz a probabilidade).
- **Pisos absolutos de segurança** (independentes do percentual): o gatilho do tórax exige TAMBÉM ≥25 de dano absoluto no hit; o da cabeça exige TAMBÉM ≥10 de dano absoluto. Um hit que atinge o percentual mas fica abaixo do piso absoluto NÃO dispara o roll (evita desmaio por hit percentualmente grande mas fisicamente insignificante, ex.: tórax com 8 de vida restante recebendo 5 de dano = 62% mas só 5 de dano absoluto).
- **Sem agregação de pellets:** cada hit (pellet de espingarda, fragmento) é avaliado individualmente contra a vida que a parte tinha antes DAQUELE hit específico — não se soma o dano de múltiplos pellets do mesmo disparo antes de comparar.
- O restante do pipeline (registro em `BlackoutTimers`, duração — hoje fixa, item 008 trata da duração aleatória —, wake, grace, sincronização Fika, guard de re-entrada durante blackout/grace) permanece EXATAMENTE como está hoje. Este item só troca a CONDIÇÃO que decide se o roll acontece e a probabilidade do roll.
- **Decisão de design (tomada autonomamente nesta sessão, diretiva do usuário de prosseguir o overhaul até o fim):** diferente dos itens 003-006, "Sistema de Desmaio" (`ConfigBlackoutEnabled`) hoje é o MASTER de TODO o pipeline de desmaio (gatilho + `BlackoutTimers` + wake + grace + sincronização), não só do efeito — ao contrário de "Sistema de Pernas/Braços/Estomago", cujo tracking já rodava por baixo do motor independente do toggle legado. `ConfigBlackoutEnabled` CONTINUA sendo o master do pipeline como um todo (papel que já cumpre desde antes do Trauma 2.0 existir); o toggle placeholder "Blackout 2.0 (item 007)" nasce ON nesta entrega e atua como um SUB-toggle que só decide se o gatilho de ENTRADA usa a lógica percentual nova (este item) — nunca reintroduzindo o limiar fixo em paralelo. Razão: trocar o master de um pipeline com histórico extenso de bugs de timing (CR-04/CR-05, documentados na memória do mod) por um risco desnecessário fora do escopo deste item; a opção alternativa (Blackout 2.0 substituir `ConfigBlackoutEnabled` como master) fica registrada como não escolhida, não como incorreta.

## Critérios de aceite

- [ ] Tórax com vida atual pré-tiro de 40 recebendo um hit de 25 de dano (62,5% da vida atual, ≥50%, ≥25 de dano absoluto) rola p=50% de desmaiar; sem analgésico ativo.
- [ ] Mesmo hit acima, mas com o jogador sob efeito de analgésico no instante do tiro: NUNCA desmaia (imunidade total do tórax).
- [ ] Cabeça com vida atual pré-tiro de 30 recebendo um hit de 12 de dano (40% da vida atual, ≥25%, ≥10 de dano absoluto) rola p=50% sem analgésico; com analgésico ativo no instante do tiro, rola p=25% (não fica imune).
- [ ] Um hit que atinge o percentual (ex.: tórax com 8 de vida recebendo 5 de dano = 62,5%) mas fica abaixo do piso absoluto (5 < 25) NÃO dispara nenhum roll — log de log deve mostrar o hit sendo ignorado por piso, não por percentual.
- [ ] Uma rajada com múltiplos pellets no mesmo disparo (ex.: espingarda) avalia cada pellet contra a vida da parte IMEDIATAMENTE ANTES daquele pellet específico, nunca soma os pellets antes de comparar.
- [ ] Os quatro números do gatilho (percentual tórax, percentual cabeça, piso absoluto tórax, piso absoluto cabeça) são configuráveis via F12, com defaults 50%/25%/25/10 — igual em espírito aos demais itens do overhaul (003-006), que expõem seus limiares como `ConfigEntry` documentadas em `PROPRIEDADES.md`.
- [ ] Os limiares fixos legados (dano ≥35 tórax / ≥10 cabeça, sem gate de analgésico) são REMOVIDOS do caminho de gatilho ativo — não existe toggle que reative o "modo antigo" em paralelo ao novo (ver nota de decisão sobre qual `ConfigEntry` governa o pipeline como um todo).
- [ ] **Fika/multiplayer:** o gatilho roda no processo DONO do jogador atingido (humano local avalia a si mesmo; bot avalia no host/headless) — nenhum protocolo novo, reaproveita a sincronização (`FikaBridge.SyncFaintStatus`) já validada pelo pipeline atual.
- [ ] **Estado entre raids:** nenhum estado novo persiste entre raids — o gatilho é avaliado a cada hit, sem cache entre raids; os timers/guards existentes (`BlackoutTimers`, `FaintedPlayerIds`, `BotFaintCooldowns`) continuam resetando na fronteira de raid como hoje.

## Corner cases

- [ ] **Vida atual pré-tiro já é zero ou negativa antes do hit** (parte já destruída/quebrada por hits anteriores no mesmo frame): não deve gerar divisão por zero nem percentual inválido — comportamento esperado é não disparar o roll percentual (parte já comprometida ao extremo não é o alvo deste gatilho).
- [ ] **Analgésico expira ENTRE o momento do tiro e a avaliação do gatilho** (mesmo frame, ordem de patches): o gate de analgésico deve refletir o estado NO INSTANTE do hit que causou o dano, não um estado reavaliado depois. Diferente dos estados contínuos (mancar, tremor, agachar) dos itens 003-006, o desmaio é um evento único (roll de probabilidade), não um estado que se reavalia quando o analgésico expira depois — não há "revert" a fazer aqui.
- [x] **Hit simultâneo em tórax E cabeça no mesmo frame** (ex.: explosão com múltiplas partes atingidas): resolvido na spec técnica §7 (nota PA-01-03) — cada região gera sua própria chamada de `ApplyDamageInfo`; a primeira a ter sucesso escreve `BlackoutTimers[id]`, e a guard existente (`BlackoutTimers.ContainsKey`) faz a segunda chamada retornar antes de avaliar o gatilho — sem sobrescrita de deadline nem desmaio duplo.
- [x] **Bot sob analgésico:** resolvido — o predicado de analgésico do motor (`IsUnderPainkiller`, já usado pelos itens 003-006) consulta `player.HealthController` genericamente e funciona tanto para o humano local quanto para bots do host/headless (mesmo `ActiveHealthController`). Bots avaliam o gate de analgésico da mesma forma que o jogador humano — sem caminho separado a definir.

## Fora de escopo

- [ ] Duração aleatória do desmaio (min-max) — é o item 008, que monta sobre o ponto único `RANGE-READY` já marcado no código.
- [ ] Qualquer mudança no pipeline pós-gatilho (wake, grace, sincronização, guard de re-entrada, contusão) — permanece intocado.
- [ ] Regiões além de tórax e cabeça (braços, pernas, estômago) — o desmaio nunca foi gatilhado por essas regiões e continua não sendo.

## Referências

- [docs/trauma-matrix.md](../../docs/trauma-matrix.md) — decisão 8/9 (comparação pré-tiro; imunidade do tórax), decisão 15 (pisos absolutos de dano)
- [mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/HealthPatches.cs](../../modded/Patches/Trauma/HealthPatches.cs) — implementação atual do gatilho (linhas 50-95)
- [008-desmaio-duracao-aleatoria/](../008-desmaio-duracao-aleatoria/) — item seguinte, monta sobre o ponto `RANGE-READY` que este item preserva

## Histórico

| Data | Evento |
|---|---|
| 2026-07-19 | Item criado via `/create-spec` (retomada do overhaul Trauma 2.0, P-3.7/P-3.4) |
| 2026-07-19 | Revisão `/review-spec` — corner case "bot sob analgésico" resolvido (predicado `IsUnderPainkiller` do motor já é genérico); 2 critérios de aceite adicionados (config F12 dos 4 números; remoção do caminho de gatilho fixo legado); decisão de design tomada autonomamente (diretiva do usuário) — `ConfigBlackoutEnabled` permanece master do pipeline; "Blackout 2.0" é sub-toggle do gatilho. |
