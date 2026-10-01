---
title: "001 — Sincronização FIKA e Estabilidade de Mãos · Code Review 01"
date: 2026-09-05
status: 🟢 Vivo
authors: Antigravity
---

# 001 — Sincronização FIKA e Estabilidade de Mãos · Code Review 01

**Mod:** HandsAreNotBusy  
**Spec funcional:** *(item legado — sem spec funcional formal; base: `docs/relatorio-auditoria-codigo-01.md`)*  
**Spec técnica:** *(item legado — sem spec técnica formal; base: auditoria AUD-01-01..10)*  
**Asbuild:** *(sem 05-asbuild.md; fallback §4 da auditoria aplicado — ≥ 50% arquivos presentes)*  
**Data:** 2026-09-05  

> Análise crítica do código implementado em v1.7.1 (correções das falhas AUD-01-01..10 da auditoria anterior). Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.

> **Aviso de pré-condição (item legado):** `001-fika-sync-and-stability` não possui pasta de backlog formal nem `05-asbuild.md`. Condição (b) aplicada — arquivos `HANB_Component.cs`, `HANB_FikaSync.cs`, `HANB_Patch.cs`, `HANB_Plugin.cs` presentes e modificados (100% dos arquivos citados na auditoria). Pré-condição satisfeita. Recomendado gerar `asbuild` na próxima rodada.

---

## Resumo

> 🔴 Bloqueadores: 1 · 🟠 Fortes: 2 · 🟡 Médios: 1 · 🟢 Menores: 3 · ✅ Resolvidos: 0 · Total: 7

**Memória consultada:** sem `sessions.md` formal — base de contexto: `docs/relatorio-auditoria-codigo-01.md` (auditoria v1.7.0) e sessão truncada do checkpoint.  
**Pendências que afetam esta review:** AUD-01-05 (desinscrição de callbacks genéricos) e AUD-01-08 (centralização do Update) estavam **pendentes** (sem decisão na auditoria) — revalidados como CR-01-03 e CR-01-04.

---

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | A — Crítico | 🔴 Bloqueador | Dupla destruição de `handsController`: `Destroy()` após `SpawnController` destrói controller já trocado | `[ ]` Pendente |
| CR-01-02 | B — Bug latente | 🟠 Forte | Pacote FIKA não é desregistrado em `OnNetworkManagerDestroyed` — handler órfão entre raids | `[ ]` Pendente |
| CR-01-03 | B — Bug latente | 🟠 Forte | Desinscrição de callbacks de movimento restrita a `FirearmController` (AUD-01-05 pendente) | `[ ]` Pendente |
| CR-01-04 | D — Arquitetura | 🟡 Médio | `HANB_Component.Update()` polling a cada frame — otimização arquitetural pendente (AUD-01-08) | `[ ]` Pendente |
| CR-01-05 | B — Bug latente | 🟢 Menor | `OnClearInventoryPacketReceived` — thread safety confirmada via FIKA `PollEvents` no Unity Update | `[ ]` Pendente |
| CR-01-06 | E — Legibilidade | 🟢 Menor | `GEventArgs1` e `List_0` sem comentário de conceito — ofuscação sem alias nomeado | `[ ]` Pendente |
| CR-01-07 | E — Legibilidade | 🟢 Menor | Mensagem de log de erro no `else` do `InventoryController` menciona campo inexistente | `[ ]` Pendente |

---

## Categorias

- **A — Crítico** — bug grave, crash garantido, corrupção de estado, security issue.
- **B — Bug latente** — comportamento errado em cenário plausível, não acionado pelo caminho golden.
- **C — Gap vs. spec** — código não implementa critério de aceite, corner case, ou AC da spec.
- **D — Arquitetura** — viola padrões do repo, duplica código, leak de estado, abuso de reflection.
- **E — Legibilidade/manutenção** — nomes ruins, comentário "porquê" ausente, código morto, complexidade desnecessária.
- **F — Melhoria opcional** — refactor de qualidade, micro-otimização, simplificação.

## Impacto

- 🔴 **Bloqueador** — fix obrigatório antes de fechar o item.
- 🟠 **Forte** — fix recomendado; pode ser deferido para `06-fix-NN.md` futuro.
- 🟡 **Médio** — anotar, decidir caso a caso.
- 🟢 **Menor** — opcional.

---

## Pontos

### ✅ CR-01-01 · A — Crítico · 🔴 Bloqueador — Aplicado em 2026-09-05

**Dupla destruição de `handsController` com ordem de operações incorreta**

