# 024 — Descarte instantâneo das mãos no EmergencyDrop · Review Técnica 01

**Mod:** TRL-ImmersiveCombatMedicine
**Spec técnica revisada:** [024-acelerar-fechamento-kit-emergencydrop-02-spec-tech.md](024-acelerar-fechamento-kit-emergencydrop-02-spec-tech.md)
**Data:** 2026-09-12

> Análise crítica da spec técnica. Cada ponto recebe um ID `PA-01-MM` (review 01, ponto MM). Resolver até zerar bloqueadores antes de `/code-mod`.

## Resumo

> 🔴 Bloqueadores: 0 · 🟡 Importantes: 2 · 🟢 Menores: 1 · ✅ Resolvidos: 3 · Total: 3

Memória consultada: `mods/TRL-ImmersiveCombatMedicine/memory/sessions.md`, snapshot de 2026-09-12 (Sessão 12). Nenhuma pendência 🔴 afeta este item. [P-9.2] (torniquetes/necrose) não relacionado.

## Índice

| ID | Categoria | Impacto | Título | Status |
|---|---|---|---|---|
| PA-01-01 | C — Erro de Lógica | 🟡 Importante | Auditoria AP-03 incompleta: `Destroy()` tem 8 overrides, spec só cita `MedsController` | ✅ Resolvido em 2026-09-12 |
| PA-01-02 | A — Gap | 🟡 Importante | Feedback ao jogador quando a penalidade de carga é aplicada não está definido | ✅ Resolvido em 2026-09-12 |
| PA-01-03 | B — Edge Case | 🟢 Menor | `_healStartTime` não é resetado após o drop de emergência (inconsistente com `CancelHealInProgress`) | ✅ Resolvido em 2026-09-12 |

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

### PA-01-01 · C — Erro de Lógica · 🟡 Importante · ✅ Resolvido em 2026-09-12

**Auditoria AP-03 incompleta: `Destroy()` é virtual com 8 overrides em `Player.cs`, a spec só cita e prova síncrono um deles**

**Problema:** A estratégia (§1) e o risco §7 apoiam a alegação "`DestroyController()` é síncrono" citando apenas `MedsController.Destroy()` ([`Player.cs:19801-19807`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L19801-L19807)). `Destroy()` é um método **virtual** de `AbstractHandsController` com **8 overrides** em `Player.cs` (linhas 2225, 13532, 15531, 17516, 17982, 18299, 19801, 21698) — a skill `graph-code-navigation`/AP-03 exige que todo alvo virtual tenha seus overrides auditados antes de assumir uma propriedade universal ("`DestroyController()` sempre síncrono para qualquer controlador"). Hoje a spec não demonstra por que só o override de `MedsController` importa aqui.

**Por que importa:** Se `doctor.HandsController` pudesse ser um controlador diferente de `MedsController` no instante em que `PASSO 2b` roda dentro de `EmergencyDrop`, e esse outro override de `Destroy()` fosse assíncrono/tivesse efeito colateral, o "instantâneo" prometido pela spec funcional deixaria de ser verdade sem que ninguém percebesse até testar em raid — exatamente o tipo de regressão silenciosa que o item 023 já mostrou ser fácil de introduzir nesse código.

