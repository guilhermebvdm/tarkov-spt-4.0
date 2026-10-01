# 024 — Descarte instantâneo das mãos no EmergencyDrop · Code Review 01

**Mod:** TRL-ImmersiveCombatMedicine
**Spec funcional:** [024-acelerar-fechamento-kit-emergencydrop-01-spec.md](024-acelerar-fechamento-kit-emergencydrop-01-spec.md)
**Spec técnica:** [024-acelerar-fechamento-kit-emergencydrop-02-spec-tech.md](024-acelerar-fechamento-kit-emergencydrop-02-spec-tech.md)
**Asbuild:** [024-acelerar-fechamento-kit-emergencydrop-05-asbuild.md](024-acelerar-fechamento-kit-emergencydrop-05-asbuild.md)
**Data:** 2026-09-12

> Análise crítica do código implementado por `/code-mod`. Cada achado recebe um ID `CR-01-MM` permanente. Resolver bloqueadores 🔴 via `/apply-code-review` antes de fechar o item.

## Resumo

> 🔴 Bloqueadores: 0 · 🟠 Fortes: 0 · 🟡 Médios: 0 · 🟢 Menores: 1 · ✅ Resolvidos: 1 · Total: 2

Memória consultada: `mods/TRL-ImmersiveCombatMedicine/memory/sessions.md`, snapshot de 2026-09-12 (Sessão 13, escrita nesta mesma sessão). Nenhuma pendência 🔴 afeta este item. Pendência 🟡 [P-13.1] (validação in-game) já cobre boa parte do achado CR-01-01 abaixo — reforça a necessidade de validar especificamente o cenário de kit quase esgotado, não só o caminho feliz.

## Índice

| ID | Categoria | Impacto | Título | Status |
| --- | --- | --- | --- | --- |
| CR-01-01 | B — Bug latente | 🟠 Forte | Penalidade de carga (`ConsumeSafe`) roda DEPOIS de `ThrowItem` — opera num item já fora do inventário | ✅ Aplicado em 2026-09-12 |
| CR-01-02 | E — Legibilidade | 🟢 Menor | Comentário do PASSO 2b mistura português/inglês (`"if HandsController == null"`) | `[ ]` Pendente |

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

### CR-01-01 · B — Bug latente · 🟠 Forte · ✅ Aplicado em 2026-09-12

**Penalidade de carga (`ConsumeSafe`) roda DEPOIS de `ThrowItem` — opera num item já fora do inventário**

