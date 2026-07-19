# 006 — Estômago: agachar probabilístico · Spec Técnica

**Mod:** TRL-ImmersiveCombatMedicine
**Status:** Em progresso (aguardando review)
**Spec funcional:** [006-estomago-agachar-01-spec.md](006-estomago-agachar-01-spec.md)
**Criado:** 2026-07-19

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/) (102 namespaces vazios — ausência ali NÃO é evidência; membro novo confirma-se por ilspycmd na DLL real). Segunda fonte canônica: o código ENTREGUE do Trauma 2.0 v1.6.0 ([TraumaEngine.cs](../../modded/Patches/Trauma/TraumaEngine.cs), [TraumaPose.cs](../../modded/Patches/Trauma/TraumaPose.cs), [TraumaLegsConsumer.cs](../../modded/Patches/Trauma/TraumaLegsConsumer.cs), [TraumaFallCycleConsumer.cs](../../modded/Patches/Trauma/TraumaFallCycleConsumer.cs)) — assinaturas citadas por `arquivo:linha` do próprio mod. Este item NÃO cria patch Harmony novo e NÃO toca o motor: é 100% consumidor + primitiva já entregue.
>
> `Memória consultada: snapshot de 2026-07-19 (Sessão 3) · pendências que afetam: [P-3.5/P-3.6 — itens 003 (v1.4.1) e 004 (v1.5.2) entregues, VALIDAÇÃO IN-GAME PENDENTE: o 006 reusa exatamente a primitiva/fila/absorção desse código], [P-3.4 — diretiva do overhaul 003→008 + rastro de premissas p/ item 011; a premissa "dedup pode virar chave (player,kind,region) na spec do 006" registrada no P-3.5 é ADOTADA aqui] / nenhuma 🔴`

## 1. Estratégia

**Quarto consumidor do motor 002 — o menor da matriz: 1 arquivo novo, 3 modificados, ZERO patch Harmony novo, ZERO mudança no motor. O roll nasce no consumidor; o agachar reusa a primitiva do 003 por chamada DIRETA (nunca pelo barramento de one-shot do motor).**

