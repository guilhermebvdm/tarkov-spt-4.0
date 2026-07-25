# 009 — Coop/bots: hardening do Trauma 2.0 · Spec Técnica

**Mod:** TRL-ImmersiveCombatMedicine
**Spec funcional:** [009-coop-hardening-01-spec.md](009-coop-hardening-01-spec.md)
**Criado:** 2026-07-20

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT deve citar `arquivo.cs:linha`. Wiki SPT e fontes externas só como complemento.

> **Escopo desta spec técnica: SÓ A3 e A4.** A1 (formalizar `tarkin-ladders` no D20), A2 (consolidar suíte de compat) e o Bloco B (protocolos de teste manual) são trabalho de documentação/protocolo puro, sem tocar `modded/` — não precisam de spec técnica (ver [009-coop-hardening-01-spec.md](009-coop-hardening-01-spec.md) §Visão geral). A3 (voz dupla-fonte 004×005) e A4 (helper de `Update()`) são os únicos sub-itens que tocam código real.

## 1. Estratégia

**Não há patch Harmony nesta spec.** A3 e A4 são, respectivamente, uma decisão de design documentada e um refactor interno de C# — nenhum dos dois introduz ou remove um `ModulePatch`/alvo de `AccessTools`.

### A4 — helper de lifecycle compartilhado

**Padrão escolhido: `struct` auxiliar mutável (`TraumaConsumerLifecycle`) com um método `Tick(...)` que recebe callbacks (`Action`/`Func<bool>`) cacheados em campos, chamado 1x por `Update()` de cada consumidor.**

Os 4 consumidores (`TraumaLegsConsumer.cs:178-258`, `TraumaFallCycleConsumer.cs:216-260`, `TraumaArmsConsumer.cs:343-430`, `TraumaStomachConsumer.cs:116-149`) repetem EXATAMENTE o mesmo esqueleto de detecção em `Update()`:

1. `GameWorld gw = Singleton<GameWorld>.Instance;` → **mundo nulo** (raid-end): cleanup específico + `_trackedWorld = null; _wasActive = IsActive(); return;`.
2. `!ReferenceEquals(gw, _trackedWorld)` → **world-swap** (transit, sem passar por null): mesmo tipo de cleanup + `_trackedWorld = gw;`.
3. `bool active = IsActive();` → **toggle ON→OFF**: desfaz efeitos aplicados. **Toggle OFF→ON**: reestabelece do snapshot do motor, sem one-shot/toast.
4. `_wasActive = active;` seguido de early-return se `!active`.

Isso é a "DETECÇÃO" (o quádruplo de `if`s que decide QUAL dos 4 eventos aconteceu). A "AÇÃO" de cada evento é inteiramente específica por consumidor (ex.: Legs limpa um `Dictionary<Player, TraumaLine>` de caps; FallCycle desengata uma FSM de 6 fases; Arms desmonta hooks de evento + lockout; Stomach só cancela um one-shot pendente) — **o helper nunca tenta generalizar essa parte**, só a decisão de "aconteceu".

**Justificativa da escolha (vs. as 2 alternativas descartadas):**

| Alternativa | Por que NÃO |
|---|---|
| Classe base abstrata (`abstract class TraumaConsumerBase : MonoBehaviour`) | Exigiria remover `sealed` das 4 classes já entregues/revisadas (003🟢/004🟢/005🟢/006🟢), mover campos para `protected`, e mudar a superfície de `Awake()`/`Update()` de TODAS elas para overrides de template method — maior área de diff em código já testado, maior risco de regressão silenciosa (exatamente o risco que a spec funcional pede para tratar com rigor de item 007). MonoBehaviour + inheritance funciona no Unity, mas o ganho não compensa o raio de impacto. |
| Método estático puro com `ref GameWorld trackedWorld, ref bool wasActive` (sem struct) | Funciona, mas espalha 2 parâmetros `ref` em vez de 1 campo — call site mais verboso (`Helper.Tick(ref _trackedWorld, ref _wasActive, ...)` vs `_lifecycle.Tick(...)`) sem ganho de segurança adicional. O struct só empacota o que já eram 2 campos soltos em cada consumidor; não é mais "esperto", só mais legível no call site. |
| **`struct` mutável + callbacks (escolhida)** | Zero mudança na hierarquia de classes (as 4 continuam `sealed class : MonoBehaviour`), zero alocação de heap (struct value-type — ao contrário de uma classe/"session object" que exigiria `new` por consumidor), e o diff de cada consumidor fica restrito ao corpo de `Update()` + extração dos callbacks já existentes para métodos nomeados. Menor raio de impacto possível para o mesmo resultado. |

**Nota de conformidade (csharp-mod-best-practices §5):** o struct é deliberadamente **mutável** (não `readonly struct`) — é uma exceção documentada à recomendação geral "prefira `readonly struct` para dados imutáveis" (essa recomendação vale para dados-valor; aqui o único propósito do tipo é bookkeeping mutável de 2 campos, o mesmo papel que `List<T>.Enumerator` cumpre no BCL).

