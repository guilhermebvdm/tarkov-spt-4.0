# 005 — Braços: Tremor + cancelamento de ADS escalonado · Code Review 01

**Mod:** TRL-ImmersiveCombatMedicine
**Spec funcional:** [005-bracos-tremor-ads-01-spec.md](005-bracos-tremor-ads-01-spec.md)
**Spec técnica:** [005-bracos-tremor-ads-02-spec-tech.md](005-bracos-tremor-ads-02-spec-tech.md)
**Asbuild:** não existe (`/code-mod` legado — pré-condição validada via fallback (b): 10/10 arquivos da §4 da spec técnica batem com o diff de `git show 60cf2fcb --stat`, ≥50%)
**Data:** 2026-07-19

> Análise crítica do código implementado pelo commit `60cf2fcb` (v1.6.0, já deployado em D:\SPT). Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.
>
> `Memória consultada: snapshot "Estado atual" + P-3.7 (aberta 2026-07-19 ~18h — overhaul pausado por custo; 005 IMPLEMENTADO v1.6.0 mas sem code-review; retomada recomendada: (1) este code-review r1); nenhum bloqueador 🔴 conhecido para o 005 na memória. P-3.4 (premissas p/ item 011, várias herdadas do 003/004) e P-3.6 (item 004 entregue v1.5.2, ciclo de queda) consultadas para calibrar o padrão de rigor esperado — nada nelas contradiz o código revisado aqui.`

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 3 · Total: 3

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | D — Arquitetura | 🟡 Médio | Predicado de incapacidade duplicado pela 3ª vez em vez de reusar `IsPauseCondition` do 004 | ✅ Aplicado |
| CR-01-02 | E — Legibilidade/manutenção | 🟢 Menor | `TryBlockReAds` não loga o bloqueio quando a voz é suprimida por incapacidade | ✅ Aplicado |
| CR-01-03 | C — Gap de entrega | 🟢 Menor | Grafo do mod (`/update-mod-graph`) não regenerado no commit de entrega do 005 | ✅ Aplicado |

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

## Veredito das 3 divergências registradas pelo implementador (mensagem do commit `60cf2fcb`)

| # | Divergência citada no commit | Veredito |
|---|---|---|
| (a) | "Discard anchor lines" — `TraumaTremor.Discard`/`Remove` ganharam guard `if (owned == null) return;` que o stub da spec (§5) não tinha | **Correta e inofensiva** — pura defesa contra log espúrio ("tremor OFF"/"DISCARD" sem nada a desfazer, ex.: `TearDownLocal` chamado num estado já limpo pela poda "stale", `TraumaArmsConsumer.cs:384-385`). Nenhuma mudança de comportamento em relação ao contrato da spec. |
| (b) | "inherited member lookups" — `AccessTools.Field(typeof(Player.FirearmController), "_player")` resolve um campo declarado na classe BASE `ItemHandsController`, não em `FirearmController` | **Correta** — confirmado no Assembly: `ItemHandsController._player` é `protected internal` (`references/eft-decompiled/Assembly-CSharp/EFT/Player.cs:17698`), e `Player.FirearmController : ItemHandsController` (`:2441`). `AccessTools.Field` do Harmony sobe a cadeia de tipos-base recursivamente quando o campo não está declarado no tipo passado — resolução correta, mesmo padrão que o FOVFix usa em produção (citado pela spec). |
| (c) | "re-enable via MainPlayer pattern" — o religar mid-raid (`TraumaArmsConsumer.cs:363-378`) usa `gw.MainPlayer` direto em vez de reaproveitar `_localPlayer` | **Correta e consistente** — é exatamente o padrão que `TraumaFallCycleConsumer.cs:244-251` (item 004) já usa para o mesmo cenário (religar = avaliação estabelecedora a partir de `gw.MainPlayer` + `TraumaEngine.IsOwnedHere`/`GetLine`). Nenhuma divergência de comportamento entre os dois consumidores. |

