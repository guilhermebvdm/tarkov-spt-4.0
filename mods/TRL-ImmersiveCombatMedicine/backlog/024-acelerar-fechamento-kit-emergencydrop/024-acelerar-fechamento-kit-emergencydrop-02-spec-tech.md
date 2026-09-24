# 024 — Descarte instantâneo das mãos no EmergencyDrop · Spec Técnica

**Mod:** TRL-ImmersiveCombatMedicine
**Spec funcional:** [024-acelerar-fechamento-kit-emergencydrop-01-spec.md](024-acelerar-fechamento-kit-emergencydrop-01-spec.md)
**Criado:** 2026-09-12

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT deve citar `arquivo.cs:linha`.

## 1. Estratégia

**Não é um novo Harmony patch.** É uma correção de sequenciamento em `EmergencyDrop()` (`BandAidController.cs`), trocando a tentativa de reequipar a arma via o caminho normal (que sempre passa por um delay fixo, não configurável) por um caminho que descarta as mãos **antes**, de forma síncrona, usando uma API pública que o próprio jogo já expõe e usa internamente.

**Achado central (o que muda tudo nesta spec):** o tempo de "guardar o kit" **não é uma animação acelerável nem uma trava condicional** — é um `Task.Delay(600)` **fixo, incondicional**, dentro de `Player.MedsController.Drop` ([`Player.cs:19967-19977`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L19967-L19977)):

```csharp
public override void Drop(float animationSpeed, Action callback, bool fastDrop = false, Item nextControllerItem = null)
{
    method_2(callback).HandleExceptions();
}

public async Task method_2(Action callback)
{
    await Task.Delay(600);
    base.Destroyed = true;
    this.ObservedMedsControllerClass.HideWeapon(callback);
}
```

Os parâmetros `animationSpeed` e `fastDrop` são recebidos e **nunca lidos** dentro do método — não existe forma de acelerar isso passando um valor diferente. Isso invalida a abordagem original cogitada (acelerar via `RemoveLeftHandItem(speed)`, que nem chega a ser o mecanismo relevante aqui — ver `Fora de escopo`/histórico da spec funcional).

**O caminho que funciona:** `Player.DestroyController()` ([`Player.cs:31676-31688`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L31676-L31688)) descarta o controlador de mãos atual **de forma síncrona**, sem passar por `Drop()`/`Task.Delay`:

```csharp
public void DestroyController()
{
    Item item = HandsController.Item;
    FastForwardCurrentOperations();
    HandsController.Destroy();
    GEventArgs10[] array = InventoryController.SelectEvents<GEventArgs10>(item).ToArray();
    foreach (GEventArgs10 activeEvent in array)
    {
        InventoryController.RemoveActiveEvent(activeEvent);
    }
    UnityEngine.Object.Destroy(HandsController);
    HandsController = null;
}
```

`MedsController.Destroy()` (a versão chamada por `HandsController.Destroy()` acima, quando o controlador atual é o do kit médico) é confirmadamente síncrona ([`Player.cs:19801-19807`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L19801-L19807)) — sem `Task.Delay`, sem animação, sem espera:

```csharp
public override void Destroy()
{
    _player.ProceduralWeaponAnimation.ClearPreviousWeapon();
    base.Destroy();
    firearmsAnimator_0 = null;
    AssetPoolObject.ReturnToPool(_controllerObject.gameObject);
}
```

**Por que isso libera a arma na hora:** `Player.Process<TController,TResult>.Execute()` ([`Player.cs:22494-22538`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L22494-L22538) — o pipeline genérico por trás de `TrySetLastEquippedWeapon`/`SetInHands`, já mapeado na spec técnica do item `023`) checa logo no início:

```csharp
if (Player_0.HandsController == null)
{
    execute();
    return;
}
```

