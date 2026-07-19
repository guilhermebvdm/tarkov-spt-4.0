# 004 — Pernas: Cair + ciclo levantar 3s/15s · Review Técnica 01

**Mod:** TRL-ImmersiveCombatMedicine
**Spec técnica revisada:** [004-pernas-cair-ciclo-02-spec-tech.md](004-pernas-cair-ciclo-02-spec-tech.md)
**Data:** 2026-07-19

> Análise crítica ADVERSARIAL da spec técnica (rodada 1). Cada ponto recebe um ID `PA-01-MM`. Resolver até zerar bloqueadores antes de `/code-mod`.
>
> `Memória consultada: snapshot Sessão 2 (2026-07-11) + pendências · afetam esta review: [P-3.5 — 003 v1.4.1 entregue, VALIDAÇÃO IN-GAME PENDENTE; o 004 estende exatamente TraumaPose/caps/gates — risco já registrado na spec §7], [P-3.4 — diretiva do overhaul + rastro de premissas p/ item 011] · nenhuma pendência 🔴`

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 15 · Total: 15 — **rodada 1 APLICADA na spec (0 pendentes, 0 conflitos)**

## Veredito das âncoras (foco nº 1)

Verificação completa contra código do mod, `references/eft-decompiled/`, `references/fika-plugin/`, decompiles de assembly real em `scratchpad/spike001/` e a instalação `D:\SPT`:

| Grupo | Verificadas | Falhas estruturais | Drift menor |
|---|---|---|---|
| Código do mod (TraumaEngine/EngineState/Pose/LegsConsumer/SpeedLimit/Input/State/Locale/Movement/Health/Plugin) | 30 | 0 | 1 (TraumaLocale.cs:20 — PA-01-13a) |
| EFT decompilado (MovementContext, Player, GamePlayerOwner, ECommand, PhraseSpeakerClass, EPhraseTrigger) | 25 | 0 | 3 (PA-01-13b/c/d) |
| Fika 2.3.4 (ObservedMovementContext, FikaPlayer, FikaBot, PlayerStateData, Client/NoInertiaMovementContext) | 7 | 0 | 0 |
| BigBrain/BotLay/SAIN/protótipos (scratchpad + DLL instalada) | 7 | 0 | 0 |
| Tooling (compile-mod.sh × BigBrain) | 1 | **1** (PA-01-10) | 0 |