**Nota de conformidade (csharp-mod-best-practices §1, alocação em hot path):** os delegates (`Func<bool>`/`Action`) passados para `Tick()` são **cacheados 1x em `Awake()`**, nunca recriados a cada `Update()` — evita a alocação de closure por frame que uma chamada com method-group/lambda inline geraria (relevante: 4 consumidores × 4-5 delegates cada, se recriados por frame, seriam ~16-20 alocações Gen0/frame a 60 FPS).

### A3 — voz dupla-fonte 004×005

**Decisão: DOCUMENTAR a aceitação da colisão como está — nenhuma arbitragem implementada.**

Investigação técnica (evidência abaixo, §6): os dois emissores (`TraumaVoice.PlayStrong` do item 004 e `TraumaVoice.TryPlayStrong` do item 005) chamam o MESMO `Player.Speaker.Play(EPhraseTrigger.OnAgony, ETagStatus.Combat | ETagStatus.Dying, demand: true, importance: 100)` (`TraumaVoice.cs:21`, `:31`). O árbitro de fato já existe no motor de voz vanilla: `PhraseSpeakerClass.Play` (`PhraseSpeakerClass.cs:176-239`) rejeita qualquer chamada nova enquanto `Busy && importance <= Int_0` (`PhraseSpeakerClass.cs:207-211`) — como as duas chamadas usam a MESMA `importance:100`, **o primeiro chamador no instante vence; o segundo é engolido** (`Play` retorna `null`).

Não há cenário prejudicial sistemático identificado:
- **Precondição estreita:** exige o MESMO jogador local com pernas no ciclo de queda (FSM ativa) E braços em lockout de re-ADS (janela de 1-1,5s, `TraumaArmsConsumer.cs:274`) sobrepostos no tempo — os dois exigem membros DIFERENTES comprometidos simultaneamente, e o evento de queda (`OnFallExecuted`) é disparado no máximo 1x por fase Blocked, não continuamente.
- **O lado 005 (`TryPlayStrong`) já foi desenhado para tolerar a colisão:** `TryBlockReAds` (`TraumaArmsConsumer.cs:286-341`) re-tenta a cada 0,3s (piso `PA-02-06`) durante toda a janela de lockout e loga explicitamente `voice=skipped(busy|blocked)` quando engolido (`TraumaArmsConsumer.cs:322-328`) — ou seja, se perder a primeira disputa contra o `OnAgony` do 004, tem várias novas chances antes da janela fechar, e a perda fica OBSERVÁVEL em log (não silenciosa).
- **O lado 004 (`PlayStrong`) tem uma assimetria pré-existente** (não introduzida por A3): seu anti-spam interno (`TraumaVoice.cs:43-50`, `Allowed()`) marca o cooldown de 2s de forma OTIMISTA — carimba `_nextAllowed[key]` ANTES de saber se `Play()` de fato tocou. Se `PlayStrong` perder a disputa contra QUALQUER outra fala de `importance>=100` em curso (não só o `TryPlayStrong` do 005 — vale para qualquer fala do próprio jogo com a mesma prioridade), a voz daquele evento específico de queda é perdida sem retry. Essa assimetria é uma propriedade geral de `PlayStrong` herdada do item 004 (não específica da colisão 004×005) — **não é escopo do A3 "consertar"** (a spec funcional veda "melhorar" decisões já fechadas de itens anteriores sem razão nova e concreta; aqui a razão nova seria só a coincidência com o 005, que não muda a natureza do comportamento pré-existente).
- **Nenhum sintoma documentado:** varredura da memória do mod (`memory/sessions.md`, P-3.6/P-4.1 e toda a Sessão 4) registra a colisão como pendência de RECONCILIAÇÃO A INVESTIGAR, nunca como bug relatado (nenhum log de "voz nunca toca" ou reclamação do usuário).

Dado isso, a spec funcional (§A3, critério de aceite) permite fechar como "aceito sem mudança, com a razão documentada" — e é a opção escolhida. **Nenhuma mudança de comportamento**; só um comentário de decisão inline em `TraumaVoice.cs` (rastreável, no padrão já usado no mod para decisões de reconciliação) para que a próxima sessão não reabra a mesma investigação.

## 2. Pontos de patch

**N/A.** Nem A3 nem A4 tocam um alvo Harmony (`ModulePatch`/`AccessTools`). A4 é refactor interno; A3 é um comentário de decisão. Nenhuma linha de `references/eft-decompiled/` é patcheada por este item — as citações da Assembly abaixo são só EVIDÊNCIA da investigação (comportamento do `PhraseSpeakerClass.Play`, já vanilla e não-patcheado).

## 3. Novas propriedades F12 (BepInEx)