Se `HandsController` já é `null` (o que `DestroyController()` garante), o pipeline **pula inteiramente** o passo `DropCurrentController`/`Drop()` (onde mora o `Task.Delay(600)`) e vai direto criar o controlador da arma. Chamando `doctor.DestroyController()` **antes** de `doctor.TrySetLastEquippedWeapon(...)`, a troca de mãos acontece sem nenhuma espera.

**Escopo: exclusivo a itens de cirurgia (decisão do usuário, 2026-09-12).** O `Task.Delay(600)` em `method_2` é incondicional pra qualquer item `MedsItemClass` (bandagem, tala, torniquete, medkit comum, CMS, Surv12 — todos passam pelo mesmo `MedsController.Drop`), mas só nos itens de cirurgia (CMS/Surv12) isso corresponde a uma animação visível de "fechar o kit" incômoda o bastante pra justificar o mecanismo deste item; nos demais, o encerramento vanilla já é curto o suficiente pra ser confortável. Por isso, a tecla de drop de emergência passa a **só produzir efeito quando `ItemStats.IsSurgery == true`** — pra qualquer outro item médico, apertar a tecla não faz nada (nem cancela, nem dropa). Isso é reforçado em dois pontos (ver §5): o gate de entrada em `Update()` (nem chama `EmergencyDrop()`) e uma re-checagem defensiva no início do próprio `EmergencyDrop()` (retorna sem efeito colateral se, por algum caminho futuro, for chamado fora desse contexto).

**Isso também fecha, por auditoria, a dúvida sobre outros overrides de `AbstractHandsController.Destroy()`.** `Destroy()` é virtual com 8 overrides em `Player.cs` (linhas 2225, 13532, 15531, 17516, 17982, 18299, 19801, 21698) — só o de `Player.MedsController` (`19801-19807`) foi auditado. Como `Player.MedsController` ([`Player.cs:19439`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L19439), `public class MedsController : ItemHandsController, GInterface203, IOnHandsUseCallback, IHandsController`) é a **única** classe de hands-controller usada para qualquer item `MedsItemClass` — confirmado pelo guard interno `if (MedsController_0.Item is MedsItemClass)` ([`Player.cs:19486`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L19486)) — e `EmergencyDrop()` só é alcançável com `_isHealingInProgress == true` (que só fica `true` dentro de `HealRoutine`, sempre com um `MedsItemClass` nas mãos), `doctor.HandsController` no instante em que `DestroyController()` é chamado só pode ser `null` (já liberado) ou `Player.MedsController` — nunca um dos outros 7 overrides. Com o escopo agora restrito a CMS/Surv12, essa garantia vale a fortiori (o universo de itens possíveis é ainda menor).

**Alternativas descartadas:**
- Patch Harmony direto em `MedsController.Drop`/`method_2` pra pular o `Task.Delay` — descartado por decisão do usuário: mexer num método central usado por qualquer descarte de kit médico (inclusive fora do escopo deste mod) é uma superfície de risco maior do que necessário, quando existe uma API pública e já usada pelo próprio jogo (`DestroyController`) que resolve o mesmo problema sem tocar em código do jogo.
- Acelerar via `RemoveLeftHandItem(speed)` — descartado: essa via controla um sistema diferente (acessórios de mão esquerda, ex. lanterna/bússola), não o `Drop()` do controlador de mãos principal; não tem efeito no cenário deste item (ver spec funcional, histórico).
- Aplicar o mecanismo a qualquer item médico (não só cirurgia) — descartado por decisão do usuário: itens não-cirúrgicos já têm um encerramento curto o bastante pra ser confortável com o comportamento vanilla; restringir o escopo reduz a superfície de mudança sem perder o objetivo real do item.

## 2. Pontos de patch

Nenhum ponto de patch no Assembly — mudança 100% em `mods/TRL-ImmersiveCombatMedicine/modded-V4/Patches/Medical/BandAidController.cs`. Tabela dos métodos do Assembly cujo uso muda:

