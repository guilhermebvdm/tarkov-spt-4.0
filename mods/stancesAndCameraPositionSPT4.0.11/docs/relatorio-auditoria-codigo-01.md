---
title: "Relatório de Auditoria Técnica de Código — TRL-StancesAndMobility (Review 01)"
date: 2026-08-30
status: 🟢 Vivo
authors: Antigravity
---

# Relatório de Auditoria Técnica de Código — TRL-StancesAndMobility (Review 01)

Este relatório consolida a auditoria técnica estática profunda do código-fonte localizado em [`modded-testchannel/`](../modded-testchannel/), com foco na estabilidade de raid, compatibilidade com EFT 0.16.9 / SPT 4.0 / FIKA Coop e na **investigação da causa raiz do bug de travamento de armas/inventário** identificado no log de raid.

---

## 1. Resumo Executivo da Auditoria

| Severidade | Quantidade | Descrição |
| :--- | :--- | :--- |
| 🔴 **Crítico** | 1 | Lock permanente de inventário (`GClass1561`) por manipulação direta de `PopTo` e atraso assíncrono em `ChamberUnload`. |
| 🟠 **Alto** | 2 | `HandsStateGuard` desconectado (trocas de postura durante itens médicos/recarga) e callbacks de rede envelopados sem failsafe. |
| 🟡 **Médio** | 3 | Polling contínuo em `StanceManager.Update`, interpolação de strings em hot paths e UIs de debug ativas. |
| 🔵 **Baixo** | 2 | Convenções de nomenclatura, campos legados e comentários defasados. |
| 💡 **Otimização** | 2 | Cache estático de `FieldInfo` e eliminação de polling redundante de GameWorld. |

---

## 2. Tabela de Achados

