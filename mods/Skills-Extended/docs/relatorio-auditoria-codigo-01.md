---
title: "Relatório de Auditoria Técnica de Código — Skills-Extended (Review 01)"
date: 2026-09-07
status: 🟢 Vivo
authors: Antigravity
---

# Relatório de Auditoria Técnica de Código — Skills-Extended (Review 01)

**Escopo auditado:** `mods/Skills-Extended/modded/` (projetos `Plugin/`, `Server/`, `Common/`, `Prepatcher/`, `FikaSync/`; `__BUILD_RELEASE__/`, `obj/`, `bin/` excluídos).
**Referências cruzadas:** `references/eft-decompiled/Assembly-CSharp/` (EFT 0.16.9), `references/spt-source/` (SPT 4.0), e **`mods/FIKA/modded/Fika-Plugin/Fika.Core/`** — o fork do FIKA mantido e rodado por este repositório, usado como fonte primária em vez do vendor genérico `references/fika-plugin/` para tudo que envolve comportamento real de rede/coop.
**Memória consultada:** `mods/Skills-Extended/memory/sessions.md` não existe — sem pendências prévias a cruzar. Nenhum relatório de auditoria anterior (`relatorio-auditoria-codigo-*.md`) existia antes deste.
**Metodologia:** as 6 dimensões foram delegadas a sub-agentes read-only em paralelo (mod grande — 6 projetos, ~228 arquivos), com Dimensão 3 usando a metodologia da skill `spt-memory-leak-analysis`. Achados duplicados entre dimensões foram mesclados citando ambas as origens.

---

## 1. Resumo Executivo da Auditoria

| Severidade | Quantidade | Descrição |
|---|---|---|
| 🔴 **Crítico** | 6 | Race conditions server-side, NRE latentes que podem travar spawn/raid, patch que ignora completamente o dono real da ação |
| 🟠 **Alto** | 9 | Mutação de templates compartilhados, gating ausente, leaks entre raids, bug de copy-paste, riscos de NRE em UI |
| 🟡 **Médio** | 6 | Bug comportamental remanescente (pós-fix de crash), GC pressure moderada, defensividade inconsistente |
| 🔵 **Baixo** | 5 | Higiene de recursos, drift de assinatura em código dormente, catch vazio isolado |
| 💡 **Otimização** | 2 | Reflection sem cache, reassert desnecessário por frame |
| **Total** | **28** | |

### ⚠️ Padrão sistêmico identificado (lê isto antes dos achados individuais)

Onze dos 28 achados compartilham a **mesma causa raiz**: `GameUtils.GetSkillManager()` / `GetPlayer()` / `GetProfile()` (`Plugin/Utils/GameUtils.cs:76-93`) são **explicitamente `[CanBeNull]`** por contrato do próprio autor, resolvendo sempre `Singleton<GameWorld>.Instance.MainPlayer` — nunca o jogador/entidade que realmente disparou o método patcheado. Isso já causou um `NullReferenceException` real em produção (`MeleeSpeedPatch`, corrigido nesta sessão — v2.2.3), e a mesma classe de bug está latente ou ativa em mais 10 pontos:

| Local | Sintoma | Achado |
|---|---|---|
| `MeleeSpeedPatch.cs:30` | NRE (🟢 já corrigido) + buff aplicado ao golpe de qualquer jogador | AUD-01-16 |
| `MovementContextSetSpeedLimitPatch.cs:21,29` | NRE + speed limit de bots/peers calculado com skill do MainPlayer | AUD-01-02 |
| `DoorSoundPatch.cs:16,24-43` | Som de **toda porta do mapa** alterado pelo Silent Ops do MainPlayer | AUD-01-01 |
| `StimulatorApplyBuffPatch.cs:12-17,28-31` | Cap de FieldMedicine de qualquer injetável usa skill do MainPlayer | AUD-01-03 |
| `HealthEffectComponentPatch.cs:22-70` | Muta `template.DamageEffects[].Cost` **compartilhado** com skill do MainPlayer | AUD-01-07 |
| `UpdateWeaponsPatch.cs:97-107,163-174` | Muta `Weapon.Template.Ergonomics`/`RecoilForce*` **compartilhado** | AUD-01-08 |
| `AbstractSkillClassSummaryLevelPatch.cs:17-38` | Cap de skill exibido usa MainPlayer em vez do dono do `__instance` | AUD-01-10 |
| `GetBarterPricePatch.cs:49,83` | NRE possível em telas de trader/flea em transição | AUD-01-11 |
| `OnGameStarted.cs:97` | `GetPlayer()!` redundante (já existe campo estático validado) | AUD-01-12 |
| `LockPickingGame.cs:30,405,423` | Property finge não ser nula, esconde o `[CanBeNull]` | AUD-01-17 |
| `ConsoleCommands.cs:61,74,87` | Acesso direto a `.Instance.MainPlayer` sem `.Instantiated` | AUD-01-18 |

**Recomendação estrutural única:** em vez de corrigir os 11 pontos individualmente com null-checks pontuais, considerar um item de backlog dedicado que (a) padronize `GameUtils` para nunca usar `!` internamente sem justificativa, (b) para os patches que afetam `Player`/`MovementContext`/objetos por-instância, adicione o guard `IsYourPlayer` já usado corretamente em `CanWalkPatch.cs`/`ProneMoveStatePatch.cs` (prova de que o padrão certo já existe no próprio mod), e (c) para os que mutam **templates compartilhados** (`HealthEffectComponentPatch`, `UpdateWeaponsPatch`), resolva o problema de fundo (ajuste por-instância em vez de por-template) antes de mexer no null-check, já que o null-check sozinho não corrige a corrupção de dados compartilhados em coop.

---

## 2. Tabela de Achados

