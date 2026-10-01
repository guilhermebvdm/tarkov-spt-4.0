---
title: "Relatório de Auditoria Técnica de Código — HandsAreNotBusy (Review 01)"
date: 2026-09-05
status: 🟢 Vivo
authors: Antigravity
---

# Relatório de Auditoria Técnica de Código — HandsAreNotBusy (Review 01)

Este documento apresenta a **auditoria técnica estática profunda, rigorosa e minuciosa** de todo o código-fonte do mod [`HandsAreNotBusy`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/HandsAreNotBusy/modded) (versão `1.7.0`), inspecionando classes, métodos, patches Harmony, estruturas de sincronização de rede FIKA e integração com o assembly descompilado do Escape From Tarkov (`0.16.9`) e com o código-fonte canônico do [`FIKA v2.3.11`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/modded/Fika-Plugin/Fika.Core).

---

## 1. Resumo Executivo da Auditoria

| Severidade | Quantidade | Descrição |
|---|---|---|
| 🔴 **Crítico** | 0 | Crashes iminentes, corrupção de dados ou memory leak descontrolado |
| 🟠 **Alto** | 2 | Acoplamento a método volátil obfuscado do compilador (`method_162`) e NRE por falta de `return` após `Destroy` |
| 🟡 **Médio** | 4 | Cast rígido para `LocalPlayer` (incompatível com `ClientPlayer` no FIKA), duplicação de componentes, desinscrição restrita a armas e retenção de `IFikaNetworkManager` pós-raid |
| 🔵 **Baixo** | 2 | Chamada inócua (`InteractionsHandlerClass.Discard` em modo simulação) e formatação de log de exceção |
| 💡 **Otimização** | 2 | Centralização do listener no `HANB_Plugin.Update()` e resolução robusta de entidades no Host via `CoopHandler.Players` |

---

## 2. Tabela de Achados

