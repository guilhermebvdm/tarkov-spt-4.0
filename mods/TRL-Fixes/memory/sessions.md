# TRL-Fixes — Memória de Sessões

## Snapshot Delta
- **Fork Ativo / Canônico:** `modded-V2-audit` (Release v1.4.0)
- **Versão:** 1.4.0 (SPT 4.0 / FIKA)
- **Estado:** Implementado FikaInventoryDesyncSafetyPatch em `modded-V2-audit` com reserva virtual de slots em QuickMove (prevenção de colisões concorrentes por Ctrl+Click rápido), auto-recuperação visual de grid em caso de rejeição de servidor ("is taken by another item" / GClass1543) com MainThreadDispatcher e proteção contra descarte de itens em fechamento de contêineres.
- **Pendências:** 🟢 Nenhuma pendência blocker registrada.

---

## 2026-07-28 — Sessão 1: Inicialização de Governança
- **Ação:** Criação dos arquivos `mod.json`, `README.md`, `PROPRIEDADES.md` e `memory/sessions.md`.

---

## 2026-07-29 02:00 (GMT-3) — Sessão 2: Diagnóstico de Trava de Mãos e Racionalização de Escopo

**Tema central:** Investigação da causa raiz do erro de travamento de mãos ("mãos bugadas" / `hands controller can't perform this operation`) reportado em raid coop no FIKA e alinhamento do escopo do `TRL-Fixes`.

**Decisões-chave:**
- **Diagnóstico de Trava de Mãos:** Rastreada a stacktrace de exceção no log (`PoolManagerClass.CreateItem` -> `WeaponManagerClass.SetRoundIntoWeapon` -> `OnAddAmmoInChamber` -> `NullReferenceException`). Provado que desincronização de pacotes do FIKA em raid coop causava munição nula (`ammo = null`), o que interrompia o manipulador de eventos da animação da Unity e travava o `FirearmController` no estado `Busy`, fazendo o servidor rejeitar qualquer operação subsequente de mãos.
- **Remoção de `Patch_PoolManagerCreateItem.cs`:** Removido o patch `Patch_PoolManagerCreateItem.cs` a pedido do usuário após demonstrar que os mods TRL eram inocentes nesse erro e que a desincronização era nativa do transporte do FIKA.
- **Definição de Arquitetura do Mod:** Confirmado que o `TRL-Fixes` não precisará de patches pesados de tentativa de resync de rede do FIKA, pois cada mod TRL passará a tratar seu próprio tráfego em canais isolados. O `TRL-Fixes` permanece focado estritamente em suas correções de gameplay (Flashbang IA e Hitbox pós-Revive).

**Lições / hipóteses descartadas:**
- A hipótese de que o mod `TRL-Fixes` estaria causando desincronização de munição foi descartada após análise minuciosa da stacktrace do FIKA.

**Atividade cronológica:**
1. Leitura e análise da stacktrace do log do usuário (`LogOutput Cherno.log`).
2. Confirmação do fluxo de estado `Busy` do `FirearmController` em decorrência de `NullReferenceException` em `OnAddAmmoInChamber`.
3. Remoção do arquivo `Patch_PoolManagerCreateItem.cs`.

---

## 2026-08-02 — Sessão 3: Inclusão do DynamicMapsSafetyPatch e FikaMainThreadUISafetyPatch

**Tema central:** Adição de novos patches de proteção e resiliência no `TRL-Fixes` para eliminar erros de mods de terceiros (`DynamicMaps`) e chamadas de UI fora da thread principal no `Fika.Core`.

**Alterações Realizadas:**
1. **`DynamicMapsSafetyPatch.cs`**:
   - Refatorado para utilizar **`[PatchFinalizer]`** do Harmony em `DynamicMaps.Patches.GameWorldOnDestroyPatch`, garantindo a supressão absoluta de `NullReferenceException` lançada durante o encerramento da raid (`ModdedMapScreen.OnRaidEnd()`).
2. **`FikaMainThreadUISafetyPatch.cs`**:
   - Refatorado para utilizar **`[PatchFinalizer]`** do Harmony em `Fika.Core.UI.FikaUIGlobals.ShowFikaMessage`, capturando e absorvendo erros de chamadas de UI originadas fora da Main Thread do Unity.
