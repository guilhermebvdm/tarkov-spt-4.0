# 001 — Fix descarte de mochila ("ZZ" / DropBackpack) no Headless/Host · Code Review 01

**Mod:** FIKA  
**Spec funcional:** [001-drop-backpack-sync-fix-01-spec.md](001-drop-backpack-sync-fix-01-spec.md)  
**Spec técnica:** [001-drop-backpack-sync-fix-02-spec-tech.md](001-drop-backpack-sync-fix-02-spec-tech.md)  
**Asbuild:** [001-drop-backpack-sync-fix-05-asbuild.md](001-drop-backpack-sync-fix-05-asbuild.md)  
**Data:** 2026-09-03  

> Análise crítica do código implementado por `/code-mod`. Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.

**Memória consultada:** snapshot de 2026-09-02 (Sessão 1, topo de `mods/FIKA/memory/sessions.md`) · pendências que afetam: nenhuma bloqueadora — [P-1.1] (validação in-game da suite completa modded) permanece aberta e foi o vetor de descoberta deste bug de sincronização de descarte de mochila em raid multiplayer com Headless.

---

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 1 · 🟢 Menores: 2 · ✅ Resolvidos: 0 · Total: 3

---

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| CR-01-01 | E — Legibilidade/manutenção | 🟢 Menor | Ausência de log de diagnóstico ao interceptar descarte remoto | `[ ]` Pendente |
| CR-01-02 | B — Bug latente | 🟡 Médio | Escopo restrito a `ObservedPlayer` vs. bots de IA no Headless | `[ ]` Pendente |
| CR-01-03 | D — Arquitetura | 🟢 Menor | Parâmetro tipado como `object abstractOperation` sem alias conceitual EFT | `[ ]` Pendente |

---

## Categorias

- **A — Crítico** — bug grave, crash garantido, corrupção de estado, security issue.
- **B — Bug latente** — comportamento errado em cenário plausível, não acionado pelo caminho golden.
- **C — Gap vs. spec** — código não implementa critério de aceite, corner case, ou AC da spec.
- **D — Arquitetura** — viola padrões do repo, duplica código, leak de estado, abuso de reflection.
- **E — Legibilidade/manutenção** — nomes ruins, comentário "porquê" ausente, código morto, complexidade desnecessária.
- **F — Melhoria opcional** — refactor de qualidade, micro-otimização, simplificação.

---

## Impacto

- 🔴 **Bloqueador** — fix obrigatório antes de fechar o item.
- 🟠 **Forte** — fix recomendado; pode ser deferido para `06-fix-NN.md` futuro.
- 🟡 **Médio** — anotar, decidir caso a caso.
- 🟢 **Menor** — opcional.

---

## Confirmações de Evidência

1. **Validação da Causa Raiz no EFT:**
   - Confirmado em `references/eft-decompiled/Assembly-CSharp/EFT/Player.cs:1494` (`method_35`) que `to == null` com slot animado (`Backpack`, `InventoryController.cs:147`) direciona a mochila para `Player.TryRemoveFromHands` (`Player.cs:1458`).
   - Confirmado em `Player.cs:32245-32261` que quando `HandsController.Item != item`, o EFT delega a `HandsController.CanExecute(abstractOperation)`.
   - Em réplicas `ObservedPlayer`, o `HandsController` responde `false`, caindo em `callback.Fail("hands controller can't perform this operation")` na linha 32261.
2. **Registro Harmony Automático:**
   - Confirmado que `FikaPlugin.cs:179` invoca `_patchManager.EnablePatches()`, que carrega por reflection todos os `ModulePatch` do assembly `Fika.Core`. O novo patch é ativado automaticamente sem necessidade de instanciação manual em `EnableModulePatches`.
3. **Compilação e Integridade:**
   - `Fika.Core.dll` e `Fika.Headless.dll` recompilados com 0 erros e 0 avisos em configuração `Release`.

---

## Pontos

### CR-01-01 · E — Legibilidade/manutenção · 🟢 Menor

**Ausência de log de diagnóstico ao interceptar descarte remoto**