| Alvo (Assembly) | Tipo de uso | Motivo |
|---|---|---|
| [`Player.cs:31676`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L31676) `DestroyController()` | Chamada nova | Descarta o controlador de mãos atual de forma síncrona, sem o delay de `Drop()`. |
| [`Player.cs:31800`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L31800) `TrySetLastEquippedWeapon(bool, Callback)` | Call site já existente (item 023) | Sem mudança de assinatura — a diferença é o **contexto** em que é chamado (depois de `HandsController` já estar `null`). |

## 3. Novas propriedades F12 (BepInEx)

N/A — nenhum `ConfigEntry` novo.

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded-V4/Patches/Medical/BandAidController.cs` | MODIFICAR | `EmergencyDrop`: adiciona `doctor.DestroyController()` antes de `TrySetLastEquippedWeapon`; adiciona penalidade de carga por cancelamento tardio (mesma regra de `CancelHealInProgress`). |

Nenhum arquivo novo.

## 5. Stubs de código

> O PASSO 2 (`CancelApplyingItem`/`ForceFinishAnimation`, item 023) **não muda** — continua necessário pra limpeza interna do controlador de meds (desinscrever eventos, disparar o callback de conclusão do próprio `ObservedMedsControllerClass`). O `DestroyController()` é um passo **adicional**, depois disso.

**Novo campo de estado** (guarda se a cura em andamento é de cirurgia — computado 1x em `HealRoutine`, evita recalcular `ItemStats` por frame no gate do `Update()`):

```csharp
// BandAidController.cs — novo campo, junto dos outros campos de estado da cura (~linha 33)
private bool _currentHealIsSurgery = false;
```

**Gate de entrada em `Update()`** — resolve a exclusividade a itens de cirurgia (decisão do usuário): a tecla não chega a chamar `EmergencyDrop()` nem consome o timer de Hold/DoubleTap pra itens não-cirúrgicos.

```csharp
// BandAidController.cs — dentro de Update(), substitui a linha ~172
// === EMERGENCY DROP === (ref: item 024 — exclusivo a itens de cirurgia)
if (_isHealingInProgress && _currentHealIsSurgery && CheckPressMode(_emergencyDropKey.Value, _emergencyDropMode.Value,
    ref _emergencyHoldTimer, ref _emergencyHoldTriggered, ref _emergencyLastTapTime))
{
    EmergencyDrop();
    return;
}
```

**Setar/resetar `_currentHealIsSurgery`** — espelha exatamente onde `_itemBeingUsed` já é setado/limpo, pra nunca ficar dessincronizado:

```csharp
// HealRoutine — logo após "_itemBeingUsed = itemUsed;" (~linha 554)
_currentHealIsSurgery = stats.IsSurgery;

// CleanupHealState, EmergencyDrop (PASSO 1) e CancelHealInProgress — junto de "_itemBeingUsed = null;"
_currentHealIsSurgery = false;

// ResetAllState — junto dos outros resets de flag de cura
_currentHealIsSurgery = false;
```

**PASSO 2b (novo) — descartar as mãos instantaneamente**, com re-checagem defensiva de escopo (não depende só do gate do `Update()`):

```csharp
// BandAidController.cs — dentro de EmergencyDrop, entre o PASSO 2 (ForceFinishAnimation, já
// existente) e o PASSO 3 (TrySetLastEquippedWeapon, já existente do item 023).