**Local:** [`mods/TRL-ImmersiveCombatMedicine/modded-V4/Patches/Medical/BandAidController.cs:554-582`](../../modded-V4/Patches/Medical/BandAidController.cs#L554-L582)

**Problema:** A ordem implementada em `EmergencyDrop` é:

```csharp
// === PASSO 4: DROPAR ITEM SALVO ===
doctor.InventoryController.ThrowItem(savedItem);   // linha 559 — item sai do inventário AGORA
...
// Punição de cancelamento ...
if (elapsed >= 1.0f && savedItem != null && savedStats != null)
{
    ...
    MedicalLogic.ConsumeSafe(doctor, savedItem, 1.0f);   // linha 579 — roda DEPOIS do throw
    itemConsumed = true;
}
```

Isso inverte a ordem usada em `CancelHealInProgress` (`BandAidController.cs:863-873`), que sempre consome a carga **antes** de qualquer mutação de posse do item (lá o item nunca sai do inventário). `MedicalLogic.ConsumeSafe` (`MedicalLogic.cs:521-563`), quando a carga chega a ~0, chama `DiscardItemNetworked(doctor, item)` (`MedicalLogic.cs:577-608`), que por sua vez começa com `if (doctor == null || item == null || item.CurrentAddress == null) return;` (linha 579) — o próprio comentário de `StartDiscardAttempt` (`MedicalLogic.cs:631-639`) documenta que `CurrentAddress == null` é o sinal canônico de "item já removido/sem endereço, nada a fazer". Depois de `ThrowItem`, é razoável esperar que `savedItem.CurrentAddress` já não aponte mais para o slot do inventário do médico (o item passou a existir solto no mundo) — nesse caso o guard silenciosamente aborta o descarte, e o "consumo até zerar" nunca dispara de fato, mesmo tendo logado "consumindo 1 carga do item desesterilizado" e setado `itemConsumed = true`.

**Por que importa:** Em dois cenários plausíveis (não é o caminho feliz, mas é alcançável):
1. **Kit quase no fim da carga:** um CMS/Surv12 usado várias vezes na mesma raid, com pouca carga restante — ao acionar o drop de emergência depois de ≥1s de uso, o log e a notificação (`TreatmentCancelledWithItemLoss`) prometem a penalidade, mas o item que sobra no chão pode não ser removido quando deveria (`HpResource <= 0.005f`), diferente do que aconteceria com o mesmo kit num `CancelHealInProgress` — inconsistência de comportamento entre os dois caminhos de cancelamento para o mesmo estado de carga.
2. **Coop:** se `InventoryController.ThrowItem` não desconecta o `CurrentAddress` no mesmo frame síncrono (rede/Fika pode ter uma etapa assíncrona), rodar `ConsumeSafe`→`DiscardItemNetworked`→`StartDiscardAttempt` (que chama `InteractionsHandlerClass.Discard(item, controller, simulate: true)` e `controller.TryRunNetworkTransaction(...)`, `MedicalLogic.cs:642-649`) logo em seguida arrisca uma segunda operação de rede concorrente sobre o MESMO item que acabou de ser jogado — exatamente a classe de bug (operação de inventário duplicada/concorrente pós-mudança de posse) que os comentários `CR-04`/`CR-05` deste mesmo arquivo documentam terem causado itens fantasma e mãos travadas em testes 2-PCs anteriores.

**Sugestão:** Mover o bloco de penalidade (cálculo de `elapsed`/`isSurgeryOrUseItem`/`ConsumeSafe`) pra **antes** do PASSO 4 (`ThrowItem`), na mesma ordem que `CancelHealInProgress` já usa (consumir primeiro, só then decidir o que fazer com o item). Como `EmergencyDrop` precisa **garantir** que o item termine no chão (diferente de `CancelHealInProgress`, que nunca dropa), a chamada a `ThrowItem` deve ficar condicionada a o item ainda ter um endereço válido depois do consumo — ex.:

```csharp
// Penalidade PRIMEIRO (mesma ordem de CancelHealInProgress)
bool itemConsumed = false;
if (elapsed >= 1.0f && savedItem != null && savedStats != null)
{
    bool isSurgeryOrUseItem = savedStats.IsSurgery || savedStats.HealAmount == 0 || savedStats.IsTourniquet;
    if (isSurgeryOrUseItem || savedItem.GetItemComponent<ResourceComponent>() != null)
    {
        MedicalLogic.ConsumeSafe(doctor, savedItem, 1.0f);
        itemConsumed = true;
    }
}

// PASSO 4: só dropar se o item ainda existe (ConsumeSafe pode ter descartado via DiscardItemNetworked)
if (savedItem != null && savedItem.CurrentAddress != null)
{
    try { doctor.InventoryController.ThrowItem(savedItem); ... }
    catch (Exception ex) { ... }
}
```

Isso reaproveita o padrão já validado de `CancelHealInProgress` e evita qualquer operação de rede sobre um item que já mudou de posse.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Sugestão aplicada conforme proposto, com uma pequena diferença: em vez de re-verificar `savedItem != null` na condição de `ThrowItem` (que já era redundante), o guard adicionado foi `savedItem != null && savedItem.CurrentAddress != null` — o `ConsumeSafe` agora roda ANTES do drop, e o `ThrowItem` só é chamado se o item ainda tiver endereço válido (não foi descartado via `DiscardItemNetworked` por ter chegado a zero carga).

**Aplicação:** [`mods/TRL-ImmersiveCombatMedicine/modded-V4/Patches/Medical/BandAidController.cs`](../../modded-V4/Patches/Medical/BandAidController.cs) — bloco de penalidade de carga movido pra antes do PASSO 4; `ThrowItem` agora condicionado a `savedItem.CurrentAddress != null`. Comentário `// ref: CR-01-01` no topo do bloco reordenado. `dotnet build -c Release` em `modded-V4`: 0 Erros, 0 Warnings.

---

### CR-01-02 · E — Legibilidade · 🟢 Menor

**Comentário do PASSO 2b mistura português/inglês (`"if HandsController == null"`)**

**Local:** [`mods/TRL-ImmersiveCombatMedicine/modded-V4/Patches/Medical/BandAidController.cs:512`](../../modded-V4/Patches/Medical/BandAidController.cs#L512)

**Problema:** O comentário de referência ao Assembly diz `Player.cs:22514-22518: "if HandsController == null, execute() direto"`, misturando o `if` em inglês com o resto da frase em português — todos os outros comentários do arquivo (incluindo os do mesmo item, ex. linha 508) usam português consistentemente.

**Por que importa:** Puramente cosmético — não afeta comportamento nem compreensão, mas destoa do padrão do resto do arquivo (que é consistentemente pt-BR nos comentários "porquê").

**Sugestão:** Trocar `"if HandsController == null, execute() direto"` por `"se HandsController == null, execute() direto"`.

**Decisão:**
- `[ ]` Pendente
- `[ ]` Aceitar sugestão
- `[ ]` Aceitar com modificação: _________________
- `[ ]` Rejeitar (deferir / aceitar como dívida): _________________

---

## Histórico

| Data | Evento |
| --- | --- |
| 2026-09-12 | Code review 01 criada via `/code-review`. 0 🔴, 1 🟠 (ordem `ThrowItem`/`ConsumeSafe`), 1 🟢 (comentário bilíngue). |
| 2026-09-12 | Aplicação automática de 1 achado via `/apply-code-review` — ID aplicado: CR-01-01. CR-01-02 permanece pendente (não marcado pelo usuário). |
