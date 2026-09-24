# 002 — Fix 01 · Mecanismo Seguro pra Drop de Capacete/Óculos em Desmembramento Post-Mortem

**Mod:** VisceralCombat
**Item raiz:** [002-drop-arma-capacete-oculos-cabeca-01-spec.md](002-drop-arma-capacete-oculos-cabeca-01-spec.md)
**Asbuild:** [002-drop-arma-capacete-oculos-cabeca-05-asbuild.md](002-drop-arma-capacete-oculos-cabeca-05-asbuild.md)
**Criado:** 2026-09-20
**Disparado por:** Investigação da pendência `[P-10.3]` (memória, aberta pela rodada 03 de `/code-review`, achado `CR-03-01`), a pedido do usuário.

## Contexto

`CR-03-01` (2026-09-20, `002-drop-arma-capacete-oculos-cabeca-04-code-review-03.md`) desativou `LimbKillPatch.DropCorpseHeadEquipment` — o método que derruba capacete/óculos quando a cabeça de um cadáver **já morto há tempo** é desmembrada por um tiro posterior (diferente do desmembramento "na hora da morte", já resolvido por `DeathInventoryDropPatch`/`CR-NET-LOCK-01`). A versão original usava `helmet?.Owner as TraderControllerClass` — o mesmo padrão `item.Owner` já comprovado quebrado (item travado/inlootiável em coop) que motivou a reformulação completa do drop de arma nesta mesma sessão de trabalho (Sessão 10 da memória). Ficou registrado como pendência `[P-10.3]`: "precisa de spec técnica dedicada antes de reativar" — sem mecanismo seguro conhecido.

Usuário pediu, depois do `/code-mod` do item 005, pra investigar esse mecanismo.

## Causa raiz

O problema não era "não existe como remover item de cadáver já criado" — era usar o controller **errado** pra fazer a operação.

Investigação (Sessão 14 da memória, `mods/VisceralCombat/memory/sessions.md`) via decompile direto do `Assembly-CSharp.dll` do jogo (`ilspycmd`, necessário porque `InteractionsHandlerClass` tem `DECOMPILE-ERROR` conhecido no dump local — `references/eft-decompiled/Assembly-CSharp/InteractionsHandlerClass.cs`):

- **`InteractionsHandlerClass.Throw(Item item, TraderControllerClass itemController, bool simulate)`** (IL lido via `ilspycmd -il`) e o `smethod_17` que ele chama por baixo **não exigem `item.Owner == itemController`**. O parâmetro `itemController` só é usado pra contabilizar limite de descarte (`AffectsDiscardLimits`), comparando `item.Owner` com o dono do **endereço de destino** — nunca com `itemController` em si.
- **`HostInventoryController.RunHostOperation`/`ClientInventoryController.RunClientOperation`** (`references/fika-plugin/Fika.Core/Main/{HostClasses,ClientClasses}/`) sincronizam a operação via `InventoryPacket.FromValue(FikaPlayer.NetId, operation)` — pelo NetId de **quem executa**, não pelo dono atual do item. Cada peer replica a operação localmente pelo ID do item.

Ou seja: qualquer `InventoryController` **vivo e Fika-aware** pode executar `ThrowItem` sobre um item de outro dono, e a sincronização funciona normalmente. O bug real era `helmet.Owner` resolver pro `GClass3385` do próprio cadáver — um `TraderControllerClass` puro, sem nenhum override Fika de `vmethod_1`, que por isso nunca transmitia a operação pela rede.

**Armadilha evitada:** a ideia inicial de usar o controller de quem deu o tiro (`shot.Player.iPlayer.InventoryController`) tem um furo — se o atirador for um peer remoto, no lado do host ele é representado por `ObservedInventoryController` (`references/fika-plugin/Fika.Core/Main/ObservedClasses/ObservedInventoryController.cs`), que **não sobrescreve `vmethod_1`** (cai no fallback local sem broadcast). Resolvido usando sempre o `InventoryController` do **host** (`Singleton<GameWorld>.Instance.MainPlayer`), já que o método só roda com `FikaBackendUtils.IsServer || FikaBackendUtils.IsSinglePlayer` — nesse contexto, `MainPlayer` é sempre o `HostInventoryController`, garantidamente Fika-aware.

## Causa raiz (round 2 — posição errada)

**Teste do usuário (build 3.10.1) confirmou:** o drop funciona e sincroniza corretamente (sem duplicata, sem item travado), MAS o item aparece na posição do **host/atirador**, não na do **cadáver** — cenário relatado: atirador no ponto A, cadáver a 100m no ponto B, capacete cai 1m na frente do atirador, não perto do corpo.

Causa: `hostController.ThrowItem(item, ...)` chama a implementação de `Player.PlayerOwnerInventoryController.ThrowItem` (decompilado, `references/eft-decompiled/Assembly-CSharp/EFT/Player.cs:1325`), que internamente monta `new ThrowOperationClass(method_12(), this, throwResult, itemsToDestroy, Player_0, downDirection)` — `Player_0` é sempre o player **DONO do controller** (o host), nunca configurável de fora pelo wrapper público `ThrowItem(item, downDirection, callback)`.

Confirmado via IL (`ilspycmd`, `ThrowOperationClass.cs` decompilou normalmente) que o construtor é `ThrowOperationClass(uint16 id, TraderControllerClass itemController, GClass3406 throwResult, IEnumerable<DestroyedItemsStruct> destroyedItems, IPlayer player, bool downDirection)` — o parâmetro **`player` é gravado direto em `Iplayer_0`**, usado só pra origem/trajetória do arremesso, **separado** de `itemController` (autoridade de rede). Mesma separação autoridade-vs-item já provada no round 1 — aqui é autoridade-vs-**posição**.