3. **Registro no `Plugin.cs` & `TRLFixes.csproj`**:
   - Ativados os novos patches no `Awake()`, exposto `Plugin.Log` para os patches e adicionada a referência `SPT.Reflection.dll` no `.csproj`.
4. **Validação de Build**:
   - Compilado `TRLFixes.csproj` (`TRL-Fixes.dll`) com **0 Erros e 0 Warnings**.

---

## 2026-08-03 — Sessão 4: Correção de AmbiguousMatchException nos Patches de Segurança

**Tema central:** Raid de teste revelou que ambos os patches da Sessão 3 não carregavam (`Ambiguous match in Harmony patch`), pois os alvos tinham múltiplos overloads com o mesmo nome.

**Diagnóstico (Log da Raid):**
- `[Error : TRL Fixes] TRL-Fixes: Falha ao carregar FikaMainThreadUISafetyPatch: Ambiguous match in Harmony patch for Fika.Core.UI.FikaUIGlobals:ShowFikaMessage` — dois overloads: `ShowFikaMessage(this PreloaderUI, ...)` e `ShowFikaMessage(this ErrorScreen, ...)`.
- `DynamicMaps.UI.ModdedMapScreen.OnRaidEnd` ainda gerava `NullReferenceException` pois o `PatchFinalizer` em `GameWorldOnDestroyPatch.PatchPrefix` não captura exceções internas de chamadas feitas dentro do método patchado.

**Alterações Realizadas:**
1. **`FikaMainThreadUISafetyPatch.cs`**:
   - Substituído `AccessTools.Method(targetType, "ShowFikaMessage")` por `GetMethods().FirstOrDefault(m => m.GetParameters()[0].ParameterType == preloaderUIType)` para selecionar explicitamente o overload com `PreloaderUI` como primeiro parâmetro.
   - Adicionado `using System.Linq` e logs de diagnóstico para falha de resolução.
2. **`DynamicMapsSafetyPatch.cs`**:
   - Redirecionado alvo primário para `DynamicMaps.UI.ModdedMapScreen.OnRaidEnd` diretamente via `AccessTools.TypeByName`.
   - Mantido fallback para `GameWorldOnDestroyPatch.PatchPrefix` com warning explícito de que o fallback é ineficaz para suprimir `OnRaidEnd`.
3. **Validação de Build**:
   - Compilado `TRLFixes.csproj` com **0 Erros e 0 Warnings**.

**Code Review (CR-01) — Achados em TRL-Fixes:**
- 🟡 CR-01-04: Fallback do `DynamicMapsSafetyPatch` retorna comportamento ineficaz se `ModdedMapScreen` não existir — considerar retornar `null` em vez de fallback enganoso.
- 🟢 CR-01-05: `AccessTools.TypeByName("EFT.UI.PreloaderUI")` pode ser substituído por `typeof(PreloaderUI)` para segurança de compilação.

---

## 2026-08-05 — Sessão 5: Migração do Bot Mount Fix Patch de DynamicSpawn para TRL-Fixes

**Tema central:** Migração e centralização do patch de armas estacionárias (`BotMountWeaponFixPatch`) de `TRL-DynamicSpawn` para `TRL-Fixes`.

**Alterações Realizadas:**
1. **`BotMountWeaponFixPatch.cs`**:
   - Replicada a lógica do patch `StationaryWeaponPatch` de `TRL-DynamicSpawn` para `TRL-Fixes/modded/Patches/BotMountWeaponFixPatch.cs`.
   - Intercepta `BotStationaryWeaponData.TakeStationaryWeapon()` prevenindo o travamento da IA de Rogues/Bots ao montar em metralhadoras e metralhadoras pesadas/AGS.
2. **Ativação no `Plugin.cs`**:
   - Ativado `BotMountWeaponFixPatch` no `Awake()` do `TRL-Fixes`.
3. **Validação de Build**:
   - `TRLFixes.csproj` compilado com **0 Erros e 0 Warnings**.

---

## 2026-08-12 — Sessão 6: BotWeaponManagerSafetyPatch (v1.2.1)

