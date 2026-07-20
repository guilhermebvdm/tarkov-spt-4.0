# 007 — Desmaio 2.0: gatilhos percentuais · Review Técnica 01

**Mod:** TRL-ImmersiveCombatMedicine
**Spec técnica revisada:** [007-desmaio-percentual-02-spec-tech.md](007-desmaio-percentual-02-spec-tech.md)
**Data:** 2026-07-19

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM` (review 01, ponto MM). Resolver até zerar bloqueadores antes de `/code-mod`.
>
> Memória consultada: snapshot de 2026-07-19 (Sessão 4, `mods/TRL-ImmersiveCombatMedicine/memory/sessions.md`). Pendências P-2.13/P-2.14/P-2.15 revisadas como barra de rigor (bugs históricos de timing/sincronização do pipeline de desmaio — relógio único quebrado por recálculo ao vivo, guard de re-entrada, sync Fika) — nenhuma reintroduzida por esta spec: a spec não toca `BlackoutTimers`/`BlackoutStartTimes`/`FikaBridge.SyncFaintStatus`, nem lê `ConfigBlackoutDuration` fora do ponto único já existente. P-3.7 confirma este é item de maior risco do overhaul restante — 2 rodadas de review previstas, esta é a rodada 1.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 3 · Total: 3

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C — Erro de Lógica | 🟡 Importante | Evidência de `GetBodyPartHealth` citada na classe errada (`GClass921`/mirror stub, não `ActiveHealthController`) | ✅ Resolvido |
| PA-01-02 | A — Gap | 🟢 Menor | Interação Prefix-escudo × captura de `__state`: `Evaluate` nunca é de fato chamado nesse caminho (não documentado) | ✅ Resolvido |
| PA-01-03 | A — Gap | 🟢 Menor | Corner case "hit simultâneo tórax+cabeça" da spec funcional não tem traço explícito no §7 da spec técnica | ✅ Resolvido |

## Categorias

- **A — Gaps de Especificação:** informações ausentes que ambiguam a implementação
- **B — Edge Cases:** cenários válidos não cobertos
- **C — Erros de Lógica:** pressupostos errados, contradições, código incompatível com SPT 4.0+

## Impacto

- 🔴 **Bloqueador** — impede implementar ou causa bug/crash garantido
- 🟡 **Importante** — pode causar comportamento errado em cenário relevante
- 🟢 **Menor** — qualidade/clareza, não bloqueia

---

## Pontos

### PA-01-01 · C — Erro de Lógica · 🟡 Importante

**Evidência de `GetBodyPartHealth` citada na classe errada**

**Problema:** A spec técnica §2 afirma: *"`GetBodyPartHealth` [...] com assinatura confirmada em [`GClass921.cs:1143`] (`public ValueStruct GetBodyPartHealth(EBodyPart bodyPart, bool rounded = false)`)"*. A linha existe e o texto bate literalmente — mas `GClass921` **não é** `ActiveHealthController` (a classe que o código do mod realmente chama via `player.ActiveHealthController.GetBodyPartHealth(...)`). Pela tabela de deofuscação (`docs/files-from-4.1/consolidated-mappings.txt:5388`), `GClass921 -> ObservedPlayerHealthController` — o controller do **espelho/observado** (usa `ObservedPlayerView_0`/`ObservedCorpse_0` nos campos, `GClass921.cs:24-27`). Pior: a implementação específica de `GetBodyPartHealth` **nessa classe é um stub que lança exceção** — `GClass921.cs:1145`: `throw new NotImplementedException();`. `ActiveHealthController` (a classe real invocada pelo Prefix/`TraumaBlackoutTrigger`) **não existe no dump local** (confirmado por grep — 0 arquivos `class ActiveHealthController` em `references/eft-decompiled/`), então a assinatura citada não pode ter vindo de lá; ela bate por coincidência porque `GetBodyPartHealth` é membro da interface `IHealthController` (também sem arquivo próprio no dump) e é reimplementado em várias classes concretas — inclusive `GClass2182`/`EFT.HealthInfoAdapter` (`GClass2182.cs:88`), que tem corpo real (`return ValueStruct_3;`), ao contrário do stub de `GClass921`.

**Por que importa:** Quem seguir a citação `arquivo:linha` esperando confirmar o comportamento de `ActiveHealthController.GetBodyPartHealth` encontra uma classe completamente diferente (mirror-only) cujo próprio método, se chamado, lançaria `NotImplementedException`. Isso não invalida a decisão técnica — a spec **já tem** evidência real e válida em outro lugar: `docs/trauma-primitives.md §P7`, seção "Provas por protótipo", prova por compilação real (`ilspycmd_assembly_real`) que `ahc.GetBodyPartHealth(EBodyPart).Current/.Maximum` compila com 1 argumento contra o `Assembly-CSharp.dll` real, onde `ahc` é `ActiveHealthController`. O problema é puramente de **citação** (AP-09 — dump incompleto para `ActiveHealthController`, mas a spec não sinalizou a lacuna nem substituiu por uma citação correta), e o risco é propagar essa citação errada para specs futuras (008, 011) que reusem o mesmo trecho.

**Sugestão:** Trocar, em §2, a citação `[GClass921.cs:1143]` (e a frase "assinatura confirmada em") por uma referência a `docs/trauma-primitives.md §P7` — "Provas por protótipo" (compilação real via `ilspycmd_assembly_real`, `ActiveHealthController` não presente no dump local — AP-09). Opcionalmente adicionar uma nota de rodapé explicando que `GClass921` (citado por engano) é na verdade `ObservedPlayerHealthController` — controller do espelho, com esse método stub — para blindar contra reuso incorreto por specs futuras que copiarem o trecho.

**Decisão:** `[x]` Aceitar sugestão

**Resolução:** Citação trocada em §2 e nos 2 comentários inline dos stubs (§5) para `docs/trauma-primitives.md §P7`, com nota explicando que `GClass921.cs:1143` (citado por engano) é `ObservedPlayerHealthController` (stub, mirror-only).

---

### PA-01-02 · A — Gap · 🟢 Menor

**Interação Prefix-escudo × captura de `__state`: `Evaluate` nunca chega a ser chamado nesse caminho**

**Problema:** O Prefix (stub §5) captura `__state` **antes** do bloco do escudo de dano (`if (!__instance.IsAI && FaintedPlayerIds.Contains(...)) { if (validDamageType) return false; }`). A spec não erra tecnicamente, mas também não documenta que, quando esse escudo dispara, o Postfix **nunca chega a chamar `TraumaBlackoutTrigger.Evaluate`** — porque o próprio Postfix tem, ANTES do cálculo de `shouldFaint`, o guard `if (TraumaState.BlackoutTimers.ContainsKey(id) || TraumaState.FaintedPlayerIds.Contains(id)) return;` (`HealthPatches.cs:57` no arquivo atual, preservado no stub da spec em `Postfix`). Como o escudo do Prefix só dispara quando `FaintedPlayerIds.Contains(id)` já é verdadeiro, essa MESMA condição sempre faz o Postfix retornar **antes** de chamar `Evaluate` — ou seja, o cenário "Postfix roda com `__state` = HP pré-hit mas HP real nunca mudou" nunca alcança `Evaluate` para computar `effectiveDamage`. A interação é segura, mas não pelo motivo que uma leitura ingênua da spec sugeriria ("`Evaluate` trata `effectiveDamage <= 0` corretamente") — é segura porque `Evaluate` é código morto nesse caminho específico.

**Por que importa:** Um futuro mantenedor lendo apenas a spec (sem rastrear os dois guards em conjunto) pode concluir que `Evaluate` é o mecanismo de segurança para esse cenário — e ao alterar o guard do Postfix (ex.: no item 008 ou numa correção futura) sem perceber esse acoplamento implícito, poderia reintroduzir o cenário sem querer, contando com uma proteção (`effectiveDamage <= 0f`) que de fato existe em `Evaluate`, mas que não é a que garante a segurança HOJE.

**Sugestão:** Adicionar uma frase ao §6 (Fluxo de dados), passo [D], explicitando: *"Nota: quando o escudo de dano do Prefix bloqueia o hit (`FaintedPlayerIds.Contains(id)`), o Postfix já retorna antes de chamar `Evaluate` (mesmo guard `FaintedPlayerIds.Contains(id)` no topo do bloco de desmaio) — `Evaluate` nunca é invocado com HP pré/pós-hit idênticos nesse caminho; se esse guard for alterado no futuro, `Evaluate` também trata `effectiveDamage <= 0f` como rede de segurança secundária."*

**Decisão:** `[x]` Aceitar sugestão

**Resolução:** Nota adicionada ao §6, passo [D], explicitando o acoplamento entre os dois guards.

---

### PA-01-03 · A — Gap · 🟢 Menor

**Corner case "hit simultâneo tórax+cabeça no mesmo frame" sem traço explícito no §7**

**Problema:** A spec funcional (`007-desmaio-percentual-01-spec.md`, seção Corner cases) lista, ainda **não marcado** (`[ ]`, não `[x]`): *"Hit simultâneo em tórax E cabeça no mesmo frame [...] confirmar que a ORDEM de avaliação entre as duas regiões no mesmo Postfix não produz dois disparos de `BlackoutTimers[id] = ...`"*. A spec técnica não tem nenhuma seção que trace esse cenário explicitamente (nem em §6 Fluxo de dados, nem em §7 Riscos). O mecanismo que resolve o caso está de fato intacto e correto — cada `bodyPartType` (Chest, Head) gera sua própria chamada de `ApplyDamageInfo`/Prefix/Postfix (uma por hit, confirmado em §1 "sem agregação de pellets"); a PRIMEIRA chamada a ter sucesso escreve `BlackoutTimers[id] = now + duration`, e a guard já existente no topo do bloco de desmaio (`if (TraumaState.BlackoutTimers.ContainsKey(id) [...]) return;`, inalterada por este item) faz a SEGUNDA chamada retornar antes mesmo de chamar `Evaluate` — sem sobrescrita de deadline. Mas essa análise não está escrita em lugar nenhum da spec técnica.

**Por que importa:** É exatamente o tipo de corner case que a spec funcional pede para ser fechado tecnicamente antes do `/code-mod` (convenção do repo: a spec técnica deve responder aos critérios/corners da funcional). Sem o traço explícito, o item fica sem uma "prova escrita" a que apontar durante `/code-review` ou validação in-game — force um leitor futuro a re-derivar o raciocínio do zero.

**Sugestão:** Adicionar ao §7 (Riscos e dependências) um parágrafo curto confirmando o corner case: citar que cada região (Chest/Head) gera uma chamada independente de `ApplyDamageInfo`, que a primeira escrita em `BlackoutTimers[id]` faz a guard existente (`BlackoutTimers.ContainsKey(id)`) bloquear a segunda chamada antes de `Evaluate`, e que portanto não há dupla escrita/sobrescrita de deadline — fechando o corner case aberto na spec funcional (que pode então ser marcado `[x]` com referência a este parágrafo).

**Decisão:** `[x]` Aceitar sugestão

**Resolução:** Parágrafo adicionado ao §7; corner case correspondente marcado `[x]` na `01-spec.md`.

---

## Pontos verificados sem achado (auditoria rigorosa, sem contra-evidência)

- **Decisão técnica central (Prefix + `__state` vs. aritmética reversa):** confirmado linha por linha contra o assembly real — `Player.cs:25289` (`HealthController`), `:25291` (`ActiveHealthController`), `:30404` (`ApplyShot`), `:30432` (chamada a `ApplyDamageInfo`), `:30463` (assinatura de `ApplyDamageInfo`, `virtual`), `:30475` (`DoWoundRelapse`, sem mutação de HP), `:30480` (`ActiveHealthController.ApplyDamage` — aqui o HP muda). Todas as linhas batem exatamente com o texto da spec. `GameWorld.cs:1966` (`ShotDelegate`), `BodyPartCollider.cs:44` (`PlayerBridge.ApplyShot`) e `:324` (`ApplyHit`) também conferidos linha a linha — corretos.
- **Ordem de patches Harmony entre mods (`__state` isolado por-patch):** mecânica confirmada — Harmony gera o local de `__state` por método de patch (não é campo compartilhado entre patches de mods diferentes no mesmo alvo); `BringBackConcussion.dll`/`VisceralCombat.dll` declarando seus próprios parâmetros não tem qualquer interferência com o `__state` do nosso Prefix. Não é uma decisão de design do mod — é garantia do framework Harmony, não carece de documentação na spec.
- **Constantes de probabilidade fixas (50/25/0%) vs. spec funcional:** confirmado — a spec funcional (`01-spec.md`, critério "Os quatro números do gatilho [...] são configuráveis") lista exatamente os 4 números (2 percentuais + 2 pisos) como `ConfigEntry`, sem qualquer menção a tornar as probabilidades de roll configuráveis. Sem ambiguidade — não é gap.
- **Decisão de não registrar em `TraumaConsumerRegistry`:** confirmado sem consequência prática — a spec funcional (`01-spec.md` inteira, seção Comportamento desejado + Critérios de aceite + Corner cases) não menciona toast/observabilidade para o item 007 (grep por "toast" = 0 ocorrências), e `TraumaConsumerRegistry.AnyActiveFor` só gateia toasts por `TraumaRegion` (`Legs/Arms/Stomach`, `TraumaEngineState.cs:9`) — domínio que não cobre tórax/cabeça por design (`TraumaEngineState.cs:8`, comentário explícito: "Desmaio [...] é EVENTO — domínio do item 007, fora do motor de estados"). `TraumaConsumerId.Blackout2` existe no enum (`TraumaEngineState.cs:61`) e o comentário do `Register` (`:129`) já cita "Blackout2/007" como o caso de "consumidor sem região de estado" — a decisão bate com o código real.
- **Piso absoluto vs. percentual — ordem de avaliação:** confirmado comutativo — são duas checagens independentes (`effectiveDamage < absFloor` / `effectiveDamage < pctThreshold * preHitHp`); qualquer ordem produz o resultado final correto (nenhum roll) sempre que qualquer uma falha, e o log da razão ("piso absoluto" / "percentual") é sempre uma afirmação verdadeira sobre a checagem específica que reprovou primeiro — não há caso em que a ordem produza um log de motivo incorreto. O AC da spec funcional (hit 62,5%/5 dano abaixo do piso 25 → log "piso", não "percentual") é satisfeito pela ordem atual (piso primeiro).
- **`ConfigVerboseEngineLog`:** confirmado existente com esse nome exato — `TRLImmersiveCombatMedicinePlugin.cs:44` (declaração) e `:123` (`Config.Bind("5. Trauma 2.0 (Motor)", "Verbose Engine Log", false, ...)`). Reuso correto.
- **AP-03 (auditoria de overrides de `Player.ApplyDamageInfo`):** re-verificado independentemente — `NetworkPlayer.cs`, `HideoutPlayer.cs`, `LocalPlayer.cs` (0 ocorrências de `ApplyDamageInfo`/`ApplyShot` nos 3 arquivos, presentes no dump local, não é caso de AP-09). No Fika: `FikaPlayer.cs:673-687` chama `base.ApplyDamageInfo` (confirmado); `FikaBot.cs:246-252` chama `base.ApplyDamageInfo` (confirmado); `ObservedPlayer.cs:570-577` **não** chama base (confirmado, só seta `Last*`) — patch inerte no espelho, dono-only por construção. Bate exatamente com a auditoria da spec em §9 check 3.

---

## Status

Nenhum bloqueador 🔴. Os 3 achados (1🟡 + 2🟢) foram aplicados na spec técnica (correções de citação/documentação, sem mudança de estratégia ou stubs de código). Rodada 2 de review técnica prevista pelo plano do item (risco do pipeline de desmaio) segue como próximo passo.
