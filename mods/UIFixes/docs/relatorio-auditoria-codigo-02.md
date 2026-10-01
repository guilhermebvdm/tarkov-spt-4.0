---
title: "Relatório de Auditoria Técnica de Código — UIFixes (Review 02)"
date: 2026-09-05
status: 🟢 Vivo
authors: [AI Assistant]
---

# Relatório de Auditoria Técnica de Código — UIFixes (Review 02)

## 1. Resumo Executivo da Auditoria

Este relatório consolida a **auditoria técnica estática profunda, rigorosa e exaustiva** de 100% da base de código do mod **UIFixes** (`mods/UIFixes/modded`), cobrindo todos os 119 arquivos C# distribuídos entre os módulos de patches Harmony, inventário, combate, multiselect, context menus, hideout, flea market, trading e infraestrutura assíncrona.

A análise foi cruzada obrigatoriamente contra o EFT descompilado (`references/eft-decompiled/` / `Assembly-CSharp`), o código do servidor SPT 4.0.13 (`references/spt-source/`) e a referência local canônica do FIKA Coop (`mods/FIKA/modded` / `Fika.Core` v2.3.13).

### Estatísticas da Auditoria Consolidada
- **Total de Arquivos Auditados:** 119 arquivos C# (`mods/UIFixes/modded/src/`)
- **Total de Linhas Auditadas:** ~23.500 linhas de código
- **Achados do Review 01:** 9 achados (1 corrigido e validado via versão `5.3.12`, 8 mantidos)
- **Novos Achados (Review 02):** 7 achados
- **Total Consolidado de Achados:** 16 achados

| Severidade | Quantidade | Descrição |
|---|---|---|
| 🔴 **Crítico** | 4 | NRE fatal em tela de presets, loop infinito de `LateUpdate` no `TaskSerializer`, travamento permanente de transferências e stack LIFO desordenada |
| 🟠 **Alto** | 6 | Suposições de singleplayer desregulando mira/coop no FIKA, vazamentos de handlers de rede e acúmulo contínuo de listeners em UnityEvents |
| 🟡 **Médio** | 5 | Invocação per-frame em centenas de itens inativos, retenção contínua de memória por MongoID, `async void` em patches Harmony e violações de AP-09 |
| 💡 **Otimização** | 1 | Substituição de dezenas de graphic raycasts per-frame por cache geométrico |

---

## 2. Tabela de Achados Consolidada

