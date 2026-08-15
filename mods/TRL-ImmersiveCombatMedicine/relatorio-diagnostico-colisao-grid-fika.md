# Diagnóstico Técnico: Colisão de Grid de Inventário no FIKA (Descarte de Itens Médicos)

**Data:** 15/08/2026  
**Módulo:** `TRL-ImmersiveCombatMedicine` (`modded-testchannel`)  
**Contexto:** Sincronização de Inventário Multiplayer (FIKA) & EFT 0.16.9 / SPT 4.0.13  

---

## 1. O Erro Observado

```text
[Error  :Fika.Client] HandleInventoryPacket::Unable to process descriptor from netId 8, error: (x: 0, y: 0, r: Horizontal) in grid 2 in item item_equipment_rig_cryeAVS_arena_bp_01 (id: 6a7fc921a0efbf20681d77cb) is taken by another item when trying to add item mag_glock_magpul_pmag_21_gl9_9x19_21 (id: 6a7fc921a0efbf20681d7992)
[Info   :TRL-ImmersiveCombatMedicine] [Trauma2] reconcile sweep n=1
[Error  :Fika.Client] HandleInventoryPacket::Unable to process descriptor from netId 8, error: (x: 0, y: 1, r: Horizontal) in grid 2 in item item_equipment_rig_cryeAVS_arena_bp_01 (id: 6a7fc921a0efbf20681d77cb) is taken by another item when trying to add item mag_glock_magpul_pmag_21_gl9_9x19_21 (id: 6a7fc921a0efbf20681d7994)
```

### O que o log indica:
1. O jogador remoto `netId 8` realizou ações no inventário e enviou descritores de rede via FIKA para mover dois carregadores de Glock (`mag_glock_magpul_pmag_21_gl9_9x19_21`) para o `grid 2` do seu colete tático `item_equipment_rig_cryeAVS_arena_bp_01` nas coordenadas `(x: 0, y: 0)` e `(x: 0, y: 1)`.
2. No cliente receptor, a engine interna do EFT (`StashGridClass.AddInternal` / `GClass1543`) rejeitou a operação com o erro `is taken by another item`, pois **aquelas coordenadas de células ainda estavam ocupadas por outro item** no espelho local do inventário.

---

## 2. Causa Raiz: Janela de Corrida no Descarte Diferido (`DeferredDiscardRoutine`)