**N/A.** Nem A3 nem A4 introduzem `ConfigEntry`. Nenhuma seção nova em `PROPRIEDADES.md`.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Patches/Trauma/TraumaConsumerLifecycle.cs` | CRIAR | `struct` com o método `Tick(...)` — detecção comum de mundo nulo/world-swap/toggle ON↔OFF, delegando ação via callbacks. |
| `modded/Patches/Trauma/TraumaLegsConsumer.cs` | MODIFICAR | `Update()` passa a chamar `_lifecycle.Tick(...)`; corpo de detecção vira 4 métodos privados (`OnWorldGone/OnWorldSwap/OnToggleOff/OnToggleOn`) cacheados como delegates em `Awake()`. Lógica per-tick ativa (sweep + `PumpDeferred`/`PumpBotRestores`) inalterada. **PA-01-01 (review 1): remover os campos antigos `_wasActive`/`_trackedWorld` da classe** — a bookkeeping agora vive dentro de `_lifecycle`; sem a remoção, ficam mortos (nunca lidos/escritos fora do `Update()` antigo). |
| `modded/Patches/Trauma/TraumaFallCycleConsumer.cs` | MODIFICAR | Idem (ver §5 — 2º exemplo completo). **PA-01-01: remover `_wasActive`/`_trackedWorld` antigos.** |
| `modded/Patches/Trauma/TraumaArmsConsumer.cs` | MODIFICAR | Idem (segue o mesmo padrão dos 2 exemplos de §5 — ver nota ao final da seção). **PA-01-01: remover `_wasActive`/`_trackedWorld` antigos.** |
| `modded/Patches/Trauma/TraumaStomachConsumer.cs` | MODIFICAR | Idem — o mais simples dos 4 (sem estado contínuo próprio, só cancela one-shot pendente; `OnToggleOn` é `null`/no-op, igual ao comportamento atual). **PA-01-01: remover `_wasActive`/`_trackedWorld` antigos.** |
| `modded/Patches/Trauma/TraumaVoice.cs` | MODIFICAR | Comentário de decisão A3 acima de `PlayStrong`/`TryPlayStrong` (zero mudança de lógica/assinatura). |

## 5. Stubs de código

> Blocos compiláveis com assinatura completa e corpo mínimo plausível. Cada referência a algo do EFT tem `// ref: Assembly-CSharp/<arquivo>:<linha>`.

### 5.1 Helper compartilhado (`TraumaConsumerLifecycle.cs`, CRIAR)

```csharp
// modded/Patches/Trauma/TraumaConsumerLifecycle.cs
using System;
using Comfort.Common;
using EFT;

namespace TRLImmersiveCombatMedicine.Trauma
{
    /// <summary>Helper de DETECÇÃO compartilhado do esqueleto de Update() dos 4 consumidores de estado contínuo
    /// (003/004/005/006) — extraído no item 009 (A4; débito registrado em 006 code-review-01 CR-01-02 e
    /// 008 code-review-01 CR-01-01, ver memory/sessions.md P-4.1). Cobre SÓ os 4 eventos de lifecycle
    /// (mundo nulo, world-swap/transit, toggle ON→OFF, toggle OFF→ON); a AÇÃO de cada evento continua
    /// 100% no consumidor via callback — NUNCA generalizada aqui (mandato da spec funcional A4).
    /// `struct` mutável deliberada (não readonly) — bookkeeping de 2 campos, sem alocação de heap por
    /// consumidor (csharp-mod-best-practices §5, exceção documentada à regra "prefira readonly struct").</summary>
    internal struct TraumaConsumerLifecycle
    {
        // PA-01-03 (review 1): o campo `_lifecycle` em cada consumidor NUNCA pode ser declarado `readonly` —
        // Tick() muta o struct em-place; `readonly` faria o C# operar sobre uma cópia defensiva silenciosa
        // a cada chamada, quebrando a detecção de mundo/toggle SEM erro de compilação (bug silencioso).
        private GameWorld _trackedWorld;
        private bool _wasActive;

        /// <summary>Chamado 1x por Update() do consumidor, com os MESMOS 5 delegates cacheados em Awake()
        /// (nunca recriados por frame — csharp-mod-best-practices §1). Retorna o `active` corrente: o
        /// consumidor decide rodar (true) ou não (false, já tratado como early-return) sua lógica per-tick.
        /// Qualquer callback pode ser null (no-op) — ex.: TraumaStomachConsumer não tem ação de toggle-on.</summary>
        internal bool Tick(
            Func<bool> isActive,
            Action onWorldGone,
            Action onWorldSwap,
            Action onToggleOff,
            Action onToggleOn)
        {
            // ref: Comfort.Common.Singleton<T> — spt-mod-best-practices §2 (único Singleton correto)
            GameWorld gw = Singleton<GameWorld>.Instance;
            if (gw == null)
            {
                onWorldGone?.Invoke();
                _trackedWorld = null;
                _wasActive = isActive();
                return false;
            }
            if (!ReferenceEquals(gw, _trackedWorld))
            {
                onWorldSwap?.Invoke();
                _trackedWorld = gw;
            }

            bool active = isActive();
            if (_wasActive && !active) onToggleOff?.Invoke();
            else if (!_wasActive && active) onToggleOn?.Invoke();
            _wasActive = active;
            return active;
        }
    }
}
```

