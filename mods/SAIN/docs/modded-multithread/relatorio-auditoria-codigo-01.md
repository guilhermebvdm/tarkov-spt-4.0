---
title: "Relatório de Auditoria Técnica de Código — SAIN modded-multithread (Review 01, modo --perf)"
date: 2026-09-19
status: 🟢 Vivo
authors: ["Claude Sonnet 5"]
---

# Relatório de Auditoria Técnica de Código — SAIN `modded-multithread` (Review 01, modo `--perf`)

> **Escopo desta rodada:** restrito ao subsistema de limitação/throttling de IA por distância — "AI Limiter" (`SAINAILimit.cs`/`AILimitSettings.cs`) e o sistema de LOD adaptativo de tick-rate introduzido no fork multithread (`BotComponent.cs`), mais a infraestrutura de rastreamento de jogadores da qual ambos dependem (`PlayerSpawnTracker.cs`, `PlayerComponent.cs`). Não é uma auditoria integral do mod (esse trabalho já existe: 30 relatórios em `docs/modded/01-30` da Sessão 1, `mods/SAIN/memory/sessions.md:9`). Esta rodada nasce de uma investigação sob demanda (comparação `original` vs `modded-multithread` do AI Limiter + descarte da hipótese de bots enxergando através de paredes/árvores — ver `mods/SAIN/memory/sessions.md`, a ser registrado via `/update-memory`) e é formalizada aqui para poder virar item de backlog pelo `/optimize-mod-performance`.

**Memória consultada:** topo de `mods/SAIN/memory/sessions.md`, Sessões 1 e 2 (última: 2026-09-07). Pendências que afetam este escopo:
- **[P-2.1]** 🟡 (aberta 2026-09-07): fix de `ArgumentOutOfRangeException` em `DirectionDataJob`/`EnemyPlaceRaycastJob`/`VisionRaycastJob` aplicado mas **não validado in-game**. `VisionRaycastJob.cs` é um dos arquivos já auditados nesta rodada (comparação de oclusão) — nenhuma interação nova encontrada com o achado atual, mas a pendência de validação permanece em aberto e independente.
- **[P-2.2]** 🟢 (aberta 2026-09-07): débito arquitetural sobre compartilhamento de `List`/`NativeArray` vivo entre `PlayerComponent` e buffers de job — mesma família de classes (`Classes/BotManager/Jobs/`) tocada por este relatório, mas mecanismo diferente (aqui é custo de execução redundante, não retenção/corrupção de estado). Não se sobrepõe aos achados abaixo.

Nenhuma pendência 🔴 encontrada. Nenhum achado abaixo duplica os 30 relatórios da Sessão 1 (escopo diferente: aqueles cobrem `docs/original/`/`docs/modded/`, este cobre o fork `modded-multithread`, inexistente até a Sessão 2).

## 1. Resumo Executivo da Auditoria

| Severidade | Quantidade | Descrição |
|---|---|---|
| 🔴 **Crítico** | 0 | — |
| 🟠 **Alto** | 0 | — |
| 🟡 **Médio** | 2 | Busca de distância duplicada por bot; oscilação de tick-rate na fronteira de LOD |
| 🔵 **Baixo** | 1 | Dois sistemas de limitação coexistindo sem coordenação/documentação |
| 💡 **Otimização** | 1 | Limiares do LOD não configuráveis via F12/F6 (CFG) |

## 2. Tabela de Achados

| ID | Severidade | Arquivo / Linha | Categoria | Descrição Resumida |
|---|---|---|---|---|
| `AUD-01-01` | 🟡 Médio | `SAIN/Classes/Bot/SAINAILimit.cs:36-46` + `SAIN/Components/BotComponent.cs:227-248` | ENT / FREQ | Duas buscas independentes de "humano mais próximo" por bot, em cadências diferentes, sem compartilhar resultado |
| `AUD-01-02` | 🟡 Médio | `SAIN/Components/BotComponent.cs:272` | FREQ | Fronteira Tier0/Tier1 do LOD sem histerese — bot parado ~50m do jogador pode alternar taxa de tick a cada 0.5s |
| `AUD-01-03` | 💡 Otimização | `SAIN/Components/BotComponent.cs:195-198` | CFG | Limiares de distância/intervalo do LOD são `const` privadas, não expostas no editor F6 (ao contrário de `AILimitSettings.AILimitRanges`) |
| `AUD-01-04` | 🔵 Baixo | `SAIN/Components/BotComponent.cs:298-304` + `SAIN/Classes/Bot/SAINAILimit.cs` (todo o arquivo) | LIFE / Arquitetura | Dois mecanismos de limitação de IA (degradação sensorial vs. throttle de tick-rate) coexistem sem coordenação nem documentação da relação entre eles |

