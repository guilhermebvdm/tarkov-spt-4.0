# 005 — Hook genérico de velocidade da animação de cura observada · Spec Técnica

**Mod:** FIKA
**Spec funcional:** [005-hook-velocidade-cura-observada-01-spec.md](005-hook-velocidade-cura-observada-01-spec.md)
**Criado:** 2026-09-08

> Fonte primária: [`mods/FIKA/modded/Fika-Plugin/Fika.Core/`](../../../modded/Fika-Plugin/Fika.Core/) (nosso fork, o que é realmente compilado/instalado). Conferido idêntico ao vendorizado em `references/fika-plugin/` para os arquivos tocados aqui (diff vazio, 2026-09-08) — citações abaixo valem para os dois.

## 1. Estratégia

`Postfix`/edição direta de fonte (não é um patch Harmony — é o PRÓPRIO fork, editado à mão) em `ObservedMedsController.cs`, nos 2 pontos que hoje calculam `FirearmsAnimator.SetUseTimeMultiplier` pra uma operação de meds REPLICADA (peer observado):

1. `ObservedMedsOperation.ObservedStart(Action callback)` — hoje não seta multiplicador nenhum (1ª parte do corpo).
2. `ObservedMedsOperation.HealthController_EffectRemovedEvent(IEffect effect)` — hoje seta `1f + Skills.SurgerySpeed.Value/100f` (partes seguintes).

Ambos passam a multiplicar o valor (implícito 1, ou o calculado nativo) por um "extra" resolvido de um novo `Func<Player, Item, float>?` estático, público, definido numa classe nova e pequena (`ObservedMedsSpeedHook`) — sem qualquer conhecimento de mod específico. `null`/exceção/valor inválido → extra = 1 (comportamento de hoje, fail-safe).

Por que uma classe NOVA e não um campo dentro de `ObservedMedsController`: aquela classe é `internal sealed`, então nenhum membro dela (mesmo `public`) fica visível fora do assembly `Fika.Core` — o hook precisa estar numa classe genuinamente `public`.

## 2. Pontos de patch