### 5.2 `TraumaLegsConsumer.cs` — antes/depois (exemplo 1 de 2)

Campos novos + `Awake()` (delegates cacheados) + `Update()` reduzido + callbacks extraídos (corpo idêntico ao `Update()` original, `TraumaLegsConsumer.cs:180-234`, só movido para métodos nomeados):

```csharp
// modded/Patches/Trauma/TraumaLegsConsumer.cs (trecho — só as partes que mudam)
private TraumaConsumerLifecycle _lifecycle; // PA-01-03: NUNCA marcar readonly (ver TraumaConsumerLifecycle.cs)
private Func<bool> _isActiveDelegate;
private Action _onWorldGone;
private Action _onWorldSwap;
private Action _onToggleOff;
private Action _onToggleOn;

private void Awake()
{
    _instance = this;
    TraumaConsumerRegistry.Register(TraumaConsumerId.LegsEffects, LegsRegions, IsActive);
    TraumaEngine.SubscribeWithSnapshot(OnTransition); // PA-02-01: pode invocar OnTransition sincronamente AQUI
    // (replay do motor, TraumaEngine.cs:89) — seguro pois OnTransitionCore nunca toca _lifecycle/delegates.
    TraumaEngine.OneShotPublished += OnOneShot;

    // ref: A4 (009) — cacheado 1x, nunca recriado no Update (csharp-mod-best-practices §1)
    _isActiveDelegate = IsActive;
    _onWorldGone = OnWorldGone;
    _onWorldSwap = OnWorldSwap;
    _onToggleOff = OnToggleOff;
    _onToggleOn = OnToggleOn;
}

// ref: corpo idêntico ao branch `gw == null` original (TraumaLegsConsumer.cs:181-190, exceto as 2 linhas
// de bookkeeping _trackedWorld/_wasActive, que agora são do struct — TraumaConsumerLifecycle.Tick)
private void OnWorldGone()
{
    if (_applied.Count > 0) _applied.Clear();
    TraumaPose.CancelAll("raid-end");
    TraumaPose.ClearBotRestores();
}

// ref: corpo idêntico ao branch world-swap original (TraumaLegsConsumer.cs:191-199)
private void OnWorldSwap()
{
    if (_applied.Count > 0) _applied.Clear();
    TraumaPose.CancelAll("raid-end");
    TraumaPose.ClearBotRestores();
}

// ref: corpo idêntico ao branch toggle ON→OFF original (TraumaLegsConsumer.cs:202-216)
private void OnToggleOff()
{
    _sweepScratch.Clear();
    foreach (KeyValuePair<Player, TraumaLine> kv in _applied) _sweepScratch.Add(kv.Key);
    _applied.Clear();
    for (int i = 0; i < _sweepScratch.Count; i++) RemoveCapGuarded(_sweepScratch[i]);
    _sweepScratch.Clear();
    TraumaPose.CancelKind(TraumaOneShotKind.InvoluntaryCrouch, TraumaRegion.Legs, "toggle-off");
    TraumaPose.FlushBotRestores();
    TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo("[Trauma2] legs consumer OFF — caps desfeitos");
}

// ref: corpo idêntico ao branch toggle OFF→ON original (TraumaLegsConsumer.cs:219-233); re-obtém gw
// (seguro: só é chamado pelo Tick quando gw != null no MESMO frame)
private void OnToggleOn()
{
    GameWorld gw = Singleton<GameWorld>.Instance;
    var players = gw.RegisteredPlayers;
    int established = 0;
    for (int i = 0; i < players.Count; i++)
    {
        var p = players[i] as Player;
        if (!TraumaEngine.IsOwnedHere(p)) continue;
        TraumaLine line = TraumaEngine.GetLine(p, TraumaRegion.Legs);
        if (line == TraumaLine.None || line == TraumaLine.LegsFallCycle) continue;
        ApplyCap(p, line);
        established++;
    }
    if (established > 0)
        TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"[Trauma2] legs consumer ON — {established} estado(s) estabelecido(s) do snapshot");
}

private void Update()
{
    bool active = _lifecycle.Tick(_isActiveDelegate, _onWorldGone, _onWorldSwap, _onToggleOff, _onToggleOn);
    if (!active) return;

    // ref: corpo idêntico à poda oportunista + pumps original (TraumaLegsConsumer.cs:239-257) — INALTERADO
    _sweepScratch.Clear();
    foreach (KeyValuePair<Player, TraumaLine> kv in _applied)
    {
        TraumaLine gl = TraumaEngine.GetLine(kv.Key, TraumaRegion.Legs);
        if (gl == TraumaLine.None || gl == TraumaLine.LegsFallCycle) _sweepScratch.Add(kv.Key);
    }
    for (int i = 0; i < _sweepScratch.Count; i++)
    {
        _applied.Remove(_sweepScratch[i]);
        RemoveCapGuarded(_sweepScratch[i]);
    }
    _sweepScratch.Clear();

    TraumaPose.PumpDeferred();
    TraumaPose.PumpBotRestores();
}
```