## Limpo em

- **Idempotência e lifecycle do tremor (P2):** `TraumaTremor.Apply` (`TraumaTremor.cs:54-95`) só cria instância nova quando `Owned == null || !Owned.Existing || !ReferenceEquals(OwnedPlayer, p)`; `Remove` (`:101-121`) anula `Owned`/`OwnedPlayer` ANTES do `ForceResidue()` — confirmado que isso impede o watchdog (`OnEffectGone`, `TraumaArmsConsumer.cs:232-240`) de latchar `_reestablishPending` numa remoção PRÓPRIA (o `EffectResidualEvent` síncrono vê `Owned==null` e `IsOurs` falha o `ReferenceEquals`). `Discard` (`:127-133`) é bookkeeping-only, nunca chama `ForceResidue` — usado corretamente só nos branches `worldDead=true` de `TearDownLocal` (`TraumaArmsConsumer.cs:246-256`, chamado em raid-end/world-swap, `:340`/`:349`).
- **Re-âncora pós-cura (PA-01-06):** `Apply` (`TraumaTremor.cs:57-68`) mantém a âncora se a parte antiga ainda está comprometida, senão remove+reaplica na parte nova; `PickArmAnchor` (`TraumaArmsConsumer.cs:142-148`) prioriza `LeftArm` se comprometido, senão `RightArm` — bate exatamente com a abertura 6 da spec ("Left se zerado/quebrado, senão Right").
- **Postfix do PWA (PA-01-03):** gate por `Owned.Existing` (Added|Started), não `Active` — `ArmsAimPatches.cs:54` — fecha o gap de 1 tick identificado na review técnica; `ReferenceEquals(__instance, p.ProceduralWeaponAnimation)` blinda espelhos/bots.
- **Timer de ADS por evento:** `HandsChangedEvent`/`OnAimingChanged` com subscribe/unsubscribe simétrico (`EnsureAimHooks`/`HookFirearmController`/`TearDownAimHooks`, `TraumaArmsConsumer.cs:177-209`) — troca de arma resubscreve corretamente (nenhuma ação sobre controller trocado, lição stances CR-01-02/AP-08); âncora `max(edge, entrada/mudança de linha)` reproduzida fielmente em `ApplyLine` (`:115-137`).
- **Lockout + voz accept-gated (PA-01-02/PA-02-06):** `_lockoutVoicePlayed` só é setado `true` se `TraumaVoice.TryPlayStrong` retornar `true` (`TryBlockReAds`, `TraumaArmsConsumer.cs:305-311`); piso de 0,3s + log skipped no máx. 1×/janela (`:320-326`) implementados fielmente.
- **AP-03 (dispatch virtual auditado):** guard `IsYourPlayer`/`!IsAI` no prefix de `SetAim` (`ArmsAimPatches.cs:32`) e no `OnTransitionCore` (`TraumaArmsConsumer.cs:104-111`) — bots excluídos com log, espelhos nunca alcançam (Fika `ObservedFirearmController`/`BotFirearmController` conforme auditoria da review técnica).
- **Reentrância (AP-07):** `OnTransition`/`OnHandsChanged` com try/catch (`TraumaArmsConsumer.cs:91-96`/`:165-174`) — exceção de consumidor não aborta o dispatch desprotegido do motor (`StateChanged?.Invoke`), padrão herdado do CR-01-04 do 004.
- **Config/entrega:** seção 9 sem colisão com a 8 (Queda, item 004); rename-at-delivery do placeholder `"Arms Effects (item 005)"` com delete-sem-copiar-valor + `Config.Save()` (`TRLImmersiveCombatMedicinePlugin.cs`, bloco espelha o padrão 003/004 — lição CR-03-01); versão 1.6.0 nos três pontos (`[BepInPlugin]`, log do Awake, `csproj`); `PROPRIEDADES.md` fiel aos `Config.Bind` literais.
- **Higiene entre raids:** nada estático novo além do documentado no §9 check 5 da spec (`_timerClampWarned`, `_reestablishStormWarned`, `TraumaTremor.Owned*`) — todos com ponto de limpeza identificado.

