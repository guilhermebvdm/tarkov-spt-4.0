---
title: "Relatório de Auditoria Técnica de Código — SPT-MagCheckInterrupt (Review 01)"
date: 2026-09-05
status: 🟢 Vivo
authors: Antigravity
---

# Relatório de Auditoria Técnica de Código — SPT-MagCheckInterrupt (Review 01)

Este relatório consolida a auditoria estática profunda e minuciosa realizada no mod **SPT-MagCheckInterrupt** (versão `1.0.2`), focando especialmente nos mecanismos de recarga, transição de animações, integração cooperativa com o **FIKA** e compatibilidade de inventário com o **UIFixes**.

---

## 1. Resumo Executivo da Auditoria

| Severidade | Quantidade | Descrição |
| :--- | :---: | :--- |
| 🔴 **Crítico** | 2 | Gatilho travado permanentemente (`bool_1`) após swap/interrupção e quebra de swap atômico do UIFixes com falha de callback. |
| 🟠 **Alto** | 2 | Omissão de pacote de rede `ReloadCalledPacket` em swaps no FIKA e vazamento cumulativo de listeners em `SettingChanged`. |
| 🟡 **Médio** | 2 | Polling contínuo de `SetAnimationSpeed` todo frame e autorização prematura de `QuickReload` fora da janela de recarga. |
| 🔵 **Baixo** | 1 | Risco de `NullReferenceException` em `AreCheckAndReloadKeysConflicting` sem validação de nulo em keybinds. |
| 💡 **Otimização** | 1 | Eliminação de micro-alocações e cacheamento defensivo de delegates de rede. |

---

## 2. Tabela de Achados