| ID | Severidade | Arquivo / Linha | Categoria | Descrição Resumida |
|---|---|---|---|---|
| `AUD-01-01` | 🔴 Crítico | [DoorSoundPatch.cs:16,24-43](../modded/Plugin/Skills/SilentOps/Patches/DoorSoundPatch.cs) | Cross-ref (D1) / AP-02 | Silencia som de qualquer porta do mapa com o Silent Ops do MainPlayer |
| `AUD-01-02` | 🔴 Crítico | [MovementContextSetSpeedLimitPatch.cs:21,29](../modded/Plugin/Skills/Strength/Patches/MovementContextSetSpeedLimitPatch.cs) | Cross-ref (D1) / AP-02 | Speed limit de qualquer `MovementContext` calculado com skill do MainPlayer + NRE |
| `AUD-01-03` | 🔴 Crítico | [StimulatorApplyBuffPatch.cs:12-17,28-31](../modded/Plugin/Skills/FieldMedicine/Patches/StimulatorApplyBuffPatch.cs) | Cross-ref (D1) / AP-02 | Cap de FieldMedicine de qualquer injetável usa MainPlayer |
| `AUD-01-04` | 🔴 Crítico | [UpdateWeaponsPatch.cs:52-111](../modded/Plugin/Skills/WeaponSkills/Patches/UpdateWeaponsPatch.cs) | AP-02 / Null-safety | Null-check único no topo de coroutine multi-frame — NRE se raid terminar no meio |
| `AUD-01-05` | 🔴 Crítico | [CultistProductionPatch.cs:17-37,52-82](../modded/Server/Patches/CultistProductionPatch.cs) | Threading (D6) | Race condition: campo `static` compartilha estado entre Prefix/Postfix sob requisições HTTP concorrentes |
| `AUD-01-06` | 🔴 Crítico | [GeneratePlayerScavPatch.cs:28,36-58,60-83](../modded/Server/Patches/GeneratePlayerScavPatch.cs) | Threading (D6) | Mesmo padrão de race condition — bot gerado com role/aparência inconsistente |
| `AUD-01-07` | 🟠 Alto | [HealthEffectComponentPatch.cs:22-70](../modded/Plugin/Skills/FirstAid/Patches/HealthEffectComponentPatch.cs) | Cross-ref (D1) | Muta `template.DamageEffects[].Cost` compartilhado com skill do MainPlayer |
| `AUD-01-08` | 🟠 Alto | [UpdateWeaponsPatch.cs:97-107,163-174](../modded/Plugin/Skills/WeaponSkills/Patches/UpdateWeaponsPatch.cs) | Cross-ref (D1) | Muta `Weapon.Template.Ergonomics`/`RecoilForce*` compartilhado |
| `AUD-01-09` | 🟠 Alto | [UpdateWeaponsPatch.cs:34-50](../modded/Plugin/Skills/WeaponSkills/Patches/UpdateWeaponsPatch.cs) | Update (D2) | Disparo por `OnScreenChanged` sem gating de raid, cache limpo a cada disparo, risco de coroutines concorrentes |
| `AUD-01-10` | 🟠 Alto | [AbstractSkillClassSummaryLevelPatch.cs:17-38](../modded/Plugin/Skills/FieldMedicine/Patches/AbstractSkillClassSummaryLevelPatch.cs) | Cross-ref (D1) | Cap de skill exibido vem do MainPlayer, não do dono de `__instance` |
| `AUD-01-11` | 🟠 Alto | [GetBarterPricePatch.cs:49,83](../modded/Plugin/Skills/SilentOps/Patches/GetBarterPricePatch.cs) | AP-02 | `GetSkillManager()!` em telas de trader/flea pode NRE em transições |
| `AUD-01-12` | 🟠 Alto | [OnGameStarted.cs:97](../modded/Plugin/Skills/Shared/Patches/OnGameStarted.cs) | AP-02 | `GetPlayer()!` redundante ignora campo estático já validado |
| `AUD-01-13` | 🟠 Alto | [ReflectionHelper.cs:10-34](../modded/Plugin/Helpers/ReflectionHelper.cs) | Código morto (D4) | 7 campos de reflection não utilizados; 2 resolvem o tipo errado (copy-paste) |
| `AUD-01-14` | 🟠 Alto | [OnGameStarted.cs:55,59,64](../modded/Plugin/Skills/Shared/Patches/OnGameStarted.cs) | Memory (D3) | 3 subscriptions de evento sem teardown de raid |
| `AUD-01-15` | 🟠 Alto | [LockPickingHelpers.cs:19,231-240](../modded/Plugin/Skills/LockPicking/LockPickingHelpers.cs) | Memory (D3) | `DoorAttempts` estático nunca limpo no headless — cresce por sessão |
| `AUD-01-16` | 🟡 Médio | [MeleeSpeedPatch.cs:30-36](../modded/Plugin/Skills/SilentOps/Patches/MeleeSpeedPatch.cs) | Cross-ref (D1) | Bug comportamental remanescente pós-fix: buff aplicado a qualquer golpe |
| `AUD-01-17` | 🟡 Médio | [LockPickingGame.cs:30,405,423](../modded/Plugin/Skills/LockPicking/LockPickingGame.cs) | AP-02 | Property `SkillManager` esconde `[CanBeNull]` |
| `AUD-01-18` | 🟡 Médio | [ConsoleCommands.cs:41-43,61,74,87](../modded/Plugin/Helpers/ConsoleCommands.cs) | AP-02 | Acesso inconsistente a `Singleton<GameWorld>`; `foreach` sobre possível `null` |
| `AUD-01-19` | 🟡 Médio | [ProneMoveStatePatch.cs:42](../modded/Plugin/Skills/ProneMovement/Patches/ProneMoveStatePatch.cs) | Memory (D3) | Closure alocada a cada tick durante prone |
| `AUD-01-20` | 🟡 Médio | [PersonalBuffPatch.cs:25](../modded/Plugin/Skills/FieldMedicine/Patches/PersonalBuffPatch.cs), [PersonalBuffStringPatches.cs:25](../modded/Plugin/Skills/FieldMedicine/Patches/PersonalBuffStringPatches.cs) | Memory (D3) | `.Clone()` alocando objeto inteiro por chamada de tooltip |
| `AUD-01-21` | 🟡 Médio | [DoorActionPatch.cs:36-37](../modded/Plugin/Skills/LockPicking/Patches/DoorActionPatch.cs), [WorldInteractionUtils.cs](../modded/Plugin/Skills/LockPicking/WorldInteractionUtils.cs) | Memory (D3) | Alocação sem pool ao montar menu de interação de porta |
| `AUD-01-22` | 🔵 Baixo | [KeyCardDoorActionPatch.cs:11-30](../modded/Plugin/Skills/LockPicking/Patches/KeyCardDoorActionPatch.cs) | Cross-ref (D1) | Assinatura de alvo diverge do decompiled atual (dormente via `[IgnoreAutoPatch]`) |
| `AUD-01-23` | 🔵 Baixo | [LockPickingGame.cs:87,129-152](../modded/Plugin/Skills/LockPicking/LockPickingGame.cs) | Memory (D3) | `_onUnlocked` não zerado em `OnDisable` |
| `AUD-01-24` | 🔵 Baixo | [UpdateChecker.cs:45](../modded/Server/Core/UpdateChecker.cs) | Memory (D3) | `HttpClient` sem `using`/`Dispose` (boot-only) |
| `AUD-01-25` | 🔵 Baixo | [UpdateChecker.cs:82-83](../modded/Server/Core/UpdateChecker.cs) | Código morto (D4) | `catch { }` vazio sem log |
| `AUD-01-26` | 🔵 Baixo | [UpdateChecker.cs:32](../modded/Server/Core/UpdateChecker.cs) | Threading (D6) | `Task.Run` fire-and-forget sem `CancellationToken` |
| `AUD-01-27` | 💡 Otimização | [SkillManagerConstructorPatch.cs:118-140](../modded/Plugin/Skills/Core/Patches/SkillManagerConstructorPatch.cs), [HackingActionHandler.cs:17](../modded/Plugin/Skills/LockPicking/Actions/HackingActionHandler.cs) | AP-04 | Reflection resolvida repetidamente em vez de cacheada |
| `AUD-01-28` | 💡 Otimização | [LockPickingGame.cs:185-192](../modded/Plugin/Skills/LockPicking/LockPickingGame.cs) | Update (D2) | Reassert de cursor/input todo frame em vez de uma vez no `Activate()` |

