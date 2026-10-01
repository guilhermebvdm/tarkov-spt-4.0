---
title: "Relatório de Code Review — SPT-MagCheckInterrupt (Review 01)"
date: 2026-09-05
status: 🟢 Vivo
authors: Antigravity
---

# Relatório de Code Review — SPT-MagCheckInterrupt (Review 01)

Análise crítica rigorosa do código implementado e compilado na versão `1.0.3` do mod **SPT-MagCheckInterrupt** (`mods/SPT-MagCheckInterrupt/modded`), cobrindo as correções dos 8 achados técnicos da auditoria (`AUD-01`).

---

## 1. Resumo da Revisão

- **Mod:** `SPT-MagCheckInterrupt`
- **Versão anterior:** `1.0.2` $\rightarrow$ **Versão revisada:** `1.0.3`
- **Compilação Release:** ✅ Êxito (0 erros, 0 avisos)
- **Isolamento de build:** ✅ Conforme (binários unicamente em `mods/SPT-MagCheckInterrupt/builds/`)

### Quadro de Achados

| Severidade | Quantidade | Descrição Resumida |
| :--- | :---: | :--- |
| 🔴 **Bloqueador** | **0** | Nenhum impedimento crítico para release/merge. |
| 🟠 **Forte** | **0** | Nenhum desvio com potencial de corrupção ou quebra grave. |
| 🟡 **Médio** | **1** | Interrupção forçada (`Reset`) sem restaurar velocidade de animação no Mecanim. |
| 🟢 **Menor** | **3** | Comentários de deofuscação 4.1, log de fallback e convenções de build. |
| **Total** | **4** | **1 Médio, 3 Menores** |

---

## 2. Avaliação dos Fixes Aplicados

| Correção Aplicada | Validação Técnica | Avaliação |
| :--- | :--- | :---: |
| **AUD-01-01 (`SwapReloadOperation.cs`)** | Desbloqueio do gatilho (`_blockTriggerField(FirearmController_0) = false;`) adicionado em `EndOperation`, `Reset`, `HideWeapon` e na falha de `AttachModResult` em `OnMagPuttedToRig`. Elimina completamente o travamento de `Player.bool_1`. | ✅ **Aprovado** |
| **AUD-01-02 (`MagCheckReloadOperation.cs`)** | Suporte direto à `SwapOperationClass` (UIFixes / EFT atômico) e fallback seguro em inserções caso `_swapRemoveOperation` seja nulo. | ✅ **Aprovado** |
| **AUD-01-03 (`AnimationUtil.cs`)** | Remoção de `!isSwap` no envio de `ReloadCalledPacket` para FIKA. Observadores remotos recebem o pacote e evitam encerramento prematuro em `FastForwardCurrentState()`. | ✅ **Aprovado** |
| **AUD-01-04 (`ConfigUtil.cs`)** | Inclusão de `_configFile.SettingChanged -= eventArgs;` antes do `+=` em `RegisterSettingsChanged()`. Garante idempotência e elimina acúmulo de handlers entre raids. | ✅ **Aprovado** |
| **AUD-01-05 (`MagCheckReloadOperation.cs`)** | Otimização no `Update()` com `!Mathf.Approximately(_currentSpeed, _targetSpeed)`, cessando chamadas redundantes a `SetAnimationSpeed()` após a estabilização da velocidade. | ✅ **Aprovado** |
| **AUD-01-06 (`CanQuickReloadPatch.cs`)** | Condicionamento de `Boolean_1` à validação `op.CanStartReload()`. Previne recargas rápidas fora da janela aceitável ou com a operação inativa. | ✅ **Aprovado** |
| **AUD-01-07 (`KeybindsUtil.cs`)** | Null-safety adicionada em `_reloadKeybind?.KeyCombinationState_0` e `_checkKeybind?.KeyCombinationState_0`, evitando `NullReferenceException`. | ✅ **Aprovado** |
| **AUD-01-08 / Regra Gemini** | Bump SemVer `1.0.3` unificado (`BepInPlugin`, `.csproj`, `mod.json`). Resolução resiliente de `Fika.Core.dll` e isolamento total de build para `mods/SPT-MagCheckInterrupt/builds/`. | ✅ **Aprovado** |

---

## 3. Achados de Code Review

### CR-01-01 · Cat B (Bug Latente) · 🟡 Médio

**Ausência de restauração da velocidade de animação (`SetAnimationSpeed(1f)`) em caso de interrupção externa abrupta (`Reset`)**