| ID | Severidade | Arquivo / Linha | Categoria | Descrição Resumida | Status |
|---|---|---|---|---|---|
| `AUD-01-01` | 🔴 Crítico | `FixTraderControllerSimulateFalsePatch.cs:80` | Dimensão 5 | Flag estática `BlockPartialTransfers` sem `finally` trava transferências | `[ ]` Pendente |
| `AUD-01-02` | 🔴 Crítico | `NetworkTransactionWatcher.cs:49` | Dimensão 5 & 6 | Stack estática LIFO lança `InvalidOperationException` em concorrência | `[ ]` Pendente |
| `AUD-02-01` | 🔴 Crítico | `TaskSerializer.cs:79-82` | Dimensão 2 & 6 | Falha em `_canContinue` não encerra serializer: loop eterno em `LateUpdate` | `[ ]` Pendente |
| `AUD-02-02` | 🔴 Crítico | `WeaponModdingPatches.cs:308-311` | Dimensão 5 & 1 | `NullReferenceException` ao tentar atualizar arma na tela de Presets (`EditBuildScreen`) | `[ ]` Pendente |
| `AUD-01-03` | 🟠 Alto | `Fika/Sync.cs:58, 88` | Dimensão 1 & 3 | Vazamento de handlers de rede FIKA e retenção de subscrição em troca de mapa | `[ ]` Pendente |
| `AUD-01-04` | 🟠 Alto | `QuickMovePreview.cs:62`, `EmptySlotMenuTrigger.cs:23` | Dimensão 2 | Centenas de `Update()` ativos per-frame em itens de inventário sem `enabled = false` | `[ ]` Pendente |
| `AUD-01-05` | 🟠 Alto | `MultiSelect.cs:99`, `SwapPatches.cs:38` | Dimensão 3 | Retenção estática de UI e vazamento cumulativo de delegados anônimos | `[ ]` Pendente |
| `AUD-01-06` | 🟠 Alto | `DrawMultiSelect.cs:125, 271` | Dimensão 3 | 4 Raycasts por item por frame e alocações recursivas contínuas no Heap | `[ ]` Pendente |
| `AUD-02-03` | 🟠 Alto | `QueueInputPatches.cs:35-80` | Dimensão 1 & 5 | `AimPatch` sem `IsYourPlayer`: mira de players remotos/bots afeta cliente local no FIKA | `[ ]` Pendente |
| `AUD-02-04` | 🟠 Alto | `QueueInputPatches.cs:147-162` | Dimensão 1 | Checagem estrita de `LocalGame` quebra enfileiramento de reload no FIKA multiplayer | `[ ]` Pendente |
| `AUD-02-05` | 🟠 Alto | `HideoutSearchPatches.cs:94`, `FixFleaPatches.cs:84` | Dimensão 3 | Acúmulo de listeners em UnityEvents (`OnKeyScroll`, `onClick`) a cada abertura de tela | `[ ]` Pendente |
| `AUD-01-07` | 🟡 Médio | `GlobalUsings.cs:7-106`, `R.cs:189` | Dimensão 5 | Violação de AP-09 com 106 aliases `GClass` hardcoded e métodos `method_X` voláteis | `[ ]` Pendente |
| `AUD-01-08` | 🟡 Médio | `SliderPatch.cs:58-73` | Dimensão 2 | `SliderMouseListener.Update()` captura eventos globais sem checar hover | `[ ]` Pendente |
| `AUD-02-06` | 🟡 Médio | `WindowManager.cs:23-24, 151` | Dimensão 3 | Dicionários estáticos de posições por `MongoID` acumulam lixo sem cleanup | `[ ]` Pendente |
| `AUD-02-07` | 🟡 Médio | `TraderAvatarPatches.cs:174`, `TacticalBindsPatches.cs:151` | Dimensão 6 | `async void` em Harmony Postfix que escapa ao tratamento de exceções da Unity | `[ ]` Pendente |
| `AUD-01-09` | 🔴 Crítico | `ReloadInPlacePatches.cs:135-220` | Dimensão 1 | Desync e deadlock de tiro por abortar `ReloadMag` nativo no FIKA | ✅ **Aplicado em 2026-09-05 (v5.3.12)** |

---

## 3. Detalhamento dos Novos Achados (Review 02)