---

## 3. Detalhamento dos Achados

### AUD-01-01 · Busca de distância ao humano mais próximo duplicada por bot

- **Severidade:** 🟡 Médio
- **Evidência:** Forte — confirmado por leitura direta dos dois pontos de chamada.
- **Execução:** per-tick-IA × N bots ativos × raid inteira. `SAINAILimit.CheckAILimit()` roda a cada ~3s por bot (`AILimitUpdateFrequency`, jitter incluso); `BotComponent.GetMinDistanceToHumanPlayer()` roda a cada 0.5s por bot. Ambas chamam `PlayerSpawnTracker.FindClosestHumanPlayer`, que é uma varredura O(M) sobre `AlivePlayersDictionary` (M = jogadores humanos vivos). Custo não cresce com a raid (M é pequeno e estável), mas dobra desnecessariamente o número de varreduras por bot por minuto.
- **Localização no Mod:** [SAINAILimit.cs:36-46](../../modded-multithread/SAIN/Classes/Bot/SAINAILimit.cs#L36), [BotComponent.cs:227-248](../../modded-multithread/SAIN/Components/BotComponent.cs#L227)
- **Referência Cruzada:** N/A — lógica interna do SAIN (não é patch sobre método do EFT).
- **Causa Raiz:** o LOD adaptativo (introduzido no fork multithread, `docs/modded-multithread/01-arquitetura-multithread-e-lod.md`) foi adicionado como sistema paralelo ao `SAINAILimit` já existente, sem integrá-lo. `BotComponent.cs:236` já reusa o mesmo método (`FindClosestHumanPlayer`, corrigido no `CR-01-02` da revisão de código do próprio fork — `docs/modded-multithread/02-code-review-etapa-1.md`), mas cada chamador ainda dispara sua própria busca independente, em cadência própria.
- **Impacto Técnico Real:** para um bot ativo numa raid de 30 minutos: ~600 buscas via `SAINAILimit` (a cada 3s) + ~3600 via `BotComponent` (a cada 0.5s) = 4200 varreduras/bot/raid, das quais até 600 são estritamente redundantes (a leitura do `SAINAILimit` poderia reusar o valor já calculado 0.5s antes). Multiplicado por N bots ativos (10-50 num mapa cheio), é overhead mensurável mas não crítico — cada varredura é O(M) com M pequeno (poucos humanos), não O(N²).
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - *Abordagem Atual:* `SAINAILimit.CheckAILimit()` chama `PlayerSpawnTracker.FindClosestHumanPlayer` diretamente.
  - *Abordagem Otimizada:* `SAINAILimit.CheckAILimit()` lê `Bot.DistanceToClosestHuman` (propriedade pública já existente em `BotComponent.cs:200`, mantida atualizada a cada 0.5s pelo LOD) em vez de rodar sua própria busca. A cadência de 0.5s é mais fina que os 3s do limiter, então não há perda de precisão.
  - *Código Refatorado (esboço, não aplicado nesta auditoria):*

```csharp
// SAINAILimit.cs — trecho ilustrativo da mudança proposta
private void CheckAILimit()
{
    // Antes: PlayerSpawnTracker.FindClosestHumanPlayer(...) próprio
    // Depois: reusa o valor já calculado pelo LOD no mesmo bot
    float distToHuman = Bot.DistanceToClosestHuman; // já público, BotComponent.cs:200
    // ... resto da lógica de CheckDistances inalterada
}
```

- **Como validar:** census agregado (contador `static long`) do número de chamadas a `PlayerSpawnTracker.FindClosestHumanPlayer` por minuto de raid, antes/depois, no mesmo mapa com contagem de bots semelhante. Critério: cai para ~metade (elimina a chamada do `SAINAILimit`, mantém a do LOD).
- **Decisão:**
  - `[ ]` Pendente
  - `[x]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-02 · Fronteira Tier0/Tier1 do LOD sem histerese (oscilação de tick-rate)

- **Severidade:** 🟡 Médio
- **Evidência:** Forte.
- **Execução:** per-tick × N bots próximos da fronteira de 50m. Checado a cada 0.5s (`_nextDistCheckTime`).
- **Localização no Mod:** [BotComponent.cs:272](../../modded-multithread/SAIN/Components/BotComponent.cs#L272)
- **Referência Cruzada:** N/A — lógica interna do SAIN.
- **Causa Raiz:** `isCloseToHuman = distToHuman <= LOD_CLOSE_DIST` é um único limiar sem margem — um bot patrulhando ou parado próximo aos 50m pode cruzar a fronteira a cada checagem de 0.5s, alternando entre Tier 0 (60Hz) e Tier 1 (25Hz) repetidamente. Diferente do gatilho de combate (que já tem `_combatLockoutTime`, uma histerese temporal de 15s), a fronteira de distância não tem proteção equivalente.
- **Impacto Técnico Real:** não é um vazamento nem uma explosão de custo — é uma variação de responsividade da IA perceptível em degraus (o bot "pisca" entre atualização rápida e lenta) para bots parados perto do limiar, e um pequeno custo extra de recomputar transições de tier com mais frequência que o necessário.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - *Abordagem Atual:* limiar único, `<=`, checado a cada 0.5s.
  - *Abordagem Otimizada:* dead-band assimétrico — entra em "perto" com `distToHuman <= LOD_CLOSE_DIST`, só sai de "perto" quando `distToHuman > LOD_CLOSE_DIST + LOD_CLOSE_DIST_MARGIN` (sugestão: 10m). Precisa de um campo de estado por bot (`_wasCloseToHuman`), mesmo padrão já usado por `_combatLockoutTime`.
  - *Código Refatorado (esboço):*

```csharp
// BotComponent.cs — dead-band na transição perto/longe
bool isCloseToHuman;
if (_wasCloseToHuman)
{
    isCloseToHuman = distToHuman <= (LOD_CLOSE_DIST + LOD_CLOSE_DIST_MARGIN);
}
else
{
    isCloseToHuman = distToHuman <= LOD_CLOSE_DIST;
}
_wasCloseToHuman = isCloseToHuman;
```

- **Como validar:** census de transições de tier por bot por minuto (contador incrementado toda vez que `CurrentLodTier` muda de valor), antes/depois, com um bot posicionado deliberadamente a ~50m do jogador por 2 minutos. Critério: número de transições cai a próximo de zero no cenário parado.
- **Decisão:**
  - `[ ]` Pendente
  - `[x]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-03 · Limiares do LOD não configuráveis via F12/F6

- **Severidade:** 💡 Otimização (CFG)
- **Evidência:** Forte — já identificado e deferido pelo próprio processo de revisão do fork (`CR-01-04`, `docs/modded-multithread/02-code-review-etapa-1.md:162-175`); esta auditoria confirma que segue não implementado no código atual.
- **Localização no Mod:** [BotComponent.cs:195-198](../../modded-multithread/SAIN/Components/BotComponent.cs#L195)
- **Causa Raiz:** `LOD_CLOSE_DIST`, `LOD_MID_DIST`, `LOD_MID_INTERVAL`, `LOD_FAR_INTERVAL` são `const` privadas — diferente de `AILimitSettings.AILimitRanges` (dicionário público com atributos `[Name]`/`[Description]`/`[MinMax]`, editável em runtime pelo F6).
- **Impacto Técnico Real:** nenhum custo de execução — é uma lacuna de configurabilidade. É a alavanca de ajuste fino de performance mais barata disponível hoje (mudar cadência/raio sem tocar em lógica), mas está inacessível ao usuário.
- **Alternativa de Melhor Lógica / Proposta de Correção:** migrar as 4 constantes para dentro de `AILimitSettings.cs` (ou uma classe irmã no mesmo padrão de atributos), com os defaults atuais preservados. `BotComponent.cs` passa a ler de `GlobalSettingsClass.Instance.General.AILimit` em vez das `const` privadas.
- **Como validar:** confirmação visual no editor F6 (os 4 campos aparecem e alteram o comportamento em tempo real).
- **Decisão:**
  - `[ ]` Pendente
  - `[x]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-04 · Dois sistemas de limitação de IA coexistindo sem coordenação/documentação

- **Severidade:** 🔵 Baixo
- **Evidência:** Forte.
- **Localização no Mod:** [BotComponent.cs:298-304](../../modded-multithread/SAIN/Components/BotComponent.cs#L298) (grupo `_tickWhenActiveClasses`, onde `SAINAILimit` está registrado e fica sujeito ao throttle do LOD por cima da própria lógica de `AILimitSettings`), documentação em [`docs/modded-multithread/01-arquitetura-multithread-e-lod.md:125-134`](01-arquitetura-multithread-e-lod.md).
- **Causa Raiz:** a documentação de arquitetura trata o LOD como *o* AI Limiter, sem mencionar que o `SAINAILimit`/`AILimitSettings` original continua rodando, agora aninhado dentro do throttle do LOD. Isso não é um bug de execução, mas é uma lacuna de manutenibilidade: um desenvolvedor futuro (ou uma sessão de IA sem este contexto) pode "corrigir" um dos dois sistemas sem perceber o outro.
- **Impacto Técnico Real:** nenhum custo de execução direto — é a causa raiz que teve `AUD-01-01` (duplicação) como sintoma. Resolver esta lacuna de documentação é o que evita que o mesmo tipo de duplicação reapareça no futuro.
- **Alternativa de Melhor Lógica / Proposta de Correção:** atualizar `docs/modded-multithread/01-arquitetura-multithread-e-lod.md` com uma seção explícita nomeando os dois mecanismos e a relação entre eles (o LOD controla frequência de tick; o `AILimitSettings` controla alcance sensorial; um bot em Tier 1/2 ainda roda `SAINAILimit`, só que a cadência mais baixa).
- **Como validar:** N/A (mudança de documentação, não de código).
- **Decisão:**
  - `[ ]` Pendente
  - `[x]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

## 4. Panorama de execução

| Superfície | Classe de frequência | Multiplicador de entidades | Gate de contexto | Quem para / quando |
|---|---|---|---|---|
| `SAINAILimit.CheckAILimit()` | per-tick-IA (~1/3s, com jitter) | × N bots ativos | `TickRequirement = ESAINTickState.OnlyBotActive` (só bot ativo) | Não roda para bot morto/inativo; segue rodando mesmo em Tier 1/2 do LOD, só que dentro do grupo throttlado |
| `BotComponent.GetMinDistanceToHumanPlayer()` | per-tick (0.5s fixo, `_nextDistCheckTime`) | × N bots ativos | Chamado incondicionalmente dentro de `ManualUpdate` | Sem gate adicional — roda sempre que o bot está ativo |
| `PlayerSpawnTracker.FindClosestHumanPlayer` | chamada por ambos acima | × N bots × O(M humanos) por chamada | — | Varredura completa a cada chamada (M é pequeno, não é ofensor por si só) |
| Grupo `_tickWhenActiveClasses` (inclui `SAINAILimit` e outras classes de decisão) | 60Hz (Tier 0) / 25Hz (Tier 1) / 8Hz (Tier 2) | × N bots, por tier | `shouldTickLOD` (`BotComponent.cs:277-296`) | Throttle correto — não é achado, é o próprio mecanismo de otimização sendo auditado |
| `VisionRaycastJob` (raycast primário de linha-de-visão) | 30Hz fixo, para todos os bots | × N bots × inimigos candidatos | `enemy.ShallCheckLoS(currentTime)` | **Não é gated pelo LOD** — confirmado nesta investigação que isso é intencional (preserva correção de oclusão); fora do escopo de otimização desta rodada |

## 5. Configuração

| Chave | Onde | Default atual | Default proposto | Alimenta |
|---|---|---|---|---|
| `AILimitSettings.AILimitRanges` (Far/VeryFar/Narnia) | `AILimitSettings.cs:24-29`, editável via F6 | 150/250/400m | Sem mudança proposta | Alcance de visão/audição degradado entre bots |
| `LOD_CLOSE_DIST` / `LOD_MID_DIST` | `BotComponent.cs:195-198`, `const` privada, **não exposta** | 50m / 150m | Expor via F6 (AUD-01-03); manter valores | Fronteira de tier do throttle de tick-rate |
| `LOD_MID_INTERVAL` / `LOD_FAR_INTERVAL` | idem | 0.04s (~25Hz) / 0.12s (~8Hz) | Expor via F6 (AUD-01-03); manter valores | Cadência de tick para Tier 1/2 |
| `LOD_CLOSE_DIST_MARGIN` (novo, proposto em `AUD-01-02`) | não existe ainda | — | 10m | Dead-band da fronteira Tier0/Tier1 |
| `COMBAT_LOCKOUT_DURATION` | `BotComponent.cs:195-198` | 15s | Sem mudança proposta | Já funciona como histerese do gatilho de combate |

## 6. Instrumentação proposta

Nenhum achado desta rodada é classificado como "Suspeita" — todos têm evidência forte por leitura direta, sem depender de medição para fechar o diagnóstico. A instrumentação listada abaixo serve apenas para a **validação** (§7 da skill, "Como validar" de cada achado), não para provar a existência do problema:

- **Census de chamadas** (`AUD-01-01`): contador `static long _findClosestHumanCallCount` incrementado dentro de `PlayerSpawnTracker.FindClosestHumanPlayer`, dump agregado no raid-end via log gated por `ConfigEntry<bool>` (`Debug.PerfInstrumentation`, default `false`). Marcar `// PERF-INSTR AUD-01-01 — temporary, remove after validation`.
- **Census de transições de tier** (`AUD-01-02`): contador por bot incrementado em `BotComponent.cs` sempre que `CurrentLodTier` muda de valor, dump agregado (total de transições / bot / minuto) no mesmo gate de config. Marcar `// PERF-INSTR AUD-01-02`.

## 7. Plano de validação

| Achado | Métrica | Cenário pareado | Critério de sucesso |
|---|---|---|---|
| `AUD-01-01` | Chamadas/min a `FindClosestHumanPlayer` | Mesmo mapa, mesma contagem de bots ativos, 10 min de raid, antes/depois | Cai para ~metade (elimina a chamada duplicada do `SAINAILimit`) |
| `AUD-01-02` | Transições de tier/bot/min | Bot posicionado a ~50m do jogador, parado, 2 min | Cai de "várias por minuto" para próximo de zero |
| `AUD-01-03` | Confirmação visual F6 | — | Campos aparecem e alteram comportamento em tempo real |
| Regressão (todos) | Bot perto de convidado (não-host) e em Headless continua Tier 0 | Raid Fika real com 2+ humanos, um bot perto só do convidado | Nenhuma mudança de comportamento — já verificado nesta investigação que o mecanismo subjacente (`FindClosestHumanPlayer`/`IsAI`) não depende de "jogador local" |
| Regressão (todos) | Nenhum membro `public`/`protected` alterado | Diff completo dos arquivos tocados | 0 renomeações/remoções de assinatura |

## 8. Plano de Ação e Recomendações

1. Agrupar `AUD-01-01`, `AUD-01-02` e `AUD-01-03` num único item de backlog via `/optimize-mod-performance SAIN --fase 2` (ou `/add-backlog-item` + ciclo normal, já que o volume é pequeno) — spec de **não-regressão** explícita cobrindo host/convidado/headless e preservação de API pública.
2. `AUD-01-04` é só documentação — pode ser aplicado direto, sem passar pelo ciclo de código.
3. Nenhum achado 🔴/🟠 nesta rodada — não há bloqueio para o mod continuar em uso enquanto a correção não é aplicada.