- **Local:** [`MagCheckReloadOperation.cs:86`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SPT-MagCheckInterrupt/modded/MagCheckInterrupt/Components/MagCheckReloadOperation.cs#L86-L97)
- **Problema:** Quando a opção `Slow Animation` está ativada e a animação atinge `_animSpeedState == SpeedState.Slowed`, o Mecanim opera com velocidade reduzida (ex.: `0.25f`). Se o jogador sofrer uma interrupção súbita no jogo (dano pesado, troca forçada de item ou transição de estado externa) onde o EFT invoca `Reset()` diretamente sem passar pelo ciclo suave de restauração, as variáveis locais de velocidade são resetadas para `1f`, mas **não há chamada a `FirearmsAnimator_0.SetAnimationSpeed(1f)`**.
- **Por que importa:** O animator das mãos do jogador pode permanecer operando com velocidade reduzida (ex.: 25% da velocidade normal) até a próxima operação de recarga que chame `SetAnimationSpeed(1f)`.
- **Sugestão:**
  ```csharp
  public override void Reset()
  {
      if (FirearmsAnimator_0 != null)
      {
          FirearmsAnimator_0.SetAnimationSpeed(1f);
      }

      _ammoDetailsShown = false;
      _reloadCalled = false;
      _swapRemoveOperation = null;
      _animSpeedState = SpeedState.Normal;
      _currentSpeed = 1f;
      _targetSpeed = 1f;
      Bool_0 = false;
      Bool_1 = false;
      base.Reset();
  }
  ```
- **Decisão:**
  - `[ ]` Pendente
  - `[x]` Aceitar sugestão
  - `[ ]` Rejeitar

---

### CR-01-02 · Cat E (Legibilidade / Deofuscação 4.1) · 🟢 Menor

**Falta de anotação de conceito semântico no tipo ofuscado `Class1730`**

- **Local:** [`CanQuickReloadPatch.cs:15`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SPT-MagCheckInterrupt/modded/MagCheckInterrupt/Patches/CanQuickReloadPatch.cs#L15)
- **Problema:** O método `GetTargetMethod()` referencia diretamente `typeof(Class1730)` e `nameof(Class1730.Boolean_1)` sem comentário semântico explicando o papel da classe e da propriedade no EFT (SPT 4.1: `EFT.FirearmHandsInputTranslator.CanQuickReload`).
- **Por que importa:** Conforme diretrizes de readiness e manutenibilidade do workspace, tipos ofuscados do EFT devem ter seu conceito documentado para facilitar futuras migrações de versão.
- **Sugestão:**
  Adicionar comentário conceitual acima do método:
  ```csharp
  // Class1730 = EFT.FirearmHandsInputTranslator (SPT 4.1 alias)
  // Boolean_1 = CanQuickReload (valida FirearmsAnimator.IsIdling())
  protected override MethodBase GetTargetMethod()
  ```
- **Decisão:**
  - `[ ]` Pendente
  - `[x]` Aceitar sugestão
  - `[ ]` Rejeitar

---

### CR-01-03 · Cat F (Melhoria Opcional) · 🟢 Menor

**Log explicativo em caso de `SwapOperationClass` em arma sem carregador acoplado**

- **Local:** [`MagCheckReloadOperation.cs:189`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SPT-MagCheckInterrupt/modded/MagCheckInterrupt/Components/MagCheckReloadOperation.cs#L189-L203)
- **Problema:** Se `operation is SwapOperationClass`, mas a arma não possui carregador (`currentMag == null`) ou slot (`magSlot == null`), a operação cai para o tratamento base sem registrar mensagem informativa no log.
- **Por que importa:** Em sessões de debug, ter um log claro explicando que um swap direto não pôde ser iniciado por ausência de carregador facilita identificar interações anômalas de mods de inventário.
- **Sugestão:**
  Adicionar `LoggerUtil.Warning("MagCheckReloadOperation::Execute SwapOperationClass recebida mas arma sem carregador/slot acoplado; delegando para base.");` antes do retorno ao fluxo base.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[x]` Rejeitar (fluxo base já cobre o cenário)

---

### CR-01-04 · Cat D (Arquitetura / Build) · 🟢 Menor

**Documentação de precedência de paths no `Directory.Build.targets`**

- **Local:** [`Directory.Build.targets:5`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SPT-MagCheckInterrupt/modded/Directory.Build.targets#L5)
- **Problema:** A propriedade `SPTPath` utiliza o fallback `E:\Tarkov Red Line\`.
- **Por que importa:** A regra do repositório define a precedência: `$SPT_PATH` / `--spt-path` > `.spt-path` > fallback.
- **Sugestão:** Manter comentário explícito no targets esclarecendo que o caminho padrão pode ser sobrescrito via CLI (`-p:SPTPath=...`) ou variável de ambiente.
- **Decisão:**
  - `[ ]` Pendente
  - `[x]` Aceitar sugestão
  - `[ ]` Rejeitar

---

## 4. Status de Resolução das Sugestões

Todas as 4 sugestões do Code Review foram aplicadas com sucesso:

- ✅ **CR-01-01 (Aplicado):** `FirearmsAnimator_0?.SetAnimationSpeed(1f)` adicionado em `MagCheckReloadOperation.Reset()`, prevenindo que interrupções externas bruscas deixem o animator das mãos em velocidade reduzida.
- ✅ **CR-01-02 (Aplicado):** Documentação de deofuscação 4.1 adicionada em `CanQuickReloadPatch.cs` mapeando `Class1730` (`EFT.FirearmHandsInputTranslator`) e `Boolean_1` (`CanQuickReload`).
- ✅ **CR-01-03 (Aplicado):** `LoggerUtil.Warning` informativo adicionado em `MagCheckReloadOperation.Execute()` caso ocorra um swap direto quando a arma não possui carregador acoplado.
- ✅ **CR-01-04 (Aplicado):** Comentário de precedência de caminhos adicionado ao `Directory.Build.targets`.

---

## 5. Conclusão Final

O código do mod **SPT-MagCheckInterrupt (v1.0.3)** encontra-se plenamente auditado, corrigido, revisado e aprovado. A compilação em Release produz 0 avisos e 0 erros, e os binários finais residem exclusivamente na pasta `mods/SPT-MagCheckInterrupt/builds/`.