## Pontos

### CR-01-01 · D — Arquitetura · 🟡 Médio · ✅ Aplicado em 2026-07-19

**Predicado de incapacidade duplicado pela 3ª vez em código, em vez de reusar `TraumaFallCycleConsumer.IsPauseCondition` já extraído no item 004**

**Local:** [`mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/TraumaArmsConsumer.cs:294-298`](../../modded/Patches/Trauma/TraumaArmsConsumer.cs#L294) × [`mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/TraumaFallCycleConsumer.cs:68-72`](../../modded/Patches/Trauma/TraumaFallCycleConsumer.cs#L68)

**Problema:** O guard de incapacidade da voz do lockout copia inline o mesmo predicado de 4 termos que já existe como método dedicado no item 004:

```csharp
// TraumaArmsConsumer.cs:297-298 (TryBlockReAds)
bool incapacitated = TraumaState.BlackoutTimers.ContainsKey(p.ProfileId) || TraumaState.IsFainted
    || p.HealthController == null || !p.HealthController.IsAlive;
```

```csharp
// TraumaFallCycleConsumer.cs:68-72
private static bool IsPauseCondition(Player p)
{
    return TraumaState.BlackoutTimers.ContainsKey(p.ProfileId) || TraumaState.IsFainted
        || p.HealthController == null || !p.HealthController.IsAlive;
}
```

`IsPauseCondition` foi extraído justamente pelo `CR-02-02` do code-review-02 do item 004 (`004-pernas-cair-ciclo-04-code-review-02.md`), que já apontava: *"o risco é o item 006/007 ... tocar a definição de 'pausado' num ponto e não no outro"*. A spec técnica do 005 (PA-02-05) foi escrita ANTES dessa extração e mandou "espelhar o predicado declarado em `TraumaFallCycleConsumer.cs:235-236`" (linhas cruas, pré-refactor) — mas por ocasião da implementação (código já em v1.5.2, com `IsPauseCondition` já existindo), o predicado foi copiado de novo em vez de reaproveitado. `IsPauseCondition` é `private static`, então `TraumaArmsConsumer` (classe diferente) não conseguiria chamá-lo sem uma mudança de visibilidade — mas essa mudança não foi feita nem considerada.

**Por que importa:** É exatamente o risco que o CR-02-02 previu se materializando pela primeira vez — agora são 3 cópias do mesmo predicado (`TickHumanCycle`/`OnFallExecuted` via `IsPauseCondition`, mais esta nova cópia). O item 007 (Blackout 2.0 — desmaio percentual) vai quase certamente tocar a definição de "jogador incapacitado"; com 3 cópias, o próximo editor corrige uma e esquece a outra — regressão silenciosa (voz tocando durante inconsciência, ou bloqueio de voz sobrevivendo além do necessário).

**Sugestão:** Trocar `private static bool IsPauseCondition` para `internal static bool IsPauseCondition` em `TraumaFallCycleConsumer.cs:68`, e em `TraumaArmsConsumer.cs:297-298` substituir a cópia inline por `bool incapacitated = TraumaFallCycleConsumer.IsPauseCondition(p);`. Mesma assembly, sem custo de acoplamento adicional (o 005 já depende implicitamente da mesma semântica).

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** `IsPauseCondition` tornado `internal` em `TraumaFallCycleConsumer.cs:68` (comentário XML atualizado registrando o reuso pelo 005). `TraumaArmsConsumer.TryBlockReAds` (agora `:301`) chama `TraumaFallCycleConsumer.IsPauseCondition(p)` em vez da cópia inline; o `using TrueTrauma;` órfão (só existia para `TraumaState`, não mais referenciado) foi removido.

**Aplicação:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/TraumaFallCycleConsumer.cs` (linha 68), `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/TraumaArmsConsumer.cs` (linhas 1-7, ~301). Versão 1.6.1, compilada e implantada em D:\SPT (0 erros, mesmos 16 warnings pré-existentes de `HealthPatches.cs`).

---

### CR-01-02 · E — Legibilidade/manutenção · 🟢 Menor · ✅ Aplicado em 2026-07-19

**`TryBlockReAds` não emite NENHUM log quando o bloqueio ocorre durante incapacidade — dificulta validar o corner "sem voz durante inconsciência" pelos logs**

**Local:** [`mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/TraumaArmsConsumer.cs:292-330`](../../modded/Patches/Trauma/TraumaArmsConsumer.cs#L292)

**Problema:** As duas únicas linhas de log do bloqueio do lockout (`voice=true` e `voice=skipped(busy|blocked)`) vivem DENTRO do bloco `if (!incapacitated)`:

```csharp
if (!inst._lockoutVoicePlayed && Time.time >= inst._nextVoiceTryAt)
{
    bool incapacitated = TraumaState.BlackoutTimers.ContainsKey(p.ProfileId) || TraumaState.IsFainted
        || p.HealthController == null || !p.HealthController.IsAlive;
    if (!incapacitated)
    {
        bool played = TraumaVoice.TryPlayStrong(p);
        if (played) { ...log "voice=true"... }
        else { ...log "voice=skipped(busy|blocked)"... }
    }
}
return true; // bloqueio sempre acontece, com ou sem log
```

Quando `incapacitated == true`, o método ainda retorna `true` (o re-ADS continua bloqueado, corretamente), mas nenhuma das duas linhas de log roda — a tentativa bloqueada durante blackout/desmaio/downed fica **sem qualquer rastro no log**.

**Por que importa:** O corner de smoke test do §8 da spec técnica ("desmaio durante ADS/lockout: SEM voz — inclusive na janela de wake e no downed Fika... tremor re-estabelecido no wake") depende de leitura de log para validação in-game (padrão do resto da spec, que cita "log confirma" repetidamente). Sem uma linha de log para o caso "bloqueado + incapacitado", fica impossível diferenciar via log entre "o jogador nem tentou re-ADS" e "tentou, foi bloqueado, mas a voz foi corretamente suprimida por incapacidade" — a distinção que o corner pede para verificar.

**Sugestão:** Mover a computação de `incapacitated` para fora do gate `!_lockoutVoicePlayed && Time.time >= _nextVoiceTryAt` (ou duplicá-la ali) e, no branch `incapacitated == true`, emitir um log leve (1×/janela, reaproveitando o padrão de `_lockoutVoiceSkipLogged` ou uma flag irmã) como `"[Trauma2] ads LOCKOUT BLOCK {p.ProfileId} remaining={...} voice=suppressed(incapacitated)"`.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** Nova flag `_lockoutIncapacitatedLogged` (reset em `ResetLockout`/`ExecuteCancel`, mesmo padrão de `_lockoutVoiceSkipLogged`); branch `else` do guard de incapacidade em `TryBlockReAds` agora loga `"[Trauma2] ads LOCKOUT BLOCK {profileId} remaining={...} voice=suppressed(incapacitated)"` 1×/janela.

**Aplicação:** `mods/TRL-ImmersiveCombatMedicine/modded/Patches/Trauma/TraumaArmsConsumer.cs` (campo + 2 resets + branch `else`). Mesmo build v1.6.1 do CR-01-01.

---

### CR-01-03 · C — Gap de entrega · 🟢 Menor · ✅ Aplicado em 2026-07-19

**Grafo do mod (`/update-mod-graph`) não foi regenerado no commit de entrega do 005 — `TraumaArmsConsumer`/`TraumaTremor`/`ArmsAimPatches` invisíveis ao grafo navegável**

**Local:** spec técnica §8 ("`PROPRIEDADES.md` … + `/update-mod-graph` no commit da entrega") × `references/graphs/mods/TRL-ImmersiveCombatMedicine/graph.json`

**Problema:** O grafo do mod foi regenerado pela última vez no commit `199e3448` ("item 004 DELIVERED — full 2x2x2 gate v1.5.2"), que é ANCESTRAL do commit `60cf2fcb` do item 005 (`git merge-base --is-ancestor 199e3448 60cf2fcb` confirma). Uma busca no `graph.json` por `TraumaArmsConsumer`, `TraumaTremor` ou `ArmsAimPatches` não retorna nenhuma ocorrência — os 3 arquivos novos do 005 (563 linhas) são invisíveis às queries de grafo (`graphify explain`/`graphify path`) e a qualquer `/create-technical-spec`/`/code-review` futuro que dependa dele.

**Por que importa:** É a mesma lacuna que o `CR-01-03` do code-review-01 do item 004 já registrou ("O item 005 (braços) declara reuso do `TraumaVoice` — a spec dele vai nascer consultando um grafo desatualizado") — e de fato aconteceu: a spec técnica do 005 foi escrita contra um grafo sem os arquivos do 004 recém-entregues, compensado por leitura manual dos arquivos. Sem a regeneração agora, o PRÓXIMO item (006/007) herda a mesma lacuna, um degrau mais fundo.

**Sugestão:** Rodar `scripts/update-graphs.sh` (ou skill `/update-mod-graph`) e commitar `references/graphs/mods/TRL-ImmersiveCombatMedicine/` junto da aplicação desta review — mesma resolução que o item 004 aplicou para o próprio `CR-01-03` (deferido ao fechamento do item, decisão do orquestrador).

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** `bash scripts/update-graphs.sh TRL-ImmersiveCombatMedicine` rodado — 815 nós/1347 arestas/45 comunidades; `TraumaArmsConsumer`/`TraumaTremor`/`ArmsAimPatches` confirmados presentes no `graph.json` regenerado.

**Aplicação:** `references/graphs/mods/TRL-ImmersiveCombatMedicine/` regenerado (a commitar junto desta review).

<!-- Após /apply-code-review: marcar a opção escolhida, trocar título para ✅ Aplicado em YYYY-MM-DD e adicionar **Resolução:** ... + **Aplicação:** descrição do que foi feito + paths -->

---

## Nota sobre `graphify affected`

`graphify` está disponível no PATH deste ambiente, mas como o grafo do mod está desatualizado (CR-01-03) — sem nós para `TraumaArmsConsumer`/`TraumaTremor`/`ArmsAimPatches` — uma consulta `graphify affected`/`explain` sobre essas classes não retornaria callers reais (o grafo simplesmente não os conhece). A verificação de impacto nesta review foi feita por leitura direta dos arquivos tocados/criados (Passo 3) e checagem cruzada com `TraumaEngine.cs`/`TraumaEngineState.cs`/`TraumaFallCycleConsumer.cs` via Grep — equivalente em cobertura para o escopo deste diff (2 patches Harmony novos + 1 consumidor novo, zero mudança no motor).

## Histórico

| Data | Evento |
| --- | --- |
| 2026-07-19 | Code review 01 criada via `/code-review` (retomada da P-3.7 — análise refeita do zero após a sessão anterior ter sido interrompida antes de escrever o artefato). 0 🔴 · 0 🟠 · 1 🟡 · 2 🟢 — 3 achados. Nenhum bloqueador: item pode ser fechado; achados são opcionais/deferíveis. |
| 2026-07-19 | Os 3 achados (CR-01-01/02/03) aplicados via `/apply-code-review`: `IsPauseCondition` reusado (internal), log de incapacidade no lockout, grafo regenerado. Build v1.6.1 (0 erros), implantado em D:\SPT. Item FECHADO 🟢 — sem 2ª rodada (todos os achados eram 🟡/🟢, sem bloqueador). |