1. **Motor: NADA a criar.** A transição da linha de estômago JÁ é publicada com o analgésico LATCHED do instante da detecção da zerada (D8): `EvaluatePlayer` deriva `StomachZeroed` ([TraumaEngine.cs:532](../../modded/Patches/Trauma/TraumaEngine.cs)), congela `rec.StomachPainkillerAtEntry` na ENTRADA e publica a transição com `PainkillerActive` = valor latched (:554-561); o replay establishing do `SubscribeWithSnapshot` também entrega o latched (:86-88). Mudança de analgésico NUNCA marca estômago dirty (:477 — latch D8; reconciliação idem, :643-647) e `From==To` não publica (:548) → **toda transição real para `StomachZeroed` é uma zerada NOVA** (re-roll da decisão 7 sai de graça). O motor NÃO publica one-shot de estômago (:567-573 — só linhas de pernas) e NÃO rola probabilidade ([TraumaEngineState.cs:29](../../modded/Patches/Trauma/TraumaEngineState.cs) declara o roll como entrega do 006). Infra pronta sem call site: `TraumaObservability.LogRoll` ([TraumaObservability.cs:41-46](../../modded/Patches/Trauma/TraumaObservability.cs)), texto do toast EN/PT ([TraumaLocale.cs:21/:32](../../modded/Patches/Trauma/TraumaLocale.cs), mapeado em :71), `TraumaConsumerId.StomachEffects` ([TraumaEngineState.cs:61](../../modded/Patches/Trauma/TraumaEngineState.cs)) e o placeholder de config ([Plugin :135-136](../../modded/TRLImmersiveCombatMedicinePlugin.cs)).
2. **Consumidor `TraumaStomachConsumer`** (MonoBehaviour no GO do plugin, padrão 003/004/005): registra `StomachEffects` p/ `Stomach` no registry (destrava o toast — decisão 20; o toast é gate do MOTOR em [TraumaObservability.cs:57-77](../../modded/Patches/Trauma/TraumaObservability.cs), dispara na ENTRADA da linha independente do roll — funcional §10) e assina `SubscribeWithSnapshot` ([TraumaEngine.cs:72](../../modded/Patches/Trauma/TraumaEngine.cs)). **NÃO assina `OneShotPublished`** — o 006 não consome nem produz eventos desse barramento (item 3). `IsActive()` = master legado + master Trauma 2.0 + toggle próprio, **SEM gate de headless** (diferença deliberada do 005: bots estão INCLUSOS — decisão 11 — e o headless é dono dos bots; o toast já é local-only pelo gate `IsYourPlayer` do motor, :65). Entry points com try/catch (CR-01-04 do 004 — exceção de consumidor não pode matar o `StateChanged?.Invoke`).
3. **Roll no consumidor, na transição REAL de entrada** (`Region==Stomach && To==StomachZeroed && !Establishing`): p vem do config pelo **analgésico LATCHED que a própria transição carrega** (`t.PainkillerActive` — nunca re-consulta `IsUnderPainkiller`; contrato do 002/corner da funcional). RNG = `UnityEngine.Random` (gênero é o padrão do repo — [MedicalLogic.cs:366](../../modded/Patches/Medical/MedicalLogic.cs) via `Random.Range`; o idioma `.value` usado aqui está em [VoiceAndHealthUtils.cs:51](../../modded/Helpers/VoiceAndHealthUtils.cs) — PA-01-02) com **extremos determinísticos** (AC1): `success = chance >= 100f || (chance > 0f && Random.value * 100f < chance)` — `Random.value` é inclusivo em 1.0, sem o curto-circuito p=100 poderia falhar e p=0 poderia suceder. TODO roll é logado via `LogRoll` (call site que faltava — [TraumaObservability.cs:41](../../modded/Patches/Trauma/TraumaObservability.cs)) com condition `zeroed`/`zeroed-pk` e p normalizado 0-1. Establishing (spawn/religar/adoção): **sem roll, sem efeito, sem toast** (o motor nem toasta establishing, :62). Saída da linha (`To==None`): nenhuma ação no consumidor — one-shot puro, não há efeito contínuo; adiado pendente morre sozinho na re-validação do pump (item 5).
4. **Publicação/consumo SEM vazamento — chamada DIRETA da primitiva, fora do pipeline `OneShotPublished`:** roll com sucesso → humano `TraumaPose.TryInvoluntaryCrouch(p, TraumaRegion.Stomach, TraumaOneShotKind.InvoluntaryCrouch)`; bot `TraumaPose.BotCrouchDip(p, TraumaRegion.Stomach)`. Motivo: o consumidor de pernas reage a QUALQUER `InvoluntaryCrouch` publicado sem discriminar região ([TraumaLegsConsumer.cs:130](../../modded/Patches/Trauma/TraumaLegsConsumer.cs)) — publicar no barramento faria o 003 executar o agachar do estômago (quebra a independência bidirecional, funcional §7). Com a chamada direta o vazamento é impossível POR CONSTRUÇÃO nas duas direções: o 006 nunca publica (003 nunca vê) e nunca assina (nunca executa one-shot de pernas). **A primitiva funciona standalone — o publish/deadline NÃO é obrigatório:** todos os toques de cooldown dela são stamp-guarded — `TryGetOneShotDeadline` sem entrada devolve false (refund vira no-op, [TraumaEngine.cs:137-143](../../modded/Patches/Trauma/TraumaEngine.cs)), `ReportOneShotCanceled` só remove com stamp idêntico (:127-134) e `ReportOneShotExecuted` stampa incondicional (:117-122). Único resíduo: um stamp EXPIRADO esquecido no dict pode ser capturado/removido pelos caminhos NOOP/ABSORB/Defer — remoção de entrada morta, semanticamente nula (§7, risco 1). O handshake correto do 006 é: **pré-checar o cooldown ANTES de chamar** (item 6) e deixar a EXECUÇÃO stampar via `ReportOneShotExecuted` (já embutido na primitiva — [TraumaPose.cs:134/:270/:405](../../modded/Patches/Trauma/TraumaPose.cs)).
5. **Corner do dedup (funcional §4) — chave da fila vira `(player, kind, region)`:** hoje o dedup do `Defer` casa por `(player, kind)` ([TraumaPose.cs:195-220](../../modded/Patches/Trauma/TraumaPose.cs)) — um adiado de PERNAS que recebe por cima a intenção do ESTÔMAGO viraria UMA entrada re-alvejada, e a cura da região apontada cancelaria TAMBÉM a intenção da outra ainda válida. Mecanismo escolhido: **adicionar `e.Region == region` à condição de match** — as duas intenções coexistem como entradas separadas, cada uma re-validando a PRÓPRIA linha no pump (`GetLine(p, e.Region) != e.RequiredLine` cancela SÓ ela, :243-249); quando a primeira executa (pose 0), a segunda cai no **NOOP natural pós-execução** (`pose already low`, :259-266) com refund stamp-guarded — o cooldown re-ancorado pela execução sobrevive (stamp difere). Escolhido sobre a re-validação multi-região porque muda MENOS a fila do 004 já entregue: struct `DeferredCrouch`, pump, refunds e `Internal` ficam INTOCADOS (quedas são sempre `Region=Legs` — o comportamento do 004 é bit-idêntico); as únicas mudanças são a condição de match e o cancel por kind ganhar região: `CancelKind(kind, region, reason)` ([TraumaPose.cs:342-353](../../modded/Patches/Trauma/TraumaPose.cs)) com o call site do 003 atualizado para `Legs` ([TraumaLegsConsumer.cs:212](../../modded/Patches/Trauma/TraumaLegsConsumer.cs)) — fecha também o corner do toggle-off (funcional: desligar o 006 cancela SÓ adiados do estômago). O fix CR-02-03 (`e.Region = region` no hit de dedup, :209-213) vira no-op inofensivo no cross-region (nunca mais há hit cross-region) e continua correto no re-publish da MESMA região.
6. **Cooldown compartilhado por (player, kind=InvoluntaryCrouch) — mantido (decisão 19), reserva ATÔMICA (PA-01-01):** ANTES de chamar a primitiva, o consumidor consulta `TraumaEngine.TryGetOneShotDeadline(p, InvoluntaryCrouch, out d)`; `d > Time.time` → **supressão LOGADA** (`stomach-crouch SUPPRESSED (cooldown)`) e NÃO re-tenta (a zerada é evento único — funcional §6); espelha a ordem do motor, que também checa cooldown antes de qualquer consumo ([TraumaEngine.cs:590-594](../../modded/Patches/Trauma/TraumaEngine.cs)). **Com o pré-check passando, o consumidor chama `TraumaEngine.ReportOneShotExecuted(p, InvoluntaryCrouch)` IMEDIATAMENTE — antes de chamar a primitiva, não depois:** o motor reserva o cooldown no INSTANTE da decisão de publicar (`TryPublishOneShot`, :595), não na execução física; sem essa reserva antecipada, um roll de estômago bem-sucedido que caia no caminho `Defer` (D7 — escada/BTR/vault, `TraumaPose.cs:124-128`) ficaria sem stamp durante toda a espera do D7, permitindo que uma zerada de pernas na mesma janela publicasse e executasse livremente — e se o jogador se levantasse voluntariamente antes do pump do estômago rodar, o adiado do estômago re-executaria, produzindo dois agachares na mesma janela (violação do "colapsam em um", especificamente na direção estômago-primeiro que o AC pede testar "e vice-versa"). A reserva antecipada é seguramente desfeita pelos caminhos que NÃO executam: `AbsorbIfCycleEngaged` e o NOOP de pose-baixa já chamam `TryGetOneShotDeadline` + `ReportOneShotCanceled` para devolver o cooldown quando não executam (`TraumaPose.cs:97-98`/`:119-120`); e `Defer` (`TraumaPose.cs:192-193`) captura ESSA reserva fresca como `PublishDeadline`, preservando-a durante toda a espera do D7. A execução real (bem-sucedida, imediata ou pós-pump) chama `ReportOneShotExecuted` de novo dentro da primitiva — idempotente, apenas re-ancora o timestamp — e é essa re-ancoragem que suprime o próximo publish de `InvoluntaryCrouch` de PERNAS dentro da janela PELO MOTOR (:590-594, log `one-shot SUPPRESSED (cooldown)`). Rajada que zera 2 pernas E estômago no MESMO frame: a ordem determinística Legs→Arms→Stomach (:541) faz o publish de pernas stampar primeiro (:595) → o roll do estômago no mesmo frame é suprimido → **um agachar só** (funcional §6, "colapsam em um"). `InvoluntaryFall` não é afetado (cooldown por kind, :27-28).
7. **Arbitragem D2 — já entregue, agora exercitável:** `AbsorbIfCycleEngaged` no topo de `TryInvoluntaryCrouch` e `BotCrouchDip` ([TraumaPose.cs:94-101/:115/:382](../../modded/Patches/Trauma/TraumaPose.cs)) absorve com refund + log ABSORB quando o ciclo do 004 está engajado (humano em qualquer fase, bot em hold — [TraumaFallCycleConsumer.cs:49-55](../../modded/Patches/Trauma/TraumaFallCycleConsumer.cs)); o refund é no-op inofensivo no caminho do 006 (sem publish). A nota "o `crouch ABSORB` por estômago só é exercitável no 006" (PA-01-09 do 004) morre junto com o legado. Ordem no consumidor: cooldown ANTES da primitiva (logo antes da absorção) — decisão registrada (§7, abertura 1). Prone/agachado atual = NOOP com refund (contrato do 003, :117-123). Desmaio/downed no frame do roll (corner da funcional): motor não publica p/ `!IsAlive` ([TraumaEngine.cs:511-516](../../modded/Patches/Trauma/TraumaEngine.cs)) — downed Fika nunca chega a rolar; blackout legado (vivo + inconsciente) força prone na entrada ([HealthPatches.cs:84](../../modded/Patches/Trauma/HealthPatches.cs)) e por frame no MainLoopPatch → o agachar cai no **NOOP pose-baixa** logado — nenhum caminho força pose em inconsciente, sem gate novo.
8. **Bots inclusos (decisão 11):** mesmo roll, mesmo log (o motor rastreia bots do dono — host/headless); sucesso → `BotCrouchDip(p, TraumaRegion.Stomach)` (fire-and-forget, nunca entra na fila — [TraumaPose.cs:370-408](../../modded/Patches/Trauma/TraumaPose.cs)); bot em hold do 004 → ABSORB pelo mesmo topo (:382). O parâmetro de região é NOVO e opcional (`= TraumaRegion.Legs`) — call site do 003 (:134) intocado; usado só p/ o log word (item 10).
9. **Legado inerte (D10):** o bloco "sem ar" de `HealthPatches.cs:98-122` é REMOVIDO por inteiro — stamina zerada (:117), pose forçada (:118) e voz "Gut" (:119), **inclusive para bots** (o Postfix roda p/ qualquer dono, sem filtro de IA), e junto morre o guard PA-01-09 (:110-114, que só existia p/ esse bloco). Substituído por comentário-lápide (padrão dos blocos de pernas/braços, :123-128). A key `Sistema de Estomago` fica com tooltip INERTE (padrão :83-85 — remoção no 010); `ConfigStomachEnabled` não tem NENHUM outro consumidor (grep: só o bind :86 e o bloco removido). O case `"Gut"` do `VoiceHelper` ([VoiceAndHealthUtils.cs:53](../../modded/Helpers/VoiceAndHealthUtils.cs)) fica sem referência — remoção é do 010, não deste item. O agachar novo NÃO ganha voz (paridade com o agachar silencioso do 003 — decisão da funcional; premissa p/ 011).
10. **Observabilidade sem quebrar greps do 003/004:** `KindWord` ([TraumaPose.cs:61-62](../../modded/Patches/Trauma/TraumaPose.cs)) ganha a região e devolve `"stomach-crouch"` p/ `(InvoluntaryCrouch, Stomach)` — os logs da fila/primitiva (DEFERRED/CANCELED/NOOP/EXECUTED/ABSORB) saem distinguíveis por região SEM alterar um byte dos formatos de pernas/queda (`crouch ...`/`fall ...` idênticos). Formatos novos/reusados na tabela do §6. Logs `bot dip ...` ficam com formato inalterado — correlação com o roll imediatamente anterior identifica a origem (abertura 4).
11. **Config seção `10. Trauma 2.0 (Estômago)`** (a 9 é dos braços — [Plugin :170-181](../../modded/TRLImmersiveCombatMedicinePlugin.cs)): 2 sliders 0-100 (defaults 75/25), **independentes entre si — sem clamp; inverter é permitido** (premissa p/ 011; diferente do min(N2,N1) do 003, que protege invariante de severidade — aqui não há invariante). Rename-at-delivery `Stomach Effects (item 006)` → `Stomach Effects` (nasce ON; órfã DELETADA sem copiar valor em `MigrateOrphanedConfigKeys` — 4º bloco, padrão :361-382 e lição CR-03-01). **Versão da entrega: 1.7.0** ([Plugin :17/:73](../../modded/TRLImmersiveCombatMedicinePlugin.cs)).
12. **Lifecycle do consumidor:** quase-stateless (sem efeito contínuo, sem dict próprio) — `Update` só rastreia `_trackedWorld`/`_wasActive` (padrão 003/004): raid-end/world-swap e toggle-off → `CancelKind(InvoluntaryCrouch, Stomach, reason)` (limpa SÓ as próprias entradas; o `CancelAll` do componente 003 no raid-end já cobriria — a chamada própria é ownership explícito, idempotente); religar mid-raid → NADA a estabelecer (one-shot puro; estômago já zerado no religar NÃO rola — paridade establishing, funcional AC de estado). **Com o toggle ativo, pumpa `TraumaPose.PumpDeferred()` e `TraumaPose.PumpBotRestores()`** — obrigatório p/ a independência bidirecional (funcional §7): com 003 E 004 OFF, ninguém mais pumparia o adiado D7 do estômago nem devolveria o dip de bot; ambos os pumps são idempotentes/inofensivos com múltiplos chamadores (pump por frame [TraumaPose.cs:236-237](../../modded/Patches/Trauma/TraumaPose.cs); restores removem por deadline). Toggle-off NÃO flusha dips de bot (auto-expiram em ≤1.5s; flushar varreria dips de pernas — abertura 3).