**Local:** [`mods/HandsAreNotBusy/modded/HANB_Component.cs:L72–L116`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/HandsAreNotBusy/modded/HANB_Component.cs#L72)

**Problema:**

```csharp
// L72 — captura referência ao controller ANTIGO
AbstractHandsController handsController = player.HandsController;

// L83–84 — SpawnController substitui player.HandsController internamente pelo novo EmptyHandsController
var emptyController = Player.EmptyHandsController.smethod_6<Player.EmptyHandsController>(player);
player.SpawnController(emptyController);

// L91–100 — TrySetLastEquippedWeapon / SetFirstAvailableItem pode spawnar um TERCEIRO controller

// L102 — SetInventoryOpened ocorre APÓS SpawnController (ordem errada)
player.SetInventoryOpened(false);

// L103 — Destroy() interno EFT no controller antigo (correto)
handsController?.Destroy();

// L105–108 — ❌ segundo Destroy Unity no mesmo objeto já teardown
if (handsController != null)
{
    Destroy(handsController);
}
```

Dois problemas distintos:

1. **Ordem errada:** `SetInventoryOpened(false)` (L102) ocorre **depois** de `SpawnController` (L84) e depois que `TrySetLastEquippedWeapon` (L94) pode ter spawnado outro controller. O inventário deveria ser fechado **antes** de qualquer troca de controller.
2. **Dupla destruição:** `handsController?.Destroy()` (L103) é o teardown canônico EFT — correto. A chamada subsequente `Destroy(handsController)` (L107) é `UnityEngine.Object.Destroy` — desnecessária e potencialmente perigosa se o `AbstractHandsController` tiver lifecycle Unity gerenciado pelo EFT. Chamar `Destroy()` Unity em um objeto cujo teardown EFT já foi feito pode causar `MissingReferenceException` em callbacks Unity pendentes.

**Por que importa:** O fluxo atual não é determinístico: `SpawnController → EquipWeapon → CloseInventory → DestroyOld`. Em casos de controller travado (o bug exato que o mod resolve), o `SpawnController` pode falhar silenciosamente (catch L86–89) mas `TrySetLastEquippedWeapon` ainda é chamado — tentando equipar sobre um controller em estado inválido.

**Sugestão — ordem correta:**

```csharp
private void FixHandsController(Player player)
{
    InventoryController inventoryController = player.InventoryController;
    if (inventoryController == null)
    {
        HANB_Plugin.HANB_Logger.LogError("[HANB] FixHandsController: player.InventoryController retornou null.");
        return;
    }

    // 1. Fechar inventário ANTES de qualquer troca de controller
    player.SetInventoryOpened(false);

    // 2. Sincronizar com Host FIKA
    HANB_FikaSync.SendResetRequestToServer(player);

    // 3. Drenar fila de operações travadas
    int length = inventoryController.List_0.Count;
    if (length > 0)
    {
        GEventArgs1[] args = new GEventArgs1[length];
        inventoryController.List_0.CopyTo(args);
        foreach (GEventArgs1 queuedEvent in args)
            inventoryController.RemoveActiveEvent(queuedEvent);
        HANB_Plugin.HANB_Logger.LogInfo($"[HANB] Cleared {length} stuck inventory operations.");
    }

    // 4. Capturar e fazer teardown do controller atual
    AbstractHandsController handsController = player.HandsController;
    if (handsController is FirearmController currentFirearmController)
    {
        player.MovementContext.OnStateChanged -= currentFirearmController.method_17;
        player.Physical.OnSprintStateChangedEvent -= currentFirearmController.method_16;
        currentFirearmController.RemoveBallisticCalculator();
    }
    handsController?.Destroy(); // teardown canônico EFT — NÃO chamar Destroy() Unity separadamente

    // 5. Spawnar controller vazio
    try
    {
        var emptyController = Player.EmptyHandsController.smethod_6<Player.EmptyHandsController>(player);
        player.SpawnController(emptyController);
    }
    catch (Exception ex)
    {
        HANB_Plugin.HANB_Logger.LogWarning($"[HANB] Exceção contida ao spawnar controlador vazio: {ex}");
    }

    // 6. Reequipar
    player.ProcessStatus = EProcessStatus.None;
    if (player.LastEquippedWeaponOrKnifeItem != null)
        player.TrySetLastEquippedWeapon();
    else
        player.SetFirstAvailableItem((result) => { });

    // 7. Sincronizar animação de arma (só se o controller final for de arma de fogo)
    if (player.HandsController is FirearmController firearmController && firearmController.Weapon != null)
    {
        Traverse.Create(player.ProceduralWeaponAnimation)
            .Field("_firearmAnimationData")
            .SetValue(firearmController);
    }
}
```

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-02 · B — Bug latente · 🟠 Forte

**Pacote FIKA não é desregistrado em `OnNetworkManagerDestroyed` — handler potencialmente duplicado**

**Local:** [`mods/HandsAreNotBusy/modded/HANB_FikaSync.cs:L61–L65`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/HandsAreNotBusy/modded/HANB_FikaSync.cs#L61)

**Problema:**

```csharp
private static void OnNetworkManagerDestroyed(FikaNetworkManagerDestroyedEvent ev)
{
    _lastRegisteredNetworkManager = null;  // ✅ referência zerada (AUD-01-09 resolvido)
    _logger?.LogInfo("[HANB] Sessão FIKA encerrada; referências de rede limpas.");
    // ❌ falta: desregistrar HanbClearInventoryPacket do NetPacketProcessor
}
```

A referência estática é zerada corretamente, mas o `HanbClearInventoryPacket` permanece **registrado no `NetPacketProcessor`** do manager que está sendo destruído. Na próxima sessão, `EnsurePacketsRegistered` vai re-registrar (pois `_lastRegisteredNetworkManager == null != currentManager`). Se o FIKA reutilizar o mesmo `NetPacketProcessor` entre sessões (ou criar um novo manager que compartilha o processador), o handler pode ser registrado duas vezes.

A API existe: [`IFikaNetworkManager.cs:L183`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/modded/Fika-Plugin/Fika.Core/Networking/IFikaNetworkManager.cs#L183) — `void UnregisterPacket<T>() where T : INetSerializable;`

**Por que importa:** Handler duplo = `OnClearInventoryPacketReceived` executado duas vezes por pacote, limpando o inventário do jogador duas vezes — inócuo na maioria dos casos, mas pode interferir em operações de inventário em andamento no segundo pass.

**Sugestão:**

```diff
 private static void OnNetworkManagerDestroyed(FikaNetworkManagerDestroyedEvent ev)
 {
+    try { _lastRegisteredNetworkManager?.UnregisterPacket<HanbClearInventoryPacket>(); }
+    catch { /* manager pode já estar em teardown */ }
     _lastRegisteredNetworkManager = null;
     _logger?.LogInfo("[HANB] Sessão FIKA encerrada; referências de rede limpas.");
 }
```

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-03 · B — Bug latente · 🟠 Forte

**Desinscrição de callbacks de movimento restrita a `FirearmController` (AUD-01-05 não decidido)**

**Local:** [`mods/HandsAreNotBusy/modded/HANB_Component.cs:L74–L79`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/HandsAreNotBusy/modded/HANB_Component.cs#L74)

**Problema:** AUD-01-05 ficou `[ ] Pendente` na auditoria e não foi aplicado na v1.7.1. O código mantém:

```csharp
if (handsController is FirearmController currentFirearmController)
{
    player.MovementContext.OnStateChanged -= currentFirearmController.method_17;
    player.Physical.OnSprintStateChangedEvent -= currentFirearmController.method_16;
    currentFirearmController.RemoveBallisticCalculator();
}
```

Quando o travamento ocorre com `MedsController` (kit médico), `BaseKnifeController` (faca) ou `BaseGrenadeHandsController` (granada), o bloco inteiro é ignorado. Controllers não-FirearmController também assinam `MovementContext.OnStateChanged` internamente durante seu ciclo de vida.

**Por que importa:** Destruir um `MedsController` via `Destroy()` sem desinscrever seus handlers deixa ponteiros pendentes em `MovementContext`. Na primeira vez que o jogador correr ou mudar de postura após o reset, o Unity tenta invocar um delegate que aponta para um objeto destruído → `MissingReferenceException`.

**Sugestão:** O fix do CR-01-01 já reorganiza o fluxo. Adicionar log de diagnóstico do tipo de controller destruído como primeiro passo; a desinscrição completa para tipos não-FirearmController requer inspeção do Assembly para identificar os nomes dos métodos equivalentes a `method_17`/`method_16` em cada subclasse. Ação mínima aceitável: logar o tipo para facilitar triagem em produção:

```csharp
HANB_Plugin.HANB_Logger.LogInfo($"[HANB] Destruindo {handsController?.GetType().Name} — callbacks de FirearmController desinscritos: {handsController is FirearmController}");
```

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-04 · D — Arquitetura · 🟡 Médio

**`HANB_Component.Update()` polling a cada frame — otimização arquitetural pendente (AUD-01-08)**

**Local:** [`mods/HandsAreNotBusy/modded/HANB_Component.cs:L34–L50`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/HandsAreNotBusy/modded/HANB_Component.cs#L34)

**Problema:** AUD-01-08 ficou `[ ] Pendente`. `HANB_Component` executa polling de tecla a cada frame via MonoBehaviour dinâmico injetado no player. O `HANB_Plugin` (BaseUnityPlugin) já é um MonoBehaviour vivo durante toda a sessão. A guarda `Singleton<GameWorld>.Instance?.MainPlayer` funciona igualmente no `Plugin.Update()`, eliminando a necessidade de patch + componente + lifecycle complexo.

**Por que importa:** A complexidade do HANB_Component (Awake/guards/NullRef) gerou metade dos bugs auditados (AUD-01-02, AUD-01-03, AUD-01-04). É dívida técnica estrutural.

**Sugestão:** Deferir para `06-fix-01.md` dedicado — refatoração significativa, não é bloqueador imediato.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-05 · B — Bug latente · 🟢 Menor

**`OnClearInventoryPacketReceived` — thread safety confirmada via FIKA `PollEvents()` no Unity `Update()`**

**Local:** [`mods/HandsAreNotBusy/modded/HANB_FikaSync.cs:L172–L173`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/HandsAreNotBusy/modded/HANB_FikaSync.cs#L172)

**Evidência (verificada):**

```csharp
// FikaServer.cs:L546–L548 (mods/FIKA/modded/Fika-Plugin/Fika.Core/Networking/FikaServer.cs)
private void Update()
{
    _netServer?.PollEvents();  // ← callbacks de pacotes disparados aqui, na thread Unity
    ...
}

// FikaClient.cs:L312–L314 (mods/FIKA/modded/Fika-Plugin/Fika.Core/Networking/FikaClient.cs)
private void Update()
{
    _netClient?.PollEvents();  // ← idem para o cliente
    ...
}
```

`PollEvents()` é chamado no `Update()` de MonoBehaviours Unity em ambos `FikaServer` e `FikaClient`. Todos os handlers de pacotes registrados via `RegisterPacket` são, portanto, executados **na thread principal do Unity** — sem risco de race condition. O acesso a `targetPlayer.ProcessStatus` e `targetPlayer.SetInventoryOpened(false)` é seguro.

**Por que ainda é menor:** Vale manter o comentário no código documentando esse contrato implícito, para que futuros mantenedores não removam a segurança sem perceber.

**Sugestão:** Adicionar comentário inline em `OnClearInventoryPacketReceived`:

```csharp
// Seguro: FikaServer/FikaClient.Update() chama PollEvents() na thread Unity —
// este callback é sempre executado na thread principal.
targetPlayer.ProcessStatus = Player.EProcessStatus.None;
targetPlayer.SetInventoryOpened(false);
```

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-06 · E — Legibilidade/manutenção · 🟢 Menor

**`GEventArgs1` e `List_0` sem comentário de conceito**

**Local:** [`mods/HandsAreNotBusy/modded/HANB_Component.cs:L63–L68`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/HandsAreNotBusy/modded/HANB_Component.cs#L63)

**Problema:** Nomes ofuscados sem alias de conceito. `GEventArgs1` = evento de operação de inventário enfileirada; `List_0` = fila de operações ativas do `InventoryController`.

**Sugestão:**

```csharp
// GEventArgs1 = evento de operação de inventário enfileirada (EFT obfuscado; conceito: InventoryOperationEvent)
// List_0 = fila interna de operações ativas do InventoryController
GEventArgs1[] args = new GEventArgs1[length];
```

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-07 · E — Legibilidade/manutenção · 🟢 Menor

**Mensagem de log de erro menciona campo `_inventoryController` inexistente no fluxo atual**

**Local:** [`mods/HandsAreNotBusy/modded/HANB_Component.cs:L120`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/HandsAreNotBusy/modded/HANB_Component.cs#L120)

**Problema:**

```csharp
HANB_Plugin.HANB_Logger.LogError("FixHandsController: could not find '_inventoryController' field!");
```

Resquício da versão anterior que usava `Traverse.Field("_inventoryController")`. O acesso atual é via `player.InventoryController` (propriedade pública). Mensagem confusa em diagnóstico de produção.

**Sugestão:**

```diff
-HANB_Plugin.HANB_Logger.LogError("FixHandsController: could not find '_inventoryController' field!");
+HANB_Plugin.HANB_Logger.LogError("[HANB] FixHandsController: player.InventoryController retornou null.");
```

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-05 | Code review 01 criada via `/code-review` |
| 2026-09-05 | Base: auditoria AUD-01-01..10 (v1.7.0) → correções aplicadas em v1.7.1 |
| 2026-09-05 | AUD-01-05 e AUD-01-08 revalidados como CR-01-03 e CR-01-04 (decisão pendente da auditoria) |