Confirmações load-bearing dignas de nota: `CanStandAt(float h)` — o parâmetro chama-se `h` de fato (binding Harmony do stub OK, MovementContext.cs:3304); `StartInteraction`/`method_18` saem de prone **via setter** de `IsInPronePose` (:3089-3093/:3115-3119) → interação NÃO fura o bloqueio (gate :690/:694 + `SetPoseLevel` :2149); `ClientMovementContext`/`NoInertiaMovementContext` não sobrescrevem `CanStandAt`/`CanSprint` (grep 0); `ObservedMovementContext.CanStandAt => true` sem base-call (:109-112); `FikaPlayer.OnPhraseTold` envia `PhrasePacket` para QUALQUER trigger (único guard `IsAlive`, :1093-1102); `BotLay.NextPosibleGetUp` público (:22-23) e `GetUp` ignora `withCheck` (:182-188); `EPhraseTrigger.OnAgony=9`/`OnBeingHurt=15` (:6/:12); `DrakiaXYZ-BigBrain.dll` presente em `D:\SPT\BepInEx\plugins\` (raiz, 43 KB); protótipo `proto-traumadowned` compilado com `bin/Debug/TraumaDownedProto.dll` gerado.

## Categorias

- **A — Gaps de Especificação:** informações ausentes que ambiguam a implementação
- **B — Edge Cases:** cenários válidos não cobertos
- **C — Erros de Lógica:** pressupostos errados, contradições, código incompatível com SPT 4.0+

## Impacto

- 🔴 **Bloqueador** — impede implementar ou causa bug garantido em ponto central
- 🟠 **Forte** — comportamento errado garantido em cenário relevante
- 🟡 **Médio** — comportamento errado em cenário plausível / gap que ambigua a implementação
- 🟢 **Menor** — qualidade/clareza, não bloqueia

---

## Pontos

### PA-01-01 · C — Erro de Lógica · 🔴 Bloqueador

**`TraumaSpeedCap` sem identidade de consumidor → undo cruzado da causa 1000 no handoff FallCycle→N1/N2**

**Local:** spec §1.7 + stub `ApplyWindowCap`/`RemoveWindowCap` (spec §5, `TraumaSpeedCap.Apply(_local, percent)` / `TraumaSpeedCap.RemoveGuarded(_local)`) × `TraumaLegsConsumer.cs:86-102` (OnTransition do 003) e `TRLImmersiveCombatMedicinePlugin.cs:156` (ordem de AddComponent).

**Problema:** A spec afirma "bookkeeping por consumidor" e "sem escrita dupla: a linha de perna é exclusiva por jogador", mas a API stubada do `TraumaSpeedCap` não carrega identidade de consumidor (`Apply(player, percent)` / `RemoveGuarded(player)` — remove a causa `(ESpeedLimit)1000` inteira). A exclusividade por linha falha exatamente no **frame de handoff**: analgésico (ou cura parcial) durante a JANELA gera a transição `LegsFallCycle → LegsLimpN1/N2`, disparada num ÚNICO `StateChanged` (TraumaEngine.cs:565) para os DOIS consumidores. Ordem de invocação = ordem de subscribe = ordem de `AddComponent` (003 em Plugin.cs:156, antes do novo 004): o handler do 003 roda primeiro e **aplica** o cap N1/N2 (`ApplyCap`, TraumaLegsConsumer.cs:101); o handler do 004 roda depois e `Disengage → RemoveWindowCap → RemoveGuarded` **remove a causa 1000 que o 003 acabou de escrever**.

**Por que importa:** Cenário AC2-adjacente garantido (analgésico com Z2+Q2 tomado na JANELA): o jogador sai do ciclo SEM mancar N2 — o cap do 003 é deletado no mesmo frame e nada o re-aplica até a próxima transição de linha (o postfix de `UpdateSpeedLimitByHealth` só re-LOGA, SpeedLimitPatches.cs:37-54 — não re-aplica). Regressão silenciosa em item 003 já entregue.

**Sugestão:** Especificar o `TraumaSpeedCap` com registro por consumidor: `Apply(TraumaConsumerId id, Player p, float percent)` guarda `(player → {id → target%})` e escreve a causa 1000 como `min` dos targets ativos (Remove+Add, padrão atual); `Remove(TraumaConsumerId id, Player p)` remove só a entrada do id e re-deriva a causa (remove a causa quando o set esvazia; mantém o RemoveGuarded downed-safe do 003 — remove a causa mesmo com `!IsAlive`, gate só no recompute/log). Atualizar os stubs de `ApplyWindowCap`/`RemoveWindowCap` (004 usa `TraumaConsumerId.FallCycle`) e a delegação do 003 (`TraumaConsumerId.LegsEffects`). Com isso o handoff em qualquer ordem de eventos fica correto por construção.

**Decisão:**
- `[x]` Caminho alternativo: **causas DISTINTAS por consumidor** — 003 mantém `(ESpeedLimit)1000` e `ApplyCap`/`RemoveCapGuarded` INTOCADOS; 004 ganha helper novo `TraumaSpeedCap` com causa própria `(ESpeedLimit)1001`. A min-composição NATIVA do dict `SpeedLimits` (`method_4()` :1798 toma o MIN das causas ativas; verificado no decompile) arbitra a coexistência em qualquer ordem de handlers — mesmo efeito da sugestão (registro por consumidor) com ZERO mudança no cap do 003 entregue e ainda não validado in-game. ✅ Aplicado: spec §1.7, §2 (hook Cap N2), §4, §5 (ApplyWindowCap/RemoveWindowCap), §6, §7 (abertura 3 marcada RESOLVIDA) e §8 reescritos p/ causa 1001; premissa registrada p/ item 011 (consumidores futuros ganham causas 1002+).

### PA-01-02 · C — Erro de Lógica · 🟠 Forte

**"OnTransition do 003 ignora a linha Cair" deixa cap N2 e bookkeeping do 003 stale na ENTRADA do ciclo**

**Local:** spec §1.7 ("o `OnTransition` do 003 ignora a linha") e §4 (linha TraumaLegsConsumer) × `TraumaLegsConsumer.cs:92-95` (só remove em `To == None`) e `:216-218` (poda só quando `GetLine == None`).

**Problema:** Na direção N1/N2 → FallCycle (expiração de analgésico com Q2 persistente — AC2 da funcional), "ignorar" a transição significa que o 003 NÃO remove o cap que aplicou nem limpa `_applied[p]`: o jogador entra no ciclo (BLOQUEIO prone) carregando o cap N2 do 003 ativo e bookkeeping stale — que a poda oportunista nunca limpa (a linha é FallCycle, não None). O postfix de re-log (SpeedLimitPatches.cs:42 `TryGetApplied`) continua logando cap "aplicado" durante todo o ciclo, e o estado só se corrige se/quando a linha voltar a N1/N2 ou None.

**Por que importa:** Escritor fantasma da mesma causa 1000 convivendo com o cap de janela do 004 (interage direto com o PA-01-01); logs de calibração AC6 mentem ("linha Cair sem cap N2 permanente" falha no grep); toggle-off do 003 durante o ciclo desfaz um cap que "pertence" ao 004 naquele momento.

**Sugestão:** Reescrever no §4: o `OnTransition` do 003 trata `To == LegsFallCycle` como **saída para efeitos do 003** — `_applied.Remove(p)` + remoção do seu registro no `TraumaSpeedCap` (com o PA-01-01, `Remove(LegsEffects, p)`) — handoff explícito ao 004. A poda oportunista pode adicionalmente podar entradas cujo `GetLine == LegsFallCycle`.

**Decisão:**
- `[x]` Aceitar sugestão — ✅ Aplicado: spec §1.7/§4/§8 — o `OnTransition` do 003 trata `To == LegsFallCycle` como SAÍDA p/ efeitos do 003 (`_applied.Remove(p)` + `RemoveCapGuarded(p)`, causa 1000 própria) e a poda oportunista poda também `GetLine == LegsFallCycle` (handoff explícito ao 004).

### PA-01-03 · C — Erro de Lógica · 🟠 Forte

**Ordem real dos eventos do motor: `StateChanged` dispara ANTES de `OneShotPublished` — o stub engaja toda entrada como JANELA antes de o one-shot chegar**

**Local:** `TraumaEngine.cs:565` (`StateChanged?.Invoke`) vs `:571-572` (`TryPublishOneShot`) × stub `OnTransition` (`if (_phase == FallPhase.None) Engage(p, t.Establishing)`) e spec §1.3.

**Problema:** A spec desenha a entrada como "OnOneShot → derrubar; one-shot suprimido → engaja em JANELA", mas no motor real a transição chega PRIMEIRO. O stub `Engage()` roda em TODA entrada com `_phase == None`: entrada publicada com one-shot vira `EnterWindow("engage-standing")` (cap aplicado + deadline de janela + log) e só depois o `OnOneShot` corrige para BLOQUEIO — churn Remove+Add+Remove da causa 1000 e logs falsos a CADA entrada. Pior: se o derrubar de entrada for ADIADO (D7 — escada/BTR/vault), o `OnFallExecuted` não roda e a FSM fica presa em JANELA com deadline de 3s; ao expirar, dispara re-queda `internalFall:true` que colide no dedup `(player, kind)` com a entrada NÃO-interna pendente — o hit de dedup atualiza a entrada (TraumaPose.cs:113-131) e a flag `Internal` sobrescreveria a original, perdendo o refund do publish no cancelamento.

**Por que importa:** O corner "one-shot suprimido por cooldown → JANELA" é indistinguível no handler como especificado (a transição não diz se um one-shot virá), e o caminho de adiamento D7 da entrada — corner explícito da funcional — produz timer de janela + colisão de fila com semântica de refund errada.

**Sugestão:** Especificar no §1.3/stub: no `OnTransition` de entrada com `!establishing`, consultar `TraumaEngine.TryGetOneShotDeadline(p, InvoluntaryFall, out d)`: `d` futuro (cooldown ativo) ⇒ o publish será suprimido ⇒ `Engage` (JANELA/BLOQUEIO-se-prone); caso contrário NÃO engajar — o `OnOneShot` do mesmo frame conduz (executa → BLOQUEIO; adia → fase de espera). Definir a fase durante adiamento de queda (ex.: `FallPending`: sem timers, sem cap, negação OFF; encerrada pelo callback do pump ou pelo cancel). Proibir enqueue de re-queda interna enquanto existir entrada não-interna pendente do mesmo kind (o pump da pendente já entrega a queda).

**Decisão:**
- `[x]` Aceitar sugestão — ✅ Aplicado: spec §1.3/§1.4 + stubs — entrada sem establishing consulta `TryGetOneShotDeadline` (cooldown ativo ⇒ engaja; senão o `OnOneShot` do frame conduz), fase nova `FallPending` p/ adiamento D7 (sem timers/cap/negação) e enqueue interno RECUSADO com entrada não-interna pendente do mesmo kind.

### PA-01-04 · B — Edge Case · 🟠 Forte

**`CancelAll` do 003 varre as quedas adiadas do 004 (toggle-off cruzado)**

**Local:** `TraumaLegsConsumer.cs:187` (`TraumaPose.CancelAll("toggle-off")` no edge do toggle DO 003) e `:161/:172` (raid-end/world-swap) × spec §4 (TraumaPose ganha fila generalizada por kind; sem mudança nos call sites de CancelAll).

**Problema:** Com a fila generalizada, `CancelAll` passa a conter entradas `InvoluntaryFall` do 004. Desligar "Legs Effects" (003) no F12 com uma queda do 004 adiada (jogador na escada/BTR) cancela + refunda a queda de um ciclo que está ON — a queda de entrada simplesmente não acontece (a FSM fica no estado do PA-01-03 até a poda `stale`/re-avaliação). A spec não particiona o cancelamento por kind nos call sites do 003.

**Por que importa:** Toggle de um consumidor não pode ter efeito colateral em outro (contrato de independência que a própria spec usa no cap da janela: "independe do toggle 003").

**Sugestão:** Adicionar ao §4: `TraumaPose.CancelAll(string reason)` ganha overload/parametro por kind (`CancelKind(TraumaOneShotKind, reason)`); o toggle-off do 003 cancela SÓ `InvoluntaryCrouch`; raid-end/world-swap seguem varrendo tudo (cada consumidor já detecta a fronteira — documentar que a dupla chamada é idempotente porque a fila já estará vazia na segunda).

**Decisão:**
- `[x]` Aceitar sugestão — ✅ Aplicado: spec §4/§5/§8 — `CancelKind(TraumaOneShotKind, reason)` novo; toggle-off do 003 cancela SÓ `InvoluntaryCrouch`; raid-end/world-swap mantêm `CancelAll` com a idempotência da dupla chamada documentada.

### PA-01-05 · C — Erro de Lógica · 🟡 Médio

**LIBERAÇÃO no fallback agachado dispara Rising imediatamente, sem decisão do jogador**

**Local:** stub `TickHumanCycle` case `Released` (`if (!mc.IsInPronePose)` → Rising) × funcional §2 ("Se o jogador optar por NÃO levantar, permanece … estado estável") e §1 (bloqueio no fallback vale para a pose corrente).

**Problema:** A detecção de "jogador decidiu levantar" é `!IsInPronePose` — correta apenas quando o bloqueio foi em prone. No fallback agachado (CanProne recusou — meds/ladeira/sem espaço), `IsInPronePose` já é false: no PRIMEIRO tick da fase Released a FSM entra em Rising (voz leve + `SetPoseLevel(0f, force)` + rampa até PoseMemo) — o jogador é LEVANTADO automaticamente ao fim do bloqueio, sem input, violando o estado estável da LIBERAÇÃO.

**Por que importa:** Comportamento errado garantido em todo bloqueio que caiu no fallback agachado (cenário real: cair com meds em uso, encosta, interior apertado).

**Sugestão:** Detectar intenção por pose, não por prone: guardar a pose do bloqueio (0f) e transicionar Released→Rising quando (a) saiu de prone (caso prone) OU (b) `mc.PoseLevel > 0.05f` (caso fallback — o jogador subiu a pose, o que só é possível porque a negação `h > PoseLevel + 0.05f` desligou com o fim do BLOQUEIO). No caso (b) a rampa parte da pose corrente.

**Decisão:**
- `[x]` Aceitar sugestão — ✅ Aplicado: spec §1.6 + stub Released — decisão de levantar lida por POSE (`_releasedFromProne ? !IsInPronePose : PoseLevel > 0.05f`, snapshot na entrada da LIBERAÇÃO); no fallback a rampa parte da pose corrente sem `SetPoseLevel(0f, force)` e a LIBERAÇÃO permanece estável sem input.

### PA-01-06 · A — Gap · 🟡 Médio

**Ciclo de vida do `PronePending` indefinido fora do BLOQUEIO + re-tentativa por frame**

**Local:** spec §1.4 ("flag `PronePending` re-tentada a cada pump enquanto o ciclo persistir") e stub TraumaPose ("PronePending: entradas do fallback re-tentam prone a cada pump").

**Problema:** (a) "Enquanto o ciclo persistir" inclui LIBERAÇÃO/RISING/JANELA: se a re-tentativa de prone suceder fora do BLOQUEIO (jogador agachado saiu do vão apertado durante a JANELA), o mod força prone fora de hora — re-queda de fato sem passar pela FSM. (b) O pump roda 1×/frame: re-tentar prone por frame executa `CanProne` (SphereCast físico, MovementContext.cs:1209-1234) todo frame enquanto o fallback durar.

**Por que importa:** (a) é um escritor de pose fora do contrato de fases; (b) é custo físico desnecessário em caminho quente (skill §3).

**Sugestão:** Especificar: `PronePending` só re-tenta com a FSM em `Blocked`; é limpo na transição para `Released` (o bloqueio acabou — o jogador vai levantar de onde está) e no `Disengage`; cadência de re-tentativa ≥0.5s (timestamp na entrada da fila), não por frame.

**Decisão:**
- `[x]` Aceitar sugestão — ✅ Aplicado: spec §1.4/§4 + stubs — `PronePending` só re-tenta com a FSM em `Blocked`, cadência ≥0.5s por timestamp; limpo via `ClearPronePending` na transição p/ `Released` e no `Disengage`.

### PA-01-07 · B — Edge Case · 🟡 Médio

**Rising: a rampa avança `_riseTarget` sem readback — teto baixo entra na JANELA com o jogador agachado**

**Local:** stub `case FallPhase.Rising` (`_riseTarget = MoveTowards(...); mc.SetPoseLevel(_riseTarget); if (_riseTarget >= p.PoseMemo - 0.01f) EnterWindow`) × MovementContext.cs:2149 (`SetPoseLevel` retorna false quando `CanStandAt(h)` vanilla recusa) e abertura 1 da spec ("de pé efetivo = fim da rampa").

**Problema:** O retorno de `SetPoseLevel` é ignorado e a rampa progride sobre a variável local `_riseTarget`, não sobre a pose real. Levantar sob teto baixo (rastejou para baixo de um caminhão/beliche): o vanilla recusa h>agachado, a pose fica em 0, mas `_riseTarget` chega a `PoseMemo` e a JANELA começa — violando o próprio contrato "de pé efetivo" da spec; 3s depois, re-queda com o jogador que nunca ficou de pé.

**Por que importa:** Contrato central da premissa nova (item 011) quebrado num cenário comum de mapa fechado.

**Sugestão:** Rampar sobre a pose real: `float next = Mathf.MoveTowards(mc.PoseLevel, p.PoseMemo, Time.deltaTime / SlowRiseSeconds); mc.SetPoseLevel(next);` e só `EnterWindow` quando `mc.PoseLevel >= p.PoseMemo - 0.01f`. Se o vanilla recusar, a rampa estaciona (o jogador rasteja/anda agachado para fora e ela retoma) — coerente com "a JANELA só começa de pé".

**Decisão:**
- `[x]` Aceitar sugestão — ✅ Aplicado: spec §1.6 + stub Rising — rampa `MoveTowards` sobre `mc.PoseLevel` REAL; `EnterWindow` só com `mc.PoseLevel >= PoseMemo - 0.01f` (sob teto baixo a rampa estaciona e retoma); campo `_riseTarget` eliminado.

### PA-01-08 · B — Edge Case · 🟡 Médio

**`Stop()` da camada força `GetUp(false)` também no fim de X — o bot vira ioiô mecânico, contradizendo "quando a IA decidir levantar" (D14/funcional 6)**

**Local:** stub `TraumaDownedLayer.Stop()` (spec §5: `NextPosibleGetUp = 0f; GetUp(false);` incondicional) × funcional item 6 ("ao fim de X, o controle volta à camada de decisão da IA; **quando a IA decidir levantar**, a reavaliação re-derruba") e Pump step 2 (re-hold quando o bot levanta).

**Problema:** No release por expiração de X, o `Stop()` LEVANTA o bot à força no mesmo tick; como a linha Cair persiste (só cura/analgésico a remove), o Pump detecta "bot de pé + linha Cair" e re-derruba imediatamente — ciclo deitar/levantar/deitar a cada X segundos, mecânico, mesmo quando a camada que assumiria (SAIN em cover, ORBIT idle) manteria o bot deitado. A funcional prevê devolução de DECISÃO, não get-up forçado. (A recomendação P6 traz `GetUp(false)` no Stop, mas no modelo dela o hold só existia enquanto a condição durasse — no ciclo do 004 o release por X com linha persistente é o caso dominante.)

**Por que importa:** Comportamento visível errado do bot (ioiô perpétuo em vez de "levanta quando a IA re-decidir e é re-derrubado"), e AC5 ("ao levantar, decisão da IA, é re-derrubado") não sai como escrito.

**Sugestão:** Diferenciar o motivo do release no `Hold` (flag): release por X-expiry → `Stop()` só zera `NextPosibleGetUp` (destrava os `BotLay.GetUp` da IA; o bot levanta quando alguma camada decidir; o Pump re-holda ao detectar `IsLay==false`); release por cura/analgésico/toggle-off → `GetUp(false)` forçado (comportamento atual, "a IA levanta sem re-derrubada").

**Decisão:**
- `[x]` Aceitar sugestão — ✅ Aplicado: spec §1.9/§6 + stubs — `Hold` ganha flag `ForceGetUp` (+ `ShouldForceGetUp` consultado pelo `Stop()`): X-expiry só zera `NextPosibleGetUp` (IA decide; Pump re-holda ao ver `IsLay==false` com linha viva); cura/analgésico/toggle-off forçam `GetUp(false)`.

### PA-01-09 · C — Erro de Lógica · 🟡 Médio

**AC4 (estômago) não é entregável como especificado: o agachar de estômago vigente é o LEGADO, que não passa pela arbitragem D2**

**Local:** spec §8 smoke "AC4 D2 (estômago zerado com ciclo ativo → `crouch ABSORB (fall-cycle)`)" × `HealthPatches.cs:97-108` (bloco legado de estômago: `SetPoseLevel(0f, true)` direto com dano ≥35 e `!IsInPronePose`, gateado só por `ConfigStomachEnabled`) × motor (nenhum one-shot de estômago até o 006 — TraumaEngineState.cs:57-59).

**Problema:** Até o item 006, zerar o estômago NÃO gera one-shot do motor — o `AbsorbIfCycleEngaged` no topo de `TryInvoluntaryCrouch`/`BotCrouchDip` nunca é exercitado por estômago, logo o log esperado `crouch ABSORB (fall-cycle)` é inalcançável no smoke. Pior: o agachar LEGADO do estômago continua ativo e escreve pose por fora do TraumaPose — durante a JANELA (de pé) ou o Rising, um tiro de estômago ≥35 agacha o jogador sem absorção nem refund, violando o AC da funcional ("com ciclo ativo, zerar o estômago NÃO executa agachar").

**Por que importa:** AC da funcional falha em jogo por um escritor de pose que a spec não mapeou na arbitragem D2 (a spec só cobriu os caminhos do motor).

**Sugestão:** (a) Guard de 1 linha no bloco legado do estômago: `if (TraumaFallCycleConsumer.IsCycleEngaged(__instance)) { log "stomach legacy suppressed (fall-cycle)"; } else { ... }`; (b) re-escopar o smoke AC4: "estômago legado suprimido durante o ciclo" (o `crouch ABSORB` do motor fica demonstrável só com o agachar do 003 — que por exclusividade de linha não coexiste com o ciclo — e plenamente no 006); (c) registrar a premissa no rastro do item 011.

**Decisão:**
- `[x]` Aceitar sugestão — ✅ Aplicado: spec §1.8(e)/§4 (linha HealthPatches.cs nova)/§7/§8 + stub — guard `IsCycleEngaged` no bloco legado (:97-108) com log `stomach legacy suppressed (fall-cycle)`; smoke AC4 re-escopado p/ esse log; premissa registrada p/ 011 (`crouch ABSORB` por estômago só demonstrável no 006).

### PA-01-10 · A — Gap · 🟡 Médio

**Build: `compile-mod.sh` NÃO resolve `DrakiaXYZ-BigBrain.dll` — a afirmação do §4 é falsa hoje e o build falharia**

**Local:** spec §4 ("Referência `DrakiaXYZ-BigBrain.dll` … resolvida pelo compile-mod de D:\SPT\BepInEx\plugins") e §7 ("mapa do script **pode** precisar da entrada") × `.agents/scripts/compile-mod.sh` (grep `BigBrain|Drakia` = 0 hits; mapa hardcoded de `resolve_references()`, linhas 272-302, não inclui a DLL).

**Problema:** O mapa do script cobre BepInEx/Assembly-CSharp/Unity/Comfort/Fika.Core etc., mas não BigBrain. Sem editar o mapa (ou colocar a DLL manualmente em `References/` — que o skip da linha 309 preservaria, mas viola a regra "nunca copiar DLL na mão", csharp-best-practices §9), o `dotnet build` falha por referência não resolvida. A DLL existe e está na raiz de `D:\SPT\BepInEx\plugins\` (43 KB).

**Por que importa:** O "pode precisar" do §7 é na verdade um PRECISA — sem isso o item não compila; deixado como checklist frouxo, vira surpresa no `/code-mod`.

**Sugestão:** Promover a item obrigatório do §8: adicionar ao array `map` do `resolve_references()` a entrada `DrakiaXYZ-BigBrain.dll` ← `$SPT/BepInEx/plugins/DrakiaXYZ-BigBrain.dll` (mesmo padrão do Fika.Core.dll), e reescrever o §4 para "resolvida pelo compile-mod **após adicionar a entrada no mapa** (parte da entrega)".

**Decisão:**
- `[x]` Aceitar sugestão — ✅ Aplicado: spec §4 (linha nova p/ `.agents/scripts/compile-mod.sh`)/§7/§8 — entrada `DrakiaXYZ-BigBrain.dll` ← `$spt/BepInEx/plugins/DrakiaXYZ-BigBrain.dll` no mapa `resolve_references()` promovida a item OBRIGATÓRIO da entrega (o "pode precisar" virou "PRECISA").

### PA-01-11 · B — Edge Case · 🟢 Menor

**Entrada estabelecedora de BOT via transição não gera hold**

**Local:** stub `TraumaBotFall.OnLine` (só trata `to != LegsFallCycle` → release) × `EstablishFromSnapshot` (só roda no religar do toggle).

**Problema:** Bot que ENTRA na linha Cair por transição estabelecedora (adoção mid-raid/spawn ferido — establishing não publica one-shot, TraumaEngine.cs:567) cai no vazio: `OnLine(p, LegsFallCycle)` não faz nada e `OnFallOneShot` nunca chega — bot com 2 pernas quebradas de pé, sem hold, até a próxima re-publicação.

**Sugestão:** `OnLine` com `to == LegsFallCycle` → hold estabelecedor (sem refund/one-shot; idempotente via `IsHeld`). Cobre também o caso raid-start com bots já feridos.

**Decisão:**
- `[x]` Aceitar sugestão — ✅ Aplicado: spec §1.9 + stub `OnLine` — `to == LegsFallCycle` gera hold estabelecedor idempotente (`IsHeld` → no-op), sem refund/one-shot; cobre adoção mid-raid e raid-start ferido.

### PA-01-12 · A — Gap · 🟢 Menor

**Dono do `PumpDeferred` não especificado — pump duplo por frame com 003 e 004 ativos**

**Local:** `TraumaLegsConsumer.cs:227` (`TraumaPose.PumpDeferred()` no Update do 003) × stub Update do 004 (também chama `TraumaPose.PumpDeferred()`).

**Problema:** Dois consumidores pumpam a mesma fila por frame. Inócuo em correção (fila re-valida por entrada), mas não especificado — e com o 003 OFF, o pump do 004 passa a processar entradas de crouch do 003 (comportamento aceitável, mas hoje implícito).

**Sugestão:** Guard de frame dentro do `PumpDeferred` (`if (Time.frameCount == _lastPumpFrame) return; _lastPumpFrame = Time.frameCount;`) e uma frase no §4 declarando o pump idempotente por frame e agnóstico a qual consumidor o chama.

**Decisão:**
- `[x]` Aceitar sugestão — ✅ Aplicado: spec §4 + stub TraumaPose — guard `Time.frameCount == _lastPumpFrame` no `PumpDeferred` e declaração explícita de idempotência por frame/agnosticismo ao chamador (003 e 004 chamam).

### PA-01-13 · C — Erro de Lógica (âncoras) · 🟢 Menor

**4 âncoras com drift menor (nenhuma estrutural)**

**Local/Problema:**
- (a) `TraumaLocale.cs:20,29,66` — :20 é o texto de ArmsAdsCancel; o EN de `LegsFall` está em **:18** (o enum `LegsFall` em :6; :29 PT e :66 map estão corretos).
- (b) "recompute único :2553-2558" — essas linhas são `ProcessSpeedLimits` (driver do dirty-flag, :2555 `if (SpeedLimitIsDirty) method_4()`); o recompute real é **`method_4()` em :1798**. Semântica preservada (recompute único por frame), âncora imprecisa.
- (c) `Player.cs:28668-28671` "OnDemandOnly no Init" — `OnDemandOnly = !aiControlled` vive no inicializador do `new PhraseSpeakerClass` (:28670); `Init(...)` é chamada separada (:28672). Conclusão da spec intacta.
- (d) `HideoutPlayerOwner.cs:564` — o override `TranslateCommand` declara-se em **:558**; :564 é a linha do `ECommand.ToggleProne` dentro dele.

**Sugestão:** Corrigir as 4 âncoras no texto da spec (§1.2, §1.7/§2, §1.10, §2 tabela).

**Decisão:**
- `[x]` Aceitar sugestão — ✅ Aplicado: 4 âncoras corrigidas na spec (+ stub TraumaVoice): (a) TraumaLocale :18; (b) recompute = `method_4()` :1798 (driver `ProcessSpeedLimits` :2553-2558); (c) `OnDemandOnly` no inicializador :28670 (`Init` :28672); (d) override `TranslateCommand` do Hideout em :558 (:564 = ToggleProne interno).

### PA-01-14 · B — Edge Case · 🟢 Menor

**Toast da linha Cair dispara mesmo com `Fall Cycle` OFF (registry é por região)**

**Local:** `TraumaConsumerRegistry.AnyActiveFor` (TraumaEngineState.cs:137-149 — granularidade por REGIÃO) × estado neutro do §3 ("OFF = linha Cair sem efeito do mod").

**Problema:** Com o 004 OFF e o 003 ON, `AnyActiveFor(Legs)` continua true (LegsEffects cobre a região) — o toast "Your legs collapse under you." aparece prometendo uma queda que não acontece (interim removido).

**Sugestão:** Aceitar e documentar no tooltip do `Fall Cycle` + premissa p/ item 011 (granularizar o registry por linha é mudança de motor, fora do 004). Alternativa barata: `MaybeToastFirstOccurrence` pular `LegsFallCycle` quando o consumidor FallCycle está inativo (caso especial documentado).

**Decisão:**
- `[x]` Aceitar sugestão (caminho principal: aceitar + documentar) — ✅ Aplicado: spec §3 — comportamento ACEITO e documentado no tooltip do `Fall Cycle` e no parágrafo de estado neutro; premissa registrada p/ item 011 (granularizar o registry por linha = mudança de motor). A alternativa barata foi DESCARTADA: `MaybeToastFirstOccurrence` vive no motor 002 (`TraumaObservability.cs:57`) e editá-lo contradiria o contrato "zero mudança no motor" da estratégia §1.

### PA-01-15 · C — Robustez · 🟢 Menor

**try/catch ausente nos corpos novos (patch e camada de IA)**

**Local:** stubs `FallAttemptCommandPatch.Prefix` (sem try/catch) e `TraumaDownedLayer.Start/Stop` + `DownedIdleLogic.Update` (dereferências `BotOwner.BotLay/ShootData` sem guard).

**Problema:** Skill csharp §3/§6 exige corpo de prefix/postfix com try/catch+log (o `CantStandUpPatch` existente segue o padrão, InputPatches.cs:46-65 — a extensão deve preservá-lo); `Start/Stop/Update` da camada rodam no tick de IA — exceção ali (bot em despawn com `BotLay` nulo) quebra o brain do bot inteiro.

**Sugestão:** Anotar nos stubs: try/catch+`LogError` nos corpos do patch novo e da camada; null-guards de `BotOwner`/`BotLay`/`ShootData` no Start/Stop/Update (despawn durante hold — CR-01-02 já exige o sweep, o guard cobre a janela de 1 tick).

**Decisão:**
- `[x]` Aceitar sugestão — ✅ Aplicado: stubs §5 — try/catch+LogError anotado no `FallAttemptCommandPatch` e no branch do `CantStandUpPatch` (padrão InputPatches.cs:46-65 preservado); `Start`/`Stop`/`Update` da camada com try/catch e null-guards de `BotOwner`/`BotLay`/`ShootData`.

---

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C | 🔴 | TraumaSpeedCap sem identidade de consumidor — undo cruzado no handoff | ✅ Aplicado |
| PA-01-02 | C | 🟠 | 003 "ignora" a entrada na linha Cair — cap/bookkeeping stale | ✅ Aplicado |
| PA-01-03 | C | 🟠 | StateChanged antes de OneShotPublished — engage espúrio + adiamento quebrado | ✅ Aplicado |
| PA-01-04 | B | 🟠 | CancelAll do 003 varre quedas do 004 | ✅ Aplicado |
| PA-01-05 | C | 🟡 | LIBERAÇÃO no fallback agachado auto-dispara Rising | ✅ Aplicado |
| PA-01-06 | A | 🟡 | PronePending sem ciclo de vida por fase + retry por frame | ✅ Aplicado |
| PA-01-07 | B | 🟡 | Rising sem readback de pose — JANELA agachado sob teto baixo | ✅ Aplicado |
| PA-01-08 | B | 🟡 | Stop() força GetUp no fim de X — bot ioiô vs "IA decide" | ✅ Aplicado |
| PA-01-09 | C | 🟡 | AC4 estômago: legado fora da arbitragem D2 e ABSORB inalcançável | ✅ Aplicado |
| PA-01-10 | A | 🟡 | compile-mod.sh sem entrada p/ BigBrain — build falharia | ✅ Aplicado |
| PA-01-11 | B | 🟢 | Establishing de bot via transição não gera hold | ✅ Aplicado |
| PA-01-12 | A | 🟢 | Dono do PumpDeferred não especificado (pump duplo) | ✅ Aplicado |
| PA-01-13 | C | 🟢 | 4 âncoras com drift menor | ✅ Aplicado |
| PA-01-14 | B | 🟢 | Toast da linha Cair com 004 OFF | ✅ Aplicado |
| PA-01-15 | C | 🟢 | try/catch ausente em patch novo e camada de IA | ✅ Aplicado |

## Histórico

| Data | Evento |
|---|---|
| 2026-07-19 | Review 01 criada via `/review-technical-spec` (adversarial, contexto limpo). ~70 âncoras verificadas (mod + EFT decompilado + Fika 2.3.4 + scratchpad/spike001 + D:\SPT): 0 falhas estruturais, 4 drifts menores (PA-01-13), 1 afirmação de tooling falsa (PA-01-10). 1 🔴 · 3 🟠 · 6 🟡 · 5 🟢. |
| 2026-07-19 | Rodada 1 APLICADA na spec (15/15 ✅, 0 conflitos). 🔴 PA-01-01 resolvido por CAMINHO ALTERNATIVO: causas distintas por consumidor (1000=003 intocado, 1001=004 via `TraumaSpeedCap` novo) — min-composição nativa (`method_4()` :1798) arbitra o handoff; PA-01-14 pelo caminho principal (aceitar+documentar — o skip no toast tocaria o motor); demais conforme sugestão. Premissas novas p/ item 011: causas de cap por consumidor (1002+ p/ futuros), `crouch ABSORB` por estômago só no 006, toast por região com 004 OFF aceito. |