**Alternativas descartadas:** (a) **publicar `InvoluntaryCrouch` no barramento do motor** — acorda o 003 (:130 não discrimina região; vazamento direto) e exigiria o 003 filtrar por linha/região = mexer em consumidor entregue e fragilizar o barramento p/ consumidores futuros; (b) **kind novo `StomachCrouch`** — quebra o contrato do cooldown compartilhado (chave é `(profileId, kind)`, [TraumaEngine.cs:27-28](../../modded/Patches/Trauma/TraumaEngine.cs)): manter a supressão cruzada exigiria acoplamento cross-kind DENTRO do motor (mudança de motor proibida pelo escopo) — e o publish continuaria acordando qualquer assinante; (c) **rolar no motor** — a funcional fixa o roll como entrega do 006 (o 002 publica estado, não decide efeito); (d) **re-validação multi-região da entrada mesclada** — muda struct + pump + bookkeeping de refund da fila do 004 (mais superfície) p/ o mesmo resultado que a chave por região dá com 2 linhas; (e) **re-consultar `IsUnderPainkiller` no roll** — viola o latch D8 (o instante canônico é a detecção da zerada; `t.PainkillerActive` JÁ é o valor certo); (f) **`System.Random` próprio** — sem ganho (não precisamos de seed/determinismo entre processos; peers não comparam rolls — D16) e fora do padrão do repo; (g) **gate de headless no `IsActive` (padrão 005)** — mataria o roll dos bots no dono deles (decisão 11 exige headless funcionando).

## 2. Pontos de patch

**ZERO patch Harmony novo; ZERO alteração de patch existente** (a mudança em `HealthPatches.cs` é REMOÇÃO de bloco dentro do Postfix já patchado — o alvo `Player.ApplyDamageInfo` e o shape do patch ficam intocados). Tudo é hook C# interno:

| Hook C# (sem patch) | Assinatura / âncora | Uso |
|---|---|---|
| `TraumaEngine.SubscribeWithSnapshot` | [TraumaEngine.cs:72](../../modded/Patches/Trauma/TraumaEngine.cs) | Entrada/saída da linha de estômago; replay establishing entrega `PainkillerActive` latched (:86-88) |
| `TraumaTransition.PainkillerActive` | [TraumaEngineState.cs:72](../../modded/Patches/Trauma/TraumaEngineState.cs) — p/ `StomachZeroed` é o valor LATCHED da entrada (D8; escrito em [TraumaEngine.cs:554-561](../../modded/Patches/Trauma/TraumaEngine.cs)) | Fonte ÚNICA do analgésico do roll — sem re-consulta |
| `TraumaEngine.TryGetOneShotDeadline` | [TraumaEngine.cs:137-143](../../modded/Patches/Trauma/TraumaEngine.cs) | Pré-check do cooldown compartilhado (supressão logada) — mesmo uso do 004 ([TraumaFallCycleConsumer.cs:103](../../modded/Patches/Trauma/TraumaFallCycleConsumer.cs)) |
| `TraumaPose.TryInvoluntaryCrouch` | [TraumaPose.cs:105-136](../../modded/Patches/Trauma/TraumaPose.cs) — absorção D2 :115, NOOP pose-baixa :117-123, guards D7→Defer :124-128, `ReportOneShotExecuted` na execução :134 | Agachar do humano — chamada DIRETA com `region=Stomach` (nunca via `OneShotPublished`) |
| `TraumaPose.BotCrouchDip` | [TraumaPose.cs:372-408](../../modded/Patches/Trauma/TraumaPose.cs) — absorção :382, NOOP :391-398, stamp :405; ganha `region` opcional (default `Legs`) | Agachar de bot (dip) — host/headless (dono dos bots) |
| `TraumaPose.PumpDeferred` / `PumpBotRestores` | [TraumaPose.cs:234-275 / :411-420](../../modded/Patches/Trauma/TraumaPose.cs) — idempotentes por frame/deadline | Pump próprio no `Update` (independência com 003/004 OFF) |
| `TraumaPose.CancelKind(kind, region, reason)` | hoje [TraumaPose.cs:342-353](../../modded/Patches/Trauma/TraumaPose.cs) — ganha o filtro de região (§1.5) | Toggle-off/raid-end do 006 cancela SÓ entradas do estômago |
| `TraumaConsumerRegistry.Register` | [TraumaEngineState.cs:132-135](../../modded/Patches/Trauma/TraumaEngineState.cs); `AnyActiveFor` :137-149 | `StomachEffects` cobre `Stomach` — destrava o toast (decisão 20; texto já existe, [TraumaLocale.cs:21/:32/:71](../../modded/Patches/Trauma/TraumaLocale.cs)) |
| `TraumaObservability.LogRoll` | [TraumaObservability.cs:41-46](../../modded/Patches/Trauma/TraumaObservability.cs) — formato estável D19 | Call site que faltava desde o 002 — todo roll logado (p usado + resultado) |
| `UnityEngine.Random.value` | idioma exato em [VoiceAndHealthUtils.cs:51](../../modded/Helpers/VoiceAndHealthUtils.cs); `UnityEngine.Random` (gênero, não `.value`) também em [MedicalLogic.cs:366](../../modded/Patches/Medical/MedicalLogic.cs) via `Random.Range` — PA-01-02 | RNG do roll, com curto-circuito determinístico em 0/100 (§1.3) |
| `TraumaFallCycleConsumer.IsCycleEngaged` | [TraumaFallCycleConsumer.cs:49-55](../../modded/Patches/Trauma/TraumaFallCycleConsumer.cs) — consumido DENTRO da primitiva (:96) | Absorção D2 (humano em ciclo / bot em hold) — nenhuma consulta nova no 006 |

## 3. Novas propriedades F12 (BepInEx)

Seção nova `10. Trauma 2.0 (Estômago)` + rename-at-delivery na seção 6 + tooltip INERTE na seção 2. `PROPRIEDADES.md` atualizado na entrega (gate).