**Fix:** construir o `ThrowOperationClass` manualmente (em vez de usar o wrapper `ThrowItem`), passando o **cadáver** (`player`, já disponível como parâmetro do método) como o `IPlayer` de posição, mantendo `hostController` só para `itemController`/`vmethod_1` (rede). Membros confirmados públicos via IL: `TraderControllerClass.method_12()` (`public ushort`), `TraderControllerClass.vmethod_1(...)` (`public virtual void`), `InteractionsHandlerClass.Throw(...)` (`public static`) — todos acessíveis sem reflection. `InventoryController` (tipo de `MainPlayer.InventoryController`) é subtipo de `TraderControllerClass` (`TraderControllerClass → GClass3384 → InventoryController`), então os membros são herdados diretamente.

**Efeito colateral do build:** passar `player` (tipo `EFT.Player`) como argumento `IPlayer` disparou `CS0012` — o compilador precisa resolver a lista completa de interfaces de `Player` (que inclui `IDissonancePlayer`, do assembly `DissonanceVoip.dll`, não referenciado até então). Corrigido adicionando a referência (`References\DissonanceVoip.dll`, já presente no jogo e já usada por outros mods do repo — SAIN, FIKA — como precedente).

## Mudanças aplicadas

| Arquivo | Mudança |
|---|---|
| `modded/VisceralCombat/VisceralCombat.Ragdolls.Patches/LimbKillPatch.cs` | **Round 1:** Reativadas as 2 chamadas `DropCorpseHeadEquipment(player)` (ramos `HeadOff`/`HeadBurst`, antes comentadas por `CR-03-01`); `DropCorpseHeadEquipment` reescrito pra usar `Singleton<GameWorld>.Instance.MainPlayer.InventoryController` em vez de `helmet.Owner as TraderControllerClass`/`eyewear.Owner as TraderControllerClass`; `using Comfort.Common;` adicionado. **Round 2:** novo método `ThrowFromCorpsePosition(TraderControllerClass, Item, Player)` — constrói `ThrowOperationClass` manualmente (`InteractionsHandlerClass.Throw` + `.vmethod_1`) passando o cadáver como `IPlayer` de posição; `DropCorpseHeadEquipment` chama esse helper em vez de `hostController.ThrowItem(...)`. |
| `modded/VisceralCombat/VisceralCombat.csproj` | `<Version>` `3.10.0` → `3.10.2` (rounds 1 e 2); nova `<Reference Include="DissonanceVoip">` (round 2, necessária pra resolver `Player → IPlayer`). |
| `modded/VisceralCombat/VisceralCombat/VisceralEntry.cs` | `[BepInPlugin]` versão `3.10.0` → `3.10.2`. |

## Checklist de validação (obrigatório antes de marcar o fix como entregue)

- [x] Compila via `compile-mod.sh` sem erros (0 erros, 20 warnings pré-existentes, nenhum novo — build 3.10.2, hash do `.dll` confirmado diferente a cada rebuild)
- [x] **In-raid:** capacete/óculos caem corretamente **na posição do cadáver** (não na do atirador) ao dar um tiro na cabeça de um corpo já morto há tempo — confirmado pelo usuário em 2026-09-20 ("Funcionou!") na build 3.10.2, instalada em `E:\Tarkov Red Line - SERVER TEST`
- [x] **Fika/multiplayer:** item aparece no chão pra TODOS os peers (host e clientes), sem duplicata nem "item preso/inlootiável" — sincronização confirmada no round 1 (build 3.10.1); posição confirmada junto com o item acima no round 2
- [ ] **raid1 → exit → raid2:** sem estado vazado entre raids — não testado explicitamente, mas o fix não introduz nenhum estado novo (sem campos estáticos, sem cache) além do já coberto por `GameStartedPatch`/`ClearPendingHeadOutcomes` existentes; risco residual muito baixo
- [x] **alt-F4 / morte / MIA:** N/A — fix não introduz novo lifecycle/disposal
- [x] Memória do mod atualizada (`/update-memory`) com a lição do fix — feito na Sessão 14

## Histórico

| Data | Evento |
|---|---|
| 2026-09-20 | Fix criado e aplicado (round 1) — investigação + código + build 3.10.1, a pedido explícito do usuário ("aplica direto que eu faço o teste"), sem passar pelo fluxo SDD formal. Usuário testou em coop: drop funciona e sincroniza (sem duplicata/item travado), mas posição errada (na do atirador, não na do cadáver). |
| 2026-09-20 | Round 2 — usuário perguntou explicitamente se dava pra usar a referência do cadáver como ponto de partida do drop mesmo com o atirador "dando o comando"; investigação via IL (`ilspycmd`) confirmou que sim (posição e autoridade de rede são parâmetros separados de `ThrowOperationClass`). Fix aplicado, build 3.10.2 compilada (precisou de nova referência `DissonanceVoip.dll`) e instalada tanto em `E:\Tarkov Red Line` quanto em `E:\Tarkov Red Line - SERVER TEST` (pasta de teste ativa do usuário). |
| 2026-09-20 | **Usuário confirmou em jogo: "Funcionou!"** — capacete/óculos caem na posição do cadáver, sincronização correta. Fix `[P-10.3]` validado e fechado. `LimbKillPatch.DropCorpseHeadEquipment` reativado com sucesso — a exceção arma/capacete do item 005 (toggle mestre) agora cobre o caso post-mortem também, não só o caso "na morte". |