**Verificação feita nesta review (fecha a dúvida, mas precisa entrar na spec):** `Player.MedsController` ([`Player.cs:19439`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L19439), `public class MedsController : ItemHandsController, GInterface203, IOnHandsUseCallback, IHandsController`) é a **única** classe de hands-controller usada para itens `MedsItemClass` — confirmado pelo guard interno `if (MedsController_0.Item is MedsItemClass)` ([`Player.cs:19486`](../../../../references/eft-decompiled/Assembly-CSharp/EFT/Player.cs#L19486)). Bandagem, tala, torniquete e kit cirúrgico são todos `MedsItemClass` (mesma hierarquia de item, distinguidos só por `ItemStats`/animação, não por classe de controller). `EmergencyDrop()` só é alcançável quando `_isHealingInProgress == true` ([`BandAidController.cs:172`](../../../../mods/TRL-ImmersiveCombatMedicine/modded-V4/Patches/Medical/BandAidController.cs#L172)), que só fica `true` dentro de `HealRoutine`, que só roda depois de `doctor.SetInHands(itemUsed, ...)` com um `itemUsed` que é sempre o item médico do slot ([`BandAidController.cs:360`](../../../../mods/TRL-ImmersiveCombatMedicine/modded-V4/Patches/Medical/BandAidController.cs#L360)). Logo, no instante em que `PASSO 2b` roda, `doctor.HandsController` só pode ser `null` (já liberado) ou `Player.MedsController` — nunca um dos outros 7 overrides. Isso fecha, de quebra, o corner case aberto na spec funcional sobre itens de uma mão só (bandagem/tala): usam a MESMA classe `MedsController`, então o mesmo `Destroy()` síncrono se aplica a eles também.

**Sugestão:** Adicionar este raciocínio (com as 3 citações acima) ao §1 ("Estratégia") ou como um item novo em §7 ("Riscos e dependências") da spec técnica, substituindo a afirmação solta "o mecanismo só foi confirmado pra itens de duas mãos" por essa prova. Isso também permite fechar (não só flagar) o corner case da spec funcional sobre bandagem/tala — atualizar a spec funcional (`01-spec.md`) removendo o `<!-- review -->` pendente sobre esse ponto, já que agora há evidência de código, não suposição.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Adicionado ao §1/§9 da spec técnica. Além disso, o usuário decidiu independentemente restringir todo o escopo do item a itens de cirurgia (CMS/Surv12) — o que reduz ainda mais o universo de hands-controllers possíveis nesse call site e fecha, de quebra, o corner case de bandagem/tala da spec funcional.

---

### PA-01-02 · A — Gap · 🟡 Importante · ✅ Resolvido em 2026-09-12

**Feedback ao jogador quando a penalidade de carga é aplicada não está definido**

**Problema:** O stub de penalidade (§5, segundo bloco) calcula `itemConsumed` e chama `MedicalLogic.ConsumeSafe(...)`, mas não define nenhuma notificação diferenciada pro jogador. O padrão já existente em `CancelHealInProgress` ([`BandAidController.cs:795-819`](../../../../mods/TRL-ImmersiveCombatMedicine/modded-V4/Patches/Medical/BandAidController.cs#L795-L819)) mostra duas mensagens distintas — `TreatmentCancelledWithItemLoss` quando consome carga, `TreatmentCancelled` quando não — mas `EmergencyDrop()` hoje sempre mostra a mesma notificação genérica `MedicTextId.ItemDropped` ([`BandAidController.cs:540`](../../../../mods/TRL-ImmersiveCombatMedicine/modded-V4/Patches/Medical/BandAidController.cs#L540)), incondicionalmente. A spec técnica não diz se isso muda.

**Por que importa:** Sem diferenciação, o jogador não tem como saber, olhando a notificação, que perdeu 1 carga do kit cirúrgico caro (Surv12/CMS) por ter acionado o drop de emergência depois de 1s — mesma informação que já é considerada importante o suficiente pra ter uma mensagem própria no cancelamento normal. Ficaria inconsistente entre os dois fluxos de cancelamento sem nenhuma decisão explícita — parece esquecimento, não escolha.

**Sugestão:** Definir explicitamente uma de duas opções na spec: (a) reusar `MedicTextId.TreatmentCancelledWithItemLoss` no lugar de `ItemDropped` quando `itemConsumed == true` (mesmo texto de string, mesmo padrão de `CancelHealInProgress`), mantendo `ItemDropped` só quando não há penalidade; ou (b) decisão consciente de manter sempre `ItemDropped` porque a semântica de "emergência" já comunica a urgência e a perda é secundária. Qualquer uma das duas resolve o gap — o que não pode é ficar implícito.

**Decisão:**
- `[x]` Aceitar sugestão (opção a)

**Resolução:** `EmergencyDrop` agora mostra `TreatmentCancelledWithItemLoss` quando a penalidade é aplicada e `ItemDropped` quando não, mesmo padrão de `CancelHealInProgress`. Ver §5/§8 da spec técnica.

---

### PA-01-03 · B — Edge Case · 🟢 Menor · ✅ Resolvido em 2026-09-12

**`_healStartTime` não é resetado após o drop de emergência (inconsistente com `CancelHealInProgress`)**

**Problema:** O stub de penalidade em §5 calcula `elapsed` a partir de `_healStartTime`, mas não zera `_healStartTime` depois — `CancelHealInProgress` faz isso explicitamente logo após capturar `elapsed` (`_healStartTime = -1f;`, [`BandAidController.cs:760`](../../../../mods/TRL-ImmersiveCombatMedicine/modded-V4/Patches/Medical/BandAidController.cs#L760)), mas `EmergencyDrop()` hoje não toca em `_healStartTime` em nenhum ponto do seu fluxo atual.

**Por que importa:** Hoje isso não causa bug funcional — `_healStartTime` é reatribuído no início de toda `HealRoutine` seguinte ([`BandAidController.cs:556`](../../../../mods/TRL-ImmersiveCombatMedicine/modded-V4/Patches/Medical/BandAidController.cs#L556)) antes de qualquer leitura, e `ResetAllState()` já zera entre raids. É puramente uma inconsistência defensiva: se algum código futuro vier a ler `_healStartTime > 0` como proxy de "há uma cura em andamento" (do jeito que `_isHealingInProgress` já é usado hoje), um valor não resetado depois do drop de emergência ficaria enganosamente "positivo" por engano.

**Sugestão:** Adicionar `_healStartTime = -1f;` ao stub, logo após a linha que calcula `elapsed`, espelhando exatamente o padrão de `CancelHealInProgress`. Mudança de 1 linha, sem risco.

**Decisão:**
- `[x]` Aceitar sugestão

**Resolução:** Adicionado ao stub de §5 da spec técnica, logo após o cálculo de `elapsed`.

---

## Status

**0 🔴 Bloqueadores, 3/3 pontos resolvidos** — a spec técnica já reflete as 3 resoluções acima, mais a decisão de escopo (exclusivo a itens de cirurgia) tomada pelo usuário depois desta review, que reforçou a resolução de PA-01-01. Pronta pra `/code-mod`.