| ID | Severidade | Arquivo / Linha | Categoria | Descrição Resumida |
| :--- | :--- | :--- | :--- | :--- |
| `AUD-01-01` | 🔴 Crítico | [`Patches/ManualChamberingPatches.cs:L347`](../modded-testchannel/Patches/ManualChamberingPatches.cs#L347) | Lock de Inventário | `PopTo` direto e `ChamberUnload` assíncrono causam lock permanente de inventário no servidor Fika (`GClass1561`). |
| `AUD-01-02` | 🟠 Alto | [`HandsStateGuard.cs:L11`](../modded-testchannel/HandsStateGuard.cs#L11) | Função Órfã / Estado | `HandsStateGuard` criado na Sessão 15 não é invocado em nenhum ponto do `StanceManager`. |
| `AUD-01-03` | 🟠 Alto | [`Patches/ActionStancePatches.cs:L91`](../modded-testchannel/Patches/ActionStancePatches.cs#L91) | FIKA / Callback | Envelopamento de `Callback` de recarga/unload sem timeout ou proteção contra cancelamento externo. |
| `AUD-01-04` | 🟡 Médio | [`Patches/ManualChamberingPatches.cs:L251`](../modded-testchannel/Patches/ManualChamberingPatches.cs#L251) | Singleplayer Assumption | `SetAmmoCompatiblePatch` e `SetAmmoOnMagPatch` assumem `MainPlayer` sem isolamento seguro. |
| `AUD-01-05` | 🟡 Médio | [`StanceManager.cs:L127`](../modded-testchannel/StanceManager.cs#L127) | Polling em Update | `StanceManager.Update` faz múltiplas checagens de estados por frame que podem ser reativas. |
| `AUD-01-06` | 🟡 Médio | [`AdsSpeedDebugUI.cs:L20`](../modded-testchannel/AdsSpeedDebugUI.cs#L20) | GC Pressure | UIs de debug instanciadas em runtime alocando strings e caixas de layout a cada frame. |
| `AUD-01-07` | 🔵 Baixo | [`Plugin.cs:L47`](../modded-testchannel/Plugin.cs#L47) | Documentação | Comentário indicando `Stance 0: irrelevante` quando a Stance 0 aplica cap intencional de 90%. |
| `AUD-01-08` | 💡 Otimização | [`StanceManager.cs:L79`](../modded-testchannel/StanceManager.cs#L79) | Cache de Singleton | `GetCachedGameWorld` pode ser consolidado com eventos de raid (`OnGameStarted` / `OnGameEnded`). |

---

## 3. Detalhamento dos Achados e Investigação da Causa Raiz do Bug

### AUD-01-01 · Lock Permanente de Inventário no Servidor Fika (`GClass1561`)
- **Severidade:** 🔴 Crítico
- **Localização:** [`Patches/ManualChamberingPatches.cs:L198`](../modded-testchannel/Patches/ManualChamberingPatches.cs#L198) e [`Patches/ManualChamberingPatches.cs:L347-L415`](../modded-testchannel/Patches/ManualChamberingPatches.cs#L347-L415)
- **Referência Cruzada:** [`references/eft-decompiled/Assembly-CSharp/TraderControllerClass.cs:L1133`](../../../references/eft-decompiled/Assembly-CSharp/TraderControllerClass.cs#L1133) e [`references/fika-plugin/Fika.Core/Networking/FikaServer.Callbacks.cs:L144`](../../../references/fika-plugin/Fika.Core/Networking/FikaServer.Callbacks.cs#L144)
- **Causa Raiz Comprovada no Log:**
  1. No log do jogador (Linha 3092), o mod registrou: `[Info :TRL-StancesAndMobility] [ManualChamber] Vanilla UnloadChamber detectado. Forçando Stance 0 e aguardando.`.
  2. O patch `ManualChamberingInputPatch.Prefix` interceptou o comando `ECommand.ChamberUnload`, bloqueou o comando imediato (`return false`) e iniciou o `ManualChamberingComponent` em `Phase = 3`.
  3. No método `RechamberRound` e no `StartEquipWeapPatch`, o código invoca `mag.Cartridges.PopTo(player.InventoryController, fc.Item.Chambers[0].CreateItemAddress())`.
  4. Essa chamada local no cliente abre uma transação de inventário (`InOutHandsProcess` / `GEventArgs17`) na lista interna `TraderControllerClass.List_0`.
  5. No modo coop FIKA, o servidor host (Headless/Host) não recebe o encerramento ordenado dessa transação. Quando o jogador tenta puxar a arma primária (Steyr AUG), o servidor do FIKA executa `item.CheckAction(null)` em `FikaServer.Callbacks.cs:144` e rejeita a operação com [`GClass1561`](../../../references/eft-decompiled/Assembly-CSharp/GClass1561.cs):
     > *"Player cannot equip item ... because item Default Inventory is currently being modified"*
  6. O cliente recebe o erro no callback e deixa o `FirearmController` em estado nulo/não-inicializado, bloqueando o disparo de todas as armas e disparando o NRE no `FirearmController.method_64()`.
- **Impacto Real:** Trava total de armas e itens médicos após ações de câmara em servidores coop FIKA.
- **Proposta de Correção:**
  - Desativar a manipulação direta de `PopTo` e substituição assíncrona de comandos de câmara quando em ambiente multiplayer FIKA, ou assegurar que operações de câmara utilizem exclusivamente os métodos nativos de hands controller do EFT sem atrasos via `MonoBehaviour.Update`.

---

### AUD-01-02 · `HandsStateGuard` Criado mas Desconectado
- **Severidade:** 🟠 Alto
- **Localização:** [`HandsStateGuard.cs:L11`](../modded-testchannel/HandsStateGuard.cs#L11)
- **Causa Raiz:** A classe [`HandsStateGuard`](../modded-testchannel/HandsStateGuard.cs) foi implementada para checar `CanChangeStance(player)` (evitando trocas de postura durante o uso de remédios, comida ou quando as mãos estiverem ocupadas), porém **nenhum método em `StanceManager.cs` ou `Plugin.cs` a invoca**.
- **Impacto Real:** O jogador consegue acionar hotkeys de stance ou a roda do mouse enquanto aplica um AFAK ou bandagem, enviando pacotes fora de sincronia e concorrendo com animações de uso de consumíveis.
- **Proposta de Correção:** Inserir a verificação `if (!HandsStateGuard.CanChangeStance(mainPlayer)) return;` no início de `HandleStanceHotkeys()`, `HandleLinearScroll()`, `Update()` e `TryInterceptTriggerDown()`.

---

### AUD-01-03 · Envelopamento de Callbacks sem Failsafe em Action Stances
- **Severidade:** 🟠 Alto
- **Localização:** [`Patches/ActionStancePatches.cs:L91-L101`](../modded-testchannel/Patches/ActionStancePatches.cs#L91-L101) e [`Patches/ActionStancePatches.cs:L169-L179`](../modded-testchannel/Patches/ActionStancePatches.cs#L169-L179)
- **Causa Raiz:** `ActionStanceReloadPatch` e `ActionStanceUnloadMagPatch` substituem o `Callback` original da operação por uma nova instância:
  ```csharp
  var orig = callback;
  callback = new Callback((res) =>
  {
      if (orig != null)
      {
          if (res.Succeed) orig.Succeed();
          else orig.Fail(res.Error);
      }
      StanceManager.EndActionStance(forceCancel: !res.Succeed);
  });
  ```
  Se a operação for abortada no meio (ex.: cancelamento de recarga por sprint ou interrupção do FIKA), a closure pode ser perdida ou manter a `ActionStance` presa em `_isActionStanceActive = true`.
- **Proposta de Correção:** Adicionar try/finally na execução do callback e um timeout de segurança no `StanceManager` para forçar o reset caso o evento de Idle não seja disparado em até 5 segundos.

---

### AUD-01-04 · Singleplayer Assumption em `SetAmmoCompatiblePatch`
- **Severidade:** 🟡 Médio
- **Localização:** [`Patches/ManualChamberingPatches.cs:L258-L265`](../modded-testchannel/Patches/ManualChamberingPatches.cs#L258-L265)
- **Causa Raiz:** O patch força `compatible = false` no `FirearmsAnimator.SetAmmoCompatible` consultando `Singleton<GameWorld>.Instance?.MainPlayer`. Se o animador for de um bot ou de outro jogador no FIKA, a checagem pode falhar ou interferir em instâncias incorretas.
- **Proposta de Correção:** Validar estritamente se o `__instance` pertence ao `MainPlayer.HandsController.FirearmsAnimator` antes de modificar o parâmetro de compatibilidade.

---

## 4. Plano de Ação e Recomendações

1. **Correção Imediata do Bug de Inventário (`AUD-01-01`):**
   - Refatorar o `ManualChamberingPatches.cs` para remover a injeção assíncrona de `PopTo` e o delay artificial de `ECommand.ChamberUnload`.
2. **Conexão do Guard de Mãos (`AUD-01-02`):**
   - Conectar [`HandsStateGuard.CanChangeStance`](../modded-testchannel/HandsStateGuard.cs) em todos os fluxos de entrada do [`StanceManager`](../modded-testchannel/StanceManager.cs).
3. **Failsafe de Callbacks (`AUD-01-03`):**
   - Implementar timeout de liberação em [`ActionStancePatches.cs`](../modded-testchannel/Patches/ActionStancePatches.cs).
4. **Limpeza de UIs de Debug (`AUD-01-06`):**
   - Garantir que `AdsSpeedDebugUI` e `SpeedLimitDebugUI` só aloquem strings e componentes quando suas respectivas opções no F12 estiverem ativadas.
