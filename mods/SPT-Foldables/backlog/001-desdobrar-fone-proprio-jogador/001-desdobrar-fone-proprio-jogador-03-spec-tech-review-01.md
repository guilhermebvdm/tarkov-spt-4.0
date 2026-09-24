# 001 — Auto-desdobrar fone só no próprio jogador · Review Técnica 01

**Mod:** SPT-Foldables
**Spec técnica revisada:** [001-desdobrar-fone-proprio-jogador-02-spec-tech.md](001-desdobrar-fone-proprio-jogador-02-spec-tech.md)
**Data:** 2026-09-24

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM` (review 01, ponto MM). Resolver até zerar bloqueadores antes de `/code-mod`.
>
> **Memória consultada:** mod novo (`mods/SPT-Foldables/`), sem `memory/sessions.md`. Nenhuma pendência prévia. Pendências que afetam: nenhuma.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 0 · 🟢 Menores: 0 · ✅ Resolvidos: 1 · Total: 1

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| [PA-01-01](#pa-01-01--c--erro-de-lógica--🔴-bloqueador) | C | 🔴 | `inventoryController` em `ItemsPanel.Show` é sempre o do próprio jogador — o guard proposto nunca dispara | ✅ Resolvido 2026-09-24 |

## Categorias

- **A — Gaps de Especificação:** informações ausentes que ambiguam a implementação
- **B — Edge Cases:** cenários válidos não cobertos
- **C — Erros de Lógica:** pressupostos errados, contradições, código incompatível com SPT 4.0+

## Impacto

- 🔴 **Bloqueador** — impede implementar ou causa bug/crash garantido
- 🟡 **Importante** — pode causar comportamento errado em cenário relevante
- 🟢 **Menor** — qualidade/clareza, não bloqueia

---

## Pontos

### PA-01-01 · C — Erro de Lógica · ✅ Resolvido em 2026-09-24

**`inventoryController` em `ItemsPanel.Show` é sempre o do próprio jogador — o guard proposto nunca dispara**

**Problema:** A spec técnica (§1, §5) propõe checar `inventoryController is Player.PlayerInventoryController playerInventoryController` e `playerInventoryController.Player_0.IsYourPlayer` sobre o parâmetro `inventoryController` do `Postfix`, assumindo que esse parâmetro representa "de quem é o inventário sendo mostrado" — e que, ao lootear um corpo/bot, ele seria o `ObservedInventoryController` daquela entidade (não o do jogador local).

Li o corpo real de `EFT.UI.ItemsPanel.Show` no Assembly ([`ItemsPanel.cs:219-292`](../../../../references/eft-decompiled/Assembly-CSharp/EFT.UI/ItemsPanel.cs#L219-L292)) e essa premissa não se sustenta:

```csharp
public async Task Show(ItemContextAbstractClass sourceContext, CompoundItem lootItem, ISession session,
    InventoryController inventoryController, IHealthController health, Profile profile, ...,
    EItemsTab currentTab, bool inRaid, ..., [CanBeNull] InventoryEquipment equipment = null)
{
    UI.Dispose();
    inventoryController_0 = inventoryController;
    inventory_0 = inventoryController_0.Inventory;
    ...
    await method_0(equipment ?? inventory_0.Equipment, sourceContext, inRaid, SplitInFrames);   // linha 273 — SEMPRE a aba "Gear" do PRÓPRIO jogador (equipment default = inventory_0.Equipment)
    ...
    if (lootItem is InventoryEquipment inventoryEquipment)
    {
        _complexStashPanel.Show(inventoryController_0, ..., inventoryEquipment, ...);            // linha 284 — o CORPO/entidade lootada entra aqui, via `lootItem`, NÃO via `inventoryController`
    }
    else if (lootItem != null)
    {
        _simpleStashPanel.Show(lootItem, inventoryController_0, ...);                            // linha 290 — container simples (baú/mochila no chão)
    }
}
```

`inventoryController` é usado, ao longo de TODO o método, só como "quem processa as operações de move/drag" (`StopProcesses()`, passado pro `_complexStashPanel`/`_simpleStashPanel` como o controller que executa as ações) — é **sempre o controller do jogador local**, tanto quando você abre só o próprio TAB quanto quando você loota um corpo simultaneamente. Quem representa a entidade sendo lootada é o parâmetro **`lootItem`** (o `CompoundItem`/`InventoryEquipment` do corpo), que a spec técnica nem captura no `Postfix`.

Confirma-se também de onde vem o crash real (`ArgumentException: TacticalVest`, reportado pelo usuário): a pilha de exceção cita `ComplexStashPanel.Show` → `ContainersPanel.Show`, exatamente o caminho da **linha 284**, que só executa quando `lootItem is InventoryEquipment` — ou seja, o crash acontece processando o **corpo** (via `lootItem`), não o jogador. Como `inventoryController` é sempre o do jogador local, `playerInventoryController.Player_0.IsYourPlayer` será **sempre `true`** nesse `Postfix`, looteando corpo ou não — o guard proposto no stub (§5) nunca retorna cedo no cenário do bug, e a correção **não altera o comportamento reportado**.

**Por que importa:** Implementar o stub exatamente como está na spec técnica resultaria numa mudança de código que **compila, não quebra nada, mas não corrige o bug relatado** — o `Postfix` continuaria disparando a rotina de auto-desdobrar (e a reentrância em `ItemsPanel.Show`/`ContainersPanel.Show`) toda vez que o jogador abre um corpo com o **próprio** fone dobrado, já que `inventoryController` nunca deixa de ser "seu". O item seria dado como resolvido sem resolver nada, e o bug voltaria a aparecer no próximo teste em raid.

**Sugestão:** Trocar o critério de "dono do `inventoryController`" por "existe um container externo (`lootItem`) sendo mostrado junto". Concretamente:

1. Ampliar a assinatura do `Postfix` pra também capturar o parâmetro `CompoundItem lootItem` do método original (Harmony injeta por nome — `lootItem` é exatamente o nome do parâmetro em `ItemsPanel.cs:219`, então basta declarar `CompoundItem lootItem` na lista de parâmetros do `Postfix`).
2. Adicionar o guard: `if (lootItem != null) return;` logo após o guard existente de `inRaid`/`currentTab` — ou seja, só roda a auto-desdobra quando o jogador está olhando **exclusivamente** o próprio inventário, sem nenhum container externo (corpo, bot, baú, mochila no chão) aberto ao lado.
3. Revisar a spec funcional (`01-spec.md`) em conjunto — o "Comportamento desejado" e os critérios estão redigidos em torno de "de quem é o inventário" (dono), quando o critério real e verificável é "há ou não um container externo aberto junto". Os critérios de aceite específicos (abrir corpo com fone dobrado / abrir o próprio TAB) continuam válidos como estão escritos — testam o comportamento observável certo — mas a prosa de "Visão geral"/"Comportamento desejado" e o corner case de "jogador observa outro jogador humano vivo" precisam ser reencaixados nesse critério (abrir o inventário de outro jogador **também** é looteá-lo com um `lootItem` não nulo, então o mesmo guard cobre esse caso — só que por "há loot externo aberto", não por "não é meu").

Isso também simplifica a spec técnica: não precisa mais de `Player.PlayerInventoryController`/`Player_0`/`IsYourPlayer` (§1, refs a `Player.cs:126/850/22719/25372`) — o guard fica menor e não depende de resolver identidade de jogador nenhuma, só de um parâmetro que o próprio método já expõe.

**Decisão:**
- `[x]` Aceitar sugestão
- `[ ]` Caminho alternativo: _________________

**Resolução:** Aplicada em `001-desdobrar-fone-proprio-jogador-02-spec-tech.md` (§1, §5, §6) — o guard passa a capturar `CompoundItem lootItem` e retornar cedo quando `lootItem != null`, em vez de checar `Player.PlayerInventoryController`/`IsYourPlayer`. `01-spec.md` também atualizada (Visão geral, Comportamento desejado, critérios e corner cases reencaixados no critério "há loot externo aberto").