---

## 3. Detalhamento dos Achados

### AUD-01-01 · `DoorSoundPatch` silencia o som de qualquer porta do mapa com o Silent Ops do jogador local
- **Severidade:** 🔴 Crítico
- **Localização:** [DoorSoundPatch.cs:16,24-43](../modded/Plugin/Skills/SilentOps/Patches/DoorSoundPatch.cs)
- **Referência Cruzada:** [WorldInteractiveObject.cs:1060](../../../references/eft-decompiled/Assembly-CSharp/EFT.Interactive/WorldInteractiveObject.cs) (`PlaySound`, método de instância por porta) e `:1088-1100` (`SmoothDoorOpenCoroutine` chama `PlaySound` **independente de `isLocalInteraction`** — também para portas abertas por bots/peers replicados).
- **Causa Raiz:** o Prefix intercepta `WorldInteractiveObject.PlaySound` (alvo é a porta, não um jogador) e retorna `false` (skip total do original) sempre que `SilentOps.Enabled`, substituindo o volume por um cálculo baseado em `GameUtils.GetSkillManager()` — sempre o `MainPlayer` local — e ignorando o parâmetro `volume` original (que varia por porta via `OpenSoundVolumeRange`).
- **Impacto Técnico Real:** em qualquer raid (single ou Fika coop), o som de **toda porta do mapa** — inclusive as abertas por bots e outros peers — fica com volume reduzido conforme o Silent Ops do jogador local. Em coop isso silencia indevidamente pistas sonoras de portas de colegas/inimigos no cliente de quem tem a skill alta.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - *Abordagem Atual:* skip total do original, volume sempre recalculado a partir do MainPlayer, ignorando quem interagiu.
  - *Abordagem Otimizada:* resolver o ator real da interação (via o handle de interação da porta, não `Singleton<GameWorld>.MainPlayer`) e só aplicar o bônus quando esse ator for o jogador local; caso contrário, `return true` (executa original com o volume nativo).
```csharp
[PatchPrefix]
public static bool Prefix(WorldInteractiveObject __instance, EDoorState state, float volume, GamePlayerOwner ___lastInteractedPlayer /* ou campo equivalente que identifique o interator real */)
{
    if (!SkillsExtendedPlugin.SkillData.SilentOps.Enabled) return true;
    if (___lastInteractedPlayer?.Player == null || !___lastInteractedPlayer.Player.IsYourPlayer) return true;

    var skillManager = GameUtils.GetSkillManager();
    if (skillManager == null) return true;

    var adjustedVolume = volume * (1f - skillManager.SkillManagerExtended.SilentOpsReduceVolumeBuff);
    __instance.PlaySound(state, adjustedVolume);
    return false;
}
```
  *(nome exato do campo/handle que identifica o interator precisa ser confirmado lendo `WorldInteractiveObject`/`Door` por completo — o ponto chave é NÃO usar `Singleton<GameWorld>.MainPlayer`.)*
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-02 · `MovementContextSetSpeedLimitPatch` aplica o Strength do MainPlayer a qualquer `MovementContext` + NRE
- **Severidade:** 🔴 Crítico
- **Localização:** [MovementContextSetSpeedLimitPatch.cs:21,29](../modded/Plugin/Skills/Strength/Patches/MovementContextSetSpeedLimitPatch.cs)
- **Referência Cruzada:** [MovementContext.cs:1702](../../../references/eft-decompiled/Assembly-CSharp/EFT/MovementContext.cs) (`method_0()`, instância por `MovementContext`) e `:157` (`protected Player _player;` — já acessível via Harmony `____player`).
- **Causa Raiz:** `Prefix(MovementContext __instance)` faz `return false` (skip total) para **qualquer** `MovementContext` (bots, peers Fika, jogador local), usando `GameUtils.GetSkillManager()!` (MainPlayer, `!` sem checagem) em vez do skill do dono real de `__instance`. O próprio mod já demonstra o padrão correto em [CanWalkPatch.cs:17-22](../modded/Plugin/Skills/FirstAid/Patches/CanWalkPatch.cs), que injeta `Player ____player` e faz `if (!____player.IsYourPlayer) return;` antes de tocar em skill — este patch não replica esse guard.
- **Impacto Técnico Real:** (1) em coop, o limite de velocidade em pântano/obstáculos de bots e outros peers é calculado com o bônus de Strength do jogador local, dessincronizando percepção de movimento entre clientes; (2) se `GameUtils.GetSkillManager()` retornar `null` (mesmo cenário do `MeleeSpeedPatch` já corrigido — headless, cliente em transição), `NullReferenceException` toda vez que qualquer `MovementContext` no mundo entra/sai de um obstáculo — potencialmente a cada bot que pisa em vegetação.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - *Abordagem Atual:* `Prefix` sem filtro de dono, `!` sem null-check.
  - *Abordagem Otimizada:* mesmo padrão de `CanWalkPatch`/`ProneMoveStatePatch`.
```csharp
[PatchPrefix]
public static bool Prefix(MovementContext __instance, Player ____player)
{
    if (____player == null || !____player.IsYourPlayer) return true;

    var skillManager = GameUtils.GetSkillManager();
    if (skillManager == null) return true;

    // ... resto do cálculo original, usando skillManager em vez de GetSkillManager()!
    return false;
}
```
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-03 · `StimulatorApplyBuffPatch` aplica o cap de FieldMedicine do MainPlayer a qualquer uso de injetável
- **Severidade:** 🔴 Crítico
- **Localização:** [StimulatorApplyBuffPatch.cs:12-17,28-31](../modded/Plugin/Skills/FieldMedicine/Patches/StimulatorApplyBuffPatch.cs)
- **Referência Cruzada:** [ActiveHealthController.cs:3064](../../../references/eft-decompiled/Assembly-CSharp/EFT.HealthSystem/ActiveHealthController.cs) (`Stimulator.smethod_0`, método **estático**) e `:2712-2724` (o caller de instância usa `base.HealthController.SkillManager_0` — o `SkillManager` correto existe no ponto de chamada original, só não é passado ao método estático).
- **Causa Raiz:** o alvo do patch é um método estático que não recebe `Player`/`SkillManager` como parâmetro — a única forma de saber "de quem" é o cap exigiria patchar o caller de instância em vez do utilitário estático. O mod contorna isso caindo em `GameUtils.GetSkillManager()` (sempre MainPlayer).
- **Impacto Técnico Real:** quando um bot ou peer remoto usa um estimulante que afeta `SkillRate`, o cap de FieldMedicine é calculado com o bônus do MainPlayer local, corrompendo o clamp do buff de outros jogadores conforme visto no cliente local.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - *Abordagem Atual:* patch no método estático utilitário, sem contexto de dono.
  - *Abordagem Otimizada:* mover o ponto de patch para o método de **instância** que chama `smethod_0` (o `Effect<GStruct394>` que expõe `HealthController.Player`/`SkillManager_0`), passando explicitamente o `SkillManager` do dono do efeito em vez de resolver globalmente via `GameUtils`.