### O Fluxo Problemático no Mod:
1. **Consumo do Item:** Durante uma ação de cura/medicina, o recurso do item médico é zerado (`HpResource = 0` ou `Resource.Value = 0`).
2. **Agendamento do Descarte:** [`MedicalLogic.ConsumeSafe`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Medical/MedicalLogic.cs#L505-L547) chama `DiscardItemNetworked`, que delega para [`BandAidController.ScheduleNetworkedDiscard`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Medical/BandAidController.cs#L944-L955).
3. **Espera Excessiva na Coroutine:** A rotina [`DeferredDiscardRoutine`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Medical/BandAidController.cs#L965-L1017) executa um polling com timeout de **até 6 segundos** esperando as mãos liberarem (`doctor.HandsController is Player.MedsController`), seguido de até **4 tentativas com 1.2s de espera de callback e 0.75s de retry**:
   ```csharp
   // BandAidController.cs linhas 974-980
   float handsDeadline = Time.time + 6f;
   while (Time.time < handsDeadline && doctor != null &&
          doctor.HandsController is Player.MedsController)
   {
       yield return new WaitForSeconds(0.25f);
   }
   yield return new WaitForSeconds(0.2f);
   ```
4. **A Colisão:** Durante essa janela de 6 a 12 segundos, o item de cura **continua registrado no grid do colete** no EFT dos outros clientes. Se o operador executa uma recarga de arma ou move magazines para o colete, o jogo tenta alocar os magazines nas células `(0, 0)` e `(0, 1)`, gerando a colisão física no grid.

---

## 3. Padrões de Referência (Vanilla EFT & FIKA)

### A. Padrão Vanilla EFT ([`GClass3017.RemoveItem`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/GClass3017.cs#L8-L33))
No EFT nativo, itens de cura consumidos são removidos pelo controlador de saúde de forma atômica no término do efeito:
```csharp
TraderControllerClass traderControllerClass = (TraderControllerClass)GClass3113.GetOwner(item.Parent);
if (item.StackObjectsCount > 1)
{
    var gStruct = InteractionsHandlerClass.SplitToNowhere(item, 1, traderControllerClass, traderControllerClass, simulate: false);
    gStruct.Value.RaiseEvents(traderControllerClass, CommandStatus.Begin);
    gStruct.Value.RaiseEvents(traderControllerClass, CommandStatus.Succeed);
}
else
{
    var gStruct2 = InteractionsHandlerClass.Discard(item, traderControllerClass);
    gStruct2.Value.RaiseEvents(traderControllerClass, CommandStatus.Begin);
    gStruct2.Value.RaiseEvents(traderControllerClass, CommandStatus.Succeed);
}
```

### B. Padrão FIKA ([`ClientSharedQuestController.cs:291-298`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/fika-plugin/Fika.Core/Main/ClientClasses/ClientSharedQuestController.cs#L291-L298))
No FIKA, transações de remoção/descarte devem ser simuladas e enviadas ao pipeline de rede:
```csharp
var removeResult = InteractionsHandlerClass.Remove(item, player.InventoryController, simulate: true);
player.InventoryController.TryRunNetworkTransaction(removeResult, result =>
{
    if (!result.Succeed)
    {
        FikaGlobals.LogError("Discard failed: " + result.Error);
    }
});
```

---

## 4. Arquivos Envolvidos no `TRL-ImmersiveCombatMedicine`

| Arquivo | Trecho Relevante | Papel no Problema |
| :--- | :--- | :--- |
| [`MedicalLogic.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Medical/MedicalLogic.cs#L505-L634) | `ConsumeSafe` (L505) & `StartDiscardAttempt` (L598) | Zera o recurso do item e agenda o descarte diferido sem remover o item do grid imediatamente. |
| [`BandAidController.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Medical/BandAidController.cs#L944-L1020) | `ScheduleNetworkedDiscard` & `DeferredDiscardRoutine` | Mantém coroutine com loop de espera de até 6 segundos + retries espaçados, mantendo o slot ocupado na rede. |
| [`MedicHealPatch.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/TRL-ImmersiveCombatMedicine/modded-testchannel/Patches/Medical/MedicHealPatch.cs#L204-L220) | `ForceFinishAnimation` | Invoca `method_9` diretamente por reflection para cortar a animação de mãos, desacoplando os callbacks normais do ciclo de vida de mãos do EFT. |

---

## 5. Sugestão de Correção para Implementação

1. **Vincular o Descarte ao Callback de Liberação de Mãos:**
   - Em vez de uma coroutine com `WaitForSeconds` arbitrário e timeout de 6s, o descarte deve ser engatilhado no momento em que `TrySetLastEquippedWeapon` / troca de controlador completa o seu callback (`Callback.Succeed`).
2. **Garantir Execução Imediata de `TryRunNetworkTransaction`:**
   - Assim que o item não estiver mais ativo no `HandsController`, executar imediatamente `InteractionsHandlerClass.Discard(item, doctor.InventoryController, simulate: true)` via `TryRunNetworkTransaction` sem retardos adicionais.
3. **Tratamento de Fallback Atômico:**
   - Se o item já estiver com `HpResource <= 0` e desconectado das mãos, executar o descarte no mesmo frame ou no próximo `LateUpdate`, garantindo que os pacotes de rede cheguem aos peers antes de qualquer operação subsequente de recarga/movimentação de carregadores.