| Alvo | Tipo | Motivo |
|---|---|---|
| [`ObservedMedsController.cs:133-143`](../../../modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/HandsControllers/ObservedMedsController.cs#L133-L143) — `ObservedMedsOperation.ObservedStart` | Edição de fonte | Aplica o extra do hook na 1ª parte do corpo (hoje sem multiplicador nenhum). |
| [`ObservedMedsController.cs:154-194`](../../../modded/Fika-Plugin/Fika.Core/Main/ObservedClasses/HandsControllers/ObservedMedsController.cs#L154-L194) — `ObservedMedsOperation.HealthController_EffectRemovedEvent` | Edição de fonte | Compõe o extra do hook em cima do multiplicador nativo (skill Cirurgia) já calculado ali. |
| `ObservedMedsController.cs` (arquivo novo ao lado) | Criação | Nova classe `ObservedMedsSpeedHook` — o ponto de extensão público. |

## 3. Novas propriedades F12 (BepInEx)

Nenhuma — FIKA não expõe F12 pra isto; quem decide o VALOR é o mod consumidor (ex. CustomClasses).

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `Fika.Core/Main/ObservedClasses/HandsControllers/ObservedMedsSpeedHook.cs` | CRIAR | Classe pública com o `Func` estático + helper de invocação segura. |
| `Fika.Core/Main/ObservedClasses/HandsControllers/ObservedMedsController.cs` | MODIFICAR | 2 call-sites (`ObservedStart`, `HealthController_EffectRemovedEvent`) passam a compor o extra do hook. |

## 5. Stubs de código

```csharp
// Fika.Core/Main/ObservedClasses/HandsControllers/ObservedMedsSpeedHook.cs
using System;
using EFT;
using EFT.InventoryLogic;
using BepInEx.Logging;

namespace Fika.Core.Main.ObservedClasses.HandsControllers;

/// <summary>
///     Item 005 — ponto de extensão GENÉRICO (sem conhecimento de nenhum mod específico) pra ajustar a
///     velocidade da animação de uma operação de meds REPLICADA (o cliente de quem OBSERVA um peer usando um
///     item médico). O Fika, sozinho, só ajusta essa velocidade pela skill Cirurgia nativa
///     (<see cref="ObservedMedsController"/>); um mod que queira refletir, pros observadores, um perk de
///     classe/velocidade próprio atribui <see cref="ExtraSpeedMultiplier"/>.
///     <para>
///     Contrato: <c>null</c> (ninguém assinou) ou uma exceção/valor inválido do delegate ⇒ multiplicador extra
///     = 1 (sem efeito, comportamento idêntico a antes deste item). O Fika NUNCA lança por causa de um
///     assinante malcomportado — ver <see cref="ResolveExtra"/>.
///     </para>
///     <para>
///     <b>Limitação consciente:</b> um único delegate (last-write-wins), não um evento multicast. Se um 2º mod
///     precisar do mesmo hook simultaneamente, ele precisa compor com o valor anterior por conta própria antes
///     de reatribuir — não há suporte nativo a múltiplos assinantes independentes.
///     </para>
///     <para>
///     <b>Ciclo de vida (PA-01-01):</b> atribuir <see cref="ExtraSpeedMultiplier"/> UMA ÚNICA VEZ, no
///     <c>Awake()</c> do mod consumidor (ou assim que ele detectar que o Fika está carregado). O campo é
///     <c>static</c> e vale pra sessão inteira do processo — não há hook de raid-start/raid-end aqui, e não é
///     necessário (nem esperado) reatribuir por raid.
///     </para>
/// </summary>
public static class ObservedMedsSpeedHook
{
    /// <summary>
    ///     Recebe o <see cref="Player"/> (peer observado) executando a operação e o <see cref="Item"/> em uso;
    ///     retorna o multiplicador EXTRA a compor sobre o que o Fika já calcula (1 = sem efeito). Atribuir
    ///     <c>null</c> remove o hook. Hoje sempre chamado com argumentos não-nulos (ver <see cref="ResolveExtra"/>),
    ///     mas consumidores devem tratar como robustez, não garantia contratual permanente.
    /// </summary>
    public static Func<Player, Item, float>? ExtraSpeedMultiplier;

    private static ManualLogSource? Log => FikaPlugin.Instance?.FikaLogger; // confirmado: FikaPlugin.cs:53-61 (propriedade pública que envelopa o Logger do BepInEx)

    /// <summary>Invocação protegida — usada pelos 2 call-sites de <see cref="ObservedMedsController"/>.
    /// Nunca lança; sempre devolve um multiplicador finito e positivo.</summary>
    internal static float ResolveExtra(Player player, Item item)
    {
        // PA-01-02: guard defensivo — hoje os 2 call-sites sempre passam player/item não-nulos, mas não é
        // um contrato garantido (um 3º call-site futuro, ou mudança de ordem no Fika, poderia mudar isso).
        // Silencioso (sem log): não é uma falha, é o hook simplesmente não se aplicando a um estado sem sentido.
        if (player == null || item == null)
        {
            return 1f;
        }

        var hook = ExtraSpeedMultiplier;
        if (hook == null)
        {
            return 1f;
        }

        try
        {
            var value = hook(player, item);
            return float.IsFinite(value) && value > 0f ? value : 1f;
        }
        catch (Exception ex)
        {
            Log?.LogError($"[ObservedMedsSpeedHook] assinante lançou exceção — ignorando (extra=1): {ex}");
            return 1f;
        }
    }
}
```

> PA-01-04: ao implementar, preservar o formato de chaves ORIGINAL do arquivo (mostrado abaixo tal como está
> em `ObservedMedsController.cs` hoje) e adicionar só as linhas marcadas `// NOVO (005)` — não reescrever o
> método inteiro. Isso mantém o diff mínimo contra o arquivo real, facilitando review e futuros merges com o
> Fika upstream.

```csharp
// Fika.Core/Main/ObservedClasses/HandsControllers/ObservedMedsController.cs — trechos alterados
// (formato de chaves idêntico ao arquivo original; só as linhas "NOVO (005)" são adicionadas)

// ref: ObservedMedsController.cs:133-143 (ObservedStart) — ANTES não setava multiplicador nenhum na 1ª parte.
public void ObservedStart(Action callback)
{
    State = Player.EOperationState.Executing;
    SetLeftStanceAnimOnStartOperation();
    callback?.Invoke();
    _animation = _observedMedsController._animation;
    ObservedMedsController_OnOutUseEvent();
    _observedMedsController.FirearmsAnimator.SetAnimationVariant(_animation);
    _observedMedsController._fikaPlayer.HealthController.EffectRemovedEvent += HealthController_EffectRemovedEvent;
    _observedMedsController.OnOutUseEvent += ObservedMedsController_OnOutUseEvent;

    // NOVO (005) — hook genérico (item 090 do CustomClasses é o 1º consumidor real).
    var extraStart = ObservedMedsSpeedHook.ResolveExtra(_observedMedsController._fikaPlayer, _observedMedsController.Item);
    if (extraStart != 1f)
    {
        _observedMedsController.FirearmsAnimator?.SetUseTimeMultiplier(extraStart);
    }
}

// ref: ObservedMedsController.cs:154-194 (HealthController_EffectRemovedEvent) — só a linha do mult muda.
public void HealthController_EffectRemovedEvent(IEffect effect)
{
    // Look for GClass increments
    if (effect is not GInterface376)
    {
        return;
    }

    if (_destroyRequested)
    {
        return;
    }

    if (_observedMedsController._player.HealthController.GetBodyPartHealth(EBodyPart.Common).AtMaximum)
    {
        return;
    }

    if (_observedMedsController.FirearmsAnimator != null)
    {
        var animator = _observedMedsController.FirearmsAnimator;

        if (animator.HasNextLimb())
        {
            animator.SetActiveParam(false, false);
            animator.SetNextLimb(true);
        }

        var mult = _observedMedsController._fikaPlayer.Skills.SurgerySpeed.Value / 100f;

        // NOVO (005) — hook genérico compõe POR CIMA do cálculo nativo (skill Cirurgia). Única linha
        // alterada nesta função: era `animator.SetUseTimeMultiplier(1f + mult);`.
        var extra = ObservedMedsSpeedHook.ResolveExtra(_observedMedsController._fikaPlayer, _observedMedsController.Item);
        animator.SetUseTimeMultiplier((1f + mult) * extra);

        _animation++;
        var variant = 0;
        if (_observedMedsController.Item.TryGetItemComponent(out AnimationVariantsComponent animationVariantsComponent))
        {
            variant = animationVariantsComponent.VariantsNumber;
        }
        var newAnim = (int)Mathf.Repeat(_animation, variant);
        animator.SetAnimationVariant(newAnim);
    }
}
```

## 6. Fluxo de dados

```
[A] Peer usa item médico (self ou redirect de outro mod, ex. TRL-ImmersiveCombatMedicine)
      → [B] Fika replica a operação no cliente de QUEM OBSERVA (ObservedMedsController)
            → [C] ObservedStart / HealthController_EffectRemovedEvent calculam a velocidade NATIVA
                  (nada na 1ª parte; skill Cirurgia nas seguintes)
            → [D] ObservedMedsSpeedHook.ResolveExtra(player, item) — NOVO — chama o Func atribuído
                  por algum mod (ou devolve 1 se ninguém assinou / assinante falhou)
            → [E] SetUseTimeMultiplier(nativo * extra) — velocidade final aplicada na réplica
      → [F] Observador vê a velocidade que o mod consumidor decidiu (ou a nativa, se nenhum mod assinou)
```

## 7. Riscos e dependências

- **Acoplamento zero com mods específicos:** o hook não importa nada de `CustomClasses`/`TRL-ImmersiveCombatMedicine` — é `Func<Player, Item, float>`, tipos só de `EFT`/`EFT.InventoryLogic` (já referenciados pelo Fika). Fika continua buildável e funcional sem NENHUM mod consumidor instalado.
- **Quem consome precisa de reflection OU referência de projeto:** um mod consumidor pode (a) referenciar `Fika.Core.dll` em tempo de build e atribuir o campo diretamente (caminho normal, já que o campo é público), OU (b) resolver por reflection (`AccessTools.TypeByName` + `AccessTools.Field`) se preferir manter zero dependência de build com o Fika — útil se o mod também precisar rodar contra um Fika NÃO forkado (vanilla, sem este hook). Ver nota equivalente na spec técnica do item 090 (`CustomClasses`).
- **`last-write-wins`:** documentado como limitação consciente na spec funcional — não é objetivo deste item resolver múltiplos assinantes.
- **Fragilidade ZERO adicional em relação a hoje:** como é edição direta do NOSSO fork (não Harmony em classe privada de terceiro), não há risco de "não resolver o tipo" — o hook sempre existe nesta versão do Fika. O risco se desloca pra "manter esse hook ao dar merge de uma versão nova do Fika upstream" — igual a qualquer outro patch já existente no fork (item 001-004).

## 8. Checklist de implementação

- [x] Criar `ObservedMedsSpeedHook.cs` com o `Func` + `ResolveExtra` (try/catch + validação finita/positiva).
- [x] Editar `ObservedStart` pra chamar o hook e aplicar o extra na 1ª parte.
- [x] Editar `HealthController_EffectRemovedEvent` pra compor o extra em cima do cálculo nativo.
- [x] Bump de versão (SemVer, patch) — `FikaVersion` 2.3.15 → 2.3.16. Build de `Fika.Core.dll` (0 erros/avisos) fica para o `/compile-mod`.
- [ ] Validar in-game SEM nenhum mod consumidor: nenhuma mudança de comportamento perceptível.
- [ ] Validar in-game COM um assinante de teste (`ExtraSpeedMultiplier = (_, _) => 0.5f` temporário): observador vê a animação nitidamente mais rápida.

## 9. Conformidade com skills (auto-checklist)

| # | Check | Status | Evidência / razão |
|---|---|---|---|
| 1 | Lifecycle de raid: start/stop hooks idempotentes | N/A | Nenhum estado por raid — o campo estático é um delegate substituível, sem coleção/timer a limpar. |
| 2 | Filtro MainPlayer/Fika em todo patch que reage a ação de player — AP-02 | ✅ | O hook só é chamado dentro de `ObservedMedsOperation`, que o Fika só instancia pra players OBSERVADOS (não o `MainPlayer` local) — mesma garantia estrutural já existente no arquivo. |
| 3 | Alvos ofuscados/virtuais resolvidos por assinatura; overrides auditados — AP-03 | N/A | Não é patch Harmony em alvo virtual/ofuscado — é edição direta de fonte própria, métodos concretos não-virtuais. |
| 4 | Mudança de estado via API canônica; side-effects mapeados — AP-04 | ✅ | Continua usando `FirearmsAnimator.SetUseTimeMultiplier` (API pública já usada pelo próprio Fika ali). |
| 5 | Estado entre raids | N/A | Mesma razão do check 1. |
| 6 | Semântica/defaults de ConfigEntry sem ambiguidade — AP-05 | N/A | Sem `ConfigEntry` (§3). |
| 7 | Reentrância — AP-07 | ✅ | `ResolveExtra` não invoca de volta `ObservedStart`/`HealthController_EffectRemovedEvent`. |
| 8 | Estado stale em troca de contexto — AP-08 | ✅ | Nenhum cache de identidade de operação — o hook é chamado com os dados CORRENTES a cada invocação. |
| 9 | Patch-point reconfirmado no `.cs` real — AP-09 | ✅ | `ObservedMedsController.cs` lido por completo (127-194) tanto no fork (`mods/FIKA/modded/`) quanto na referência vendorizada — diff vazio, confirmado 2026-09-08. |
| 10 | Skill EFT inerte — AP-10 | N/A | Não introduz lever de skill nova. |
| 11 | Pacote FIKA próprio — AP-11 | N/A | Não é `INetSerializable` — é um hook IN-PROCESS (mesmo cliente), sem nada na rede. |

## Histórico

| Data | Evento |
|---|---|
| 2026-09-08 | Spec técnica criada via `/create-technical-spec`. |
| 2026-09-09 | Aplicados os 4 pontos da [review 01](005-hook-velocidade-cura-observada-03-spec-tech-review-01.md): contrato de ciclo de vida do hook documentado (PA-01-01), null-guard em `ResolveExtra` (PA-01-02), comentário `TODO confirmar` removido/confirmado (PA-01-03), stub reformatado preservando chaves originais (PA-01-04). |