// === NOVO PASSO 2b: DESCARTAR AS MÃOS INSTANTANEAMENTE (só cirurgia) ===
// ref: item 024 — Assembly-CSharp/EFT/Player.cs:31676-31688 DestroyController()
//      descarta HandsController de forma SÍNCRONA (sem passar por Drop()/Task.Delay(600),
//      confirmado em Player.cs:19967-19977 e :19801-19807). Com HandsController == null,
//      o TrySetLastEquippedWeapon logo abaixo pula o delay inteiro (Process<>.Execute,
//      Player.cs:22514-22518: "if HandsController == null, execute() direto").
// Re-checagem defensiva: mesmo que o gate do Update() já filtre por cirurgia, este passo
// não deve ter efeito nenhum se chamado fora desse contexto (AP-03 — escopo reforçado).
if (isSurgeryItem)
{
    try
    {
        if (doctor.HandsController != null)
        {
            doctor.DestroyController();
        }
    }
    catch (Exception ex)
    {
        TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning($"EmergencyDrop DestroyController: {ex.Message}");
    }
}
```

> Penalidade de carga por cancelamento tardio — mesma regra de `CancelHealInProgress` (`BandAidController.cs:792-806`, já existente), reaproveitada aqui em vez de duplicada como lógica nova. **Atualizado pós-review 01** (`PA-01-02`, `PA-01-03`): notificação diferenciada quando consome carga (mesmo padrão do cancelamento normal) e reset de `_healStartTime` após capturar `elapsed`.

```csharp
// BandAidController.cs — dentro de EmergencyDrop, calcular `elapsed`/`isSurgeryItem` no
// início do método (PASSO 1, mesmo padrão de CancelHealInProgress) e aplicar a penalidade
// antes do PASSO 4 (ThrowItem):

float elapsed = _healStartTime > 0f ? (Time.time - _healStartTime) : 0f;
_healStartTime = -1f; // ref: PA-01-03 — mesmo padrão de CancelHealInProgress:760, evita estado estático estagnado
ItemStats savedStats = savedItem != null ? ItemDatabase.GetStats(savedItem.TemplateId.ToString()) : null;
bool isSurgeryItem = savedStats != null && savedStats.IsSurgery;

// ... (PASSO 2, PASSO 2b guardado por isSurgeryItem, PASSO 3 como acima) ...

// ref: item 024 — mesma regra de CancelHealInProgress (BandAidController.cs:792-806):
// cancelamento com >= 1s de uso consome 1 carga do item, mesmo no drop de emergência.
bool itemConsumed = false;
if (elapsed >= 1.0f && savedItem != null && savedStats != null)
{
    bool isSurgeryOrUseItem = savedStats.IsSurgery || savedStats.HealAmount == 0 || savedStats.IsTourniquet;
    if (isSurgeryOrUseItem || savedItem.GetItemComponent<ResourceComponent>() != null)
    {
        MedicalLogic.ConsumeSafe(doctor, savedItem, 1.0f);
        itemConsumed = true;
    }
}
// PASSO 4 (ThrowItem) continua rodando de qualquer jeito — a penalidade é sobre a CARGA
// do item, não sobre o drop em si (o item some da mão e vai pro chão independente disso).

// ref: PA-01-02 — notificação diferenciada quando a penalidade é aplicada, mesmo padrão
// de CancelHealInProgress (BandAidController.cs:808-819). Substitui a notificação genérica
// "ItemDropped" que hoje roda incondicionalmente no fim de EmergencyDrop.
if (itemConsumed)
{
    NotificationManagerClass.DisplayMessageNotification(
        MedicLocale.Get(MedicTextId.TreatmentCancelledWithItemLoss, itemName ?? ""),
        ENotificationDurationType.Default, ENotificationIconType.Alert);
}
else
{
    NotificationManagerClass.DisplayMessageNotification(MedicLocale.Get(MedicTextId.ItemDropped),
        ENotificationDurationType.Default, ENotificationIconType.Alert);
}
```

## 6. Fluxo de dados

```
[EmergencyDrop — novo fluxo]
[A] CancelApplyingItem() + ForceFinishAnimation() (já existente, item 023) — limpeza interna
    do MedsController (eventos, callback próprio). HandsController AINDA é o MedsController
    aqui — método_9 nunca troca isso (achado do item 024, investigação anterior).
[B] doctor.DestroyController() (NOVO) → HandsController.Destroy() (Player.cs:19801-19807,
    síncrono) → HandsController = null.
