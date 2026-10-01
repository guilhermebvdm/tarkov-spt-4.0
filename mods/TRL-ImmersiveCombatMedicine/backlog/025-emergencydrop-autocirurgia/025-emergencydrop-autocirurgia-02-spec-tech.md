# 025 — EmergencyDrop na cirurgia própria (self-heal) · Spec Técnica

**Mod:** TRL-ImmersiveCombatMedicine
**Spec funcional:** [025-emergencydrop-autocirurgia-01-spec.md](025-emergencydrop-autocirurgia-01-spec.md)
**Criado:** 2026-09-12

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT deve citar `arquivo.cs:linha`.

## 1. Estratégia

**Não é um novo Harmony patch.** É um novo método privado em `BandAidController.cs` (`EmergencyDropSelf`), disparado pela MESMA tecla/config do item 024 (`_emergencyDropKey`/`_emergencyDropMode`), num contexto **mutuamente exclusivo** ao da cura de aliado.

**Detecção de auto-cirurgia em andamento:** reusa o mesmo padrão já usado em outros pontos do arquivo (`CheckManualInputs`, `ProcessHeal`, `DeferredDiscardRoutine`) para saber se as mãos do jogador local estão ocupadas com um item médico — `doctor.HandsController is Player.MedsController meds`. Combinado com `ItemDatabase.GetStats(meds.Item.TemplateId.ToString())?.IsSurgery == true` (mesmo critério de exclusividade a CMS/Surv12 do item 024) e `!_isHealingInProgress` (garante que não há cura de aliado em andamento — ver §7 sobre por que essa checagem由explícita é necessária mesmo sendo "raro" colidir).

**Isolamento aliado × self já existe e foi auditado nesta spec (resolve a preocupação do usuário/corner case da spec funcional):** `MedicHealPatch.cs:307-349` (`Prefix` do patch em `method_9`/DoMedEffect nativo) já tem uma guarda G5 explícita:
```csharp
if (!IsRedirectingHeal || CurrentPatient == null)
{
    // G5: Se BandAidHealActive=true, bloquear self-heal vanilla DO MÉDICO
    // para evitar que _currentObservedMedsControllerClass seja sobrescrito por outra instância
    if (BandAidHealActive)
    {
        ...
        return false;
    }
    return true;
}
```
Ou seja: **enquanto uma cura de aliado está ativa (`BandAidHealActive`), o próprio mod já bloqueia o efeito nativo de self-heal do médico** — o cenário descrito pelo usuário (médico com perna zerada curando aliado com braço zerado; CMS deve curar só o aliado) já é garantido pelo código EXISTENTE, sem qualquer mudança necessária aqui. Este item só precisa garantir que sua PRÓPRIA detecção de "auto-cirurgia" (`EmergencyDropSelf`) respeita a mesma exclusão (`!_isHealingInProgress`), não que reimplemente o isolamento em si.

**Mecânica de cancelamento instantâneo — investigada a fundo para não quebrar a punição nativa de carga.** A pergunta central desta spec técnica era: a punição de "perde 1 carga se cancelar depois de `ItemRemoveAfterInterruptionTime`" roda ANTES ou DEPOIS do ponto que o `DestroyController()` (item 024) pula? Se rodasse DEPOIS (dentro da sequência de fechar o kit), usar `DestroyController()` pra self-heal removeria essa punição de graça — o oposto do problema do item 024 (lá o risco era cobrar 2x; aqui seria cobrar 0x). Investigação confirmou que a punição roda **antes, de forma síncrona, dentro do próprio `CancelApplyingItem()`**, totalmente desacoplada do controlador de mãos:

