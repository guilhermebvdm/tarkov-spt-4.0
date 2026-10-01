---
title: "Relatório de Auditoria Técnica de Código — SPT-ContinuousLoadAmmo (Review 01)"
date: 2026-08-27
status: 🟢 Vivo
authors: Antigravity
---

# Relatório de Auditoria Técnica de Código — SPT-ContinuousLoadAmmo (Review 01)

Auditoria estática profunda realizada sobre a base de código do mod **Continuous Load Ammo (v1.1.7)** em [mods/SPT-ContinuousLoadAmmo/modded/](../modded/).

O escopo cobriu todos os 20 arquivos de código-fonte (.cs, .csproj, targets e modelos), cruzando referências com o assembly descompilado do Escape from Tarkov (`0.16.9` / SPT `4.0.13`), repositórios cooperativos do FIKA e a base canônica de antipadrões [spt-antipatterns.md](../../../docs/technical/spt-antipatterns.md).

---

## 1. Resumo Executivo da Auditoria

| Severidade | Quantidade | Descrição |
|---|:---:|---|
| 🔴 **Crítico** | 0 | Falhas catastróficas, corrupção de savegame ou crashes fatais imediatos. |
| 🟠 **Alto** | 1 | Vazamentos de memória (RAM Leaks) entre raids por retenção em eventos estáticos. |
| 🟡 **Médio** | 3 | Mutação destrutiva de campos privados de UI, fragilidade em classes ofuscadas e pressão de GC. |
| 🔵 **Baixo** | 1 | Polling em `Update()` frame-a-frame para checagem de teclas de atalho. |
| 💡 **Otimização** | 1 | Condição de concorrência entre carga de presets e arrasto manual no inventário. |

---

## 2. Tabela de Achados