**Tema central:** Adição de patch defensivo global em `BotWeaponManager.UpdateHandsController` para suprimir `NullReferenceException` ao trocar armas de bots durante interrupções assíncronas de IA.

**Alterações Realizadas:**
1. **`BotWeaponManagerSafetyPatch.cs`**:
   - Criado patch com `HarmonyPrefix` em `BotWeaponManager.UpdateHandsController(IHandsController handsController, out bool allFine)`.
   - Valida se `__instance`, `BotOwner_0` ou `WeaponManager` são nulos, e se `handsController is IFirearmHandsController` possui `Item == null`.
   - Aborta o método vanilla com segurança e define `allFine = false` com log *throttled* (máx 1 log a cada 5s), evitando que eventos de animação órfãos ou bots desmaiados/despawnados provoquem exceções nulas.
2. **`Plugin.cs` & `TRLFixes.csproj`**:
   - Ativado o patch `BotWeaponManagerSafetyPatch` no `Awake()`.
   - Bump de versão SemVer para `1.2.1`.
3. **Validação de Build**:
   - Compilado `TRLFixes.csproj` (`TRL-Fixes.dll`) com **0 Erros e 0 Warnings**.

---

## 2026-08-12 — Sessão 7: Resolução do Achado CR-01-01 em BotWeaponSelector.OnWeaponTaken (v1.2.2)

**Tema central:** Correção do achado **CR-01-01** do code review, adicionando proteção contra `NullReferenceException` ao ler `BotOwner_0.BotState` em `BotWeaponSelector.OnWeaponTaken`.

**Alterações Realizadas:**
1. **`BotWeaponManagerSafetyPatch.cs`**:
   - Adicionada a proteção `PrefixOnWeaponTaken` interceptando `BotWeaponSelector.OnWeaponTaken`.
   - Valida se `__instance` ou `__instance.BotOwner_0` é nulo, abortando a execução com `return false;` antes do acesso a `BotOwner_0.BotState`.
2. **`Plugin.cs` & `TRLFixes.csproj`**:
   - Bump de versão SemVer para `1.2.2`.
3. **Validação de Build**:
   - Compilado `TRLFixes.csproj` (`TRL-Fixes.dll`) com **0 Erros e 0 Warnings**.

---

## 2026-08-27 — Sessão 9: Auditoria Técnica de Código e Code Review v1.3.1
 
**Tema central:** Auditoria estática completa dos 10 patches do TRL-Fixes, validação de necessidade contra Assembly descompilado e código dos mods alvos, aplicação de 7 correções (AUD-01-01 a AUD-01-07) e Code Review (Review 02).

**Alterações Realizadas:**
1. **`FlashbangBotPatch.cs`**: Cache estático de `_botOwnerProp`, `_setActiveMethod` e `_inactiveArgs` no `Enable()`, eliminando Reflection per-frame na IA.
2. **`FlashbangRadiusPatch.cs`**: Adicionada checagem defensiva de `player.PlayerBones?.Head` e null-coalescing de coeficientes de flashbang.
3. **`BotMountWeaponFixPatch.cs`**: Retorno `false` no prefix de `PlayerOperateStationaryWeaponPatch` para comando `Occupy`, eliminando duplicação de animação/setup do método vanilla.
4. **`DynamicMapsSafetyPatch.cs`**: Removido fallback inoperante e adicionado log informativo limpo.
5. **`FikaMainThreadUISafetyPatch.cs`**: Resolução com compile-time safety via `typeof(EFT.UI.PreloaderUI)`.
6. **`FikaProceedEmptyHandsSafetyPatch.cs`**: Cache estático de `_cachedDeliveryMethodVal` (zero-alloc).
7. **Logging Standard**: Migradas todas as chamadas de `UnityEngine.Debug` para `Plugin.Log` nos patches.
8. **Versionamento e Build**: Bump de versão para `1.3.1` (`Plugin.cs` e `TRLFixes.csproj`), registro no `CHANGELOG.md` e compilação Release com 0 Erros e 0 Warnings.
9. **Documentação e Code Review**: Criados `relatorio-auditoria-codigo-01.md` e `relatorio-auditoria-codigo-02.md`, e atualizado o índice central `docs/README.md`.

