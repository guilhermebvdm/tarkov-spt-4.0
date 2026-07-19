# 006 — Estômago: agachar probabilístico · Review Técnica 01

**Mod:** TRL-ImmersiveCombatMedicine
**Spec técnica revisada:** [006-estomago-agachar-02-spec-tech.md](006-estomago-agachar-02-spec-tech.md)
**Data:** 2026-07-19

> Análise crítica da spec técnica (rodada 1). Cada ponto recebe um ID `PA-01-MM`. Resolver até zerar bloqueadores antes de `/code-mod`.
>
> `Memória consultada: snapshot de 2026-07-19 (Sessão 3, mods/TRL-ImmersiveCombatMedicine/memory/sessions.md) · pendências que afetam esta review: [P-3.5 — item 003 v1.4.1 ENTREGUE, validação in-game pendente; o 006 reusa a MESMA primitiva TraumaPose/fila de adiados/absorção D2], [P-3.6 — item 004 v1.5.2 ENTREGUE, validação in-game pendente; o 006 reusa AbsorbIfCycleEngaged/IsCycleEngaged], [P-3.4 — diretiva do overhaul 003→008 + rastro de premissas p/ item 011, correta e citada no cabeçalho da própria spec técnica] · nenhuma pendência 🔴 específica do item 006. Os IDs P-3.4/P-3.5/P-3.6 citados no cabeçalho da spec técnica (linha 10) batem com o conteúdo real da memória.`

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 2 · Total: 2

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | B — Edge Case | 🟡 | Cooldown compartilhado não é reservado no `Defer` — janela sem stamp entre o pré-check do 006 e a execução real permite um segundo agachar de pernas na mesma janela | ✅ Resolvido |
| PA-01-02 | C — Erro de Lógica | 🟢 | Citação `MedicalLogic.cs:366` no §1.3/§2 mostra `Random.Range`, não `Random.value` — o idioma citado só existe em `VoiceAndHealthUtils.cs:51` | ✅ Resolvido |

## Categorias

- **A — Gaps de Especificação:** informações ausentes que ambiguam a implementação
- **B — Edge Cases:** cenários válidos não cobertos
- **C — Erros de Lógica:** pressupostos errados, contradições, código incompatível com SPT 4.0+

## Impacto

- 🔴 **Bloqueador** — impede implementar ou causa bug/crash garantido
- 🟡 **Importante** — pode causar comportamento errado em cenário relevante
- 🟢 **Menor** — qualidade/clareza, não bloqueia

## Verificação de evidências (resumo)

Todos os `arquivo.cs:linha` citados na spec técnica (§1, §2, §5, §6, Plugin) foram conferidos contra o estado ATUAL do HEAD (pós-commit `60cf2fcb`, v1.6.0) em `TraumaEngine.cs`, `TraumaEngineState.cs`, `TraumaPose.cs`, `TraumaLegsConsumer.cs`, `TraumaFallCycleConsumer.cs`, `HealthPatches.cs`, `TraumaObservability.cs`, `TraumaLocale.cs` e `TRLImmersiveCombatMedicinePlugin.cs`. **Todas batem exatamente** (linha a linha, incluindo ranges de método completos como `TraumaEngine.cs:127-134`/`:117-122`/`:137-143`, o bloco legado `HealthPatches.cs:98-122` e o padrão de migração de órfã `Plugin.cs:361-382`). Nenhuma linha citada diverge do código real — nenhum achado de Categoria C sobre âncora quebrada. A seção 9 "Conformidade com skills" foi conferida item a item: os 8 checks marcados ✅ têm evidência real e verificável (nenhum `N/A` frágil, nenhum ✅ vazio) — não gera bloqueador automático pela regra do command. O grafo de código não foi necessário além do Grep dirigido: os únicos call sites de `CancelKind`/`BotCrouchDip`/`TryInvoluntaryCrouch`/`KindWord`/`AbsorbIfCycleEngaged` em todo `modded/` são os já listados pela spec (confirmado por grep exaustivo) — nenhum caller esquecido (AP-03 satisfeito: zero patch novo, zero alvo virtual novo).

---

## Pontos

### PA-01-01 · B — Edge Case · 🟡 Importante · ✅ Resolvido em 2026-07-19

**Cooldown compartilhado não é reservado no `Defer` — janela sem stamp entre o pré-check do 006 e a execução real permite dois agachares na mesma janela de anti-thrash**

