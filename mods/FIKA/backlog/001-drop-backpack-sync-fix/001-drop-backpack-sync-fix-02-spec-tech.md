# 001 — Fix descarte de mochila ("ZZ" / DropBackpack) no Headless/Host · Spec Técnica

**Mod:** FIKA  
**Status:** 🟢 Concluído  
**Data:** 2026-09-03  
**Autor:** Antigravity / saraiva  
**Spec Funcional:** [001-drop-backpack-sync-fix-01-spec.md](001-drop-backpack-sync-fix-01-spec.md)  

---

## 1. Análise da Causa Raiz

No código nativo do Escape From Tarkov (`references/eft-decompiled/Assembly-CSharp/`):

1. O slot `EquipmentSlot.Backpack` é classificado como o único `AnimatedSlot` do sistema de inventário (`InventoryController.cs:147`).
2. Quando um jogador invoca `DropBackpack()` (`Player.cs:31889`), o jogo executa `InventoryController.TryThrowItem(containedItem)`.
3. Isso dispara a pipeline de transferência com `OutProcess` (`Player.cs:1110`), que chama `method_34` (`Player.cs:1453`).
4. `method_34` chama `method_35(item, from, to)` (`Player.cs:1471`). Como `to == null` e `IsAnimatedSlot(from)` é verdadeiro para `EquipmentSlot.Backpack` (`Player.cs:1494`), `method_35` retorna a própria mochila como o item que requer remoção com animação:
   ```csharp
   return (to != null || !Player_0.InventoryController.IsAnimatedSlot(from)) ? null : item;
   ```
5. `method_34` então invoca `Player.TryRemoveFromHands(item2, abstractOperation, callback)` (`Player.cs:1458`).
6. Em `Player.TryRemoveFromHands` (`Player.cs:32223`):
   ```csharp
   else if (HandsController.CanExecute(abstractOperation))
   {
       ...
   }
   else
   {
       callback.Fail("hands controller can't perform this operation");
   }
   ```
7. No cliente local do jogador original, o controlador de mãos possui a máquina de estados para animações locais de descarte. Porém, no **Headless** e nos outros clientes, a entidade é um proxy de rede do tipo `ObservedPlayer`. O `HandsController` do `ObservedPlayer` responde `CanExecute() == false` para essa operação abstrata local.
8. Como consequência, o callback falha com `"hands controller can't perform this operation"`, abortando o descarte na réplica remota e causando corrupção da propriedade do item e da sincronização física do objeto no mundo.

---

## 2. Arquitetura da Solução

Interceptar a chamada de `Player.TryRemoveFromHands` via patch Harmony Prefix:
- **Classe:** `ObservedPlayer_DropBackpackSafety_Patch : ModulePatch`
- **Método Alvo:** `Player.TryRemoveFromHands(Item item, GInterface438 abstractOperation, Callback callback)`
- **Condição:**
  Se a entidade for uma instância de `ObservedPlayer` (jogador remoto) e o item que está saindo **não** for o item empunhado nas mãos (`HandsController.Item != item`), significa que se trata do descarte de um slot animado (mochila) já validado no cliente de origem.
- **Ação:**
  Completar o callback com sucesso imediatamente (`callback?.Succeed()`) e retornar `false` para ignorar a validação de mãos locais no proxy remoto.
- **Fallback:**
  Se for o jogador local ou se o item for a própria arma empunhada, o fluxo original do EFT é mantido intacto (`return true;`).

---

## 3. Localização do Código

| Ação | Arquivo | Descrição |
|---|---|---|
| [NEW] | `Fika.Core/Main/Patches/PlayerPatches/ObservedPlayer_DropBackpackSafety_Patch.cs` | Patch Harmony que intercepta `Player.TryRemoveFromHands` para `ObservedPlayer`. |
| [MODIFY] | `Fika.Core/FikaPlugin.cs` | Bump de versão SemVer de `2.3.10` para `2.3.11`. |
| [MODIFY] | `mods/FIKA/mod.json` | Bump de versão SemVer de `2.3.10` para `2.3.11`. |
| [SYNC] | `mods/FIKA/modded/Fika-Headless/References/Fika.Core.dll` | Cópia do binário compilado de `Fika.Core.dll` para a pasta de referências do Headless. |

---

## 4. Plano de Verificação

1. **Compilação:**
   - Compilar `Fika.Core` em modo Release (`dotnet build -c Release`).
   - Compilar `Fika.Headless` em modo Release garantindo 0 erros.
2. **Teste em Raid:**
   - Iniciar sessão Headless.
   - Conectar cliente de teste.
   - Pressionar "ZZ" para soltar a mochila.
   - Confirmar ausência da mensagem de erro no console do Headless.
   - Recolher a mochila do chão e validar persistência de inventário.