[C] doctor.TrySetLastEquippedWeapon(true, callback) → TryProceed → Process<>.Execute()
    (Player.cs:22514-22518) → HandsController == null → pula DropCurrentController/Drop()
    inteiro → cria o controlador da arma imediatamente.
[D] Penalidade de carga (NOVO, se elapsed >= 1s) — mesma regra de CancelHealInProgress.
[E] ThrowItem(savedItem) (já existente, inalterado) — dropa o item no chão, independente de [D].
```

Comparação com o fluxo antigo: antes, `[C]` disparava `DropCurrentController` → `Drop()` → `Task.Delay(600)` → só depois criava o controlador da arma. O passo `[B]` novo elimina esse delay adiantando o descarte do controlador de meds pra ANTES da tentativa de reequipar. **Escopo:** todo o fluxo `[A]`-`[E]` só é alcançado quando `_currentHealIsSurgery == true` — pra qualquer outro item médico, o gate em `Update()` nem chega a chamar `EmergencyDrop()` (ver §5).

## 7. Riscos e dependências

- **`Player.AbstractProcess_0`/`ProcessStatus`** ([`Player.cs:22462-22492`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L22462-L22492)) — mecanismo interno do jogo que rastreia se existe uma troca de mãos "agendada"/pendente. **Investigado nesta revisão:** `Player_0.AbstractProcess_0 = this` só é atribuído dentro de `Begin()` (`Player.cs:22474`), que é chamado pelo delegate `execute` (`Player.cs:22504`) — o MESMO delegate que roda tanto no caminho instantâneo (`HandsController == null`, linha 22514-22518) quanto no caminho assíncrono antigo (depois do callback de `DropCurrentController`, linha 22525-22536). Ou seja, o bookkeeping de `AbstractProcess_0` roda de forma **idêntica** nos dois caminhos — `DestroyController()` não pula nem altera essa lógica, só decide qual dos dois caminhos `Execute()` escolhe no início. Risco rebaixado de "não confirmado" pra "confirmado por leitura direta, sem necessidade de teste em raid pra esse ponto específico" — mantido como validação de campo (§8) só por precaução (fragilidade histórica desta área do código, não por dúvida técnica pendente).
- **`FastForwardCurrentOperations()`** (chamado dentro de `DestroyController()`, `Player.cs:31662-31673`) já existe pra cobrir exatamente esse tipo de "descarte no meio de uma operação" — reforça que `DestroyController()` é uma via pensada pelo próprio jogo pra interrupções, não um uso indevido de API interna.
- **Item 023** (`SetInHands`/`TrySetLastEquippedWeapon` com callback) — este item não muda nada do que foi implementado lá; só muda o MOMENTO em que `TrySetLastEquippedWeapon` é chamado dentro de `EmergencyDrop`.
- **`GEventArgs10`** — `DestroyController()` já limpa esses eventos ativos do item descartado (`InventoryController.RemoveActiveEvent`), o mesmo tipo de bookkeeping que os itens `006`/`007`/`008` do FIKA (nesta sessão, noutro mod) trataram como causa raiz de travas de mãos — reforça que essa via é, se algo, mais completa que o fluxo atual, não menos.
- **Bandagem/tala/torniquete/medkit comum (não-cirúrgicos)** — **resolvido por decisão de escopo:** esses itens não acionam mais `EmergencyDrop()` (gate `_currentHealIsSurgery` em `Update()`, §5) — não é mais um corner case em aberto, é comportamento definido: a tecla não tem efeito neles.
- **`_currentHealIsSurgery` dessincronizado de `_itemBeingUsed`** — risco introduzido por este item: como é um campo novo espelhando `_itemBeingUsed`, um ponto de reset esquecido deixaria os dois incoerentes (ex.: `_itemBeingUsed == null` mas `_currentHealIsSurgery == true` de uma cura anterior). Mitigado setando/resetando nos EXATOS mesmos pontos onde `_itemBeingUsed` já é setado/limpo (§5) — checklist de implementação cobre isso explicitamente (§8).

## 8. Checklist de implementação

- [x] Adicionar o campo `_currentHealIsSurgery` e setar/resetar nos mesmos pontos de `_itemBeingUsed` (`HealRoutine`, `CleanupHealState`, `EmergencyDrop`, `CancelHealInProgress`, `DeactivateMedicMode`, `ResetAllState` — 6 pontos, confirmados via grep).
- [x] Alterar o gate de `Update()` (linha ~172) pra incluir `_currentHealIsSurgery` na condição — a tecla de EmergencyDrop só dispara durante cirurgia.
- [x] Em `EmergencyDrop`, calcular `elapsed`/`savedStats`/`isSurgeryItem` no início do método (PASSO 1), resetando `_healStartTime = -1f` logo em seguida (mesmo padrão de `CancelHealInProgress`).
- [x] Adicionar o PASSO 2b (`doctor.DestroyController()`, guardado por `isSurgeryItem`, com guard `if (doctor.HandsController != null)` e try/catch) logo após o PASSO 2 (`ForceFinishAnimation`) existente.
- [x] Adicionar a lógica de penalidade de carga (≥ 1s → `ConsumeSafe(doctor, savedItem, 1.0f)`), reaproveitando o mesmo critério de `CancelHealInProgress`.
- [x] Trocar a notificação final de `EmergencyDrop` pra diferenciar quando a penalidade foi aplicada (`TreatmentCancelledWithItemLoss`) do caso sem penalidade (`ItemDropped`), mesmo padrão de `CancelHealInProgress`.
- [x] Confirmar por compilação real que `DestroyController()` é acessível publicamente do jeito que a spec assume (sem reflection) — `dotnet build -c Release` em `modded-V4`: 0 Erros, 0 Warnings.
- [ ] Validar manualmente: usar um kit cirúrgico (CMS ou Surv12), acionar o drop de emergência, confirmar que a arma atira **no mesmo instante** (sem o intervalo de antes). _(pendente — validação in-game, P-13.1)_
- [ ] Validar manualmente: usar bandagem/tala/torniquete/medkit comum, acionar a tecla de drop de emergência — confirmar que **nada acontece** (sem cancelar, sem dropar). _(pendente — P-13.1)_
- [ ] Validar a penalidade: acionar o drop de emergência (cirurgia) depois de ≥ 1s de uso, confirmar que consome 1 carga e mostra a notificação de perda; acionar com < 1s, confirmar que não consome e mostra a notificação genérica. _(pendente — P-13.1)_
- [ ] Validar em raid Headless/coop — sem trava de mãos, sem log de colisão do lado FIKA (itens 003/004/006/007/008), tanto pro jogador que aciona quanto pra quem observa a cura. _(pendente — P-13.1)_

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes (`GameWorld.OnDestroy` + `BaseLocalGame.Stop`) — AP-01 | N/A | Nenhum estado novo entre raids, nenhum hook de lifecycle novo — `EmergencyDrop` continua um método síncrono chamado por input do jogador, mesmo escopo temporal de hoje. `_currentHealIsSurgery` é resetado em `ResetAllState()` (§5), não atravessa raids. |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | Não é um patch novo; `EmergencyDrop` já só roda pro `MainPlayer` local (mesmo Ownership Guard documentado no item 023). |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; TODOS os overrides auditados — AP-03 | ✅ | `Destroy()` é virtual com 8 overrides em `Player.cs` — auditado que só `MedsController.Destroy()` (`:19801-19807`) é alcançável neste call site, pois `Player.MedsController` é a única classe usada por qualquer `MedsItemClass` (`:19439`, `:19486`) e, com o escopo restrito a cirurgia (§1), o universo de itens é ainda menor. Ver §1 "Isso também fecha, por auditoria...". |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | ✅ | `DestroyController()` é a própria API pública que o jogo usa internamente pra esse fim (não é bypass nem gambiarra) — side-effects mapeados em §6/§7 (limpeza de `GEventArgs10`, `FastForwardCurrentOperations`). |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | Ver spec funcional "Estado entre raids: N/A" — sequenciamento local de uma única ativação, sem estado persistente. Corner case de morte simultânea coberto na spec funcional. |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade (incl. estado neutro) — AP-05 | N/A | Nenhum `ConfigEntry` novo (§3). |
| 7 | Re-invocação de método patcheado tem reentry-guard/`ReversePatch` (sem recursão infinita) — AP-07 | N/A | Não é um patch — não há recursão de patch envolvida. |
| 8 | Flags/caches de intercept validados contra o contexto atual após troca (arma/operação/tela) — AP-08 | ✅ | `_currentHealIsSurgery` é setado/resetado nos mesmos pontos de `_itemBeingUsed` (§5/§7), nunca fica com valor de uma cura anterior. |
| 9 | Todo patch-point reconfirmado no `.cs` do dump (não só no recon); "não existe" conferido no `types-index.json`, nunca num grep vazio — AP-09 | ✅ | `DestroyController` (`Player.cs:31676`), `MedsController.Destroy` (`:19801`), `MedsController.Drop`/`method_2` (`:19967-19977`), `Process<>.Execute` (`:22494-22538`), `Begin()`/`AbstractProcess_0` (`:22462-22504`), `MedsController` (`:19439`, `:19486`) todos lidos diretamente nesta sessão, com o decompile já gerado (item 023). |
| 10 | Skill EFT usada como lever confirmada não-inerte (`SkillsSettings` ≠ `[]` no `globals.json`); se inerte, efeito entregue por patch direto — AP-10 | N/A | Não envolve skills de personagem. |
| 11 | Pacote FIKA próprio: envelope de comprimento + só `TryGet*` + flag `Valid`, campos resetados no `Deserialize`, envio só na main thread, registro por instância/evento (nunca `bool`), zero `UnregisterPacket`, airbag com throttle em todo callback — AP-11 | N/A | Nenhum `INetSerializable` novo ou modificado — puramente sequenciamento de chamadas locais. |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-12 | Spec técnica criada via `/create-technical-spec`, após pivô de abordagem na spec funcional (de "acelerar animação" pra "descarte instantâneo"). Achado central: `MedsController.Drop` tem um `Task.Delay(600)` fixo que ignora parâmetros de velocidade (`Player.cs:19967-19977`); `Player.DestroyController()` (`Player.cs:31676-31688`) é a API pública que evita esse delay por completo, confirmada síncrona (`MedsController.Destroy`, `Player.cs:19801-19807`) e já usada pelo próprio jogo pra esse fim. |
| 2026-09-12 | `/review-technical-spec` 01 — 2 pontos 🟡 (auditoria AP-03 incompleta; feedback de notificação ausente) + 1 🟢 (`_healStartTime` não resetado), 0 🔴. Todos resolvidos nesta mesma sessão. |
| 2026-09-12 | Escopo restrito a itens de cirurgia (CMS/Surv12) por decisão do usuário — adicionado campo `_currentHealIsSurgery`, gate em `Update()`, re-checagem defensiva no PASSO 2b. Isso também fechou PA-01-01 (auditoria AP-03: universo de hands-controller possíveis reduzido a `MedsController`) e o corner case de bandagem/tala da spec funcional. PA-01-02 resolvido com notificação diferenciada (`TreatmentCancelledWithItemLoss`/`ItemDropped`); PA-01-03 resolvido com reset de `_healStartTime`. Investigação adicional de `Begin()`/`AbstractProcess_0` (`Player.cs:22462-22504`) confirmou que o bookkeeping de processo pendente roda igual nos dois caminhos (instantâneo e assíncrono) — risco antes "não confirmado" agora está confirmado por leitura direta. |
