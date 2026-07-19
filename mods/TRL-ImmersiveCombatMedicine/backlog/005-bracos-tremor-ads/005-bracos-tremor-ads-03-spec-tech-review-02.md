# 005 — Braços: Tremor + cancelamento de ADS escalonado · Review Técnica 02

**Mod:** TRL-ImmersiveCombatMedicine
**Spec técnica revisada:** [005-bracos-tremor-ads-02-spec-tech.md](005-bracos-tremor-ads-02-spec-tech.md)
**Data:** 2026-07-19

> Análise crítica ADVERSARIAL da spec técnica (rodada 2). Cada ponto recebe um ID `PA-02-MM`. Resolver até zerar bloqueadores antes de `/code-mod`.
>
> **Foco desta rodada:** (1) defasagens spec-005 × código v1.5.0 pós-item-004 (commit fd799426 — a spec foi escrita ANTES do 004 aterrissar); (2) verificação da APLICAÇÃO dos 8 achados da rodada 1 (coerência entre si); (3) âncoras novas da r1.
>
> `Memória consultada: snapshot "Estado atual" (Sessão 2, 2026-07-11) + pendências · afetam esta review: [P-3.5 — item 003 v1.4.1 entregue, validação in-game pendente; 005 reutiliza motor/registry/padrões], [P-3.4 — diretiva do overhaul 003→008 + rastro de premissas p/ item 011 (P-005-A/B na spec)] · a entrega do 004 (fd799426, v1.5.0) ainda NÃO está registrada na memória do mod · nenhuma pendência 🔴`

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 8 · Total: 8

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-02-01 | C — Erro | 🟠 | Versão `1.5.0` já foi consumida pelo item 004 — a entrega do 005 é `1.6.0` | `[x]` ✅ Aplicado |
| PA-02-02 | C — Erro | 🟠 | Seção de config `8. Trauma 2.0 (Braços)` colide com `8. Trauma 2.0 (Queda)` criada pelo 004 — renumerar p/ 9 | `[x]` ✅ Aplicado |
| PA-02-03 | A — Gap | 🟡 | `TraumaVoice` (004) existe, se declara "reusável pelo 005" e usa o MESMO OnAgony — a spec não decide unificação; reuso ingênuo violaria PA-01-02 | `[x]` ✅ Aplicado |
| PA-02-04 | C — Erro | 🟡 | `HealthPatches.cs:113-119` (voz "Arm") aponta hoje para código NOVO do 004 — bloco real migrou p/ :126-132 | `[x]` ✅ Aplicado |
| PA-02-06 | B — Edge | 🟡 | Retry accept-gated da voz (PA-01-02) × premissa P-005-A (hold por frame): `Speaker.Play` + 2 logs POR FRAME sob Blocker de squad — falta piso de re-tentativa | `[x]` ✅ Aplicado |
| PA-02-05 | B — Edge | 🟢 | Guard de inconsciência da voz cobre só `BlackoutTimers`; o 004 estabeleceu o predicado mais largo (`IsFainted` + downed Fika `!IsAlive`) | `[x]` ✅ Aplicado |
| PA-02-07 | A — Gap | 🟢 | §9 check 5 não enumera os estáticos novos que a PRÓPRIA rodada 1 introduziu (`_reestablishStormWarned`, `TraumaTremor.Owned*`) | `[x]` ✅ Aplicado |
| PA-02-08 | A — Gap | 🟢 | Âncoras do Plugin deslocadas pelo 004 (`Plugin:238-263`/`:265-287`/`:113`) — conteúdo existe, linhas não batem mais | `[x]` ✅ Aplicado |

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

## Veredito da aplicação da rodada 1

Os 8 achados da r1 foram verificados na spec ponto a ponto (não re-levantados; aqui só a APLICAÇÃO):