**Local:** [`mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/Patches/PlayerPatches/ObservedPlayer_DropBackpackSafety_Patch.cs:28-32`](../../modded/Fika-Plugin/Fika.Core/Main/Patches/PlayerPatches/ObservedPlayer_DropBackpackSafety_Patch.cs#L28-L32)

**Problema:** O patch intercepta o descarte e bypassa o `HandsController` de forma 100% silenciosa:
```csharp
if (__instance is ObservedPlayer && (__instance.HandsController == null || __instance.HandsController.Item != item))
{
    callback?.Succeed();
    return false;
}
```

**Por que importa:** Em servidores dedicados (Headless) e clientes em rede, quando ocorrem descartes de mochila, não há confirmação visual ou rastreio no log de console de que a salvaguarda agiu com sucesso, dificultando a depuração em caso de relatos de problemas com inventário multiplayer.

**Sugestão:** Adicionar log de depuração condicional (ex.: `FikaPlugin.Instance?.FikaLogger.LogDebug`):
```csharp
if (__instance is ObservedPlayer && (__instance.HandsController == null || __instance.HandsController.Item != item))
{
#if DEBUG
    FikaPlugin.Instance?.FikaLogger.LogDebug($"[DropBackpackSafety] Intercepted remote drop for {__instance.Profile?.Nickname ?? __instance.name}, item: {item.ShortName.Localized()} ({item.Id})");
#endif
    callback?.Succeed();
    return false;
}
```

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-02 · B — Bug latente · 🟡 Médio

**Escopo restrito a `ObservedPlayer` vs. bots de IA no Headless**

**Local:** [`mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/Patches/PlayerPatches/ObservedPlayer_DropBackpackSafety_Patch.cs:28`](../../modded/Fika-Plugin/Fika.Core/Main/Patches/PlayerPatches/ObservedPlayer_DropBackpackSafety_Patch.cs#L28)

**Problema:** A condição filtra estritamente por `if (__instance is ObservedPlayer && ...)`. No Headless, bots de IA (Scavs, PMCs, Bosses) não herdam de `ObservedPlayer`, mas de `Player` (ou `FikaBot : Player`).

**Por que importa:** Se um mod de IA (ex.: SAIN, Questing Bots) ou uma rotina do EFT ordenar que um bot de IA execute um comando de descarte rápido de mochila em runtime, e suas mãos estiverem em um estado não-receptivo a `abstractOperation`, o bot sofrerá a mesma exceção no Headless (`hands controller can't perform this operation`). Embora bots normalmente utilizem descarte direto de inventário sem animação de mãos, entidades de bot não-locais podem estar vulneráveis se chamarem `Player.DropBackpack()`.

**Sugestão:** Avaliar se a guarda deve permanecer restrita a jogadores humanos (`ObservedPlayer`) ou ser estendida de forma defensiva para entidades que não sejam o jogador local:
```csharp
bool isRemoteEntity = __instance is ObservedPlayer || (!__instance.IsYourPlayer && __instance is not ClientPlayer);
if (isRemoteEntity && (__instance.HandsController == null || __instance.HandsController.Item != item))
```

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

### CR-01-03 · D — Arquitetura · 🟢 Menor

**Parâmetro tipado como `object abstractOperation` sem alias conceitual EFT (Readiness SPT 4.1)**

**Local:** [`mods/FIKA/modded/Fika-Plugin/Fika.Core/Main/Patches/PlayerPatches/ObservedPlayer_DropBackpackSafety_Patch.cs:24`](../../modded/Fika-Plugin/Fika.Core/Main/Patches/PlayerPatches/ObservedPlayer_DropBackpackSafety_Patch.cs#L24)

**Problema:** A assinatura do Prefix Harmony declara:
```csharp
public static bool Prefix(Player __instance, Item item, object abstractOperation, Callback callback)
```
O tipo nativo no EFT 0.16.9 é `GInterface438`.

**Por que importa:** Conforme as diretrizes de desofuscação e convenção de arquitetura do repositório, parâmetros de interfaces ofuscadas devem citar o conceito semântico (no SPT 4.1 mapeado para `EFT.InventoryLogic.Operations.IInventoryOperation`). O uso de `object` é seguro porque o Harmony faz bind por nome do parâmetro e o valor não é consumido pelo prefix, mas sem o comentário da interface a intenção arquitetural fica oculta.

**Sugestão:** Adicionar comentário explicativo no parâmetro nomeando o conceito do EFT:
```csharp
// abstractOperation: GInterface438 (SPT 4.1 alias: EFT.InventoryLogic.Operations.IInventoryOperation)
public static bool Prefix(Player __instance, Item item, object abstractOperation, Callback callback)
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
| 2026-09-03 | Code review 01 criada via `/code-review`. 0🔴 / 0🟠 / 1🟡 / 2🟢. Sem bloqueadores 🔴 — item aprovável para validação final. |