### 5.3 `TraumaFallCycleConsumer.cs` — antes/depois (exemplo 2 de 2)

```csharp
// modded/Patches/Trauma/TraumaFallCycleConsumer.cs (trecho — só as partes que mudam)
private TraumaConsumerLifecycle _lifecycle; // PA-01-03: NUNCA marcar readonly (ver TraumaConsumerLifecycle.cs)
private Func<bool> _isActiveDelegate;
private Action _onWorldGone;
private Action _onWorldSwap;
private Action _onToggleOff;
private Action _onToggleOn;

private void Awake()
{
    _instance = this;
    TraumaConsumerRegistry.Register(TraumaConsumerId.FallCycle, LegsRegions, IsActive);
    TraumaEngine.SubscribeWithSnapshot(OnTransition); // PA-02-01: pode invocar OnTransition sincronamente AQUI
    // (replay do motor, TraumaEngine.cs:89) — seguro pois OnTransitionCore nunca toca _lifecycle/delegates.
    TraumaEngine.OneShotPublished += OnOneShot;

    _isActiveDelegate = IsActive;
    _onWorldGone = OnWorldGone;
    _onWorldSwap = OnWorldSwap;
    _onToggleOff = OnToggleOff;
    _onToggleOn = OnToggleOn;
}

// ref: corpo idêntico ao branch `gw == null` original (TraumaFallCycleConsumer.cs:220-227, exceto o
// bookkeeping _trackedWorld/_wasActive, agora do struct)
private void OnWorldGone()
{
    if (_phase != FallPhase.None) Disengage("raid-end");
    TraumaBotFall.ClearAll();
    TraumaVoice.Clear();
    TraumaSpeedCap.Clear();
}

// ref: corpo idêntico ao branch world-swap original (TraumaFallCycleConsumer.cs:229-236)
private void OnWorldSwap()
{
    if (_phase != FallPhase.None) Disengage("world-swap");
    TraumaBotFall.ClearAll();
    TraumaVoice.Clear();
    TraumaSpeedCap.Clear();
}

// ref: corpo idêntico ao branch toggle ON→OFF original (TraumaFallCycleConsumer.cs:239-244)
private void OnToggleOff()
{
    if (_phase != FallPhase.None) Disengage("toggle-off");
    TraumaBotFall.ReleaseAll("toggle-off");
}

// ref: corpo idêntico ao branch toggle OFF→ON original (TraumaFallCycleConsumer.cs:246-252)
private void OnToggleOn()
{
    GameWorld gw = Singleton<GameWorld>.Instance;
    Player mp = gw.MainPlayer; // ref: Assembly-CSharp/EFT/GameWorld.cs:572
    if (mp != null && TraumaEngine.IsOwnedHere(mp)
        && TraumaEngine.GetLine(mp, TraumaRegion.Legs) == TraumaLine.LegsFallCycle)
        Engage(mp, establishing: true);
    TraumaBotFall.EstablishFromSnapshot(gw);
}

private void Update()
{
    bool active = _lifecycle.Tick(_isActiveDelegate, _onWorldGone, _onWorldSwap, _onToggleOff, _onToggleOn);
    if (!active) return;

    // ref: corpo idêntico ao original (TraumaFallCycleConsumer.cs:257-259) — INALTERADO
    TickHumanCycle();
    TraumaPose.PumpDeferred();
    TraumaBotFall.Pump();
}
```

**`TraumaArmsConsumer.cs` e `TraumaStomachConsumer.cs` seguem o MESMO padrão** (mesmos 6 membros novos — `_lifecycle` + 4 delegates cacheados + `Update()` reduzido a 1 chamada + early-return):

- **Arms:** `OnWorldGone`/`OnWorldSwap` viram, cada um, uma chamada a `TearDownLocal(reason, worldDead: true)` + `ResetLockout()` — **PA-01-02 (review 1): corpo idêntico a `TraumaArmsConsumer.cs:350-351`/`359-360` (só as 2 chamadas), EXCETO o bookkeeping `_trackedWorld = null`/`_wasActive = IsActive()`/`_trackedWorld = gw`/`return`, que fica de fora do callback e agora vive dentro de `Tick()`** (mesma ressalva já dada ao FallCycle acima — não copiar essas linhas para dentro de `OnWorldGone()`/`OnWorldSwap()`); `OnToggleOff` = `TearDownLocal("toggle-off")` + `ResetLockout()` + log (`:369-371`); `OnToggleOn` = bloco do snapshot (`:378-387`), re-obtendo `gw.MainPlayer` dentro do callback. Lógica per-tick ativa (poda + watchdog + deadline do timer, `:392-429`) permanece 100% fora do helper, chamada após `if (!active) return;`.
- **Stomach:** `OnWorldGone`/`OnWorldSwap`/`OnToggleOff` viram, cada um, 1 chamada a `TraumaPose.CancelKind(TraumaOneShotKind.InvoluntaryCrouch, TraumaRegion.Stomach, "<razão>")` (corpo idêntico a `TraumaStomachConsumer.cs:122-123`/`128-129`/`137-138`); `OnToggleOn` é **`null`** (o comentário original — `:140`, "Religar mid-raid: NADA a estabelecer" — já não tinha corpo, então o parâmetro correspondente do `Tick()` é passado como `null`, e `?.Invoke()` trata isso como no-op, preservando o comportamento exato).

