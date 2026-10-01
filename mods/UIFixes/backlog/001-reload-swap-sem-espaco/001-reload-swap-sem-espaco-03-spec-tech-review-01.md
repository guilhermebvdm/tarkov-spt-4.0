# 001 — Swap de magazine no mesmo slot ao recarregar com R sem espaço · Review Técnica 01

**Mod:** UIFixes
**Spec técnica revisada:** [001-reload-swap-sem-espaco-02-spec-tech.md](001-reload-swap-sem-espaco-02-spec-tech.md)
**Data:** 2026-09-06

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM`. Resolver até zerar bloqueadores antes de `/code-mod`.

**Memória consultada:** sem memória prévia relevante ao item · pendências que afetam: nenhuma.
**Docs técnicos:** `spt-antipatterns.md` relido (AP-04, AP-08) — nenhuma violação nova encontrada além do ponto abaixo.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 2 · Total: 2

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C — Erro de lógica | 🟠 | Ordem da checagem nova muda a prioridade existente do slot escolhido quando `AlwaysSwapMags` está desligado | ✅ Resolvido em 2026-09-06 |
| PA-01-02 | A — Gap | 🟢 | Diferença de tamanho entre `currentMagazine` e `magazine` não é mencionada como limitação pré-existente | ✅ Resolvido em 2026-09-06 |

## Categorias

- **A — Gaps de Especificação:** informações ausentes que ambiguam a implementação
- **B — Edge Cases:** cenários válidos não cobertos
- **C — Erros de Lógica:** pressupostos errados, contradições, código incompatível com SPT 4.0+

## Impacto

- 🔴 **Bloqueador** — impede implementar ou causa bug/crash garantido
- 🟠 **Forte** — pode causar comportamento errado em cenário relevante
- 🟢 **Menor** — qualidade/clareza, não bloqueia

---

## Pontos

### PA-01-01 · C — Erro de lógica · 🟠 ✅ Resolvido em 2026-09-06

**Ordem da checagem nova muda a prioridade existente do slot quando `AlwaysSwapMags` está desligado**

**Problema:** O stub em §5 propõe checar `magAddress` (o endereço exato que o carregador novo está deixando) **antes** da busca ampla existente (`GetPrioritizedGridsForUnloadedObject`), incondicionalmente:

```csharp
ItemAddress candidateAddress = null;
if (magAddress is GridItemAddress gridMagAddress)
{
    candidateAddress = gridMagAddress.Grid.FindLocationForItem(currentMagazine);
}
candidateAddress ??= controller.Inventory.Equipment.GetPrioritizedGridsForUnloadedObject(false)...
```

O código **existente** (`ReloadInPlacePatches.cs:192-197`, citado na própria spec) só prioriza `magAddress` quando `Settings.AlwaysSwapMags.Value` está **ligado** (`OrderByDescending(address => Settings.AlwaysSwapMags.Value && address.Equals(magAddress))`). Com `AlwaysSwapMags` desligado, o comportamento atual é escolher a **menor vaga livre disponível** em qualquer lugar do colete/bolsos (`ThenBy(...GridWidth*GridHeight)`), não necessariamente o slot vacado. A ordem proposta no stub ignora essa configuração e **sempre** tenta `magAddress` primeiro, independente do valor de `Settings.AlwaysSwapMags`.

**Por que importa:** Isso muda o comportamento do caso que **já funciona hoje** (colete com espaço livre, `AlwaysSwapMags` desligado, existe uma vaga menor/melhor em outro lugar) — o carregador antigo passaria a ir sempre para o slot vacado em vez da vaga que o usuário configurou como preferência (não forçar sempre o mesmo slot). Isso contradiz diretamente o critério de aceite nº2 da spec funcional (`001-reload-swap-sem-espaco-01-spec.md`): *"O mesmo teste, com pelo menos uma vaga livre no colete, continua funcionando como hoje (sem regressão)"*. A própria spec técnica já reconhece o risco em §7 ("registrar como ponto a validar manualmente"), mas registrar como risco não é suficiente quando existe um desenho que evita o risco por completo.

**Sugestão:** Inverter a ordem: manter a busca ampla existente (`GetPrioritizedGridsForUnloadedObject(false)...FirstOrDefault()`) **exatamente como está hoje**, e só usar a checagem direta em `magAddress` como **fallback**, quando a busca ampla retornar `null` — que é exatamente o caso relatado pelo usuário (colete/bolsos sem espaço, ou carregador vindo da mochila). Reescrever o stub §5 assim:

```csharp
ItemAddress candidateAddress = controller.Inventory.Equipment.GetPrioritizedGridsForUnloadedObject(false)
    .Select(grid => grid.FindLocationForItem(currentMagazine))
    .Where(address => address != null)
    .OrderByDescending(address => Settings.AlwaysSwapMags.Value && address.Equals(magAddress))
    .ThenBy(address => address.Grid.GridWidth * address.Grid.GridHeight)
    .FirstOrDefault();