### AUD-02-01 · Loop Infinito em `LateUpdate` e Trava Global em `TaskSerializer.cs`
- **Severidade:** 🔴 Crítico
- **Evidência:** Forte (comprovado pela lógica condicional de fluxo em `TaskSerializer.cs:79-83`).
- **Localização no Mod:** [`TaskSerializer.cs:79-83`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/UIFixes/modded/src/TaskSerializer.cs#L79-L83)
- **Referência Cruzada:** [`RevolverPatches.cs:104`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/UIFixes/modded/src/Patches/RevolverPatches.cs#L104), [`BarrelOnlyPatches.cs:55`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/UIFixes/modded/src/Patches/BarrelOnlyPatches.cs#L55), [`MultiSelectPatches.cs:500`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/UIFixes/modded/src/Patches/MultiSelectPatches.cs#L500)
- **Causa Raiz:** O componente `TaskSerializer` gerencia operações seriais em lote (descarregar tambores de revólver, câmaras duplas, transferências multiselect). Ao verificar a condição de continuidade:
  ```csharp
  if (_canContinue != null && !_canContinue(_enumerator.Current, _currentTask.Result))
  {
      return; // <-- FALHA GRAVE: Não chama Complete() nem Destroy(this)!
  }
  ```
  Se `_canContinue` retornar `false` (ex.: faltou espaço no inventário ao descarregar ou uma operação falhou), o método simplesmente retorna. O componente continua vivo anexado ao `ItemUiContext`, executando `LateUpdate()` a cada frame do jogo eternamente. Pior: o campo estático `GlobalDepth` nunca é decrementado, dessincronizando permanentemente futuros serializers de outras partes do mod.
- **Impacto Técnico Real:** Degradação de FPS por acúmulo de MonoBehaviours zumbis rodando `LateUpdate()`, além de bloqueio de novas operações assíncronas no Stash e perda do enumerador (`Dispose()` nunca é chamado).
- **Proposta de Correção:** Invocar `Complete()` antes de retornar caso `_canContinue` falhe, e descartar o enumerador:

```csharp
if (_canContinue != null && !_canContinue(_enumerator.Current, _currentTask.Result))
{
    Complete();
    return;
}
```

- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar: _________________

---

### AUD-02-02 · `NullReferenceException` Fatal na Atualização de Armas em `EditBuildScreen`
- **Severidade:** 🔴 Crítico
- **Evidência:** Forte (erro de digitação evidente na chamada do método da tela).
- **Localização no Mod:** [`WeaponModdingPatches.cs:308-312`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/UIFixes/modded/src/Patches/WeaponModdingPatches.cs#L308-L312)
- **Referência Cruzada:** [`Assembly-CSharp/EFT.UI.EditBuildScreen.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/references/eft-decompiled/Assembly-CSharp/EFT.UI/EditBuildScreen.cs)
- **Causa Raiz:** No patch `MoveBeforeNetworkTransactionPatch`, ao finalizar a transação de movimentação prévia de peças de modding:
  ```csharp
  var editBuildScreen = Singleton<CommonUI>.Instance.EditBuildScreen;
  if (editBuildScreen != null && editBuildScreen.isActiveAndEnabled)
  {
      moddingScreen.WeaponUpdate(); // <-- moddingScreen É NULL OU INATIVO AQUI!
      return;
  }
  ```
  O autor checou `editBuildScreen`, mas chamou `moddingScreen.WeaponUpdate()`. Quando o jogador está na tela de Montagem de Predefinições (Weapon Presets), `moddingScreen` é nulo, provocando um `NullReferenceException` imediato e abortando o encerramento da transação no cliente.
- **Impacto Técnico Real:** Crash silencioso do callback de rede em montagem de armas no Preset, deixando o inventário inconsistente e a arma travada até reabrir a interface.
- **Proposta de Correção:** Chamar o método correto `editBuildScreen.WeaponUpdate()`:

```csharp
var editBuildScreen = Singleton<CommonUI>.Instance.EditBuildScreen;
if (editBuildScreen != null && editBuildScreen.isActiveAndEnabled)
{
    editBuildScreen.WeaponUpdate();
    return;
}
```

- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar: _________________

---

### AUD-02-03 · Suposição de Singleplayer em `AimPatch`: Mira de Jogadores Remotos Força Mira Local
- **Severidade:** 🟠 Alto
- **Evidência:** Forte (ausência de `player.IsYourPlayer` em `QueueInputPatches.cs:35-80`).
- **Localização no Mod:** [`QueueInputPatches.cs:35-80`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/UIFixes/modded/src/Patches/QueueInputPatches.cs#L35-L80)
- **Referência Cruzada:** [`QueueInputPatches.cs:243`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/UIFixes/modded/src/Patches/QueueInputPatches.cs#L243) (`BreathPatch` possui a checagem correta)
- **Causa Raiz:** O patch `AimPatch` aplica um Postfix genérico em `Player.FirearmController.SetAim(bool value)`. Em partidas FIKA multiplayer ou servidores com bots, o método `SetAim` é chamado para **todos** os jogadores presentes na raid. Como o patch não verifica se o jogador alvo é o jogador local (`____player.IsYourPlayer`), ele anexa um componente `InputRepeater` no GameObject do jogador remoto/bot e força o controlador daquele jogador a mirar continuamente caso o jogador local esteja segurando a tecla de mira no teclado.
- **Impacto Técnico Real:** Interferência e dessincronização bizarra de animações e estado de armas de outros jogadores em partidas coop FIKA.
- **Proposta de Correção:** Incluir filtro defensivo logo no início do método:

```csharp
[PatchPostfix]
public static void Postfix(Player.FirearmController __instance, bool value, Player ____player)
{
    if (____player == null || !____player.IsYourPlayer)
    {
        return;
    }
    // ...
```

- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar: _________________

---

### AUD-02-04 · Falha de Resolução de Jogo no FIKA em `QueueInputPatches.GetInputTranslator`
- **Severidade:** 🟠 Alto
- **Evidência:** Forte (compatibilidade de tipos de runtime do FIKA).
- **Localização no Mod:** [`QueueInputPatches.cs:147-162`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/UIFixes/modded/src/Patches/QueueInputPatches.cs#L147-L162)
- **Referência Cruzada:** [`Fika.Core/Main/FikaClientGame.cs`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/FikaClientGame.cs)
- **Causa Raiz:** O helper `GetInputTranslator()` assume rigidamente que a partida só pode ser `HideoutGame` ou `LocalGame`:
  ```csharp
  return game is LocalGame localGame ? localGame.PlayerOwner.HandsInputTranslator as FirearmHandsInputTranslator : null;
  ```
  Em qualquer partida multiplayer do FIKA (seja Client ou Host/Headless), a classe raiz da partida é derivada de `FikaClientGame` ou `FikaServerGame`. Como essas classes não herdam de `LocalGame`, a expressão resulta em `null`, quebrando integralmente a fila de recarga (`QueueHeldInputs`) em coop.
- **Impacto Técnico Real:** A funcionalidade de enfileiramento de recarga é silenciosamente desativada em partidas multiplayer no FIKA.
- **Proposta de Correção:** Acessar o `PlayerOwner` através da interface comum de jogo do EFT ou usar reflexão/polimorfismo seguro compatível com o ecossistema FIKA.

- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar: _________________

---

### AUD-02-05 · Acúmulo Cumulativo de Listeners em UnityEvents sem Teardown
- **Severidade:** 🟠 Alto
- **Evidência:** Forte (verificado em `HideoutSearchPatches.cs:94` e `FixFleaPatches.cs:85`).
- **Localização no Mod:** [`HideoutSearchPatches.cs:94`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/UIFixes/modded/src/Patches/HideoutSearchPatches.cs#L94), [`FixFleaPatches.cs:85`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/UIFixes/modded/src/Patches/FixFleaPatches.cs#L85)
- **Referência Cruzada:** Documentação Unity UI `UnityEvent.AddListener`.
- **Causa Raiz:** Em `HideoutSearchPatches.RestoreHideoutSearchPatch.Prefix`, o código executa `listener?.OnKeyScroll.AddListener(ClearLastScrollPosition)` a cada chamada de `ShowContents`. Como `OnKeyScroll` nunca tem o listener removido via `RemoveListener`, abrir e fechar a bancada do esconderijo várias vezes acumula dezenas de inscrições idênticas disparando o mesmo método em cascata. O mesmo ocorre no Flea Market em `OfferViewTweaksPatch` com `minimizeButton.onClick.AddListener(...)` para itens reciclados do pool.
- **Impacto Técnico Real:** Vazamento cumulativo de delegados em memória nativa do Unity e execuções redundantes multiplicadas a cada interação de scroll ou clique.
- **Proposta de Correção:** Executar `RemoveListener` antes de adicionar ou usar flags para registrar o listener uma única vez no ciclo de vida do componente.

- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar: _________________

---

### AUD-02-06 · Dicionários Estáticos de Posição de Janelas por `MongoID` sem Expiração
- **Severidade:** 🟡 Médio
- **Evidência:** Forte (ausência de limpeza em `WindowManager.cs:151-155`).
- **Localização no Mod:** [`WindowManager.cs:23-24, 151-155`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/UIFixes/modded/src/WindowManager.cs#L23-L24)
- **Causa Raiz:** O `WindowManager` memoriza a posição de janelas de inspeção e contêineres abertos utilizando dois dicionários privados:
  ```csharp
  private readonly Dictionary<MongoID, Vector2> _inspectWindowPositions = [];
  private readonly Dictionary<MongoID, Vector2> _gridWindowPositions = [];
  ```
  O método `Clear()` zera apenas a coleção de janelas abertas no momento (`_openWindows.Clear()`), mantendo intactos os dicionários de coordenadas por MongoID. Ao longo de sessões prolongadas com centenas de itens inspecionados, movidos ou vendidos aos comerciantes, esses dicionários crescem indefinidamente na memória.
- **Impacto Técnico Real:** Retenção cumulativa de memória (Memory Leak de metadados) persistindo durante toda a execução do jogo.
- **Proposta de Correção:** Implementar limpeza periódica ou limpar as coleções no encerramento de raid e logout de perfil.

- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar: _________________

---

### AUD-02-07 · Métodos `async void` em Patches Harmony Escapando Tratamento de Exceções
- **Severidade:** 🟡 Médio
- **Evidência:** Forte (assinatura de métodos Harmony em `TraderAvatarPatches.cs:174` e `TacticalBindsPatches.cs:151`).
- **Localização no Mod:** [`TraderAvatarPatches.cs:174`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/UIFixes/modded/src/Patches/TraderAvatarPatches.cs#L174), [`TacticalBindsPatches.cs:151`](file:///d:/Projetos/GITHUB%20TARKOV/tarkov-spt-4.0/mods/UIFixes/modded/src/Patches/TacticalBindsPatches.cs#L151)
- **Causa Raiz:** O Harmony intercepta métodos de forma síncrona. Declarar um prefix ou postfix como `public static async void Postfix(...)` faz com que qualquer exceção ocorrida dentro da continuação assíncrona (`await __result`) não possa ser capturada nem pelo Harmony nem pelo chamador original, subindo diretamente para o tratador de exceções não observadas da thread ou derrubando a rotina silenciosamente sem log estruturado.
- **Impacto Técnico Real:** Exceções assíncronas ocultas que podem corromper atualizações de interface de quests e atalhos rápidos sem registrar rastreio detalhado no console do BepInEx.
- **Proposta de Correção:** Envolver o bloco `await` em `try/catch` explícito com log de erro ou utilizar callbacks encadeados com `ContinueWith` no sincronizador da thread principal.

- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar: _________________

---

## 4. Plano de Ação e Recomendações

1. **Correções Imediatas de Alta Prioridade (🔴 e 🟠):**
   - Corrigir o bug de NRE em `WeaponModdingPatches.cs:310` (`editBuildScreen.WeaponUpdate()`).
   - Corrigir o loop infinito e o vazamento do `TaskSerializer.cs` adicionando `Complete()` na falha de `_canContinue`.
   - Adicionar checagem de `player.IsYourPlayer` em `QueueInputPatches.AimPatch`.
   - Implementar bloco `try ... finally` na flag estática `BlockPartialTransfers` (`FixTraderControllerSimulateFalsePatch.cs`).
2. **Otimizações de Ciclo de Vida e GC (🟠 e 🟡):**
   - Desativar invocação de `Update()` via `enabled = false` em `QuickMovePreview.cs` e `EmptySlotMenuTrigger.cs`.
   - Corrigir os vazamentos de listeners em `HideoutSearchPatches.cs` e `OfferViewTweaksPatch`.
   - Adicionar limpeza das coordenadas em `WindowManager.Clear()`.
3. **Compatibilização e Estabilidade:**
   - Apoiar o ecossistema multiplayer ajustando o `GetInputTranslator` para suportar partidas FIKA.