| Seção | Nome (EN) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|
| `10. Trauma 2.0 (Estômago)` | `Stomach Crouch Chance Percent` | float | `75` | 0 a 100 | — | Chance (%) de agachar involuntário ao ZERAR o estômago SEM analgésico ativo. Rolada 1× por zerada (curar e zerar de novo rola de novo; estômago que permanece zerado não re-rola). 0 = nunca agacha (rolls seguem logados); 100 = sempre. |
| `10. Trauma 2.0 (Estômago)` | `Stomach Crouch Chance Under Painkiller Percent` | float | `25` | 0 a 100 | — | Chance (%) com analgésico ativo NO INSTANTE da zerada (valor congelado nessa hora — tomar/expirar analgésico depois não muda nada até a próxima zerada). Independente do slider sem analgésico — sem trava entre eles; inverter é permitido. |
| `6. Trauma 2.0 (Consumidores)` | `Stomach Effects` | bool | **`true`** (rename do placeholder, era `false`) | — | — | Agachar involuntário probabilístico ao zerar o estômago (item 006). Governado pelo master Trauma 2.0; desligar mid-raid cancela agachares pendentes DO ESTÔMAGO (não toca os de pernas); o "sem ar" legado NÃO volta (inerte permanente). |
| `2. Mecanicas (Trauma)` | `Sistema de Estomago` (tooltip novo) | bool | `true` (inalterado) | — | — | (INERTE desde a v1.7.0 — substituído pelo Trauma 2.0 / Stomach Effects. Remoção da key no item 010.) |

Estado neutro: `Stomach Effects` OFF = zerada de estômago **sem roll e sem efeito por NENHUM caminho** (legado removido; motor segue publicando/logando a transição); o toast da linha fica suprimido com log `toast SUPPRESSED (no consumer)` — nenhum outro consumidor cobre `Stomach` além do Debug Test ([TraumaEngineState.cs:137-149](../../modded/Patches/Trauma/TraumaEngineState.cs)). Sliders lidos por `.Value` no instante de CADA roll (sem cache — mudanças no F12 valem na próxima zerada). Sem clamp entre os dois sliders (decisão do §1.11; premissa p/ 011). Nota PROPRIEDADES: linha na tabela **Renomeadas** (`Stomach Effects (item 006)` → `Stomach Effects`, órfã deletada sem copiar valor) + seção 10 nova + INERTE na seção 2 + Histórico de Alterações.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Patches/Trauma/TraumaStomachConsumer.cs` | CRIAR | Consumidor 006: registry (`StomachEffects`/`Stomach`) + `SubscribeWithSnapshot` (SEM `OneShotPublished`); roll na transição real de entrada com pk LATCHED da transição + extremos determinísticos; `LogRoll`; pré-check do cooldown compartilhado com supressão logada; humano → `TryInvoluntaryCrouch(region=Stomach)`, bot → `BotCrouchDip(region=Stomach)`; try/catch no entry point; `Update` com world-swap/toggle edges + `CancelKind(InvoluntaryCrouch, Stomach, …)` + pumps próprios (`PumpDeferred`/`PumpBotRestores`). |
| `modded/Patches/Trauma/TraumaPose.cs` | MODIFICAR | (1) dedup do `Defer` casa por `(player, kind, region)` (:195-220 — fecha o corner da funcional §4; entradas de pernas e estômago coexistem, cada uma re-valida a própria linha; segunda vira NOOP pós-execução); (2) `CancelKind` ganha `TraumaRegion region` (:342-353) — cancel por (kind, região); (3) `KindWord(kind, region)` devolve `stomach-crouch` p/ agachar de estômago (:61-62) — formatos de pernas/queda bit-idênticos; (4) `AbsorbIfCycleEngaged(p, kind, region)` (:94-101) e `BotCrouchDip(p, region = Legs)` (:372) propagam a região ao log; logs NOOP/EXECUTED de `TryInvoluntaryCrouch`/pump usam o word por região (:121/:135/:264/:271). |
| `modded/Patches/Trauma/TraumaLegsConsumer.cs` | MODIFICAR | Call site único: toggle-off usa `CancelKind(InvoluntaryCrouch, TraumaRegion.Legs, "toggle-off")` (:212) — nunca varre adiados do estômago (paridade com PA-01-04 do 004). Nada mais muda (o filtro por kind do `OnOneShotCore` :130 segue correto — o 006 não publica). |
| `modded/Patches/Trauma/HealthPatches.cs` | MODIFICAR | Bloco legado "sem ar" REMOVIDO por inteiro (:98-122 — gate `ConfigStomachEnabled`, stamina+pose+voz "Gut", guard PA-01-09 incluso); comentário-lápide D10 no lugar (padrão dos blocos de pernas :123-124 e braços :125-128). Patch/alvo intocados. |
| `modded/TRLImmersiveCombatMedicinePlugin.cs` | MODIFICAR | Versão `1.7.0` (:17 e log :73); tooltip INERTE em `Sistema de Estomago` (:86 — padrão :83-85); rename do bind `Stomach Effects (item 006)` → `Stomach Effects` ON com tooltip real (:135-136); binds da seção 10 (2 sliders, após :181); 4º bloco de delete de órfã em `MigrateOrphanedConfigKeys` (padrão :361-382); `AddComponent<TraumaStomachConsumer>()` após o 005 (:205 — DEPOIS do `TraumaEngine`, ordem do 003). |
| `PROPRIEDADES.md` | MODIFICAR | Seção 10 nova; INERTE na seção 2; tooltip real do `Stomach Effects` na seção 6; linha na tabela Renomeadas; Histórico de Alterações. Gate de entrega. |

Sem mudança em: motor (`TraumaEngine`/`TraumaEngineState`/`TraumaMatrixResolver`), `TraumaObservability` (call site só), `TraumaLocale` (texto já existe), `TraumaFallCycleConsumer`, `csproj`, `compile-mod.sh` (nenhuma referência nova).

## 5. Stubs de código

> Pré-código: assinaturas completas + corpo mínimo plausível. Cada referência tem `// ref:`. Contrato do motor citado do código implementado v1.6.0.