- **Nota:** esta é a correção arquiteturalmente mais trabalhosa dos 3 achados 🔴 de mesmo padrão — exige reidentificar o ponto de patch correto, não é um one-liner. Recomendo tratar como item de spec técnica separado dentro do backlog, não como fix pontual.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-04 · `UpdateWeaponsPatch` — null-check único no topo de uma coroutine multi-frame
- **Severidade:** 🔴 Crítico
- **Localização:** [UpdateWeaponsPatch.cs:52-111](../modded/Plugin/Skills/WeaponSkills/Patches/UpdateWeaponsPatch.cs) (mesma estrutura em `:113-178`)
- **Causa Raiz:** `UpdateUsecWeapons()` checa `SkillManager is null` uma única vez na linha 54, mas o `foreach` faz `yield return null;` (linha 109) — a coroutine atravessa múltiplos frames. Nas linhas 63, 89 e 107, `GameUtils.GetProfile(side)!`/`GameUtils.GetSkillManager()!` são chamados de novo, sem novo null-check.
- **Impacto Técnico Real:** se o jogador extrair/morrer/sair de raid enquanto a coroutine ainda itera armas (`StaticManager.BeginCoroutine` não é cancelada no fim de raid), as chamadas seguintes retornam `null` e o `!` gera `NullReferenceException` no frame seguinte.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
```csharp
foreach (var weapon in weapons)
{
    var skillManager = GameUtils.GetSkillManager();
    if (skillManager == null) yield break; // raid encerrou no meio da iteração

    // ... resto do corpo usando skillManager local em vez de GetSkillManager()!
    yield return null;
}
```
- **Como validar:** sair de raid (extração ou morte) enquanto a tela de armas está aberta processando; confirmar que não há exceção no log e que a coroutine encerra silenciosamente.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-05 · `CultistProductionPatch` — race condition em campo estático compartilhado entre requisições HTTP concorrentes
- **Severidade:** 🔴 Crítico
- **Localização:** [CultistProductionPatch.cs:17-37](../modded/Server/Patches/CultistProductionPatch.cs) (`StartSacrificePatch`) e `:52-82` (`CultistProductionPatch`)
- **Referência Cruzada:** [CircleOfCultistService.cs:92](../../../references/spt-source/Libraries/SPTarkov.Server.Core/Services/CircleOfCultistService.cs) (confirma que `GetCircleCraftingInfo` é chamado *dentro* de `StartSacrifice`) — assinatura real `GetCircleCraftingInfo(double, CultistCircleSettings, DirectRewardSettings?)`, sem `sessionId`/`pmcData`.
- **Causa Raiz:** `StartSacrificePatch.Prefix` grava o `sessionId` da requisição em `internal static MongoId PmcProfileId`. `CultistProductionPatch.Postfix` lê esse mesmo campo estático para aplicar o desconto de Shadow Connections. O campo é global ao processo; o servidor SPT (.NET 9 ASP.NET) processa requisições HTTP concorrentes no thread pool. Em Fika coop, duas chamadas a `StartSacrifice` quase simultâneas de jogadores diferentes corrompem esse campo compartilhado entre si.
- **Impacto Técnico Real:** o desconto de tempo do círculo cultista de um jogador pode ser calculado com o nível de Shadow Connections de outro; o `Postfix` de `StartSacrificePatch` reseta `PmcProfileId = MongoId.Empty()` ao final de uma chamada, podendo fazer a chamada aninhada de outra requisição ainda em andamento encontrar `IsEmpty == true` e lançar `InvalidOperationException`, derrubando a requisição HTTP do outro jogador **depois** dos itens já terem sido removidos do inventário dele.
- **Alternativa de Melhor Lógica / Proposta de Correção:**
  - *Abordagem Atual:* campo `static` como canal de comunicação entre Prefix e Postfix de métodos diferentes.
  - *Abordagem Otimizada:* mover a lógica de desconto para um **Postfix de `StartSacrifice`** (que já recebe `sessionId`/`pmcData` como parâmetros diretos), localizando a produção cultista recém-registrada em `pmcData.Hideout.Production` (flag `SptIsCultistCircle`) e aplicando o multiplicador ali — zero estado compartilhado entre threads.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-06 · `GeneratePlayerScavPatch` — mesmo padrão de race condition, bot gerado inconsistente