### 5.4 `TraumaVoice.cs` — comentário de decisão A3 (MODIFICAR, zero mudança de lógica)

```csharp
// modded/Patches/Trauma/TraumaVoice.cs (trecho — só o comentário novo, assinaturas INALTERADAS)

/// <summary>FORTE (queda executada + tentativa negada): OnAgony com importance explícita — fura o Busy do
/// Speaker em tiroteio (demand só fura OnDemandOnly+roll — correção P5).
/// ref: PhraseSpeakerClass.cs:175/206-227; EPhraseTrigger.cs:6.
/// DECISÃO A3 (009, 2026-07-20): compete pelo MESMO Speaker/importance:100 com TryPlayStrong (005/lockout
/// de re-ADS). Investigado e ACEITO sem arbitragem — precondição estreita (pernas em ciclo de queda E braços
/// em lockout no MESMO player, sobrepostos no tempo), sem sintoma documentado (memory/sessions.md P-3.6/P-4.1),
/// e o lado 005 já tolera a perda via retry 0,3s + log voice=skipped (ArmsConsumer.TryBlockReAds). O motor
/// vanilla (PhraseSpeakerClass.Play, Busy && importance<=Int_0 → :207-211) já arbitra "primeiro chega, leva" —
/// nenhuma camada adicional foi criada por cima dessa garantia (spec funcional 009 corner A3).</summary>
internal static void PlayStrong(Player p)
{
    if (!Allowed(p, strong: true)) return;
    p.Speaker?.Play(EPhraseTrigger.OnAgony, ETagStatus.Combat | ETagStatus.Dying, demand: true, importance: 100);
}
```

## 6. Fluxo de dados

### A4 — detecção compartilhada

```
[A] Unity MonoBehaviour.Update() (1x/frame, por consumidor)
  → [B] TraumaConsumerLifecycle.Tick() — TraumaConsumerLifecycle.cs (novo)
      detecta: mundo nulo? (Singleton<GameWorld>.Instance == null)
               world-swap? (!ReferenceEquals(gw, _trackedWorld))
               toggle ON→OFF / OFF→ON? (_wasActive vs IsActive())
    → [C] callback específico do consumidor (OnWorldGone/OnWorldSwap/OnToggleOff/OnToggleOn) —
          AÇÃO ORIGINAL, movida verbatim (ex.: TraumaLegsConsumer.OnToggleOff limpa `_applied` +
          RemoveCapGuarded por player — TraumaLegsConsumer.cs:202-216 antes da extração)
      → [D] retorno bool `active` → consumidor decide (`if (!active) return;`) rodar sua lógica
            per-tick própria (TickHumanCycle / PumpDeferred / deadline de timer / etc.) — NUNCA
            tocada pelo helper
```

### A3 — colisão de voz (investigação, sem mudança de fluxo)

```
[A] Evento físico local: queda executada (TraumaFallCycleConsumer.OnFallExecuted, item 004)
    OU tentativa de re-ADS durante lockout (TraumaArmsConsumer.TryBlockReAds, item 005)
  → [B] TraumaVoice.PlayStrong(p) [004: fire-and-forget, anti-spam otimista 2s, TraumaVoice.cs:18-22]
        OU TraumaVoice.TryPlayStrong(p) [005: sem anti-spam próprio, retorno honesto, TraumaVoice.cs:28-32]
    → [C] Player.Speaker.Play(OnAgony, Combat|Dying, demand:true, importance:100)
          ref: Assembly-CSharp/PhraseSpeakerClass.cs:176-239; Player.Speaker — EFT/Player.cs:24347
      → [D] Busy && importance <= Int_0 → SKIP (PhraseSpeakerClass.cs:207-211).
            Mesma importance (100) nos dois lados ⇒ o PRIMEIRO chamador no instante vence;
            o segundo é engolido. Nenhuma arbitragem cross-consumer adicionada (decisão A3, §1).
```

## 7. Riscos e dependências