```csharp
// modded/Patches/Trauma/TraumaStomachConsumer.cs
using Comfort.Common;
using EFT;
using UnityEngine;

namespace TRLImmersiveCombatMedicine.Trauma
{
    /// <summary>Consumidor de ESTÔMAGO (spec 006): agachar involuntário probabilístico ao ZERAR o estômago.
    /// O roll nasce AQUI (o motor publica só a transição com o analgésico LATCHED — D8, TraumaEngine.cs:554-561);
    /// o agachar reusa a primitiva do 003 por chamada DIRETA — nunca pelo barramento OneShotPublished (o 003
    /// escuta qualquer InvoluntaryCrouch sem discriminar região, TraumaLegsConsumer.cs:130 — spec 006 §1.4).
    /// Dono-only herdado do motor (D16); bots INCLUSOS (decisão 11) — sem gate de headless.</summary>
    public sealed class TraumaStomachConsumer : MonoBehaviour
    {
        private static TraumaStomachConsumer _instance;

        private bool _wasActive;
        private GameWorld _trackedWorld; // padrão 003/004: world-swap/transit + null-detect

        private static readonly TraumaRegion[] StomachRegions = { TraumaRegion.Stomach };

        private void Awake()
        {
            _instance = this;
            // Registro destrava o toast de 1ª ocorrência da linha (decisão 20; texto TraumaLocale.cs:21/:32).
            // O toast é gate do MOTOR (TraumaObservability.cs:57-77) — dispara na ENTRADA da linha,
            // independente do resultado do roll (funcional §10).
            TraumaConsumerRegistry.Register(TraumaConsumerId.StomachEffects, StomachRegions, IsActive);
            TraumaEngine.SubscribeWithSnapshot(OnTransition); // replay establishing — ref: TraumaEngine.cs:72
            // SEM TraumaEngine.OneShotPublished += ...  — vazamento impossível por construção (spec 006 §1.4)
        }

        /// <summary>Master legado + master Trauma 2.0 + toggle próprio (comportamento 9 do 002).
        /// SEM gate de headless (≠ 005): bots rolam/dipam no processo DONO deles — decisão 11.</summary>
        internal static bool IsActive()
        {
            return TRLImmersiveCombatMedicinePlugin.ConfigMasterEnabled.Value
                && TRLImmersiveCombatMedicinePlugin.ConfigTrauma2Enabled.Value
                && TRLImmersiveCombatMedicinePlugin.ConfigConsumerStomachEffects.Value;
        }

        private void OnTransition(TraumaTransition t)
        {
            // ref: CR-01-04 do 004 — exceção de consumidor não pode subir p/ o StateChanged?.Invoke do motor
            try { OnTransitionCore(t); }
            catch (System.Exception ex)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogError($"[Trauma2] StomachConsumer.OnTransition: {ex.Message}");
            }
        }

        private void OnTransitionCore(TraumaTransition t)
        {
            if (t.Region != TraumaRegion.Stomach) return;
            if (!IsActive()) return;              // toggle off = ignora (motor segue publicando — 002)
            Player p = t.Player;
            if (p is null) return;
            if (t.To != TraumaLine.StomachZeroed) return; // saída da linha: nada a desfazer (one-shot puro;
                                                          //   adiado pendente morre na re-validação do pump)
            if (t.Establishing) return;           // spawn/religar/adoção: SEM roll, SEM efeito, SEM toast (funcional/AC)

            // ---- ROLL (entrega do 006 — TraumaEngineState.cs:29) ----
            // Analgésico = valor LATCHED que a transição carrega (D8 — instante da detecção da zerada;
            // NUNCA re-consultar IsUnderPainkiller aqui — corner da funcional). ref: TraumaEngine.cs:554-561/:86-88.
            float chance = t.PainkillerActive
                ? TRLImmersiveCombatMedicinePlugin.ConfigStomachCrouchChancePkPercent.Value
                : TRLImmersiveCombatMedicinePlugin.ConfigStomachCrouchChancePercent.Value;
            chance = Mathf.Clamp(chance, 0f, 100f);
            // Extremos DETERMINÍSTICOS (AC1): Random.value é inclusivo em 1.0 — sem o curto-circuito,
            // p=100 poderia falhar (value==1) e p=0 nunca deve suceder. ref: idioma Random.value em VoiceAndHealthUtils.cs:51
            // (MedicalLogic.cs:366 usa Random.Range — mesmo gênero UnityEngine.Random, não o idioma .value — PA-01-02).
            bool success = chance >= 100f || (chance > 0f && Random.value * 100f < chance);
            TraumaObservability.LogRoll(p, TraumaRegion.Stomach,
                t.PainkillerActive ? "zeroed-pk" : "zeroed", chance / 100f, success); // ref: TraumaObservability.cs:41
            if (!success) return; // falha → nenhum efeito físico (o toast é da LINHA, já tratado pelo motor)

            // ---- Cooldown compartilhado (player, kind=InvoluntaryCrouch) — decisão 19 / funcional §6 ----
            // Pré-check ANTES da primitiva (espelha a ordem do motor — TryPublishOneShot checa cooldown primeiro,
            // TraumaEngine.cs:590-594). Sucesso suprimido é LOGADO e NÃO re-tenta (zerada é evento único).
            if (TraumaEngine.TryGetOneShotDeadline(p, TraumaOneShotKind.InvoluntaryCrouch, out float cd) && cd > Time.time)
            {
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] stomach-crouch SUPPRESSED (cooldown) {p.ProfileId}");
                return;
            }
            // PA-01-01: reserva ATÔMICA na decisão de tentar (espelha TryPublishOneShot, que stampa no publish,
            // não na execução) — sem isto, um roll que caia no Defer (D7) fica sem stamp durante toda a espera
            // e uma zerada de pernas na mesma janela executaria livremente, permitindo 2 agachares se o jogador
            // se levantar antes do pump do estômago rodar. Desfeita pelos caminhos que NÃO executam
            // (AbsorbIfCycleEngaged/NOOP pose-baixa já chamam ReportOneShotCanceled — TraumaPose.cs:97-98/:119-120);
            // Defer (TraumaPose.cs:192-193) captura esta reserva fresca como PublishDeadline, preservando-a
            // durante a espera do D7. A execução real re-stampa dentro da primitiva (idempotente).
            TraumaEngine.ReportOneShotExecuted(p, TraumaOneShotKind.InvoluntaryCrouch);

            // ---- Efeito — chamada DIRETA da primitiva (sem publish; stamps são guard-por-stamp — spec 006 §1.4) ----
            // Desfechos possíveis, todos logados pela primitiva: EXECUTED | DEFERRED (D7) | NOOP pose-baixa |
            // ABSORB (ciclo 004 engajado — AbsorbIfCycleEngaged no topo, TraumaPose.cs:115/:382).
            if (p.IsAI)
            {
                TraumaPose.BotCrouchDip(p, TraumaRegion.Stomach); // dip fire-and-forget — ref: TraumaPose.cs:372
                return;
            }
            if (!p.IsYourPlayer) return; // defesa extra — motor só publica donos (D16); espelho nunca chega
            TraumaPose.TryInvoluntaryCrouch(p, TraumaRegion.Stomach, TraumaOneShotKind.InvoluntaryCrouch);
        }

        private void Update()
        {
            GameWorld gw = Singleton<GameWorld>.Instance;
            if (gw == null)
            {
                // padrão N1/003: mundo morreu — cancela SÓ as próprias entradas (ownership explícito; o CancelAll
                // do componente 003 no raid-end é redundância idempotente). Refund vira no-op (cooldowns já resetados).
                TraumaPose.CancelKind(TraumaOneShotKind.InvoluntaryCrouch, TraumaRegion.Stomach, "raid-end");
                _trackedWorld = null; _wasActive = IsActive();
                return;
            }
            if (!ReferenceEquals(gw, _trackedWorld))
            {
                TraumaPose.CancelKind(TraumaOneShotKind.InvoluntaryCrouch, TraumaRegion.Stomach, "world-swap"); // transit
                _trackedWorld = gw;
            }

            bool active = IsActive();
            if (_wasActive && !active)
            {
                // Toggle OFF mid-raid: rolls param (gate do OnTransitionCore); adiados DO ESTÔMAGO cancelados com
                // refund SEM varrer os de pernas (chave por região — funcional corner do toggle). Legado NÃO volta.
                TraumaPose.CancelKind(TraumaOneShotKind.InvoluntaryCrouch, TraumaRegion.Stomach, "toggle-off");
            }
            // Religar mid-raid: NADA a estabelecer (one-shot puro) — estômago já zerado não rola (paridade establishing).
            _wasActive = active;
            if (!active) return;

            // Independência bidirecional (funcional §7): com 003 E 004 OFF, o 006 é o único a pumpar o
            // adiado D7 do estômago e a devolução do dip de bot. Ambos idempotentes com múltiplos chamadores
            // (pump 1×/frame — TraumaPose.cs:236-237; restores por deadline — :411-420).
            TraumaPose.PumpDeferred();
            TraumaPose.PumpBotRestores();
        }
    }
}
```

```csharp
// modded/Patches/Trauma/TraumaPose.cs — MODIFICAÇÕES (chave por região + cancel por região + log word)

// (1) KindWord ganha a região — formatos de pernas/queda BIT-IDÊNTICOS; estômago sai grep-ável:
private static string KindWord(TraumaOneShotKind kind, TraumaRegion region) =>
    kind == TraumaOneShotKind.InvoluntaryFall ? "fall"
        : region == TraumaRegion.Stomach ? "stomach-crouch" : "crouch";
// Call sites atualizados: Defer (:205/:217/:226), PumpDeferred cancel (:248), NOOP (:264 — usa e.Region),
// EXECUTED (:271), CancelAll (:334), CancelKind (:351), CancelFallsFor (fall — inalterado na prática),
// TryInvoluntaryCrouch NOOP/EXECUTED (:121/:135 — recebem o param region da própria assinatura).

// (2) Dedup do Defer casa por (player, kind, REGION) — fecha o corner da funcional §4 (spec 006 §1.5):
//     entradas de PERNAS e ESTÔMAGO coexistem; cada uma re-valida a PRÓPRIA linha no pump; a cura de uma
//     região cancela SÓ a entrada dela; quando a primeira executa (pose 0), a segunda cai no NOOP
//     "pose already low" com refund stamp-guarded (o cooldown re-ancorado pela execução sobrevive — o
//     Approximately de ReportOneShotCanceled não casa, TraumaEngine.cs:132).
for (int i = 0; i < _deferred.Count; i++)
{
    DeferredCrouch e = _deferred[i];
    if (ReferenceEquals(e.Player, p) && e.Kind == kind && e.Region == region) // ← e.Region == region é a mudança
    {
        // DEFER-SKIP interno (PA-01-03) inalterado — quedas são sempre Region=Legs (004 bit-idêntico).
        // O update `e.Region = region` do CR-02-03 vira no-op (região já é parte da chave) — mantido
        // p/ o caso re-publish da MESMA região atualizar PublishDeadline/RequiredLine (comentário ajustado).
        ...
    }
}

// (3) CancelKind ganha região — cancel por (kind, região); refund só de não-internas (inalterado):
/// <summary>PA-01-04 (estendido no 006): cancela SÓ (kind, região) — toggle-off do 003 usa
/// (InvoluntaryCrouch, Legs) e do 006 usa (InvoluntaryCrouch, Stomach); nenhum varre o outro
/// nem as quedas do 004 (InvoluntaryFall, Legs).</summary>
internal static void CancelKind(TraumaOneShotKind kind, TraumaRegion region, string reason)
{
    for (int i = _deferred.Count - 1; i >= 0; i--)
    {
        DeferredCrouch e = _deferred[i];
        if (e.Kind != kind || e.Region != region) continue;
        ... // remoção + refund stamp-guarded + log — corpo atual (:344-352) inalterado
    }
}

// (4) Absorção/bot dip propagam a região SÓ p/ o log word (comportamento intocado):
private static bool AbsorbIfCycleEngaged(Player p, TraumaOneShotKind kind, TraumaRegion region)
{
    // corpo atual (:94-101); log vira $"[Trauma2] {KindWord(kind, region)} ABSORB (fall-cycle) {p.ProfileId}"
}
internal static void BotCrouchDip(Player botPlayer, TraumaRegion region = TraumaRegion.Legs)
{
    // corpo atual (:372-408); default Legs mantém o call site do 003 (TraumaLegsConsumer.cs:134) intocado;
    // logs "bot dip ..." ficam com formato inalterado (correlação pelo roll anterior — spec 006 abertura 4)
}
```