- `ActiveHealthController.CancelApplyingItem()` (já usado pelo mod, ex. `BandAidController.cs:498`/`850`) → cadeia síncrona até `MedEffect.Residue()` (ramo "interrompido").
- `Residue()` interrompido (confirmado em [`ActiveHealthController.cs:1944-1949`](../../../../references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs#L1944-L1949)):
  ```csharp
  float num = base.WholeTime - base.DelayTime;
  float itemRemoveAfterInterruptionTime = Singleton<BackendConfigSettingsClass>.Instance.ItemsSettings.ItemRemoveAfterInterruptionTime;
  if (MedKitComponent_0 != null && num > itemRemoveAfterInterruptionTime && GClass855.IsZero(MedKitComponent_0.HpResourceRate))
  {
      MedKitComponent_0.HpResource = Mathf.Round(MedKitComponent_0.HpResource - 1f);
  }
  ```
  Isso roda **dentro** da chamada de `CancelApplyingItem()` (via `RemoveMedEffect` → `ForceResidue` → `method_0` → `Residue()`, sem `Task.Delay`, sem depender do hands controller) — totalmente independente de `Drop()`/`method_2`/`Task.Delay(600)` (`Player.cs:19967-19977`) ou de `DestroyController()` (`Player.cs:31676-31688`, que nunca toca `HealthController`). **Conclusão: chamar `CancelApplyingItem()` ANTES de `DestroyController()` preserva a punição nativa intacta — nem 2x, nem 0x.**

**Achado extra relevante — o próprio cancelamento nativo do jogo (Mouse0) também tem uma espera de 600ms, DIFERENTE da do item 024.** `Player.MedsController.Remove()` (o método que o input nativo de cancelamento chama) também é assíncrono:
```csharp
// Player.cs:19825-19834
public void Remove() { method_1().HandleExceptions(); }
public async Task method_1()
{
    await Task.Delay(600);
    this.ObservedMedsControllerClass.Remove();
}
```
E `ObservedMedsControllerClass.Remove()` (o que de fato cancela) faz exatamente os 2 passos que este item precisa replicar, SEM o delay:
```csharp
// Player.cs:19596-19600
public void Remove()
{
    Queue_0.Clear();
    MedsController_0._player.HealthController.CancelApplyingItem();
}
```
`Player.HealthController` ([`Player.cs:25289`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L25289): `public IHealthController HealthController => _healthController;`) e `Player.ActiveHealthController` ([`Player.cs:25291`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L25291): `public ActiveHealthController ActiveHealthController => _healthController as ActiveHealthController;`) são o **mesmo objeto** (`_healthController`), só expostos com tipos diferentes — confirmado por leitura direta. Logo, `doctor.ActiveHealthController?.CancelApplyingItem()` (já usado pelo mod) é exatamente equivalente à chamada nativa.

`Queue_0` é `public Queue<EBodyPart>` dentro de `ObservedMedsControllerClass` ([`Player.cs:19453`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L19453)), mas `MedsController` já expõe um wrapper público mais limpo: `public void ClearQueue() { this.ObservedMedsControllerClass.ClearQueue(); }` ([`Player.cs:19836-19839`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L19836-L19839)) — usar esse wrapper em vez de acessar `Queue_0` diretamente (menos acoplamento a um campo interno).

**`ForceFinishAnimation()` do item 024 NÃO é reusado aqui — decisão deliberada, não esquecimento.** `MedicHealPatch._currentObservedMedsControllerClass` só é populado dentro do `Prefix` de `method_5` **quando `IsRedirectingHeal && CurrentPatient != null`** (`MedicHealPatch.cs:338`: `if (!IsRedirectingHeal || CurrentPatient == null) { ...; return true; }` — o `method_5` original roda sem interceptação). Ou seja: **numa auto-cirurgia genuína (sem redirect), `_currentObservedMedsControllerClass` nunca é setado por essa operação** — chamar `ForceFinishAnimation()` aqui seria, na melhor hipótese, um no-op (variável `null`, já resetada ao fim de toda cura de aliado anterior) e, na pior, um risco desnecessário de tocar num estado que pertence a um contexto diferente. `method_9`/`ForceFinishAnimation` é uma muleta que existe *só* porque o redirect da cura de aliado suprime o sinal nativo de conclusão — a auto-cirurgia nunca tem esse problema, o jogo já sabe finalizar sozinho.

## 2. Pontos de patch

Nenhum ponto de patch novo no Assembly. Tabela dos métodos do Assembly cujo uso é novo neste item (nenhum é patcheado, são chamadas diretas):

| Alvo (Assembly) | Tipo de uso | Motivo |
|---|---|---|
| [`Player.cs:19836`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L19836) `MedsController.ClearQueue()` | Chamada nova | Espelha o `Queue_0.Clear()` que o `Remove()` nativo já faz antes de cancelar — evita item de múltiplas partes deixar entradas na fila. |
| [`Player.cs:25291`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L25291) `Player.ActiveHealthController` / [`CancelApplyingItem()`](../../../../references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs#L1944-L1949) | Call site já existente no mod (reuso) | Mesma chamada de `EmergencyDrop`/`CancelHealInProgress` — dispara a punição nativa de carga de forma síncrona, sem esperar o fechamento do kit. |
| [`Player.cs:31676`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L31676) `DestroyController()` | Call site já existente (item 024, reuso) | Descarta o controlador de mãos de forma síncrona, sem o `Task.Delay(600)` de `Drop()`. |
| [`Player.cs:31800`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L31800) `TrySetLastEquippedWeapon(bool, Callback)` | Call site já existente (item 023/024, reuso) | Mesmo caminho — instantâneo porque `HandsController` já é `null`. |

## 3. Novas propriedades F12 (BepInEx)

N/A — reusa o mesmo `EmergencyDropKey`/`EmergencyDropMode` já existente (item original).

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded-V4/Patches/Medical/BandAidController.cs` | MODIFICAR | Novo gate em `Update()` pra detectar auto-cirurgia; novo método privado `EmergencyDropSelf(Player doctor, Player.MedsController meds)`. |

Nenhum arquivo novo.

## 5. Stubs de código

**Gate em `Update()`** — adiciona um segundo contexto de ativação pra MESMA tecla, mutuamente exclusivo com o gate do item 024:

```csharp
// BandAidController.cs — dentro de Update(), substitui o bloco "=== EMERGENCY DROP ===" (~linha 174-182)
var mainPlayer = Singleton<GameWorld>.Instance.MainPlayer; // já garantido non-null neste ponto do Update()
bool selfSurgeryInProgress = !_isHealingInProgress
    && mainPlayer.HandsController is Player.MedsController selfMeds
    && (ItemDatabase.GetStats(selfMeds.Item.TemplateId.ToString())?.IsSurgery ?? false);

// === EMERGENCY DROP === (ref: item 024 — cura de aliado; item 025 — auto-cirurgia)
if ((_isHealingInProgress && _currentHealIsSurgery || selfSurgeryInProgress) && CheckPressMode(
    _emergencyDropKey.Value, _emergencyDropMode.Value,
    ref _emergencyHoldTimer, ref _emergencyHoldTriggered, ref _emergencyLastTapTime))
{
    if (_isHealingInProgress)
        EmergencyDrop();
    else
        EmergencyDropSelf(mainPlayer, (Player.MedsController)mainPlayer.HandsController);
    return;
}
```

**Novo método `EmergencyDropSelf`** — mesma ideia do `EmergencyDrop`, sem nenhuma das partes específicas de cura de aliado (sem `_itemBeingUsed`/`_isHealingInProgress`/redirect, sem `ForceFinishAnimation`, sem `ConsumeSafe`):

```csharp
// BandAidController.cs — novo método privado
/// <summary>
/// Drop emergencial durante AUTO-cirurgia (self-heal nativo, sem redirect de aliado).
/// ref: item 025 — mesma tecla/config do EmergencyDrop (item 024), contexto diferente.
/// </summary>
private void EmergencyDropSelf(Player doctor, Player.MedsController meds)
{
    Item item = meds.Item;
    string itemName = item?.ShortName?.Localized() ?? "?";

    // ref: item 025 — Assembly-CSharp/EFT/Player.cs:19596-19600 ObservedMedsControllerClass.Remove()
    //      faz exatamente Queue_0.Clear() + HealthController.CancelApplyingItem() — replicado aqui
    //      SEM o Task.Delay(600) que o MedsController.Remove() nativo (Player.cs:19825-19834) usa
    //      pra chegar lá. CancelApplyingItem() dispara a punição de carga nativa de forma síncrona
    //      (ActiveHealthController.cs:1944-1949, ItemRemoveAfterInterruptionTime) — este método
    //      NÃO reimplementa nem duplica essa lógica, só chama a mesma API que o jogo já usa.
    try
    {
        meds.ClearQueue();
        doctor.ActiveHealthController?.CancelApplyingItem();
    }
    catch (Exception ex)
    {
        TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning($"EmergencyDropSelf CancelApplyingItem: {ex.Message}");
    }

    // ref: item 024 — Player.cs:31676-31688 DestroyController() descarta HandsController de forma
    // síncrona, sem o Task.Delay(600) de Drop()/method_2 (Player.cs:19967-19977).
    try
    {
        if (doctor.HandsController != null)
            doctor.DestroyController();
    }
    catch (Exception ex)
    {
        TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning($"EmergencyDropSelf DestroyController: {ex.Message}");
    }

    // ref: item 023 — Player.cs:31800 TrySetLastEquippedWeapon(bool, Callback), instantâneo porque
    // HandsController já é null (Process<>.Execute, Player.cs:22514-22518).
    try
    {
        Callback weaponReturnCallback = delegate(IResult result)
        {
            if (result != null && !string.IsNullOrEmpty(result.Error))
                TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning($"EmergencyDropSelf: TrySetLastEquippedWeapon falhou ({result.Error}).");
        };
        doctor.TrySetLastEquippedWeapon(true, weaponReturnCallback);
    }
    catch (Exception ex)
    {
        TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning($"EmergencyDropSelf Limpar Mãos: {ex.Message}");
    }

    // ref: item 025 — SEM lógica de consumo própria (decisão do usuário): a punição de carga já
    // rodou de forma síncrona dentro de CancelApplyingItem(), acima. Só dropar se o item ainda
    // existir (mesmo guard defensivo de CR-01-01, item 024).
    if (item != null && item.CurrentAddress != null)
    {
        try
        {
            doctor.InventoryController.ThrowItem(item);
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogInfo($"EmergencyDropSelf: {itemName} dropado (auto-cirurgia).");
        }
        catch (Exception ex)
        {
            TRLImmersiveCombatMedicinePlugin.ModLogger.LogWarning($"EmergencyDropSelf ThrowItem: {ex.Message}");
        }
    }

    // ref: PA-01-01 — UsingMeds confirmado gate de movimento puro (MovementContext.cs:1193,1248,
    // 1276,1316,3306; PlayerPhysicalClass.cs:268,671), nunca lido pela máquina de estado do efeito
    // — escrever aqui é seguro. Mantido por precaução: o código nativo que normalmente desligaria
    // essa flag pode estar dentro do trecho de Drop()/fechamento que DestroyController() pula de
    // propósito (ver §1) — validar em raid (checklist §8) que o jogador não fica sem poder correr.
    doctor.MovementContext.SetPhysicalCondition(EPhysicalCondition.UsingMeds, false);
    // ref: PA-01-02 — no-op confirmado neste contexto (HealingLegs nunca é ligado fora de
    // HealRoutine, doc-comment do próprio método, BandAidController.cs:803-808); mantido só por
    // consistência com EmergencyDrop, custo desprezível.
    ReleaseSurgeryImmobilize(doctor);

    NotificationManagerClass.DisplayMessageNotification(MedicLocale.Get(MedicTextId.ItemDropped),
        ENotificationDurationType.Default, ENotificationIconType.Alert);
}
```

## 6. Fluxo de dados

```
[EmergencyDropSelf — auto-cirurgia]
[A] Update() detecta: !_isHealingInProgress (nenhuma cura de aliado ativa) + HandsController é
    MedsController + item é cirurgia (IsSurgery) + tecla acionada.
[B] meds.ClearQueue() + doctor.ActiveHealthController.CancelApplyingItem() — mesma dupla que
    ObservedMedsControllerClass.Remove() (Player.cs:19596-19600) faz, sem o Task.Delay(600) do
    MedsController.Remove() nativo (Player.cs:19825-19834). Dispara a punição de carga NATIVA
    de forma síncrona (ActiveHealthController.cs:1944-1949) — nem duplicada, nem pulada.
[C] doctor.DestroyController() (Player.cs:31676-31688, síncrono) → HandsController = null.
[D] doctor.TrySetLastEquippedWeapon(true, callback) → HandsController == null → pula o delay
    de Drop() inteiro (Process<>.Execute, Player.cs:22514-22518).
[E] ThrowItem(item), só se item.CurrentAddress != null (guard CR-01-01) — dropa o kit no chão.
```

Comparação com `EmergencyDrop` (item 024, cura de aliado): a diferença central é que ali a punição de carga é REIMPLEMENTADA manualmente (`MedicalLogic.ConsumeSafe`) porque o caminho de aliado é um redirect que contorna a lógica nativa de cura; aqui, a auto-cirurgia nunca sai do caminho nativo — `CancelApplyingItem()` já dispara a punição sozinha, então este item só cancela e acelera o descarte das mãos, sem tocar em consumo.

## 7. Riscos e dependências

- **Isolamento aliado × self** — já garantido pelo guard G5 existente em `MedicHealPatch.cs:340-347` (bloqueia self-heal vanilla do médico enquanto `BandAidHealActive`). Este item só precisa preservar essa garantia não disparando `EmergencyDropSelf` quando `_isHealingInProgress` está ativo (feito via `!_isHealingInProgress` no gate do `Update()`).
- **`ForceFinishAnimation()` deliberadamente omitido** — ver §1. Reusar essa chamada aqui seria inofensivo na prática (variável estática já `null` fora de um redirect), mas incorreto conceitualmente e um risco desnecessário caso a implementação de `MedicHealPatch` mude no futuro. Não reusar.
- **`item.CurrentAddress` pós-`CancelApplyingItem()`** — diferente do item 024 (onde o item já tinha sido jogado ANTES do consumo, causando o bug CR-01-01), aqui a ordem já é a correta desde o início (cancelar/punir ANTES de tentar largar). **Confirmado (PA-01-03)** por leitura de [`ActiveHealthController.cs:1935-1964`](../../../../references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs#L1935-L1964) (já citado em §1) que o ramo nativo de cancelamento tardio só decrementa `HpResource` (`Mathf.Round`/`Mathf.Floor`) e atualiza a UI (`RaiseRefreshEvent`/`method_33`) — nenhum discard/remove automático acontece nesse caminho, ao contrário do helper próprio do mod (`ConsumeSafe`/`DiscardItemNetworked`, exclusivo do caminho de aliado). O guard `item.CurrentAddress != null` antes do `ThrowItem` é mantido por consistência/defesa (custo zero), não porque essa situação seja esperada aqui.
- **`UsingMeds`/`ReleaseSurgeryImmobilize`** — ver comentários inline no stub de §5 (PA-01-01/PA-01-02): `UsingMeds` é confirmado gate de movimento puro, seguro de escrever, e mantido por precaução já que o clear nativo correspondente pode viver dentro do trecho de `Drop()` que este item pula deliberadamente; `ReleaseSurgeryImmobilize` é um no-op confirmado neste contexto (mantido só por consistência de código com `EmergencyDrop`).
- **Item 024** — nenhuma mudança no método `EmergencyDrop` existente; `EmergencyDropSelf` é um método novo e paralelo, sem compartilhar código (decisão deliberada: as duas variantes têm responsabilidades diferentes o bastante — cura de aliado tem bookkeeping de redirect e consumo reimplementado; auto-cirurgia não tem nenhum dos dois — pra não forçar uma abstração prematura).
- **Corner case de item quase sem carga** (spec funcional) — como a punição roda dentro de `CancelApplyingItem()` via lógica nativa do próprio jogo, e não via `MedicalLogic.ConsumeSafe`, o comportamento de "o que acontece quando a carga chega a zero" (discard automático ou não) é inteiramente decidido pelo jogo — não há necessidade de investigar/replicar esse comportamento no mod, só garantir que `ThrowItem` não seja chamado num item que o jogo já tenha invalidado (guard já incluído no stub).

## 8. Checklist de implementação

- [x] Adicionar `selfSurgeryInProgress` no gate de `Update()`, mutuamente exclusivo com `_isHealingInProgress`.
- [x] Adicionar o método privado `EmergencyDropSelf(Player doctor, Player.MedsController meds)` conforme stub de §5.
- [x] Confirmar por compilação real que `MedsController.ClearQueue()` é acessível publicamente (sem reflection) a partir de uma referência `Player.MedsController` — `dotnet build -c Release`: 0 Erros, 0 Warnings.
- [ ] Validar manualmente: iniciar auto-cirurgia com CMS/Surv12 em si mesmo, acionar a tecla de drop de emergência, confirmar que a arma atira no mesmo instante. _(pendente — validação in-game)_
- [ ] Validar manualmente: usar bandagem/tala/torniquete/medkit comum em si mesmo, acionar a tecla — confirmar que nada acontece. _(pendente)_
- [ ] Validar o cenário de isolamento (spec funcional): médico com perna zerada examina aliado com braço zerado, inicia tratamento nele — usar CMS deve curar só o aliado; encerrar a interação e usar CMS em si mesmo deve funcionar normalmente sem interferência. _(pendente)_
- [ ] Validar a punição de carga: cancelar auto-cirurgia com ≥ `ItemRemoveAfterInterruptionTime` de uso deve perder 1 carga (comportamento nativo, sem notificação especial do mod — diferente do item 024, que TEM notificação própria porque reimplementa a lógica); cancelar antes do threshold não deve perder carga. _(pendente)_
- [ ] Validar em raid Headless/coop — sem trava de mãos, sem log de colisão do lado FIKA. _(pendente)_
- [ ] Validar que o jogador não fica preso sem poder correr após o drop (checagem da PA-01-01 sobre `UsingMeds`). _(pendente)_

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start hook + stop hooks idempotentes — AP-01 | N/A | Nenhum estado novo persistente — `EmergencyDropSelf` é um método síncrono chamado por input, sem campos novos de instância. |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | Não é um patch novo; `Update()` já só roda no `MainPlayer` local (`Singleton<GameWorld>.Instance.MainPlayer`, já validado non-null no topo do método). |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; TODOS os overrides auditados — AP-03 | ✅ | Mesma auditoria do item 024 (§9, PA-01-01): `Destroy()` só é alcançável via `MedsController` neste contexto (item na mão é sempre `MedsItemClass` quando `HandsController is Player.MedsController`). |
| 4 | Mudança de estado via API canônica do EFT; side-effects mapeados — AP-04 | ✅ | `ClearQueue()`/`CancelApplyingItem()`/`DestroyController()`/`TrySetLastEquippedWeapon` são todas APIs públicas do próprio jogo, replicando exatamente a sequência que `ObservedMedsControllerClass.Remove()` já faz nativamente (§1). Side-effects mapeados em §6/§7. |
| 5 | Estado entre raids: raid1→exit→raid2 e alt-F4/morte/MIA cobertos | ✅ | Sem estado persistente novo — sequenciamento local de uma única ativação (mesmo raciocínio do item 024). |
| 6 | Semântica/defaults/faixas de cada ConfigEntry sem ambiguidade — AP-05 | N/A | Nenhum `ConfigEntry` novo (§3) — reusa o existente. |
| 7 | Re-invocação de método patcheado tem reentry-guard — AP-07 | N/A | Não é um patch. |
| 8 | Flags/caches de intercept validados contra o contexto atual — AP-08 | ✅ | `selfSurgeryInProgress` é uma variável local recalculada a cada frame do `Update()`, nunca cacheada entre frames — sem risco de ficar desatualizada. |
| 9 | Todo patch-point reconfirmado no `.cs` do dump — AP-09 | ✅ | Todas as citações desta spec (`Player.cs:19453,19596-19600,19825-19834,19836-19839,25289,25291,31676-31688,22514-22518`; `ActiveHealthController.cs:1944-1949`) foram lidas diretamente nesta sessão a partir do decompile já gerado. |
| 10 | Skill EFT usada como lever confirmada não-inerte — AP-10 | N/A | Não envolve skills de personagem. |
| 11 | Pacote FIKA próprio — AP-11 | N/A | Nenhum `INetSerializable` novo — auto-cirurgia é estritamente local, sem pacote de rede novo. |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-12 | Spec técnica criada via `/create-technical-spec`. Investigação dedicada (subagent + verificação manual) confirmou que a punição nativa de perda de carga por cancelamento tardio roda de forma síncrona DENTRO de `ActiveHealthController.CancelApplyingItem()` (`ActiveHealthController.cs:1944-1949`), totalmente desacoplada do `Drop()`/`DestroyController()` — logo, usar `DestroyController()` pra pular a animação de fechar o kit NÃO afeta essa punição (nem duplica, nem pula). Confirmado também que o cancelamento nativo do próprio jogo (`MedsController.Remove()`, `Player.cs:19825-19834`) tem seu PRÓPRIO `Task.Delay(600)` antes de chamar `ObservedMedsControllerClass.Remove()` — a sequência `ClearQueue()` + `CancelApplyingItem()` (`Player.cs:19596-19600`) é replicada aqui sem esse delay. Confirmado que `ForceFinishAnimation()` (item 024) é irrelevante pra auto-cirurgia, porque `_currentObservedMedsControllerClass` só é populado durante redirect de cura de aliado (`MedicHealPatch.cs:338`) — omitido deliberadamente. Confirmado também que o isolamento aliado×self do exemplo do usuário já existe via guard G5 em `MedicHealPatch.cs:340-347`, sem necessidade de nova lógica. |