- **Patches Harmony existentes em `modded/Patches/Trauma/`:** nenhum é tocado por A3/A4 — os patches que consultam os consumidores (ex.: `CantStandUpPatch`, `ArmsAimPatches`/`SetAim` prefix que chama `TryBlockReAds`) só usam os métodos `internal static` públicos dos consumidores (`IsBlockedPhase`, `TryGetApplied`, `TryBlockReAds`, etc.) — nenhuma dessas assinaturas muda.
- **Dependências chamadas pelos callbacks (todas INTOCADAS por este item):** `TraumaEngine` (motor, `SubscribeWithSnapshot`/`OneShotPublished`/`GetLine`/`IsOwnedHere`), `TraumaPose` (primitiva de agachar/cair), `TraumaBotFall`, `TraumaSpeedCap`, `TraumaTremor`, `TraumaVoice`. A4 só reorganiza QUEM chama essas APIs e QUANDO — nunca o quê elas fazem.
- **Ordem de inicialização:** `Awake()` de cada consumidor continua rodando antes de qualquer `Update()` no mesmo frame de spawn (garantia do Unity) — os novos campos de delegate são atribuídos em `Awake()`, então já existem no primeiro `Update()`. Nenhuma mudança na ordem relativa de `Awake()` entre os 4 consumidores (registro no `TraumaConsumerRegistry`, assinatura do motor) é necessária. **PA-02-01 (review 2):** `TraumaEngine.SubscribeWithSnapshot` (chamada em `Awake()` ANTES do cache dos delegates) pode invocar `OnTransition` sincronamente durante o próprio `Awake()`, se o motor já tiver registros ativos (`TraumaEngine.cs:72-96`, replay em `:89`). Isso é seguro porque `OnTransitionCore`/`OnOneShotCore` nunca leem/escrevem `_lifecycle` nem os delegates cacheados (`_isActiveDelegate`/`_onWorldGone`/`_onWorldSwap`/`_onToggleOff`/`_onToggleOn`) — são trilhas de estado independentes. Qualquer mudança futura que acople as duas trilhas precisa mover o cache dos delegates para ANTES de `SubscribeWithSnapshot`.
- **Fika/multiplayer:** nenhuma mudança — os 4 consumidores já eram dono-only (D16, herdado do motor); `Tick()` não lê nem publica nenhum estado de rede, só orquestra chamadas locais que já existiam.
- **Risco principal de A4 (regressão silenciosa):** a extração muda ONDE o código mora, não o quê ele faz — mitigado por (a) comparação linha-a-linha documentada em §5 (cada callback cita a linha exata do bloco original que substitui) e (b) o checklist de implementação (§8) exige uma passada de verificação estática comparando literalmente old vs. new antes de fechar o item, cobrindo o corner case da spec funcional ("toggle OFF mid-raid em cada um dos 4 continua desfazendo o efeito correspondente sem resíduo").
- **Risco de A3:** nenhum — é decisão documentada, zero mudança de comportamento. Dependência: `PhraseSpeakerClass` (vanilla, não patcheado, comportamento confirmado por leitura direta do Assembly em §1/§6).

## 8. Checklist de implementação

- [ ] Criar `modded/Patches/Trauma/TraumaConsumerLifecycle.cs` com o `struct TraumaConsumerLifecycle` e o método `Tick(...)` (§5.1).
- [ ] Migrar `TraumaLegsConsumer.cs`: adicionar os 6 campos novos (`_lifecycle` + 4 delegates) **e remover os 2 campos antigos (`_wasActive`/`_trackedWorld`) — PA-01-01**, extrair `OnWorldGone/OnWorldSwap/OnToggleOff/OnToggleOn`, cachear delegates em `Awake()`, reduzir `Update()` à chamada do helper + lógica per-tick já existente (§5.2).
- [ ] Repetir para `TraumaFallCycleConsumer.cs` (§5.3) — remover `_wasActive`/`_trackedWorld` antigos (PA-01-01).
- [ ] Repetir para `TraumaArmsConsumer.cs` (padrão descrito em §5.3, nota final) — remover `_wasActive`/`_trackedWorld` antigos (PA-01-01); atenção especial para NÃO copiar o bookkeeping (`_trackedWorld = null`/`_wasActive = IsActive()`/`return`) para dentro de `OnWorldGone()`/`OnWorldSwap()` — ver citação corrigida (PA-01-02).
- [ ] Repetir para `TraumaStomachConsumer.cs` (padrão descrito em §5.3, nota final — `OnToggleOn = null`) — remover `_wasActive`/`_trackedWorld` antigos (PA-01-01).
- [ ] **Nunca marcar o campo `_lifecycle` como `readonly` em nenhum dos 4 consumidores (PA-01-03)** — `Tick()` muta o struct em-place; `readonly` faria o C# operar sobre uma cópia defensiva silenciosa, quebrando a detecção sem erro de compilação.
- [ ] Adicionar o comentário de decisão A3 em `TraumaVoice.cs` acima de `PlayStrong` (§5.4) — sem tocar `TryPlayStrong` nem qualquer assinatura.
- [ ] Compilar via `/compile-mod` — 0 erros esperados (nenhuma API nova, só reorganização + 1 tipo novo interno).
- [ ] **Verificação de regressão (obrigatória, corner A4 da spec funcional):** para CADA um dos 4 consumidores, comparar `git diff` do `Update()` antigo vs. novo linha-a-linha, confirmando que toda condição, toda chamada e toda ORDEM relativa de execução persiste idêntica — nenhuma reordenação, nenhum "enquanto estou aqui" de melhoria.
- [ ] Regenerar o grafo do mod (`bash scripts/update-graphs.sh mods/TRL-ImmersiveCombatMedicine` / `/update-mod-graph TRL-ImmersiveCombatMedicine`) — arquivo novo + 4 arquivos modificados.
- [ ] Atualizar `mods/TRL-ImmersiveCombatMedicine/memory/sessions.md`: fechar a pendência P-4.1 (helper extraído) e registrar a decisão A3 (aceito sem arbitragem) na sessão que executar o `/code-mod`.

