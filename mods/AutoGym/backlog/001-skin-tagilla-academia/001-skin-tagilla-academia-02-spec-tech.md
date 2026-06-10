# 001 — skin-tagilla-academia · Spec Técnica

**Mod:** AutoGym
**Spec funcional:** [001-skin-tagilla-academia-01-spec.md](001-skin-tagilla-academia-01-spec.md)
**Criado:** 2026-06-10

> Fonte primária de verdade para qualquer assinatura, fórmula ou ponto de patch: [references/eft-decompiled/Assembly-CSharp/](../../../../references/eft-decompiled/Assembly-CSharp/). Toda referência ao código do EFT deve citar `arquivo.cs:linha`. Wiki SPT e fontes externas só como complemento.
>
> **Nota sobre o decompile:** `PlayerBody.cs`, `HideoutPlayerOwner.cs`, `ShrinkingCircleQTE.cs` e `MongoID.cs` não existiam no decompile original (falha do ilspycmd em modo projeto); foram extraídos pontualmente nesta sessão com `ilspycmd -t` do mesmo `Assembly-CSharp.dll` (D:\SPT) e adicionados a `references/eft-decompiled/Assembly-CSharp/`.

## 1. Estratégia

**Reusar os dois patches Harmony já existentes no AutoGym** (`HideoutPlayerOwner.PrepareWorkout` Prefix e `HideoutPlayerOwner.StopWorkout` Finalizer — [Plugin.cs:48-67](../../modded/Plugin.cs)) e acrescentar, ao lado de `WorkoutGearVisibility`, um novo helper estático `WorkoutBodySkinSwap` que troca **apenas a parte `EBodyModelPart.Body`** do `PlayerBody` via o método público **`PlayerBody.SetSkin(KeyValuePair<EBodyModelPart, ResourceKey>, Skeleton)`** ([PlayerBody.cs:747](../../../../references/eft-decompiled/Assembly-CSharp/EFT/PlayerBody.cs#L747)).

`SetSkin` é o mecanismo que o próprio `PlayerBody.Init` usa para vestir cada parte do corpo ([PlayerBody.cs:607-616](../../../../references/eft-decompiled/Assembly-CSharp/EFT/PlayerBody.cs#L607-L616)): instancia a `LoddedSkin` do bundle, faz skin no esqueleto, parenteia no `_meshTransform` e **destrói a skin anterior da mesma parte** ([PlayerBody.cs:758-762](../../../../references/eft-decompiled/Assembly-CSharp/EFT/PlayerBody.cs#L758-L762)). Trocar e restaurar é, portanto, chamar `SetSkin` duas vezes com `ResourceKey`s diferentes.

**Memória da skin anterior:** `PlayerBody.BodyCustomization` ([PlayerBody.cs:514](../../../../references/eft-decompiled/Assembly-CSharp/EFT/PlayerBody.cs#L514)) guarda o dicionário de customização aplicado no `Init` e **não é alterado por `SetSkin`** — ele permanece como fonte de verdade do perfil. O restore lê `BodyCustomization[EBodyModelPart.Body]` e re-resolve o bundle original. Nenhuma escrita em `Profile.Customization` ([Profile.cs:634](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Profile.cs#L634)) acontece — persistência intocada por construção.

**Resolução id→bundle:** `Singleton<CustomizationSolverClass>.Instance.GetBundle(MongoID)` ([CustomizationSolverClass.cs:348-351](../../../../references/eft-decompiled/Assembly-CSharp/CustomizationSolverClass.cs#L348-L351)) retorna o `ResourceKey` do template ou `null` se o template não existir (AllTheClothes ausente → fallback no-op).

**Carregamento do bundle:** se o jogador nunca vestiu a skin, o bundle `top_boss_tagilla.skin.bundle` não está carregado. Usar o mesmo padrão do preview de roupas do trader (`GClass1041.method_0`, [GClass1041.cs:184-198](../../../../references/eft-decompiled/Assembly-CSharp/GClass1041.cs#L184-L198)): `GClass1857.Retain(IEasyAssets, paths)` ([GClass1857.cs:125](../../../../references/eft-decompiled/Assembly-CSharp/GClass1857.cs#L125)) + `await GClass1857.LoadBundles(handle)` ([GClass1857.cs:173](../../../../references/eft-decompiled/Assembly-CSharp/GClass1857.cs#L173)), com `Release()` do handle no restore ([GClass1041.cs:112-116](../../../../references/eft-decompiled/Assembly-CSharp/GClass1041.cs#L112-L116)). O fato de o preview do Fence renderizar a skin do AllTheClothes prova que esse caminho carrega bundles de mods sob demanda.

**Alternativas descartadas:**
- *Re-`Init` do `PlayerBody` com customização temporária* — `Init` ([PlayerBody.cs:575](../../../../references/eft-decompiled/Assembly-CSharp/EFT/PlayerBody.cs#L575)) só tem 2 call-sites no jogo, sempre em corpo recém-criado ([Player.cs:28629](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L28629), [GClass1041.cs:138](../../../../references/eft-decompiled/Assembly-CSharp/GClass1041.cs#L138)); re-init em corpo vivo recria todos os `SlotViews` e não é comprovadamente seguro.
- *Trocar também `EBodyModelPart.Hands`* — o suite "Tagilla's Chest" define `hands` próprias (`66a257a4a5b72803728b43c9`), mas no hideout a câmera é 3ª pessoa e a `LoddedSkin` de Hands fica com renderers desligados (`UpdatePlayerRenders`, [PlayerBody.cs:712-713](../../../../references/eft-decompiled/Assembly-CSharp/EFT/PlayerBody.cs#L712-L713)) — as mãos visíveis em 3ª pessoa pertencem ao mesh do Body. Trocar Hands acrescentaria complexidade (relógio `_watches` plugado no esqueleto das mãos, [PlayerBody.cs:699-704](../../../../references/eft-decompiled/Assembly-CSharp/EFT/PlayerBody.cs#L699-L704)) sem efeito visual.
- *Mutar `Profile.Customization` + recriar corpo* — viola o critério de persistência zero da spec funcional.

## 2. Pontos de patch

Nenhum patch novo — os dois alvos já são patcheados pelo mod; os patches existentes ganham uma chamada a mais.

| Alvo (Assembly) | Tipo | Motivo |
|---|---|---|
| [`EFT/HideoutPlayerOwner.cs:753`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/HideoutPlayerOwner.cs#L753) `PrepareWorkout(...)` | Prefix (existente) | Disparar `WorkoutBodySkinSwap.Apply(owner)` ao iniciar o treino. |
| [`EFT/HideoutPlayerOwner.cs:769`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/HideoutPlayerOwner.cs#L769) `StopWorkout()` | Finalizer (existente) | Disparar `WorkoutBodySkinSwap.Restore(owner)` ao encerrar (cobre caminho de exceção). |

## 3. Novas propriedades F12 (BepInEx)

| Seção | Nome (EN) | Tipo | Padrão | Faixa | Avançado | Tooltip (pt-BR) |
|---|---|---|---|---|---|---|
| `Visuals` | `Swap Workout Body Skin` | bool | `true` | — | — | Troca temporariamente o torso do personagem para a skin de treino configurada durante exercícios na academia do esconderijo. Restaurado ao encerrar o treino. |
| `Visuals` | `Workout Body Skin Id` | string | `66a25a3af12f29d8a2599527` | — | — | Id do template de customização (Body) usado durante o treino. Padrão: "Tagilla's Chest" do mod AllTheClothes. Se o template não existir, nenhuma troca ocorre. |

> O default `66a25a3af12f29d8a2599527` é o **body template** do suite "Tagilla's Chest" (suite id `66a258e3c6b9ee37e81abcd2`), conforme `D:\SPT\SPT\user\mods\AllTheClothes\config\config.jsonc` (entrada `top_boss_tagilla_nohead`, chaves `body`/`id`).

## 4. Arquivos do mod

| Arquivo | Ação | Resumo |
|---|---|---|
| `modded/Plugin.cs` | MODIFICAR | Bind das 2 novas `ConfigEntry`; chamadas a `WorkoutBodySkinSwap.Apply/Restore` nos patches `PrepareWorkout`/`StopWorkout` existentes. |
| `modded/WorkoutBodySkinSwap.cs` | CRIAR | Helper estático: resolve bundle, carrega async, `SetSkin` Tagilla, restore + `Release` do handle de bundle. |
| `PROPRIEDADES.md` | MODIFICAR | Documentar as 2 novas propriedades na seção `Visuals`. |

## 5. Stubs de código

```csharp
// modded/WorkoutBodySkinSwap.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Comfort.Common;
using EFT;

namespace AutoGym;

internal static class WorkoutBodySkinSwap
{
    private static PlayerBody? _swappedBody;
    private static global::DependencyGraphClass<IEasyBundle>.GClass1661? _retainedBundles;
    private static int _generation;

    internal static void Apply(HideoutPlayerOwner owner)
    {
        // ref: PA-01-01 — sanear estado órfão (corpo destruído sem StopWorkout parear)
        if (_swappedBody is not null && !_swappedBody)
        {
            _swappedBody = null;
            _retainedBundles?.Release();
            _retainedBundles = null;
        }
        if (Plugin.SwapWorkoutBodySkin?.Value != true)
        {
            return;
        }
        // ref: Assembly-CSharp/EFT/HideoutPlayerOwner.cs:120 (HideoutPlayer property)
        PlayerBody? playerBody = owner?.HideoutPlayer?.PlayerBody;
        if (playerBody == null || _swappedBody != null)
        {
            return; // início duplo sem Stop: mantém o primeiro estado (idempotente)
        }
        _ = ApplyAsync(playerBody, ++_generation);
    }

    private static async Task ApplyAsync(PlayerBody playerBody, int generation)
    {
        try
        {
            // ref: Assembly-CSharp/EFT/MongoID.cs:59 (ctor de string)
            MongoID skinId = new MongoID(Plugin.WorkoutBodySkinId.Value);
            // ref: Assembly-CSharp/EFT/PlayerBody.cs:514 (BodyCustomization preserva o perfil)
            MongoID originalId = playerBody.BodyCustomization[EBodyModelPart.Body];
            if (skinId == originalId)
            {
                return; // jogador já usa a skin alvo
            }
            // ref: Assembly-CSharp/CustomizationSolverClass.cs:348
            ResourceKey? bundle = Singleton<CustomizationSolverClass>.Instance?.GetBundle(skinId);
            if (bundle == null)
            {
                // ref: PA-01-05 — distinguir suite id de template ausente
                // ref: Assembly-CSharp/CustomizationSolverClass.cs:391 (GetSuite)
                if (Singleton<CustomizationSolverClass>.Instance?.GetSuite(skinId) != null)
                {
                    Plugin.Log?.LogWarning($"AutoGym: {skinId} is a SUITE id; configure the body template id instead (see AllTheClothes config 'body' field).");
                }
                else
                {
                    Plugin.Log?.LogWarning($"AutoGym: workout body skin {skinId} not found (AllTheClothes missing?), skipping swap.");
                }
                return;
            }
            // ref: Assembly-CSharp/GClass1857.cs:125 + :173 (mesmo padrão de GClass1041.cs:195-196)
            var handle = GClass1857.Retain(Singleton<IEasyAssets>.Instance, new[] { bundle.path });
            await GClass1857.LoadBundles(handle);
            if (generation != _generation || playerBody == null)
            {
                handle.Release(); // treino acabou durante o load — não aplicar
                return;
            }
            _retainedBundles = handle;
            // ref: Assembly-CSharp/EFT/PlayerBody.cs:747 (SetSkin destrói a skin anterior da parte)
            playerBody.SetSkin(
                new KeyValuePair<EBodyModelPart, ResourceKey>(EBodyModelPart.Body, bundle),
                playerBody.SkeletonRootJoint); // ref: Assembly-CSharp/EFT/PlayerBody.cs:510
            _swappedBody = playerBody;
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogWarning($"AutoGym failed to swap workout body skin: {ex}");
        }
    }

    internal static void Restore(HideoutPlayerOwner owner)
    {
        _generation++; // invalida qualquer ApplyAsync em voo
        PlayerBody? playerBody = _swappedBody;
        _swappedBody = null;
        try
        {
            if (playerBody != null)
            {
                MongoID originalId = playerBody.BodyCustomization[EBodyModelPart.Body];
                ResourceKey? bundle = Singleton<CustomizationSolverClass>.Instance?.GetBundle(originalId);
                if (bundle != null)
                {
                    playerBody.SetSkin(
                        new KeyValuePair<EBodyModelPart, ResourceKey>(EBodyModelPart.Body, bundle),
                        playerBody.SkeletonRootJoint);
                }
            }
        }
        catch (Exception ex)
        {
            Plugin.Log?.LogWarning($"AutoGym failed to restore body skin: {ex}");
        }
        finally
        {
            _retainedBundles?.Release(); // ref: Assembly-CSharp/GClass1041.cs:112-116
            _retainedBundles = null;
        }
    }
}
```

```csharp
// modded/Plugin.cs — trechos a MODIFICAR (mod já existente)

// Awake(): adicionar binds após os existentes
internal static ConfigEntry<bool> SwapWorkoutBodySkin = null!;
internal static ConfigEntry<string> WorkoutBodySkinId = null!;
// ...
SwapWorkoutBodySkin = Config.Bind("Visuals", "Swap Workout Body Skin", true,
    "Temporarily swaps the character's torso to the configured workout skin during hideout gym workouts.");
WorkoutBodySkinId = Config.Bind("Visuals", "Workout Body Skin Id", "66a25a3af12f29d8a2599527",
    "Customization template id (Body part) applied during workouts. Default: Tagilla's Chest from AllTheClothes.");

// HideoutPlayerOwnerPrepareWorkoutPatch.Prefix — adicionar após o Hide existente:
//   WorkoutBodySkinSwap.Apply(__instance);
// HideoutPlayerOwnerStopWorkoutPatch.Finalizer — adicionar após o Restore existente:
//   WorkoutBodySkinSwap.Restore(__instance);
```

## 6. Fluxo de dados

```
[A] Jogador inicia treino na academia
 → [B] EFT chama HideoutPlayerOwner.PrepareWorkout            // ref: EFT/HideoutPlayerOwner.cs:753
 → [C] Prefix existente do AutoGym (Plugin.cs:51)
     → WorkoutGearVisibility.Hide (existente, intocado)
     → WorkoutBodySkinSwap.Apply
         → lê id da config → MongoID                          // ref: EFT/MongoID.cs:59
         → originalId = playerBody.BodyCustomization[Body]    // ref: EFT/PlayerBody.cs:514
         → bundle = CustomizationSolver.GetBundle(skinId)     // ref: CustomizationSolverClass.cs:348
         → Retain + await LoadBundles                         // ref: GClass1857.cs:125,173
         → playerBody.SetSkin(Body, bundle, SkeletonRootJoint)// ref: EFT/PlayerBody.cs:747
             → instancia LoddedSkin nova, destrói a anterior  // ref: EFT/PlayerBody.cs:749-762

[D] Jogador encerra treino (ou exceção no fluxo)
 → [E] EFT chama HideoutPlayerOwner.StopWorkout               // ref: EFT/HideoutPlayerOwner.cs:769
 → [F] Finalizer existente do AutoGym (Plugin.cs:63)
     → WorkoutGearVisibility.Restore (existente, intocado)
     → WorkoutBodySkinSwap.Restore
         → SetSkin(Body, GetBundle(BodyCustomization[Body]))  // restaura skin do perfil
         → Release do handle de bundles                       // ref: GClass1041.cs:112-116
```

`Profile.Customization` nunca é escrito; `PlayerBody.BodyCustomization` e `BodyCustomizationId` também não são tocados pelo mod — quando o corpo for recriado em qualquer troca de cena, o jogo reaplica o perfil original por conta própria (rede de segurança extra para o corner case de fechamento abrupto).

## 7. Riscos e dependências

- **Patches existentes em `modded/`:** `HideoutPlayerOwnerPrepareWorkoutPatch` e `HideoutPlayerOwnerStopWorkoutPatch` são modificados (uma chamada a mais cada). `WorkoutGearVisibility` opera sobre `SlotViews` (equipamento); `WorkoutBodySkinSwap` opera sobre `BodySkins` — conjuntos disjuntos, sem interferência.
- **`_bodyRenderers` obsoleto:** `SetSkin` não atualiza `PlayerBody._bodyRenderers` ([PlayerBody.cs:617-624](../../../../references/eft-decompiled/Assembly-CSharp/EFT/PlayerBody.cs#L617-L624)), que mantém referências aos renderers da skin destruída (usado para decals de sangue). No hideout não há dano/decals, e o corpo é recriado ao trocar de cena — risco aceito e documentado.
- **Temperatura corporal:** a skin nova não recebe `SetTemperatureForBody` ([PlayerBody.cs:877](../../../../references/eft-decompiled/Assembly-CSharp/EFT/PlayerBody.cs#L877)) até o próximo update do jogo — efeito cosmético desprezível no hideout.
- **Par Prepare/Stop não garantido (PA-01-01):** não há evidência de que `StopWorkout` rode no teardown do hideout; o `Apply` saneia estado órfão (corpo Unity-destruído) antes de qualquer early-return, liberando o handle de bundle retido.
- **`CustomizationClipping` acumula (PA-01-03):** `SetSkin` só adiciona flags ([PlayerBody.cs:754-757](../../../../references/eft-decompiled/Assembly-CSharp/EFT/PlayerBody.cs#L754-L757)); flags da skin de treino persistem até o corpo ser recriado. Risco cosmético, zero para a skin padrão (Tagilla, sem `ClippingRuleChanger` esperado).
- **`HasIntergratedArmor` obsoleto durante o swap (PA-01-04):** `SetSkin` não recalcula ([PlayerBody.cs:606](../../../../references/eft-decompiled/Assembly-CSharp/EFT/PlayerBody.cs#L606)); com `Hide Workout Gear` desligado e colete equipado pode haver clipping cosmético durante o treino. Risco aceito; corrigir via setter público ([PlayerBody.cs:571](../../../../references/eft-decompiled/Assembly-CSharp/EFT/PlayerBody.cs#L571)) só se trivial no `/code-mod`.
- **Dependência externa:** mod AllTheClothes (server) fornece template + bundle. Ausente → `GetBundle` retorna `null` → no-op com `LogWarning` (critério da spec funcional).
- **FIKA:** `SetSkin` é local (nenhum pacote de rede); troca não replica para outros clientes — comportamento desejado pela spec.
- **Async/threading:** continuations de `await` retornam ao contexto Unity (mesmo padrão do `CompleteInSuccessWindow` já existente no mod, [Plugin.cs:235](../../modded/Plugin.cs)); `SetSkin` roda no main thread. Token de geração (`_generation`) invalida load em voo se o treino acabar antes (race coberta).
- **Ordem de inicialização:** `Singleton<CustomizationSolverClass>` e `Singleton<IEasyAssets>` já existem quando o hideout carrega (usados pelo próprio `PlayerBody.Init` do hideout player) — sem risco.

## 8. Checklist de implementação

- [x] Criar `modded/WorkoutBodySkinSwap.cs` conforme stub (§5).
- [x] Adicionar `ConfigEntry` `SwapWorkoutBodySkin` e `WorkoutBodySkinId` em `modded/Plugin.cs` `Awake()`.
- [x] Adicionar `WorkoutBodySkinSwap.Apply(__instance)` no Prefix de `PrepareWorkout` (após `Hide`).
- [x] Adicionar `WorkoutBodySkinSwap.Restore(__instance)` no Finalizer de `StopWorkout` (após `Restore` do gear).
- [x] Atualizar `PROPRIEDADES.md` (seção `Visuals`, 2 novas linhas).
- [x] Compilar (`/compile-mod`) sem warnings novos.
- [ ] Validar in-game: treino troca a skin; encerrar restaura; toggle off não troca; id inválido só loga warning.

## Histórico

| Data | Evento |
|---|---|
| 2026-06-10 | Spec técnica criada via `/create-technical-spec` |
| 2026-06-10 | Review 01 aplicada: PA-01-01/02 (saneamento de estado órfão no `Apply`), PA-01-03/04 (riscos documentados), PA-01-05 (log distingue suite id) |