// ref: 001-reload-swap-sem-espaco — fallback quando a busca ampla não encontra nada (colete/bolsos
// sem espaço, ou o carregador novo veio da mochila, que GetPrioritizedGridsForUnloadedObject(false)
// não considera). Só entra em ação quando o comportamento de hoje já falharia de qualquer forma,
// então não muda nenhuma prioridade existente.
if (candidateAddress == null && magAddress is GridItemAddress gridMagAddress)
{
    candidateAddress = gridMagAddress.Grid.FindLocationForItem(currentMagazine);
}
```

Isso fecha exatamente o bug relatado sem alterar nenhum comportamento do caminho que já funciona — zero risco de regressão na priorização existente, e o ponto de risco descrito em §7 deixa de existir (pode ser removido em vez de só documentado).

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Ordem invertida em `001-reload-swap-sem-espaco-02-spec-tech.md` §1.4/§5/§6/§7: busca ampla existente roda primeiro sem alteração; checagem em `magAddress` vira fallback, só executado quando `candidateAddress == null`. Zero mudança de prioridade no caso que já funciona.

---

### PA-01-02 · A — Gap · 🟢 ✅ Resolvido em 2026-09-06

**Diferença de tamanho entre os dois carregadores não é mencionada como limitação pré-existente**

**Problema:** Tanto a busca ampla existente quanto a checagem nova em `magAddress` dependem de `FindLocationForItem(currentMagazine)` conseguir encaixar o carregador antigo exatamente no espaço vago — o que só funciona garantidamente se `currentMagazine` tiver o mesmo tamanho/formato (ou menor) que `magazine`. Se o carregador antigo for fisicamente maior (ex.: trocando um carregador padrão por um estendido), a vaga do carregador que saiu pode não ser grande o suficiente, mesmo com a correção. A spec técnica não menciona essa limitação em lugar nenhum.

**Por que importa:** Não é uma regressão (a busca ampla original já tinha essa mesma limitação implícita), mas deixar isso sem registro pode fazer o `/code-mod` ou uma validação futura tratar esse cenário como um bug novo introduzido por esta correção, quando na verdade é uma limitação estrutural pré-existente do próprio `FindLocationForItem`.

**Sugestão:** Adicionar uma frase em §7 (Riscos e dependências) explicitando: "Carregadores de tamanhos físicos diferentes (ex.: padrão vs. estendido) podem não caber no espaço vacado — limitação pré-existente de `FindLocationForItem`, não introduzida por esta correção; nesses casos o comportamento cai de volta ao padrão do jogo (derruba no chão), igual acontece hoje." Não precisa de mudança de código, só documentação.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Nota adicionada em `001-reload-swap-sem-espaco-02-spec-tech.md` §1.4 e §7 explicitando a limitação pré-existente de tamanho físico entre carregadores.