- **Severidade:** 🔴 Crítico
- **Localização:** [GeneratePlayerScavPatch.cs:28,36-58,60-83](../modded/Server/Patches/GeneratePlayerScavPatch.cs)
- **Causa Raiz:** `private static bool _generateAsCultist` passa a decisão do `Prefix` (patch em `BotGenerator.GeneratePlayerScav`) para o `Postfix` do mesmo método. Em Fika coop, múltiplos jogadores podem gerar scav em janela de tempo próxima, processados em threads distintas do pool ASP.NET.
- **Impacto Técnico Real:** se o `Prefix` do jogador B (`_generateAsCultist = false`) executar entre o `Prefix` e o `Postfix` do jogador A (que sorteou `true`), o `Postfix` de A lê `false` e pula `SetAppearance`/`SetHealth` — um bot com `role = "sectantWarrior"` mas aparência/vida de scav comum. O inverso também vale.
- **Alternativa de Melhor Lógica / Proposta de Correção:** usar `__state` do Harmony para passar o valor do Prefix ao Postfix sem estado estático compartilhado:
```csharp
[PatchPrefix]
public static void Prefix(MongoId sessionId, ref string role, out bool __state)
{
    __state = false;
    if (!ConfigController.SkillsConfig.ShadowConnections.Enabled) return;
    if (!SkillUtil.TryGetSkillLevel(sessionId, SkillTypes.Shadowconnections, out var level)) return;
    __state = RandomUtil.GetChance100(ConfigController.SkillsConfig.ShadowConnections.ScavGenerateAsCultistChance * level);
    if (__state) role = "sectantWarrior";
}

[PatchPostfix]
public static void Postfix(PmcData __result, bool __state)
{
    if (!__state) return;
    // ... resto da lógica, sem reset manual de campo estático
}
```
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-07 · `HealthEffectComponentPatch` muta o template compartilhado do item médico
- **Severidade:** 🟠 Alto
- **Localização:** [HealthEffectComponentPatch.cs:22-70](../modded/Plugin/Skills/FirstAid/Patches/HealthEffectComponentPatch.cs) (mutações em `Cost` nas linhas 105, 136, 166)
- **Referência Cruzada:** [HealthEffectsComponent.cs:34](../../../references/eft-decompiled/Assembly-CSharp/EFT.InventoryLogic/HealthEffectsComponent.cs) (`DamageEffects => IHealthEffect.DamageEffects;` — repassa a referência do `template`, tipicamente compartilhado por todos os `Item` do mesmo `TemplateId`).
- **Causa Raiz:** o Postfix do construtor `HealthEffectsComponent(Item, IHealthEffect template)` ajusta `template.DamageEffects[X].Cost` **in-place** com base em `GameUtils.GetSkillManager()` (MainPlayer). Como `template` não é clonado por item, a mutação afeta o objeto compartilhado do `TemplateId` inteiro.
- **Impacto Técnico Real:** em coop, se o cliente local instanciar `HealthEffectsComponent` para itens de outro jogador (dependendo de como o Fika materializa itens remotos localmente), o custo de recurso do medicamento fica ajustado pelo FirstAid do MainPlayer local para **todos** os usos daquele `TemplateId` nesse cliente.
- **Alternativa de Melhor Lógica / Proposta de Correção:** aplicar o ajuste de custo por-instância (wrapper não compartilhado, ou recalcular no ponto de consumo passando o `SkillManager` do dono) em vez de mutar `template.DamageEffects` diretamente.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-08 · `UpdateWeaponsPatch` muta `Weapon.Template.Ergonomics`/`RecoilForce*` compartilhado
- **Severidade:** 🟠 Alto
- **Localização:** [UpdateWeaponsPatch.cs:97-107,163-174](../modded/Plugin/Skills/WeaponSkills/Patches/UpdateWeaponsPatch.cs)
- **Causa Raiz:** mesmo padrão do achado anterior — `weapon.Template.Ergonomics`/`RecoilForceUp`/`RecoilForceBack` são escritos diretamente no objeto de template compartilhado por `TemplateId`, calculados a partir do skill do MainPlayer, disparado a cada `MenuTaskBar.OnScreenChanged`.
- **Impacto Técnico Real:** se um bot ou peer remoto usar arma do mesmo `TemplateId`, herda os stats ajustados pelo Weapon Systems do MainPlayer local nesse cliente específico (visualmente incorreto no lado observador).
- **Alternativa de Melhor Lógica / Proposta de Correção:** não mutar `Template` diretamente; aplicar via `Item.Attributes`/bônus por-instância em vez de campos compartilhados do template.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-09 · `UpdateWeaponsPatch` recalcula o inventário inteiro a cada troca de tela de menu, sem gating de raid
- **Severidade:** 🟠 Alto
- **Localização:** [UpdateWeaponsPatch.cs:34-50](../modded/Plugin/Skills/WeaponSkills/Patches/UpdateWeaponsPatch.cs) (Prefix em `MenuTaskBar.OnScreenChanged`)
- **Causa Raiz:** o gatilho dispara em qualquer navegação de tela do menu (stash, hideout, trader), inclusive **fora de raid** (`GameUtils.GetProfile()` usa `Session.Profile`, que existe fora de raid também). O Prefix faz `UsecWeaponInstanceIds.Clear()`/`EasternWeaponInstanceIds.Clear()` **antes** de iniciar a coroutine, invalidando o cache "já ajustado neste nível" a cada disparo, forçando reprocessamento completo (LINQ `.Where()` + rebuild de dois `Dictionary`) mesmo quando o nível de skill não mudou.
- **Impacto Técnico Real:** navegação rápida entre abas do menu pode empilhar múltiplas coroutines concorrentes sobre o mesmo dicionário estático compartilhado — risco de corrupção de estado, não só desperdício de CPU fora de raid.
- **Alternativa de Melhor Lógica / Proposta de Correção:** gatear com `GameUtils.IsInRaid()` ou, melhor, vincular a um evento real de mudança de skill (`Player.Skills.OnMasteringExperienceChanged`, já usado em `OnGameStarted.cs`); trocar `.Clear()` incondicional por dirty-flag por nível de skill.
- **Como validar:** contador de execuções da coroutine antes/depois navegando rapidamente entre 10 telas de menu no hideout; critério: cai de N (uma por troca de tela) para 0 fora de raid e apenas quando o nível de skill muda dentro de raid.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-10 · `AbstractSkillClassSummaryLevelPatch` usa o cap do MainPlayer em vez do dono de `__instance`
- **Severidade:** 🟠 Alto
- **Localização:** [AbstractSkillClassSummaryLevelPatch.cs:17-38](../modded/Plugin/Skills/FieldMedicine/Patches/AbstractSkillClassSummaryLevelPatch.cs)
- **Causa Raiz:** `Prefix(AbstractSkillClass __instance, ref int __result)` usa `__instance.Level`/`__instance.Buff` corretamente, mas o cap (`FieldMedicineSkillCap`) vem de `GameUtils.GetSkillManager()` (MainPlayer) em vez do `SkillManager` que realmente possui `__instance`.
- **Impacto Técnico Real:** se `SummaryLevel` for lido para o `AbstractSkillClass` de outro jogador/perfil (telas de comparação de stats), o cap aplicado é o do MainPlayer local. Alcance real não totalmente confirmado (nem todos os call-sites de `SummaryLevel` foram cruzados no decompiled) — daí 🟠 em vez de 🔴.
- **Alternativa de Melhor Lógica / Proposta de Correção:** resolver o `SkillManager` a partir de `__instance` (via campo de referência interno, se existir) em vez de `GameUtils.GetSkillManager()`.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-11 · `GetBarterPricePatch`/`RequiredItemsCountPatch` — `GetSkillManager()!` em telas de trader
- **Severidade:** 🟠 Alto
- **Localização:** [GetBarterPricePatch.cs:49,83](../modded/Plugin/Skills/SilentOps/Patches/GetBarterPricePatch.cs)
- **Causa Raiz:** ambos os Postfix rodam em `TraderAssortmentControllerClass.GetBarterPrice`/`RequiredItemsCount` — telas que podem abrir em estados de transição (hideout carregando, troca de perfil) onde `GetSkillManager()` pode legitimamente ser `null`.
- **Impacto Técnico Real:** NRE ao abrir a loja de um trader nesses estados, podendo travar a UI de trade.
- **Alternativa de Melhor Lógica / Proposta de Correção:** capturar em variável, checar null e pular o bônus — mesmo padrão já correto em `HealthEffectComponentPatch`/`HealthEffectUseTimePatch` no mesmo grupo de arquivos.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-12 · `OnGameStarted.ApplyMedicalXp` — chamada redundante e insegura ignora campo já validado
- **Severidade:** 🟠 Alto
- **Localização:** [OnGameStarted.cs:97](../modded/Plugin/Skills/Shared/Patches/OnGameStarted.cs)
- **Causa Raiz:** `GameUtils.GetPlayer()!.Skills.FirstAid.IsEliteLevel` é chamado dentro do handler `EffectStartedEvent`, assinado no Postfix (linha 55) usando o campo estático `Player` já validado — mas o corpo do handler re-resolve via `GameUtils.GetPlayer()` em vez de reusar `Player`.
- **Impacto Técnico Real:** se o evento disparar numa janela onde `Singleton<GameWorld>.Instance` momentaneamente diverge (fim de raid, troca de cena), NRE.
- **Alternativa de Melhor Lógica / Proposta de Correção:** trocar por `Player.Skills.FirstAid` (reusar o campo estático já non-null nesse ponto do fluxo), eliminando a chamada redundante e o `!`.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-13 · `ReflectionHelper` — 7 campos não utilizados, 2 com bug de copy-paste
- **Severidade:** 🟠 Alto
- **Localização:** [ReflectionHelper.cs:10-34](../modded/Plugin/Helpers/ReflectionHelper.cs)
- **Causa Raiz:** `BleedType`, `LightBleedType`, `HeavyBleedType`, `FractureType`, `PainType`, `MedEffectType`, `StimulatorType` são resolvidos via reflection no `static ReflectionHelper()` mas **nenhum é lido em lugar nenhum do projeto** (confirmado via Grep qualificado/não-qualificado). Bug real embutido: `BleedType` e `LightBleedType` (linhas 22-23) resolvem o **mesmo** tipo `"LightBleeding"`; `StimulatorType` (linha 28) resolve `"MedEffect"`, duplicando `MedEffectType`, em vez de `"Stimulator"`.
- **Impacto Técnico Real:** hoje inofensivo (nada lê esses campos), mas se código futuro passar a consumir `StimulatorType` esperando o tipo certo, vai silenciosamente comparar contra o tipo errado.
- **Alternativa de Melhor Lógica / Proposta de Correção:** remover os 6 campos não utilizados, ou corrigir os nomes de tipo e adicionar ao menos um caller que prove o uso.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-14 · `OnGameStartedPatch` — 3 subscriptions de evento sem teardown de raid
- **Severidade:** 🟠 Alto
- **Localização:** [OnGameStarted.cs:55,59,64](../modded/Plugin/Skills/Shared/Patches/OnGameStarted.cs)
- **Causa Raiz:** `Postfix` faz `Player.ActiveHealthController.EffectStartedEvent += ApplyMedicalXp`, `Player.Skills.OnMasteringExperienceChanged += ApplyNatoRifleXp/ApplyEasternRifleXp` a cada raid, sem nenhum patch em `GameWorld.OnDestroy`/`BaseLocalGame.Stop` fazendo `-=`. Confirmado por Grep: `-=` não aparece em nenhum arquivo de `Plugin/`.
- **Impacto Técnico Real:** não é crescimento ilimitado (o campo estático `Player` é sobrescrito na próxima raid), mas atrasa a coleta do grafo inteiro do `Player` anterior (inventário, armas, animator) durante todo o tempo no menu entre raids.
- **Alternativa de Melhor Lógica / Proposta de Correção:** patch idempotente em `GameWorld.OnDestroy` que faça os 3 `-=` e zere `Player = null`.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-15 · `LockPickingHelpers.DoorAttempts` nunca é limpo no Fika headless
- **Severidade:** 🟠 Alto
- **Localização:** [LockPickingHelpers.cs:19,231-240](../modded/Plugin/Skills/LockPicking/LockPickingHelpers.cs); guard que pula o clear: [OnGameStarted.cs:43](../modded/Plugin/Skills/Shared/Patches/OnGameStarted.cs); escrita via rede: [LockPickingFikaController.cs:52](../modded/FikaSync/Controllers/LockPickingFikaController.cs)
- **Causa Raiz:** `InitializeLockpickingForLocation()` (que faz `DoorAttempts.Clear()`) só roda a partir de `OnGameStartedPatch.Postfix`, que retorna cedo com `if (!__instance.MainPlayer || SkillsExtendedInfo.IsFikaHeadless) return;` — **nunca roda no headless**. `LockPickingFikaController.HandlePacket` (sem esse guard) escreve em `DoorAttempts` a cada tentativa sincronizada, em qualquer mapa.
- **Impacto Técnico Real:** IDs de porta de mapas diferentes se acumulam ao longo de uma sessão headless longa. Teto = total de portas trancáveis do jogo (baixo em bytes absolutos), mas é um `static Dictionary` sem eviction que só cresce durante toda a vida do processo headless.
- **Alternativa de Melhor Lógica / Proposta de Correção:** no `FikaSync/Patches/OnGameStartedPatch.cs`, chamar também `LockPickingHelpers.DoorAttempts.Clear()` no início de cada raid, independente de ser headless.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-16 · `MeleeSpeedPatch` — bug comportamental remanescente após o fix de NRE (v2.2.3)
- **Severidade:** 🟡 Médio
- **Localização:** [MeleeSpeedPatch.cs:30-36](../modded/Plugin/Skills/SilentOps/Patches/MeleeSpeedPatch.cs)
- **Referência Cruzada:** [ObjectInHandsAnimator.cs:214-217](../../../references/eft-decompiled/Assembly-CSharp/ObjectInHandsAnimator.cs) (`SetMeleeSpeed(float speed)`, sem parâmetro de jogador) e [Player.cs:24110,24721](../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs) (`HandsAnimator` é campo privado de `Player`, **sem referência inversa** de `ObjectInHandsAnimator` para o `Player` dono — confirmado no decompiled).
- **Causa Raiz:** a correção já aplicada nesta sessão (null-check em vez de `!`) elimina o `NullReferenceException`, mas não corrige a causa comportamental: o bônus `SilentOpsIncMeleeSpeedBuff` continua sendo sempre o do `MainPlayer`, aplicado à velocidade de golpe de **qualquer** jogador/bot que balance uma arma branca — porque `ObjectInHandsAnimator` genuinamente não guarda referência ao seu `Player` dono (confirmado no decompiled, não é suposição).
- **Impacto Técnico Real:** em coop, a velocidade de golpe de outros peers/bots (como renderizada no cliente local) reflete o Silent Ops do MainPlayer local, não do próprio ator. Severidade reduzida (🟡, não 🔴) porque o crash já foi eliminado — o que resta é um valor visualmente incorreto, não uma falha.
- **Alternativa de Melhor Lógica / Proposta de Correção:** resolver o `Player` dono via `Singleton<GameWorld>.Instance.AllAlivePlayersList` comparando `p.HandsAnimator == __instance` (evento não é hot-path por frame, custo aceitável) e aplicar o skill desse `Player`; ou, se o custo não for aceitável, documentar explicitamente no código que o patch é "best-effort, viés para MainPlayer" — isso já deveria estar comentado no arquivo.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-17 · `LockPickingGame.SkillManager` esconde a nulidade real da property
- **Severidade:** 🟡 Médio
- **Localização:** [LockPickingGame.cs:30,405,423](../modded/Plugin/Skills/LockPicking/LockPickingGame.cs)
- **Causa Raiz:** `private static SkillManager SkillManager => GameUtils.GetSkillManager();` — o tipo de retorno declarado (`SkillManager`, não `SkillManager?`) esconde o `[CanBeNull]` real. `SetSweetSpotRange`/`SetTimeLimit` usam `SkillManager.SkillManagerExtended...` direto, sem checagem.
- **Impacto Técnico Real:** risco menor que os achados 🔴/🟠 acima (o minigame só ativa via interação do `MainPlayer`), mas ainda é um NRE-em-potencial se o jogador for removido do mundo entre `Activate()` e o próximo uso. Mesmo padrão "mentiroso" repetido em `DoorSoundPatch.cs:16` e `UpdateWeaponsPatch.cs:27`.
- **Alternativa de Melhor Lógica / Proposta de Correção:** checar null no `Activate()`/antes de usar, ou centralizar um helper que não reexponha `[CanBeNull]` como não-nulo via property.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-18 · `ConsoleCommands` — acesso não-defensivo inconsistente + `foreach` sobre possível `null`
- **Severidade:** 🟡 Médio
- **Localização:** [ConsoleCommands.cs:61,74,87](../modded/Plugin/Helpers/ConsoleCommands.cs) vs. `:51` (`ResetDoorLocks`, correto); `:41-43` (`GetAllWeaponIDsInInventory`)
- **Causa Raiz:** `DoDamage`/`DoDie`/`DoFracture` fazem `Singleton<GameWorld>.Instance.MainPlayer` direto, sem checar `.Instantiated` — enquanto `ResetDoorLocks`, no mesmo arquivo, já faz o check corretamente. Além disso, `GetAllWeaponIDsInInventory` itera `foreach (var weapon in weapons)` onde `weapons` pode ser `null` (a cadeia `GameUtils.GetProfile(side)?.Inventory?.AllRealPlayerItems.Where(...)` propaga null se `Inventory` for null).
- **Impacto Técnico Real:** comandos de debug crasham se digitados no menu principal/hideout antes do raid iniciar. Baixo impacto em produção (comandos de console manuais), mas barato de corrigir e é o padrão exato do antipattern.
- **Alternativa de Melhor Lógica / Proposta de Correção:** usar `GameUtils.GetPlayer()` (já embute o check de raid) em vez do acesso cru; `weapons?.ToList() ?? []` antes do `foreach`.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-19 · Closure alocada a cada tick de `ClampSpeed` durante Prone
- **Severidade:** 🟡 Médio
- **Localização:** [ProneMoveStatePatch.cs:42](../modded/Plugin/Skills/ProneMovement/Patches/ProneMoveStatePatch.cs)
- **Causa Raiz:** `player.ExecuteSkill(() => player.Skills.ProneAction.Complete(ProneData.XpPerAction));` cria uma lambda capturando `player`/`ProneData.XpPerAction` toda vez que `MovementContext.ClampSpeed` é chamado — continuamente enquanto o jogador local está de bruços.
- **Impacto Técnico Real:** só afeta o jogador local (`player.IsYourPlayer`), não multiplica por bots. Pequeno por alocação (~24-40 bytes), mas contínuo durante qualquer trecho prono — soma-se a um fluxo constante de lixo em jogo furtivo.
- **Alternativa de Melhor Lógica / Proposta de Correção:** cachear um `Action` estático reutilizável, ou reescrever como método estático sem captura de variáveis locais.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-20 · `PersonalBuffPatch`/`PersonalBuffStringPatches` — `.Clone()` alocando por chamada de tooltip
- **Severidade:** 🟡 Médio
- **Localização:** [PersonalBuffPatch.cs:25](../modded/Plugin/Skills/FieldMedicine/Patches/PersonalBuffPatch.cs); [PersonalBuffStringPatches.cs:25](../modded/Plugin/Skills/FieldMedicine/Patches/PersonalBuffStringPatches.cs)
- **Causa Raiz:** cada chamada faz `(InjectorBuff)__result.Clone()`/`(InjectorBuff)__instance.Clone()` incondicionalmente, alocando um `InjectorBuff` inteiro só para `AdjustStimulatorBuff` mutar `Duration`/`Chance` — o clone é descartado logo em seguida.
- **Impacto Técnico Real:** moderado — depende da frequência real de re-chamada em refresh de UI/tooltip (não confirmada em profiler), documentado como parte do fluxo de tooltip de item injetável.
- **Alternativa de Melhor Lógica / Proposta de Correção:** mudar a assinatura de `AdjustStimulatorBuff` para retornar `(duration, chance)` por valor, sem clonar o objeto inteiro.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-21 · Churn de alocação ao montar o menu de interação em portas trancadas
- **Severidade:** 🟡 Médio
- **Localização:** [DoorActionPatch.cs:36-37](../modded/Plugin/Skills/LockPicking/Patches/DoorActionPatch.cs); [WorldInteractionUtils.cs:24,35,47,59,70,82,99,105](../modded/Plugin/Skills/LockPicking/WorldInteractionUtils.cs)
- **Causa Raiz:** toda vez que `GetActionsClass.smethod_5` é chamado, `AddLockpickingInteraction`/`AddInspectInteraction` alocam `new LockPickingInteraction(...)`, `new LockInspectInteraction(...)` e `new ActionsTypesClass { ... }` sem cache/pool.
- **Impacto Técnico Real:** pequeno por chamada, descartado imediatamente pelo próprio `ActionsReturnClass` do jogo. Relevante para hitches em sessões client longas olhando pra portas com puzzle de tranca; irrelevante no headless (guard early-return já cobre).
- **Alternativa de Melhor Lógica / Proposta de Correção:** pool de `ActionsTypesClass`/handler por porta (chave = `interactiveObject.Id`), invalidado só quando o estado da porta muda.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-22 · `KeyCardDoorActionPatch` — assinatura de alvo diverge do decompiled atual
- **Severidade:** 🔵 Baixo
- **Localização:** [KeyCardDoorActionPatch.cs:11-30](../modded/Plugin/Skills/LockPicking/Patches/KeyCardDoorActionPatch.cs)
- **Referência Cruzada:** [GetActionsClass.cs:1705](../../../references/eft-decompiled/Assembly-CSharp/GetActionsClass.cs) — `smethod_9(GamePlayerOwner owner, Item rootItem, TraderControllerClass lootItemOwner, string lootItemId, string lootItemName, IPlayer lootItemLastOwner)`.
- **Causa Raiz:** o Postfix declara um parâmetro `KeycardDoor door` que **não existe** na assinatura real de `smethod_9` na EFT 0.16.9 (trata de loot de container/trader, não porta com keycard).
- **Impacto Técnico Real:** hoje inofensivo — a classe tem `[IgnoreAutoPatch]` e o corpo está comentado (feature incompleta). Se alguém remover `[IgnoreAutoPatch]` sem atualizar a assinatura, `PatchManager.EnablePatches()` falha ao aplicar o patch na inicialização do plugin.
- **Alternativa de Melhor Lógica / Proposta de Correção:** ao retomar a feature, re-resolver o alvo real (provavelmente outro `smethod_N` específico de `KeycardDoor`) contra o decompiled atual antes de remover `[IgnoreAutoPatch]`.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-23 · `LockPickingGame._onUnlocked` não é zerado em `OnDisable`
- **Severidade:** 🔵 Baixo
- **Localização:** [LockPickingGame.cs:87,129-152](../modded/Plugin/Skills/LockPicking/LockPickingGame.cs)
- **Causa Raiz:** `LockPickingGame` é um `GameObject` único com `DontDestroyOnLoad`, sobrevive a todas as raids. `_onUnlocked` é setado em `Activate()` a cada tentativa, mas `OnDisable()` não o zera. Se a raid terminar abruptamente com o minigame ativo, o `Action<bool>` retém o `LockPickActionHandler`/`GamePlayerOwner`/`WorldInteractiveObject` daquela raid até a próxima chamada de `Activate()`.
- **Impacto Técnico Real:** pequeno e não-cumulativo — cada nova tentativa sobrescreve `_onUnlocked`. Achado de higiene, não fonte de OOM.
- **Alternativa de Melhor Lógica / Proposta de Correção:** `_onUnlocked = null;` em `OnDisable()`.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-24 · `UpdateChecker` — `HttpClient` sem `Dispose`
- **Severidade:** 🔵 Baixo
- **Localização:** [UpdateChecker.cs:45](../modded/Server/Core/UpdateChecker.cs)
- **Causa Raiz:** `new HttpClient()` sem `using`/`Dispose()` dentro de `CheckForUpdate()`.
- **Impacto Técnico Real:** nulo em runtime normal — `OnLoad()` dispara essa checagem uma única vez por boot do servidor SPT, não por request. Antipadrão clássico, mas sem recorrência.
- **Alternativa de Melhor Lógica / Proposta de Correção:** `using var httpClient = new HttpClient();` por disciplina.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-25 · `UpdateChecker` — `catch { }` vazio sem log
- **Severidade:** 🔵 Baixo
- **Localização:** [UpdateChecker.cs:82-83](../modded/Server/Core/UpdateChecker.cs)
- **Causa Raiz:** único `catch` verdadeiramente vazio (sem log) em todo o escopo auditado. Comentário na linha 81 justifica a intenção ("we ignore errors"), mas zero log mesmo em `LogDebug`.
- **Impacto Técnico Real:** se a API do GitHub mudar formato ou o release JSON quebrar o parse, isso fica invisível para sempre — inclusive para o próprio dev investigando por que a checagem de update parou de funcionar.
- **Alternativa de Melhor Lógica / Proposta de Correção:** `catch (Exception ex) { Logger.LogDebug($"UpdateChecker: {ex.Message}"); }`.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-26 · `UpdateChecker` — `Task.Run` fire-and-forget sem `CancellationToken`
- **Severidade:** 🔵 Baixo
- **Localização:** [UpdateChecker.cs:32](../modded/Server/Core/UpdateChecker.cs)
- **Causa Raiz:** `_ = Task.Run(CheckForUpdate);` sem `CancellationToken` amarrado ao ciclo de vida do servidor.
- **Impacto Técnico Real:** baixo — o `catch` genérico engole timeout/cancelamento, e a task não referencia objetos de raid/sessão. Único efeito: em shutdown do servidor no meio da chamada HTTP, a task continua até o `HttpClient` completar ou expirar (sem timeout explícito configurado).
- **Alternativa de Melhor Lógica / Proposta de Correção:** `httpClient.Timeout = TimeSpan.FromSeconds(5)` para evitar pendência indefinida se a API do GitHub não responder.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-27 · Reflection resolvida repetidamente em vez de cacheada
- **Severidade:** 💡 Otimização
- **Localização:** [SkillManagerConstructorPatch.cs:118-140](../modded/Plugin/Skills/Core/Patches/SkillManagerConstructorPatch.cs) (`AccessTools.Field(typeof(SkillClass), "Locked")` chamado 8× em `LockSkills()`); [HackingActionHandler.cs:17](../modded/Plugin/Skills/LockPicking/Actions/HackingActionHandler.cs) (`AccessTools.Method(...).Invoke(...)` resolvido a cada terminal hackeado)
- **Causa Raiz:** nenhum dos dois cacheia o `FieldInfo`/`MethodInfo` em `private static readonly`.
- **Impacto Técnico Real:** baixo — ambos rodam em paths de baixa frequência (construção de `SkillManager` por perfil; sucesso de hacking), não hot-path por frame. Padrão AP-04 exato, correção trivial.
- **Alternativa de Melhor Lógica / Proposta de Correção:** `private static readonly FieldInfo LockedField = AccessTools.Field(typeof(SkillClass), "Locked");` uma vez no topo da classe; idem para o `MethodInfo` de `Unlock`.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### AUD-01-28 · `LockPickingGame.Update()` reassert incondicional de cursor/input todo frame
- **Severidade:** 💡 Otimização
- **Localização:** [LockPickingGame.cs:185-192](../modded/Plugin/Skills/LockPicking/LockPickingGame.cs)
- **Causa Raiz:** `CursorSettings.SetCursor(ECursorType.Idle)`, `Cursor.lockState`, e os dois flags de `GamePlayerOwner.IgnoreInput*` são escritos todo frame dentro de `Update()`, mesmo sem mudar enquanto o minigame está aberto.
- **Impacto Técnico Real:** negligível (atribuições de bool/enum, sem alocação) — valor é higiene/clareza de intenção, não performance. **Ressalva:** se algum sistema nativo do EFT reimpõe esses valores a cada frame por conta própria (padrão comum em overlays de UI), essa reasserção pode ser defensiva e intencional — não confirmável só por leitura estática.
- **Alternativa de Melhor Lógica / Proposta de Correção:** mover para `OnEnable()`/`Activate()`, que já fazem setup equivalente — validar em runtime antes de remover, dada a ressalva acima.
- **Decisão:**
  - `[ ]` Pendente
  - `[ ]` Aceitar sugestão
  - `[ ]` Aceitar com modificação: _________________
  - `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

## 4. Plano de Ação e Recomendações

1. **Priorizar os 6 achados 🔴** — três deles (`AUD-01-01`, `02`, `03`) são variações do mesmo padrão sistêmico (uso de `MainPlayer` em vez do dono real da ação); recomenda-se resolvê-los juntos num único item de backlog que também estabeleça a convenção (`IsYourPlayer` guard) para os demais 8 pontos do mesmo padrão listados na tabela da seção 1. Os outros três 🔴 (`AUD-01-04` null-check multi-frame, `AUD-01-05`/`06` race conditions server-side) são independentes e podem ser corrigidos em paralelo.
2. **Os 9 achados 🟠** dividem-se em dois grupos: mutação de template compartilhado (`07`, `08` — arquiteturalmente mais trabalhosos, tratar como spec técnica própria) e o restante (gating ausente, leaks de evento, bug de copy-paste) — correções pontuais e independentes.
3. **Achados 🟡/🔵/💡** — agrupar numa segunda rodada de limpeza após os itens acima; nenhum é bloqueante isoladamente.
4. **Cobertura não confirmada** (registrar como pendência, não como achado): ~10 dos ~30 `ModulePatch` do mod não foram cruzados linha-a-linha contra o decompiled (`SkillClassOnTriggerPatch`, `CreateSkillPatches`, os 3 patches de UI de ícone) — nenhum indício de bug pelo nome/contexto, mas não confirmados por orçamento da sessão. O Prepatcher (Mono.Cecil sobre `EBuffId`/`SkillManager`) foi validado apenas estruturalmente, não byte-a-byte no IL gerado.
5. **Encaminhamento:** conforme a convenção do repositório, este relatório não é consumível diretamente por `/apply-code-review` — os achados aceitos devem ser agrupados num item de backlog via `/add-backlog-item` (um item por rodada, não por achado) e seguir o ciclo normal de spec/review/code.