| ID | Severidade | Arquivo / Linha | Categoria | Descrição Resumida |
|---|---|---|---|---|
| `AUD-01-01` | 🟠 Alto | [`HANB_Component.cs:L81`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/HandsAreNotBusy/modded/HANB_Component.cs#L81) | Assinatura EFT | Chamada ao método obfuscado `player.method_162()` gerado por compilador em vez da API canônica |
| `AUD-01-02` | 🟠 Alto | [`HANB_Component.cs:L19-L29`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/HandsAreNotBusy/modded/HANB_Component.cs#L19-L29) | Fluxo / NRE | Falta de `return` após `Destroy(this)` em `Awake()`, causando NRE se `_player == null` |
| `AUD-01-03` | 🟡 Médio | [`HANB_Component.cs:L13, L17`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/HandsAreNotBusy/modded/HANB_Component.cs#L13-L17) | Tipagem / Coop | Cast explícito para `LocalPlayer` em vez de `Player`, vulnerável a `InvalidCastException` no FIKA |
| `AUD-01-04` | 🟡 Médio | [`HANB_Patch.cs:L30`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/HandsAreNotBusy/modded/HANB_Patch.cs#L30) | Ciclo de Vida | Adição incondicional de `HANB_Component` em `RegisterPlayer` sem checar se já existe na entidade |
| `AUD-01-05` | 🟡 Médio | [`HANB_Component.cs:L72-L77`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/HandsAreNotBusy/modded/HANB_Component.cs#L72-L77) | Limpeza de Estado | Desinscrição de callbacks de movimento restrita a `FirearmController`, ignorando `MedsController` e consumíveis |
| `AUD-01-06` | 🔵 Baixo | [`HANB_Component.cs:L90`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/HandsAreNotBusy/modded/HANB_Component.cs#L90) | Código Morto | Chamada placebo a `InteractionsHandlerClass.Discard(..., simulate: true)` com retorno descartado |
| `AUD-01-07` | 🔵 Baixo | [`HANB_Component.cs:L85`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/HandsAreNotBusy/modded/HANB_Component.cs#L85) | Tratamento de Erro | Log de exceção referencia `ex.InnerException` de forma parcial sem logar a exceção raiz |
| `AUD-01-08` | 💡 Otimização | [`HANB_Component.cs:L32-L48`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/HandsAreNotBusy/modded/HANB_Component.cs#L32-L48) | Arquitetura | Polling de tecla transferível para `HANB_Plugin.Update()`, eliminando `HANB_Patch` e alocação de MonoBehaviour |
| `AUD-01-09` | 🟡 Médio | [`HANB_FikaSync.cs:L36-L58`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/HandsAreNotBusy/modded/HANB_FikaSync.cs#L36-L58) | Memória FIKA (AP-01) | Retenção estática de `IFikaNetworkManager` no menu pós-raid por falta de inscrição em `FikaNetworkManagerDestroyedEvent` |
| `AUD-01-10` | 💡 Otimização | [`HANB_FikaSync.cs:L118-L126`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/HandsAreNotBusy/modded/HANB_FikaSync.cs#L118-L126) | Rede FIKA | Resolução de jogador remoto no Host via `CoopHandler.Players` em vez de depender apenas de `AllAlivePlayersList` |

---

## 3. Detalhamento dos Achados

### AUD-01-01 · Acoplamento a Método Volátil Gerado por Compilador (`player.method_162`)
- **Severidade:** 🟠 Alto
- **Evidência:** Forte
- **Localização no Mod:** [`HANB_Component.cs:L81`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/HandsAreNotBusy/modded/HANB_Component.cs#L81)
- **Referência Cruzada:** [`references/eft-decompiled/Assembly-CSharp/EFT/Player.cs:L32574-L32577`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L32574-L32577) e [`Player.cs:L2270`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L2270)
- **Causa Raiz:** O mod invoca `player.SpawnController(player.method_162())`. Ao inspecionar o assembly descompilado do EFT 0.16.9, constata-se:
  ```csharp
  [CompilerGenerated]
  public EmptyHandsController method_162()
  {
      return EmptyHandsController.smethod_6<EmptyHandsController>(this);
  }
  ```
  Métodos marcados com `[CompilerGenerated]` mudam de identificador numérico a cada build do EFT (ex.: em versões anteriores era `method_156`).
- **Impacto Técnico Real:** Em qualquer patch menor ou recompilação do EFT/SPT, a chamada a `method_162` resulta em `MissingMethodException`, impedindo o destravamento das mãos.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - Invocar diretamente a factory estática canônica: `EmptyHandsController.smethod_6<EmptyHandsController>(player)`.
  ```csharp
  // Solução canônica estável
  var emptyController = Player.EmptyHandsController.smethod_6<Player.EmptyHandsController>(player);
  player.SpawnController(emptyController);
  ```
- **Decisão:**
  - `[x]` Aceitar sugestão
  - **Resolução:** ✅ Aplicado em 2026-09-05 (v1.7.1)
  - **Aplicação:** `HANB_Component.cs:L79` — uso direto de `Player.EmptyHandsController.smethod_6<Player.EmptyHandsController>(player)`

---

### AUD-01-02 · Falta de Interrupção de Fluxo em `Awake()` após `Destroy(this)`
- **Severidade:** 🟠 Alto
- **Evidência:** Forte
- **Localização no Mod:** [`HANB_Component.cs:L19-L29`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/HandsAreNotBusy/modded/HANB_Component.cs#L19-L29)
- **Causa Raiz:** Em Unity C#, `Destroy(this)` não interrompe a execução sequencial do método. No bloco:
  ```csharp
  if (_player == null)
  {
      HANB_Plugin.HANB_Logger.LogError("Unable to find LocalPlayer, destroying module.");
      Destroy(this);
  }

  if (!_player.IsYourPlayer) // NRE se _player for nulo!
  {
      HANB_Plugin.HANB_Logger.LogError("MainPlayer is not your player, destroying module");
      Destroy(this);
  }
  ```
  Se `_player` for nulo, a linha subsequente acessa `_player.IsYourPlayer` e lança `NullReferenceException`.
- **Impacto Técnico Real:** Se o componente inicializar antes do `MainPlayer` estar completamente pronto, ocorre exceção não tratada no log do Unity, poluindo o console e abortando o ciclo de vida do MonoBehaviour.
- **Proposta de Correção:** Adicionar `return;` após cada cláusula de guarda `Destroy(this);`.
- **Decisão:**
  - `[x]` Aceitar sugestão
  - **Resolução:** ✅ Aplicado em 2026-09-05 (v1.7.1)
  - **Aplicação:** `HANB_Component.cs:L23, L29` — adicionado `return;` após `Destroy(this)`

---

### AUD-01-03 · Cast Rígido para `LocalPlayer` Incompatível com `ClientPlayer` no FIKA Coop
- **Severidade:** 🟡 Médio
- **Evidência:** Forte
- **Localização no Mod:** [`HANB_Component.cs:L13, L17`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/HandsAreNotBusy/modded/HANB_Component.cs#L13-L17)
- **Referência Cruzada (FIKA v2.3.11):** [`mods/FIKA/modded/Fika-Plugin/Fika.Core/Networking/FikaClient.cs:L260`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/modded/Fika-Plugin/Fika.Core/Networking/FikaClient.cs#L260), [`references/eft-decompiled/Assembly-CSharp/EFT/ClientPlayer.cs:L35`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/EFT/ClientPlayer.cs#L35) e [`references/eft-decompiled/Assembly-CSharp/EFT/LocalPlayer.cs:L16`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/EFT/LocalPlayer.cs#L16)
- **Causa Raiz:** O campo está declarado como `private LocalPlayer _player;` e inicializado com `_player = (LocalPlayer)Singleton<GameWorld>.Instance.MainPlayer;`. No modelo de coop do FIKA v2.3.11:
  - O Host roda com `LocalPlayer`.
  - O Convidado no cliente do FIKA frequentemente opera como `ClientPlayer` (derivado de `NetworkPlayer -> Player`).
  - `ClientPlayer` e `LocalPlayer` são classes irmãs e não herdam entre si. O cast explícito `(LocalPlayer)` gera `InvalidCastException`.
- **Impacto Técnico Real:** O componente do HANB falha ao carregar no convidado do FIKA, impedindo o jogador de recuperar as mãos travadas em partidas multiplayer.
- **Proposta de Correção:** Tipar o campo `_player` como `EFT.Player`, já que todos os membros acessados pertencem à classe base `EFT.Player`.
- **Decisão:**
  - `[x]` Aceitar sugestão
  - **Resolução:** ✅ Aplicado em 2026-09-05 (v1.7.1)
  - **Aplicação:** `HANB_Component.cs:L13, L17` — tipagem alterada para `EFT.Player`

---

### AUD-01-04 · Adição Incondicional de `HANB_Component` em `RegisterPlayer`
- **Severidade:** 🟡 Médio
- **Evidência:** Forte
- **Localização no Mod:** [`HANB_Patch.cs:L30`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/HandsAreNotBusy/modded/HANB_Patch.cs#L30)
- **Causa Raiz:** O patch intercepta `GameWorld.RegisterPlayer` e executa:
  ```csharp
  Singleton<GameWorld>.Instance.MainPlayer.gameObject.AddComponent<HANB_Component>();
  ```
  Não existe verificação se o GameObject já possui o componente (`GetComponent<HANB_Component>() == null`). Se o método for disparado mais de uma vez durante o carregamento de raid ou reconexão no coop, múltiplos componentes são adicionados ao mesmo GameObject.
- **Impacto Técnico Real:** Cada instância do componente ouve a mesma tecla `End`. Ao pressionar a tecla, o método `FixHandsController` é chamado 2 ou mais vezes consecutivas no mesmo frame, causando corrida de spawn de controladores e potencial desincronização.
- **Proposta de Correção:**
  ```csharp
  var mainPlayer = Singleton<GameWorld>.Instance?.MainPlayer;
  if (mainPlayer != null && mainPlayer.GetComponent<HANB_Component>() == null)
  {
      mainPlayer.gameObject.AddComponent<HANB_Component>();
  }
  ```
- **Decisão:**
  - `[x]` Aceitar sugestão
  - **Resolução:** ✅ Aplicado em 2026-09-05 (v1.7.1)
  - **Aplicação:** `HANB_Patch.cs:L30-L40` — adicionada guarda `GetComponent<HANB_Component>() == null`

---

### AUD-01-05 · Desinscrição de Callbacks de Movimento Restrita a `FirearmController`
- **Severidade:** 🟡 Médio
- **Evidência:** Forte
- **Localização no Mod:** [`HANB_Component.cs:L72-L77`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/HandsAreNotBusy/modded/HANB_Component.cs#L72-L77)
- **Causa Raiz:** O código apenas desinscreve listeners de movimento caso `handsController is FirearmController`:
  ```csharp
  if (handsController is FirearmController currentFirearmController)
  {
      player.MovementContext.OnStateChanged -= currentFirearmController.method_17;
      player.Physical.OnSprintStateChangedEvent -= currentFirearmController.method_16;
      currentFirearmController.RemoveBallisticCalculator();
  }
  ```
  Contudo, travamentos de mãos ocorrem com frequência durante o consumo de itens médicos (`MedsController`), uso de faca (`BaseKnifeController`) ou granadas (`BaseGrenadeHandsController`).
- **Impacto Técnico Real:** Ao destruir forçadamente controladores que não são armas de fogo, eventos internos do `MovementContext` podem manter referências a instâncias destruídas, provocando `MissingReferenceException` em corridas ou transições de postura subsequentes.
- **Proposta de Correção:** Invocar formalmente o ciclo de encerramento do controller antes do descarte.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar: _________________

---

### AUD-01-06 · Chamada Placebo a `InteractionsHandlerClass.Discard(..., simulate: true)`
- **Severidade:** 🔵 Baixo
- **Evidência:** Forte
- **Localização no Mod:** [`HANB_Component.cs:L90`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/HandsAreNotBusy/modded/HANB_Component.cs#L90)
- **Causa Raiz:**
  ```csharp
  InteractionsHandlerClass.Discard(player.LastEquippedWeaponOrKnifeItem, inventoryController, true);
  ```
  O terceiro parâmetro (`simulate = true`) roda a operação em modo de checagem preliminar (dry-run). O resultado retornado (`GStruct154`) não é verificado nem aplicado. A chamada não altera o estado do inventário nem do item.
- **Impacto Técnico Real:** Código morto que consome ciclos de CPU sem nenhum efeito prático.
- **Proposta de Correção:** Remover a linha inócua.
- **Decisão:**
  - `[x]` Aceitar sugestão
  - **Resolução:** ✅ Aplicado em 2026-09-05 (v1.7.1)
  - **Aplicação:** `HANB_Component.cs:L88` — chamada inócua removida

---

### AUD-01-07 · Formatação do Log de Exceção em `SpawnController`
- **Severidade:** 🔵 Baixo
- **Evidência:** Forte
- **Localização no Mod:** [`HANB_Component.cs:L85`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/HandsAreNotBusy/modded/HANB_Component.cs#L85)
- **Causa Raiz:** O catch loga `"Stopped exception when spawning controller. InnerException: " + ex.InnerException`. Se `ex.InnerException` for nulo, a mensagem não imprime o stack trace nem a mensagem da exceção principal (`ex.Message`).
- **Proposta de Correção:** Logar a exceção completa com interpolação formatada:
  ```csharp
  HANB_Plugin.HANB_Logger.LogWarning($"[HANB] Exceção contida ao spawnar controlador vazio: {ex}");
  ```
- **Decisão:**
  - `[x]` Aceitar sugestão
  - **Resolução:** ✅ Aplicado em 2026-09-05 (v1.7.1)
  - **Aplicação:** `HANB_Component.cs:L84` — interpolação formatada com `$"{ex}"` completo

---

### AUD-01-08 · Centralização do Listener de Teclado no `HANB_Plugin.Update()`
- **Severidade:** 💡 Otimização
- **Evidência:** Forte
- **Localização no Mod:** [`HANB_Component.cs:L32-L48`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/HandsAreNotBusy/modded/HANB_Component.cs#L32-L48) e [`HANB_Patch.cs:L8-L33`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/HandsAreNotBusy/modded/HANB_Patch.cs#L8-L33)
- **Causa Raiz:** A arquitetura atual utiliza um patch Harmony em `GameWorld.RegisterPlayer` exclusivamente para injetar um `MonoBehaviour` dinâmico (`HANB_Component`) no GameObject do jogador, que fica executando `Update()` a cada frame apenas para checar `ResetKey.Value.IsDown()`.
- **Ganhos com a Otimização:**
  1. **Eliminação do Patch Harmony:** Elimina completamente o `HANB_Patch` e o overhead de hook em `GameWorld.RegisterPlayer`.
  2. **Eliminação de MonoBehaviour Dinâmico:** Não há risco de componentes órfãos, vazamento de GameObject ou duplicação de componentes no jogador.
  3. **Simplicidade Arquitetural:** O `HANB_Plugin` (que já é um MonoBehaviour vivo durante toda a sessão) monitora a tecla diretamente no seu próprio `Update()` com uma guarda simples:
     ```csharp
     protected void Update()
     {
         if (ResetKey.Value.IsDown())
         {
             var player = Singleton<GameWorld>.Instance?.MainPlayer;
             if (player != null && player.IsYourPlayer)
             {
                 HANB_Recovery.FixHandsController(player);
             }
         }
     }
     ```
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar: _________________

---

### AUD-01-09 · Retenção de `IFikaNetworkManager` no Menu pós-Raid (AP-01)
- **Severidade:** 🟡 Médio
- **Evidência:** Forte
- **Localização no Mod:** [`HANB_FikaSync.cs:L36-L58`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/HandsAreNotBusy/modded/HANB_FikaSync.cs#L36-L58)
- **Referência Cruzada (FIKA v2.3.11):** [`mods/FIKA/modded/Fika-Plugin/Fika.Core/Modding/Events/FikaNetworkManagerDestroyedEvent.cs:L5-L8`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/modded/Fika-Plugin/Fika.Core/Modding/Events/FikaNetworkManagerDestroyedEvent.cs#L5-L8)
- **Causa Raiz:** O campo estático `_lastRegisteredNetworkManager` retém uma referência forte à instância de `FikaServer` ou `FikaClient`. O mod assina `FikaNetworkManagerCreatedEvent`, mas **não se inscreve em `FikaNetworkManagerDestroyedEvent`**. Ao finalizar a raid e voltar ao menu principal, o manager antigo permanece preso em memória através do campo estático, impedindo que o Garbage Collector colete os buffers de rede do LiteNetLib e pools de pacotes do FIKA.
- **Impacto Técnico Real:** Violação do antipadrão AP-01 (falta de teardown entre raids), retendo dados de sessão no menu principal e acumulando memória raid-a-raid.
- **Proposta de Correção:**
  Inscrever-se no evento `FikaNetworkManagerDestroyedEvent` para resetar `_lastRegisteredNetworkManager = null;`:
  ```csharp
  FikaEventDispatcher.SubscribeEvent<FikaNetworkManagerDestroyedEvent>(ev => {
      _lastRegisteredNetworkManager = null;
  });
  ```
- **Decisão:**
  - `[x]` Aceitar sugestão
  - **Resolução:** ✅ Aplicado em 2026-09-05 (v1.7.1)
  - **Aplicação:** `HANB_FikaSync.cs:L46, L59-L63` — inscrição em `FikaNetworkManagerDestroyedEvent` com limpeza de referências estáticas

---

### AUD-01-10 · Resolução de Jogador Remoto no Host via `CoopHandler.Players`
- **Severidade:** 💡 Otimização
- **Evidência:** Forte
- **Localização no Mod:** [`HANB_FikaSync.cs:L118-L126`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/HandsAreNotBusy/modded/HANB_FikaSync.cs#L118-L126)
- **Referência Cruzada (FIKA v2.3.11):** [`mods/FIKA/modded/Fika-Plugin/Fika.Core/Networking/IFikaNetworkManager.cs:L27`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/modded/Fika-Plugin/Fika.Core/Networking/IFikaNetworkManager.cs#L27) e [`mods/FIKA/modded/Fika-Plugin/Fika.Core/Networking/FikaServer.Callbacks.cs:L468`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/modded/Fika-Plugin/Fika.Core/Networking/FikaServer.Callbacks.cs#L468)
- **Causa Raiz:** O Host tenta localizar o jogador remoto através de:
  ```csharp
  var targetPlayer = gameWorld.AllAlivePlayersList.FirstOrDefault(p => p != null && p.ProfileId == packet.ProfileId);
  ```
  No FIKA v2.3.11, os jogadores remotos são instâncias de `ObservedPlayer` gerenciadas diretamente pelo subsistema de rede em `CoopHandler.Players` e `IFikaNetworkManager.ObservedPlayers`. Se o jogador remoto sofrer desconexão, morte iminente ou respawn durante o travamento, `AllAlivePlayersList` pode falhar na localização do `ProfileId`.
- **Proposta de Correção:**
  No Host, buscar primeiramente em `Singleton<IFikaNetworkManager>.Instance.ObservedPlayers` ou `gameWorld.RegisteredPlayers` (que abrange todos os perfis registrados, mesmo em transição):
  ```csharp
  var targetPlayer = Singleton<IFikaNetworkManager>.Instance?.ObservedPlayers?.FirstOrDefault(p => p != null && p.ProfileId == packet.ProfileId)
                     ?? gameWorld.RegisteredPlayers?.FirstOrDefault(p => p != null && p.ProfileId == packet.ProfileId) as Player;
  ```
- **Decisão:**
  - `[x]` Aceitar sugestão
  - **Resolução:** ✅ Aplicado em 2026-09-05 (v1.7.1)
  - **Aplicação:** `HANB_FikaSync.cs:L128-L144` — resolução em cascata (`ObservedPlayers` -> `AllAlivePlayersList` -> `RegisteredPlayers`)

---

## 4. Plano de Ação e Recomendações

1. **Correção Imediata de Estabilidade (Fase 1):**
   - Substituir `player.method_162()` pela API direta `Player.EmptyHandsController.smethod_6<Player.EmptyHandsController>(player)`.
   - Inserir guardas de `return;` após `Destroy(this)` em `HANB_Component.Awake()`.
   - Alterar tipo de `_player` de `LocalPlayer` para `EFT.Player`.
   - Adicionar handler para `FikaNetworkManagerDestroyedEvent` zerando a referência estática `_lastRegisteredNetworkManager`.
2. **Limpeza e Refatoração Estrutural (Fase 2):**
   - Adicionar checagem defensiva contra duplicação de componentes em `HANB_Patch.cs` (ou migrar a escuta da tecla diretamente para `HANB_Plugin.Update()`, tornando o patch e o componente obsoletos).
   - Utilizar `ObservedPlayers` do FIKA v2.3.11 para busca infalível de jogadores remotos no Host.
   - Remover a chamada placebo `InteractionsHandlerClass.Discard(..., simulate: true)`.
   - Melhorar o tratamento de desinscrição para controladores genéricos além de `FirearmController`.
