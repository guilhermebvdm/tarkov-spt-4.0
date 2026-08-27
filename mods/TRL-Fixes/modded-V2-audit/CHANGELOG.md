# Changelog — TRL-Fixes

Versões mais recentes primeiro.

## v1.3.2 (2026-08-27)

### Mitigação de Bug Visual e Sincronização de Armas Estacionárias
- **`BotMountWeaponFixPatch` / `FikaPlayerOperateStationaryWeaponPatch`**:
  - Adicionado tratamento completo do comando `Leave` para IAs em armas montadas.
  - Criado o sub-patch `BotStationaryWeaponDataDropCurWeaponPatch` para desativar a postura estacionária (`MovementContext.PlayerAnimatorSetStationary(false)`) e forçar o saque da arma primária (`Selector.TakeMainWeapon()`) no exato momento em que o bot larga a metralhadora ou o AGS-30.
  - Elimina o bug visual onde Rogues/IAs ficavam em pé andando pelo mapa segurando uma réplica da arma pesada nas mãos enquanto o tripé continuava na mureta.

---

## v1.3.1 (2026-08-27)

### Otimizações de Performance, Null Safety e Padronização
- **`FlashbangBotPatch` (AUD-01-01)**: Cache estático de `PropertyInfo` e `MethodInfo` do SAIN no `Enable()`, eliminando lookups de Reflection e alocações de argumentos per-frame em `SAINActivationClass.ManualUpdate`.
- **`FlashbangRadiusPatch` (AUD-01-02)**: Null-checks defensivos em `player.PlayerBones.Head` e leitura resiliente de `FileSettings.Grenade.FLASH_GRENADE_TIME_COEF` prevenindo NREs durante explosões de flashbangs.
- **`BotMountWeaponFixPatch` (AUD-01-03)**: Retorno `false` no prefix de `PlayerOperateStationaryWeaponPatch` para comando `Occupy`, eliminando a duplicação desnecessária de setup/animação vanilla no mesmo frame.
- **`DynamicMapsSafetyPatch` (AUD-01-04)**: Remoção de fallback inoperante e log informativo limpo caso o `SPT-DynamicMaps` não esteja instalado.
- **`FikaMainThreadUISafetyPatch` (AUD-01-05)**: Resolução com compile-time type safety via `typeof(PreloaderUI)`.
- **`FikaProceedEmptyHandsSafetyPatch` (AUD-01-06)**: Otimização zero-alloc através do cache estático de `_cachedDeliveryMethodVal` durante a inicialização.
- **Logging Padronizado (AUD-01-07)**: Unificação de todos os patches para `Plugin.Log`, garantindo a tag canônica `[Info : TRL Fixes]` no console BepInEx e no arquivo de log.

---

## v1.3.0 (2026-08-16)

### Correções de Compatibilidade Fika e Sincronização de Inventário
- **`FikaProceedEmptyHandsSafetyPatch`**: Intercepta `FikaServer.OnProceedRequestPacketReceived` para transições de mãos vazias (`EProceedType.EmptyHands`). Resolve a rejeição indevida do servidor ao buscar itens com `MongoID` vazio, eliminando o erro `[HandleCallbackResponse]: Could not execute callback with id XX on the server` que ocorria durante recargas contínuas fora do inventário (ex.: `SPT-ContinuousLoadAmmo` / `LoadAmmoAnim`).
- **`FikaRefreshSlotViewsSafetyPatch`**: Corrige a colisão de chaves em `ObservedPlayer.RefreshSlotViews`. Substitui a indexação vulnerável por dicionário de `slot.FullId` por uma estrutura de pares chave-valor segura, eliminando o log de `CRITICAL ERROR DICTIONARY: mod_tactical` ao sincronizar armas com múltiplos slots/trilhos táticos.

---

## v1.2.4 (2026-08-12)

### Correções de Montagem de Armas e Gerenciador de Armas
- **`BotMountWeaponFixPatch`**: Correções para montagem de bots em armas estacionárias e sincronização no Fika.
- **`BotWeaponManagerSafetyPatch`**: Validações nulas defensivas no gerenciador de armas dos bots.
- **`FikaMainThreadUISafetyPatch`**: Execução segura de mensagens de UI do Fika despachadas para a Main Thread.

---



### Nova correção — trava de controles ao pegar item do chão

- **`PickupAimingSafetyPatch`**: ao pegar ou equipar um item do chão pelo menu de ação nativo (mais comum com
  coletes e rigs), o corpo do personagem podia **congelar** — não anda, não agacha, não vira a visão — enquanto
  o inventário e a troca de arma continuavam respondendo. O patch impede a trava.
- O patch **já existia** neste mod, foi movido para `stancesAndCameraPositionSPT4.0.11` em 2026-07-25 sem
  registro, e voltou para cá: é remendo sobre bug do jogo base, e o mod de stances está sendo preparado para
  publicação pública.
- **Logging forense**: a primeira ocorrência sai no console com a **pilha de chamadas completa**; as seguintes
  saem com throttle de 5 s e contador acumulado. A causa raiz descrita no diagnóstico
  ([`docs/handoff-pickup-aiming-safety.md`](../docs/handoff-pickup-aiming-safety.md)) é coerente com o
  decompilado mas **nunca foi capturada em raid** — esse primeiro registro é o que confirma ou refuta.
- Reescrito no estilo do mod (Harmony direto, sem SPT.Reflection).

### Manutenção

- Versão passa a ser declarada também no `.csproj` (`Version`/`AssemblyVersion`/`FileVersion`). Sem isso a DLL
  saía marcada como `1.0.0.0` independentemente da versão do plugin.

---

## v1.0.0

Versão inicial: `FlashbangBotPatch`, `FlashbangRadiusPatch`, `Patch_PoolManagerCreateItem` e
`FixFikaReviveRagdollPatch`.