```csharp
// modded/Patches/Trauma/TraumaLegsConsumer.cs — 1 linha (call site do cancel por região)
TraumaPose.CancelKind(TraumaOneShotKind.InvoluntaryCrouch, TraumaRegion.Legs, "toggle-off"); // era (kind, reason) — :212
```

```csharp
// modded/Patches/Trauma/HealthPatches.cs — REMOÇÃO do bloco legado (:98-122) → comentário-lápide:
// ref: spec 006 §1.9 (D10) — bloco legado de ESTÔMAGO removido ("sem ar" por hit ≥35: stamina zerada +
// SetPoseLevel(0f, true) + voz "Gut", INCLUSIVE bots — o Postfix não filtrava IA). A reação de estômago
// agora é do Trauma 2.0 (motor 002 publica a zerada; TraumaStomachConsumer rola p=75/25 e agacha via
// TraumaPose). O guard IsCycleEngaged (PA-01-09 do 004) morre junto — a arbitragem D2 do estômago passa
// a ser a absorção padrão da primitiva (TraumaPose.AbsorbIfCycleEngaged). A key "Sistema de Estomago"
// fica INERTE (remoção no item 010). Desmaio (acima) segue legado até o item 007.
```

```csharp
// modded/TRLImmersiveCombatMedicinePlugin.cs — MODIFICAÇÕES
[BepInPlugin("com.trl.immersivecombatmedicine", "TRL-ImmersiveCombatMedicine", "1.7.0")] // :17 (+ log :73)

public static ConfigEntry<float> ConfigStomachCrouchChancePercent;   // --- Trauma 2.0 — Estômago (spec 006 §3) ---
public static ConfigEntry<float> ConfigStomachCrouchChancePkPercent;

// :86 — tooltip INERTE (padrão :83-85; key/valor preservados até o 010):
ConfigStomachEnabled = Config.Bind("2. Mecanicas (Trauma)", "Sistema de Estomago", true,
    "(INERTE desde a v1.7.0 — substituído pelo Trauma 2.0 / Stomach Effects. Remoção da key no item 010.)");

// :135-136 — RENAME-AT-DELIVERY do placeholder (nasce ON; órfã deletada sem copiar valor — padrão 003/004/005):
ConfigConsumerStomachEffects = Config.Bind("6. Trauma 2.0 (Consumidores)", "Stomach Effects", true,
    "Agachar involuntário probabilístico ao zerar o estômago (item 006). Governado pelo master Trauma 2.0; " +
    "desligar mid-raid cancela agachares pendentes DO ESTÔMAGO (não toca os de pernas); o \"sem ar\" legado NÃO volta.");

// após :181 — seção 10 (sliders independentes, SEM clamp entre si — inverter é permitido; premissa p/ 011):
ConfigStomachCrouchChancePercent = Config.Bind("10. Trauma 2.0 (Estômago)", "Stomach Crouch Chance Percent", 75f,
    new ConfigDescription("Chance (%) de agachar involuntário ao ZERAR o estômago SEM analgésico ativo. Rolada 1× por zerada " +
        "(curar e zerar de novo rola de novo; estômago que permanece zerado não re-rola). 0 = nunca agacha (rolls seguem logados); 100 = sempre.",
        new AcceptableValueRange<float>(0f, 100f)));
ConfigStomachCrouchChancePkPercent = Config.Bind("10. Trauma 2.0 (Estômago)", "Stomach Crouch Chance Under Painkiller Percent", 25f,
    new ConfigDescription("Chance (%) com analgésico ativo NO INSTANTE da zerada (valor congelado nessa hora — tomar/expirar depois " +
        "não muda nada até a próxima zerada). Independente do slider sem analgésico — sem trava entre eles; inverter é permitido.",
        new AcceptableValueRange<float>(0f, 100f)));

// MigrateOrphanedConfigKeys — 4º bloco (padrão literal do 005, :361-382):
//   section == "6. Trauma 2.0 (Consumidores)" && key == "Stomach Effects (item 006)" → orphans.Remove + Config.Save
//   + LogWarning "[Config] Key placeholder órfã DELETADA (rename-at-delivery, sem copiar valor): 'Stomach Effects (item 006)' → 'Stomach Effects'."

gameObject.AddComponent<TraumaStomachConsumer>(); // após :205 — DEPOIS do TraumaEngine (ordem do 003; replay vazio inofensivo)
```

## 6. Fluxo de dados

```
[hit/cirurgia zera o ESTÔMAGO no DONO (humano local, ou bot do host/headless)]
        ▼
[motor 002: EvaluatePlayer → StomachZeroed (TraumaEngine.cs:532); latch do analgésico NA DETECÇÃO (:554-561)]
        │ StateChanged (transição com PainkillerActive=LATCHED; From==To não publica :548 — toda entrada é zerada NOVA)
        │ + toast de 1ª ocorrência da LINHA (gate do motor: consumidor ativo + humano local + não-establishing —
        │   TraumaObservability.cs:57-77; independe do resultado do roll — funcional §10)
        ▼
[TraumaStomachConsumer.OnTransitionCore]
        ├─ establishing → RETURN (sem roll/efeito/toast — spawn ferido, religar, adoção)
        ├─ ROLL: p = config (75/25) pelo pk LATCHED da transição; UnityEngine.Random com extremos determinísticos
        │      → LogRoll SEMPRE ("roll <id> Stomach zeroed|zeroed-pk p=0.75 result=true|false")
        ├─ falha → nada (toast já foi da linha)
        ├─ sucesso + cooldown (player, InvoluntaryCrouch) ativo → "stomach-crouch SUPPRESSED (cooldown)" — sem re-tentativa
        │      (rajada pernas+estômago no MESMO frame: Legs publica antes — ordem :541 — e stampa :595 → colapsa em 1 agachar)
        ├─ sucesso + cooldown livre → ReportOneShotExecuted(p, InvoluntaryCrouch) IMEDIATO (reserva atômica — PA-01-01;
        │      fecha a janela D7 sem stamp; desfeita por ABSORB/NOOP via ReportOneShotCanceled; Defer a herda como PublishDeadline)
        ├─ sucesso + humano → TraumaPose.TryInvoluntaryCrouch(p, Stomach, InvoluntaryCrouch)  [chamada DIRETA — sem publish]
        │      ├─ ciclo 004 engajado → "stomach-crouch ABSORB (fall-cycle)" + refund no-op (D2)
        │      ├─ pose já baixa (incl. blackout que força prone) → "stomach-crouch NOOP (pose already low)"
        │      ├─ D7 (escada/BTR/vault) → "stomach-crouch DEFERRED (<guard>)" — entrada PRÓPRIA (player, kind, Stomach);
        │      │      pump re-valida GetLine(p, Stomach)==StomachZeroed: curou → CANCELED (refund no-op); executou →
        │      │      "stomach-crouch EXECUTED" + ReportOneShotExecuted (re-ancora o cooldown compartilhado)
        │      └─ executa → "stomach-crouch EXECUTED" + ReportOneShotExecuted → agacha (animação vanilla, levanta livre;
        │             pose→peers via sync nativo — D16, sem protocolo)
        └─ sucesso + bot → TraumaPose.BotCrouchDip(p, Stomach) → dip + restore agendado (ABSORB se em hold do 004)

[legs crouch publicado pelo motor ≤3-5s DEPOIS de um roll de estômago bem-sucedido — inclusive se o estômago
 ainda está no Defer (D7) esperando: a reserva atômica (PA-01-01) já stampou o cooldown na decisão de tentar]
        → suprimido PELO MOTOR ("one-shot SUPPRESSED (cooldown) InvoluntaryCrouch" — TraumaEngine.cs:590-594);
          o 003 nem é acordado. Direção inversa idem (pré-check do consumidor). InvoluntaryFall não é afetado.
```