| Achado r1 | Aplicação | Coerência interna |
|---|---|---|
| PA-01-01 (Discard vs Remove) | ✅ consistente em §1.3/§4/§5 (stubs `Discard`/`TearDownLocal(worldDead)`) /§8/§9 check 1 | Discard no raid-end/world-swap limpa `Owned*` E `_reestablishPending`/hooks no `TearDownLocal` — watchdog não sobrevive ao mundo morto ✓ |
| PA-01-02 (throttle accept-gated) | ✅ `_lockoutVoicePlayed = bank != null` em §1.6/§5/§6/§7/§8 | Interage com a premissa P-005-A → ponto NOVO PA-02-06; e com o `TraumaVoice` que o 004 entregou → PA-02-03 |
| PA-01-03 (`Existing` no postfix) | ✅ trocado nos três pontos (§1.4/§2/§5 — stub usa `Owned == null \|\| !Owned.Existing`) | Compatível com a re-âncora (PA-01-06): `Remove("re-anchor")` anula `Owned` antes do `ForceResidue` → postfix não re-asserta durante a troca; flicker ≤1 frame documentado ✓ |
| PA-01-04 (try/catch no Apply/Remove) | ✅ §4/§5 (stubs)/§7 abertura 4/§9 check 7 | Degradação `_resolveOk=false` coerente com P-005-B ✓ |
| PA-01-05 (anular antes do ForceResidue + piso 0,5 s + storm warn) | ✅ §1.3/§5 (campos `_nextReestablishAt`/`_reestablishCount`/`_reestablishStormWarned`) | O NOVO estático `_reestablishStormWarned` ficou fora da enumeração do §9 check 5 → PA-02-07 |
| PA-01-06 (re-âncora pós-cura) | ✅ §1.3/§4/§5 (campo `OwnedAnchor`)/§7 abertura 6/§8 | Condição de manter (âncora antiga ainda comprometida) correta; interação com `Existing` ✓ |
| PA-01-07 (gate `IsHeadless`) | ✅ §1.2/§2/§5 (`IsActive()`)/§7/§9 check 2 | Âncora REAL: `references/fika-plugin/Fika.Core/Main/Utils/FikaBackendUtils.cs:49` (`public static bool IsHeadless { get; set; }`); namespace `Fika.Core.Main.Utils` correto no stub; `Fika.Core` é referência hard do csproj (TRL-ImmersiveCombatMedicine.csproj:41-44) ✓ |
| PA-01-08 (âncora com filename) | ✅ §6 `fika ObservedPlayer.cs:737-751` | Âncora REAL: `OnHealthEffectAdded` só toca som de fratura (:739-746), `OnHealthEffectRemoved` vazio (:748-751) ✓ |

**Âncoras re-verificadas nesta rodada (amostragem adversarial):** Player.cs :25291 (`ActiveHealthController` get) / :25544 (`HandsChangedEvent`) / :20037+:20146-20149 (`OnAimingChanged`) / :13695-13743 (funil `ToggleAim`→`SetAim(int)`→`SetAim(bool)` com Blindfire early-return, `AimingInterruptedByOverlap=false`, `CurrentOperation.SetAiming`) ✓; spike001/ProceduralWeaponAnimation.cs:1175-1192 (gate `OnPainkillers`→`TremorOn=false` :1182-1186, else :1189) ✓; spike001/ActiveHealthController.cs:219-233 (`Existing`=Added|Started) e :3514-3538 (`AddEffect` 6 parâmetros, `GInterface331` pula merge, residued force-removidos antes do Create) ✓; spike001/PhraseSpeakerClass.cs:206-215 (Busy `importance <= Int_0` → skip; `SpeakerManager.FreeToSpeak` sem bypass) ✓ — nota: há um TERCEIRO caminho de retorno null (`tagBank.Match` sem clipe p/ as tags), coberto pelo gating em retorno não-null da PA-01-02 sem mudança.

**Defasagens 005 × código-004 (foco nº 1):** motor (`TraumaEngine.cs` :48/:72/:110/:531/:565/:571-572/:638-642), `TraumaEngineState.cs:25-28`/`:132`, `TraumaMatrixResolver`, `TraumaLocale.cs:6`/`:67-70` — INTOCADOS pelo 004, âncoras válidas ✓. `MovementPatches.cs:119-139` (fadiga) e `:47`/`:123` — INTACTOS ✓. `TraumaState.cs:22`/`:43` ✓. `TraumaLegsConsumer.cs:157-175` (padrão Discard) — ainda válido pós-004 ✓. **Divergiram:** versão (PA-02-01), seção 8 de config (PA-02-02), `TraumaVoice` novo (PA-02-03), `HealthPatches` (PA-02-04), âncoras do Plugin (PA-02-08), predicado de inconsciência (PA-02-05). Interações 004×005 sem achado: prone forçado/`CanStandAt` não tocam mira; `FallAttemptCommandPatch` (InputPatches.cs:93-112) escuta só ToggleProne/ToggleDuck/Jump/NextWalkPose; os únicos `SetAim` do mod são `false` (HealthPatches.cs:85, MovementPatches.cs:47/:130 — passam livres pelo prefix `!value`); fases do ciclo (Blocked/Released/Window) mantêm o jogador consciente → tremor/timer/lockout operando durante o ciclo é comportamento correto por design.