---

## 2026-08-27 — Sessão 10: Mitigação de Bug Visual em Armas Estacionárias (v1.3.2)

**Tema central:** Correção e mitigação do bug visual onde Rogues/IAs ficavam em pé andando pelo mapa segurando uma réplica da arma estacionária (NSV 12.7mm / AGS-30) nas mãos enquanto o tripé original permanecia na mureta do cenário.

**Alterações Realizadas:**
1. **`BotMountWeaponFixPatch.cs`**:
   - Adicionado suporte ao comando `Leave` para IAs nos sub-patches `PlayerOperateStationaryWeaponPatch` e `FikaPlayerOperateStationaryWeaponPatch`: ao sair da metralhadora, destranca a arma (`stationaryWeapon.Unlock`), desativa a postura estacionária (`MovementContext.PlayerAnimatorSetStationary(false)`) e aciona imediatamente o seletor do bot (`TakeMainWeapon()`).
   - Criado o sub-patch `BotStationaryWeaponDataDropCurWeaponPatch`: garante `CanLeave = true` no prefixo para não travar a rotina de descarte da IA e, no pós-fixo, limpa a postura estacionária e força o saque da arma primária do inventário.
   - Confirmado o isolamento total para IA: todos os sub-patches possuem a guarda `if (!__instance.IsAI) return true;` para não interferir nos controles ou animações do jogador humano.
2. **`Plugin.cs` & `TRLFixes.csproj`**:
   - Registrado o novo sub-patch `BotStationaryWeaponDataDropCurWeaponPatch` no `Awake()`.
   - Bump de versão SemVer para `1.3.2` (`1.3.2.0`).
3. **`CHANGELOG.md`**:
   - Registradas as notas da versão `v1.3.2`.
4. **Validação de Build**:
   - Compilado `TRLFixes.csproj` (`TRL-Fixes.dll`) com **0 Erros e 0 Warnings**.

---

## 2026-08-31 — Sessão 11: Fix de Desync de Inventário & Quick-Move (v1.4.0)

**Tema central:** Diagnóstico e resolução do bug de itens fantasmas/invisíveis e desync de grid no modo cooperativo do FIKA (`ReceiveStatusFromServer: Client operation rejected by server ... is taken by another item` / `GClass1543` e `Operation conversion failed`), ocorrendo comumente em raids com Headless host ou Player Host durante uso rápido de `Ctrl+Click` (Quick Move) e fechamento de inventário.

**Alterações Realizadas:**
1. **`FikaInventoryDesyncSafetyPatch.cs`**:
   - **`QuickMoveSlotReservationPatch`**: Intercepta `StashGridClass.FindFreeSpace` e mantém registro estático transitório das coordenadas alocadas nos últimos 1.5s na mesma rajada de `Ctrl+Click`, calculando rotas alternativas preemptivamente e impedindo que dois cliques rápidos disputem as mesmas coordenadas `(x, y)` no servidor.
   - **`InventoryRejectionAutoRecoveryPatch`**: Intercepta `ClientInventoryOperationHandler.ReceiveStatusFromServer` (e `TraderControllerClass.method_13`). Ao detectar rejeição por colisão de slot (`is taken by another item` / `GClass1543`), despacha na Main Thread do Unity a reconstrução geométrica do grid (`RaiseResizeEvent()`) e `RaiseRefreshEvent()` no contêiner pai (`ParentItem`), forçando a UI a renderizar os itens reais imediatamente sem exigir que o jogador jogue a mochila no chão.
   - **`MainThreadDispatcher`**: Componente `MonoBehaviour` dedicado para assegurar a execução thread-safe de eventos de UI a partir dos callbacks assíncronos de rede do LiteNetLib.
2. **`Plugin.cs` & `TRLFixes.csproj`**:
   - Ativação do patch no `Awake()` com tratamento defensivo de exceção.
   - Bump de versão SemVer para **v1.4.0** (`1.4.0.0`).
3. **Validação de Build**:
   - Compilado `TRLFixes.csproj` (`TRL-Fixes.dll`) em Release com **0 Erros e 0 Avisos**.