### Observabilidade (formatos de log — D19; estáveis/grep-áveis)

| Evento | Formato | Origem |
|---|---|---|
| Roll (SEMPRE, sucesso ou falha, humano ou bot) | `[Trauma2] roll <id> Stomach zeroed\|zeroed-pk p=<0.###> result=true\|false` | `LogRoll` ([TraumaObservability.cs:41-46](../../modded/Patches/Trauma/TraumaObservability.cs)) — call site novo |
| Sucesso suprimido por cooldown | `[Trauma2] stomach-crouch SUPPRESSED (cooldown) <id>` | consumidor (novo) |
| Absorção D2 (ciclo 004 / bot em hold) | `[Trauma2] stomach-crouch ABSORB (fall-cycle) <id>` | `AbsorbIfCycleEngaged` (word por região) |
| Execução / NOOP / adiamento / cancelamento | `[Trauma2] stomach-crouch EXECUTED\|NOOP (pose already low)\|DEFERRED (<guard>)\|CANCELED (<reason>) <id>` | primitiva/fila (word por região) |
| Supressão inversa (pernas na janela) | `[Trauma2] one-shot SUPPRESSED (cooldown) InvoluntaryCrouch <id>` | motor (:592 — formato existente) |
| Toast suprimido com 006 OFF | `[Trauma2] toast SUPPRESSED (no consumer) StomachZeroed` | motor (:69 — formato existente) |

Formatos de pernas/queda (`crouch ...`/`fall ...`/`bot dip ...`) permanecem **bit-idênticos** — greps dos ACs do 003/004 intactos.

Exemplo (AC estatístico + determinístico): zerar o estômago de pé sem analgésico → `Stomach: None -> StomachZeroed reason=Damage ... pk=false` + toast 1ª vez + `roll <id> Stomach zeroed p=0.75 result=true` + `stomach-crouch EXECUTED` → agachado, levanta livre. Curar (cirurgia) → `StomachZeroed -> None`; zerar de novo → NOVO roll no log. Tomar analgésico com estômago JÁ zerado → nenhuma linha nova de Stomach, nenhum roll (latch D8). Config a 0% → `p=0 result=false` em toda zerada, nunca agacha.

## 7. Riscos e dependências

- **Stamp expirado no dict de cooldown (caminho sem publish):** o motor nunca poda entradas expiradas de `_cooldownUntil` (só no untrack, [TraumaEngine.cs:413-417](../../modded/Patches/Trauma/TraumaEngine.cs)) — os caminhos NOOP/ABSORB/Defer do 006 podem capturar um stamp EXPIRADO e o `ReportOneShotCanceled` removê-lo (Approximately casa). Efeito: remoção de entrada morta — **semanticamente nulo** (toda leitura compara `now < deadline`). Um stamp ATIVO nunca é alcançado por esses caminhos no fluxo do 006: o pré-check do consumidor barra antes (supressão). Documentado como contrato, não como fix.
- **`Random.value` inclusivo em 1.0:** sem o curto-circuito dos extremos, o AC determinístico (100% → sempre; 0% → nunca) teria falha residual ~1/2^24. Resolvido na fórmula (§1.3); o clamp 0-100 do slider já vem do `AcceptableValueRange`, o `Mathf.Clamp` no código é cinto contra edição manual do .cfg.
- **P-3.5/P-3.6 (memória):** 003 (v1.4.1) e 004 (v1.5.2) ainda NÃO validados in-game — o 006 reusa exatamente a primitiva/fila/absorção deles. Bug estrutural achado na validação deles = retrabalho herdado aqui (risco aceito pela diretiva P-3.4; validações combináveis numa raid só).
- **Mudança na chave do dedup atravessa o 003/004:** a fila é compartilhada — regressão possível se o match por região alterar o comportamento das quedas. Mitigação por construção: `InvoluntaryFall` só existe com `Region=Legs` (call sites: [TraumaFallCycleConsumer.cs:144/:344](../../modded/Patches/Trauma/TraumaFallCycleConsumer.cs)) → o predicado novo é equivalente ao antigo p/ todo o tráfego do 004; p/ o 003, um único crouch de pernas por jogador existe por vez (mesma região) → dedup idêntico. Smoke dedicado no §8.
- **Coexistência de DOIS crouches adiados do mesmo jogador (pernas+estômago):** cenário raro (exige D7 prolongado + cooldown expirado entre os publishes). O pump executa em ordem reversa de inserção; o segundo vira NOOP com refund stamp-guarded — nenhum double-crouch possível (a pose já está baixa). Sem cap novo de fila (máx. 3 entradas/jogador: 2 crouch + 1 fall).
- **Fika/multiplayer (D16):** zero protocolo novo — transições só existem no processo dono (`IsOwnedHere`); pose do agachar replica pelo sync nativo (PlayerStateData — mesmo canal do 003); toast local-only (gate `IsYourPlayer` do motor); headless roda roll+dip dos bots (sem gate de headless no `IsActive` — §1.2). Nenhum log de roll no processo não-dono (AC Fika da funcional é grep de ausência).
- **Legado removido é irreversível por config:** decisão D10 da funcional (inerte permanente, key INERTE até o 010) — não há flag de rollback; quem quiser o "sem ar" antigo fica sem (registrado; a funcional já fechou isso em 2 reviews).

### Aberturas explícitas para os reviewers

1. **Ordem cooldown → absorção:** o pré-check de cooldown roda ANTES da chamada da primitiva (logo, antes do `AbsorbIfCycleEngaged`). Consequência: roll-sucesso com ciclo 004 engajado E cooldown ativo loga `SUPPRESSED (cooldown)` (não `ABSORB`). Espelha a ordem do motor (:590-594) e evita tocar o dict de cooldown com stamp ativo. Os dois logs satisfazem ACs distintos da funcional — inversão só mudaria o rótulo do log nesse corner duplo.
2. **Log word `stomach-crouch`:** distingue região sem quebrar os formatos do 003/004 (bit-idênticos). Alternativa (token `region=` em todos os logs da fila) alteraria formatos estáveis já usados em greps de AC — descartada.
3. **Toggle-off do 006 NÃO flusha dips de bot:** `BotRestore` não tem região; flushar varreria dips de pernas. Dips auto-expiram em ≤1.5s ([TraumaPose.cs:403](../../modded/Patches/Trauma/TraumaPose.cs)) — janela residual aceita (premissa p/ 011).
4. **Logs `bot dip` sem região:** formato inalterado; a origem (pernas × estômago) sai por correlação com o `roll`/`one-shot` imediatamente anterior no log. Adicionar região quebraria o formato usado no AC5 do 003.
5. **Condition strings do roll `zeroed`/`zeroed-pk`:** o campo `condition` do D19 fica auto-descritivo e grep-ável p/ o AC estatístico (20 rolls por série, por condição). O campo "dano" do D19 pertence aos rolls por hit do 007 (funcional §10 — premissa p/ 011).
6. **`p` logado normalizado 0-1** (`p=0.75`): segue o `{probability:0.###}` do formato estável do `LogRoll` (:45); o slider é 0-100 mas o log é probabilidade. Alternativa (logar 75) é cosmética — decidir na review se preferem paridade com o slider.
7. **`Update` do consumidor pumpa mesmo sendo o 3º chamador:** custo zero (guard por frame no pump; restores por deadline) e é a ÚNICA garantia da independência 006-ON/003-OFF/004-OFF. Alternativa (pump centralizado num único componente) é refactor fora do escopo (premissa p/ 011).