---

## Pontos

### PA-02-01 · C — Erro de Lógica · 🟠 Forte

**Versão `1.5.0` já foi consumida pelo item 004 — a entrega do 005 precisa ser `1.6.0`**

**Problema:** A spec manda "bump `1.5.0`" (§4, linha do Plugin), o stub §5 termina com `[BepInPlugin(..., "1.5.0")]` e o tooltip do `Sistema de Braços` (§3 e §5) diz "(INERTE desde a v1.5.0 ...)". Mas o item 004 aterrissou DEPOIS da spec e entregou exatamente a v1.5.0: `TRLImmersiveCombatMedicinePlugin.cs:17` (`[BepInPlugin("com.trl.immersivecombatmedicine", "TRL-ImmersiveCombatMedicine", "1.5.0")]`), log em `:67`, `TRL-ImmersiveCombatMedicine.csproj:7` (`<Version>1.5.0</Version>`), commit fd799426 ("item 004 — fall + get-up cycle 3s/15s (v1.5.0)").

**Por que importa:** Entregar o 005 como "1.5.0" é no-bump — o gate mecânico de versão do `/compile-mod` (memória: toda release evolui semver; F12=BepInPlugin é a fonte canônica) barra ou, pior, gera zip `-v1.5.0` ambíguo com a build do 004 já distribuída pelo launcher/ModUpdater. O tooltip "INERTE desde a v1.5.0" mentiria: na v1.5.0 real o legado de braços ainda está ATIVO (`MovementPatches.cs:119-139` e `HealthPatches.cs:126-132` presentes).

**Sugestão:** Trocar TODAS as ocorrências de `1.5.0`-da-entrega na spec por `1.6.0`: §4 (linha do Plugin: "bump `1.6.0`"), §5 (stub `[BepInPlugin(..., "1.6.0")]`), §3 + §5 (tooltip "(INERTE desde a v1.6.0 — ...)"), §8 checklist. Anotar que o bump cobre também `csproj <Version>` e a string do log do Awake (`Plugin:67`) — os três pontos que o 004 bumpou em conjunto.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

✅ **Aplicado (2026-07-19):** todas as ocorrências de `1.5.0`-da-entrega trocadas por `1.6.0` (§3 tooltip INERTE, §4 linha do Plugin, §5 stub `[BepInPlugin]` + tooltip, §8 checklist), com nota dos TRÊS pontos do bump ([BepInPlugin] Plugin:17 + log do Awake Plugin:67 + csproj `<Version>` :7 — conjunto do 004).

### PA-02-02 · C — Erro de Lógica · 🟠 Forte

**Seção de config `8. Trauma 2.0 (Braços)` colide com `8. Trauma 2.0 (Queda)` criada pelo 004 — renumerar para `9`**

**Problema:** A spec §3 declara "Seção nova `8. Trauma 2.0 (Braços)`" (4 entries usam essa seção; stub §5 idem). O veredito da review-01 ("'8. Trauma 2.0 (Braços)' NÃO colide") era verdadeiro PRÉ-004 — mas o 004 ocupou o número: `TRLImmersiveCombatMedicinePlugin.cs:149-157` binda `Config.Bind("8. Trauma 2.0 (Queda)", ...)` (3 entries: Fall Window/Fall Block/Bot Fall Hold) e `PROPRIEDADES.md:99` documenta "## Seção 8. Trauma 2.0 (Queda)".

**Por que importa:** BepInEx casa por string literal, então não há clobber de dados — mas o F12 exibiria DUAS seções "8." (ordenadas alfabeticamente: "8. Trauma 2.0 (Braços)" ANTES de "8. Trauma 2.0 (Queda)"), quebrando a numeração sequencial que o mod mantém desde a seção 1 e a estrutura do `PROPRIEDADES.md` (single source — skill repo-workflow §7). Todo item futuro (006/007) herdaria a ambiguidade.