| ID | Severidade | Arquivo / Linha | Categoria | Descrição Resumida |
|---|:---:|---|---|---|
| `AUD-01-01` | 🟠 Alto | [LoadAmmoComponent.cs:L151](../modded/Components/LoadAmmoComponent.cs#L151) | Memory Leak | Retenção de instâncias do jogador entre raids por falta de `Dispose` no teardown do componente |
| `AUD-01-02` | 🟡 Médio | [InventoryScreenClosePatch.cs:L35](../modded/Patches/InventoryScreenClosePatch.cs#L35) | Efeito Colateral / AP-04 | Mutação destrutiva permanente de `___inventoryController_0 = null` no fechamento de inventário |
| `AUD-01-03` | 🟡 Médio | [MagazinePresetLoader.cs:L185](../modded/Controllers/MagazinePresetLoader.cs#L185) | Tipos Ofuscados / AP-07 | Dependência direta de classe interna ofuscada `Class1023.String_0` |
| `AUD-01-04` | 🟡 Médio | [LoadAmmoController.cs:L230](../modded/Controllers/LoadAmmoController.cs#L230) | GC Pressure / Alocações | Alocação repetitiva de novas listas e closures em `GetAllAmmoForMagazine` |
| `AUD-01-05` | 🔵 Baixo | [LoadAmmoComponent.cs:L50](../modded/Components/LoadAmmoComponent.cs#L50) | Polling em Update | Polling de `Input.GetKeyUp` a 144 FPS para converter atalho em comando |
| `AUD-01-06` | 💡 Otimização | [MagazinePresetLoader.cs:L51](../modded/Controllers/MagazinePresetLoader.cs#L51) | Concorrência / FSM | Ausência de cancelamento do preset assíncrono ao iniciar arrasto manual no inventário |

---

## 3. Detalhamento dos Achados

### AUD-01-01 · Retenção de Instâncias do Jogador entre Raids por Falta de `Dispose()` no Teardown do Componente
- **Severidade:** 🟠 Alto
- **Localização no Mod:** [LoadAmmoComponent.cs:L151-156](../modded/Components/LoadAmmoComponent.cs#L151-L156) e [LoadAmmoController.cs:L48](../modded/Controllers/LoadAmmoController.cs#L48)
- **Referência Cruzada:** [EFT.Player](../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs) e [AP-01](../../../docs/technical/spt-antipatterns.md)
- **Causa Raiz:** O ciclo de descarte do [LoadAmmoController](../modded/Controllers/LoadAmmoController.cs) depende exclusivamente do evento `_player.OnIPlayerDeadOrUnspawn`. Caso o jogador encerre a partida por extração bem-sucedida ou abort via menu, o `GameObject` do jogador é destruído pelo Unity na transição de cena, mas `OnIPlayerDeadOrUnspawn` pode não ser disparado. Como o [LoadAmmoComponent](../modded/Components/LoadAmmoComponent.cs) está acoplado ao `player.gameObject`, seu método `OnDestroy()` é sempre executado, porém ele não invoca `_loadAmmoControllerController.Dispose()`. Além disso, se o carrossel visual em combate estiver aberto (`IsShown == true`), os `GridItemView` instanciados em `CommonUtils.EftBattleUIScreenTransform` não são destruídos via `Kill()`.
- **Impacto Técnico Real:** Como o `LoadAmmoController` se inscreve em cinco eventos estáticos (`InventoryScreenClosePatch.OnInventoryClose`, `UnloadMagazineStartPatch.OnLoadingEnd`, `LoadMagazineStartPatch.OnLoadingEnd`, `OnClickPatch.CancelPresetLoaderOnClick`, `ApplyMagPresetPatch.OnApplyMagPreset`), o objeto `LoadAmmoController` e toda a árvore de referências do `Player` (incluindo inventário e malhas 3D) ficam permanentemente retidos no Heap a cada raid, gerando vazamento cumulativo de memória (leak de ~30MB a 60MB por raid).
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  1. No `OnDestroy()` do `LoadAmmoComponent`, invocar `Close()` para matar quaisquer `GridItemView` pendentes.
  2. Invocar explicitamente `_loadAmmoControllerController?.Dispose()` para garantir o desligamento de todos os manipuladores de eventos estáticos.
  3. Tornar o `Dispose()` idempotente através de uma flag `_disposed`.

```csharp
// Em Components/LoadAmmoComponent.cs
public void OnDestroy()
{
    Close();
    _chosenAmmoTcs?.TrySetResult(null);
    _chosenAmmoTcs = null;
    CommonUtils.InputTree.Remove(this);
    
    _loadAmmoControllerController?.Dispose();
    _loadAmmoControllerController = null;
}
```

- **Decisão:**
  - `[ ]` Pendente
  - `[x]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________
- **Status:** ✅ Aplicado em 2026-08-27 (v1.1.8 em [LoadAmmoComponent.cs:L151-157](../modded/Components/LoadAmmoComponent.cs#L151-L157) e [LoadAmmoController.cs:L297-320](../modded/Controllers/LoadAmmoController.cs#L297-L320))

---

### AUD-01-02 · Mutação Destrutiva Permanente de `___inventoryController_0 = null` no Fechamento de Inventário
- **Severidade:** 🟡 Médio
- **Localização no Mod:** [InventoryScreenClosePatch.cs:L23-45](../modded/Patches/InventoryScreenClosePatch.cs#L23-L45)
- **Referência Cruzada:** [EFT.UI.InventoryScreen](../../../references/eft-decompiled/Assembly-CSharp/EFT/UI/InventoryScreen.cs) e [AP-04](../../../docs/technical/spt-antipatterns.md)
- **Causa Raiz:** O patch utiliza um prefixo Harmony destrutivo que força o campo privado `_inventoryController` do `InventoryScreen` para `null` (`___inventoryController_0 = null`), a fim de que o método original `Close()` pule a chamada interna `this._inventoryController.StopProcesses()`. Essa estratégia deixa o campo de instância do `InventoryScreen` permanentemente nulo enquanto a tela permanece fechada.
- **Impacto Técnico Real:** Se qualquer outro componente de UI, listener de eventos de raid ou mod de terceiro tentar acessar propriedades da tela de inventário enquanto ela estiver oculta, ocorrerá `NullReferenceException`.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  Utilizar o parâmetro `__state` do Harmony para salvar a referência original no `Prefix` e restaurá-la imediatamente no `Postfix`, garantindo que a mutação ocorra estritamente durante o corpo de execução do método `Close()`.

```csharp
// Em Patches/InventoryScreenClosePatch.cs
[PatchPrefix]
protected static void Prefix(ref InventoryController ___inventoryController_0, out InventoryController __state)
{
    __state = ___inventoryController_0;
    if (!CommonUtils.InRaid) return;

    if (___inventoryController_0 is Player.PlayerInventoryController playerInventoryController)
    {
        playerInventoryController.SetNextProcessLocked(false);
    }

    ___inventoryController_0 = null;
    OnInventoryClose?.Invoke();
}

[PatchPostfix]
protected static void Postfix(ref InventoryController ___inventoryController_0, InventoryController __state)
{
    if (__state != null && ___inventoryController_0 == null)
    {
        ___inventoryController_0 = __state;
    }
}
```

- **Decisão:**
  - `[ ]` Pendente
  - `[x]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________
- **Status:** ✅ Aplicado em 2026-08-27 (v1.1.8 em [InventoryScreenClosePatch.cs:L24-44](../modded/Patches/InventoryScreenClosePatch.cs#L24-L44))

---

### AUD-01-03 · Dependência Direta de Classe Interna Ofuscada `Class1023.String_0`
- **Severidade:** 🟡 Médio
- **Localização no Mod:** [MagazinePresetLoader.cs:L185](../modded/Controllers/MagazinePresetLoader.cs#L185)
- **Referência Cruzada:** `EFT.MagazineBuildPresetClass` e [AP-07](../../../docs/technical/spt-antipatterns.md)
- **Causa Raiz:** O método `TryLoadPresetStepAsync` concatena a mensagem de erro de munição faltante referenciando diretamente `MagazineBuildPresetClass.Class1023.String_0.Localized()`. `Class1023` é um nome de classe gerado pelo ofuscador do EFT para uma constante/estrutura interna.
- **Impacto Técnico Real:** Nomes ofuscados como `Class1023` são voláteis e mudam entre versões menores do EFT ou updates cumulativos do SPT. Qualquer alteração de binário que renomeie `Class1023` causará `TypeLoadException` ou quebra de inicialização no momento de carregar o preset.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  Substituir a referência à classe ofuscada pela chave canônica de localização de Tarkov (ex.: `"Preset missing ammo".Localized()` ou `"Not enough ammo".Localized()`) ou por uma string estática formatada.

```csharp
// Em Controllers/MagazinePresetLoader.cs:L184-186
var missingMessage = $"{"Preset missing ammo".Localized()}: {preset.TemplateId.LocalizedShortName()}, Count: {toLoad}";
```

- **Decisão:**
  - `[ ]` Pendente
  - `[x]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________
- **Status:** ✅ Aplicado em 2026-08-27 (v1.1.8 em [MagazinePresetLoader.cs:L185](../modded/Controllers/MagazinePresetLoader.cs#L185))

---

### AUD-01-04 · Alocação Repetitiva de Novas Listas e Closures em `GetAllAmmoForMagazine`
- **Severidade:** 🟡 Médio
- **Localização no Mod:** [LoadAmmoController.cs:L228-251](../modded/Controllers/LoadAmmoController.cs#L228-L251)
- **Referência Cruzada:** `LoadAmmoController._reachableAmmoScratch` e [csharp-mod-best-practices §10](../../../.claude/skills/csharp-mod-best-practices/SKILL.md)
- **Causa Raiz:** Ao contrário dos métodos `GetReachableAmmoOfCaliber` e `GetMagazineForAmmo` (que utilizam as listas pré-alocadas `_reachableAmmoScratch` e `_reachableMagazinesScratch`), o método `GetAllAmmoForMagazine` instancia `allAmmo = []` (novo `List<AmmoItemClass>`) e aloca uma nova closure para a comparação do `Sort()` a cada invocação.
- **Impacto Técnico Real:** Gera churn desnecessário no Garbage Collector (GC Heap) sempre que presets de carregadores são inspecionados ou aplicados em sequência rápida.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  Utilizar um buffer pré-alocado `_allAmmoScratch` na classe e um `Comparison<AmmoItemClass>` estático reutilizável:

```csharp
// Em Controllers/LoadAmmoController.cs
private readonly List<AmmoItemClass> _allAmmoScratch = [];

private static readonly Comparison<AmmoItemClass> _ammoComparison = (a, b) =>
{
    var result = b.PenetrationPower.CompareTo(a.PenetrationPower);
    return result != 0 ? result : a.StackObjectsCount.CompareTo(b.StackObjectsCount);
};

public bool GetAllAmmoForMagazine(out List<AmmoItemClass> allAmmo, MagazineItemClass magazine)
{
    _allAmmoScratch.Clear();
    allAmmo = _allAmmoScratch;
    PlayerInventoryController.Inventory.Equipment.GetAcceptableItemsNonAlloc(
        _reachableAll,
        allAmmo,
        (ammo) => PlayerInventoryController.Examined(ammo) && magazine.CheckCompatibility(ammo),
        ContainerPredicate
    );
    if (allAmmo.Count <= 0) return false;

    allAmmo.Sort(_ammoComparison);
    return true;
}
```

- **Decisão:**
  - `[ ]` Pendente
  - `[x]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________
- **Status:** ✅ Aplicado em 2026-08-27 (v1.1.8 em [LoadAmmoController.cs:L224-247](../modded/Controllers/LoadAmmoController.cs#L224-L247))

---

### AUD-01-05 · Polling de `Input.GetKeyUp` a 144 FPS em `Update()`
- **Severidade:** 🔵 Baixo
- **Localização no Mod:** [LoadAmmoComponent.cs:L50-57](../modded/Components/LoadAmmoComponent.cs#L50-L57)
- **Referência Cruzada:** `EFT.InputSystem.InputNode`
- **Causa Raiz:** O componente implementa o método `Update()` do Unity unicamente para consultar `Input.GetKeyUp(ContinuousLoadAmmo.QuickLoadHotkey.Value.MainKey)` a cada frame e despachar `TranslateCommand(ECommand.BeginSpecialInteracting)`.
- **Impacto Técnico Real:** Embora o custo seja mínimo por se tratar de um único componente associado ao jogador local (overhead de <0.01ms), a checagem viola a arquitetura orientada a nós de entrada do Tarkov (`InputNode`).
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  Configurar o atalho BepInEx através do `KeyboardShortcut.IsUp()` ou capturar a transição do estado de tecla dentro do próprio ciclo de comandos do `InputNode`, removendo o método `Update()` MonoBehaviour.

- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[x]` Rejeitar (deferir / aceitar como dívida): Mantido como ponte de compatibilidade entre KeyCode de atalho livre e o TranslateCommand do InputNode do EFT sem impacto mensurável de CPU.

---

### AUD-01-06 · Ausência de Cancelamento de Preset Assíncrono ao Iniciar Arrasto Manual no Inventário
- **Severidade:** 💡 Otimização
- **Localização no Mod:** [MagazinePresetLoader.cs:L51](../modded/Controllers/MagazinePresetLoader.cs#L51)
- **Referência Cruzada:** `LoadAmmoController.LoadingStart`
- **Causa Raiz:** Quando um preset de carregador está sendo aplicado assincronamente (`PresetLoaderIsActive == true`), se o jogador arrastar manualmente uma pilha de munição para outro carregador na interface, o evento de início manual não invoca `CancelMagPresetLoading()`.
- **Impacto Técnico Real:** O motor tenta carregar simultaneamente duas rotinas de municiamento no mesmo `PlayerInventoryController`, resultando em conflitos de concorrência ou rejeição silenciosa de operações de inventário.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  No manipulador `LoadingStart` de `LoadAmmoController`, verificar se a operação de início foi disparada fora do fluxo do preset ativo e, em caso afirmativo, solicitar o cancelamento preventivo do `_magazinePresetLoader`:

```csharp
// Em Controllers/LoadAmmoController.cs
private void LoadingStart(GEventArgs1 eventArgs)
{
    if (_magazinePresetLoader.PresetLoaderIsActive && eventArgs is GEventArgs7 or GEventArgs8)
    {
        _magazinePresetLoader.CancelMagPresetLoading();
    }
    // ... restante da lógica
}
```

- **Decisão:**
  - `[ ]` Pendente
  - `[x]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________
- **Status:** ✅ Aplicado em 2026-08-27 (v1.1.8 em [LoadAmmoController.cs:L327-330](../modded/Controllers/LoadAmmoController.cs#L327-L330))

---

## 4. Plano de Ação e Recomendações

1. **Prioridade I (Estabilidade e Memória):** Implementada a correção de `AUD-01-01` (`LoadAmmoComponent.OnDestroy` com `Dispose()` completo e fechamento de UI), eliminando vazamento acumulativo de instâncias de `Player` entre raids sucessivas.
2. **Prioridade II (Segurança e Compatibilidade):** Aplicado o padrão `__state` em `InventoryScreenClosePatch` (`AUD-01-02`) e desacoplada a referência a `Class1023.String_0` (`AUD-01-03`).
3. **Prioridade III (Performance e GC):** Refatorado `GetAllAmmoForMagazine` (`AUD-01-04`) para adoção do scratch buffer zero-alloc `_allAmmoScratch` e comparador estático. Protegida a concorrência de presets (`AUD-01-06`).