## 8. Checklist de implementação

- [x] `TraumaPose.cs`: match do `Defer` por `(player, kind, region)`; `CancelKind(kind, region, reason)`; `KindWord(kind, region)` com `stomach-crouch`; `AbsorbIfCycleEngaged(p, kind, region)`; `BotCrouchDip(p, region = Legs)`; call sites de log atualizados (NOOP/EXECUTED/DEFERRED/CANCELED/ABSORB) — formatos de pernas/queda bit-idênticos (diff de log vazio p/ kind=fall e região=Legs).
- [x] `TraumaLegsConsumer.cs`: call site `CancelKind(InvoluntaryCrouch, TraumaRegion.Legs, "toggle-off")` (:212).
- [x] `TraumaStomachConsumer.cs`: registry + `SubscribeWithSnapshot` (sem `OneShotPublished`); roll com pk latched + extremos determinísticos + `LogRoll`; pré-check de cooldown com supressão logada; humano→`TryInvoluntaryCrouch(Stomach)` / bot→`BotCrouchDip(Stomach)`; try/catch; `Update` (world-swap/toggle edges + `CancelKind(…, Stomach, …)` + `PumpDeferred` + `PumpBotRestores`).
- [x] `HealthPatches.cs`: bloco :98-122 removido por inteiro + comentário-lápide D10 (inclui a morte do guard PA-01-09).
- [x] Plugin: versão 1.7.0 (:17/:73); tooltip INERTE `Sistema de Estomago` (:86); rename `Stomach Effects` ON (:135-136); binds seção 10; 4º bloco do `MigrateOrphanedConfigKeys`; `AddComponent<TraumaStomachConsumer>()`.
- [x] `PROPRIEDADES.md`: seção 10 + INERTE seção 2 + tooltip seção 6 + tabela Renomeadas + Histórico. `/update-mod-graph` no commit da entrega (pendente — fora do escopo do `/code-mod`, roda em command separado).
- [ ] Smoke (greps por AC da funcional): **determinístico** — 100%: toda zerada de pé → EXECUTED|DEFERRED|ABSORB|SUPPRESSED|NOOP (todos logados); 0%: só `p=0 result=false`. **Estatístico** — 20 rolls `zeroed` → 11-19 `result=true`; 20 rolls `zeroed-pk` (analgésico no instante da zerada) → 1-9. **Re-roll** — curar+re-zerar = 2 rolls; permanecer zerado (novo hit na região, tomar/expirar analgésico) = 0 rolls novos. **D2** — roll-sucesso no ciclo do 004 → `stomach-crouch ABSORB (fall-cycle)`; em pé normal → agacha e levanta livre. **Bots** — bot dono zera → roll logado + dip; bot em hold → ABSORB; repetir no headless. **Cooldown** — agachar de pernas + zerada ≤3-5s → `stomach-crouch SUPPRESSED (cooldown)` (e inverso: estômago executa → `one-shot SUPPRESSED (cooldown) InvoluntaryCrouch` no publish de pernas); nunca 2 agachares na janela. **Toggles** — 006 OFF/003 ON: zerada sem roll (grep ausente) e sem agachar; 006 ON/003 OFF: roll+agachar normais (pump próprio). **Legado** — hit ≥35 no estômago SEM zerar → sem stamina drain/pose/voz "Gut" (humano E bot, key legada em qualquer estado). **Estado** — spawn com estômago zerado: sem roll/toast (establishing); curar+re-zerar na mesma raid → roll; raid nova zera toasts/cooldowns. **Fika** — peer vê pose via sync; sem log de roll no processo não-dono.
- [ ] Smoke extra (corners): zerada dupla no mesmo frame → 1 transição/1 roll (dirty-flag do motor); D7 (BTR/escada) → `stomach-crouch DEFERRED` e cura antes da execução → `CANCELED (state-changed)` sem tocar entrada de pernas coexistente (corner do dedup — validar com pernas adiado junto); toggle 006 OFF com adiado → `CANCELED (toggle-off)` só do estômago; roll no frame de blackout → `NOOP (pose already low)` (nunca pose forçada em inconsciente); rajada pernas+estômago mesmo frame → 1 agachar + 1 supressão; FullRestore da cirurgia → saída via reconciliação e re-zerada rola normal; **PA-01-01** — roll de estômago adiado por D7 (BTR/escada mantido) + zerada de pernas NA JANELA → publish de pernas deve sair `one-shot SUPPRESSED (cooldown)` (nunca o inverso: o estômago adiado não pode "perder" a reserva durante a espera); levantar voluntariamente ANTES do pump do estômago rodar não deve produzir um segundo agachar físico na mesma janela.

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start/stop idempotentes — AP-01 | ✅ | Consumidor quase-stateless; herda fronteiras do motor (transições só em raid armada); `Update` com null-detect/world-swap (padrão 003/004) cancela SÓ as próprias entradas; sem timers/deadlines próprios; toasts/cooldowns resetados pelo motor (AC8 do 002). |
| 2 | Filtro MainPlayer/Fika em todo ponto que reage a player — AP-02 | ✅ | Efeitos via motor (só donos — `IsOwnedHere`); humano gateado `IsYourPlayer` (defesa extra — motor não publica espelhos, D16); bots pelo caminho `IsAI` no processo dono (host/headless); toast local-only no gate do motor. |
| 3 | Alvos ofuscados/virtuais; overrides auditados — AP-03 | ✅ | ZERO patch novo e zero membro EFT novo — só APIs do próprio mod já auditadas (003/004: `SetPoseLevel`/`IsInPronePose`/espelhos imunes); nenhum GClass tocado. |
| 4 | Mudança de estado via API canônica; side-effects mapeados — AP-04 | ✅ | Pose exclusivamente pela primitiva compartilhada (funil vanilla com guards/NOOP/refund — contrato do 003); cooldown pelas APIs do motor (`TryGetOneShotDeadline`/`ReportOneShotExecuted` — stamps guard-por-stamp, §7 risco 1); remoção do legado elimina um escritor de pose fora do motor (menos side-effect, não mais). |
| 5 | Estado entre raids: raid1→exit→raid2, alt-F4/morte/MIA | ✅ | Sem estado persistente próprio; fila limpa por `CancelKind` próprio + `CancelAll` do 003 (redundância idempotente); establishing nunca rola (spawn ferido/transit); morte → untrack do motor → transição nunca chega. |
| 6 | ConfigEntry sem ambiguidade (incl. estado neutro) — AP-05 | ✅ | §3: faixas 0-100 com semântica dos extremos NO tooltip; independência dos sliders explícita (sem clamp — decisão registrada); estado neutro OFF documentado (sem roll/efeito por nenhum caminho; legado não volta); rename-at-delivery com órfã deletada (lição CR-03-01); INERTE na key legada. |
| 7 | Reentry-guard em método patcheado re-invocado — AP-07 | ✅ | Nenhum patch novo; o consumidor não roda dentro de método patcheado próprio (handler de evento C#); a primitiva já carrega os guards do 003/004 (`StandReentryFlag` é do ciclo, não usado aqui). |
| 8 | Flags/caches validados contra o contexto atual — AP-08 | ✅ | Sem cache próprio: p lido por `.Value` a cada roll; pk vem DA TRANSIÇÃO (latch D8 é contrato, não cache do consumidor); adiado re-valida `GetLine(p, Stomach)` no pump (chave por região garante a linha CERTA — fecha o corner CR-02-03 de vez); cooldown consultado ao vivo. |

## Histórico

| Data | Evento |
|---|---|
| 2026-07-19 | Spec técnica criada via `/create-technical-spec` (base v1.6.0 — motor 002 + consumidores 003/004/005 entregues; memória: P-3.5/P-3.6 validação in-game pendente — risco herdado em §7; premissa do P-3.5 "dedup (player,kind,region)" adotada em §1.5). Decisões: chamada DIRETA da primitiva sem `OneShotPublished` (vazamento p/ o 003 impossível por construção), chave da fila por região fechando o corner da funcional §4, pré-check do cooldown compartilhado com supressão logada, log word `stomach-crouch` sem quebrar formatos do 003/004, zero patch Harmony novo. |