| ID | Severidade | Arquivo / Linha | Categoria | Descrição Resumida |
| :--- | :---: | :--- | :--- | :--- |
| `AUD-01-01` | 🔴 **Crítico** | [`SwapReloadOperation.cs:48, 350`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SPT-MagCheckInterrupt/original/MagCheckInterrupt/Components/SwapReloadOperation.cs#L48) | Concorrência / Hands | `_blockTriggerField` (`FirearmController.bool_1`) não é restaurado para `false` em `EndOperation`, travando o disparo da arma. |
| `AUD-01-02` | 🔴 **Crítico** | [`MagCheckReloadOperation.cs:186-236`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SPT-MagCheckInterrupt/original/MagCheckInterrupt/Components/MagCheckReloadOperation.cs#L186-L236) | Inventário / UIFixes | Incompatibilidade com `SwapOperationClass`: espera duas operações separadas, resultando em `callback.Fail` e quebra de inventário. |
| `AUD-01-03` | 🟠 **Alto** | [`AnimationUtil.cs:47`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SPT-MagCheckInterrupt/original/MagCheckInterrupt/Utils/AnimationUtil.cs#L47) | Netcode / FIKA | Não envio de `ReloadCalledPacket` quando `isSwap == true`, causando encerramento prematuro via `FastForward` em clientes remotos no FIKA. |
| `AUD-01-04` | 🟠 **Alto** | [`External/Fika.cs:78, 125`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SPT-MagCheckInterrupt/original/MagCheckInterrupt/External/Fika.cs#L78) | Memory Leak | Inscrição cumulativa de `SettingChanged` em `OnFikaNetworkManagerCreated` sem desinscrição (`-=`), duplicando envio de pacotes a cada raid como Host. |
| `AUD-01-05` | 🟡 **Médio** | [`MagCheckReloadOperation.cs:81-82`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SPT-MagCheckInterrupt/original/MagCheckInterrupt/Components/MagCheckReloadOperation.cs#L81-L82) | Performance / CPU | Execução redundante de `FirearmsAnimator_0.SetAnimationSpeed(_currentSpeed)` a cada frame mesmo com velocidade constante `1.0f`. |
| `AUD-01-06` | 🟡 **Médio** | [`CanQuickReloadPatch.cs:19-27`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SPT-MagCheckInterrupt/original/MagCheckInterrupt/Patches/CanQuickReloadPatch.cs#L19-L27) | Input / Animação | Postfix força `Boolean_1 = true` em `Class1730` mesmo fora da janela válida de recarga (`CanStartReload() == false`), truncando animações de mãos. |
| `AUD-01-07` | 🔵 **Baixo** | [`KeybindsUtil.cs:29-35`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SPT-MagCheckInterrupt/original/MagCheckInterrupt/Utils/KeybindsUtil.cs#L29-L35) | Robustez / NRE | `AreCheckAndReloadKeysConflicting()` acessa `_reloadKeybind` e `_checkKeybind` sem null-check prévio, podendo gerar NRE em inicialização assíncrona. |
| `AUD-01-08` | 💡 **Otimização** | [`External/Fika.cs:42-45`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SPT-MagCheckInterrupt/original/MagCheckInterrupt/External/Fika.cs#L42-L45) | Ciclo de Vida | Ausência de hook de encerramento para `FikaEventDispatcher` ao descarregar ou reiniciar instâncias de plugin. |

---

## 3. Detalhamento dos Achados

### AUD-01-01 · Bloqueio permanente de gatilho (`bool_1`) em `SwapReloadOperation`
- **Severidade:** 🔴 Crítico
- **Localização no Mod:** [`SwapReloadOperation.cs:L48`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SPT-MagCheckInterrupt/original/MagCheckInterrupt/Components/SwapReloadOperation.cs#L48) e [`SwapReloadOperation.cs:L347-L356`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SPT-MagCheckInterrupt/original/MagCheckInterrupt/Components/SwapReloadOperation.cs#L347-L356)
- **Referência Cruzada:** [`references/eft-decompiled/Assembly-CSharp/EFT/Player.cs:L5435, L5309`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L5435)
- **Causa Raiz:** Ao iniciar a operação de swap, o mod ativa o campo obfuscado de bloqueio de gatilho do EFT via Reflection:
  ```csharp
  _blockTriggerField(FirearmController_0) = true; // bool_1 = true
  ```
  No código nativo do Tarkov (`Player.cs:5435`), quando `FirearmController.bool_1 == true`, qualquer clique de disparo é descartado incondicionalmente. O EFT só restaura `bool_1 = false` no método `IdlingOperation.OnIdleStartEvent()` (`Player.cs:5309`). Contudo, se a animação do Unity sofrer *CrossFade*, interrupção ou troca de arma (`HideWeapon`), o evento de animação `OnIdleStartEvent` **nunca é chamado**. Em `EndOperation()`, `Reset()` e falhas de inserção (`insertResult.Failed`), o mod nunca redefine `_blockTriggerField(FirearmController_0) = false`.
- **Impacto Técnico Real:** O jogador realiza o swap ou tem a recarga interrompida e a arma **para de atirar completamente**. O clique esquerdo é ignorado e o jogador só recupera o controle se reiniciar a arma ou trocar de slot de inventário.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  Garantir a liberação de `_blockTriggerField(FirearmController_0) = false` em `EndOperation()`, em caso de falha (`insertResult.Failed`) e no método `Reset()` / `HideWeapon()`.
  
```csharp
private void EndOperation()
{
    LoggerUtil.Debug("SwapReloadOperation::EndOperation");

    _blockTriggerField(FirearmController_0) = false; // Restaura disparo imediatamente
    State = EOperationState.Finished;
    FirearmController_0.RecalculateErgonomic();
    FirearmController_0.InitiateOperation<IdlingOperation>().Start(null);
    _finishCallback?.Succeed();
    FirearmController_0.WeaponModified();
}
```

- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar: _________________

---

### AUD-01-02 · Quebra de execução com `SwapOperationClass` do UIFixes
- **Severidade:** 🔴 Crítico
- **Localização no Mod:** [`MagCheckReloadOperation.cs:L184-L236`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SPT-MagCheckInterrupt/original/MagCheckInterrupt/Components/MagCheckReloadOperation.cs#L184-L236)
- **Referência Cruzada:** [`Assembly-CSharp/SwapOperationClass.cs:L12, L102-L109`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/SwapOperationClass.cs#L12)
- **Causa Raiz:** O patch `CanExecuteSwapPatch` autoriza expressamente a operação `SwapOperationClass` a ser processada pelo `FirearmController`:
  ```csharp
  if (operation is not (SwapOperationClass or RemoveOperation or AttachOperation)) return;
  __result = true;
  ```
  No entanto, o método receptor `MagCheckReloadOperation.Execute` espera que o processo ocorra em duas etapas sequenciais: primeiro uma `RemoveOperation` isolada (para preencher `_swapRemoveOperation`) e depois uma inserção (`To1.IsChildOf(Weapon_0)`). Quando o UIFixes envia um `SwapOperationClass` atômico do EFT:
  1. `SwapOperationClass` implementa `IOneItemOperation`, mas **não** é do tipo `RemoveOperation`.
  2. `_swapRemoveOperation` nunca é atribuído (permanece `null`).
  3. A condição `To1.IsChildOf(Weapon_0)` é verdadeira, acionando a linha 213:
     ```csharp
     callback.Fail("Remove magazine operation is missing during execution of swap reload");
     ```
  4. O callback falha, o swap é rejeitado pelo controlador e a operação é abortada abruptamente, deixando o inventário em estado de erro.
- **Impacto Técnico Real:** Carregadores piscando no inventário, mensagens de erro no console BepInEx e impossibilidade de recarregar arrastando o carregador para a arma durante a inspeção.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  Identificar diretamente quando a operação for uma instância de `SwapOperationClass`, extraindo o carregador antigo diretamente da propriedade `Item_0` ou `Item1` da operação sem depender de duas mensagens desconexas.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar: _________________

---

### AUD-01-03 · Omissão de `ReloadCalledPacket` para recargas de Swap no FIKA
- **Severidade:** 🟠 Alto
- **Localização no Mod:** [`AnimationUtil.cs:L47`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SPT-MagCheckInterrupt/original/MagCheckInterrupt/Utils/AnimationUtil.cs#L47)
- **Referência Cruzada:** [`mods/FIKA/modded/.../QuickReloadMagPacket.cs:L47`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/modded/Fika-Plugin/Fika.Core/Networking/Packets/FirearmController/SubPackets/QuickReloadMagPacket.cs#L47) e [`ReloadMagPacket.cs:L77`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/modded/Fika-Plugin/Fika.Core/Networking/Packets/FirearmController/SubPackets/ReloadMagPacket.cs#L77)
- **Causa Raiz:** Em `AnimationUtil.TransitionToReload`:
  ```csharp
  // But if it's a swap reload, no need to send a packet.
  if (!isSwap && External.Fika.IsPresent)
  {
      External.Fika.SendReloadCalledPacket();
  }
  ```
  O autor supôs que swaps não precisavam notificar o FIKA. Porém, quando o FIKA processa a recarga em clientes observadores (`ObservedFirearmController`), ele **sempre** invoca `FastForwardCurrentState()`. Sem o `ReloadCalledPacket`, o cliente remoto não ativa a flag `_reloadCalled`. Ao rodar `FastForward()`, o cliente remoto encerra prematuramente a operação de recarga e retorna para o estado Idle, dessincronizando a animação de recarga do personagem para os demais jogadores.
- **Impacto Técnico Real:** Dessincronização visual em partidas cooperativas: os companheiros veem o jogador cancelar a checagem e ficar com a arma travada em mãos enquanto ele localmente está efetuando a troca de magazine.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  Disparar `External.Fika.SendReloadCalledPacket()` independentemente de `isSwap` quando houver transição de recarga válida em primeira pessoa.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar: _________________

---

### AUD-01-04 · Inscrição cumulativa de `SettingChanged` sem desinscrição no FIKA
- **Severidade:** 🟠 Alto
- **Localização no Mod:** [`External/Fika.cs:L78`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SPT-MagCheckInterrupt/original/MagCheckInterrupt/External/Fika.cs#L78) e [`ConfigUtil.cs:L151`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SPT-MagCheckInterrupt/original/MagCheckInterrupt/Utils/ConfigUtil.cs#L151)
- **Referência Cruzada:** [`docs/technical/spt-antipatterns.md (AP-01)`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/docs/technical/spt-antipatterns.md)
- **Causa Raiz:** O evento `FikaNetworkManagerCreatedEvent` é disparado a cada partida coop iniciada. No bloco do Host (`case FikaServer`), o mod inscreve o handler de configurações:
  ```csharp
  ConfigUtil.RegisterSettingsChanged(OnHostSettingsChanged);
  ```
  No `ConfigUtil`, a inscrição é feita diretamente no evento estático do `ConfigFile` (`_configFile.SettingChanged += eventArgs;`) sem nenhuma desinscrição (`-=`) anterior ou ao término da partida (`FikaGameEndedEvent`).
- **Impacto Técnico Real:** Vazamento cumulativo de listeners de delegates por toda a vida útil do executável do cliente. A cada raid jogada como Host, múltiplos pacotes `ConfigPacket` idênticos são transmitidos pela rede a cada clique no menu de configuração.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  Tornar a inscrição idempotente via desinscrição defensiva prévia:
  ```csharp
  public static void RegisterSettingsChanged(EventHandler<SettingChangedEventArgs> eventArgs)
  {
      _configFile.SettingChanged -= eventArgs;
      _configFile.SettingChanged += eventArgs;
  }
  ```
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar: _________________

---

### AUD-01-05 · Polling contínuo e incondicional de `SetAnimationSpeed` no `Update`
- **Severidade:** 🟡 Médio
- **Localização no Mod:** [`MagCheckReloadOperation.cs:L81-L82`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SPT-MagCheckInterrupt/original/MagCheckInterrupt/Components/MagCheckReloadOperation.cs#L81-L82)
- **Referência Cruzada:** [`docs/technical/spt-antipatterns.md (AP-03)`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/docs/technical/spt-antipatterns.md)
- **Causa Raiz:** No método `Update`, após calcular a interpolação `_currentSpeed`:
  ```csharp
  _currentSpeed = Mathf.MoveTowards(_currentSpeed, _targetSpeed, ConfigUtil.SlowSmoothing.Value * deltaTime);
  FirearmsAnimator_0.SetAnimationSpeed(_currentSpeed); // Set every frame okay?
  ```
  Mesmo quando o recurso `SlowAnimation` não está ativado, ou quando a animação já concluiu a desaceleração e retornou para a velocidade normal `1.0f`, a propriedade do Mecanim é gravada todos os frames a 60-144 FPS.
- **Impacto Técnico Real:** Chamadas JNI/PInvoke desnecessárias para o motor C++ do Unity todo frame, gerando overhead constante no loop da animação de armas.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  Aplicar guarda com epsilon (`Mathf.Approximately`): só invocar `SetAnimationSpeed` quando houver diferença real entre a velocidade atual e a desejada.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar: _________________

---

### AUD-01-06 · Autorização prematura de `QuickReload` em `Class1730`
- **Severidade:** 🟡 Médio
- **Localização no Mod:** [`CanQuickReloadPatch.cs:L19-L27`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SPT-MagCheckInterrupt/original/MagCheckInterrupt/Patches/CanQuickReloadPatch.cs#L19-L27)
- **Referência Cruzada:** [`references/eft-decompiled/Assembly-CSharp/Class1730.cs:L411, L438`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/Class1730.cs#L411)
- **Causa Raiz:** O patch em `Class1730.Boolean_1` (que valida se a arma está apta a receber comando de recarga rápida) força `__result = true` sempre que a operação for `MagCheckReloadOperation`, **sem verificar se a animação está dentro da janela configurada** (`ReloadWindowStart` a `ReloadWindowEnd`).
- **Impacto Técnico Real:** Se o jogador der duplo clique em R nos primeiros 5% da animação de checagem, o input translator aceita o comando e chama `_player.RemoveLeftHandItem(3f)`, mas a operação rejeita o `QuickReloadMag` (`CanStartReload() == false`). O jogador tem itens retirados da mão temporariamente e a animação engasga.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  Verificar se a operação ativa pode de fato recarregar no momento da checagem:
  ```csharp
  if (__instance.IfirearmHandsController_0 is FirearmController { CurrentOperation: MagCheckReloadOperation op } && op.CanStartReload())
  {
      __result = true;
  }
  ```
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar: _________________

---

### AUD-01-07 · Acesso desprotegido a referências de Keybinds em `KeybindsUtil`
- **Severidade:** 🔵 Baixo
- **Localização no Mod:** [`KeybindsUtil.cs:L29-L35`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SPT-MagCheckInterrupt/original/MagCheckInterrupt/Utils/KeybindsUtil.cs#L29-L35)
- **Causa Raiz:** `AreCheckAndReloadKeysConflicting()` avalia diretamente `_reloadKeybind.KeyCombinationState_0` e `_checkKeybind.KeyCombinationState_0`. Se o jogador iniciar uma checagem antes de `UpdateBindingsPatch` ter sido invocado pelo EFT, os campos estáticos permanecem nulos, resultando em `NullReferenceException`.
- **Impacto Técnico Real:** Risco de NRE silenciosa ou travamento na inicialização da operação em circunstâncias de carregamento rápido.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  Inserir verificação de nulo (`if (_reloadKeybind == null || _checkKeybind == null) return false;`).
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar: _________________

---

### AUD-01-08 · Ausência de Teardown de Eventos em `External/Fika.cs`
- **Severidade:** 💡 Otimização
- **Localização no Mod:** [`External/Fika.cs:L42-L45`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/SPT-MagCheckInterrupt/original/MagCheckInterrupt/External/Fika.cs#L42-L45)
- **Causa Raiz:** Inscrições globais no `FikaEventDispatcher` sem método correspondente de cleanup em caso de unload de plugins.
- **Impacto Técnico Real:** Baixo em produção mono-instância, mas relevante para conformidade arquitetural com o ciclo de vida do ecossistema.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  Criar método `Dispose()` ou `Cleanup()` para desinscrição formal caso o plugin seja desativado.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar: _________________

---

## 4. Plano de Ação e Recomendações

1. **Correção Imediata dos Itens Críticos (`AUD-01-01` e `AUD-01-02`):**
   * Desbloquear explicitamente o gatilho (`_blockTriggerField = false`) no término de `SwapReloadOperation`.
   * Tratar `SwapOperationClass` nativo do EFT dentro de `MagCheckReloadOperation.Execute`, permitindo que recargas in-place com o inventário cheio funcionem sem abortar a operação.
2. **Sincronização FIKA (`AUD-01-03` e `AUD-01-04`):**
   * Transmitir `ReloadCalledPacket` independentemente da modalidade de recarga (normal ou swap) para manter o cliente observador informado antes do `FastForward`.
   * Tornar a inscrição de `SettingChanged` estritamente idempotente.
3. **Refinamento de Inputs e Animação (`AUD-01-05` e `AUD-01-06`):**
   * Adicionar gating de velocidade em `Update` e condicionar `CanQuickReloadPatch` ao estado real `CanStartReload()`.
