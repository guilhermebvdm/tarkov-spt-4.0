# 002 — fika-emptyhands-slotviews-sync · Code Review 01

**Mod:** TRL-Fixes  
**Spec funcional:** [002-fika-emptyhands-slotviews-sync-01-spec.md](002-fika-emptyhands-slotviews-sync-01-spec.md)  
**Spec técnica:** [002-fika-emptyhands-slotviews-sync-02-spec-tech.md](002-fika-emptyhands-slotviews-sync-02-spec-tech.md)  
**Asbuild:** [002-fika-emptyhands-slotviews-sync-05-asbuild.md](002-fika-emptyhands-slotviews-sync-05-asbuild.md)  
**Data:** 2026-08-16  

> Análise crítica do código implementado por `/code-mod`. Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 2 · Total: 2

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | B — Bug latente | 🟡 Médio | Limpeza e descarte de `ObservedSlotViewHandler` prévios em re-sincronizações | `[x]` ✅ Aplicado |
| CR-01-02 | E — Legibilidade | 🟢 Menor | Constantes explícitas para valores mágicos de enum (`EmptyHands` e `ReliableOrdered`) | `[x]` ✅ Aplicado |

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

### CR-01-01 · Cat B — Bug latente · 🟡 Médio

**Limpeza e descarte de `ObservedSlotViewHandler` prévios em re-sincronizações**

**Local:** [`mods/TRL-Fixes/modded/Patches/FikaRefreshSlotViewsSafetyPatch.cs:77-83`](../../modded/Patches/FikaRefreshSlotViewsSafetyPatch.cs#L77-L83)

**Problema:**
```csharp
if (handlersList != null && _handlerConstructor != null)
{
    foreach (var equipmentSlot in PlayerBody.SlotNames)
    {
        var slot = __instance.Inventory.Equipment.GetSlot(equipmentSlot);
        if (slot != null)
        {
            var handler = _handlerConstructor.Invoke(new object[] { slot, __instance, equipmentSlot });
            if (handler != null)
            {
                handlersList.Add(handler);
            }
        }
    }
```
Em sessões onde `RefreshSlotViews` é acionado múltiplas vezes (ex.: quando ocorrem re-sincronizações de inventário via `SetInventory`), novos handlers de slot são adicionados à lista `_observedSlotViewHandlers` sem que os handlers criados na sincronização anterior sejam descartados (`Dispose()`) e removidos da lista.

**Por que importa:**
Embora o Fika nativo também sofresse desse comportamento, o acúmulo de instâncias de `ObservedSlotViewHandler` em raids longas com múltiplas re-sincronizações pode reter eventos de slot redundantes na memória.

**Sugestão:**
Iterar pelos itens existentes em `handlersList`, chamar `Dispose()` caso implementem `IDisposable` e invocar `handlersList.Clear()` antes do loop de adição:
```csharp
if (handlersList != null && _handlerConstructor != null)
{
    for (int i = 0; i < handlersList.Count; i++)
    {
        if (handlersList[i] is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
    handlersList.Clear();

    foreach (var equipmentSlot in PlayerBody.SlotNames)
    ...
```

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** ✅ Aplicado em 2026-08-16. `handlersList` agora itera descartando (`Dispose()`) handlers prévios e invoca `Clear()` antes de registrar novos slots em `FikaRefreshSlotViewsSafetyPatch.cs`.

---

### CR-01-02 · Cat E — Legibilidade/Manutenção · 🟢 Menor

**Constantes explícitas para valores mágicos de enum (`EmptyHands` e `ReliableOrdered`)**

**Local:** [`mods/TRL-Fixes/modded/Patches/FikaProceedEmptyHandsSafetyPatch.cs:98, 137`](../../modded/Patches/FikaProceedEmptyHandsSafetyPatch.cs#L98)

**Problema:**
O patch utiliza literais numéricos em `proceedTypeVal != 0` e `Enum.ToObject(_deliveryMethodEnum, 2)` sem constantes simbólicas locais.

**Por que importa:**
Facilita a manutenção futura e documenta de forma imediata que `0 == EProceedType.EmptyHands` e `2 == DeliveryMethod.ReliableOrdered`.

**Sugestão:**
Declarar no topo da classe:
```csharp
private const byte EmptyHandsProceedType = 0;
private const int ReliableOrderedDeliveryMethod = 2;
```
E substituir no código:
```csharp
if (proceedTypeVal != EmptyHandsProceedType) return true;
...
object deliveryMethodVal = Enum.ToObject(_deliveryMethodEnum, ReliableOrderedDeliveryMethod);
```

**Decisão:**
- `[ ]` Pendente
- `[x]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

**Resolução:** ✅ Aplicado em 2026-08-16. Constantes privadas declaradas e utilizadas no Prefix de `FikaProceedEmptyHandsSafetyPatch.cs`.

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-08-16 | Code review 01 criada e resolvida via `/code-review` |

