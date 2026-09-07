# 002 — Watchdog de Timeout de Inventário e Desync em Coop/Headless · Spec Técnica

**Mod:** FIKA  
**Status:** 🟢 Concluído  
**Data:** 2026-09-05  
**Autor:** Antigravity / saraiva  
**Spec Funcional:** [002-inventory-desync-watchdog-01-spec.md](002-inventory-desync-watchdog-01-spec.md)  

---

## 1. Análise da Causa Raiz

No cliente cooperativo do FIKA (`Fika.Core`):

1. **`WaitingForCallback`:**
   - Em `FikaPlayer.cs:65`:
     ```csharp
     public bool WaitingForCallback => _baseInventoryController.StrictSync && (OperationCallbacks.Count > 0 || _proceedCallbacks.Count > 0);
     ```
2. **Impacto nos Controladores de Mãos:**
   - Em `FikaClientFirearmController.cs:110`:
     ```csharp
     return !_fikaPlayer.WaitingForCallback && base.CanPressTrigger();
     ```
   - Em `FikaClientGrenadeController.cs:58`:
     ```csharp
     return !_fikaPlayer.WaitingForCallback && base.CanThrow();
     ```
   - Flares dependem de `FirearmController.CanPressTrigger()`.
3. **Comportamento do Melee:**
   - `FikaClientKnifeController` herda diretamente de `Player.KnifeController` e **não sobrescreve nem consulta** `WaitingForCallback`. Seus ataques ocorrem de forma puramente local, explicando por que facas continuam funcionando enquanto armas de fogo e granadas são bloqueadas.
4. **Ciclo de Vida do Callback:**
   - Quando o jogador inicia uma operação de inventário, `ClientInventoryController.AddOperationCallback` adiciona a transação em `FikaPlayer.OperationCallbacks[id]`.
   - Se o pacote `OperationCallbackPacket` for perdido na rede, ou se o Host/Headless falhar silenciosamente ao emitir resposta, a entrada permanece no dicionário indefinidamente.
   - O `ClientInventoryOperationHandler` associado nunca recebe o callback e nunca devolve a operação ao pool, mantendo o item em estado "blinking" na UI.

---

## 2. Arquitetura da Solução

Implementar um mecanismo de watchdog passivo no getter de `FikaPlayer.WaitingForCallback`:
1. **Rastreamento de Timestamp:**
   - `_operationCallbackTimestamps: Dictionary<uint, float>` registra `UnityEngine.Time.time` no momento do registro em `ClientInventoryController.AddOperationCallback`.
   - `_proceedCallbackTimestamps: Dictionary<uint, float>` registra `UnityEngine.Time.time` no momento do registro em `CreateProceedCallback`.
2. **Método `CleanupExpiredCallbacks()`:**
   - Avalia callbacks com idade superior a 5.0 segundos (`CALLBACK_TIMEOUT_SECONDS`).
   - Para callbacks de inventário expirados: remove do dicionário e invoca o callback com `ServerOperationStatus(EOperationStatus.Failed, "Network operation timed out")`.
   - Isso faz com que `ClientInventoryOperationHandler.ReceiveStatusFromServer` execute `HandleResultDelegate(new FailedResult(...))`, limpando o estado do item, disparando `RaiseRefreshEvent` e devolvendo o handler ao pool.
   - Para callbacks de proceed expirados: invoca `callback.Fail("Proceed network request timed out")`.
3. **Execução Automática:**
   - O método `CleanupExpiredCallbacks()` é executado no início do getter de `WaitingForCallback`.

---

## 3. Preservação de API Pública

Para garantir que nenhum mod dependente (`TRL-DynamicSpawn`, `stancesAndCameraPositionSPT4.0.11`, `SAIN`, etc.) quebre:
- `public readonly Dictionary<uint, Action<ServerOperationStatus>> OperationCallbacks` permanece inalterado.
- `public bool WaitingForCallback { get; }` permanece com a mesma assinatura.
- Nenhuma classe, struct ou interface pública teve campos renomeados ou removidos.