**Sugestão:** Renumerar para `9. Trauma 2.0 (Braços)` em §3 (cabeçalho + 4 linhas da tabela), §4 (linha do Plugin: "seção 9"), §5 (stub: `Binds §3 na seção "9. Trauma 2.0 (Braços)"`), §8 checklist e na linha do `PROPRIEDADES.md`. Registrar no §7 que a sequência pós-005 fica: 7=Pernas, 8=Queda, 9=Braços (006/007 seguem 10/11).

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

✅ **Aplicado (2026-07-19):** seção renumerada para `9. Trauma 2.0 (Braços)` em §3 (cabeçalho + 4 linhas), §4 (Plugin e PROPRIEDADES.md), §5 (stub) e §8; sequência pós-005 registrada em §3 e §7 (7=Pernas, 8=Queda, 9=Braços; 006/007 → 10/11).

### PA-02-03 · A — Gap de Especificação · 🟡 Médio

**`TraumaVoice` (entregue pelo 004) existe, declara-se "reusável pelo 005" e usa o MESMO `OnAgony` importance:100 — a spec não decide a unificação; reuso ingênuo violaria a PA-01-02**

**Problema:** A spec §1.6/§5 planeja voz via `p.Speaker.Play(EPhraseTrigger.OnAgony, ETagStatus.Combat, demand: true, importance: 100)` direto, com janela accept-gated (PA-01-02). Escrita antes do 004, ela não menciona que agora existe `modded/Patches/Trauma/TraumaVoice.cs` — cujo doc-comment (:7-9) diz explicitamente "**Reusável pelo 005 (P9)**" — com `PlayStrong` no MESMO trigger/importance (:21, tags `ETagStatus.Combat | ETagStatus.Dying` — diferente do `Combat` puro da spec) e anti-spam próprio de 2 s por (player, tipo) que é consumido ANTES do Play e IGNORA o retorno (:33-39, :21) — ou seja, o reuso direto de `PlayStrong` violaria exatamente a PA-01-02 (janela queimada em chamada engolida) e a semântica "1 voz por janela de lockout" (cooldown fixo 2 s ≠ janela 1,0–1,5 s). Na direção oposta, canal privado cria DOIS emissores independentes de OnAgony no mesmo jogador: o 004 toca em queda executada e tentativa de levantar negada (`TraumaFallCycleConsumer.cs:124`, `InputPatches.cs:104`), o 005 tocaria em tentativa de re-ADS no lockout — cenário concreto: 2 pernas + 2 braços comprometidos (granada), fase Blocked, o jogador tenta levantar (OnAgony do 004) e no mesmo segundo tenta re-mirar no lockout (Play do 005 → engolido pelo Busy ≥100 do próprio 004 → retry) — os dois throttles não se enxergam.

**Por que importa:** O implementador vai encontrar o utilitário com "Reusável pelo 005" escrito no cabeçalho e uma spec que nem o cita — vai escolher um caminho no escuro. Reuso ingênuo desfaz um achado aplicado da r1; canal privado sem registro deixa a interação (dupla fonte de OnAgony, tags divergentes, swallow mútuo) fora do AC-6/AC-9 e do rastro de premissas p/ o item 011 (P-3.4).

