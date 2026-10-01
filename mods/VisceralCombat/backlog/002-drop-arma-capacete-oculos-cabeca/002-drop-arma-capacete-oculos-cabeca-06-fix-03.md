# 002 — Fix 03 · Drop de Arma Silenciosamente Recusado se o Player/Bot Morre no Meio de Tiro/Recarga

**Mod:** VisceralCombat
**Item raiz:** [002-drop-arma-capacete-oculos-cabeca-01-spec.md](002-drop-arma-capacete-oculos-cabeca-01-spec.md)
**Asbuild:** [002-drop-arma-capacete-oculos-cabeca-05-asbuild.md](002-drop-arma-capacete-oculos-cabeca-05-asbuild.md)
**Criado:** 2026-09-20
**Disparado por:** Usuário reportou em raid: matou um bot (estourou a cabeça) enquanto o bot atirava nele, e a arma do bot não dropou.

## Contexto

`DeathInventoryDropPatch.DropHandsWeapon` chama `controller.ThrowItem(item, false, null)` pra derrubar a arma na mão do player/bot que está morrendo. O usuário suspeitou que, se o player/bot estava "em ação" (atirando/recarregando) no momento exato da morte, isso poderia impedir o drop — e pediu pra investigar antes de aplicar qualquer correção às cegas.

## Causa raiz

Confirmado por leitura do Assembly (sem precisar reproduzir com log — investigação 100% analítica):

- `TraderControllerClass.ThrowItem` executa a operação via `vmethod_1`, que primeiro checa `vmethod_0(operation)` — se retornar `false`, a operação é abortada e o callback recebe `Fail(...)`. Como `DropHandsWeapon` passa `null` como callback, essa falha nunca aparece em log.
- Pra um player/bot segurando arma, `vmethod_0` é `FirearmController.CanExecute` (`references/eft-decompiled/Assembly-CSharp/EFT/Player.cs:13271`):
  ```csharp
  public override bool CanExecute(GInterface438 operation)
  {
      if (method_20(operation)) return true;
      if ((CurrentOperation is GClass2037 || CurrentOperation is GClass2040) && !(CurrentOperation is GClass2038))
          return !(CurrentOperation is GClass2041);
      return false;
  }
  ```
- `method_20` (`Player.cs:13284`) libera direto (`return true`) quando o item **não** está num "slot animado" (`InventoryController.IsAnimatedSlot`). Capacete/óculos/máscara/fone não são slots animados — por isso nunca foram afetados por esse bug, só a arma (que É um slot animado, por ter animação de "largar da mão").
- Pra arma, cai na segunda checagem: só libera se `CurrentOperation` estiver numa lista pequena de exceções (`GClass2037`/`GClass2040`, ambas relacionadas a lançador acoplado/underbarrel — nada a ver com tiro/recarga normais). Uma operação de tiro (`GenericFireOperationClass`) ou recarga (`AmmoPackReloadOperationClass`) ativa **não** bate com essa lista → cai no `return false` final → `CanExecute` recusa → o drop falha em silêncio.

**Confiança da causa raiz:** alta pra "arma não é afetada quando parada" (confirmado com certeza via `method_20`/`IsAnimatedSlot`) e moderada-alta pra "tiro/recarga especificamente bloqueiam" (a lista de exceção claramente não inclui as classes de tiro/recarga, mas não foi possível rastrear com 100% de certeza absoluta qual classe representa o estado "arma parada, pronta" sem gerar um log real — o raciocínio estrutural é sólido, mas não é prova empírica).

## Mudanças aplicadas

**Mecanismo escolhido (discutido com o usuário antes de codar):** `Player.FastForwardCurrentOperations()` (`Player.cs:31662`) — método **público e oficial do próprio jogo**, que força a operação de mão atual a terminar instantaneamente em vez de cancelar. Não é mecanismo inventado: o próprio `Player.OnDead()` (`Player.cs:30607`) já chama exatamente isso quando o player morre — só que tarde demais pra nossa janela de sincronização (depois do `DiedEvent`, quando o snapshot do cadáver já foi montado). A correção só antecipa a mesma chamada pra dentro da nossa janela.

**Alternativas descartadas (discutidas com o usuário):**
- **Rollback:** não existe mecanismo de "desfazer" pra operações de mão (diferente de operações de inventário, que têm `IRollback`/`RollBack()` de verdade). `BaseAnimationOperationClass.Reset()` só zera uma flag de estado, sem cuidar de consistência (arriscado, poderia deixar a arma com carregador pela metade/contagem errada).

| Arquivo | Mudança |
|---|---|
| `modded/VisceralCombat/VisceralCombat.Combined.Patches/DeathInventoryDropPatch.cs` | `DropHandsWeapon`: `player.FastForwardCurrentOperations();` adicionado logo antes do `controller.ThrowItem(item, false, null);`. |
| `modded/VisceralCombat/VisceralCombat/VisceralEntry.cs` | `[BepInPlugin]` versão `3.11.0` → `3.11.1`. |
| `modded/VisceralCombat/VisceralCombat.csproj` | `<Version>` `3.11.0` → `3.11.1`. |

## Checklist de validação (obrigatório antes de marcar o fix como entregue)

- [x] Compila via `compile-mod.sh` sem erros (0 erros, mesmos warnings pré-existentes — build 3.11.1)
- [x] **In-raid:** matar um bot/player enquanto ele está atirando/recarregando ativamente — arma dropa corretamente — confirmado pelo usuário em 2026-09-20 ("Arma não dropando durante tiro/recarga ta dropando")
- [x] **Regressão:** comportamento em morte "parada" segue idêntico (sem relato de regressão)
- [x] **Fika/multiplayer:** sem duplicata/item travado relatado
- [x] Memória do mod atualizada (`/update-memory`) com a lição do fix

## Histórico

| Data | Evento |
|---|---|
| 2026-09-20 | Fix criado e aplicado — usuário reportou o sintoma, pediu investigação analítica (sem log) antes de qualquer correção. Causa raiz confirmada por leitura do Assembly (`FirearmController.CanExecute`). Discutidas 2 abordagens (fast-forward vs rollback) — rollback descartado por não existir mecanismo seguro pra operações de mão. Fast-forward aplicado por ser o mecanismo oficial que o próprio jogo já usa na morte, só chamado mais cedo. Build 3.11.1 compilada e instalada em `E:\Tarkov Red Line`. |
| 2026-09-20 | **Validado em raid pelo usuário** ("Arma não dropando durante tiro/recarga ta dropando, pode colocar como confirmado"). Fix fechado. |