**Problema:** O consumidor (§1.6/§5, stub `OnTransitionCore`) faz um **pré-check** de `TraumaEngine.TryGetOneShotDeadline(p, InvoluntaryCrouch, out cd)` e só então chama `TraumaPose.TryInvoluntaryCrouch(...)`. Isso é diferente do padrão do MOTOR (`TryPublishOneShot`, `TraumaEngine.cs:588-596`), que **stampa o cooldown no MESMO instante em que decide publicar** (`_cooldownUntil[key] = now + OneShotCooldownSeconds();` — `TraumaEngine.cs:595`), ANTES de o consumidor sequer rodar — logo mesmo que a execução física seja adiada (D7), a reserva já está ativa e protege contra uma segunda tentativa concorrente. O 006, ao pular o barramento `OneShotPublished` por desenho (§1.4, correto — evita acordar o 003), também perde essa reserva atômica: se o roll do estômago tiver sucesso mas cair no caminho `Defer` (D7 — escada/BTR/vault, `TraumaPose.cs:124-128`), **nenhum stamp é gravado** até a execução de fato ocorrer (`Defer`, em `TraumaPose.cs:189-227`, só LÊ o deadline existente via `TryGetOneShotDeadline` para fins de refund futuro — nunca grava um novo). Nessa janela (que pode durar vários segundos, ex. BTR), se as PERNAS zerarem e o motor publicar `InvoluntaryCrouch` via `TryPublishOneShot`, o cooldown dict não mostra nada ativo (chave `(profileId, InvoluntaryCrouch)` compartilhada, `TraumaEngine.cs:27-28`) → publica, stampa, e `TraumaLegsConsumer.OnOneShotCore` executa o agachar de pernas imediatamente (se o contexto de pernas não estiver bloqueado por D7). O agachar de pernas força a pose baixa, então o adiado do estômago, quando seu D7 desbloquear e o pump rodar, tipicamente vira NOOP ("pose already low", `TraumaPose.cs:259-266`) — **mas só enquanto o jogador permanecer agachado**. Como o agachar da primitiva é "só-para-baixo sem lock" (decisão 5, `TraumaPose.cs:10-13`), o jogador pode se levantar voluntariamente entre a execução de pernas e o pump do estômago; nesse caso o adiado do estômago re-executa (`TraumaPose.cs:267-271`) e produz um SEGUNDO agachar físico dentro da mesma janela de 3-5s — violando a AC "Nenhum caso de dois agachares na mesma janela" e o comportamento 6 ("colapsam em um") da spec funcional, especificamente na direção "estômago adiado primeiro, pernas depois" que o próprio AC pede testar ("e vice-versa").

**Por que importa:** É a única lacuna encontrada no mecanismo NOVO que o item 006 introduz (o "peek-then-call" fora do barramento) — 003/004 nunca tiveram esse risco porque só existia UM produtor de `InvoluntaryCrouch` (o motor), sempre com stamp atômico no publish. O gatilho exige D7 ativo no momento do roll do estômago + uma segunda zerada de pernas na janela + o jogador se levantando voluntariamente antes do pump do estômago rodar — não é o caminho comum, mas é exatamente o tipo de corner que o AC pede para ser coberto explicitamente ("e vice-versa"), e o smoke test do §8 ("nunca 2 agachares na janela") não descreve esse sub-caso D7 especificamente.

**Sugestão:** No stub de `TraumaStomachConsumer.OnTransitionCore` (spec §5), após o pré-check de cooldown passar e ANTES de chamar `TraumaPose.TryInvoluntaryCrouch`, reservar o cooldown chamando `TraumaEngine.ReportOneShotExecuted(p, TraumaOneShotKind.InvoluntaryCrouch)` imediatamente (mesma semântica de "stampar na decisão de tentar", espelhando `TryPublishOneShot`). Isso é seguro com os caminhos existentes da primitiva: `AbsorbIfCycleEngaged` e o NOOP de pose-baixa já fazem `TryGetOneShotDeadline` + `ReportOneShotCanceled` para desfazer a reserva quando não executam (`TraumaPose.cs:97-98`/`:119-120`); e `Defer` (`TraumaPose.cs:192-193`) vai capturar ESSA reserva fresca como `PublishDeadline`, preservando-a durante toda a espera do D7 — fechando exatamente a janela descrita acima. Documentar a mudança em §1.6 e adicionar ao smoke do §8 o cenário "roll de estômago adiado por D7 + zerada de pernas na janela → pernas suprimido (log `one-shot SUPPRESSED`), nunca o inverso".