## 9. Conformidade com skills (auto-checklist)

> Preenchido ANTES de salvar. Cada linha: ✅ com evidência, ou **N/A + razão**. Taxonomia: [docs/technical/spt-antipatterns.md](../../../../docs/technical/spt-antipatterns.md).

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes (`GameWorld.OnDestroy` + `BaseLocalGame.Stop`) — AP-01 | ✅ | Este item não muda o MODELO de lifecycle (os 4 consumidores já detectam raid-end/world-swap por poll em `Update()`, padrão estabelecido em 002/003, não por patch em `GameWorld.OnDestroy`). A4 só relocaliza a MESMA detecção para `TraumaConsumerLifecycle.Tick()` — equivalência linha-a-linha em §5.2/§5.3, preservando os pontos de retorno idempotentes (`_trackedWorld = null` no null-branch, guard `!ReferenceEquals` no swap-branch). |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | N/A | Nenhum patch novo. Os filtros existentes (`IsYourPlayer`/`IsOwnedHere`/`p.IsAI`) vivem em `OnTransition`/`OnOneShot` de cada consumidor — código intocado por A3/A4 (só `Update()` e `Awake()` mudam). |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; TODOS os overrides auditados — AP-03 | N/A | Nenhum alvo Harmony/virtual patcheado por esta spec (§2). |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | N/A | A4 não introduz nenhuma mutação de estado nova — todas as chamadas (`RemoveCapGuarded`, `TearDownLocal`, `Disengage`, `ApplyCap`, etc.) já existiam e já foram auditadas nos code-reviews de 003-006; A4 só muda QUEM as invoca (callback nomeado em vez de bloco inline), nunca O QUÊ é invocado. |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | §5.2/§5.3 mostram que `OnWorldGone`/`OnWorldSwap` preservam EXATAMENTE as mesmas chamadas de cleanup (`TraumaPose.CancelAll`, `TraumaBotFall.ClearAll`, `TraumaVoice.Clear`, `TraumaSpeedCap.Clear`, `Disengage("raid-end"/"world-swap")`) nas mesmas condições — cobre raid1→raid2 (world-swap) e alt-F4/qualquer forma de fim de raid (mundo nulo) sem alterar o comportamento auditado em 003 code-review-1 achado 2. |
| 6 | Semântica/defaults/faixas de cada `ConfigEntry` sem ambiguidade (incl. estado neutro) — AP-05 | N/A | Nenhuma `ConfigEntry` nova (§3). |
| 7 | Re-invocação de método patcheado tem reentry-guard/`ReversePatch` (sem recursão infinita) — AP-07 | N/A | `Tick()` não é um alvo patcheado nem resulta de resurrection de operação — é chamado 1x por frame pelo próprio `Update()` do consumidor, sem caminho de re-entrada. |
| 8 | Flags/caches de intercept validados contra o contexto atual após troca (arma/operação/tela) — AP-08 | ✅ | A detecção de world-swap (`!ReferenceEquals(gw, _trackedWorld)`) É o mecanismo que já cobria esse antipattern nos 4 consumidores (fix original do 003, code-review 1 achado 2) — A4 preserva esse guard verbatim dentro de `Tick()`, sem enfraquecê-lo; nenhum cache novo introduzido. |

## Histórico

| Data | Evento |
|---|---|
| 2026-07-20 | Spec técnica criada via `/create-technical-spec`, cobrindo só A3 (decisão de voz dupla-fonte — documentada, sem arbitragem) e A4 (helper `TraumaConsumerLifecycle` como `struct` + callbacks cacheados). |
| 2026-07-20 | Review técnica 01 aplicada: PA-01-01 (remoção explícita de `_wasActive`/`_trackedWorld` antigos nos 4 consumidores), PA-01-02 (citação de linhas do Arms corrigida), PA-01-03 (aviso contra `readonly` no campo `_lifecycle`). 0 achados pendentes — segue para rodada 2 (plano de 2 rodadas dado o raio de impacto em código já entregue). |
| 2026-07-20 | Review técnica 02 (última rodada planejada) aplicada: PA-02-01 (documentado o replay síncrono de `SubscribeWithSnapshot` em `Awake()` e por que é seguro). 0 achados pendentes — pronta para `/code-mod`. |