**Sugestão:** Decidir explicitamente na spec (§1.6 + §7 bullet "Voz"): **caminho principal — manter canal privado** (a semântica por-janela accept-gated é incompatível com o cooldown fixo upfront do `TraumaVoice`), documentando: (a) por que NÃO reusa `PlayStrong` (violaria PA-01-02); (b) que o OnAgony do 004 é um segundo emissor no mesmo Speaker — a chamada do 005 pode ser engolida pelo Busy≥100 dele (o retry da PA-01-02 já cobre) e ambos alimentam o Blocker de grupo; (c) alinhar as tags com o 004 (`ETagStatus.Combat | ETagStatus.Dying`) ou justificar o `Combat` puro. Alternativa aceitável: estender `TraumaVoice` com `TryPlayStrong(Player) : bool` accept-gated (retorno do Play) e rotear os DOIS itens por ele — mas isso adiciona `TraumaVoice.cs` à lista §4 de arquivos modificados e re-toca código do 004 entregue. Em ambos os caminhos, adicionar corner de smoke: fase Blocked do ciclo + tentativa de re-ADS no lockout (dupla fonte de OnAgony, sem voz dupla simultânea).

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[x]` Caminho alternativo: **extensão do `TraumaVoice`** — a alternativa listada na própria sugestão, escolhida por diretiva do gate: `TryPlayStrong(Player):bool` accept-gated (retorno do Play), SEM tocar `PlayStrong`/anti-spam do 004 (canal separado por consumidor → comportamento do 004 inalterado), tags alinhadas (`Combat | Dying`); `TraumaVoice.cs` entrou na lista §4 de arquivos modificados.

✅ **Aplicado (2026-07-19):** decisão registrada em §1.6 + §7 (bullet "Voz") com (a) por que NÃO reusar `PlayStrong` (violaria PA-01-02), (b) dupla fonte de OnAgony no mesmo Speaker (004: TraumaFallCycleConsumer.cs:124 / InputPatches.cs:104 — Busy≥100 mútuo + Blocker de grupo compartilhado) e (c) tags alinhadas ao 004; premissa p/ o item 011 (reconciliação dos canais de voz — rastro P-3.4) registrada; corner de smoke "fase Blocked + re-ADS no lockout (sem voz dupla)" adicionado ao §8.

### PA-02-04 · C — Erro de Lógica · 🟡 Médio

**`HealthPatches.cs:113-119` (voz "Arm") aponta hoje para código NOVO do 004 — o bloco real migrou para `:126-132`**

**Problema:** A spec §1.7, §4 e §8 mandam remover a voz "Arm" citando "`HealthPatches.cs` (:113-119)". O 004 inseriu o guard de supressão do agachar legado de estômago (PA-01-09 do 004) ANTES desse bloco: hoje `:110-115` são o `IsCycleEngaged` + log `stomach legacy suppressed`, `:116-121` o else do estômago legado, e o bloco da voz "Arm" (`ConfigArmsEnabled` + `IsPartDestroyed` + `VoiceHelper.TriggerTraumaVoice(__instance, "Arm")`) vive em `:126-132`.

**Por que importa:** É uma instrução de DELEÇÃO com range errado: seguida mecanicamente, remove o guard do 004 (re-quebrando o agachar de estômago durante o ciclo de queda — regressão de item recém-entregue) e deixa a voz "Arm" viva. A regra da review (arquivo:linha que não bate = Categoria C) existe exatamente p/ instrução destrutiva.

**Sugestão:** Re-ancorar §1.7/§4/§8 para "`HealthPatches.cs:126-132` (bloco `ConfigArmsEnabled` — voz 'Arm')" e acrescentar à linha do §4: atualizar o comentário de fronteira `:123-125` ("Desmaio (acima), estômago e braços seguem legados até os itens 007/006/005") para refletir que braços saiu na entrega do 005 (paridade com a nota que o 003 deixou no mesmo arquivo).

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

✅ **Aplicado (2026-07-19):** §1.7/§4/§8/abertura 1 re-ancorados para `HealthPatches.cs:126-132` (bloco `ConfigArmsEnabled` — voz "Arm"; guard de estômago do 004 em :110-121 fica INTACTO) + instrução de atualizar o comentário de fronteira :123-125 na entrega.

### PA-02-06 · B — Edge Case · 🟡 Médio

**Retry accept-gated (PA-01-02) × premissa P-005-A (hold pode re-disparar por frame): `Speaker.Play` + 2 logs POR FRAME sob Blocker de squad — falta um piso de re-tentativa de voz**

**Problema:** A PA-01-02 aplicada diz "a PRÓXIMA tentativa da mesma janela re-tenta (custo = 1 Play por tentativa, cadência de input — sem hot path)" (§1.6/§5). Mas a abertura 3 da PRÓPRIA spec registra a premissa P-005-A: não está provado se segurar o botão em modo hold re-dispara `SetAim(true)` 1× ou POR FRAME. Se for por frame, o pior caso composto é: lockout ativo + hold + Blocker de grupo armado (squad coop — ambiente primário, memória `feedback_coop_multiplayer_sync`) → `TryBlockReAds` chama `Speaker.Play` a CADA FRAME por até 1,5 s (~90-135 chamadas), cada uma alocando string de log no vanilla (`PhraseSpeakerClass.cs:208/:213 — LogInfo em todo skip`) + o nosso log `voice=skipped(busy|blocked)` por tentativa. A claim "sem hot path" só vale se a premissa P-005-A resolver para "1×" — a spec não pode apostar nos dois lados.

**Por que importa:** Flood de log (BepInEx console + LogOutput) e alocação por frame dentro de um prefix exatamente no cenário que o AC-6 manda validar ("hold E toggle sem spam nem furo"); contamina a leitura do log na validação in-game (o mesmo problema de observabilidade que a PA-01-02 corrigiu no `voice=true` mentiroso).

**Sugestão:** No stub de `TryBlockReAds` (§5) + §1.6: após um Play engolido, armar piso de re-tentativa de voz `_nextVoiceTryAt = Time.time + 0.3f` (tentativas antes disso não chamam Play nem logam); logar `voice=skipped(...)` no máximo 1×/janela (flag separado do `_lockoutVoicePlayed`). Mantém o AC-6 (re-tentativa em cadência humana continua tocando na próxima janela livre) e limita o pior caso do hold-por-frame a ~5 Plays por janela. Campo novo zerado no `ExecuteCancel` junto com `_lockoutVoicePlayed`.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

✅ **Aplicado (2026-07-19):** piso de re-tentativa de voz `_nextVoiceTryAt = Time.time + 0.3f` + log `voice=skipped` no máx 1×/janela (`_lockoutVoiceSkipLogged`) em §1.6/§5 (campos novos zerados no `ExecuteCancel`); abertura 3 (P-005-A) e AC-6 do §8 atualizados — pior caso hold-por-frame limitado a ~5 Plays/janela.

### PA-02-05 · B — Edge Case · 🟢 Menor

**Guard de inconsciência da voz cobre só `BlackoutTimers`; o 004 estabeleceu o predicado mais largo (`IsFainted` + downed Fika `!IsAlive`)**

**Problema:** A spec §1.6/§5 gateia a voz do lockout com `!TraumaState.BlackoutTimers.ContainsKey(p.ProfileId)`. O 004, entregue depois, consolidou o predicado de "jogador incapaz" do mod em `TraumaFallCycleConsumer.cs:235-236`: `BlackoutTimers.ContainsKey(id) || TraumaState.IsFainted || p.HealthController == null || !p.HealthController.IsAlive` (blackout legado OU downed do Fika — contrato downed-safe herdado do CR-02 do 003, que a própria spec 005 aplica no `Remove` do tremor). Dois furos do guard estreito: (a) no wake, `MainLoopPatch` remove `BlackoutTimers` (`MovementPatches.cs:61`) um instante antes de `WakeLocalPlayer` limpar `IsFainted` — janela de frames em que a voz passaria com o jogador ainda em transição; (b) downed do Fika (`IsAlive=false` aguardando revive) não tem entrada em `BlackoutTimers` — um `SetAim(true)` residual/enfileirado dentro de uma janela de lockout ativa tocaria OnAgony num corpo caído.

**Por que importa:** Reachability baixa (lockout é 1,0–1,5 s), mas o corner "desmaio durante ADS/lockout → NENHUMA voz do 005 durante inconsciência" é AC explícito da funcional, e o mod agora TEM um predicado canônico para isso — divergir dele é exatamente o tipo de des-alinhamento entre consumidores que o item 011 vai ter que reconciliar.

**Sugestão:** Trocar o guard da voz em §5 (`TryBlockReAds`) e §1.6 por: `TraumaState.BlackoutTimers.ContainsKey(p.ProfileId) || TraumaState.IsFainted || p.HealthController == null || !p.HealthController.IsAlive → sem voz` (espelho declarado de `TraumaFallCycleConsumer.cs:235-236`). O bloqueio do re-ADS em si continua (só a voz é suprimida) — mesma semântica já especificada p/ blackout.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

✅ **Aplicado (2026-07-19):** guard da voz em §1.6/§5 trocado pelo espelho declarado do predicado canônico do 004 (`TraumaFallCycleConsumer.cs:235-236`: BlackoutTimers ‖ IsFainted ‖ HealthController null ‖ !IsAlive → sem voz; bloqueio do re-ADS continua); corner de desmaio do §8 estendido à janela de wake e ao downed Fika.

### PA-02-07 · A — Gap de Especificação · 🟢 Menor

**§9 check 5 não enumera os estáticos novos que a própria rodada 1 introduziu**

**Problema:** O §9 check 5 afirma "nada estático sobrevive além de `_timerClampWarned`/cache de reflection (intencionais, por sessão)". A aplicação da PA-01-05 introduziu `private static bool _reestablishStormWarned` (stub §5, campo do consumidor — warn 1×/sessão) e a PA-01-06 formalizou `TraumaTremor.Owned/OwnedPlayer/OwnedAnchor` como estáticos de classe estática (limpos por `Remove`/`Discard`, mas estáticos). A enumeração do check ficou desatualizada em relação ao próprio texto que a r1 escreveu — inconsistência interna da aplicação.

**Por que importa:** O check 5 é o inventário auditável de estado entre raids (skill csharp §2: "every static collection/flag has a documented clear point"); um estático fora da lista é exatamente o que o code-review vai cobrar com CR novo.

**Sugestão:** Reescrever a evidência do check 5: "nada estático sobrevive além de `_timerClampWarned`, `_reestablishStormWarned` (warns 1×/sessão intencionais) e cache de reflection; `TraumaTremor.Owned/OwnedPlayer/OwnedAnchor` são estáticos LIMPOS nos caminhos Remove/Discard (sweeps §5)".

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

✅ **Aplicado (2026-07-19):** evidência do §9 check 5 reescrita — enumera `_timerClampWarned`, `_reestablishStormWarned` (warns 1×/sessão) e cache de reflection, e declara `TraumaTremor.Owned/OwnedPlayer/OwnedAnchor` como estáticos LIMPOS nos caminhos Remove/Discard.

### PA-02-08 · A — Gap de Especificação · 🟢 Menor

**Âncoras do Plugin deslocadas pelo 004 — `Plugin:238-263`, `:265-287` e `:113` não batem mais**

**Problema:** O 004 adicionou ~55 linhas ao `TRLImmersiveCombatMedicinePlugin.cs` (configs da seção Queda, terceiro bloco de delete de key órfã, componente novo). As âncoras da spec ficaram para trás: §1.7 "migração mojibake ... (Plugin:238-263)" → hoje o bloco vive em `:256-288`; §1.8/§4/§5 "padrão 003, Plugin:265-287" (delete da key `Legs Effects (item 003)`) → hoje `:290-312`; e o placeholder `"Arms Effects (item 005)"` (citado no veredito da r1 como Plugin:113) → hoje `:124-125`. O conteúdo existe intacto — só as linhas migraram.

**Por que importa:** Âncora não-resolvível quebra a verificação mecânica do code-review (convenção `arquivo.cs:linha` do repo); num arquivo que o 005 vai EDITAR, âncora errada custa retrabalho de localização.

**Sugestão:** Re-ancorar: mojibake → `Plugin:256-288`; padrão de delete de placeholder → `Plugin:290-312` (e citar o bloco IRMÃO mais novo do 004, `Plugin:314-334` — `Fall Cycle (item 004)` — como template literal a replicar p/ `"Arms Effects (item 005)"`); placeholder → `Plugin:124-125`.

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

✅ **Aplicado (2026-07-19):** âncoras do Plugin re-apontadas (mojibake → `Plugin:256-288`; delete de placeholder → `Plugin:290-312` com o template irmão do 004 `Plugin:314-334` citado; placeholder `"Arms Effects (item 005)"` → `Plugin:124-125`) em §1.7/§1.8/§4/§5/§8 — todas conferidas no arquivo v1.5.0 antes da re-âncora.

## Histórico

| Data | Evento |
|---|---|
| 2026-07-19 | Review técnica 02 criada via `/review-technical-spec` (rodada 2 adversarial: aplicação dos 8 achados da r1 verificada ✅ 8/8 coerente — 2 inconsistências derivadas viram PA-02-06/PA-02-07; foco em defasagens spec-005 × código v1.5.0 pós-item-004 fd799426 — 4 achados de drift: versão, seção 8, HealthPatches, âncoras do Plugin; `TraumaVoice` novo do 004 sem decisão de unificação; âncoras novas da r1 confirmadas reais: FikaBackendUtils.cs:49, ObservedPlayer.cs:737-751) |
| 2026-07-19 | Aplicação da rodada 2 via `/apply-code-review`: 8/8 achados ✅ Aplicados (0 refutados) — contadores zerados; PA-02-03 resolvido pelo caminho alternativo (extensão `TraumaVoice.TryPlayStrong` accept-gated, canal separado por consumidor — 004 inalterado); âncoras novas conferidas no código v1.5.0 antes de citar; Status da spec → Pronto para /code-mod (0 pendências) — gate 2x2 fechado |