**Decisão:** `[ ]` Pendente · `[x]` Aceitar sugestão · `[ ]` Caminho alternativo (descrever)

**Resolução:** Spec técnica atualizada (§1.6, fluxo de dados §6, stub §5, smoke §8): o consumidor agora chama `TraumaEngine.ReportOneShotExecuted(p, InvoluntaryCrouch)` IMEDIATAMENTE após o pré-check de cooldown passar — antes de chamar `TryInvoluntaryCrouch`/`BotCrouchDip` — reservando a janela atomicamente mesmo quando o roll cai no caminho `Defer` (D7). Os caminhos ABSORB/NOOP já desfazem a reserva via `ReportOneShotCanceled` (código existente do 003/004); `Defer` a herda como `PublishDeadline`. Adicionado cenário de smoke dedicado no §8.

### PA-01-02 · C — Erro de Lógica · 🟢 Menor · ✅ Resolvido em 2026-07-19

**Citação `MedicalLogic.cs:366` no §1.3/§2 mostra `Random.Range`, não `Random.value` — o idioma citado só existe em `VoiceAndHealthUtils.cs:51`**

**Problema:** A spec técnica cita "RNG = `UnityEngine.Random` (padrão do repo — [MedicalLogic.cs:366], [VoiceAndHealthUtils.cs:51])" no §1.3 e na tabela de Pontos de Patch (§2), na linha do row `UnityEngine.Random.value`. Conferido: `modded/Patches/Medical/MedicalLogic.cs:366` é `float penalty = UnityEngine.Random.Range(stats.SurgeryPenaltyMin, stats.SurgeryPenaltyMax);` — usa `Random.Range`, não `Random.value`. O idioma `Random.value` (o que de fato importa para a fórmula do stub, `chance >= 100f || (chance > 0f && Random.value * 100f < chance)`) só está presente em `modded/Helpers/VoiceAndHealthUtils.cs:51-52` (`(UnityEngine.Random.value > 0.5f) ? ...`).

**Por que importa:** Não afeta a implementação (o padrão geral "usar `UnityEngine.Random`, não `System.Random`" é real e válido, sustentado por ambas as citações) — mas um revisor futuro que confira a âncora especificamente pelo texto "`.value`" encontra `.Range` em `MedicalLogic.cs:366` e pode achar que a citação está quebrada, gerando retrabalho de verificação.

**Sugestão:** Trocar a citação de `.value` no §1.3/§2 para apontar só `VoiceAndHealthUtils.cs:51` (que de fato demonstra `Random.value`), mantendo `MedicalLogic.cs:366` como exemplo separado de "`UnityEngine.Random` é o padrão do repo" (sem prometer que é especificamente `.value` ali).

**Decisão:** `[ ]` Pendente · `[x]` Aceitar sugestão · `[ ]` Caminho alternativo (descrever)

**Resolução:** Citações corrigidas em §1.3, §2 e no stub §5 — `MedicalLogic.cs:366` agora anotado como "gênero `UnityEngine.Random` via `Random.Range`, não o idioma `.value`"; `VoiceAndHealthUtils.cs:51` é a única fonte do idioma `.value` citado.

## Histórico

| Data | Evento |
|---|---|
| 2026-07-19 | Review técnica 01 criada via `/review-technical-spec` (sessão anterior interrompida antes de escrever o artefato — refeita do zero). Verificação linha-a-linha de TODAS as âncoras citadas contra o HEAD atual (v1.6.0, pós-005): 100% batem. 2 achados (1 🟡 — janela de reserva de cooldown não-atômica no caminho D7-adiado do 006; 1 🟢 — citação de RNG imprecisa). Zero bloqueadores. |
| 2026-07-19 | PA-01-01 e PA-01-02 resolvidos na spec técnica (reserva atômica do cooldown via `ReportOneShotExecuted` movida para antes da chamada da primitiva; citação de RNG corrigida). Sem 2ª rodada de `/review-technical-spec` — item pequeno, reuso extenso já validado 2x (003/004), único achado 🟡 fechado por mudança de 1 linha auditável no `/code-review` que segue. Spec pronta para `/code-mod`. |
