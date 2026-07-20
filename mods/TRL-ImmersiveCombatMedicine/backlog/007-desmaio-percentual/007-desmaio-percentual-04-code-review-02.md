# 007 — Desmaio 2.0: gatilhos percentuais · Code Review 02

**Mod:** TRL-ImmersiveCombatMedicine
**Spec funcional:** [007-desmaio-percentual-01-spec.md](007-desmaio-percentual-01-spec.md)
**Spec técnica:** [007-desmaio-percentual-02-spec-tech.md](007-desmaio-percentual-02-spec-tech.md)
**Asbuild:** [007-desmaio-percentual-05-asbuild.md](007-desmaio-percentual-05-asbuild.md)
**Data:** 2026-07-19

> Análise crítica do código implementado por `/code-mod`. Cada achado recebe um ID `CR-02-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.
>
> **Rodada 2 (SEGUNDA e ÚLTIMA planejada, P-3.7).** Revisão adversarial de contexto limpo — não assume que a rodada 1 (`007-desmaio-percentual-04-code-review-01.md`) pegou tudo. Memória consultada: topo de `mods/TRL-ImmersiveCombatMedicine/memory/sessions.md` (Sessão 4) + pendências P-2.13/P-2.14/P-2.15 (bugs históricos do pipeline de desmaio — relógio único, guard de re-entrada, sync Fika). Nenhuma reaberta: `git diff` contra HEAD confirma que `TraumaState.BlackoutTimers`/`BlackoutStartTimes`/`FikaBridge.SyncFaintStatus`/`BotFaintCooldowns`/o guard `ContainsKey`/`FaintedPlayerIds` seguem byte-idênticos ao pipeline já validado — só a condição de entrada (`shouldFaint`) foi trocada.
>
> **Os 2 achados da rodada 1 (CR-01-01, CR-01-02) foram reconferidos contra o código/spec ATUAIS, não contra o relatado:** ambos DE FATO aplicados. CR-01-01 (spec técnica §7 corrigida — `Priority.High` = 600, regra "maior valor executa primeiro entre Prefixes" documentada com a fonte do decompile) confirmado por leitura direta de `007-desmaio-percentual-02-spec-tech.md:263` — sem nenhum resíduo do valor/regra errados (`grep` no diretório do item, zero ocorrência de "Priority.High...200" fora das citações históricas dentro dos próprios arquivos de review). CR-01-02 (captura de `__state` movida para depois do gate `ConfigMasterEnabled`/`IsAlive`) confirmado em `HealthPatches.cs:14-32` — nenhum acionado.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 1 · Total: 1

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-02-01 | D — Arquitetura | 🟢 Menor | `05-asbuild.md` não foi atualizado com os fixes da rodada 1 do code-review (CR-01-01/02 aplicados fora do fluxo `/apply-code-review`) | ✅ Aplicado |

## Categorias

- **A — Crítico** — bug grave, crash garantido, corrupção de estado, security issue.
- **B — Bug latente** — comportamento errado em cenário plausível, não acionado pelo caminho golden.
- **C — Gap vs. spec** — código não implementa critério de aceite, corner case, ou AC da spec.
- **D — Arquitetura** — viola padrões do repo, duplica código, leak de estado, abuso de reflection.
- **E — Legibilidade/manutenção** — nomes ruins, comentário "porquê" ausente, código morto, complexidade desnecessária.
- **F — Melhoria opcional** — refactor de qualidade, micro-otimização, simplificação.

## Impacto

- 🔴 **Bloqueador** — fix obrigatório antes de fechar o item.
- 🟠 **Forte** — fix recomendado; pode ser deferido para `06-fix-NN.md` futuro.
- 🟡 **Médio** — anotar, decidir caso a caso.
- 🟢 **Menor** — opcional.

---

## Verificações de foco (Passo 4 da tarefa)

**1. Reordenamento do Prefix (CR-01-02) — `__state` definitivamente atribuído em todos os caminhos + `?.` removido com segurança:** confirmado sem achado. `HealthPatches.cs:15-19`: `__state = -1f;` roda **incondicionalmente** na primeira linha do Prefix, antes de qualquer `return`. Os dois early-returns seguintes (`if (!ConfigMasterEnabled.Value) return true;` e `if (__instance == null || !__instance.HealthController.IsAlive) return true;`) acontecem DEPOIS dessa atribuição — logo `__state` está definitivamente atribuído (exigência do C# para parâmetros `out`) em TODOS os caminhos de saída, incluindo os dois early-returns. A remoção do `?.` em `__instance.ActiveHealthController` (linha 30) é segura: nesse ponto do método, `__instance == null` já causou um `return true` na linha anterior — `__instance` não-null é garantido pelo fluxo de controle, não por coincidência.

**2. Precisão numérica do roll (`float` em `TraumaBlackoutTrigger.Evaluate`):** avaliado como risco teórico, irrelevante na prática — sem achado. A idiomática `if (effectiveDamage < absFloor) return false;` / `if (effectiveDamage < pctThreshold * preHitHp) return false;` implementa corretamente a semântica "≥" da spec funcional (hit que **atinge ou supera** o piso/percentual dispara o roll) — rejeitar só quando estritamente menor é o espelho certo de aceitar quando `>=`. Quanto à precisão de ponto flutuante no limite exato: com os defaults (`50`, `25`, `25`, `10`), `pctThreshold` (`50f/100f=0.5f`, `25f/100f=0.25f`) é exatamente representável em `float` (frações binárias), então não há erro de arredondamento na constante. Para valores de config não binários (ex.: `33`), há arredondamento de `33f/100f`, mas o cenário de risco (dano computado bit-a-bit igual a `pctThreshold * preHitHp`) exigiria que o resultado da cadeia de cálculo de dano do EFT (armadura, multiplicadores, `ApplyDamage`) colidisse exatamente com essa constante multiplicada — não uma coincidência plausível em combate real (dano é um valor contínuo derivado de fórmulas de blindagem/penetração, não escolhido para bater com o config). Documentado já como risco residual aceito em `007-desmaio-percentual-02-spec-tech.md` §7 ("dano de peer é quantizado... aceitável, sem mudança de comportamento").

**3. Bots (paridade com humano, sem guard `IsAI`/`IsYourPlayer` no gatilho novo):** confirmado sem achado. `grep` em `TraumaBlackoutTrigger.cs` inteiro: zero ocorrência de `IsAI`/`IsYourPlayer` — `Evaluate` não distingue humano de bot. `TraumaEngine.IsUnderPainkiller(player)` (`TraumaEngine.cs:99-107`) consulta `player.HealthController` (interface `IHealthController`, genérica) via `FindActiveEffect<GInterface358>()` — funciona identicamente para `Player` humano e para bot no host/headless (mesma classe concreta `ActiveHealthController` por trás da interface, confirmado por `TraumaEngine.IsOwnedHere`, `TraumaEngine.cs:110-114`: `p.HealthController is ActiveHealthController`). A paridade bot↔humano é estrutural (mesmo código, mesma classe), não uma decisão explícita a auditar linha por linha além disso — consistente com a auditoria AP-02/AP-03 já feita na spec técnica (`ObservedPlayer.ApplyDamageInfo` não chama `base`, inerte no espelho; `FikaPlayer`/`FikaBot` chamam `base`, patch dispara no dono humano E no bot).

**4. `ConfigVerboseEngineLog` — log `[Blackout2]` gated, não incondicional:** confirmado sem achado. Releitura direta de `TraumaBlackoutTrigger.cs:70-74` (log de sucesso/roll) e `:79-83` (`LogIgnored`, chamado nos 2 caminhos de rejeição por piso/percentual): AMBOS os pontos de log começam com `if (!TRLImmersiveCombatMedicinePlugin.ConfigVerboseEngineLog.Value) return;` (dentro de `LogIgnored`) ou `if (ConfigVerboseEngineLog.Value) { ... }` (no corpo principal de `Evaluate`) — nenhuma chamada a `ModLogger.LogInfo` roda incondicionalmente. A interpolação de string (`$"[Blackout2] ..."`) também só é avaliada dentro do bloco condicional — sem custo de formatação quando o log está desligado (default `false`).

**5. Ângulo adicional (auditoria independente desta rodada):** revisado `player.ActiveHealthController` sendo obtido de novo dentro de `Evaluate` (`TraumaBlackoutTrigger.cs:26`) além do Prefix — não é redundância problemática: o Prefix precisa do valor **pré**-hit, `Evaluate` precisa reler o valor **pós**-hit (`postHitHp`, linha 29) para computar `effectiveDamage` — são duas leituras com propósitos diferentes, ambas baratas (getter, sem alocação, fora de hot path per-frame). `Random.value` (UnityEngine) chamado só quando `rollChance > 0f` (short-circuit, linha 69) — sem custo de RNG desperdiçado no caso de imunidade do tórax sob analgésico. Revisado também `BandAidNetworkHandler.cs:130-144` (lado espelho da sincronização Fika, escreve em `BlackoutTimers`/`BlackoutStartTimes` via pacote) — não tocado por este item, gatilhado só pelo `FikaBridge.SyncFaintStatus` que o Postfix já chama após `Evaluate` retornar `true`; nenhuma duplicação de lógica de gatilho no lado espelho (o espelho só ecoa o resultado já decidido pelo dono). Sem achado.

---

## Pontos

### CR-02-01 · D — Arquitetura · 🟢 Menor · ✅ Aplicado em 2026-07-19

**`05-asbuild.md` não reflete os fixes aplicados na rodada 1 do code-review**

**Local:** [`mods/TRL-ImmersiveCombatMedicine/backlog/007-desmaio-percentual/007-desmaio-percentual-05-asbuild.md:35-39`](../../backlog/007-desmaio-percentual/007-desmaio-percentual-05-asbuild.md)

**Problema:** A seção "Mudanças posteriores" do as-built ainda está vazia (`(vazio inicialmente — preenchido por /apply-code-review)`), mas CR-01-01 (correção da spec técnica §7 — valores/regra de `HarmonyLib.Priority`) e CR-01-02 (reordenamento do Prefix em `HealthPatches.cs` para capturar `__state` só depois do gate `ConfigMasterEnabled`/`IsAlive`) já foram aplicados — confirmados no código/spec atuais, com "Resolução"/"Aplicação" preenchidas dentro do próprio `007-desmaio-percentual-04-code-review-01.md`. Pelo convênio do repo (`repo-workflow-best-practices` §2, tabela de artefatos: "05-asbuild.md" é "criado por `/code-mod`; atualizado por `/apply-code-review`"), a rodada de fixes foi aplicada diretamente (mesmo padrão descrito na memória do mod para os itens 005/006 nesta sessão: "Fixes... aplicados diretamente") em vez de passar pelo `/apply-code-review` formal — e por isso o as-built nunca ganhou a entrada correspondente.

**Por que importa:** O próprio cabeçalho do as-built declara que ele é a fonte de verdade quando diverge da spec técnica ("quando o conteúdo aqui diverge da spec técnica, este documento ganha"). Para o item de MAIOR RISCO do overhaul (P-3.7), um mantenedor futuro que consulte só o as-built (sem cruzar os 2 arquivos de code-review) não vê que o Prefix foi reordenado — poderia reintroduzir a captura de `__state` antes do gate `ConfigMasterEnabled` (regressão da otimização CR-01-02) achando que está apenas restaurando o "as-built original". Puramente documentação — nenhum código ou comportamento incorreto.

**Sugestão:** Adicionar uma entrada em "Mudanças posteriores" do `007-desmaio-percentual-05-asbuild.md` cobrindo a rodada 1 do code-review: `| Code Review 01 | CR-01-01 (spec técnica §7 — Priority.High=600, regra de ordenação corrigida) + CR-01-02 (Prefix reordenado — captura de __state após o gate ConfigMasterEnabled/IsAlive) | mods/.../007-desmaio-percentual-02-spec-tech.md, mods/.../modded/Patches/Trauma/HealthPatches.cs |`. Pode ser feito manualmente (edição direta, sem precisar re-rodar `/apply-code-review`) já que os fixes já estão no código/spec.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Entrada adicionada em "Mudanças posteriores" do `05-asbuild.md` cobrindo as 2 rodadas de code-review e seus achados.

---

## Veredito

Segunda e última rodada planejada (P-3.7) não encontrou nenhum bug novo de comportamento, gap vs. spec, ou risco de arquitetura de peso no código do item 007. As 4 frentes de auditoria cética pedidas (reordenamento do Prefix, precisão de `float` no roll, paridade bot↔humano, gate de `ConfigVerboseEngineLog`) foram todas confirmadas corretas por leitura direta do código atual — nenhuma delas tinha um bug real, só riscos teóricos já mitigados pela estrutura do código ou já documentados como residual aceito na spec técnica. O único achado (CR-02-01) é de documentação/rastreabilidade (as-built desatualizado com os fixes da rodada 1), não bloqueia o fechamento do item.

**Item 007 pode ser fechado (🟢) após esta rodada.** Nenhum bloqueador em nenhuma das duas rodadas de code-review; o `mod-backlog.md` já reflete 🟢. Recomendo aplicar CR-02-01 (edição de texto, 5 minutos) antes de seguir para o item 008, mas não é impeditivo.

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-19 | Code review 02 criada via `/code-review` (revisor adversarial de contexto limpo, segunda e última rodada planejada — P-3.7): 0 🔴 · 0 🟠 · 0 🟡 · 1 🟢. Confirmado que os 2 achados da rodada 1 (CR-01-01, CR-01-02) estão de fato aplicados no código/spec atuais (não só relatados). Achado novo: CR-02-01 (`05-asbuild.md` não atualizado com os fixes da rodada 1 — documentação/rastreabilidade, não-bloqueador). |
